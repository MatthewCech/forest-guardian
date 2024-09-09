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

        public static void StepUnitTo(PlayfieldUnit unit, Playfield playfield, Vector2Int pos, int moveCost)
        {
            UnityEngine.Assertions.Assert.IsFalse(unit.movementBudget - moveCost < 0, $"You must have enough available steps to pay for the projected move cost. Some check elsewhere probably failed. Had: {unit.movementBudget}, Cost: {moveCost}");

            PlayfieldTile targetTile = playfield.world.Get(pos.x, pos.y);

            // Associate new tile.
            targetTile.associatedUnitID = unit.id;

            // Head remains at 0, we just moved here.
            unit.locations.Insert(0, pos);

            // Charge 'em!
            unit.movementBudget -= moveCost;
        }
    }
}