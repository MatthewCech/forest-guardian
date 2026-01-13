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
        [Header("Settings (live update)")]
        [SerializeField][Min(1)] private int sizeX = 1;
        [SerializeField][Min(1)] private int sizeY = 1;
        [SerializeField] private int seed = 123;
        [SerializeField][Min(0.001f)] private float scale = 0.1f;
        [SerializeField] private float thresholdLand = 0.5f;
        [SerializeField] private float thresholdMarsh = 0.4f;



        private Playfield workingPlayfield = null;

        private int prevSizeX = -1;
        private int prevSizeY = -1;
        private int prevSeed = -1;
        private float prevScale = -1;
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
                || prevSeed != seed
                || prevScale != scale
                || prevThresholdLand != thresholdLand
                || prevThresholdMarsh != thresholdMarsh)
            {
                prevSizeX = sizeX;
                prevSizeY = sizeY;
                prevSeed = seed;
                prevScale = scale;
                prevThresholdLand = thresholdLand;
                prevThresholdMarsh = thresholdMarsh;

                RefreshPlayfield();
            }
        }

        private void RefreshPlayfield()
        {
            workingPlayfield = Utils.CreatePlayfield(visualLookup, sizeX, sizeY, workingPlayfield);

            for(int x = 0; x < sizeX; ++x)
            {
                for(int y = 0; y < sizeY; ++y)
                {
                    float height01 = Mathf.PerlinNoise(seed + (x * scale), seed + (y * scale));

                    PlayfieldTile tile = new PlayfieldTile();

                    if (height01 > thresholdLand)
                    {
                        tile.tag = "Basic";
                    }
                    else if (height01 > thresholdMarsh)
                    {
                        tile.tag = "Marsh";
                    }
                    else
                    {
                        tile.tag = "Nothing";
                    }

                    tile.id = workingPlayfield.GetNextID();

                    workingPlayfield.world.Set(x, y, tile);
                }
            }

            visualPlayfield.DisplayAll(workingPlayfield);
        }
    }
}