using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Curva de dificuldade como ScriptableObject: dá para ajustar no Inspector
    /// (ou criar variantes "fácil"/"difícil") sem mexer em código.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "Gota/Difficulty Config")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Velocidade de queda (m/s)")]
        [Min(0.1f)] public float startSpeed = 4f;
        [Tooltip("Quanto a velocidade aumenta a cada 100 metros.")]
        [Min(0f)] public float speedStepPer100m = 0.6f;
        [Min(0.1f)] public float maxSpeed = 10f;

        [Header("Intervalo entre obstáculos (segundos)")]
        [Min(0.1f)] public float startInterval = 1.6f;
        [Tooltip("Quanto o intervalo diminui a cada 100 metros.")]
        [Min(0f)] public float intervalStepPer100m = 0.12f;
        [Min(0.1f)] public float minInterval = 0.7f;

        [Header("Coletáveis")]
        [Range(0f, 1f)] public float collectibleChance = 0.3f;
        [Min(0)] public int collectibleValue = 10;

        [Header("Obstáculos que se movem")]
        [Tooltip("Metros percorridos antes do primeiro obstáculo móvel aparecer.")]
        [Min(0f)] public float moverUnlockDistance = 150f;

        public float SpeedAt(float distanceMeters)
        {
            float steps = Mathf.Floor(distanceMeters / 100f);
            return Mathf.Min(maxSpeed, startSpeed + steps * speedStepPer100m);
        }

        public float IntervalAt(float distanceMeters)
        {
            float steps = Mathf.Floor(distanceMeters / 100f);
            return Mathf.Max(minInterval, startInterval - steps * intervalStepPer100m);
        }
    }
}
