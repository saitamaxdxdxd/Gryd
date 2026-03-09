using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Gryd.Menu
{
    /// <summary>
    /// Controla el estado visual de un botón de nivel (bloqueado / desbloqueado).
    /// Asignar en el prefab LevelButton.
    /// </summary>
    public class LevelButton : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _levelNumberText;
        [SerializeField] private GameObject _lockIcon;

        private int _levelIndex;
        private LevelSelectController _controller;

        public void Setup(int levelIndex, bool isUnlocked, LevelSelectController controller)
        {
            _levelIndex = levelIndex;
            _controller = controller;

            _levelNumberText.text = levelIndex.ToString();
            _lockIcon.SetActive(!isUnlocked);
            _button.interactable = isUnlocked;
            _button.onClick.AddListener(OnPressed);
        }

        private void OnPressed()
        {
            _controller.OnLevelSelected(_levelIndex);
        }
    }
}
