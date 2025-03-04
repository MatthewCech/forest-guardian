using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{
    public class PlayfieldEditorUISelectable : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMPro.TextMeshProUGUI buttonLabel;

        public SelectionType SelectableType { get; private set; }
        public string SelectableTag { get; private set; }
        public GameObject Visual { get; private set; }

        // Internal
        private System.Action<PlayfieldEditorUISelectable> onClick;

        public void SetData(GameObject visual, SelectionType type, string tag, System.Action<PlayfieldEditorUISelectable> onClick)
        {
            this.Visual = visual;
            this.SelectableType = type;
            this.SelectableTag = tag;
            this.onClick = onClick;

            buttonLabel.text = tag;
        }

        private void OnEnable()
        {
            button.onClick.AddListener(ButtonClicked);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(ButtonClicked);
        }

        void ButtonClicked()
        {
            onClick?.Invoke(this);
        }
    }
}