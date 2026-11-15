using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Respiração da gota fora da partida (menu e game over): escala pulsa de leve
    /// e ela balança um pouco. Em jogo, volta ao tamanho normal.
    /// Coloque no objeto visual da gota (filho do Player), não no Player.
    /// </summary>
    public class MenuIdle : MonoBehaviour
    {
        [SerializeField] private float pulseAmount = 0.05f;
        [SerializeField] private float pulseSpeed = 1.6f;
        [SerializeField] private float swayAmount = 0.08f;

        private Vector3 baseScale;
        private Vector3 basePosition;
        private bool idle = true;

        private void Awake()
        {
            baseScale = transform.localScale;
            basePosition = transform.localPosition;
        }

        private void OnEnable() => GameEvents.OnStateChanged += HandleState;
        private void OnDisable() => GameEvents.OnStateChanged -= HandleState;

        private void HandleState(GameState state)
        {
            idle = state == GameState.Menu || state == GameState.GameOver;
            if (!idle)
            {
                transform.localScale = baseScale;
                transform.localPosition = basePosition;
            }
        }

        private void Update()
        {
            if (!idle) return;
            float t = Time.time * pulseSpeed;
            float pulse = 1f + Mathf.Sin(t) * pulseAmount;
            transform.localScale = new Vector3(baseScale.x * pulse, baseScale.y / pulse, baseScale.z * pulse);
            transform.localPosition = basePosition + new Vector3(0f, 0f, Mathf.Sin(t * 0.7f) * swayAmount);
        }
    }
}
