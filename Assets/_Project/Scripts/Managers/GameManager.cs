using System;
using UnityEngine;
using Gryd.Core;

namespace Gryd.Managers
{
    public enum GameState { Playing, Paused, LevelComplete, GameOver }

    /// <summary>
    /// Controla el estado global del juego dentro de GameScene.
    /// Colocar en GameScene. Otros sistemas se suscriben a los eventos.
    ///
    /// Uso:
    ///   GameManager.Instance.SetState(GameState.GameOver);
    ///   GameManager.OnStateChanged += HandleStateChanged;
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public static event Action<GameState> OnStateChanged;

        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            SetState(GameState.Playing);
            AudioManager.Instance?.PlayPlaylist(AudioManager.Instance.musicGame);
        }

        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;

            Time.timeScale = newState == GameState.Paused ? 0f : 1f;

            OnStateChanged?.Invoke(newState);
        }

        // ── Helpers públicos ─────────────────────────────────

        public void LevelComplete()  => SetState(GameState.LevelComplete);
        public void GameOver()       => SetState(GameState.GameOver);
        public void Pause()          => SetState(GameState.Paused);
        public void Resume()         => SetState(GameState.Playing);

        public void RestartLevel()
        {
            SetState(GameState.Playing);
            SceneLoader.Instance.Load(SceneNames.Game);
        }

        public void ExitToMenu()
        {
            Time.timeScale = 1f;
            SceneLoader.Instance.Load(SceneNames.MainMenu);
        }
    }
}
