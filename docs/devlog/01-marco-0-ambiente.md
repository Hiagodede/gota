# Devlog 1 — Marco 0: Ambiente

**Data:** 12/11/2026

Primeiro dia do Gota, meu primeiro jogo mobile. A meta deste marco era só uma: ter tudo preparado para que nenhum marco seguinte trave por causa de ferramenta.

## O que ficou pronto

- Unity 6 (6000.6.4f1) com Android Build Support, SDK, NDK e OpenJDK instalados.
- Projeto Unity 2D (URP) criado do zero, separado do projeto de testes.
- Repositório `gota` no GitHub com `.gitignore` de Unity. As pastas `Library`, `Temp` e `Logs` ficam fora do versionamento porque o Unity as regenera sozinho.
- Estrutura de pastas em `Assets/`: `Scripts`, `Prefabs`, `Scenes`, `Art`, `Audio`, `Data`.
- Celular (Xiaomi Redmi Note 12) em modo desenvolvedor com depuração USB. Um build vazio instalou e abriu no aparelho.

## O que aprendi

- Na Xiaomi, além da "Depuração USB", é preciso ligar **"Instalar via USB"**. Sem isso o `adb install` falha com `INSTALL_FAILED_USER_RESTRICTED`.
- O primeiro build Android demora (uns 11 minutos) porque o Unity prepara o Gradle. Os seguintes são bem mais rápidos.
- Git não guarda pasta vazia. Um arquivo `.gitkeep` em cada pasta resolve.

## Próximo marco

Mecânica principal: cena com duas paredes, a gota, o toque que troca de parede e o primeiro obstáculo.
