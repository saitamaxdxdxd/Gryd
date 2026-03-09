using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gryd.Core
{
    /// <summary>
    /// Carga la GameScene de forma asíncrona con un mínimo de 3 segundos de espera.
    /// </summary>
    public class LoadingController : MonoBehaviour
    {
        private const float MinimumLoadTime = 3f;

        private void Start()
        {
            StartCoroutine(LoadGameScene());
        }

        private IEnumerator LoadGameScene()
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(SceneNames.Game);
            op.allowSceneActivation = false;

            float elapsed = 0f;

            while (elapsed < MinimumLoadTime || op.progress < 0.9f)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            op.allowSceneActivation = true;
        }
    }
}
