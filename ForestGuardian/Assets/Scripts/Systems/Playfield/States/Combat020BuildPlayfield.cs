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

            // At this point, we only have units 
            foreach (PlayfieldUnit unit in StateMachine.Playfield.units)
            {
                UnityEngine.Assertions.Assert.IsFalse(unit.team == Team.DEFAULT, "A unit has an unassigned team. This needs to be updated in JSON, and likely represents an editor export issue.");

                Unit template = StateMachine.VisualLookup.GetUnitTemplateByName(unit.tag);
                unit.curMovementBudget = template.data.speed;
                unit.curMaxMovementBudget = template.data.speed;
                unit.curMovesTaken = 0;
                unit.curMaxSize = template.data.maxSize;
                unit.curSelectedMove = 0;
            }

            foreach (PlayfieldTile tile in StateMachine.Playfield.world)
            {
                Tile template = StateMachine.VisualLookup.GetTileByTag(tile.tag);
                tile.curIsImpassable = template.isImpassable;
                tile.curMoveDifficulty = template.moveDifficulty;
            }

            // Try and auto-assign origin locations
            List<PlayfieldOrigin> partyOrigins = StateMachine.Playfield.origins.FindAll(
                origin => origin.partyIndex != PlayfieldOrigin.NO_INDEX_SELECTED);
            if(partyOrigins.Count > 0)
            {
                List<PlayfieldUnit> units = Core.Instance.GameData.lastFloor.GetPlayerUnits();

                foreach(PlayfieldOrigin origin in partyOrigins)
                {
                    int index = origin.partyIndex;
                    if (index < units.Count)
                    {
                        // <carry over or omit party stats here>
                        PlayfieldUnit unitToAdd = units[index];
                        unitToAdd.locations.Clear();
                        unitToAdd.locations.Add(origin.location);

                        StateMachine.Playfield.units.Add(unitToAdd);
                    }
                }

                // Redraw units because we made modifications.
                StateMachine.VisualPlayfield.DisplayUnits(StateMachine.Playfield);
            }
            else
            {
                // This needs to be cleared a better way...
                Core.Instance.GameData.lastFloor = null;
            }

            // Clear any origins left, nothing more we're going to use them for.
            StateMachine.Playfield.origins.Clear();
            StateMachine.VisualPlayfield.DisplayOrigins(StateMachine.Playfield);

            // We're done, lets get moving forward
            yield return new WaitForSeconds(StateMachine.turnDelay);
            StateMachine.SetState<Combat030PrepareTurn>();
        }
    }
}