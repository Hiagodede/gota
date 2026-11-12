using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Qualquer coisa que sobe junto com o cenário (obstáculos, coletáveis).
    /// Avisa quando sai pelo topo da tela para quem quiser reciclar o objeto.
    /// </summary>
    public class Scrollable : MonoBehaviour
    {
        [Tooltip("Acima desta altura o objeto já saiu da tela.")]
        [SerializeField] private float despawnY = 10f;

        public event System.Action<Scrollable> OnLeftScreen;

        private void Update()
        {
            var scroller = WorldScroller.Instance;
            if (scroller == null) return;

            Vector3 pos = transform.position;
            pos.y += scroller.DeltaThisFrame;
            transform.position = pos;

            if (pos.y >= despawnY)
            {
                OnLeftScreen?.Invoke(this);
            }
        }
    }
}
