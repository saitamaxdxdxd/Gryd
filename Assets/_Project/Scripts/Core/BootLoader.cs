using System.Collections;
using UnityEngine;
using Gryd.Managers;

namespace Gryd.Core
{
    /// <summary>
    /// Secuencia Boot: Panel empresa → Panel juego → MainMenu.
    ///
    /// Setup en la escena Boot:
    ///   - Un Canvas con dos paneles hijos: _companyPanel y _gamePanel.
    ///   - Cada panel tiene un Animator con un trigger "Play" que dispara su animación.
    ///   - Si un panel no tiene Animator, se muestra durante _fallbackDuration segundos.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [Header("Paneles de logo")]
        [SerializeField] private GameObject _companyPanel;
        [SerializeField] private GameObject _gamePanel;

        [Header("Duración si no hay animación")]
        [SerializeField] private float _fallbackDuration = 2f;

        private static readonly int PlayTrigger = Animator.StringToHash("Play");

        private void Start()
        {
            _companyPanel.SetActive(false);
            _gamePanel.SetActive(false);
            StartCoroutine(BootSequence());
        }

        private IEnumerator BootSequence()
        {
            yield return StartCoroutine(ShowPanel(_companyPanel));
            yield return StartCoroutine(ShowPanel(_gamePanel));
            SceneLoader.Instance.Load(SceneNames.MainMenu);
        }

        private IEnumerator ShowPanel(GameObject panel)
        {
            panel.SetActive(true);

            Animator anim = panel.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger(PlayTrigger);
                // Esperar a que el Animator termine su estado actual
                yield return null; // un frame para que el Animator arranque
                yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
            }
            else
            {
                yield return new WaitForSeconds(_fallbackDuration);
            }

            panel.SetActive(false);
        }
    }
}
