using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
    public class Combat09Defeat : CombatState
    {
        private bool firstStep = false;
        /*
        private VisualElement resultBanner;
        private Label resultLabel;
        */

        public Combat09Defeat(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {
            /*
            resultBanner = StateMachine.ModernUI.rootVisualElement.Q<VisualElement>("result");
            resultLabel = StateMachine.ModernUI.rootVisualElement.Q<Label>("resultLabel");
            */
        }

        public override void Update()
        {
            if (!firstStep)
            {
                firstStep = true;
                StateMachine.UI.result.gameObject.SetActive(true);
                StateMachine.UI.result.text = "Area Overgrown - Defeat";
                /*
                resultBanner.visible = true;
                resultLabel.text = "Area Overgrown - Defeat.";
                */
                Loam.CoroutineObject.Instance.StartCoroutine(ScreenDelay());
            }
        }

        private IEnumerator ScreenDelay()
        {
            yield return new WaitForSeconds(StateMachine.resultScreenTime);
            StateMachine.SetState<Combat20Shutdown>();
        }
    }
}