# GOTA — Game Design Document

**Versão:** 0.1 (rascunho para discussão) · **Data:** 15/11/2026 · **Autor:** Hiago do Carmo Lopes (design), com Claude (redação e análise técnica)

> Este documento descreve o jogo **como queremos que ele seja**, não só o que já existe. O protótipo atual (Marcos 0–3 do plano) é a base técnica; aqui definimos a direção de arte, as mecânicas novas, a progressão e o que entra em cada versão. Tudo que for marcado **[1.0]** entra no lançamento; **[1.1]** vem logo depois; **[2.0]** é a visão de longo prazo.

Referências visuais: os 11 quadros do vídeo-conceito estão em [`docs/gdd/referencias/`](gdd/referencias/).

---

## 1. Visão geral

| | |
|---|---|
| **Pitch** | Você é uma gota d'água caindo por um cano infinito. Toque na tela para pular para a parede oposta, desvie do que vem de baixo e **não evapore** antes de chegar ao fundo que não existe. |
| **Gênero** | Arcade infinito vertical, um toque (*one-tap endless faller*) |
| **Plataformas** | Android (Google Play) e navegador (itch.io, WebGL). iOS depois. |
| **Sessão típica** | 30 s a 3 min. Jogo de fila de banco e intervalo de aula. |
| **Público** | Quem joga casual mobile (Alto's, Downwell, Super Hexagon) e quem gosta de estética *sci-fi neon*. Classificação livre. |
| **Fantasia central** | Ser algo minúsculo e frágil caindo por um lugar imenso, frio e bonito, e sobreviver na base de reflexo. |
| **Pilares** | 1. **Um toque, sempre.** 2. **Legível em 0,3 s.** 3. **Bonito parado, hipnótico em movimento.** 4. **Morrer é rápido, recomeçar é instantâneo.** |

### 1.1 Por que estes pilares

- **Um toque:** é o que torna o jogo jogável com uma mão, no ônibus, sem olhar o botão. Toda mecânica nova precisa funcionar com o mesmo toque ou ser automática. Nada de segundo botão, arrastar ou segurar (segurar pode virar exceção, ver §4.3).
- **Legível em 0,3 s:** a velocidade sobe até 10 m/s. Se o jogador não distingue "perigo" de "bônus" num relance, a morte parece injusta. Por isso a paleta é restrita e cada categoria tem forma + cor + som próprios.
- **Bonito parado:** o jogo é o cartão de visita do portfólio. Uma captura de tela precisa vender o jogo sozinha.
- **Recomeçar instantâneo:** game over → "Jogar de novo" → nova partida em menos de 1 segundo, sem tela de carregamento. Já temos isso com pooling.

---

## 2. Direção de arte

### 2.1 O que o vídeo-conceito diz

Os quadros mostram quatro ideias fortes que vamos adotar:

| Quadro | O que mostra | O que vira no jogo |
|---|---|---|
| 01, 11 | Túnel quadrado de concreto escuro, aros de neon ciano se afastando até o ponto de fuga; a gota no centro, de vidro, refratando as linhas | **A identidade visual do jogo.** Túnel em perspectiva, aros de luz marcando a profundidade, gota translúcida. |
| 02, 03 | Explosão de luz branca em forma de estrela ao bater na parede; faíscas voando | **Morte.** Não é "sangue", é a gota virando luz e se dissipando. |
| 04, 05 | Linhas de neon viram traços diagonais ("velocidade de dobra"); cristais/prismas nas paredes; a gota deforma com a velocidade | **Alta velocidade** (acima de 7 m/s): as luzes esticam, os obstáculos ficam mais angulosos, a gota alonga. |
| 06 | Esfera de cristal facetado brilhando | **Power-up principal (escudo de cristal)** e ícone de "recorde". |
| 07 | Ponto de luz com raios radiais, câmera em túnel escuro | **Portal / mergulho**: trecho de velocidade dobrada com pontuação x2. Também é a transição de bioma. |
| 08, 09, 10 | Cano enferrujado, gota se formando e caindo, partículas suspensas | **Abertura do jogo** e tela de menu. A gota nasce aqui, toda partida. Ferrugem = primeiro bioma. |

### 2.2 Decisão: como chegar nesse visual (2D, 2.5D ou 3D)

O protótipo atual é **2D de lado** (câmera ortográfica, paredes como retângulos). O vídeo é um **túnel em perspectiva** visto de cima, com a gota entre a câmera e o fundo. São três caminhos:

| Opção | Como | Custo | Resultado |
|---|---|---|---|
| **A. 2D neon** | Manter a cena atual, trocar paleta e sprites, adicionar linhas de neon e *bloom* | +3 dias | Bonito, mas não é o vídeo. Parece "jogo de celular de 2015 com filtro". |
| **B. 2.5D túnel (recomendada)** | Cena **3D** no Unity (URP 3D): câmera em perspectiva olhando para baixo do túnel; paredes são quads com material emissivo; a gota é uma esfera deformada com material de vidro; obstáculos são cubos/prismas. **O gameplay continua 2D**: a gota só anda no eixo X (esquerda/direita), tudo o mais é apresentação. | +2 semanas (um marco novo) | É o vídeo. E o código de jogo (estados, pool, eventos, dificuldade) é reaproveitado inteiro, porque ele nunca soube que a cena era 2D. |
| **C. 3D completo** | Túnel com 4 paredes, a gota gira ao redor do eixo | +6 semanas | Vira outro jogo (tipo *Tunnel Rush*). Perde o "um toque". Descartado. |

**Decisão proposta: B.** Justificativa em §8.2. O que muda no código é pequeno: `WorldScroller` move no eixo Z em vez de Y; `Scrollable` idem; colisão passa a ser `Collider` 3D em vez de `Collider2D` (ou mantém 2D num plano, ver §8.2). Tudo que escuta `GameEvents` não muda uma linha.

### 2.3 Paleta

Mantemos a regra de **4 cores por bioma** (já implementada via `GamePalette`), mas agora cada bioma tem sua paleta. A base, tirada dos quadros:

| Papel | Cor | Hex | Uso |
|---|---|---|---|
| Fundo | Preto-azulado profundo | `#06121C` | Concreto do túnel, céu do ponto de fuga |
| Parede | Azul-petróleo escuro | `#143E4F` | Blocos do túnel, UI secundária |
| Neon | Ciano elétrico | `#5FE3F0` | Aros de luz, gota, botões, textos de destaque. **Única cor com bloom.** |
| Luz | Branco-gelo | `#E6FBFF` | Obstáculos, texto principal, flash de morte |

Regra de leitura: **neon = seguro/bônus, branco = perigo.** Nunca inverter.

### 2.4 Biomas (temas do túnel)

O túnel muda a cada **300 m**. A transição é o "portal" do quadro 07: 1,5 s de luz radial, velocidade x2, invulnerabilidade, e o próximo trecho aparece com outra paleta e outro conjunto de obstáculos. O jogador sente progresso mesmo num jogo infinito.

| # | Bioma | Metros | Paleta (fundo / parede / neon / luz) | Clima | Obstáculos novos |
|---|---|---|---|---|---|
| 1 | **Ferrugem** | 0–300 | marrom-escuro / ferrugem / âmbar / creme | O cano do quadro 08. Úmido, orgânico, lento. | Bloco, espinho |
| 2 | **Neon** | 300–600 | `#06121C` / `#143E4F` / `#5FE3F0` / `#E6FBFF` | O túnel do vídeo. Limpo, geométrico. | + Móvel, laser pulsante |
| 3 | **Cristal** | 600–900 | azul-noite / índigo / violeta-claro / branco | Prismas crescem nas paredes (quadro 04). Refração. | + Cristal que cresce, lâmina |
| 4 | **Vapor** | 900–1200 | cinza-quente / grafite / laranja / branco | Calor. A gota evapora mais rápido (§4.2). | + Zona de calor, jato de vapor |
| 5 | **Vazio** | 1200+ | preto / cinza-azulado / branco-azulado / branco | Quase sem paredes visíveis, só aros de luz. Velocidade máxima. | Todos, sorteados |

Depois do Vazio, os biomas repetem em ordem com velocidade no teto. **[1.0]** entrega Ferrugem e Neon. **[1.1]** adiciona Cristal e Vapor. **[2.0]** o Vazio.

### 2.5 A gota

- **Forma:** esfera levemente achatada embaixo quando parada na parede (tensão superficial), alongada quando atravessa (quadros 04 e 05). Em 3D: `Mesh` de esfera com *vertex shader* simples de deformação por velocidade; em 2D: *squash & stretch* via escala.
- **Material:** vidro. Refrata o fundo (em URP: `Opaque Texture` + distorção no shader) e tem um brilho especular pequeno no topo-esquerda. É o único objeto do jogo com transparência real.
- **Tamanho:** varia com a **massa** (§4.2). Entre 60 % e 100 % do tamanho base.
- **Rastro:** deixa uma trilha molhada na parede (sprite de linha com fade de 0,8 s). Cosmético, mas vende a ideia de "escorrer".
- **Idle no menu:** a gota se forma na boca do cano enferrujado, cresce, estica e cai. Ao tocar "Jogar", a câmera a segue para dentro do cano. É o quadro 09 virando o quadro 01.

### 2.6 Efeitos (ordem de prioridade)

1. **Bloom** só no canal neon (URP Volume, threshold alto). É o efeito que faz o visual. Custo: ~1,5 ms em Android intermediário com *downsampling*; medir no Profiler.
2. **Morte**: flash branco de 2 quadros, estrela de luz (quadro 02) com 30 partículas, tremida de câmera (já existe), *slow-motion* de 0,4 s a 30 % antes da tela de game over.
3. **Velocidade**: acima de 7 m/s as linhas de neon ganham *motion blur* falso (sprites alongados), e as partículas de poeira viram traços.
4. **Troca de parede**: 8 gotículas se soltam na direção oposta e um "ploc" (já existe).
5. **Coleta**: a gotinha é sugada para dentro da gota, que pulsa 10 % maior por 0,1 s.
6. **Portal**: raios radiais (quadro 07), *fov* da câmera abre de 60° para 75° por 1,5 s.

### 2.7 Inspirações

| Jogo / obra | O que pegamos | O que NÃO pegamos |
|---|---|---|
| **Tron: Legacy** (filme) | Paleta ciano sobre preto, neon como única luz, geometria limpa | Personagens, motos, qualquer propriedade intelectual |
| **Downwell** | Queda infinita, paleta restrita, sessões curtas, *game feel* de pixel | Tiro, inimigos, pixel art |
| **Super Hexagon** | Legibilidade em alta velocidade, morte instantânea sem drama, música que acelera | Rotação, geometria abstrata total |
| **Thumper** | Sensação de velocidade esmagadora, ritmo visual | Mecânica rítmica obrigatória |
| **Alto's Odyssey** | Modo zen, menus calmos, uma skin por conquista | Física de snowboard, mundo aberto |
| **Rez Infinite** | Sinestesia som+luz, partículas como feedback | Mira, tiro |

---

## 3. Mecânica central (o que já existe e fica)

- **Toque = troca de parede.** 0,15 s de travessia, ease-out. Pode tocar no meio da travessia (inverte).
- **O mundo sobe, a gota fica.** Velocidade única em `WorldScroller`, consumida por paredes, fundo, obstáculos e coletáveis.
- **Colisão = morte.** Sem vida, sem *checkpoint*. (Escudo de cristal é a única exceção, §4.4.)
- **Pontos:** 1 por metro, +10 por gotinha.
- **Dificuldade por distância:** velocidade e intervalo entre obstáculos mudam a cada 100 m via `DifficultyConfig`.

---

## 4. Mecânicas novas

Cada mecânica abaixo responde a uma pergunta: **"o que, além de desviar, dá tensão sem precisar de um segundo botão?"**

### 4.1 Resumo

| Mecânica | Versão | O que adiciona | Risco de design |
|---|---|---|---|
| Evaporação (massa da gota) | **[1.0]** | Um relógio invisível que força a coletar | Pode parecer injusto se o jogador não entender por que morreu |
| Zonas de calor | [1.1] | Evaporação localizada, decisão de rota | Precisa ser muito visível |
| Laser pulsante | **[1.0]** | Timing, não só posição | Som precisa antecipar o pulso |
| Jato de vapor | [1.1] | Empurra a gota para a outra parede sem o jogador pedir | Tira o controle: usar raramente |
| Parede ausente | [1.1] | Trecho em que não dá para trocar | Combinar com obstáculo = morte garantida; nunca fazer |
| Combo de gotinhas | **[1.0]** | Recompensa por risco | Nenhum |
| Escudo de cristal | **[1.0]** | Única "segunda chance" | Não pode ser comum |
| Portal / mergulho | **[1.0]** | Transição de bioma, pontos x2 | Nenhum (é invulnerável) |
| Ímã e câmera lenta | [1.1] | Power-ups secundários | Câmera lenta facilita demais: 3 s no máximo |

### 4.2 Evaporação — a gota fica menor (mecânica principal nova)

**Ideia do Hiago:** uma gota de verdade perde água ao escorrer. No jogo, a gota **perde massa continuamente** e **fica menor**. Se chegar a zero, evapora: game over sem bater em nada. Coletar gotinhas devolve massa.

**Por que funciona:** cria um segundo objetivo que compete com o primeiro. Desviar é mais fácil grudado numa parede sem trocar, mas as gotinhas aparecem do lado oposto ao obstáculo. O jogador que só "sobrevive" seca; o que só "coleta" bate. A tensão vem da troca entre os dois.

**Regras:**

| Parâmetro | Valor inicial | Onde mora |
|---|---|---|
| Massa máxima | 1,0 | `DifficultyConfig.maxMass` |
| Massa inicial | 1,0 | |
| Perda por segundo | 0,04 (25 s para secar sem coletar nada) | `DifficultyConfig.evaporationPerSecond` |
| Perda extra em zona de calor | +0,15/s enquanto dentro | `HeatZone.extraLoss` |
| Ganho por gotinha | +0,25 | `Collectible.massValue` |
| Massa mínima para o tamanho | 0,0 → escala 0,6 | `PlayerController.minScale` |
| Morte por evaporação | massa ≤ 0 | evento `OnPlayerEvaporated` |

**Trade-off escondido (o detalhe bom):** gota menor = **colisor menor** = mais fácil passar raspando. Então ficar quase seco é *vantajoso* por alguns segundos. Jogador avançado vai surfar perto do zero de propósito. Isso é profundidade sem botão novo.

**Feedback para o jogador (obrigatório, senão parece bug):**
- A gota visivelmente encolhe (escala 1,0 → 0,6) e fica mais transparente.
- Abaixo de 30 %: um anel fino ao redor da gota pisca em neon; o "ploc" do toque fica mais agudo.
- Abaixo de 15 %: gotículas de vapor saem da gota (partículas subindo), a música perde os graves.
- Ao evaporar: a gota vira uma nuvem de vapor que sobe (não a estrela de luz da colisão). Tela de game over diz **"Evaporou"** em vez de "Fim".
- Ao coletar: a gota pulsa e a trilha molhada fica mais larga por 1 s.
- **Sem barra de HUD.** A própria gota é a barra. (Opção de acessibilidade liga um anel numérico, §6.4.)

**Implementação (esboço):**

```csharp
// Novo componente na gota. Não conhece UI nem som; só dispara eventos.
public class DropMass : MonoBehaviour
{
    [SerializeField] DifficultyConfig config;
    [SerializeField] float minScale = 0.6f;
    public float Mass { get; private set; } = 1f;
    float extraLossPerSecond; // somado por HeatZone enquanto dentro

    void OnEnable()  { GameEvents.OnCollected += Gain; }
    void OnDisable() { GameEvents.OnCollected -= Gain; }

    public void ResetMass() { Mass = config.maxMass; Apply(); }

    void Update()
    {
        if (!WorldScroller.Instance.IsScrolling) return;
        Mass -= (config.evaporationPerSecond + extraLossPerSecond) * Time.deltaTime;
        Apply();
        if (Mass <= 0f) GameEvents.RaisePlayerEvaporated();
    }

    void Gain(int points) { Mass = Mathf.Min(config.maxMass, Mass + config.massPerCollectible); Apply(); }

    void Apply()
    {
        float t = Mathf.Clamp01(Mass / config.maxMass);
        float s = Mathf.Lerp(minScale, 1f, t);
        transform.localScale = Vector3.one * s;      // o colisor acompanha a escala
        GameEvents.RaiseMassChanged(t);               // UI, som e efeitos escutam
    }

    public void SetExtraLoss(float perSecond) => extraLossPerSecond = perSecond;
}
```

`HeatZone` é um trigger que chama `SetExtraLoss` ao entrar e zera ao sair. `GameStateMachine` trata `OnPlayerEvaporated` como morte com um motivo diferente (`DeathCause.Evaporated`), para a UI e o som escolherem a variante certa.

**Balanceamento inicial:** com 25 s para secar e uma gotinha a cada ~3 obstáculos (chance 30 %), o jogador precisa pegar 1 em cada 2 gotinhas que vê. Ajustar no `DifficultyConfig` depois de 20 partidas de teste. **Meta:** 40 % das mortes de jogador novo por evaporação, 60 % por colisão. Se passar de 50 % evaporação, está punitivo.

### 4.3 Obstáculos (catálogo completo)

Forma + cor + som únicos por tipo. Todos brancos (cor "Luz"), porque branco = perigo.

| Obstáculo | Versão | Forma | Comportamento | Como ler |
|---|---|---|---|---|
| **Bloco** | existe | Retângulo arredondado grudado na parede | Fixo | O mais simples. Primeiros 100 m só têm ele. |
| **Espinho** | existe | Triângulo apontando para o centro | Fixo, colisor em polígono | Mais estreito que o bloco: dá para passar mais perto. |
| **Móvel** | existe | Bloco fino | Vai e volta entre as paredes (senoide) | Liberado em 150 m. Som de "zum" contínuo. |
| **Laser pulsante** | **[1.0]** | Linha fina de parede a parede | Liga por 0,5 s, desliga por 1 s. Só mata ligado. | Aviso: a linha aparece fraca 0,4 s antes de ligar, com "tic" agudo. **Única mecânica de timing.** |
| **Lâmina** | [1.1] | Semicírculo que gira saindo da parede | Gira 180° e volta | Som de corte. Ocupa metade do cano quando aberta. |
| **Cristal que cresce** | [1.1] | Prisma (quadro 04) | Nasce pequeno e cresce por 1,5 s até fechar 70 % do cano | Dá para passar se for rápido. Recompensa agressividade. |
| **Zona de calor** | [1.1] | Faixa translúcida laranja em uma parede, com ondas | Não mata. Evaporação +0,15/s enquanto a gota está nela | Convida a trocar de parede sem obstáculo. |
| **Jato de vapor** | [1.1] | Bocal na parede soltando partículas | Empurra a gota para a outra parede ao passar | Raro (máx. 1 a cada 200 m). Nunca junto de obstáculo do lado oposto. |
| **Grade** | [2.0] | Duas metades que fecham | Fecham em 0,8 s deixando uma fresta no centro... que não existe. O jogador precisa passar *antes* de fechar | Combina com portal: "corra". |

**Regra de geração (anti-injustiça):** o spawner nunca coloca dois obstáculos em paredes opostas com menos de `switchDuration × velocidade × 2` metros de distância. Isso garante que sempre existe uma trajetória possível. Hoje isso já acontece por acaso (alternância de lado); vira regra explícita em `ObstacleSpawner.IsFair()`.

### 4.4 Coletáveis e power-ups

| Item | Versão | Visual | Efeito | Frequência |
|---|---|---|---|---|
| **Gotinha** | existe | Gota pequena, neon | +10 pontos, +0,25 massa | 30 % dos spawns |
| **Combo** | **[1.0]** | Gotinhas seguidas sem bater e sem pular nenhuma | 3 seguidas = x2 pontos de gotinha; 5 = x3; qualquer gotinha perdida zera | Automático |
| **Cristal** (quadro 06) | **[1.0]** | Esfera facetada, gira, brilho forte | **Escudo:** próxima colisão não mata; o cristal estilhaça (estrela de luz) e empurra a gota para a outra parede | 1 a cada 250 m, nunca nos primeiros 100 m |
| **Portal** (quadro 07) | **[1.0]** | Ponto de luz radial no centro do cano | Transição de bioma: 1,5 s a velocidade x2, invulnerável, pontos x2 | Fixo a cada 300 m |
| **Ímã** | [1.1] | Anel neon pulsando | Por 6 s, gotinhas num raio de 2 m são puxadas | 1 a cada 400 m |
| **Gelo** | [1.1] | Floco | Câmera lenta 50 % por 3 s. A música desacelera junto | 1 a cada 500 m |

Nenhum power-up é comprável. Todos aparecem por sorteio com peso em `DifficultyConfig`.

### 4.5 Pontuação final

```
pontos = metros + Σ(gotinha × multiplicadorDeCombo) × (2 se dentro de portal)
```

Recorde pessoal salvo. **[1.1]** estatísticas: maior distância, maior combo, mortes por tipo, total de gotinhas (vira moeda de desbloqueio, §5.2).

---

## 5. Progressão e meta-jogo

Jogo infinito não tem "fim", então o progresso precisa estar **fora** da partida.

### 5.1 O que puxa o jogador de volta

1. **Recorde** (existe). Mostrado no menu e no game over com "faltaram X m".
2. **Biomas:** "cheguei no Cristal" é uma conquista natural. O menu mostra os biomas desbloqueados como aros de luz coloridos.
3. **Skins da gota** (§5.2).
4. **Desafio diário** [1.1]: a mesma semente de obstáculos para todo mundo no dia; recorde do dia separado. Sem servidor: semente = data.
5. **Conquistas** [1.1]: 20 conquistas locais (sem Google Play Games na 1.0).

### 5.2 Skins

**Regra:** skins são **100 % cosméticas**. Mesmo colisor, mesma física, mesma massa. Nada de "skin que evapora mais devagar".

Moeda: **gotinhas acumuladas** (total histórico, não gasta). Cada skin tem um custo em gotinhas OU uma conquista.

| Skin | Visual | Rastro | Som do toque | Desbloqueio |
|---|---|---|---|---|
| **Água** | Vidro transparente, neon | Trilha molhada | "ploc" | Inicial |
| **Óleo** | Preto com reflexo arco-íris | Mancha escura | "plop" grave | 500 gotinhas |
| **Mercúrio** | Metal líquido, espelhado | Esferas que se juntam | "tin" metálico | Chegar ao bioma Cristal |
| **Tinta** | Cor sólida (escolhe entre 4) | Respingo | "splat" | 1.000 gotinhas |
| **Lava** | Laranja incandescente, fumaça | Rastro que brilha e apaga | "tsss" | Sobreviver 60 s no bioma Vapor |
| **Cristal** | A esfera do quadro 06 | Faíscas | Vidro | Recorde ≥ 1.500 m |
| **Vazio** | Só o contorno neon, interior preto | Nenhum | Silêncio | Chegar ao bioma Vazio |

**[1.0]:** Água, Óleo, Tinta. **[1.1]:** o resto. Implementação: `DropSkin` (ScriptableObject) com material, prefab de rastro, clipes de som e condição de desbloqueio; `SkinService` lê/salva em `PlayerPrefs`.

### 5.3 Modos de jogo

| Modo | Versão | O que muda |
|---|---|---|
| **Normal** | existe | Tudo como descrito |
| **Zen** | [1.1] | Sem evaporação, sem morte (bater só faz a gota ricochetear e perde combo). Música ambiente. Para quem quer só olhar o túnel. Não conta recorde. |
| **Desafio diário** | [1.1] | Semente fixa por dia, 1 tentativa que vale, ranking local do dia |
| **Hardcore** | [2.0] | Evaporação x2, sem escudo, começa em 6 m/s. Recorde separado. |

---

## 6. Interface e fluxo

### 6.1 Fluxo de telas

```
[Abertura: cano pingando, 3 s, pulável]
            │
            ▼
┌───────────────────────┐
│         MENU          │  gota idle na boca do cano
│  GOTA                 │  recorde · biomas alcançados
│  ▶ Jogar              │
│  ◉ Skins   ⚙ Opções   │  ♪ som on/off
└───────────┬───────────┘
            │ Jogar
            ▼
┌───────────────────────┐      ┌─────────────────────┐
│        PARTIDA        │ ─II─▶│       PAUSA         │
│  metros (topo)        │◀─────│ Continuar · Menu    │
│  combo (aparece só    │      └─────────────────────┘
│  quando ≥ 2)          │
│  II (canto)           │
└───────────┬───────────┘
            │ morte / evaporação
            ▼
┌───────────────────────┐
│   FIM  /  EVAPOROU    │  pontos grandes
│  Recorde: 1.240       │  "faltaram 87 m" ou "NOVO RECORDE"
│  ▶ Jogar de novo      │  ◉ skin nova desbloqueada (se houver)
│  Menu                 │
└───────────────────────┘
```

Opções: som, música, vibração, modo daltônico, anel de massa, resetar recorde, créditos, versão.

### 6.2 HUD durante a partida

Minimalista de propósito. Só três coisas, e duas delas somem quando não importam:

- **Metros** no topo, fonte grande, cor "Luz". Sempre visível.
- **Combo** ("x2", "x3") aparece ao lado da gota por 1 s quando sobe; some depois.
- **Pausa** no canto superior direito, 44×44 pt mínimo (área de toque confortável).

A **massa** não tem HUD: é o tamanho da gota (§4.2).

### 6.3 Toque e áreas mortas

- Toda a tela é área de toque, **exceto** o botão de pausa. Já funciona assim.
- Toque nos primeiros 0,2 s após a partida começar é ignorado (evita o toque do botão "Jogar" virar troca de parede).
- Com o *Input System*, o mesmo código serve mouse (editor/WebGL) e toque (Android). Já é assim.

### 6.4 Acessibilidade

- **Modo daltônico:** a paleta "Neon" vira "Âmbar/Azul" (perigo âmbar, seguro azul). É só outra `GamePalette`. Grátis com a arquitetura atual.
- **Anel de massa:** opção que desenha um anel numérico ao redor da gota para quem não percebe a escala.
- **Vibração:** opcional, curta (20 ms) no toque e longa (200 ms) na morte.
- **Sem texto obrigatório durante a partida.** A dica inicial é um pictograma + texto.
- Botões ≥ 44 pt, contraste ≥ 4,5:1 (já atendido com a paleta).

---

## 7. Áudio

### 7.1 Direção

Eletrônico ambiente, frio, com pulso. A música é o **velocímetro**: quanto mais rápido, mais camadas. Técnica: *vertical layering* com 4 faixas em loop sincronizadas, cada uma ligando acima de uma velocidade.

| Camada | Liga em | Conteúdo |
|---|---|---|
| 1. Pad | sempre | Acorde grave sustentado, reverb longo |
| 2. Baixo pulsado | 4 m/s | O que já existe (baixo + arpejo pentatônico) |
| 3. Percussão | 6 m/s | *Hi-hat* sintetizado, *kick* abafado |
| 4. Lead | 8 m/s | Melodia aguda, *delay* |

Por bioma, muda a tônica (Ferrugem em Dó menor, Neon em Ré menor, Cristal em Mi maior...). Tudo continua **sintetizado por código** (já temos o gerador de WAV); o custo é zero e combina com a estética.

### 7.2 Efeitos sonoros (lista completa)

| Evento | Som | Nota |
|---|---|---|
| Toque | "ploc" curto | Pitch sobe conforme a massa cai (gota menor, som mais agudo) |
| Coleta | 2 notas | A 2ª nota sobe um tom a cada passo do combo |
| Combo quebrado | Nota descendente seca | |
| Laser aviso | "tic" agudo | 0,4 s antes de ligar. **Obrigatório** |
| Laser ligado | Zumbido | |
| Escudo pego | Acorde de vidro | |
| Escudo quebrado | Vidro estilhaçando + impacto | |
| Portal | *Riser* de 1,5 s + *whoosh* | A música sobe um semitom durante |
| Morte por colisão | Impacto grave + estrela de luz (ruído branco curto) | |
| Evaporação | "Tsss" + a música perde os graves em 1 s | Diferente da colisão de propósito |
| Novo recorde | Arpejo ascendente | |
| UI | Clique suave | |

---

## 8. Técnico

### 8.1 O que já existe (e fica)

| Sistema | Arquivo | Estado |
|---|---|---|
| Eventos | `Core/GameEvents.cs` | Pronto. Ganha `OnMassChanged`, `OnPlayerEvaporated`, `OnComboChanged`, `OnBiomeChanged`, `OnShieldChanged` |
| Máquina de estados | `Core/GameStateMachine.cs` | Pronto. `GameOverState` recebe `DeathCause` |
| Pool | `Core/ObjectPool.cs` | Pronto. Genérico, serve para 3D igual |
| Spawner | `Obstacles/ObstacleSpawner.cs` | Ganha pesos por bioma e `IsFair()` |
| Dificuldade | `Data/DifficultyConfig.cs` | Ganha os parâmetros de massa e a lista de biomas |
| Paleta | `Data/GamePalette.cs` + `PaletteColor.cs` | Pronto. Uma paleta por bioma; `BiomeManager` troca com *crossfade* de 1,5 s |
| Pontuação | `Core/ScoreManager.cs` | Ganha combo |
| UI | `UI/UIManager.cs` | Ganha tela de skins e opções |
| Áudio | `Audio/AudioManager.cs` | Ganha camadas por velocidade |

### 8.2 Migração para 2.5D (opção B)

O que muda, em ordem:

1. **Cena nova** `Tunnel.unity` com câmera em perspectiva (FOV 60°) em `(0, 0, 0)` olhando para `+Z`. O "cano" se estende em Z. A gota fica em `z = 3`, nas paredes `x = ±1,2`.
2. **`WorldScroller`** passa a expor `Vector3 ScrollDirection = -Z` (as coisas vêm de longe para a câmera). `ScrollingSegment` e `Scrollable` usam essa direção. São 3 linhas.
3. **Colisão:** duas opções. (a) Trocar para `Rigidbody`/`Collider` 3D e `OnTriggerEnter`. (b) **Manter 2D**: toda a lógica de colisão acontece num plano XY em `z = 3`; os obstáculos têm um `BoxCollider2D` filho que só "existe" quando o obstáculo cruza `z = 3`. (a) é mais limpo. Vamos de (a); é trocar `2D` por nada em meia dúzia de tipos.
4. **Visual do túnel:** 4 quads por segmento (chão, teto, duas paredes) com material de concreto escuro + um quad fino emissivo por aro de neon. Segmento de 10 m, 6 segmentos vivos, reciclados como hoje.
5. **Gota:** esfera com shader de vidro (URP Lit, *transparent*, *Opaque Texture* ligado para refração). *Squash & stretch* por escala não-uniforme.
6. **Bloom:** URP Volume global, `threshold 1.0`, `intensity 0.8`, só o neon passa (emissão HDR = 2.0).
7. **Câmera:** leve *lag* ao trocar de parede (0,05 unidades em X, 0,1 s) e *roll* de 2° para dar peso. `CameraShake` continua.

Estimativa: **8 a 10 sessões de 2 h** (um marco). Nenhum sistema de jogo é reescrito.

### 8.3 Orçamento de desempenho (Android intermediário, 60 FPS = 16,6 ms)

| Item | Orçamento |
|---|---|
| CPU total | ≤ 8 ms |
| Spawner + scroll | ≤ 0,5 ms (zero alocação) |
| Partículas | ≤ 1 ms, máx. 200 vivas |
| Bloom | ≤ 2 ms (renderizar a 50 % e subir) |
| Draw calls | ≤ 60 (atlas + *SRP Batcher*) |
| Memória | ≤ 150 MB |
| Tamanho do APK | ≤ 40 MB |

Se o bloom estourar, o *fallback* é um sprite de brilho pré-desenhado atrás de cada linha de neon (zero pós-processamento).

### 8.4 Dados salvos (PlayerPrefs → JSON em [1.1])

```json
{
  "highScore": 1240,
  "bestDistance": 1180,
  "bestCombo": 7,
  "totalDrops": 843,
  "unlockedSkins": ["water", "oil"],
  "selectedSkin": "oil",
  "biomesReached": 3,
  "settings": { "sound": true, "music": true, "vibration": true, "colorblind": false, "massRing": false },
  "hintShown": true
}
```

---

## 9. Escopo por versão

| | **1.0 — Lançamento** | **1.1 — Profundidade** | **2.0 — Visão** |
|---|---|---|---|
| Arte | 2.5D túnel, biomas Ferrugem + Neon, bloom | Cristal + Vapor | Vazio |
| Mecânicas | Evaporação, laser, combo, escudo, portal | Calor, jato, lâmina, cristal, ímã, gelo | Grade, Hardcore |
| Meta | Recorde, 3 skins | 7 skins, conquistas, diário, Zen, estatísticas | Ranking online |
| Áudio | 2 camadas + SFX completo | 4 camadas, tônica por bioma | |
| Plataformas | Android + WebGL | | iOS |
| Monetização | Nenhuma | Anúncio recompensado para continuar 1 vez | |

**Regra do plano original continua valendo:** se atrasar, corta polimento, nunca a publicação. Se a migração 2.5D passar de 3 semanas, lança 1.0 em 2D neon (opção A) e a 2.5D vira a 1.1.

---

## 10. Plano de marcos revisado

O plano original (Marcos 0–4) está feito até o 4 parcial. Entram três marcos antes de publicar:

### Marco 5 — Direção de arte 2.5D (2 semanas)
- [ ] Cena `Tunnel` com câmera em perspectiva, 6 segmentos reciclados — túnel rola a 60 FPS no celular
- [ ] Materiais: concreto, neon emissivo, vidro da gota — captura de tela parece o quadro 01
- [ ] Bloom no URP, medido no Profiler — ≤ 2 ms
- [ ] Portar `WorldScroller`, `Scrollable`, colisão para 3D — partida completa jogável na cena nova
- [ ] Paletas Ferrugem e Neon + `BiomeManager` com portal — troca em 300 m sem engasgo
- [ ] Abertura do cano pingando (3 s, pulável) e gota idle no menu — vídeo-conceito vira menu

### Marco 6 — Evaporação e tensão (1 semana)
- [ ] `DropMass` + escala + eventos — gota encolhe visivelmente em 25 s
- [ ] Feedback de massa baixa (anel, partículas, pitch) — tester novo entende por que morreu sem explicação
- [ ] Morte por evaporação com tela "Evaporou" — diferente da colisão
- [ ] Laser pulsante com aviso sonoro — 0 mortes "injustas" em 20 partidas de teste
- [ ] Combo e escudo de cristal — aparecem, funcionam, têm som
- [ ] `ObstacleSpawner.IsFair()` — nunca gera combinação impossível (teste automatizado)
- [ ] Balancear `DifficultyConfig` com 20 partidas — 40/60 evaporação/colisão

### Marco 7 — Meta e polimento (1 semana)
- [ ] 3 skins com `DropSkin` SO e tela de skins — troca sem reiniciar
- [ ] Tela de opções (som, música, vibração, daltônico, anel) — tudo persiste
- [ ] Música em 2 camadas por velocidade — camada 2 entra em 6 m/s
- [ ] Vibração opcional — funciona no Android
- [ ] Perfilar 3 min de partida no celular — 60 FPS estáveis

Depois, o **Marco 4 original** (publicação) segue como está: GIF, itch.io, Play Console, LinkedIn.

---

## 11. Perguntas abertas (para o Hiago decidir)

1. **2.5D agora ou depois?** A recomendação é agora (Marco 5), porque muda capturas, GIF, ícone e página do itch.io. Fazer depois significa refazer o material de divulgação.
2. **Evaporação na 1.0?** É a mecânica que diferencia o jogo de "mais um one-tap". Mas adiciona uma forma de morrer que precisa ser muito bem comunicada. Se os testes com amigos mostrarem confusão, cai para 1.1.
3. **Nome:** "Gota" é curto e bom em português. Para itch.io/Play internacional, "Gota" ou "Drop"? Sugestão: **"Gota"** como marca, com subtítulo em inglês na loja ("Gota — one-tap fall").
4. **Abertura cinematográfica:** 3 s pulável toda vez que abre o app, ou só na primeira vez? Sugestão: só na primeira vez e depois como idle do menu.
5. **Portal invulnerável:** mantém, ou o portal pode ter obstáculos "fantasma" para não virar descanso? Sugestão: mantém; é o único respiro do jogo.
6. **Anúncio para continuar (1.1):** uma vez por partida, ou nunca? Decide depois de ver a retenção sem anúncio.

---

## 12. Glossário rápido

- **Bioma:** trecho de 300 m com paleta e obstáculos próprios.
- **Massa:** quanto de água a gota ainda tem (0 a 1). Define o tamanho.
- **Portal:** transição de bioma, 1,5 s invulnerável a velocidade x2.
- **Combo:** gotinhas seguidas sem perder nenhuma.
- **Escudo:** cristal que absorve uma colisão.
- **2.5D:** cena 3D com gameplay em um único eixo.
