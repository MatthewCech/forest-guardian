using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using Loam;

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
        private List<Indicator> indicatorTracking;

        // Base parent object
        private Transform spawnParent;
        private VisualLookup lookup;

        private void Awake()
        {
            tileTracking = new List<Tile>();
            unitTracking = new List<Unit>();
            itemTracking = new List<Item>();

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

            if(unit.curMovementBudget == 0)
            {
                return;
            }

            DisplayIndicatorPreviewDiamond(unit, unit.curMovementBudget, playfield, headLocation, lookup.movePreviewTemplate);

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
            DisplayIndicatorPreviewDiamond(unit, unit.curAttackRange, playfield, headLocation, lookup.attackPreview);
        }

        private void DisplayIndicatorPreviewDiamond(PlayfieldUnit unit, int range, Playfield playfield, Vector2Int headLocation, Indicator indicator)
        {
            if(range == 0)
            {
                return;
            }

            Vector2Int moveSquareCorner = new Vector2Int(headLocation.x + range, headLocation.y + range);

            int moveAreaWidth = range * 2 + 1; // Guarenteed to be odd
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
                    if(modX == headLocation.x && modY == headLocation.y)
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

        private Unit GetUnit(PlayfieldUnit unit)
        {
            for(int i = 0; i < unitTracking.Count; ++i)
            {
                Unit cur = unitTracking[i];
                if(cur.associatedData.id == unit.id)
                {
                    return cur;
                }
            }

            return null;
        }

        public void DamageUnit(PlayfieldUnit attackingUnit, PlayfieldUnit defendingUnit, Playfield playfield)
        {
            if(attackingUnit.id == defendingUnit.id)
            {
                return;
            }

            Unit attacking = GetUnit(attackingUnit);
            Unit defending = GetUnit(defendingUnit);

            int damage = attacking.attackDamage;
            if(damage <= 0)
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

            if(defendingUnit.id == Playfield.NO_ID)
            {
                unitTracking.Remove(defending); // Remove tracking of visuals
                Destroy(defending.gameObject); // Destroy visuals
            }
            
            DisplayUnits(playfield);
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

            for (int i = 0; i < toDisplay.items.Count; ++i)
            {
                PlayfieldItem item = toDisplay.items[i];
                CreateItem(item);
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
        }

        /// <summary>
        /// Internal utility for clearing out all tracked lists and objects. Expensive, but sometimes necessary.
        /// </summary>
        private void DestroyAll()
        {
            ClearMonoBehaviourList(tileTracking);
            ClearMonoBehaviourList(unitTracking);
            ClearMonoBehaviourList(itemTracking);
            ClearMonoBehaviourList(indicatorTracking);
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
                
                instance.SetBodyVisibility(i == PlayfieldUnit.HEAD_INDEX);

                float x = Offset(curLocation.x);
                float y = Offset(curLocation.y);
                instance.transform.position = new Vector3(x, -y, -lookup.unitZPriority);

                unitTracking.Add(instance);
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
        /// Clear out a list of monobehaviors and clears the list in place. All elements cleared after a Destroy call.
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