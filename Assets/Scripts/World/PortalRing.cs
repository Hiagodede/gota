using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Aro grande que atravessa a tela na troca de bioma. Nasce longe, sobe com
    /// o cenário e se esconde ao passar pela câmera.
    /// </summary>
    public class PortalRing : MonoBehaviour
    {
        [SerializeField] private float startY = -14f;
        [SerializeField] private float endY = 14f;

        private void Update()
        {
            var scroller = WorldScroller.Instance;
            if (scroller == null) return;
            Vector3 pos = transform.position;
            pos.y += scroller.DeltaThisFrame;
            transform.position = pos;
            if (pos.y >= endY) Hide();
        }

        public void Play()
        {
            Vector3 pos = transform.position;
            pos.y = startY;
            transform.position = pos;
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
