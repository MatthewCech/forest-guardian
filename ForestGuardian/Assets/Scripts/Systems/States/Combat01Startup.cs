using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class Combat01Startup : CombatState
    {
        private bool firstStep = false;

        public Combat01Startup(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Update()
        {
            if(!firstStep)
            {
                firstStep = true;
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