using UnityEngine;
using UnityEngine.UI;

namespace Gota
{
    /// <summary>
    /// Liga cada estado do jogo ao painel certo e mantém os textos atualizados.
    /// Só escuta GameEvents; nunca é chamado pelo gameplay.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Painéis")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("Textos")]
        [SerializeField] private Text hudScoreText;
        [SerializeField] private Text menuHighScoreText;
        [SerializeField] private Text gameOverScoreText;
        [SerializeField] private Text gameOverHighScoreText;
        [SerializeField] private Text newRecordText;

        [Header("Botões")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseMenuButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button gameOverMenuButton;

        private int lastScore;
        private bool gotNewRecord;

        private void Awake()
        {
            playButton.onClick.AddListener(() => GameStateMachine.Instance.StartGame());
            pauseButton.onClick.AddListener(() => GameStateMachine.Instance.Pause());
            resumeButton.onClick.AddListener(() => GameStateMachine.Instance.Resume());
            pauseMenuButton.onClick.AddListener(() => GameStateMachine.Instance.GoToMenu());
            retryButton.onClick.AddListener(() => GameStateMachine.Instance.StartGame());
            gameOverMenuButton.onClick.AddListener(() => GameStateMachine.Instance.GoToMenu());
        }

        private void OnEnable()
        {
            GameEvents.OnStateChanged += HandleStateChanged;
            GameEvents.OnScoreChanged += HandleScoreChanged;
            GameEvents.OnHighScoreChanged += HandleHighScoreChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnStateChanged -= HandleStateChanged;
            GameEvents.OnScoreChanged -= HandleScoreChanged;
            GameEvents.OnHighScoreChanged -= HandleHighScoreChanged;
        }

        private void HandleStateChanged(GameState state)
        {
            menuPanel.SetActive(state == GameState.Menu);
            hudPanel.SetActive(state == GameState.Playing || state == GameState.Paused);
            pausePanel.SetActive(state == GameState.Paused);
            gameOverPanel.SetActive(state == GameState.GameOver);

            switch (state)
            {
                case GameState.Menu:
                    menuHighScoreText.text = "Recorde: " + HighScoreService.Load();
                    break;
                case GameState.Playing:
                    gotNewRecord = false;
                    break;
                case GameState.GameOver:
                    gameOverScoreText.text = lastScore.ToString();
                    gameOverHighScoreText.text = "Recorde: " + HighScoreService.Load();
                    newRecordText.gameObject.SetActive(gotNewRecord);
                    break;
            }
        }

        private void HandleScoreChanged(int score)
        {
            lastScore = score;
            hudScoreText.text = score.ToString();
        }

        private void HandleHighScoreChanged(int score)
        {
            gotNewRecord = true;
        }
    }
}
