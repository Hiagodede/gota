using UnityEngine;

namespace Gota
{
    public enum ObstacleKind
    {
        Block,   // bloco fixo na parede
        Spike,   // espinho fixo na parede
        Mover    // bloco que vai e volta entre as paredes
    }

    /// <summary>
    /// Marca um objeto como perigoso. Encostar nele termina a partida.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private ObstacleKind kind = ObstacleKind.Block;

        public ObstacleKind Kind => kind;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }
    }
}
