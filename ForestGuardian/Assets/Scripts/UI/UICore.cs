using forest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest 
{
    public class UICore : MonoBehaviour
    {
        [Header("Confirmation of Destructive Action dialogue")]
        [SerializeField] private CanvasGroup canvasGroupCoDA;
        [SerializeField] private TMPro.TextMeshProUGUI dialogueCoDA;
        [SerializeField] private Button dialogueCoDAYes;
        [SerializeField] private Button dialogueCoDANo;
        private System.Action onYes;
        private System.Action onNo;

        private void Awake()
        {
            Core.Instance.TryRegisterUICore(this);
            SetCoDAVisibility(false);
        }

        private void OnEnable()
        {
            dialogueCoDAYes.onClick.AddListener(OnYes);
            dialogueCoDANo.onClick.AddListener(OnNo);
        }

        private void OnDisable()
        {
            dialogueCoDANo.onClick.RemoveListener(OnNo);
            dialogueCoDAYes.onClick.RemoveListener(OnYes);
        }

        private void OnYes()
        {
            SetCoDAVisibility(false);
            onYes?.Invoke();
        }

        private void OnNo()
        {
            SetCoDAVisibility(false);
            onNo?.Invoke();
        }

        private void SetCoDAVisibility(bool isVisible)
        {
            canvasGroupCoDA.alpha = isVisible ? 1 : 0;
            canvasGroupCoDA.interactable = isVisible;
            canvasGroupCoDA.blocksRaycasts = isVisible;
        }

        public void DisplayCoDA(string message = "fr?", System.Action onYes = null, System.Action onNo = null)
        {
            this.onYes = onYes;
            this.onNo = onNo;

            dialogueCoDA.text = message;

            SetCoDAVisibility(true);
        }
    }
}