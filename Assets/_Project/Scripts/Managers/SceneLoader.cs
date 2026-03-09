using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gryd.Managers
{
    /// <summary>
    /// Servicio de carga de escenas. Persiste durante toda la sesión.
    /// Colocar en la escena Boot junto al BootLoader.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        public bool IsLoading { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Garantiza que SceneLoader exista aunque se entre directo a cualquier escena
        /// desde el Editor, sin pasar por Boot.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstance()
        {
            if (Instance != null) return;
            var go = new GameObject("SceneLoader [Auto]");
            go.AddComponent<SceneLoader>();
        }

        // Carga inmediata
        public void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        // Carga asíncrona con callbacks opcionales de progreso y completado
        public void LoadAsync(string sceneName, Action<float> onProgress = null, Action onComplete = null)
        {
            if (IsLoading) return;
            StartCoroutine(LoadAsyncRoutine(sceneName, onProgress, onComplete));
        }

        private IEnumerator LoadAsyncRoutine(string sceneName, Action<float> onProgress, Action onComplete)
        {
            IsLoading = true;

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                onProgress?.Invoke(op.progress / 0.9f);
                yield return null;
            }

            onProgress?.Invoke(1f);
            yield return new WaitForSeconds(0.1f);

            op.allowSceneActivation = true;
            IsLoading = false;
            onComplete?.Invoke();
        }
    }
}
