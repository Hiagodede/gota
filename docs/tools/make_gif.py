"""Monta um GIF a partir de PNGs (só biblioteca padrão do Python).

Uso: python make_gif.py <pasta_com_pngs> <saida.gif> [fps]

Os PNGs precisam ser RGB 8 bits sem entrelaçamento (como os que o
GameplayCapture do Unity gera). A paleta é montada com as 255 cores
mais frequentes; as demais vão para a cor mais próxima.
"""
import struct
import sys
import zlib
from collections import Counter
from pathlib import Path


def read_png(path):
    data = Path(path).read_bytes()
    assert data[:8] == b"\x89PNG\r\n\x1a\n", "não é PNG"
    pos = 8
    idat = b""
    width = height = color_type = bit_depth = None
    while pos < len(data):
        length, ctype = struct.unpack(">I4s", data[pos:pos + 8])
        chunk = data[pos + 8:pos + 8 + length]
        pos += 12 + length
        if ctype == b"IHDR":
            width, height, bit_depth, color_type = struct.unpack(">IIBB", chunk[:10])
        elif ctype == b"IDAT":
            idat += chunk
        elif ctype == b"IEND":
            break
    assert bit_depth == 8 and color_type in (2, 6), "PNG precisa ser RGB/RGBA 8 bits"
    bpp = 3 if color_type == 2 else 4
    raw = zlib.decompress(idat)
    stride = width * bpp
    rows = []
    prev = bytearray(stride)
    i = 0
    for _ in range(height):
        f = raw[i]
        i += 1
        cur = bytearray(raw[i:i + stride])
        i += stride
        if f == 1:
            for x in range(bpp, stride):
                cur[x] = (cur[x] + cur[x - bpp]) & 255
        elif f == 2:
            for x in range(stride):
                cur[x] = (cur[x] + prev[x]) & 255
        elif f == 3:
            for x in range(stride):
                left = cur[x - bpp] if x >= bpp else 0
                cur[x] = (cur[x] + ((left + prev[x]) >> 1)) & 255
        elif f == 4:
            for x in range(stride):
                a = cur[x - bpp] if x >= bpp else 0
                b = prev[x]
                c = prev[x - bpp] if x >= bpp else 0
                p = a + b - c
                pa, pb, pc = abs(p - a), abs(p - b), abs(p - c)
                pred = a if (pa <= pb and pa <= pc) else (b if pb <= pc else c)
                cur[x] = (cur[x] + pred) & 255
        rows.append(bytes(cur))
        prev = cur
    pixels = []
    for row in rows:
        for x in range(0, stride, bpp):
            pixels.append((row[x], row[x + 1], row[x + 2]))
    return width, height, pixels


def build_palette(frames):
    counter = Counter()
    for _, _, px in frames:
        counter.update(px)
    palette = [c for c, _ in counter.most_common(255)]
    # índice 0 reservado: garante tamanho 256
    palette = [(0, 0, 0)] + palette
    while len(palette) < 256:
        palette.append((0, 0, 0))
    return palette


def nearest_index(color, palette, cache):
    if color in cache:
        return cache[color]
    r, g, b = color
    best, best_d = 0, 1 << 30
    for i, (pr, pg, pb) in enumerate(palette):
        d = (r - pr) ** 2 + (g - pg) ** 2 + (b - pb) ** 2
        if d < best_d:
            best, best_d = i, d
            if d == 0:
                break
    cache[color] = best
    return best


def lzw_encode(indices, min_code_size=8):
    clear = 1 << min_code_size
    eoi = clear + 1
    out = bytearray()
    bitbuf = 0
    bitcount = 0

    def emit(code, size):
        nonlocal bitbuf, bitcount
        bitbuf |= code << bitcount
        bitcount += size
        while bitcount >= 8:
            out.append(bitbuf & 255)
            bitbuf >>= 8
            bitcount -= 8

    dictionary = {bytes([i]): i for i in range(clear)}
    next_code = eoi + 1
    code_size = min_code_size + 1
    emit(clear, code_size)
    w = b""
    for idx in indices:
        wc = w + bytes([idx])
        if wc in dictionary:
            w = wc
        else:
            emit(dictionary[w], code_size)
            if next_code < 4096:
                dictionary[wc] = next_code
                next_code += 1
                if next_code > (1 << code_size) and code_size < 12:
                    code_size += 1
            else:
                emit(clear, code_size)
                dictionary = {bytes([i]): i for i in range(clear)}
                next_code = eoi + 1
                code_size = min_code_size + 1
            w = bytes([idx])
    if w:
        emit(dictionary[w], code_size)
    emit(eoi, code_size)
    if bitcount:
        out.append(bitbuf & 255)
    return bytes(out)


def write_gif(path, frames, fps):
    width, height, _ = frames[0]
    palette = build_palette(frames)
    cache = {}
    delay = max(2, round(100 / fps))
    out = bytearray()
    out += b"GIF89a"
    out += struct.pack("<HHBBB", width, height, 0xF7, 0, 0)  # GCT 256 cores
    for r, g, b in palette:
        out += bytes((r, g, b))
    out += b"\x21\xFF\x0BNETSCAPE2.0\x03\x01\x00\x00\x00"  # loop infinito
    for n, (w, h, px) in enumerate(frames):
        indices = bytes(nearest_index(c, palette, cache) for c in px)
        out += b"\x21\xF9\x04\x00" + struct.pack("<H", delay) + b"\x00\x00"
        out += b"\x2C" + struct.pack("<HHHHB", 0, 0, w, h, 0)
        out += b"\x08"
        data = lzw_encode(indices)
        for i in range(0, len(data), 255):
            block = data[i:i + 255]
            out += bytes([len(block)]) + block
        out += b"\x00"
        print(f"quadro {n + 1}/{len(frames)}", end="\r")
    out += b"\x3B"
    Path(path).write_bytes(out)
    print(f"\nGIF salvo em {path} ({len(out) / 1024:.0f} KB)")


def main():
    if len(sys.argv) < 3:
        print(__doc__)
        sys.exit(1)
    folder, output = sys.argv[1], sys.argv[2]
    fps = int(sys.argv[3]) if len(sys.argv) > 3 else 12
    files = sorted(Path(folder).glob("*.png"))
    if not files:
        print("nenhum PNG em", folder)
        sys.exit(1)
    frames = [read_png(f) for f in files]
    write_gif(output, frames, fps)


if __name__ == "__main__":
    main()
