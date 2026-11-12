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
        [SerializeField] private SpriteRenderer playerSprite;

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
            if (playerSprite != null) playerSprite.enabled = false;
        }

        private void HandleCollected(int value)
        {
            PlayAt(collectBurst, player.position);
        }

        private void HandleState(GameState state)
        {
            if (playerSprite != null && state != GameState.GameOver) playerSprite.enabled = true;
        }

        private static void PlayAt(ParticleSystem ps, Vector3 position)
        {
            if (ps == null) return;
            ps.transform.position = position;
            ps.Play();
        }
    }
}
