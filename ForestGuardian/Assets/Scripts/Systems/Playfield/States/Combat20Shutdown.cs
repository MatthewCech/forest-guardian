using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
    public class Combat20Shutdown : CombatState
    {
        private bool firstStep = false;

        /*
        private VisualElement resultBanner;
        */

        public Combat20Shutdown(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {
            /*
            resultBanner = StateMachine.ModernUI.rootVisualElement.Q<VisualElement>("result");
            */
        }

        public override void Update()
        {
            if (!firstStep)
            {
                firstStep = true;
                StateMachine.UI.result.gameObject.SetActive(false);
                Core.Instance.LoadLevelMap();
            }
        }
    }
}