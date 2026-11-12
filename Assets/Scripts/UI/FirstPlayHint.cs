using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Mostra "Toque para trocar de parede" só na primeira partida do aparelho.
    /// Some no primeiro toque ou depois de alguns segundos.
    /// </summary>
    public class FirstPlayHint : MonoBehaviour
    {
        private const string Key = "Gota.HintShown";

        [SerializeField] private GameObject hintPanel;
        [SerializeField] private float autoHideSeconds = 4f;

        private float timer;
        private bool showing;

        private void OnEnable()
        {
            GameEvents.OnStateChanged += HandleState;
            GameEvents.OnWallSwitched += Hide;
            hintPanel.SetActive(false);
        }

        private void OnDisable()
        {
            GameEvents.OnStateChanged -= HandleState;
            GameEvents.OnWallSwitched -= Hide;
        }

        private void HandleState(GameState state)
        {
            if (state == GameState.Playing && PlayerPrefs.GetInt(Key, 0) == 0)
            {
                PlayerPrefs.SetInt(Key, 1);
                PlayerPrefs.Save();
                hintPanel.SetActive(true);
                showing = true;
                timer = autoHideSeconds;
            }
            else if (state != GameState.Playing && state != GameState.Paused)
            {
                Hide();
            }
        }

        private void Update()
        {
            if (!showing) return;
            timer -= Time.deltaTime;
            if (timer <= 0f) Hide();
        }

        private void Hide()
        {
            if (!showing) return;
            showing = false;
            hintPanel.SetActive(false);
        }
    }
}
