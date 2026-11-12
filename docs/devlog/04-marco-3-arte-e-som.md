# Devlog 4 — Marco 3: Arte, som e sensação

**Data:** 12/11/2026

Meta do marco: parecer um jogo, não um protótipo.

## O que ficou pronto

- **Paleta de 4 cores** como ScriptableObject (`GamePalette`) e um componente `PaletteColor` que pinta `SpriteRenderer`, `Image`, `Text`, `Camera` e `ParticleSystem`. Nenhuma cor fora da paleta entra na cena.
- **Sprites gerados por código**: gota, bloco arredondado, espinho, gotinha coletável e bolhas de fundo foram desenhados com funções de distância (SDF) e salvos como PNG. Sem depender de saber desenhar.
- Fundo com bolhas em **parallax** (metade da velocidade das paredes) para dar profundidade ao cano.
- **Efeitos**: partículas ao trocar de parede, ao coletar e ao morrer; tremida de câmera na morte.
- **Sons sintetizados**: toque, coleta, morte e uma música em loop de 9,6 s (baixo + arpejo pentatônico), todos gerados como WAV por código. Botão de mudo com estado salvo.
- **Dica na primeira partida**: "Toque para trocar de parede" aparece só na primeira vez e some no primeiro toque.

## Decisão técnica

Tudo que é "sensação" (som, partículas, UI) é um ouvinte de `GameEvents`. O `PlayerController` continua sem referenciar `AudioSource` nem UI, exatamente como o plano pedia.

## Próximo marco

Publicação: desempenho no celular, ícone, WebGL no itch.io, README e teste fechado no Google Play.
