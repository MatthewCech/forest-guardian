using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{
    public class MenuUIMain : MonoBehaviour
    {
        [SerializeField] private Button buttonSettings;
        [SerializeField] private Button buttonAbout;
        [SerializeField] private Button buttonQuit;

        private void OnEnable()
        {
            buttonSettings.onClick.AddListener(ShowSettings);
            buttonAbout.onClick.AddListener(ShowAbout);
            buttonQuit.onClick.AddListener(Quit);
        }

        private void OnDisable()
        {
            buttonQuit.onClick.RemoveListener(Quit);
            buttonAbout.onClick.RemoveListener(ShowAbout);
            buttonSettings.onClick.RemoveListener(ShowSettings);
        }

        private void ShowSettings()
        {
            Debug.LogWarning("Nothing yet!");
        }

        private void ShowAbout()
        {
            Debug.LogWarning("Nothing yet!");
        }

        private void Quit()
        {
            Core.Instance.uiCore.DisplayCoDA("Quit fr?", () => { Application.Quit(); }, null);
        }
    }
}