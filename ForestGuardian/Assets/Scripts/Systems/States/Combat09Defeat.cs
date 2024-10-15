using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class Combat09Defeat : CombatState
    {
        private bool firstStep = false;

        public Combat09Defeat(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Update()
        {
            if (!firstStep)
            {
                firstStep = true;
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