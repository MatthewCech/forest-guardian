using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{
    public class RulesetEditor : MonoBehaviour
    {
        [Header("Links")]
        [SerializeField] private Camera viewingCamera;
        [SerializeField] private VisualPlayfield visualPlayfield;
        [SerializeField] private VisualLookup visualLookup;

        [Header("Settings (General)")]
        [SerializeField][Min(1)] private int sizeX = 1;
        [SerializeField][Min(1)] private int sizeY = 1;

        [Header("Settings (Perlin)")]
        [SerializeField] private bool useRandomSeed = false;
        [SerializeField] private int seed = 123;
        [SerializeField][Min(0.001f)] private float scale = 0.1f;
        [SerializeField][Range(0, 1)] private float thresholdWall = 0.9f;
        [SerializeField][Range(0, 1)] private float thresholdLand = 0.5f;
        [SerializeField][Range(0, 1)] private float thresholdMarsh = 0.4f;

        private Playfield workingPlayfield = null;

        private int prevSizeX = -1;
        private int prevSizeY = -1;
        private bool prevUseRandomSeed = false;
        private int prevSeed = -1;
        private float prevScale = -1;
        private float prevThresholdWall = -1;
        private float prevThresholdLand = -1;
        private float prevThresholdMarsh = -1;

        private void Start()
        {
            prevSizeX = sizeX;
            prevSizeY = sizeY;

            visualPlayfield.Initialize(visualLookup);

            RefreshPlayfield();
            Utils.CenterCamera(viewingCamera, visualPlayfield);
        }

        private void Update()
        {
            if (prevSizeX != sizeX
                || prevSizeY != sizeY
                || prevUseRandomSeed != useRandomSeed
                || prevSeed != seed
                || prevScale != scale
                || prevThresholdWall != thresholdWall
                || prevThresholdLand != thresholdLand
                || prevThresholdMarsh != thresholdMarsh)
            {
                prevSizeX = sizeX;
                prevSizeY = sizeY;
                prevUseRandomSeed = useRandomSeed;
                prevSeed = seed;
                prevScale = scale;
                prevThresholdWall = thresholdWall;
                prevThresholdLand = thresholdLand;
                prevThresholdMarsh = thresholdMarsh;

                RefreshPlayfield();
            }
        }

        class Pair
        {
            public float threshold;
            public string tag;
        }

        private void RefreshPlayfield()
        {
            if (useRandomSeed)
            {
                seed = Random.Range(-999999, 999999);
                prevSeed = seed;
            }

            workingPlayfield = Utils.CreatePlayfield(visualLookup, sizeX, sizeY, workingPlayfield);

            List<Pair> thresholds = new List<Pair>();
            thresholds.Add(new Pair() { threshold = thresholdLand, tag = "Basic" });
            thresholds.Add(new Pair() { threshold = thresholdMarsh, tag = "Marsh" });
            thresholds.Add(new Pair() { threshold = thresholdWall, tag = "Wall" });

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
                        Pair cur = thresholds[i];
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


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(RulesetEditor))]
    public class RulesetEditorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Refresh Playfield"))
            {
                (target as RulesetEditor).RefreshPlayfield();
            }

            base.OnInspectorGUI();
        }
    }
#endif
    }
}