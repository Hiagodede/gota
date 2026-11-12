using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Fonte única da velocidade de "queda". A gota fica parada no eixo Y;
    /// o cenário e os obstáculos sobem com esta velocidade.
    /// </summary>
    public class WorldScroller : MonoBehaviour
    {
        public static WorldScroller Instance { get; private set; }

        [Tooltip("Velocidade inicial de queda em unidades (metros) por segundo.")]
        [SerializeField] private float speed = 4f;

        [Tooltip("Começa rolando sozinho? (útil para testes sem menu)")]
        [SerializeField] private bool scrollOnStart = true;

        /// <summary>Velocidade atual em metros por segundo. Zero quando o jogo não está rodando.</summary>
        public float Speed { get; set; }

        /// <summary>Verdadeiro enquanto o cenário deve se mover.</summary>
        public bool IsScrolling { get; set; }

        /// <summary>Deslocamento vertical (em metros) a aplicar neste frame.</summary>
        public float DeltaThisFrame => IsScrolling ? Speed * Time.deltaTime : 0f;

        private void Awake()
        {
            Instance = this;
            Speed = speed;
            IsScrolling = scrollOnStart;
        }

        /// <summary>Velocidade configurada no Inspector (ponto de partida de cada partida).</summary>
        public float BaseSpeed => speed;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
