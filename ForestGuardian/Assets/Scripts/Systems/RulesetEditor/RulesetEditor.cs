using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{
    public class RulesetEditor : MonoBehaviour
    {
        public enum GeneratorType
        {
            DEFAULT = 0,

            // Single generators
            PERLIN,
            PATH,
            SUBDIVIDE,
            CITIES,

            // Combo generators
            PERLIN_PATH = 10,
            SUBDIVIDE_PATH = 11,
            PATH_STAIN = 12,
            SUBDIVIDE_PATH_STAIN = 13
        }

        [SerializeField] private GeneratorType generatorType = GeneratorType.PERLIN;
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
        [SerializeField] private bool trimPlayfield = false;
        [SerializeField][Min(0)] private int trimPadding = 0;
        private int prevSizeXMin = -1;
        private int prevSizeYMin = -1;
        private int prevSizeXMax = -1;
        private int prevSizeYMax = -1;
        private bool prevUseRandomSeed = false;
        private int prevSeed = -1;
        private Playfield aggregatePlayfield = null;
        private bool prevTrimPlayfield = false;
        private int prevTrimPadding = 0;

        [Header("PERLIN (settings)")]
        [SerializeField][Min(0.001f)] private float scale = 0.1f;
        [SerializeField][Range(0, 1)] private float thresholdWall = 0.9f;
        [SerializeField][Range(0, 1)] private float thresholdLand = 0.5f;
        [SerializeField][Range(0, 1)] private float thresholdMarsh = 0.4f;
        private float prevScale = -1;
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

        [Header("SUBDIVIDE (settings)")]
        [SerializeField][Min(0)] private int roomPadding = 0;
        [SerializeField][Min(1)] private int minRoomSize = 3;
        [SerializeField][Range(0, 0.1f)] private float splitChancePerUnitArea = 0.01f;
        private int prevRoomPadding = 0;
        private int prevMinRoomSize = 3;
        private float prevSplitChancePerUnitArea = 0.01f;

        [Header("STAIN (settings)")]
        [SerializeField][Range(0, 1)] private float stainThreshold = 0.8f;
        private float prevStainThreshold = 0.8f;

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
            needsRedraw |= TryUpdate(ref prevTrimPlayfield, trimPlayfield);
            needsRedraw |= TryUpdate(ref prevTrimPadding, trimPadding);

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

            needsRedraw |= TryUpdate(ref prevRoomPadding, roomPadding);
            needsRedraw |= TryUpdate(ref prevMinRoomSize, minRoomSize);
            needsRedraw |= TryUpdate(ref prevSplitChancePerUnitArea, splitChancePerUnitArea);

            needsRedraw |= TryUpdate(ref prevStainThreshold, stainThreshold);

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
                case GeneratorType.PERLIN:
                    Playfield perlin = CreatePerlinPlayfield(aggregatePlayfield);
                    aggregatePlayfield = TrimPerSettings(perlin);
                    break;

                case GeneratorType.PATH:
                    Playfield path = CreatePathPlayfield();
                    aggregatePlayfield = TrimPerSettings(path);
                    break;

                case GeneratorType.SUBDIVIDE:
                    Playfield subdivided = CreateSubdividePlayfield();
                    aggregatePlayfield = TrimPerSettings(subdivided);
                    break;

                case GeneratorType.CITIES:
                    Playfield cities = CreateCitiesPlayfield();
                    aggregatePlayfield = TrimPerSettings(cities);
                    break;

                case GeneratorType.PERLIN_PATH:
                    ClearWorkingPlayfield();
                    Playfield perlinUnderlay = CreatePerlinPlayfield(aggregatePlayfield);
                    Playfield pathOverlay = CreatePathPlayfield();
                    Playfield layered = Utils.LayerPlayfields(perlinUnderlay, pathOverlay);
                    aggregatePlayfield = TrimPerSettings(layered);
                    break;

                case GeneratorType.SUBDIVIDE_PATH:
                    Playfield subdividedPath = SubdividePath();
                    aggregatePlayfield = TrimPerSettings(subdividedPath);
                    break;

                case GeneratorType.PATH_STAIN:
                    ClearWorkingPlayfield();
                    Playfield toStain = CreatePathPlayfield();
                    Playfield trimmedPath = TrimPerSettings(toStain);
                    Playfield pretrimTopStain = StainPlayfield(trimmedPath, VisualLookup.TILE_GENERIC_MARSH, onTop: true);
                    break;

                case GeneratorType.SUBDIVIDE_PATH_STAIN:
                    ClearWorkingPlayfield();
                    Playfield subdividepath = SubdividePath();
                    Playfield trimmedPreStain = TrimPerSettings(subdividepath);
                    aggregatePlayfield = StainPlayfield(trimmedPreStain, VisualLookup.TILE_GENERIC_WALL, onTop: false);
                    break;
            }

            visualPlayfield.DisplayAll(aggregatePlayfield);
            CenterCamera();
        }

        public Playfield TrimPerSettings(Playfield playfield)
        {
            if(trimPlayfield)
            {
                return Utils.TrimPlayfield(playfield, trimPadding);
            }

            return playfield;
        }

        public Playfield CreateCitiesPlayfield()
        {
            Random.InitState(seed);

            int sizeX = Random.Range(sizeXMin, sizeXMax + 1);
            int sizeY = Random.Range(sizeYMin, sizeYMax + 1);

            Playfield workingPlayfield = Utils.CreatePlayfield(sizeX, sizeY);

            // #TODO

            return workingPlayfield;
        }

        private class PerlinThresholdPair
        {
            public float threshold;
            public string tag;
        }
        public enum SplitStyle
        {
            Horizontal,
            Vertical
        }

        private class SplitNode
        {
            public int left = -1;
            public int right = -1;
            public int top = -1;
            public int bottom = -1;
            public int borderRadius = 0;

            public SplitStyle splitStyle = SplitStyle.Horizontal;

            public SplitNode child1 = null;
            public SplitNode child2 = null;
            public SplitNode parent = null;

            public Vector2Int GetCenter() => new Vector2Int(left + (right - left) / 2, top + (bottom - top) / 2);

            public bool IsLeaf => child1 == null && child2 == null;
            public int Width => Mathf.Abs(right - left);
            public int Height => Mathf.Abs(top - bottom);
        }

        private Playfield CreateSubdividePlayfield()
        {
            Playfield playfield = SubdivideInternal(out SplitNode _, out List<SplitNode> _);
            return playfield;
        }

        private class PairLineDraw
        {
            public Vector2Int start;
            public Vector2Int end;
        }

        private Playfield SubdividePath()
        {
            Playfield subdivides = SubdivideInternal(out SplitNode root, out List<SplitNode> rooms);
            Playfield paths = Utils.CreatePlayfield(subdivides.world.GetWidth(), subdivides.world.GetHeight());

            List<PairLineDraw> toDraw = new List<PairLineDraw>();

            for(int i = 1; i < rooms.Count; ++i)
            {
                PairLineDraw line = new PairLineDraw();
                line.start = rooms[i - 1].GetCenter();
                line.end = rooms[i].GetCenter();
                toDraw.Add(line);
            }

            for (int i = 0; i < toDraw.Count; ++i)
            {
                PairLineDraw pair = toDraw[i];
                CreatePathToTarget(paths, pair.start, pair.end);
            }

            for (int outlines = 0; outlines < outlineLayers; ++outlines)
            {
                AddTileOutline(paths, VisualLookup.TILE_GENERIC_GROUND, VisualLookup.TILE_GENERIC_MARSH);
            }

            Playfield combined = Utils.LayerPlayfields(paths, subdivides);

            SplitNode portalRoom = rooms[rooms.Count - 1];
            PlayfieldPortal portal = new PlayfieldPortal();
            portal.id = combined.GetNextID();
            int portalRelX = Random.Range(roomPadding, portalRoom.Width - roomPadding);
            int portalRelY = Random.Range(roomPadding, portalRoom.Height - roomPadding);
            portal.location = new Vector2Int(portalRoom.left + portalRelX, portalRoom.top + portalRelY);
            combined.portals.Add(portal);
            
            bool placing = true;
            int tries = 20;
            SplitNode originRoom = rooms[0];
            while (placing && tries-- > 0)
            {
                int originRelX = Random.Range(roomPadding, originRoom.Width - roomPadding);
                int originRelY = Random.Range(roomPadding, originRoom.Height - roomPadding);
                Vector2Int pos = new Vector2Int(originRoom.left + originRelX, originRoom.top + originRelY);
                if (!combined.TryGetPortalAt(pos, out PlayfieldPortal _))
                {
                    PlayfieldOrigin origin = new PlayfieldOrigin();
                    origin.id = combined.GetNextID();
                    origin.location = pos;
                    combined.origins.Add(origin);
                    placing = false;
                }
            }

            return combined;
        }

        private Playfield SubdivideInternal(out SplitNode root, out List<SplitNode> rooms)
        {
            Random.InitState(seed);

            int sizeX = Random.Range(sizeXMin, sizeXMax + 1);
            int sizeY = Random.Range(sizeYMin, sizeYMax + 1);

            Playfield workingPlayfield = Utils.CreatePlayfield(sizeX, sizeY);

            root = new SplitNode();
            root.left = 0;
            root.right = sizeX;
            root.top = 0;
            root.bottom = sizeY;
            root.borderRadius = roomPadding;

            Stack<SplitNode> toSplit = new Stack<SplitNode>();
            toSplit.Push(root);

            while (toSplit.Count > 0)
            {
                SplitNode node = toSplit.Pop();

                int splitPadding = Mathf.CeilToInt(minRoomSize / 2.0f);
                if(node.Width < splitPadding * 2 || node.Height < splitPadding * 2)
                {
                    continue;
                }

                float splitRoll = Random.Range(0f, 1f);
                float chance = node.Width * node.Height * splitChancePerUnitArea;
                if (chance < splitRoll) // roll failed
                {
                    continue;
                }
                
                node.splitStyle = Random.Range(0, 2) > 0 ? SplitStyle.Horizontal : SplitStyle.Vertical;
                if(node.parent != null)
                {
                    if(node.parent.splitStyle == SplitStyle.Horizontal)
                    {
                        node.splitStyle = SplitStyle.Vertical;
                    }
                    else
                    {
                        node.splitStyle = SplitStyle.Horizontal;
                    }
                }

                if (node.splitStyle == SplitStyle.Horizontal)
                {
                    int horizontalSplitPos = Random.Range(node.left + splitPadding, node.right - splitPadding + 1);

                    SplitNode newLeft = new SplitNode();
                    newLeft.left = node.left;
                    newLeft.right = horizontalSplitPos;
                    newLeft.top = node.top;
                    newLeft.bottom = node.bottom;
                    newLeft.borderRadius = roomPadding;
                    newLeft.parent = node;

                    node.child1 = newLeft;
                    toSplit.Push(newLeft);

                    SplitNode newRight = new SplitNode();
                    newRight.left = horizontalSplitPos;
                    newRight.right = node.right;
                    newRight.top = node.top;
                    newRight.bottom = node.bottom;
                    newRight.borderRadius = roomPadding;
                    newRight.parent = node;

                    node.child2 = newRight;
                    toSplit.Push(newRight);
                }
                else
                {
                    int verticalSplitPos = Random.Range(node.top + splitPadding, node.bottom - splitPadding + 1);

                    SplitNode newTop = new SplitNode();
                    newTop.left = node.left;
                    newTop.right = node.right;
                    newTop.top = node.top;
                    newTop.bottom = verticalSplitPos;
                    newTop.borderRadius = roomPadding;
                    newTop.parent = node;

                    node.child1 = newTop;
                    toSplit.Push(newTop);

                    SplitNode newBottom = new SplitNode();
                    newBottom.left = node.left;
                    newBottom.right = node.right;
                    newBottom.top = verticalSplitPos;
                    newBottom.bottom = node.bottom;
                    newBottom.borderRadius = roomPadding;
                    newBottom.parent = node;

                    node.child2 = newBottom;
                    toSplit.Push(newBottom);
                }
            }

            rooms = new List<SplitNode>();
            Stack<SplitNode> walk = new Stack<SplitNode>();
            walk.Push(root);
            while (walk.Count > 0)
            {
                SplitNode cur = walk.Pop();
                if (cur.IsLeaf)
                {
                    if (cur.Width >= minRoomSize && cur.Height >= minRoomSize)
                    {
                        rooms.Add(cur);
                    }
                }
                else
                {
                    if (cur.child1 != null)
                    {
                        walk.Push(cur.child1);
                    }

                    if(cur.child2 != null)
                    {
                        walk.Push(cur.child2);
                    }
                }
            }

            foreach(SplitNode cur in rooms)
            {
                for(int x = cur.left + cur.borderRadius; x < cur.right - cur.borderRadius; ++x)
                {
                    for(int y = cur.top + cur.borderRadius; y < cur.bottom - cur.borderRadius; ++y)
                    {
                        workingPlayfield.world.Set(x, y, new PlayfieldTile()
                        {
                            id = workingPlayfield.GetNextID(),
                            tag = VisualLookup.TILE_GENERIC_GROUND
                        });
                    }
                }
            }

            return workingPlayfield;
        }

        private Playfield StainPlayfield(Playfield toStain, string stainTag, bool onTop)
        {
            Random.InitState(seed);

            int width = toStain.Width();
            int height = toStain.Height();

            Playfield stain = Utils.CreatePlayfield(toStain.Width(), toStain.Height());
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    float height01 = Mathf.PerlinNoise(seed + (x * scale), seed + (y * scale));

                    if(height01 > stainThreshold)
                    {
                        PlayfieldTile tile = new PlayfieldTile();
                        tile.id = stain.GetNextID();
                        tile.tag = stainTag;

                        stain.world.Set(x, y, tile);
                    }
                }
            }

            if(onTop)
            {
                return Utils.LayerPlayfields(toStain, stain);
            }
            else
            {
                return Utils.LayerPlayfields(stain, toStain);
            }
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

            Playfield workingPlayfield = Utils.CreatePlayfield(sizeX, sizeY, existingPlayfield);

            List<PerlinThresholdPair> thresholds = new List<PerlinThresholdPair>
            {
                new PerlinThresholdPair() { threshold = thresholdLand, tag = VisualLookup.TILE_GENERIC_GROUND },
                new PerlinThresholdPair() { threshold = thresholdMarsh, tag = VisualLookup.TILE_GENERIC_MARSH },
                new PerlinThresholdPair() { threshold = thresholdWall, tag = VisualLookup.TILE_GENERIC_WALL }
            };

            thresholds.Sort((lhs, rhs) => { return lhs.threshold < rhs.threshold ? 1 : -1; });

            for (int x = 0; x < sizeX; ++x)
            {
                for(int y = 0; y < sizeY; ++y)
                {
                    float height01 = Mathf.PerlinNoise(seed + (x * scale), seed + (y * scale));

                    PlayfieldTile tile = new PlayfieldTile();

                    tile.tag = VisualLookup.TILE_DEFAULT_NAME;
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
        private Playfield CreatePathPlayfield(Playfield existingPlayfield = null)
        {
            Random.InitState(seed);

            // Ensure minimum size
            int sizeX = Random.Range(sizeXMin, sizeXMax + 1);
            int sizeY = Random.Range(sizeYMin, sizeYMax + 1);

            int minSize = Mathf.Max(originBorderRange * 2, 2);
            sizeX = Mathf.Max(sizeX, minSize);
            sizeY = Mathf.Max(sizeY, minSize);

            // Create playfield based on previous
            Playfield workingPlayfield = Utils.CreatePlayfield(sizeX, sizeY, existingPlayfield);

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
                target = Globals.NEXT_GENERATOR_FLOOR_KEY
            });

            // Draw wibbly core path by stepping from the origin to the exit portal
            CreatePathToTarget(workingPlayfield, originPos, portalPos);

            // Perform outline passes
            for (int outlines = 0; outlines < outlineLayers; ++outlines)
            {
                AddTileOutline(workingPlayfield, VisualLookup.TILE_GENERIC_GROUND, VisualLookup.TILE_GENERIC_GROUND);
            }

            return workingPlayfield;
        }

        /// <summary>
        /// Given a specific type of tyle, goes and adds an outline at the expense of any other tile
        /// at the target location. Does so greedily, inefficiently, and in a way that burns a lot of IDs.
        /// </summary>
        private static void AddTileOutline(Playfield workingPlayfield, string targetTag, string outlineTag, bool writeOverExisting = false)
        {
            int sizeX = workingPlayfield.world.GetWidth();
            int sizeY = workingPlayfield.world.GetHeight();

            List<Vector2Int> toAdd = new List<Vector2Int>();

            void AddToEmpty(Vector2Int pos)
            {
                if (!writeOverExisting)
                {
                    if (workingPlayfield.world.IsPosInGrid(pos))
                    {
                        PlayfieldTile tile = workingPlayfield.world.Get(pos);

                        // Tile must be default to apply outline
                        if (tile.tag.Equals(VisualLookup.TILE_DEFAULT_NAME))
                        {
                            toAdd.Add(pos);
                        }
                    }
                }
                else
                {
                    toAdd.Add(pos);
                }
            }

            for (int x = 0; x < workingPlayfield.world.GetWidth(); ++x)
            {
                for (int y = 0; y < workingPlayfield.world.GetHeight(); ++y)
                {
                    PlayfieldTile existingTile = workingPlayfield.world.Get(x, y);
                    if (existingTile.tag.Equals(targetTag))
                    {
                        AddToEmpty(new Vector2Int(x + 1, y));
                        AddToEmpty(new Vector2Int(x - 1, y));
                        AddToEmpty(new Vector2Int(x, y + 1));
                        AddToEmpty(new Vector2Int(x, y - 1));
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
                tag = VisualLookup.TILE_GENERIC_GROUND
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
                    tag = VisualLookup.TILE_GENERIC_GROUND
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
                Random.InitState(seed);
                sizeX = Random.Range(sizeXMin, sizeXMax + 1);
                sizeY = Random.Range(sizeYMin, sizeYMax + 1);
            }

            aggregatePlayfield = Utils.CreatePlayfield(sizeX, sizeY);
            visualPlayfield.DisplayAll(aggregatePlayfield);
        }

        public void CenterCamera()
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
                    (target as RulesetEditor).CenterCamera();
                }

                if (GUILayout.Button("Refresh Playfield New Seed"))
                {
                    (target as RulesetEditor).NewRandSeed();
                    (target as RulesetEditor).DrawPlayfield();
                    (target as RulesetEditor).CenterCamera();
                }

                base.OnInspectorGUI();
            }
        }
#endif
    }
}