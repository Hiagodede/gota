# Devlog 2 — Marco 1: Mecânica principal

**Data:** 12/11/2026

Meta do marco: jogar uma partida com quadrados, sem arte. Conseguimos.

## O que ficou pronto

- Cena vertical (9:16) com câmera ortográfica e duas paredes.
- A "queda" é uma ilusão: a gota fica parada no eixo Y e **o cenário sobe**. Cada parede tem dois segmentos que, ao saírem pelo topo, voltam para baixo. Loop infinito com apenas 4 sprites.
- `PlayerController`: um toque em qualquer ponto da tela troca a gota de parede em **0,15 s**, com ease-out para a chegada ficar suave. Usa o novo Input System (`Pointer.current`), que cobre mouse no editor e toque no celular com o mesmo código.
- Primeiro obstáculo com `BoxCollider2D` em modo trigger. Ao encostar, o jogo dispara o evento `OnPlayerDied` e um `Debug.Log("Game Over")` aparece.

## Decisão técnica

A gota usa `Rigidbody2D` cinemático com `useFullKinematicContacts = true`. Sem essa flag, um corpo cinemático não gera eventos de trigger com colliders estáticos, e a colisão simplesmente não acontece. Essa foi a primeira "pegadinha" do projeto.

## Próximo marco

Transformar o protótipo em um jogo completo: estados, spawner com pool, pontuação e telas.
