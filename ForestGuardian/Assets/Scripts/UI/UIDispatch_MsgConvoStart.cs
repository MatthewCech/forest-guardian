using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Loam;
using UnityEngine.UI;

namespace forest
{
    public class UIDispatch_MsgConvoStart : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private string dialogName;

        private void OnValidate()
        {
            if (button == null)
            {
                button = this.GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            button.onClick.AddListener(ButtonInteraction);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(ButtonInteraction);
        }

        private void ButtonInteraction()
        {
            Loam.Postmaster.Instance.Send(new MsgConvoStart() { convoName = dialogName });
        }
    }
}