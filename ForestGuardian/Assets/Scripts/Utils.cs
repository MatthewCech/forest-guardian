using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public static class Utils
    {
        public static void CenterCamera(Camera target, VisualPlayfield playfield)
        {
            Vector2 targetPos = playfield.GetTileCenter();
            target.transform.position = new Vector3(targetPos.x, targetPos.y, target.transform.position.z);
        }

        /// <summary>
        /// This forcibly moves you to a new location, but does not restrict unexpected movements.
        /// </summary>
        /// <param name="unitToMove"></param>
        /// <param name="playfield"></param>
        /// <param name="pos"></param>
        /// <param name="moveCost"></param>
        public static void StepUnitTo(PlayfieldUnit unitToMove, Playfield playfield, Vector2Int pos, int moveCost)
        {
            UnityEngine.Assertions.Assert.IsFalse(unitToMove.curMovementBudget - moveCost < 0, $"You must have enough available steps to pay for the projected move cost. Some check elsewhere probably failed. Had: {unitToMove.curMovementBudget}, Cost: {moveCost}");

            PlayfieldTile targetTile = playfield.world.Get(pos.x, pos.y);

            // See if we're going to be stepping on ourselves.
            bool isSteppingOnSelf = false;
            int indexBeingSteppedOn = -1;
            for(int i = 0; i < unitToMove.locations.Count; ++i)
            {
                if (unitToMove.locations[i] == pos)
                {
                    isSteppingOnSelf = true;
                    indexBeingSteppedOn = i;
                    break;
                }
            }

            // If we're stepping on ourselves, so we need need to move the position we know
            // we already have to the head index - so we need to pull it out.
            if (isSteppingOnSelf)
            {
                unitToMove.locations.RemoveAt(indexBeingSteppedOn);
            }

            // Head remains where it was, we just moved here, so insert at that spot.
            unitToMove.locations.Insert(PlayfieldUnit.HEAD_INDEX, pos);

            // Trim end if it's too long
            if (unitToMove.locations.Count > unitToMove.curMaxSize)
            {
                ShortenUnitTailByOne(unitToMove, playfield);
            }

            // Charge 'em!
            unitToMove.curMovementBudget -= moveCost;
        }

        public static void ShortenUnitTailByOne(PlayfieldUnit unitToShorten, Playfield playfield)
        {
            int index = unitToShorten.locations.Count - 1;
            Vector2Int tileToUpdatePos = unitToShorten.locations[index];
            PlayfieldTile tileToUpdate = playfield.world.Get(tileToUpdatePos);
            unitToShorten.locations.RemoveAt(unitToShorten.locations.Count - 1);

            // Suggest deletion if there are no location left
            if(unitToShorten.locations.Count == 0)
            {
                playfield.units.Remove(unitToShorten);

                unitToShorten.id = Playfield.NO_ID;
                unitToShorten.locations = null;
                unitToShorten.team = Team.DEFAULT;
            }
        }

        /// <summary>
        /// Determine if you can move to a specified tile
        /// </summary>
        /// <param name="unitTryingToMove"></param>
        /// <param name="targetTile"></param>
        /// <returns></returns>
        public static bool CanMovePlayfieldUnitTo(Playfield playfield, PlayfieldUnit unitTryingToMove, Vector2Int targetLocation)
        {
            PlayfieldTile targetTile = playfield.world.Get(targetLocation);

            // No impassable tiles, do it as a permitted list so you can't go by default.
            if (targetTile.curIsImpassable)
            {
                return false;
            }

            // If there is a unit and it's not us, no-go.
            if(playfield.TryGetUnitAt(targetLocation, out PlayfieldUnit atTile))
            {
                if(atTile.id != unitTryingToMove.id)
                {
                    return false;
                }
            }

            return true;
        }

        public static void MoveUnitToLocation(Playfield playfield, VisualPlayfield visualizerPlayfield, PlayfieldUnit unit, Vector2Int target)
        {
            // Step the unit to the new place. Ensure this happens before visualizer update.
            Utils.StepUnitTo(unit, playfield, target, moveCost: 1);

            if (playfield.TryGetItemAt(target, out PlayfieldItem item))
            {
                playfield.RemoveItemAt(target);
            }

            visualizerPlayfield.DisplayUnits(playfield);
            visualizerPlayfield.DisplayItems(playfield);
            visualizerPlayfield.DisplayIndicatorMovePreview(unit, playfield);
        }
    }
}