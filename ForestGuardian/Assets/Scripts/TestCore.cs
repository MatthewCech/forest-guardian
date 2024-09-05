using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class TestCore : MonoBehaviour
    {
        public Playfield playfield;
        public VisualizerPlayfield visualizerPlayfield;
        public Camera mainCam;

        void Start()
        {
            playfield = new Playfield();

            visualizerPlayfield.Display(playfield);

            CenterCam(mainCam, visualizerPlayfield);
        }

        public static void CenterCam(Camera target, VisualizerPlayfield playfield)
        {
            Vector2 targetPos = playfield.GetTileCenter();
            target.transform.position = new Vector3(targetPos.x, targetPos.y, target.transform.position.z);
        }
    }
}