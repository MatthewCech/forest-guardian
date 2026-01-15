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
        private Playfield workingPlayfield = null;

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
        private int prevOriginBorderMin = 1;
        private int prevOriginBorderRange = 2;
        private int prevPortalBorderMin = 1;
        private int prevPortalBorderRange = 2;

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
                workingPlayfield = null;
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

            if (needsRedraw)
            {
                DrawPlayfield();
            }
        }

        private void DrawPlayfield()
        {
            if (useRandomSeed)
            {
                seed = Random.Range(-999999, 999999);
                prevSeed = seed;
            }

            switch(generatorType)
            {
                case GeneratorType.PERLIN_ONLY:
                    CreatePerlinPlayfield();
                    break;
                case GeneratorType.PATH_ONLY:
                    CreatePathPlayfield();
                    break;
            }
        }

        private class PairPerlin
        {
            public float threshold;
            public string tag;
        }

        /// <summary>
        /// A straight perlin noise only approach to generating terrain. Allows different
        /// thresholds to be selected but makes no guarantees of clean pathing.
        /// </summary>
        private void CreatePerlinPlayfield()
        {
            int sizeX = Random.Range(sizeXMin, sizeXMax + 1);
            int sizeY = Random.Range(sizeYMin, sizeYMax + 1);

            workingPlayfield = Utils.CreatePlayfield(visualLookup, sizeX, sizeY, workingPlayfield);

            List<PairPerlin> thresholds = new List<PairPerlin>
            {
                new PairPerlin() { threshold = thresholdLand, tag = "Basic" },
                new PairPerlin() { threshold = thresholdMarsh, tag = "Marsh" },
                new PairPerlin() { threshold = thresholdWall, tag = "Wall" }
            };

            thresholds.Sort((lhs, rhs) => { return lhs.threshold < rhs.threshold ? 1 : -1; });

            for (int x = 0; x < sizeX; ++x)
            {
                for(int y = 0; y < sizeY; ++y)
                {
                    float height01 = Mathf.PerlinNoise(seed + (x * scale), seed + (y * scale));

                    PlayfieldTile tile = new PlayfieldTile();

                    tile.tag = "Nothing";
                    for (int i = 0; i < thresholds.Count; ++i)
                    {
                        PairPerlin cur = thresholds[i];
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

            visualPlayfield.DisplayAll(workingPlayfield);
        }

        private void CreatePathPlayfield()
        {
            int sizeX = Random.Range(sizeXMin, sizeXMax + 1);
            int sizeY = Random.Range(sizeYMin, sizeYMax + 1);

            int minSize = Mathf.Max(originBorderRange * 2, 2);

            // Ensure minimum size
            sizeX = Mathf.Max(sizeX, minSize);
            sizeY = Mathf.Max(sizeY, minSize);

            // Create playfield based on previous
            workingPlayfield = Utils.CreatePlayfield(visualLookup, sizeX, sizeY, null);

            // Establish origin location
            bool isHorizontal = Random.Range(0, 2) > 0;
            bool isPositive = Random.Range(0, 2) > 0;
            int offset = Random.Range(originBorderMin, originBorderMin + originBorderRange);
            int xPos = -1;
            int yPos = -1;
            if (isHorizontal)
            {
                // We're going to select randomly on the X axis
                xPos = Random.Range(originBorderMin, sizeX - originBorderMin);
                yPos = isPositive ? sizeY - offset - 1 : offset; // positive Y side vs negative Y side offset
            }
            else
            {
                yPos = Random.Range(originBorderMin, sizeY - originBorderMin);
                xPos = isPositive ? sizeX - offset - 1 : offset; // positive X side vs negative X side offset
            }

            workingPlayfield.origins.Add(new PlayfieldOrigin
            {
                id = workingPlayfield.GetNextID(),
                location = new Vector2Int(xPos, yPos),
                partyIndex = 0
            });

            // Establish portal location.
            // we match the horizontal or vertical slant, but opposite side.
            bool portalIsHorizontal = isHorizontal;
            bool portalIsPositive = !isPositive;
            int portalOffset = Random.Range(portalBorderMin, portalBorderMin + portalBorderRange);
            int portalXPos = -1;
            int portalYPos = -1;
            if (portalIsHorizontal)
            {
                // We're going to select randomly on the X axis
                portalXPos = Random.Range(portalBorderMin, sizeX - portalBorderMin);
                portalYPos = portalIsPositive ? sizeY - portalOffset - 1 : portalOffset; // positive Y side vs negative Y side offset
            }
            else
            {
                portalYPos = Random.Range(portalBorderMin, sizeY - portalBorderMin);
                portalXPos = portalIsPositive ? sizeX - portalOffset - 1 : portalOffset; // positive X side vs negative X side offset
            }

            workingPlayfield.portals.Add(new PlayfieldPortal
            {
                id = workingPlayfield.GetNextID(),
                location = new Vector2Int(portalXPos, portalYPos),
                target = "next" // PLACEHOLDER NAME FOR NEXT FLOOR I GUESS?
            });

            visualPlayfield.DisplayAll(workingPlayfield);
        }









        private bool TryUpdate(ref GeneratorType prevValue, GeneratorType value)
        {
            if (prevValue != value)
            {
                prevValue = value;
                return true;
            }

            return false;
        }

        private bool TryUpdate(ref int prevValue, int value)
        {
            if (prevValue != value)
            {
                prevValue = value;
                return true;
            }

            return false;
        }

        private bool TryUpdate(ref float prevValue, float value)
        {
            if (prevValue != value)
            {
                prevValue = value;
                return true;
            }

            return false;
        }

        private bool TryUpdate(ref bool prevValue, bool value)
        {
            if (prevValue != value)
            {
                prevValue = value;
                return true;
            }

            return false;
        }

        public void ClearWorkingPlayfield()
        {
            int sizeX = -1;
            int sizeY = -1;

            if (workingPlayfield != null)
            {
                sizeX = workingPlayfield.world.GetWidth();
                sizeY = workingPlayfield.world.GetHeight();
            }
            else
            {
                sizeX = Random.Range(sizeXMin, sizeXMax + 1);
                sizeY = Random.Range(sizeYMin, sizeYMax + 1);
            }

            workingPlayfield = Utils.CreatePlayfield(visualLookup, sizeX, sizeY);
            visualPlayfield.DisplayAll(workingPlayfield);
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
                }

                if(GUILayout.Button("Clear Playfield"))
                {
                    (target as RulesetEditor).ClearWorkingPlayfield();
                }

                base.OnInspectorGUI();
            }
        }
#endif
    }
}