using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Toca sons e música reagindo a GameEvents. O gameplay nunca chama isto diretamente.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private const string MutedKey = "Gota.Muted";

        [Header("Fontes")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Clipes")]
        [SerializeField] private AudioClip tapClip;
        [SerializeField] private AudioClip collectClip;
        [SerializeField] private AudioClip deathClip;
        [SerializeField] private AudioClip musicClip;

        [Header("Volumes")]
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.35f;

        public bool IsMuted { get; private set; }
        public event System.Action<bool> OnMuteChanged;

        private void Awake()
        {
            IsMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
            ApplyMute();

            if (musicSource != null && musicClip != null)
            {
                musicSource.clip = musicClip;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        private void OnEnable()
        {
            GameEvents.OnWallSwitched += HandleTap;
            GameEvents.OnCollected += HandleCollected;
            GameEvents.OnPlayerDied += HandleDeath;
        }

        private void OnDisable()
        {
            GameEvents.OnWallSwitched -= HandleTap;
            GameEvents.OnCollected -= HandleCollected;
            GameEvents.OnPlayerDied -= HandleDeath;
        }

        public void ToggleMute()
        {
            IsMuted = !IsMuted;
            PlayerPrefs.SetInt(MutedKey, IsMuted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyMute();
            OnMuteChanged?.Invoke(IsMuted);
        }

        private void ApplyMute()
        {
            AudioListener.volume = IsMuted ? 0f : 1f;
        }

        private void HandleTap() => Play(tapClip);
        private void HandleCollected(int value) => Play(collectClip);
        private void HandleDeath() => Play(deathClip);

        private void Play(AudioClip clip)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        private void OnValidate()
        {
            if (musicSource != null) musicSource.volume = musicVolume;
        }
    }
}
