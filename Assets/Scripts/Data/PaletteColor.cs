using UnityEngine;
using UnityEngine.UI;

namespace Gota
{
    /// <summary>
    /// Pinta este objeto com uma cor da paleta (SpriteRenderer, Image, Text ou Camera).
    /// Garante que nenhuma cor fora da paleta entre no jogo.
    /// </summary>
    [ExecuteAlways]
    public class PaletteColor : MonoBehaviour
    {
        [SerializeField] private GamePalette palette;
        [SerializeField] private PaletteSlot slot = PaletteSlot.Light;
        [Range(0f, 1f)]
        [SerializeField] private float alpha = 1f;
        [Tooltip("Escurece (negativo) ou clareia (positivo) a cor da paleta.")]
        [Range(-0.5f, 0.5f)]
        [SerializeField] private float shade = 0f;

        private void OnEnable() => Apply();
        private void OnValidate() => Apply();

        public void Apply()
        {
            if (palette == null) return;

            Color c = palette.Get(slot);
            c = shade >= 0f ? Color.Lerp(c, Color.white, shade) : Color.Lerp(c, Color.black, -shade);
            c.a = alpha;

            if (TryGetComponent<SpriteRenderer>(out var sr)) sr.color = c;
            if (TryGetComponent<Image>(out var img)) img.color = c;
            if (TryGetComponent<Text>(out var txt)) txt.color = c;
            if (TryGetComponent<Camera>(out var cam)) cam.backgroundColor = c;
            if (TryGetComponent<ParticleSystem>(out var ps))
            {
                var main = ps.main;
                main.startColor = c;
            }
        }
    }
}
