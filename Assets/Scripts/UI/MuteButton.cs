using UnityEngine;
using UnityEngine.UI;

namespace Gota
{
    /// <summary>
    /// Botão de mudo: alterna o AudioManager e mostra o estado no rótulo.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MuteButton : MonoBehaviour
    {
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private Text label;
        [SerializeField] private string soundOnText = "SOM: ON";
        [SerializeField] private string soundOffText = "SOM: OFF";

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => audioManager.ToggleMute());
        }

        private void OnEnable()
        {
            audioManager.OnMuteChanged += Refresh;
            Refresh(audioManager.IsMuted);
        }

        private void OnDisable()
        {
            audioManager.OnMuteChanged -= Refresh;
        }

        private void Refresh(bool muted)
        {
            if (label != null) label.text = muted ? soundOffText : soundOnText;
        }
    }
}
