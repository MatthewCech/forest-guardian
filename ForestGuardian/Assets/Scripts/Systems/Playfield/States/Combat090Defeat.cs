using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
    public class Combat090Defeat : CombatState
    {
        private bool firstStep = false;

        public Combat090Defeat(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {

        }

        public override void Update()
        {
            if (!firstStep)
            {
                firstStep = true;
                StateMachine.UI.result.gameObject.SetActive(true);
                StateMachine.UI.result.text = "Area Overgrown - Defeat";
                Loam.CoroutineObject.Instance.StartCoroutine(ScreenDelay());
            }
        }

        private IEnumerator ScreenDelay()
        {
            yield return new WaitForSeconds(StateMachine.resultScreenTime);
            StateMachine.SetState<Combat200Shutdown>();
        }
    }
}