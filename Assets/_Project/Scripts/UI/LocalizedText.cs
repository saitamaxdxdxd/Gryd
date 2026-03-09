using UnityEngine;
using TMPro;
using Gryd.Managers;

namespace Gryd.UI
{
    /// <summary>
    /// Agrega este componente a cualquier TextMeshPro para que se actualice
    /// automáticamente al cambiar de idioma.
    /// Solo asignar la Key en el Inspector.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string _key;

        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += Refresh;
        }

        private void OnDisable()
        {
            LocalizationManager.OnLanguageChanged -= Refresh;
        }

        private void Start()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (LocalizationManager.Instance == null) return;
            _text.text = LocalizationManager.Instance.Get(_key);
        }
    }
}
