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
        private List<VisualSpawnedObjectTracker> tileTracking;
        private List<VisualSpawnedObjectTracker> unitTracking;
        private List<GameObject> moveVisuals;

        // Base parent object
        private Transform spawnParent;
        private Lookup lookup;

        private void Awake()
        {
            tileTracking = new List<VisualSpawnedObjectTracker>();
            unitTracking = new List<VisualSpawnedObjectTracker>();
            moveVisuals = new List<GameObject>();
        }

        public void Initialize(Lookup lookup)
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
                VisualSpawnedObjectTracker cur = tileTracking[i];
                float xCur = cur.spawned.transform.position.x;
                float yCur = cur.spawned.transform.position.y;

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

            foreach(GameObject visual in moveVisuals)
            {
                GameObject.Destroy(visual);
            }

            moveVisuals.Clear();
        }

        public void ShowMove(PlayfieldUnit unit)
        {
            Vector2Int headLocation = unit.locations[unit.headIndex];

            Vector2Int moveSquareCorner = new Vector2Int(headLocation.x + unit.movesRemaining, headLocation.y + unit.movesRemaining);
            int moveSquareWidth = unit.movesRemaining * 2 + 1;

            for (int x = 0; x < moveSquareWidth; ++x)
            {
                for(int y = 0; y < moveSquareWidth; ++y)
                {
                    ShowMove(moveSquareCorner.x - x, moveSquareCorner.y - y);
                }
            }
        }

        public void ShowMove(int x, int y)
        {
            GameObject movePreview = Instantiate(lookup.moveTemplate);
            float xPos = Offset(x);
            float yPos = Offset(y);
            movePreview.transform.position = new Vector2(xPos, -yPos);
            moveVisuals.Add(movePreview);
        }

        /// <summary>
        /// Re-draws just the units
        /// </summary>
        /// <param name="toDisplay"></param>
        public void UpdateUnits(Playfield toDisplay)
        {
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

        public void CreateTile(float x, float y, PlayfieldTile data)
        {
            EnsureParentObjectExists();

            Tile template = lookup.GetTileByType(data.tileType).tileTemplate;

            Tile instance = GameObject.Instantiate(template, spawnParent);
            instance.transform.position = new Vector3(x, -y, 0);

            VisualSpawnedObjectTracker tile = new VisualSpawnedObjectTracker(data.tileID);
            tile.spawned = instance.gameObject;

            tileTracking.Add(tile);
        }

        public void CreatUnit(PlayfieldUnit data)
        {
            EnsureParentObjectExists();

            Unit template = lookup.GetUnityByTag(data.tag).unitTemplate;

            for (int i = 0; i < data.locations.Count; ++i)
            {
                Vector2Int curLocation = data.locations[i];
                Unit instance = GameObject.Instantiate(template, spawnParent);

                float x = Offset(curLocation.x);
                float y = Offset(curLocation.y);
                instance.transform.position = new Vector3(x, -y, 0);

                VisualSpawnedObjectTracker unit = new VisualSpawnedObjectTracker(data.id);
                unit.spawned = instance.gameObject;
                unitTracking.Add(unit);
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