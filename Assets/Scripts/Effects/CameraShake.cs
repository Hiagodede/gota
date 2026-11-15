using System.Collections;
using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Tremida curta de câmera. Chame Shake(); a câmera volta sozinha ao lugar.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private float magnitude = 0.25f;

        private Vector3 restPosition;
        private Coroutine running;

        private void Awake()
        {
            restPosition = transform.localPosition;
        }

        public void Shake()
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float strength = magnitude * (1f - elapsed / duration);
                Vector2 offset = Random.insideUnitCircle * strength;
                // Treme nos eixos da própria câmera (direita/cima), para valer em 2D e em 3D.
                transform.localPosition = restPosition + transform.right * offset.x + transform.up * offset.y;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            transform.localPosition = restPosition;
            running = null;
        }
    }
}
