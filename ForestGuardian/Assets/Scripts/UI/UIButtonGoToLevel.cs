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
            Core.Instance.LoadLevel(levelToLoad);
        }
    }
}