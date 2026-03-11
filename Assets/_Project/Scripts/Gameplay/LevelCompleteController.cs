using UnityEngine;
using Gryd.Core;
using Gryd.Managers;
using Gryd.Menu;

namespace Gryd.Gameplay
{
    /// <summary>
    /// Panel de nivel completado. Se muestra automáticamente cuando GameManager → LevelComplete.
    /// Desbloquea el siguiente nivel en SaveManager.
    ///
    /// Setup en Inspector:
    ///   - Asignar _panel (el GameObject raíz del panel, empieza desactivado)
    ///   - Conectar Btn_Next  → OnNextPressed()
    ///   - Conectar Btn_Menu  → OnMenuPressed()
    /// </summary>
    public class LevelCompleteController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;

        private int CurrentLevel => LevelSelectController.SelectedLevel > 0
            ? LevelSelectController.SelectedLevel
            : 1;

        private void OnEnable()
        {
            GameManager.OnStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.LevelComplete)
                Show();
        }

        private void Show()
        {
            _panel.SetActive(true);
            UnlockNextLevel();
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxLevelComplete);
        }

        private void UnlockNextLevel()
        {
            int nextLevel = CurrentLevel + 1;
            if (nextLevel > SaveManager.Instance.Data.unlockedLevels)
            {
                SaveManager.Instance.Data.unlockedLevels = nextLevel;
                SaveManager.Instance.Save();
                AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxLevelUnlocked);
            }
        }

        // ── Botones ───────────────────────────────────────────

        public void OnNextPressed()
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButtonClick);
            _panel.SetActive(false);
            // TODO: cuando haya escenas de nivel individuales, cargar CurrentLevel + 1
            // Por ahora vuelve a LevelSelect para elegir el siguiente nivel desbloqueado
            GameManager.Instance.ExitToMenu();
            SceneLoader.Instance.Load(SceneNames.LevelSelect);
        }

        public void OnMenuPressed()
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButtonBack);
            _panel.SetActive(false);
            GameManager.Instance.ExitToMenu();
        }
    }
}
