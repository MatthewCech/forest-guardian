using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class VisualizerPlayfield : MonoBehaviour
    {
        [System.Serializable]
        public class PairItem
        {
            public GameObject tileTemplate;
            public TileType tileType;
        }

        [System.Serializable]
        public class TileInstance
        {
            public GameObject spawned;
            public ulong dataID;
        }

        // External
        [SerializeField] private List<PairItem> tiles;

        // Internal tracking
        private List<TileInstance> tracking;

        public VisualizerPlayfield()
        {
            tracking = new List<TileInstance>();
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

            for (int i = 0; i < tracking.Count; i++)
            {
                TileInstance cur = tracking[i];
                float xCur = cur.spawned.transform.position.x;
                float yCur = cur.spawned.transform.position.y;

                xMin = Mathf.Min(xCur, xMin);
                yMin = Mathf.Min(yCur, yMin);
                xMax = Mathf.Max(xCur, xMax);
                yMax = Mathf.Max(yCur, yMax);
            }

            return new Vector2(xMin + (xMax - xMin) / 2, yMin + (yMax - yMin) / 2);
        }

        public void Display(Playfield toDisplay)
        {
            int width = toDisplay.world.GetWidth();
            int height = toDisplay.world.GetHeight();

            for (int x = 0; x < width; ++x)
            {
                for(int y = 0; y < height; ++y)
                {
                    CreateTile(x, y, toDisplay.world.Get(x, y));
                }
            }
        }

        public PairItem GetTileType(TileType type)
        {
            for(int i = 0; i < tiles.Count; ++i)
            {
                PairItem cur = tiles[i];
                if (cur.tileType == type)
                {
                    return cur;
                }
            }

            return null;
        }

        public void CreateTile(int x, int y, Tile data)
        {
            GameObject instance = GameObject.Instantiate(GetTileType(data.tileType).tileTemplate);
            instance.transform.position = new Vector3(x, -y, 0);

            TileInstance tile = new TileInstance();
            tile.spawned = instance;
            tile.dataID = data.id;

            tracking.Add(tile);
        }
    }
}