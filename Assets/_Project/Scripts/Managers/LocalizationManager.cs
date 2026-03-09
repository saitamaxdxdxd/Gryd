using System;
using UnityEngine;
using Gryd.Data;

namespace Gryd.Managers
{
    public enum Language { English, Spanish }

    /// <summary>
    /// Maneja el idioma actual del juego.
    /// Persiste durante toda la sesión (DontDestroyOnLoad).
    /// Colocar en la escena Boot.
    ///
    /// Uso:
    ///   LocalizationManager.Instance.Get("btn_play")  → "Play" o "Jugar"
    ///   LocalizationManager.Instance.SetLanguage(Language.Spanish);
    ///   LocalizationManager.OnLanguageChanged += Refresh;
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        public static event Action OnLanguageChanged;

        [SerializeField] private LocalizationData _data;

        public Language CurrentLanguage { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstance()
        {
            if (Instance != null) return;
            var go = new GameObject("LocalizationManager [Auto]");
            // Cargar el asset ANTES de AddComponent para que Awake lo encuentre
            var data = Resources.Load<LocalizationData>("LocalizationData");
            var lm = go.AddComponent<LocalizationManager>();
            lm._data = data;
        }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_data == null)
                _data = Resources.Load<LocalizationData>("LocalizationData");

            if (_data == null) { Debug.LogWarning("[Localization] LocalizationData no encontrado en Resources/"); return; }

            _data.BuildCache();

            int saved = SaveManager.Instance.Data.language;
            CurrentLanguage = saved == -1
                ? DetectDeviceLanguage()
                : (Language)saved;
        }

        public string Get(string key)
        {
            if (!_data.TryGet(key, out var entry))
            {
                Debug.LogWarning($"[Localization] Key no encontrada: '{key}'");
                return key;
            }

            return CurrentLanguage == Language.Spanish ? entry.spanish : entry.english;
        }

        public void SetLanguage(Language language)
        {
            if (CurrentLanguage == language) return;
            CurrentLanguage = language;
            SaveManager.Instance.Data.language = (int)language;
            SaveManager.Instance.Save();
            OnLanguageChanged?.Invoke();
        }

        public void ToggleLanguage()
        {
            SetLanguage(CurrentLanguage == Language.English ? Language.Spanish : Language.English);
        }

        private static Language DetectDeviceLanguage()
        {
            return Application.systemLanguage == SystemLanguage.Spanish
                ? Language.Spanish
                : Language.English;
        }
    }
}
