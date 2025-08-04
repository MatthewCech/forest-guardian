using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace forest
{
    public class Combat02PrepareTurn : CombatState
    {
        private bool firstStep = false;

        public Combat02PrepareTurn(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Update()
        {
            if (!firstStep)
            {
                firstStep = true;
                Loam.CoroutineObject.Instance.StartCoroutine(ProcessWithDelay());
            }
        }

        private IEnumerator ProcessWithDelay()
        {
            yield return new WaitForSeconds(StateMachine.turnDelay);

            foreach (PlayfieldUnit unit in StateMachine.Playfield.units)
            {
                UnityEngine.Assertions.Assert.IsFalse(unit.team == Team.DEFAULT);

                Unit template = StateMachine.Lookup.GetUnityByTag(unit.tag);
                unit.curMovementBudget = template.moveSpeed;
                unit.curMaxMomvementBudget = template.moveSpeed;
                unit.curMovesTaken = 0;
                unit.curMaxSize = template.maxSize;
                unit.curAttackRange = template.attackRange;
            }

            foreach (PlayfieldTile tile in StateMachine.Playfield.world)
            {
                Tile template = StateMachine.Lookup.GetTileByTag(tile.tag);
                tile.curIsImpassable = template.isImpassable;
                tile.curMoveDifficulty = template.moveDifficulty;
            }

            yield return null;
            StateMachine.SetState<Combat03PlayerMove>();
        }
    }
}
