using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Gota
{
    /// <summary>
    /// Troca o bioma (paleta do túnel) a cada N metros, com um portal no meio:
    /// 1,5 s invulnerável a velocidade dobrada, sem obstáculos novos, cores
    /// interpolando. Só lê ScoreManager.Distance; nunca é chamado pelo gameplay.
    /// </summary>
    public class BiomeManager : MonoBehaviour
    {
        [Header("Biomas (em ordem)")]
        [SerializeField] private GamePalette[] biomes;
        [Tooltip("Metros por bioma. O último bioma continua até o fim da partida.")]
        [SerializeField] private float biomeLength = 300f;

        [Header("Portal")]
        [SerializeField] private float portalDuration = 1.5f;
        [SerializeField] private float portalSpeedMultiplier = 2f;
        [SerializeField] private PortalRing portal;
        [SerializeField] private CanvasGroup biomeLabel;
        [SerializeField] private Text biomeLabelText;

        [Header("Dependências")]
        [SerializeField] private ScoreManager score;
        [SerializeField] private WorldScroller scroller;
        [SerializeField] private PlayerController player;
        [SerializeField] private ObstacleSpawner spawner;

        public int CurrentBiome { get; private set; }
        public bool InPortal { get; private set; }

        private Coroutine portalRoutine;

        private void OnEnable() => GameEvents.OnStateChanged += HandleState;
        private void OnDisable() => GameEvents.OnStateChanged -= HandleState;

        private void Start()
        {
            ApplyInstant(0);
        }

        private void HandleState(GameState state)
        {
            bool newRun = state == GameState.Playing && !GameStateMachine.Instance.IsResuming;
            if (state == GameState.Menu || newRun)
            {
                CancelPortal();
                ApplyInstant(0);
            }
        }

        private void Update()
        {
            if (InPortal || biomes == null || biomes.Length == 0) return;
            if (GameStateMachine.Instance == null || GameStateMachine.Instance.Current != GameState.Playing) return;

            int target = Mathf.Min(biomes.Length - 1, Mathf.FloorToInt(score.Distance / biomeLength));
            if (target != CurrentBiome)
            {
                portalRoutine = StartCoroutine(PortalTo(target));
            }
        }

        private IEnumerator PortalTo(int target)
        {
            InPortal = true;
            int from = CurrentBiome;
            spawner.Stop();                      // o portal é o único respiro do jogo
            player.Invulnerable = true;
            scroller.SpeedMultiplier = portalSpeedMultiplier;
            if (portal != null) portal.Play();
            if (biomeLabelText != null) biomeLabelText.text = biomes[target].name.ToUpperInvariant();
            GameEvents.RaiseBiomeChanged(target);

            float elapsed = 0f;
            while (elapsed < portalDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / portalDuration);
                TunnelMaterials.Instance?.Apply(biomes[from], biomes[target], Mathf.SmoothStep(0f, 1f, t));
                // Nome do bioma: aparece no meio do portal e some no fim.
                if (biomeLabel != null) biomeLabel.alpha = Mathf.Sin(t * Mathf.PI);
                yield return null;
            }

            CurrentBiome = target;
            FinishPortal();
            spawner.Begin();
            portalRoutine = null;
        }

        private void CancelPortal()
        {
            if (portalRoutine != null) StopCoroutine(portalRoutine);
            portalRoutine = null;
            FinishPortal();
        }

        private void FinishPortal()
        {
            InPortal = false;
            player.Invulnerable = false;
            scroller.SpeedMultiplier = 1f;
            if (biomeLabel != null) biomeLabel.alpha = 0f;
            if (portal != null) portal.Hide();
        }

        private void ApplyInstant(int index)
        {
            if (biomes == null || biomes.Length == 0) return;
            CurrentBiome = index;
            TunnelMaterials.Instance?.Apply(biomes[index], biomes[index], 1f);
        }
    }
}
