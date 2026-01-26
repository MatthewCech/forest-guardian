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
            for (int i = 0; i < unitToMove.locations.Count; ++i)
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
            if (unitToShorten.locations.Count == 0)
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
                if (atTile.id != unitTryingToMove.id)
                {
                    return false;
                }
            }

            // If the move is too expensive AND we've already moved, we can't move.
            // This is written like this to allow a single movement if we have a speed > 0 but we haven't moved yet. 
            if (unitTryingToMove.curMovementBudget - targetTile.curMoveDifficulty < 0 && unitTryingToMove.curMovesTaken != 0)
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
        /// Check against direct up/down/left/right world tiles to see if they can be moved to.
        /// </summary>
        /// <param name="playfield"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool CanAffordAnyMove(Playfield playfield, PlayfieldUnit unitTryingToMove)
        {
            // If we have no budget, don't even bother running checks.
            if (unitTryingToMove.curMovementBudget <= 0)
            {
                return false;
            }

            // Ok we have a budget, so can we do anything?
            Vector2Int head = unitTryingToMove.locations[PlayfieldUnit.HEAD_INDEX];

            bool canMove = false;

            canMove |= CanMovePlayfieldUnitTo(playfield, unitTryingToMove, head + Vector2Int.right);
            canMove |= CanMovePlayfieldUnitTo(playfield, unitTryingToMove, head + Vector2Int.left);
            canMove |= CanMovePlayfieldUnitTo(playfield, unitTryingToMove, head + Vector2Int.up);
            canMove |= CanMovePlayfieldUnitTo(playfield, unitTryingToMove, head + Vector2Int.down);

            return canMove;
        }

        /// <summary>
        /// Locates the first non-default tiles on the left, right, top, and bottom.
        /// Adds a new playfield with the expected border on both sides, and copies over
        /// the previous playfield non-destructively with specified border.
        /// </summary>
        public static Playfield TrimPlayfield(Playfield toTrim, int border)
        {
            int GetLeft()
            {
                for (int x = 0; x < toTrim.Width(); ++x)
                {
                    for (int y = 0; y < toTrim.Height(); ++y)
                    {
                        if (!toTrim.world.Get(x, y).tag.Equals(VisualLookup.TILE_DEFAULT_NAME))
                        {
                            return x;
                        }
                    }
                }

                return 0;
            }

            int GetRight()
            {
                for (int x = toTrim.Width() - 1; x >= 0; --x)
                {
                    for (int y = 0; y < toTrim.Height(); ++y)
                    {
                        if (!toTrim.world.Get(x, y).tag.Equals(VisualLookup.TILE_DEFAULT_NAME))
                        {
                            return x;
                        }
                    }
                }

                return toTrim.Width() - 1;
            }

            int GetTop()
            {
                for (int y = 0; y < toTrim.Height(); ++y)
                {
                    for (int x = 0; x < toTrim.Width(); ++x)
                    {
                        if (!toTrim.world.Get(x, y).tag.Equals(VisualLookup.TILE_DEFAULT_NAME))
                        {
                            return y;
                        }
                    }
                }

                return 0;
            }

            int GetBottom()
            {
                for (int y = toTrim.Height() - 1; y >= 0; --y)
                {
                    for (int x = 0; x < toTrim.Width(); ++x)
                    {
                        if (!toTrim.world.Get(x, y).tag.Equals(VisualLookup.TILE_DEFAULT_NAME))
                        {
                            return y;
                        }
                    }
                }

                return toTrim.Height() - 1;
            }

            int left = GetLeft();
            int right = GetRight();
            int top = GetTop();
            int bottom = GetBottom();

            int width = right - left + border * 2 + 1;
            int height = bottom - top + border * 2 + 1;

            Playfield newPlayfield = Utils.CreatePlayfield(width, height);

            for (int x = left; x < right + 1; ++x)
            {
                for (int y = top; y < bottom + 1; ++y)
                {
                    PlayfieldTile newTile = toTrim.world.Get(x, y).Clone(newPlayfield.GetNextID());
                    newPlayfield.world.Set(x - left + border, y - top + border, newTile);
                }
            }

            newPlayfield.tagLabel = toTrim.tagLabel;
            newPlayfield.description = toTrim.description;

            foreach (string tag in toTrim.tagsBestowed)
            {
                newPlayfield.tagsBestowed.Add(tag);
            }

            for (int i = 0; i < toTrim.units.Count; ++i)
            {
                PlayfieldUnit unit = toTrim.units[i].Clone(newPlayfield.GetNextID());
                for (int loc = 0; loc < unit.locations.Count; ++loc)
                {
                    Vector2Int moved = unit.locations[loc];
                    moved.x = moved.x - left + border;
                    moved.y = moved.y - top + border;
                    unit.locations[loc] = moved;
                }
                
                newPlayfield.units.Add(unit);
            }

            for (int i = 0; i < toTrim.items.Count; ++i)
            {
                PlayfieldItem item = toTrim.items[i].Clone(newPlayfield.GetNextID());
                item.location.x = item.location.x - left + border;
                item.location.y = item.location.y - top + border;

                newPlayfield.items.Add(item);
            }

            for (int i = 0; i < toTrim.portals.Count; ++i)
            {
                PlayfieldPortal portal = toTrim.portals[i].Clone(newPlayfield.GetNextID());
                portal.location.x = portal.location.x - left + border;
                portal.location.y = portal.location.y - top + border;

                newPlayfield.portals.Add(portal);
            }

            for (int i = 0; i < toTrim.origins.Count; ++i)
            {
                PlayfieldOrigin origin = toTrim.origins[i].Clone(newPlayfield.GetNextID());
                origin.location.x = origin.location.x - left + border;
                origin.location.y = origin.location.y - top + border;

                newPlayfield.origins.Add(origin);
            }

            return newPlayfield;
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
        public static Playfield CreatePlayfield(int newWidth, int newHeight, Playfield existing = null)
        {
            UnityEngine.Assertions.Assert.IsTrue(newWidth > 0);
            UnityEngine.Assertions.Assert.IsTrue(newHeight > 0);

            // Create and configure blank playfield
            Playfield newPlayfield = new Playfield();
            newPlayfield.tagLabel = null;
            newPlayfield.description = null;
            newPlayfield.tagsBestowed = new List<string>();

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
                    tile.tag = VisualLookup.TILE_DEFAULT_NAME;
                    tile.id = newPlayfield.GetNextID();
                    newPlayfield.world.Set(x, y, tile);
                }
            }

            if (existing != null)
            {
                MoveDataFromExistingPlayfield(newPlayfield, existing);
            }

            return newPlayfield;
        }

        /// <summary>
        /// Combines two existing playfields. There's an on-top specified because in cases
        /// where an aspect stamps over another, it's important to decide which tiles overwrite.
        /// 
        /// Things that get written over:
        /// - Origin list
        /// - Portal List
        /// </summary>
        public static Playfield LayerPlayfields(Playfield under, Playfield onTop)
        {
            UnityEngine.Assertions.Assert.IsNotNull(under);
            UnityEngine.Assertions.Assert.IsNotNull(onTop);
            UnityEngine.Assertions.Assert.IsTrue(under.world.GetWidth() == onTop.world.GetWidth());
            UnityEngine.Assertions.Assert.IsTrue(under.world.GetHeight() == onTop.world.GetHeight());

            int width = under.world.GetWidth();
            int height = under.world.GetHeight();

            Playfield layered = CreatePlayfield(width, height);

            for(int x = 0; x < width; ++x)
            {
                for(int y = 0; y < height; ++y)
                {
                    PlayfieldTile topTile = onTop.world.Get(x, y);

                    if(topTile.tag.Equals(VisualLookup.TILE_DEFAULT_NAME))
                    {
                        // If we have nothing above, we use what's below.
                        PlayfieldTile tile = under.world.Get(x, y).Clone(layered.GetNextID());
                        layered.world.Set(x, y, tile);
                    }
                    else
                    {
                        PlayfieldTile tile = onTop.world.Get(x, y).Clone(layered.GetNextID());
                        layered.world.Set(x, y, tile);
                    }
                }
            }

            layered.tagLabel = under.tagLabel;
            if (!string.IsNullOrWhiteSpace(onTop.tagLabel))
            {
                layered.tagLabel = onTop.tagLabel;
            }

            layered.description = under.description;
            if(!string.IsNullOrWhiteSpace(onTop.description))
            {
                layered.tagLabel = onTop.description;
            }

            if (onTop.tagsBestowed != null && onTop.tagsBestowed.Count > 0)
            {
                layered.tagsBestowed = new List<string>(onTop.tagsBestowed);
            }
            else if(under.tagsBestowed != null)
            {
                layered.tagsBestowed = new List<string>(under.tagsBestowed);
            }

            for (int i = 0; i < under.units.Count; ++i)
            {
                layered.units.Add(under.units[i].Clone(layered.GetNextID()));
            }
            for (int i = 0; i < onTop.units.Count; ++i)
            {
                layered.units.Add(onTop.units[i].Clone(layered.GetNextID()));
            }

            for (int i = 0; i < under.items.Count; ++i)
            {
                layered.items.Add(under.items[i].Clone(layered.GetNextID()));
            }
            for (int i = 0; i < onTop.items.Count; ++i)
            {
                layered.items.Add(onTop.items[i].Clone(layered.GetNextID()));
            }

            for (int i = 0; i < under.portals.Count; ++i)
            {
                layered.portals.Add(under.portals[i].Clone(layered.GetNextID()));
            }
            for (int i = 0; i < onTop.portals.Count; ++i)
            {
                layered.portals.Add(onTop.portals[i].Clone(layered.GetNextID()));
            }

            for (int i = 0; i < under.origins.Count; ++i)
            {
                layered.origins.Add(under.origins[i].Clone(layered.GetNextID()));
            }
            for (int i = 0; i < onTop.origins.Count; ++i)
            {
                layered.origins.Add(onTop.origins[i].Clone(layered.GetNextID()));
            }

            return layered;
        }

        /// <summary>
        /// Does an aggressive and destructive move from an existing playfield to a new playfield.
        /// Handles different sizes of playfield.
        /// </summary>
        /// <param name="target">The playfield, likely new, that will get the items from the existing.</param>
        /// <param name="existing">The existing playfield to pull from</param>
        /// <param name=""></param>
        private static void MoveDataFromExistingPlayfield(Playfield target, Playfield existing)
        {
            // Local function to check if we're within bounds.
            bool InCombinedBounds(Vector2Int pos)
            {
                bool inFirst = pos.x < existing.world.GetWidth() && pos.y < existing.world.GetHeight();
                bool inSecond = pos.x < target.world.GetWidth() && pos.y < target.world.GetHeight();
                return inFirst && inSecond;
            }

            target.world.ScrapeDataFrom(existing.world);

            // World related content
            target.tagLabel = existing.tagLabel;
            target.description = existing.description;
            target.tagsBestowed = existing.tagsBestowed;

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
            target.units = existing.units;

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
            target.items = existing.items;

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
            target.portals = existing.portals;

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
            target.origins = existing.origins;

            if (existing.exit != null)
            {
                Vector2Int curExitPos = existing.exit.location;
                if (!InCombinedBounds(curExitPos))
                {
                    existing.exit = null;
                }
            }
            target.exit = existing.exit;
        }


        public static void CopyToClipboard(string toClipboard)
        {
            GUIUtility.systemCopyBuffer = toClipboard;
        }

    }
}