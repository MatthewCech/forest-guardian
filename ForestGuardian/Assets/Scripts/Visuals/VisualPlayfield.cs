using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using Loam;
using static UnityEditor.Progress;
using static UnityEngine.UI.CanvasScaler;

namespace forest
{
    public class VisualPlayfield : MonoBehaviour
    {
        // External
        [Range(0f, 1f)][SerializeField] private float gridSpacing = 0.1f;

        // Internal tracking
        private List<Tile> tileTracking;
        private List<Unit> unitTracking;
        private List<Item> itemTracking;
        private List<Portal> portalTracking;
        private Exit trackedExit;

        private List<Indicator> indicatorTracking;

        // Base parent object
        private Transform spawnParent;
        private VisualLookup lookup;

        private void Awake()
        {
            tileTracking = new List<Tile>();
            unitTracking = new List<Unit>();
            itemTracking = new List<Item>();
            portalTracking = new List<Portal>();
            trackedExit = null;

            indicatorTracking = new List<Indicator>();
        }

        public void Initialize(VisualLookup lookup)
        {
            this.lookup = lookup;
        }

        /// <summary>
        /// Low efficiency trawl to find bounds. Combine 
        /// this to happen during spawning ideally, done individually for debug for now.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetTileCenter()
        {
            Vector2 center = Vector2.zero;
            float xMin = int.MaxValue;
            float yMin = int.MaxValue;
            float xMax = int.MinValue;
            float yMax = int.MinValue;

            for (int i = 0; i < tileTracking.Count; i++)
            {
                Tile cur = tileTracking[i];
                float xCur = cur.transform.position.x;
                float yCur = cur.transform.position.y;

                xMin = Mathf.Min(xCur, xMin);
                yMin = Mathf.Min(yCur, yMin);
                xMax = Mathf.Max(xCur, xMax);
                yMax = Mathf.Max(yCur, yMax);
            }

            return new Vector2(xMin + (xMax - xMin) / 2, yMin + (yMax - yMin) / 2);
        }

        /// <summary>
        /// Given a unit, shows the moves the unit can take based on remaining moves available and the current playfield.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="playfield"></param>
        public void DisplayIndicatorMovePreview(PlayfieldUnit unit, Playfield playfield)
        {
            ClearMonoBehaviourList(indicatorTracking);

            Vector2Int headLocation = unit.locations[PlayfieldUnit.HEAD_INDEX];

            if (unit.curMovementBudget == 0)
            {
                return;
            }

            //MoveDiamondDisplay(unit, unit.curMovementBudget, playfield, headLocation, lookup.movePreviewTemplate);
            MoveFlood(unit, unit.curMovementBudget, playfield, lookup.movePreviewTemplate);

            DisplayIndicator(headLocation.x - 1, headLocation.y, playfield, unit, lookup.moveInteractionTemplate);
            DisplayIndicator(headLocation.x + 1, headLocation.y, playfield, unit, lookup.moveInteractionTemplate);
            DisplayIndicator(headLocation.x, headLocation.y - 1, playfield, unit, lookup.moveInteractionTemplate);
            DisplayIndicator(headLocation.x, headLocation.y + 1, playfield, unit, lookup.moveInteractionTemplate);
        }

        public void HideIndicators()
        {
            ClearMonoBehaviourList(indicatorTracking);
        }

        public void DisplayIndicatorAttackPreview(PlayfieldUnit unit, Playfield playfield)
        {
            ClearMonoBehaviourList(indicatorTracking);

            Vector2Int headLocation = unit.locations[PlayfieldUnit.HEAD_INDEX];
            DisplayIndicatorDiamond(unit, unit.curAttackRange, playfield, headLocation, lookup.attackPreview);
        }

