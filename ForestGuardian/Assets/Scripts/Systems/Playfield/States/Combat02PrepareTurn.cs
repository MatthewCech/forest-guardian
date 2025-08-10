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
                UnityEngine.Assertions.Assert.IsFalse(unit.team == Team.DEFAULT, "A unit has an unassigned team. This needs to be updated in JSON, and likely represents an editor export issue.");

                Unit template = StateMachine.VisualLookup.GetUnityByTag(unit.tag);
                unit.curMovementBudget = template.data.speed;
                unit.curMaxMovementBudget = template.data.speed;
                unit.curMovesTaken = 0;
                unit.curMaxSize = template.data.maxSize;

                unit.curAttackRange = template.data.attacks[0].attackRange;
            }

            foreach (PlayfieldTile tile in StateMachine.Playfield.world)
            {
                Tile template = StateMachine.VisualLookup.GetTileByTag(tile.tag);
                tile.curIsImpassable = template.isImpassable;
                tile.curMoveDifficulty = template.moveDifficulty;
            }

            yield return null;
            StateMachine.SetState<Combat03PlayerMove>();
        }
    }
}
