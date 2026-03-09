using System.IO;
using UnityEngine;
using Gryd.Data;

namespace Gryd.Managers
{
    /// <summary>
    /// Gestiona todos los datos persistentes del juego en un único archivo JSON.
    /// Ruta: Application.persistentDataPath/save.json
    ///
    /// Uso:
    ///   SaveManager.Instance.Data.musicVolume = 0.8f;
    ///   SaveManager.Instance.Save();
    ///
    ///   int unlocked = SaveManager.Instance.Data.unlockedLevels;
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        public SaveData Data { get; private set; }

        private string _savePath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstance()
        {
            if (Instance != null) return;
            var go = new GameObject("SaveManager [Auto]");
            go.AddComponent<SaveManager>();
        }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _savePath = Path.Combine(Application.persistentDataPath, "save.json");
            Load();
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(Data, prettyPrint: true);
            File.WriteAllText(_savePath, json);
        }

        private void Load()
        {
            if (File.Exists(_savePath))
            {
                string json = File.ReadAllText(_savePath);
                Data = JsonUtility.FromJson<SaveData>(json);
            }
            else
            {
                Data = new SaveData(); // valores por defecto
            }
        }

        // Guardar automáticamente al cerrar la app
        private void OnApplicationQuit() => Save();
        private void OnApplicationPause(bool paused) { if (paused) Save(); }
    }
}
