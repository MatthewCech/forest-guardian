using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
    /// <summary>
    /// USAGE: One time
    /// </summary>
    public class Combat020BuildPlayfield : CombatState
    {
        private bool firstStep = false;


        public Combat020BuildPlayfield(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {
            StateMachine.UI.SetSelectorVisibility(false);
            StateMachine.UI.result.gameObject.SetActive(false);
        }

        public override void Update()
        {
            if (!firstStep)
            {
                firstStep = true;
                Loam.CoroutineObject.Instance.StartCoroutine(QueueNext());
            }
        }

        private IEnumerator QueueNext()
        {
            // Handle initial playfield configuration
            yield return new WaitForSeconds(StateMachine.turnDelay);

            foreach (PlayfieldUnit unit in StateMachine.Playfield.units)
            {
                UnityEngine.Assertions.Assert.IsFalse(unit.team == Team.DEFAULT, "A unit has an unassigned team. This needs to be updated in JSON, and likely represents an editor export issue.");

                Unit template = StateMachine.VisualLookup.GetUnitTemplateByName(unit.tag);
                unit.curMovementBudget = template.data.speed;
                unit.curMaxMovementBudget = template.data.speed;
                unit.curMovesTaken = 0;
                unit.curMaxSize = template.data.maxSize;

                unit.curAttackRange = template.data.moves[0].moveRange;
            }

            foreach (PlayfieldTile tile in StateMachine.Playfield.world)
            {
                Tile template = StateMachine.VisualLookup.GetTileByTag(tile.tag);
                tile.curIsImpassable = template.isImpassable;
                tile.curMoveDifficulty = template.moveDifficulty;
            }

            // We're done, lets get moving forward
            yield return new WaitForSeconds(StateMachine.turnDelay);
            StateMachine.SetState<Combat030PrepareTurn>();
        }
    }
}