        private void MoveFlood(PlayfieldUnit unit, int range, Playfield playfield, Indicator indicator)
        {
            List<SearchNode<Tile>> pending = new List<SearchNode<Tile>>();
            List<SearchNode<Tile>> visited = new List<SearchNode<Tile>>();

            // Not required but explicit future guard
            pending.Clear();
            visited.Clear();

            bool VisitedContains(Tile toFind)
            {
                return visited.Find(f => f.data == toFind) != null;
            }

            bool PendingContains(Tile toFind)
            {
                return pending.Find(f => f.data == toFind) != null;
            }

            int DistanceFromHead(SearchNode<Tile> item)
            {
                int distance = 0;
                while (item.parent != null)
                {
                    distance += item.StartingCost;
                    item = item.parent;
                }

                return distance;
            }

            void TryAdd(SearchNode<Tile> parent, Vector2Int offset, int maxDistance)
            {
                Vector2Int pos = parent.data.associatedPos;
                Vector2Int targetPos = pos + offset;

                if (playfield.world.IsPosInGrid(targetPos))
                {
                    Tile toAdd = FindTile(targetPos);
                    if (VisitedContains(toAdd))
                    {
                        return;
                    }

                    if (toAdd.isImpassable)
                    {
                        return;
                    }

                    if (!PendingContains(toAdd))
                    {
                        SearchNode<Tile> node = new SearchNode<Tile>(toAdd, toAdd.moveDifficulty);
                        node.parent = parent;

                        int dist = DistanceFromHead(node);
                        if (dist > maxDistance)
                        {
                            return;
                        }

                        pending.Add(node);
                    }
                }
            }

            Vector2Int startPos = unit.locations[PlayfieldUnit.HEAD_INDEX];
            Tile start = FindTile(startPos);
            pending.Add(new SearchNode<Tile>(start, start.moveDifficulty));

            while (pending.Count > 0)
            {
                SearchNode<Tile> item = pending[0];
                pending.RemoveAt(0);

                --item.curNodeCost;
                if (item.curNodeCost > 0)
                {
                    pending.Add(item);
                    continue;
                }

                visited.Add(item);

                TryAdd(item, Vector2Int.left, range);
                TryAdd(item, Vector2Int.right, range);
                TryAdd(item, Vector2Int.up, range);
                TryAdd(item, Vector2Int.down, range);
            }

            foreach (var tileNode in visited)
            {
                bool isHeadLocation = tileNode.parent == null;
                if (isHeadLocation)
                {
                    continue;
                }

                DisplayIndicator(tileNode.data.associatedPos, playfield, unit, indicator);
            }
        }

        private void DisplayIndicatorDiamond(PlayfieldUnit unit, int range, Playfield playfield, Vector2Int headLocation, Indicator indicator)
        {
            if (range == 0)
            {
                return;
            }

            Vector2Int moveSquareCorner = new Vector2Int(headLocation.x + range, headLocation.y + range);

            int moveAreaWidth = range * 2 + 1; // Guaranteed to be odd
            int halfWidth = moveAreaWidth / 2;

            for (int x = 0; x < moveAreaWidth; ++x)
            {
                for (int y = 0; y < moveAreaWidth; ++y)
                {
                    int modX = moveSquareCorner.x - x;
                    int modY = moveSquareCorner.y - y;

                    // Prevent wrapping around the map, we just don't want to deal with that now.
                    if (modX < 0 || modY < 0)
                    {
                        continue;
                    }

                    // Prevent showing preview on head specifically.
                    if (modX == headLocation.x && modY == headLocation.y)
                    {
                        continue;
                    }

                    // If something doesn't exist or does exist and is impassable, then no target can be on it so don't show.
                    Tile curTarget = FindTile(new Vector2Int(modX, modY));
                    if (curTarget == null || curTarget.isImpassable)
                    {
                        continue;
                    }

                    if (x >= Mathf.Abs(moveAreaWidth - y - 1 - halfWidth)
                    && moveAreaWidth - x > Mathf.Abs(moveAreaWidth - y - 1 - halfWidth))
                    {
                        DisplayIndicator(modX, modY, playfield, unit, indicator);
                    }
                }
            }
        }

        public Unit FindUnit(PlayfieldUnit unit)
        {
            return unitTracking.Find((cur) => cur.associatedData.id == unit.id);
        }

