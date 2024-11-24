using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
    public class Combat09Defeat : CombatState
    {
        private bool firstStep = false;
        private VisualElement resultBanner;
        private Label resultLabel;

        public Combat09Defeat(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {
            resultBanner = StateMachine.UI.rootVisualElement.Q<VisualElement>("result");
            resultLabel = StateMachine.UI.rootVisualElement.Q<Label>("resultLabel");
        }

        public override void Update()
        {
            if (!firstStep)
            {
                firstStep = true;
                resultBanner.visible = true;
                resultLabel.text = "Area Overgrown - Defeat.";
                Loam.CoroutineObject.Instance.StartCoroutine(ScreenDelay());
            }
        }

        private IEnumerator ScreenDelay()
        {
            yield return new WaitForSeconds(StateMachine.resultScreenTime);
            StateMachine.SetState<Combat10Shutdown>();
        }
    }
}