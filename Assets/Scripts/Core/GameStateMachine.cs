using System.Collections.Generic;
using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Máquina de estados da partida: Menu → Jogando ⇄ Pausado → GameOver → Menu/Jogando.
    /// Cada estado é uma classe com Enter/Exit; a máquina só troca o estado atual
    /// e avisa o resto do jogo por GameEvents.OnStateChanged.
    /// </summary>
    public class GameStateMachine : MonoBehaviour
    {
        public static GameStateMachine Instance { get; private set; }

        [Header("Referências")]
        [SerializeField] private PlayerController player;
        [SerializeField] private WorldScroller scroller;
        [SerializeField] private ObstacleSpawner spawner;
        [SerializeField] private ScoreManager score;

        [Header("Configuração")]
        [SerializeField] private int targetFrameRate = 60;

        public GameState Current => current != null ? current.Id : GameState.Menu;
        public PlayerController Player => player;
        public WorldScroller Scroller => scroller;
        public ObstacleSpawner Spawner => spawner;
        public ScoreManager Score => score;

        private readonly Dictionary<GameState, GameStateBase> states = new Dictionary<GameState, GameStateBase>();
        private GameStateBase current;

        private void Awake()
        {
            Instance = this;
            Application.targetFrameRate = targetFrameRate;

            states[GameState.Menu] = new MenuState(this);
            states[GameState.Playing] = new PlayingState(this);
            states[GameState.Paused] = new PausedState(this);
            states[GameState.GameOver] = new GameOverState(this);
        }

        private void OnEnable() => GameEvents.OnPlayerDied += HandlePlayerDied;
        private void OnDisable() => GameEvents.OnPlayerDied -= HandlePlayerDied;

        private void Start()
        {
            ChangeState(GameState.Menu);
        }

        private void Update()
        {
            current?.Tick();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        // --- Transições públicas (chamadas pela UI) ---
        public void StartGame() => ChangeState(GameState.Playing);
        public void GoToMenu() => ChangeState(GameState.Menu);

        public void Pause()
        {
            if (Current == GameState.Playing) ChangeState(GameState.Paused);
        }

        public void Resume()
        {
            if (Current != GameState.Paused) return;
            ChangeState(GameState.Playing, resuming: true);
        }

        /// <summary>Verdadeiro só durante a transição Pausado → Jogando (não é partida nova).</summary>
        public bool IsResuming { get; private set; }

        public void TogglePause()
        {
            if (Current == GameState.Playing) Pause();
            else if (Current == GameState.Paused) Resume();
        }

        private void HandlePlayerDied()
        {
            if (Current == GameState.Playing) ChangeState(GameState.GameOver);
        }

        private void ChangeState(GameState next, bool resuming = false)
        {
            if (current != null && current.Id == next) return;

            IsResuming = resuming;
            current?.Exit();
            current = states[next];
            current.Enter();
            IsResuming = false;
            GameEvents.RaiseStateChanged(next);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // Celular foi para segundo plano: pausa a partida em vez de deixar a gota morrer.
            if (pauseStatus) Pause();
        }
    }

    public abstract class GameStateBase
    {
        protected readonly GameStateMachine Fsm;
        public abstract GameState Id { get; }

        protected GameStateBase(GameStateMachine fsm) { Fsm = fsm; }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Tick() { }
    }

    public class MenuState : GameStateBase
    {
        public override GameState Id => GameState.Menu;
        public MenuState(GameStateMachine fsm) : base(fsm) { }

        public override void Enter()
        {
            Fsm.Scroller.IsScrolling = false;
            Fsm.Player.InputEnabled = false;
            Fsm.Player.ResetToWall();
            Fsm.Spawner.Clear();
            Fsm.Score.ResetRun();
        }
    }

    public class PlayingState : GameStateBase
    {
        public override GameState Id => GameState.Playing;

        public PlayingState(GameStateMachine fsm) : base(fsm) { }

        public override void Enter()
        {
            if (!Fsm.IsResuming)
            {
                // Partida nova
                Fsm.Score.ResetRun();
                Fsm.Spawner.Clear();
                Fsm.Player.ResetToWall();
                Fsm.Spawner.Begin();
            }
            Fsm.Scroller.IsScrolling = true;
            Fsm.Player.InputEnabled = true;
        }

        public override void Exit()
        {
            Fsm.Player.InputEnabled = false;
        }
    }

    public class PausedState : GameStateBase
    {
        public override GameState Id => GameState.Paused;
        public PausedState(GameStateMachine fsm) : base(fsm) { }

        public override void Enter()
        {
            Time.timeScale = 0f;
            Fsm.Scroller.IsScrolling = false;
        }

        public override void Exit()
        {
            Time.timeScale = 1f;
        }
    }

    public class GameOverState : GameStateBase
    {
        public override GameState Id => GameState.GameOver;
        public GameOverState(GameStateMachine fsm) : base(fsm) { }

        public override void Enter()
        {
            Fsm.Scroller.IsScrolling = false;
            Fsm.Spawner.Stop();
            HighScoreService.TrySubmit(Fsm.Score.Score);
        }
    }
}