        public Tile FindTile(Vector2Int location)
        {
            return tileTracking.Find((cur) => cur.associatedPos == location);
        }

        public Portal FindPortal(Vector2Int location)
        {
            return portalTracking.Find((cur) => cur.associatedPos == location);
        }

        public void DamageUnit(PlayfieldUnit attackingUnit, PlayfieldUnit defendingUnit, Playfield playfield)
        {
            if (attackingUnit.id == defendingUnit.id)
            {
                return;
            }

            Unit attacking = FindUnit(attackingUnit);
            Unit defending = FindUnit(defendingUnit);

            int damage = attacking.data.moves[0].moveDamage;
            if (damage <= 0)
            {
                return;
            }

            while (damage-- > 0 && defendingUnit.id != Playfield.NO_ID)
            {
                Vector2Int willBeNuked = defendingUnit.locations.Tail();
                Utils.ShortenUnitTailByOne(defendingUnit, playfield);

                MsgUnitSegmentDestroyed msg = new MsgUnitSegmentDestroyed();
                msg.attackingUnitID = attackingUnit.id;
                msg.defendingUnitID = defendingUnit.id;
                msg.destroyedPosition = willBeNuked;

                Postmaster.Instance.Send(msg);
            }

            if (defendingUnit.id == Playfield.NO_ID)
            {
                unitTracking.Remove(defending); // Remove tracking of visuals
                Destroy(defending.gameObject); // Destroy visuals
            }

            DisplayUnits(playfield);
        }


        public void ShowMovePath(Playfield playfield, PlayfieldUnit unit, List<Tile> steps)
        {
            foreach (Tile tile in steps)
            {
                DisplayIndicator(tile.associatedPos, playfield, unit, lookup.movePathTemplate);
            }
        }

        private void DisplayIndicator(Vector2Int pos, Playfield playfield, PlayfieldUnit unitDisplayingIndicatorFor, Indicator indicatorTemplate)
        {
            DisplayIndicator(pos.x, pos.y, playfield, unitDisplayingIndicatorFor, indicatorTemplate);
        }

        private void DisplayIndicator(int x, int y, Playfield playfield, PlayfieldUnit unitDisplayingIndicatorFor, Indicator indicatorTemplate)
        {
            EnsureParentObjectExists();

            if (x < 0 || x >= playfield.world.GetWidth() || y < 0 || y >= playfield.world.GetHeight())
            {
                return;
            }

            PlayfieldTile toOverlay = playfield.world.Get(x, y);

            // Confirm if immediate move is possible if that's the indicator type.
            if (indicatorTemplate.type == IndicatorType.ImmediateMove || indicatorTemplate.type == IndicatorType.Preview)
            {
                if (!Utils.CanMovePlayfieldUnitTo(playfield, unitDisplayingIndicatorFor, new Vector2Int(x, y)))
                {
                    return;
                }
            }

            DisplayIndicatorAt(x, y, playfield, indicatorTemplate, unitDisplayingIndicatorFor);
        }

        /// <summary>
        /// Without any checking, displays the specified indicator at the specified location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="playfield"></param>
        /// <param name="indicatorTemplate"></param>
        /// <param name="unitDisplayingIndicatorFor"></param>
        public void DisplayIndicatorAt(int x, int y, Playfield playfield, Indicator indicatorTemplate, PlayfieldUnit unitDisplayingIndicatorFor = null)
        {
            PlayfieldTile toOverlay = playfield.world.Get(x, y);

            Indicator movePreview = Instantiate(indicatorTemplate, spawnParent);
            movePreview.associatedTile = toOverlay;
            movePreview.ownerUnit = unitDisplayingIndicatorFor;
            movePreview.overlaidPosition = new Vector2Int(x, y);

            float xPos = Offset(x);
            float yPos = Offset(y);
            movePreview.transform.position = new Vector3(xPos, -yPos, -lookup.interactionZPriority);

            indicatorTracking.Add(movePreview);
        }

