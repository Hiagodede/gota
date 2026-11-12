# Devlog 3 — Marco 2: Jogo completo

**Data:** 12/11/2026

Meta do marco: uma partida com início, meio e fim. Agora o jogo tem menu, HUD, pausa e game over, e dá para jogar quantas vezes quiser sem reiniciar o app.

## O que ficou pronto

- **GameStateMachine** com estados `Menu`, `Playing`, `Paused` e `GameOver`. Cada estado é uma classe com `Enter`/`Exit`; a UI só escuta o evento `OnStateChanged` e liga o painel certo.
- **ObstacleSpawner com object pooling**: todos os obstáculos e coletáveis são criados no `Awake` e depois só ligados e desligados. Nenhum `Instantiate`/`Destroy` durante a partida.
- Três tipos de obstáculo: bloco fixo, espinho fixo e bloco que vai e volta entre as paredes (liberado depois de 150 m).
- **DifficultyConfig** como ScriptableObject: velocidade inicial, incremento a cada 100 m, limite, intervalo entre obstáculos e chance de coletável. Tudo ajustável no Inspector.
- Pontuação: 1 ponto por metro + 10 por gotinha coletada. Recorde salvo com `PlayerPrefs`.
- Telas de menu, pausa e game over com "Jogar de novo".

## Decisão técnica

Separei "quem produz" de "quem consome" com uma classe estática `GameEvents` (C# events). O `PlayerController` dispara `OnWallSwitched` e `OnPlayerDied`; UI, som e efeitos se inscrevem. Assim o controle da gota não conhece nada além de física e input.

## Próximo marco

Arte, som e sensação: paleta, sprites, partículas, sons e a tela de dica.
