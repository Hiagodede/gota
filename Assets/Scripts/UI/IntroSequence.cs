using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gota
{
    /// <summary>
    /// Abertura: um cano enferrujado pinga, a gota cresce na boca do cano e cai
    /// até a parede. Só na primeira vez que o app abre (chave em PlayerPrefs);
    /// depois, o cano fica como cenário do menu. Um toque pula.
    /// </summary>
    public class IntroSequence : MonoBehaviour
    {
        public const string PrefKey = "Gota.IntroShown";

        [Header("Referências")]
        [SerializeField] private Transform dropVisual;   // filho "Visual" do Player
        [SerializeField] private Canvas canvas;          // some durante a abertura
        [SerializeField] private ParticleSystem splash;  // opcional
        [SerializeField] private GameObject pipe;        // cano: só aparece fora da partida

        [Header("Tempos (segundos)")]
        [SerializeField] private float growDuration = 1.6f;
        [SerializeField] private float fallDuration = 0.55f;
        [SerializeField] private float settleDuration = 0.4f;

        [Tooltip("Posição local (no Player) da boca do cano, onde a gota nasce.")]
        [SerializeField] private Vector3 spoutLocalPosition = new Vector3(0f, 1.5f, 3.2f);

        [Tooltip("Marque para ver a abertura toda vez (teste).")]
        [SerializeField] private bool alwaysPlay = false;

        public bool IsPlaying { get; private set; }

        private Vector3 restScale;
        private Vector3 restPosition;
        private bool skipRequested;

        private void Start()
        {
            if (dropVisual == null) return;
            restScale = dropVisual.localScale;
            restPosition = dropVisual.localPosition;

            bool shown = PlayerPrefs.GetInt(PrefKey, 0) == 1;
            if (shown && !alwaysPlay) return;
            StartCoroutine(Play());
        }

        private void OnEnable() => GameEvents.OnStateChanged += HandleState;
        private void OnDisable() => GameEvents.OnStateChanged -= HandleState;

        private void HandleState(GameState state)
        {
            if (pipe != null) pipe.SetActive(state == GameState.Menu || state == GameState.GameOver);
        }

        private void Update()
        {
            if (!IsPlaying) return;
            var pointer = Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame) skipRequested = true;
        }

        private IEnumerator Play()
        {
            IsPlaying = true;
            skipRequested = false;
            if (canvas != null) canvas.enabled = false;
            var idle = dropVisual.GetComponent<MenuIdle>();
            if (idle != null) idle.enabled = false;

            // 1. Cresce na boca do cano.
            dropVisual.localPosition = spoutLocalPosition;
            float t = 0f;
            while (t < growDuration && !skipRequested)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / growDuration);
                dropVisual.localScale = restScale * Mathf.Lerp(0.15f, 1f, k);
                // alonga para baixo (eixo Z é "para baixo" na tela) conforme pesa
                dropVisual.localScale = new Vector3(dropVisual.localScale.x, dropVisual.localScale.y, dropVisual.localScale.z * (1f + 0.35f * k));
                yield return null;
            }

            // 2. Cai (acelerando) até a parede.
            t = 0f;
            while (t < fallDuration && !skipRequested)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fallDuration);
                float eased = k * k; // ease-in: gravidade
                dropVisual.localPosition = Vector3.Lerp(spoutLocalPosition, restPosition, eased);
                dropVisual.localScale = new Vector3(restScale.x, restScale.y, restScale.z * Mathf.Lerp(1.35f, 1f, k));
                yield return null;
            }

            // 3. Assenta: esmaga e volta.
            if (splash != null && !skipRequested)
            {
                splash.transform.position = dropVisual.position;
                splash.Play();
            }
            t = 0f;
            while (t < settleDuration && !skipRequested)
            {
                t += Time.deltaTime;
                float k = t / settleDuration;
                float squash = 1f + Mathf.Sin(k * Mathf.PI) * 0.25f * (1f - k);
                dropVisual.localScale = new Vector3(restScale.x * squash, restScale.y * squash, restScale.z / squash);
                yield return null;
            }

            dropVisual.localScale = restScale;
            dropVisual.localPosition = restPosition;
            if (idle != null) idle.enabled = true;
            if (canvas != null) canvas.enabled = true;
            PlayerPrefs.SetInt(PrefKey, 1);
            PlayerPrefs.Save();
            IsPlaying = false;
        }
    }
}