        /// <summary>
        /// Re-draws just playfield  units
        /// </summary>
        /// <param name="toDisplay"></param>
        public void DisplayUnits(Playfield toDisplay)
        {
            ClearMonoBehaviourList(unitTracking);

            if(toDisplay.units == null)
            {
                return;
            }

            for (int i = 0; i < toDisplay.units.Count; ++i)
            {
                PlayfieldUnit unit = toDisplay.units[i];
                CreateUnit(unit);
            }
        }

        /// <summary>
        /// Re-draws just playfield items
        /// </summary>
        /// <param name="toDisplay"></param>
        public void DisplayItems(Playfield toDisplay)
        {
            ClearMonoBehaviourList(itemTracking);

            if(toDisplay.items == null)
            {
                return;
            }

            for (int i = 0; i < toDisplay.items.Count; ++i)
            {
                PlayfieldItem item = toDisplay.items[i];
                CreateItem(item);
            }
        }

        public void DisplayPortals(Playfield toDisplay)
        {
            ClearMonoBehaviourList(portalTracking);

            if(toDisplay.portals == null)
            {
                return;
            }

            for (int i = 0; i < toDisplay.portals.Count; ++i)
            {
                PlayfieldPortal portal = toDisplay.portals[i];
                CreatePortal(portal);
            }
        }

        public void DisplayExit(Playfield toDisplay)
        { 
            if(trackedExit != null)
            {
                Destroy(trackedExit.gameObject);
                trackedExit = null;
            }

            if(toDisplay.exit != null)
            {
                CreateExit(toDisplay.exit);
            }
        }

        /// <summary>
        /// Expensive draw-from-scratch for all items
        /// </summary>
        /// <param name="toDisplay"></param>
        public void DisplayAll(Playfield toDisplay)
        {
            int width = toDisplay.world.GetWidth();
            int height = toDisplay.world.GetHeight();

            DestroyAll();

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    CreateTile(x, y, toDisplay.world.Get(x, y));
                }
            }

