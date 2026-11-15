using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Pedaço de cenário (parede ou fundo) que sobe e, ao sair pelo topo,
    /// volta para baixo. Dois segmentos por coluna bastam para um loop infinito.
    /// </summary>
    public class ScrollingSegment : MonoBehaviour
    {
        [Tooltip("Altura de um segmento. O loop total é 2x este valor.")]
        [SerializeField] private float segmentHeight = 18f;

        [Tooltip("Quando o centro passar desta altura, o segmento volta para baixo.")]
        [SerializeField] private float wrapY = 18f;

        [Tooltip("1 = mesma velocidade do cenário; menor = fundo distante (parallax).")]
        [SerializeField] private float speedMultiplier = 1f;

        [Tooltip("Comprimento total do loop. 0 = 2x a altura do segmento (dois segmentos por coluna).")]
        [SerializeField] private float loopLength = 0f;

        private float LoopLength => loopLength > 0f ? loopLength : segmentHeight * 2f;

        private void Update()
        {
            var scroller = WorldScroller.Instance;
            if (scroller == null) return;

            Vector3 pos = transform.position;
            pos.y += scroller.DeltaThisFrame * speedMultiplier;
            if (pos.y >= wrapY)
            {
                pos.y -= LoopLength;
            }
            transform.position = pos;
        }
    }
}
