using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace forest
{
    /// <summary>
    /// USAGE: Per turn
    /// </summary>
    public class Combat030PrepareTurn : CombatState
    {
        private bool firstStep = false;

        public Combat030PrepareTurn(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Update()
        {
            if (!firstStep)
            {
                firstStep = true;
                Loam.CoroutineObject.Instance.StartCoroutine(PreparePlayfield());
            }
        }

        private IEnumerator PreparePlayfield()
        {
            yield return new WaitForSeconds(StateMachine.turnDelay);

            foreach (PlayfieldUnit unit in StateMachine.Playfield.units)
            {
                unit.curMovementBudget = unit.curMaxMovementBudget;
                unit.curMovesTaken = 0;
            }

            yield return null;
            StateMachine.SetState<Combat040PlayerMove>();
        }
    }
}
