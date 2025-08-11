using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
    public class Combat01Startup : CombatState
    {
        private bool firstStep = false;

        /*
        private VisualElement resultBanner;
        */

        public Combat01Startup(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {
            /*
            resultBanner = StateMachine.ModernUI.rootVisualElement.Q<VisualElement>("result");
            */
        }

        public override void Update()
        {
            if(!firstStep)
            {
                firstStep = true;
                StateMachine.UI.result.gameObject.SetActive(false);
                Loam.CoroutineObject.Instance.StartCoroutine(QueueUpNext());
            }
        }

        private IEnumerator QueueUpNext()
        {
            yield return new WaitForSeconds(StateMachine.turnDelay);
            StateMachine.SetState<Combat02PrepareTurn>();
        }
    }
}