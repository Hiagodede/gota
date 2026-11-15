using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Partículas e tremida de câmera. Só escuta GameEvents.
    /// </summary>
    public class EffectsManager : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private ParticleSystem switchBurst;
        [SerializeField] private ParticleSystem deathBurst;
        [SerializeField] private ParticleSystem collectBurst;
        [SerializeField] private CameraShake cameraShake;
        [Tooltip("Renderer da gota (sprite na cena 2D, esfera na cena 3D). Some ao morrer.")]
        [SerializeField] private Renderer playerSprite;

        private void OnEnable()
        {
            GameEvents.OnWallSwitched += HandleSwitch;
            GameEvents.OnPlayerDied += HandleDeath;
            GameEvents.OnCollected += HandleCollected;
            GameEvents.OnStateChanged += HandleState;
        }

        private void OnDisable()
        {
            GameEvents.OnWallSwitched -= HandleSwitch;
            GameEvents.OnPlayerDied -= HandleDeath;
            GameEvents.OnCollected -= HandleCollected;
            GameEvents.OnStateChanged -= HandleState;
        }

        private void HandleSwitch()
        {
            PlayAt(switchBurst, player.position);
        }

        private void HandleDeath()
        {
            PlayAt(deathBurst, player.position);
            if (cameraShake != null) cameraShake.Shake();
            SetPlayerVisible(false);
        }

        private void HandleCollected(int value)
        {
            PlayAt(collectBurst, player.position);
        }

        private void HandleState(GameState state)
        {
            if (state != GameState.GameOver) SetPlayerVisible(true);
        }

        private void SetPlayerVisible(bool visible)
        {
            if (playerSprite == null) return;
            // Inclui filhos: na cena 3D a gota é vidro + núcleo.
            foreach (var r in playerSprite.GetComponentsInChildren<Renderer>(true)) r.enabled = visible;
        }

        private static void PlayAt(ParticleSystem ps, Vector3 position)
        {
            if (ps == null) return;
            ps.transform.position = position;
            ps.Play();
        }
    }
}
