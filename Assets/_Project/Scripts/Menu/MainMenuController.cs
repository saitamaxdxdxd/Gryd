using UnityEngine;
using Gryd.Core;
using Gryd.Managers;

namespace Gryd.Menu
{
    /// <summary>
    /// Controla la pantalla principal del menú.
    /// Conectar botones de Unity UI a los métodos públicos.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        // Botón Play → conectar en Inspector
        public void OnPlayPressed()
        {
            SceneLoader.Instance.Load(SceneNames.LevelSelect);
        }

        // Botón Settings (futuro)
        public void OnSettingsPressed()
        {
            // TODO: abrir panel de settings
        }

        // Botón Quit
        public void OnQuitPressed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
