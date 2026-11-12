using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Conta metros descidos e pontos. 1 ponto por metro + bônus dos coletáveis.
    /// Também aplica a curva de dificuldade na velocidade do cenário.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private DifficultyConfig difficulty;
        [SerializeField] private WorldScroller scroller;

        public float Distance { get; private set; }
        public int Bonus { get; private set; }
        public int Score => Mathf.FloorToInt(Distance) + Bonus;

        /// <summary>Intervalo atual entre obstáculos, já com a dificuldade aplicada.</summary>
        public float CurrentSpawnInterval => difficulty.IntervalAt(Distance);
        public float CollectibleChance => difficulty.collectibleChance;
        public bool MoversUnlocked => Distance >= difficulty.moverUnlockDistance;

        private int lastReportedScore = -1;

        private void OnEnable() => GameEvents.OnCollected += HandleCollected;
        private void OnDisable() => GameEvents.OnCollected -= HandleCollected;

        public void ResetRun()
        {
            Distance = 0f;
            Bonus = 0;
            lastReportedScore = -1;
            scroller.Speed = difficulty.startSpeed;
            Report();
        }

        private void Update()
        {
            if (!scroller.IsScrolling) return;

            Distance += scroller.DeltaThisFrame;
            scroller.Speed = difficulty.SpeedAt(Distance);
            Report();
        }

        private void HandleCollected(int value)
        {
            Bonus += value;
            Report();
        }

        private void Report()
        {
            int s = Score;
            if (s != lastReportedScore)
            {
                lastReportedScore = s;
                GameEvents.RaiseScoreChanged(s);
            }
        }
    }
}
