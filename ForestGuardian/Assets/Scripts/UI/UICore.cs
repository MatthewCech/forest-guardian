using forest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Loam.Convo;
using UnityEngine.EventSystems;

namespace forest 
{
    public class UICore : MonoBehaviour
    {
        public const string UI_LAYER_NAME = "UI";

        [Header("Dialog")]
        [SerializeField] private DialogueUICore convoCore;

        [Header("Confirmation of Destructive Action dialogue")]
        [SerializeField] private CanvasGroup canvasGroupCoDA;
        [SerializeField] private TMPro.TextMeshProUGUI dialogueCoDA;
        [SerializeField] private Button dialogueCoDAYes;
        [SerializeField] private Button dialogueCoDANo;
        
        private System.Action onYes;
        private System.Action onNo;
        private int layerUI;

        public bool IsWorldInteractable { get; private set; } = true;

        private void Awake()
        {
            Core.Instance.TryRegisterUICore(this);
            
            canvasGroupCoDA.gameObject.SetActive(true);

            convoCore.Initialize();

            SetCoDAVisibility(false);
        }

        private void Start()
        {
            layerUI = LayerMask.NameToLayer(UI_LAYER_NAME);
        }

        /// <summary>
        /// Check if anything under the cursor is tagged as UI via event system raycast.
        /// NOTE: Leans on EventSystem, so may not work in certain situations.
        /// TODO: Consider caching if this ends up getting used a lot.
        /// </summary>
        public bool IsMouseOverUIElement()
        {
            EventSystem eventSystem = EventSystem.current;
            List<RaycastResult> raycastResults = new List<RaycastResult>();

            eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = Input.mousePosition }, raycastResults);
            foreach(RaycastResult result in raycastResults)
            {
                if(result.gameObject.layer == layerUI)
                {
                    return true;
                }
            }

            return false;
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

            IsWorldInteractable = !isVisible; // If visible, not enabled.
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