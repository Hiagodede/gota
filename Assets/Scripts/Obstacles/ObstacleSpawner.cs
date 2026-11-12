using System.Collections.Generic;
using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Sorteia e posiciona obstáculos e coletáveis abaixo da tela.
    /// Usa um ObjectPool por prefab: zero Instantiate/Destroy durante a partida.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private Obstacle blockPrefab;
        [SerializeField] private Obstacle spikePrefab;
        [SerializeField] private Obstacle moverPrefab;
        [SerializeField] private Collectible collectiblePrefab;

        [Header("Posições")]
        [SerializeField] private float spawnY = -10f;
        [Tooltip("Centro X de um obstáculo grudado na parede.")]
        [SerializeField] private float wallX = 2.9f;
        [SerializeField] private float collectibleOffsetY = -1.5f;

        [Header("Pool")]
        [SerializeField] private int prewarmPerType = 8;

        [Header("Dependências")]
        [SerializeField] private ScoreManager score;

        private ObjectPool<Obstacle> blockPool;
        private ObjectPool<Obstacle> spikePool;
        private ObjectPool<Obstacle> moverPool;
        private ObjectPool<Collectible> collectiblePool;

        private readonly List<Obstacle> activeObstacles = new List<Obstacle>();
        private readonly List<Collectible> activeCollectibles = new List<Collectible>();

        private bool running;
        private float timer;
        private bool lastWasLeft;

        public int ActiveCount => activeObstacles.Count + activeCollectibles.Count;

        private void Awake()
        {
            blockPool = new ObjectPool<Obstacle>(blockPrefab, transform, prewarmPerType);
            spikePool = new ObjectPool<Obstacle>(spikePrefab, transform, prewarmPerType);
            moverPool = new ObjectPool<Obstacle>(moverPrefab, transform, prewarmPerType / 2);
            collectiblePool = new ObjectPool<Collectible>(collectiblePrefab, transform, prewarmPerType);
        }

        public void Begin()
        {
            running = true;
            timer = 0.6f; // primeiro obstáculo chega rápido, mas dá tempo de reagir
        }

        public void Stop() => running = false;

        /// <summary>Devolve tudo ao pool (fim de partida ou volta ao menu).</summary>
        public void Clear()
        {
            running = false;
            for (int i = activeObstacles.Count - 1; i >= 0; i--) Release(activeObstacles[i]);
            for (int i = activeCollectibles.Count - 1; i >= 0; i--) Release(activeCollectibles[i]);
        }

        private void Update()
        {
            if (!running) return;

            timer -= Time.deltaTime;
            if (timer > 0f) return;

            SpawnOne();
            timer = score.CurrentSpawnInterval;
        }

        private void SpawnOne()
        {
            // Sorteio do tipo: móvel só depois de certa distância.
            ObstacleKind kind;
            float roll = Random.value;
            if (score.MoversUnlocked && roll < 0.25f) kind = ObstacleKind.Mover;
            else if (roll < 0.6f) kind = ObstacleKind.Block;
            else kind = ObstacleKind.Spike;

            // Alterna o lado com um pouco de sorte para não ficar previsível.
            bool left = Random.value < 0.5f ? !lastWasLeft : lastWasLeft;
            lastWasLeft = left;

            Obstacle obstacle = PoolFor(kind).Get();
            float x = kind == ObstacleKind.Mover ? 0f : (left ? -wallX : wallX);
            obstacle.transform.position = new Vector3(x, spawnY, 0f);
            obstacle.transform.rotation = Quaternion.Euler(0f, 0f, SpikeRotation(kind, left));

            var scrollable = obstacle.GetComponent<Scrollable>();
            scrollable.OnLeftScreen -= HandleObstacleLeft;
            scrollable.OnLeftScreen += HandleObstacleLeft;
            activeObstacles.Add(obstacle);

            // Coletável do lado oposto, um pouco abaixo.
            if (Random.value < score.CollectibleChance)
            {
                Collectible c = collectiblePool.Get();
                float cx = kind == ObstacleKind.Mover ? (left ? -wallX : wallX) : -x;
                c.transform.position = new Vector3(cx, spawnY + collectibleOffsetY, 0f);
                var cs = c.GetComponent<Scrollable>();
                cs.OnLeftScreen -= HandleCollectibleLeft;
                cs.OnLeftScreen += HandleCollectibleLeft;
                c.OnCollectedBy -= Release;
                c.OnCollectedBy += Release;
                activeCollectibles.Add(c);
            }
        }

        private static float SpikeRotation(ObstacleKind kind, bool left)
        {
            if (kind != ObstacleKind.Spike) return 0f;
            // Triângulo aponta para cima por padrão; vira para o centro do cano.
            return left ? -90f : 90f;
        }

        private ObjectPool<Obstacle> PoolFor(ObstacleKind kind)
        {
            switch (kind)
            {
                case ObstacleKind.Spike: return spikePool;
                case ObstacleKind.Mover: return moverPool;
                default: return blockPool;
            }
        }

        private void HandleObstacleLeft(Scrollable s) => Release(s.GetComponent<Obstacle>());
        private void HandleCollectibleLeft(Scrollable s) => Release(s.GetComponent<Collectible>());

        private void Release(Obstacle obstacle)
        {
            if (obstacle == null) return;
            activeObstacles.Remove(obstacle);
            PoolFor(obstacle.Kind).Return(obstacle);
        }

        private void Release(Collectible c)
        {
            if (c == null) return;
            activeCollectibles.Remove(c);
            collectiblePool.Return(c);
        }
    }
}
