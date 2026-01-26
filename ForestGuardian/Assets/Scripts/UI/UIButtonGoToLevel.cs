using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{
    public class UIButtonGoToLevel : MonoBehaviour
    {
        [SerializeField] private Core.ForestLevel levelToLoad;
        [SerializeField] private Button target;
        [SerializeField] private bool confirmAction = false;
        [SerializeField] private bool saveOnAction = false;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (target == null)
            {
                target = this.gameObject.GetComponent<Button>();
            }
        }
#endif

        void OnEnable()
        {
            target.onClick.AddListener(TryLoad);
        }

        private void OnDisable()
        {
            target.onClick.RemoveListener(TryLoad);
        }

        void TryLoad()
        {
            if (saveOnAction)
            {
                Loam.Postmaster.Instance.Send(new MsgSaveGame());
            }

            if (confirmAction)
            {
                Core.Instance.UICore.DisplayCoDA($"Really head to {levelToLoad.ToString()}?", () =>
                {
                    Core.Instance.LoadLevel(levelToLoad);
                });
            }
            else
            {
                Core.Instance.LoadLevel(levelToLoad);
            }
        }
    }
}