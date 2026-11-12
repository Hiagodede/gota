using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Ouvinte de desenvolvimento: escreve "Game Over" no console quando a gota morre.
    /// </summary>
    public class GameOverLogger : MonoBehaviour
    {
        private void OnEnable() => GameEvents.OnPlayerDied += HandleDied;
        private void OnDisable() => GameEvents.OnPlayerDied -= HandleDied;

        private void HandleDied()
        {
            Debug.Log("Game Over");
        }
    }
}
