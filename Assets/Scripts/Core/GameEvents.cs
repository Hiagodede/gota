using System;

namespace Gota
{
    /// <summary>
    /// Canal central de eventos do jogo (C# events).
    /// Quem produz (gameplay) dispara; quem consome (UI, som, efeitos) escuta.
    /// Assim o PlayerController nunca precisa conhecer AudioSource nem UI.
    /// </summary>
    public static class GameEvents
    {
        public static event Action OnWallSwitched;
        public static event Action OnPlayerDied;
        public static event Action<int> OnCollected;      // valor coletado
        public static event Action<int> OnScoreChanged;   // pontuação atual
        public static event Action<int> OnHighScoreChanged;
        public static event Action<GameState> OnStateChanged;

        public static void RaiseWallSwitched() => OnWallSwitched?.Invoke();
        public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
        public static void RaiseCollected(int value) => OnCollected?.Invoke(value);
        public static void RaiseScoreChanged(int score) => OnScoreChanged?.Invoke(score);
        public static void RaiseHighScoreChanged(int score) => OnHighScoreChanged?.Invoke(score);
        public static void RaiseStateChanged(GameState state) => OnStateChanged?.Invoke(state);
    }

    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }
}
