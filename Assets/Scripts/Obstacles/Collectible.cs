using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Gotinha coletável: vale pontos extras. Some ao ser pega.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        [SerializeField] private int value = 10;

        public event System.Action<Collectible> OnCollectedBy;

        public void Collect()
        {
            GameEvents.RaiseCollected(value);
            OnCollectedBy?.Invoke(this);
            gameObject.SetActive(false);
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }
    }
}
