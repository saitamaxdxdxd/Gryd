using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gryd.Core;
using Gryd.Data;
using Gryd.Managers;

namespace Gryd.Menu
{
    /// <summary>
    /// Pantalla de selección de niveles con paginación.
    /// Crea los botones de la página actual y navega con flechas izquierda/derecha.
    /// </summary>
    public class LevelSelectController : MonoBehaviour
    {
        public static int SelectedLevel { get; private set; }

        [Header("Config")]
        [SerializeField] private LevelRegistry _registry;
        [SerializeField] private int _levelsPerPage = 9;

        [Header("Referencias")]
        [SerializeField] private LevelButton _levelButtonPrefab;
        [SerializeField] private Transform _gridContainer;
        [SerializeField] private Button _btnLeft;
        [SerializeField] private Button _btnRight;
        [SerializeField] private TextMeshProUGUI _pageText;

        private int _currentPage = 0;
        private int _unlockedLevels;
        private int TotalLevels => _registry != null ? _registry.levels.Length : 0;
        private int TotalPages => Mathf.CeilToInt((float)TotalLevels / _levelsPerPage);

        private void Start()
        {
            _unlockedLevels = SaveManager.Instance.Data.unlockedLevels;
            ShowPage(0);
        }

        public void OnNextPage()
        {
            if (_currentPage < TotalPages - 1)
                ShowPage(_currentPage + 1);
        }

        public void OnPrevPage()
        {
            if (_currentPage > 0)
                ShowPage(_currentPage - 1);
        }

        private void ShowPage(int page)
        {
            _currentPage = page;

            // Limpiar botones anteriores
            foreach (Transform child in _gridContainer)
                Destroy(child.gameObject);

            // Generar botones de esta página
            int startLevel = page * _levelsPerPage + 1;
            int endLevel = Mathf.Min(startLevel + _levelsPerPage - 1, TotalLevels);

            for (int i = startLevel; i <= endLevel; i++)
            {
                LevelButton btn = Instantiate(_levelButtonPrefab, _gridContainer);
                btn.Setup(i, i <= _unlockedLevels, this);
            }

            // Actualizar flechas y texto de página
            _btnLeft.interactable = _currentPage > 0;
            _btnRight.interactable = _currentPage < TotalPages - 1;
            _pageText.text = $"{_currentPage + 1} / {TotalPages}";
        }

        public void OnLevelSelected(int levelIndex)
        {
            SelectedLevel = levelIndex;
            SceneLoader.Instance.LoadAsync(SceneNames.Loading);
        }

        public static void UnlockNextLevel(int completedLevel)
        {
            var data = SaveManager.Instance.Data;
            if (completedLevel + 1 > data.unlockedLevels)
            {
                data.unlockedLevels = completedLevel + 1;
                SaveManager.Instance.Save();
            }
        }

        public void OnBackPressed()
        {
            SceneLoader.Instance.Load(SceneNames.MainMenu);
        }
    }
}