            DisplayUnits(toDisplay);
            DisplayItems(toDisplay);
            DisplayPortals(toDisplay);
            DisplayExit(toDisplay);
        }

        /// <summary>
        /// Internal utility for clearing out all tracked lists and objects. Expensive, but sometimes necessary.
        /// </summary>
        private void DestroyAll()
        {
            ClearMonoBehaviourList(tileTracking);
            ClearMonoBehaviourList(unitTracking);
            ClearMonoBehaviourList(itemTracking);
            ClearMonoBehaviourList(portalTracking);
            ClearMonoBehaviourList(indicatorTracking);

            if (trackedExit != null)
            {
                Destroy(trackedExit.gameObject);
            }

            trackedExit = null;
        }

        /// <summary>
        /// Generate a tracked tile at a specified grid location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="data"></param>
        public void CreateTile(int x, int y, PlayfieldTile data)
        {
            EnsureParentObjectExists();

            Tile template = lookup.GetTileByTag(data.tag);
            Tile instance = GameObject.Instantiate(template, spawnParent);

            float xPos = Offset(x);
            float yPos = Offset(y);

            instance.transform.position = new Vector3(xPos, -yPos, 0);
            instance.associatedData = data;
            instance.associatedPos = new Vector2Int(x, y);

            tileTracking.Add(instance);
        }

        /// <summary>
        /// Generate a unit based on the specified data and place it in the world
        /// </summary>
        public void CreateUnit(PlayfieldUnit data)
        {
            EnsureParentObjectExists();

            Unit template = lookup.GetUnityByTag(data.tag);
            for (int i = 0; i < data.locations.Count; ++i)
            {
                Vector2Int curLocation = data.locations[i];
                Unit instance = GameObject.Instantiate(template, spawnParent);
                instance.associatedData = data;
                instance.gridPos = curLocation;

                bool isHead = i == PlayfieldUnit.HEAD_INDEX;
                instance.SetBodyVisibility(isHead);

                float x = Offset(curLocation.x);
                float y = Offset(curLocation.y);
                instance.transform.position = new Vector3(x, -y, -lookup.unitZPriority);

                unitTracking.Add(instance);

                // Link up pending body locations using head's line drawing solution if there are multiple segments.
                // Otherwise, hide the line.
                if (isHead && data.locations.Count > 1)
                {
                    int segmentCount = data.locations.Count;
                    Vector3[] positions = new Vector3[segmentCount];
                    for (int lineIndex = 0; lineIndex < segmentCount; ++lineIndex)
                    {
                        Vector2Int cur = data.locations[lineIndex];
                        float lineX = Offset(cur.x);
                        float lineY = Offset(cur.y);
                        positions[lineIndex] = new Vector3(lineX, -lineY, 0);
                    }

                    instance.segmentLink.enabled = true;
                    instance.segmentLink.positionCount = segmentCount;
                    instance.segmentLink.SetPositions(positions);
                }
                else
                {
                    instance.segmentLink.enabled = false;
                }
            }
        }

        /// <summary>
        /// Create a visual representation of the provided data within the world itself
        /// </summary>
        /// <param name="data"></param>
        public void CreateItem(PlayfieldItem data)
        {
            EnsureParentObjectExists();

            Vector2Int curLocation = data.location;

            Item template = lookup.GetItemByTag(data.tag);
            Item instance = GameObject.Instantiate(template, spawnParent);

            instance.associatedData = data;
            instance.gridPos = curLocation;

            float x = Offset(curLocation.x);
            float y = Offset(curLocation.y);

            instance.transform.position = new Vector3(x, -y, -lookup.unitZPriority);
            itemTracking.Add(instance);
        }

        public void CreatePortal(PlayfieldPortal data)
        {
            EnsureParentObjectExists();

            Vector2Int curLocation = data.location;

            Portal template = lookup.GetPortalByTag(data.tag);
            Portal instance = GameObject.Instantiate(template, spawnParent);

            instance.associatedData = data;
            instance.associatedPos = curLocation;

            float x = Offset(curLocation.x);
            float y = Offset(curLocation.y);

            instance.transform.position = new Vector3(x, -y, -lookup.unitZPriority);
            portalTracking.Add(instance);
        }

        public void CreateExit(PlayfieldExit data)
        {
            EnsureParentObjectExists();

            Vector2Int curLocation = data.location;

            Exit template = lookup.GetExitByTag(data.tag);
            Exit instance = GameObject.Instantiate(template, spawnParent);

            instance.associatedData = data;
            instance.gridPos = curLocation;

            float x = Offset(curLocation.x);
            float y = Offset(curLocation.y);

            instance.transform.position = new Vector3(x, -y, -lookup.unitZPriority);

            if(trackedExit != null)
            {
                Debug.LogError("Woah, there are TWO exists trying to draw. This is not good. Going to track the most recent but things are probably broken.");
            }

            trackedExit = instance;
        }

        /// <summary>
        /// Applies the positional offset to the specified coordinate location.
        /// For example, putting in 1 would return 1 + (1 * the offset). This allows building offset by position, as a portion of it.
        /// Not really intended for non-int inputs. Inputs are conceptualized as an X or Y coordinate on a grid.
        /// </summary>
        /// <param name="rawVal"></param>
        /// <returns></returns>
        private float Offset(int rawVal)
        {
            return rawVal + (rawVal * gridSpacing);
        }

        /// <summary>
        /// Ensure there's an object to hold all spawned objects under.
        /// </summary>
        private void EnsureParentObjectExists()
        {
            if (spawnParent == null)
            {
                spawnParent = new GameObject(this.GetType().Name).transform;
            }
        }

        /// <summary>
        /// Clear out a list of monobehaviours and clears the list in place. All elements cleared after a Destroy call.
        /// </summary>
        /// <param name="target">The list to operate on.</param>
        private void ClearMonoBehaviourList<T>(List<T> target) where T: MonoBehaviour
        {
            foreach (MonoBehaviour behavior in target)
            {
                Destroy(behavior.gameObject);
            }

            target.Clear();
        }
    }
}