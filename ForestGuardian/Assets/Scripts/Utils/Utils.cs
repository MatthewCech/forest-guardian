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
            UnityEngine.Assertions.Assert.IsFalse(unitToMove.curMovementBudget - moveCost < 0 && unitToMove.curMovesTaken > 0, $"You must have enough available steps to pay for the projected move cost. Some check elsewhere probably failed. Had: {unitToMove.curMovementBudget}, Cost: {moveCost}");

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

            // If we're stepping on ourselves, so we need to move the position we know
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

            // Charge 'em, and track the move!
            unitToMove.curMovementBudget -= moveCost;
            unitToMove.curMovesTaken += 1;
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

            // If we have no ability to move, there's no rule that will save us.
            if (unitTryingToMove.curMaxMovementBudget <= 0)
            {
                return false;
            }

            // If there is a unit and it's not us, no-go. That's overlap.
            if (playfield.TryGetUnitAt(targetLocation, out PlayfieldUnit atTile))
            {
                if(atTile.id != unitTryingToMove.id)
                {
                    return false;
                }
            }

            // If the move is too expensive AND we've already moved, we can't move.
            // This is written like this to allow a single movement if we have a speed > 0 but we haven't moved yet. 
            if(unitTryingToMove.curMovementBudget - targetTile.curMoveDifficulty < 0 && unitTryingToMove.curMovesTaken != 0)
            {
                return false;
            }
               
            return true;
        }

        /// <summary>
        /// Take the provided playfield unit and move it to the specified location on the provided playfield,
        /// then update visuals to reflect this. 
        /// 
        /// #TODO: Move this logic?
        /// For the player only, collect currency / collect money / collect acorns if the new location has that item.
        /// </summary>
        /// <param name="playfield"></param>
        /// <param name="visualizerPlayfield"></param>
        /// <param name="unit"></param>
        /// <param name="target"></param>
        public static void MoveUnitToLocation(Playfield playfield, VisualPlayfield visualizerPlayfield, PlayfieldUnit unit, Vector2Int target)
        {
            int moveCost = playfield.world.Get(target).curMoveDifficulty;

            // Step the unit to the new place. Ensure this happens before visualizer update.
            StepUnitTo(unit, playfield, target, moveCost);

            // #TODO: Consider moving this?
            if (unit.team == Team.Player && playfield.TryGetItemAt(target, out PlayfieldItem item))
            {
                // NOTE: Assume all items are currency at this time...
                ++Core.Instance.GameData.currency;
                playfield.RemoveItemAt(target);
            }

            visualizerPlayfield.DisplayUnits(playfield);
            visualizerPlayfield.DisplayItems(playfield);
            visualizerPlayfield.DisplayIndicatorMovePreview(unit, playfield);
        }

        /// <summary>
        /// Initializes a new playfield with the specialized size with default tiles.
        /// 
        /// If an existing playfield is provided, attempts to move the data over by either cropping 
        /// the existing data to fit the new map or expanding the map and copying over what's present.
        /// </summary>
        /// <param name="newWidth">Width of the new playfield, minimum 1</param>
        /// <param name="newHeight">Height of the new playfield, minimum 1</param>
        /// <param name="lookup">Visual lookup, provides default tile information.</param>
        /// <param name="existing">Optional. An existing playfield to try and copy data over from - clones all the way down.</param>
        /// <returns></returns>
        public static Playfield CreatePlayfield(VisualLookup lookup, int newWidth, int newHeight, Playfield existing = null)
        {
            UnityEngine.Assertions.Assert.IsTrue(newWidth > 0);
            UnityEngine.Assertions.Assert.IsTrue(newHeight > 0);

            // Create and configure blank playfield
            Playfield newPlayfield = new Playfield();
            newPlayfield.items = new List<PlayfieldItem>();
            newPlayfield.units = new List<PlayfieldUnit>();
            newPlayfield.world = new Collection2D<PlayfieldTile>(newWidth, newHeight);
            newPlayfield.portals = new List<PlayfieldPortal>();
            newPlayfield.origins = new List<PlayfieldOrigin>();
            newPlayfield.exit = null;

            for (int x = 0; x < newWidth; ++x)
            {
                for (int y = 0; y < newHeight; ++y)
                {
                    PlayfieldTile tile = new PlayfieldTile();
                    tile.tag = lookup.defaultTileTemplate.name;
                    tile.id = newPlayfield.GetNextID();
                    newPlayfield.world.Set(x, y, tile);
                }
            }

            if (existing != null)
            {
                MoveDataToNewPlayfield(existing, newPlayfield);
            }

            return newPlayfield;
        }


        private static void MoveDataToNewPlayfield(Playfield existing, Playfield newPlayfield)
        {
            // Local function to check if we're within bounds.
            bool InCombinedBounds(Vector2Int pos)
            {
                bool inFirst = pos.x < existing.world.GetWidth() && pos.y < existing.world.GetHeight();
                bool inSecond = pos.x < newPlayfield.world.GetWidth() && pos.y < newPlayfield.world.GetHeight();
                return inFirst && inSecond;
            }

            newPlayfield.world.ScrapeDataFrom(existing.world);

            // Back to front move over units that are still in the new bounds of the playfield.
            // The entire location/body of the unit must be in the new resize playfield or it all gets removed.
            for (int i = existing.units.Count - 1; i >= 0; --i)
            {
                PlayfieldUnit unit = existing.units[i];

                bool shouldRemove = false;
                for (int bodyPos = 0; bodyPos < unit.locations.Count; ++bodyPos)
                {
                    Vector2Int cur = unit.locations[bodyPos];

                    if (!InCombinedBounds(cur))
                    {
                        shouldRemove = true;
                        break;
                    }
                }

                if (shouldRemove)
                {
                    existing.units.RemoveAt(i);
                    continue;
                }
            }
            newPlayfield.units = existing.units;

            for (int i = existing.items.Count - 1; i >= 0; --i)
            {
                PlayfieldItem item = existing.items[i];
                Vector2Int cur = item.location;

                if (!InCombinedBounds(cur))
                {
                    existing.units.RemoveAt(i);
                    continue;
                }
            }
            newPlayfield.items = existing.items;

            for (int i = existing.portals.Count - 1; i >= 0; --i)
            {
                PlayfieldPortal portal = existing.portals[i];
                Vector2Int cur = portal.location;

                if (!InCombinedBounds(cur))
                {
                    existing.portals.RemoveAt(i);
                    continue;
                }
            }
            newPlayfield.portals = existing.portals;

            for (int i = existing.origins.Count - 1; i >= 0; --i)
            {
                PlayfieldOrigin origin = existing.origins[i];
                Vector2Int cur = origin.location;

                if (!InCombinedBounds(cur))
                {
                    existing.origins.RemoveAt(i);
                    continue;
                }
            }
            newPlayfield.origins = existing.origins;

            if (existing.exit != null)
            {
                Vector2Int curExitPos = existing.exit.location;
                if (!InCombinedBounds(curExitPos))
                {
                    existing.exit = null;
                }
            }
            newPlayfield.exit = existing.exit;

            // Other data
            newPlayfield.tagLabel = existing.tagLabel;
            newPlayfield.tagsBestowed = existing.tagsBestowed;
        }
    }
}