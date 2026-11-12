using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Obstáculo que vai e volta entre as paredes enquanto sobe.
    /// </summary>
    public class MovingObstacle : MonoBehaviour
    {
        [SerializeField] private float amplitude = 2.9f;
        [SerializeField] private float period = 1.8f;

        private float phase;

        private void OnEnable()
        {
            phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            if (Time.timeScale == 0f) return;
            Vector3 pos = transform.position;
            pos.x = Mathf.Sin(Time.time * (Mathf.PI * 2f / period) + phase) * amplitude;
            transform.position = pos;
        }
    }
}
