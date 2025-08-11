using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace forest
{
    public class Combat030PrepareTurn : CombatState
    {
        private bool firstStep = false;

        public Combat030PrepareTurn(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Update()
        {
            if (!firstStep)
            {
                firstStep = true;
                WriteOriginUnitsToPlayfield();
                Loam.CoroutineObject.Instance.StartCoroutine(ProcessWithDelay());
            }
        }

        /// <summary>
        /// Writes and displays the origin units to the playfield
        /// </summary>
        private void WriteOriginUnitsToPlayfield()
        {
            if(StateMachine.Playfield.origins == null || StateMachine.Playfield.origins.Count == 0)
            {
                return;
            }

            foreach (PlayfieldOrigin origin in StateMachine.Playfield.origins)
            {
                if(origin.curRosterIndex == PlayfieldOrigin.ROSTER_NONE_SELECTED)
                {
                    continue;
                }

                UnitData unit = Core.Instance.gameData.roster[origin.curRosterIndex];

                PlayfieldUnit unitToAdd = new PlayfieldUnit();
                unitToAdd.tag = unit.unitName;
                unitToAdd.id = StateMachine.Playfield.GetNextID();
                unitToAdd.team = Team.Player;
                unitToAdd.locations = new List<Vector2Int>() { origin.location };
                unitToAdd.rosterOverride = unit;

                StateMachine.Playfield.units.Add(unitToAdd);
            }

            StateMachine.Playfield.origins.Clear();

            StateMachine.VisualPlayfield.DisplayUnits(StateMachine.Playfield);
            StateMachine.VisualPlayfield.DisplayOrigins(StateMachine.Playfield);
        }

        private IEnumerator ProcessWithDelay()
        {
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

            yield return null;
            StateMachine.SetState<Combat040PlayerMove>();
        }
    }
}
