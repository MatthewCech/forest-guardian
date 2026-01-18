using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{
    public class RulesetEditor : MonoBehaviour
    {
        private const string NEXT_GENERATOR_FLOOR_KEY = "Next";

        private const string TILE_NO_TILE = "Nothing";
        private const string TILE_GENERIC_GROUND = "Basic";
        private const string TILE_GENERIC_MARSH = "Marsh";
        private const string TILE_GENERIC_WALL = "Wall";

        public enum GeneratorType
        {
            DEFAULT = 0,

            PERLIN_ONLY,
            PATH_ONLY
        }

        [SerializeField] private GeneratorType generatorType = GeneratorType.PERLIN_ONLY;
        [SerializeField] private bool repeatedlyRefresh = false;
        private GeneratorType prevGeneratorType = GeneratorType.DEFAULT;

        [Header("Links")]
        [SerializeField] private Camera viewingCamera;
        [SerializeField] private VisualPlayfield visualPlayfield;
        [SerializeField] private VisualLookup visualLookup;

        [Header("General")]
        [SerializeField][Min(1)] private int sizeXMin = 10;
        [SerializeField][Min(1)] private int sizeXMax = 20;
        [SerializeField][Min(1)] private int sizeYMin = 10;
        [SerializeField][Min(1)] private int sizeYMax = 20;
        [SerializeField] private bool useRandomSeed = false;
        [SerializeField] private int seed = 123;
        [SerializeField][Min(0.001f)] private float scale = 0.1f;
        private int prevSizeXMin = -1;
        private int prevSizeYMin = -1;
        private int prevSizeXMax = -1;
        private int prevSizeYMax = -1;
        private bool prevUseRandomSeed = false;
        private int prevSeed = -1;
        private float prevScale = -1;
        private Playfield aggregatePlayfield = null;

        [Header("PERLIN (settings)")]
        [SerializeField][Range(0, 1)] private float thresholdWall = 0.9f;
        [SerializeField][Range(0, 1)] private float thresholdLand = 0.5f;
        [SerializeField][Range(0, 1)] private float thresholdMarsh = 0.4f;
        private float prevThresholdWall = -1;
        private float prevThresholdLand = -1;
        private float prevThresholdMarsh = -1;

        [Header("PATH (settings)")]
        [SerializeField][Range(0, 25)] private int originBorderMin = 1;
        [SerializeField][Range(0, 25)] private int originBorderRange = 2;
        [SerializeField][Range(0, 25)] private int portalBorderMin = 1;
        [SerializeField][Range(0, 25)] private int portalBorderRange = 2;
        [SerializeField][Range(0, 1)] private float pathNoise = 0.1f;
        [SerializeField][Min(0)] private int noiseWatchdog = 50;
        [SerializeField][Range(0, 10)] private int outlineLayers = 0;
        private int prevOriginBorderMin = 1;
        private int prevOriginBorderRange = 2;
        private int prevPortalBorderMin = 1;
        private int prevPortalBorderRange = 2;
        private float prevPathNoise = 0.1f;
        private int prevOutlineLayers = 0;
        private int prevNoiseWatchdog = 50;

        private void Start()
        {
            prevSizeXMin = sizeXMin;
            prevSizeYMin = sizeYMin;
            prevSizeXMax = sizeXMax;
            prevSizeYMax = sizeYMax;

            visualPlayfield.Initialize(visualLookup);

            DrawPlayfield();
            Utils.CenterCamera(viewingCamera, visualPlayfield);
        }

        private void Update()
        {
            bool needsRedraw = repeatedlyRefresh;
            if(TryUpdate(ref prevGeneratorType, generatorType))
            {
                // If generator type changes, always clear out existing playfield.
                aggregatePlayfield = null;
                needsRedraw = true;
            }

            needsRedraw |= TryUpdate(ref prevSizeXMin, sizeXMin);
            needsRedraw |= TryUpdate(ref prevSizeYMin, sizeYMin);
            needsRedraw |= TryUpdate(ref prevSizeXMax, sizeXMax);
            needsRedraw |= TryUpdate(ref prevSizeYMax, sizeYMax);
            needsRedraw |= TryUpdate(ref prevUseRandomSeed, useRandomSeed);
            needsRedraw |= TryUpdate(ref prevSeed, seed);
            needsRedraw |= TryUpdate(ref prevScale, scale);

            needsRedraw |= TryUpdate(ref prevThresholdWall, thresholdWall);
            needsRedraw |= TryUpdate(ref prevThresholdLand, thresholdLand);
            needsRedraw |= TryUpdate(ref prevThresholdMarsh, thresholdMarsh);

            needsRedraw |= TryUpdate(ref prevOriginBorderMin, originBorderMin);
            needsRedraw |= TryUpdate(ref prevOriginBorderRange, originBorderRange);
            needsRedraw |= TryUpdate(ref prevPortalBorderMin, portalBorderMin);
            needsRedraw |= TryUpdate(ref prevPortalBorderRange, portalBorderRange);
            needsRedraw |= TryUpdate(ref prevPathNoise, pathNoise);
            needsRedraw |= TryUpdate(ref prevNoiseWatchdog, noiseWatchdog);
            needsRedraw |= TryUpdate(ref prevOutlineLayers, outlineLayers);

            if (needsRedraw)
            {
                DrawPlayfield();
            }
        }

        private void DrawPlayfield()
        {
            if (useRandomSeed)
            {
                NewRandSeed();
            }

            switch(generatorType)
            {
                case GeneratorType.PERLIN_ONLY:
                    aggregatePlayfield = CreatePerlinPlayfield(aggregatePlayfield);
                    break;
                case GeneratorType.PATH_ONLY:
                    aggregatePlayfield = CreatePathPlayfield();
                    break;
            }

            visualPlayfield.DisplayAll(aggregatePlayfield);
        }

        private class PerlinThresholdPair
        {
            public float threshold;
            public string tag;
        }

        /// <summary>
        /// A straight perlin noise only approach to generating terrain. Allows different
        /// thresholds to be selected but makes no guarantees of clean pathing.
        /// </summary>
        private Playfield CreatePerlinPlayfield(Playfield existingPlayfield = null)
        {
            Random.InitState(seed);

            int sizeX = Random.Range(sizeXMin, sizeXMax + 1);
            int sizeY = Random.Range(sizeYMin, sizeYMax + 1);

            Playfield workingPlayfield = Utils.CreatePlayfield(visualLookup, sizeX, sizeY, existingPlayfield);

            List<PerlinThresholdPair> thresholds = new List<PerlinThresholdPair>
            {
                new PerlinThresholdPair() { threshold = thresholdLand, tag = TILE_GENERIC_GROUND },
                new PerlinThresholdPair() { threshold = thresholdMarsh, tag = TILE_GENERIC_MARSH },
                new PerlinThresholdPair() { threshold = thresholdWall, tag = TILE_GENERIC_WALL }
            };

            thresholds.Sort((lhs, rhs) => { return lhs.threshold < rhs.threshold ? 1 : -1; });

            for (int x = 0; x < sizeX; ++x)
            {
                for(int y = 0; y < sizeY; ++y)
                {
                    float height01 = Mathf.PerlinNoise(seed + (x * scale), seed + (y * scale));

                    PlayfieldTile tile = new PlayfieldTile();

                    tile.tag = TILE_NO_TILE;
                    for (int i = 0; i < thresholds.Count; ++i)
                    {
                        PerlinThresholdPair cur = thresholds[i];
                        if (height01 > cur.threshold)
                        {
                            tile.tag = cur.tag;
                            break;
                        }
                    }    

                    tile.id = workingPlayfield.GetNextID();

                    workingPlayfield.world.Set(x, y, tile);
                }
            }

            return workingPlayfield;
        }

        /// <summary>
        /// A wanderer draws a path between a start and an end that get placed in specific areas.
        /// Noise is added at various points, but creates a wibbly line of an experience.
        /// </summary>
        private Playfield CreatePathPlayfield()
        {
            Random.InitState(seed);

            // Ensure minimum size
            int sizeX = Random.Range(sizeXMin, sizeXMax + 1);
            int sizeY = Random.Range(sizeYMin, sizeYMax + 1);

            int minSize = Mathf.Max(originBorderRange * 2, 2);
            sizeX = Mathf.Max(sizeX, minSize);
            sizeY = Mathf.Max(sizeY, minSize);

            // Create playfield based on previous
            Playfield workingPlayfield = Utils.CreatePlayfield(visualLookup, sizeX, sizeY, null);

            // Establish origin location
            bool originIsHorizontal = Random.Range(0, 2) > 0;
            bool originIsPositive = Random.Range(0, 2) > 0;
            int originOffset = Random.Range(originBorderMin, originBorderMin + originBorderRange);
            int portalOffset = Random.Range(portalBorderMin, portalBorderMin + portalBorderRange);
            Vector2Int originPos = new Vector2Int(-1, -1);
            Vector2Int portalPos = new Vector2Int(-1, -1);

            if (originIsHorizontal) 
            {
                // Randomly place along the X axis
                originPos.x = Random.Range(originBorderMin, sizeX - originBorderMin);
                portalPos.x = Random.Range(portalBorderMin, sizeX - portalBorderMin);

                // Place origin and portal on opposite top/bottom sides
                originPos.y = originIsPositive ? sizeY - originOffset - 1 : originOffset;
                portalPos.y = !originIsPositive ? sizeY - portalOffset - 1 : portalOffset; 
            }
            else
            {
                // Randomly place along the Y axis
                originPos.y = Random.Range(originBorderMin, sizeY - originBorderMin);
                portalPos.y = Random.Range(portalBorderMin, sizeY - portalBorderMin);

                // Place origin and portal on opposite left/right sides
                portalPos.x = !originIsPositive ? sizeX - portalOffset - 1 : portalOffset;
                originPos.x = originIsPositive ? sizeX - originOffset - 1 : originOffset; 
            }

            workingPlayfield.origins.Add(new PlayfieldOrigin
            {
                id = workingPlayfield.GetNextID(),
                location = originPos,
                partyIndex = 0
            });

            workingPlayfield.portals.Add(new PlayfieldPortal
            {
                id = workingPlayfield.GetNextID(),
                location = portalPos,
                target = NEXT_GENERATOR_FLOOR_KEY
            });

            // Draw wibbly core path by stepping from the origin to the exit portal
            CreatePathToTarget(workingPlayfield, originPos, portalPos);

            // Perform outline passes
            for (int outlines = 0; outlines < outlineLayers; ++outlines)
            {
                AddTileOutline(workingPlayfield, TILE_GENERIC_GROUND, TILE_GENERIC_GROUND);
            }

            return workingPlayfield;
        }

        /// <summary>
        /// Given a specific type of tyle, goes and adds an outline at the expense of any other tile
        /// at the target location. Does so greedily, inefficiently, and in a way that burns a lot of IDs.
        /// </summary>
        private static void AddTileOutline(Playfield workingPlayfield, string targetTag, string outlineTag)
        {
            int sizeX = workingPlayfield.world.GetWidth();
            int sizeY = workingPlayfield.world.GetHeight();

            List<Vector2Int> toAdd = new List<Vector2Int>();
            for (int x = 0; x < workingPlayfield.world.GetWidth(); ++x)
            {
                for (int y = 0; y < workingPlayfield.world.GetHeight(); ++y)
                {
                    PlayfieldTile existingTile = workingPlayfield.world.Get(x, y);
                    if (existingTile.tag.Equals(targetTag))
                    {
                        toAdd.Add(new Vector2Int(x + 1, y));
                        toAdd.Add(new Vector2Int(x - 1, y));
                        toAdd.Add(new Vector2Int(x, y + 1));
                        toAdd.Add(new Vector2Int(x, y - 1));
                    }
                }
            }

            for (int i = 0; i < toAdd.Count; ++i)
            {
                Vector2Int cur = toAdd[i];

                int x = Mathf.Clamp(cur.x, 0, sizeX - 1);
                int y = Mathf.Clamp(cur.y, 0, sizeY - 1);

                workingPlayfield.world.Set(new Vector2Int(x, y), new PlayfieldTile()
                {
                    id = workingPlayfield.GetNextID(),
                    tag = outlineTag
                });
            }
        }

        /// <summary>
        /// Create a wanderer that goes from the start to the target, taking X or Y steps based
        /// on how many in that direction are needed still. Eg: 4 to the right, 2 up gives a 4/6
        /// chance to move on the X axis.
        /// 
        /// This is modified a noise roll that overrides this if the roll is made, selecting a random
        /// direction. To prevent runaway noisy wandering, a watchdog is honored and if the number of 
        /// noise steps exceeds the watchdog's allowed steps, walk conventionally.
        /// </summary>
        /// <param name="workingPlayfield"></param>
        /// <param name="start"></param>
        /// <param name="target"></param>
        private void CreatePathToTarget(Playfield workingPlayfield, Vector2Int start, Vector2Int target)
        {
            int sizeX = workingPlayfield.world.GetWidth();
            int sizeY = workingPlayfield.world.GetHeight();

            int xCurrent = start.x;
            int yCurrent = start.y;

            int watchdog = noiseWatchdog;
            bool didWatchdogNotify = false;

            workingPlayfield.world.Set(new Vector2Int(xCurrent, yCurrent), new PlayfieldTile()
            {
                id = workingPlayfield.GetNextID(),
                tag = TILE_GENERIC_GROUND
            });

            while (xCurrent != target.x || yCurrent != target.y)
            {
                float xDiff = Mathf.Abs(target.x - xCurrent);
                float yDiff = Mathf.Abs(target.y - yCurrent);

                float moveOnX = xDiff / (xDiff + yDiff);
                float rollXY = Random.Range(0.0f, 1.0f);

                float rollNoise = Random.Range(0.0f, 1.0f);
                bool preferNoise = pathNoise > rollNoise;
                if (watchdog-- < 0)
                {
                    if (!didWatchdogNotify)
                    {
                        Debug.LogWarning($"Noise watchdog(${noiseWatchdog}) hit. Est steps from target min:{xDiff + yDiff}, est: {(xDiff + yDiff) / (1 - pathNoise)}");
                        didWatchdogNotify = true;
                    }

                    preferNoise = false;
                }

                if (preferNoise)
                {
                    bool doX = Random.Range(0, 2) > 0;

                    if (doX)
                    {
                        xCurrent += Random.Range(-1, 2);
                    }
                    else
                    {
                        yCurrent += Random.Range(-1, 2);
                    }
                }
                else
                {
                    if (rollXY < moveOnX)
                    {
                        xCurrent += target.x < xCurrent ? -1 : 1;
                    }
                    else
                    {
                        yCurrent += target.y < yCurrent ? -1 : 1;
                    }
                }

                xCurrent = Mathf.Clamp(xCurrent, 0, sizeX - 1);
                yCurrent = Mathf.Clamp(yCurrent, 0, sizeY - 1);

                workingPlayfield.world.Set(new Vector2Int(xCurrent, yCurrent), new PlayfieldTile()
                {
                    id = workingPlayfield.GetNextID(),
                    tag = TILE_GENERIC_GROUND
                });
            }
        }

        public void ClearWorkingPlayfield()
        {
            int sizeX = -1;
            int sizeY = -1;

            if (aggregatePlayfield != null)
            {
                sizeX = aggregatePlayfield.world.GetWidth();
                sizeY = aggregatePlayfield.world.GetHeight();
            }
            else
            {
                sizeX = Random.Range(sizeXMin, sizeXMax + 1);
                sizeY = Random.Range(sizeYMin, sizeYMax + 1);
            }

            aggregatePlayfield = Utils.CreatePlayfield(visualLookup, sizeX, sizeY);
            visualPlayfield.DisplayAll(aggregatePlayfield);
        }

        public void CameraCenter()
        {
            Utils.CenterCamera(viewingCamera, visualPlayfield);
        }

        private void NewRandSeed()
        {
            seed = Random.Range(-999999, 999999);
            prevSeed = seed;
        }

        private bool TryUpdate<T>(ref T prevValue, T value)
        {
            if (!prevValue.Equals(value))
            {
                prevValue = value;
                return true;
            }

            return false;
        }


#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(RulesetEditor))]
        public class RulesetEditorEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                if (GUILayout.Button("Refresh Playfield"))
                {
                    (target as RulesetEditor).DrawPlayfield();
                    (target as RulesetEditor).CameraCenter();
                }

                if (GUILayout.Button("Refresh Playfield New Seed"))
                {
                    (target as RulesetEditor).NewRandSeed();
                    (target as RulesetEditor).DrawPlayfield();
                    (target as RulesetEditor).CameraCenter();
                }

                if (GUILayout.Button("Clear Playfield"))
                {
                    (target as RulesetEditor).ClearWorkingPlayfield();
                    (target as RulesetEditor).CameraCenter();
                }

                base.OnInspectorGUI();
            }
        }
#endif
    }
}