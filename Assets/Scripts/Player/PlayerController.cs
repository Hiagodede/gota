using UnityEngine;
using UnityEngine.InputSystem;

namespace Gota
{
    /// <summary>
    /// A gota. Um toque em qualquer lugar da tela a leva para a parede oposta.
    /// Não conhece UI nem áudio: só dispara eventos em GameEvents.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Paredes")]
        [SerializeField] private float leftX = -3f;
        [SerializeField] private float rightX = 3f;

        [Header("Movimento")]
        [Tooltip("Tempo da travessia entre paredes, em segundos (meta do plano: < 0,2 s).")]
        [SerializeField] private float switchDuration = 0.15f;

        [Tooltip("Começa grudada na parede esquerda?")]
        [SerializeField] private bool startOnLeft = true;

        /// <summary>Quando falso, ignora toques (menu, pausa, game over).</summary>
        public bool InputEnabled { get; set; } = true;

        public bool IsOnLeft { get; private set; }

        private Rigidbody2D rb;
        private bool isSwitching;
        private float switchTimer;
        private float fromX;
        private float toX;
        private float startY;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true; // necessário para triggers com colliders estáticos
            startY = transform.position.y;
        }

        private void OnEnable()
        {
            ResetToWall();
        }

        /// <summary>Volta a gota para a posição inicial (usado ao começar uma partida).</summary>
        public void ResetToWall()
        {
            IsOnLeft = startOnLeft;
            isSwitching = false;
            float x = IsOnLeft ? leftX : rightX;
            rb.position = new Vector2(x, startY);
            transform.position = rb.position;
        }

        private void Update()
        {
            if (InputEnabled && WasTapped())
            {
                SwitchWall();
            }
        }

        private static bool WasTapped()
        {
            // Pointer cobre mouse (editor) e tela de toque (celular).
            var pointer = Pointer.current;
            return pointer != null && pointer.press.wasPressedThisFrame;
        }

        /// <summary>Inicia a travessia para a parede oposta. Pode ser chamada no meio de uma travessia.</summary>
        public void SwitchWall()
        {
            IsOnLeft = !IsOnLeft;
            fromX = rb.position.x;
            toX = IsOnLeft ? leftX : rightX;
            switchTimer = 0f;
            isSwitching = true;
            GameEvents.RaiseWallSwitched();
        }

        private void FixedUpdate()
        {
            if (!isSwitching) return;

            switchTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(switchTimer / switchDuration);
            // Ease-out: sai rápido e chega suave na parede.
            float eased = 1f - (1f - t) * (1f - t);
            float x = Mathf.Lerp(fromX, toX, eased);
            rb.MovePosition(new Vector2(x, startY));

            if (t >= 1f) isSwitching = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!InputEnabled) return;

            if (other.TryGetComponent<Collectible>(out var collectible))
            {
                collectible.Collect();
                return;
            }

            if (other.GetComponent<Obstacle>() != null)
            {
                GameEvents.RaisePlayerDied();
            }
        }
    }
}
