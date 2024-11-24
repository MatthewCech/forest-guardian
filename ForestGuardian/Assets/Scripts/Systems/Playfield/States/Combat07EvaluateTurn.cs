using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class Combat07EvaluateTurn : CombatState
    {
        private bool firstStep = false;

        public Combat07EvaluateTurn(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Update()
        {
            if(!firstStep)
            {
                firstStep = true;
                Loam.CoroutineObject.Instance.StartCoroutine(Evaluate());
            }
        }

        private IEnumerator Evaluate()
        {
            yield return new WaitForSeconds(StateMachine.turnDelay);

            // Win value, in this case I've hard-coded to be "No items left"
            if (StateMachine.Playfield.items.Count == 0 && !HasEnemies())
            {
                yield return null;
                StateMachine.SetState<Combat08Victory>();
            }
            else
            {
                yield return null;
                StateMachine.SetState<Combat02PrepareTurn>();
            }
        }
    }
}