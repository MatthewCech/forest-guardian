using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{
    public class UIDispatch_MsgMove : MonoBehaviour
    {
        [SerializeField] private Button button;

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
            Loam.Postmaster.Instance.Send(new MsgMove_UI());
        }
    }
}