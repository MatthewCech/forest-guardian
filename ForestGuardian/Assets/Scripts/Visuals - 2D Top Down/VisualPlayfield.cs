using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class VisualPlayfield : MonoBehaviour
    {
        // External
        [Range(0f, 1f)][SerializeField] private float gridSpacing = 0.1f;

        // Internal tracking
        private List<Tile> tileTracking;
        private List<Unit> unitTracking;
        private List<Indicator> moveVisuals;

        // Base parent object
        private Transform spawnParent;
        private VisualLookup lookup;

        private void Awake()
        {
            tileTracking = new List<Tile>();
            unitTracking = new List<Unit>();
            moveVisuals = new List<Indicator>();
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

        public void HideMove()
        {
            if(moveVisuals == null)
            {
                return;
            }

            foreach(Indicator visual in moveVisuals)
            {
                GameObject.Destroy(visual.gameObject);
            }

            moveVisuals.Clear();
        }

        public void ShowMove(PlayfieldUnit unit, Playfield playfield)
        {
            HideMove();

            Vector2Int headLocation = unit.locations[unit.headIndex];

            if(unit.movementBudget == 0)
            {
                return;
            }

            DisplayMovePreviewDiamond(unit, playfield, headLocation);

            ShowMove(headLocation.x - 1, headLocation.y, playfield, unit, lookup.moveInteractionTemplate);
            ShowMove(headLocation.x + 1, headLocation.y, playfield, unit, lookup.moveInteractionTemplate);
            ShowMove(headLocation.x, headLocation.y - 1, playfield, unit, lookup.moveInteractionTemplate);
            ShowMove(headLocation.x, headLocation.y + 1, playfield, unit, lookup.moveInteractionTemplate);
        }


        private void DisplayMovePreviewDiamond(PlayfieldUnit unit, Playfield playfield, Vector2Int headLocation)
        {
            if(unit.movementBudget == 0)
            {
                return;
            }

            Vector2Int moveSquareCorner = new Vector2Int(headLocation.x + unit.movementBudget, headLocation.y + unit.movementBudget);

            int moveAreaWidth = unit.movementBudget * 2 + 1; // Guarenteed to be odd
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
                        ShowMove(modX, modY, playfield, unit, lookup.movePreviewTemplate);
                    }
                }
            }
        }

        private void ShowMove(int x, int y, Playfield playfield, PlayfieldUnit associatedUnit, Indicator indicatorTempalte)
        {
            EnsureParentObjectExists();

            PlayfieldTile tile = playfield.world.Get(x, y);

            // Explode on DEFAULT case. That's a bad tile, and it should be known about!
            UnityEngine.Assertions.Assert.IsFalse(tile.tileType == TileType.DEFAULT, "Default tiles are an invalid type of tile, and are not allowed for gameplay. Ensure all tiles are properly initialized.");

            // No impassable tiles!
            if(tile.tileType == TileType.Impassable || tile.tileType == TileType.Nothing)
            {
                return;
            }

            Indicator movePreview = Instantiate(indicatorTempalte, spawnParent);
            PlayfieldUnit unit = associatedUnit;
            movePreview.associatedTile = tile;
            movePreview.ownerUnit = unit;
            movePreview.overlaidPosition = new Vector2Int(x, y);

            float xPos = Offset(x);
            float yPos = Offset(y);
            movePreview.transform.position = new Vector3(xPos, -yPos, -lookup.interactionZPriority);

            moveVisuals.Add(movePreview);
        }

        /// <summary>
        /// Re-draws just the units
        /// </summary>
        /// <param name="toDisplay"></param>
        public void UpdateUnits(Playfield toDisplay)
        {
            for(int i = 0; i < unitTracking.Count; ++i)
            {
                Destroy(unitTracking[i].gameObject);
            }

            unitTracking.Clear();

            for(int i = 0; i < toDisplay.units.Count; ++i)
            {
                PlayfieldUnit unit = toDisplay.units[i];
                CreatUnit(unit);
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

            for (int x = 0; x < width; ++x)
            {
                float xPos = Offset(x);
                for (int y = 0; y < height; ++y)
                {
                    float yPos = Offset(y);
                    CreateTile(xPos, yPos, toDisplay.world.Get(x, y));
                }
            }

            UpdateUnits(toDisplay);
        }

        /// <summary>
        /// Generate a tracked tile at a specified grid location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="data"></param>
        public void CreateTile(float x, float y, PlayfieldTile data)
        {
            EnsureParentObjectExists();

            Tile template = lookup.GetTileByType(data.tileType).tileTemplate;

            Tile instance = GameObject.Instantiate(template, spawnParent);
            instance.transform.position = new Vector3(x, -y, 0);
            instance.associatedDataID = data.id;

            tileTracking.Add(instance);
        }

        public void CreatUnit(PlayfieldUnit data)
        {
            EnsureParentObjectExists();

            Unit template = lookup.GetUnityByTag(data.tag).unitTemplate;

            for (int i = 0; i < data.locations.Count; ++i)
            {
                Vector2Int curLocation = data.locations[i];
                Unit instance = GameObject.Instantiate(template, spawnParent);
                instance.associatedData = data;

                instance.SetBodyVisibility(i == data.headIndex);

                float x = Offset(curLocation.x);
                float y = Offset(curLocation.y);
                instance.transform.position = new Vector3(x, -y, -lookup.unitZPriority);

                unitTracking.Add(instance);
            }

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

        private void EnsureParentObjectExists()
        {
            if (spawnParent == null)
            {
                spawnParent = new GameObject(this.GetType().Name).transform;
            }
        }
    }
}