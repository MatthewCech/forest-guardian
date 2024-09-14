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
        /// This forcebly moves you to a new location, but does not restrict unexpected movements.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="playfield"></param>
        /// <param name="pos"></param>
        /// <param name="moveCost"></param>
        public static void StepUnitTo(PlayfieldUnit unit, Playfield playfield, Vector2Int pos, int moveCost)
        {
            UnityEngine.Assertions.Assert.IsFalse(unit.curMovementBudget - moveCost < 0, $"You must have enough available steps to pay for the projected move cost. Some check elsewhere probably failed. Had: {unit.curMovementBudget}, Cost: {moveCost}");

            PlayfieldTile targetTile = playfield.world.Get(pos.x, pos.y);

            // Associate new tile.
            targetTile.associatedUnitID = unit.id;

            // See if we're going to be stepping on ourselves.
            bool isSteppingOnSelf = false;
            int indexBeingSteppedOn = -1;
            for(int i = 0; i < unit.locations.Count; ++i)
            {
                if (unit.locations[i] == pos)
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
                unit.locations.RemoveAt(indexBeingSteppedOn);
            }

            // Head remains where it was, we just moved here, so insert at that spot.
            unit.locations.Insert(PlayfieldUnit.HEAD_INDEX, pos);

            // Trim end if it's too long
            if (unit.locations.Count > unit.curMaxSize)
            {
                unit.locations.RemoveAt(unit.locations.Count - 1);
            }

            // Charge 'em!
            unit.curMovementBudget -= moveCost;
        }
    }
}