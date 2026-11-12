using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Guarda o recorde no aparelho com PlayerPrefs.
    /// </summary>
    public static class HighScoreService
    {
        private const string Key = "Gota.HighScore";

        public static int Load() => PlayerPrefs.GetInt(Key, 0);

        /// <summary>Salva se for maior que o recorde atual. Retorna true quando houve recorde novo.</summary>
        public static bool TrySubmit(int score)
        {
            if (score <= Load()) return false;

            PlayerPrefs.SetInt(Key, score);
            PlayerPrefs.Save();
            GameEvents.RaiseHighScoreChanged(score);
            return true;
        }
    }
}
