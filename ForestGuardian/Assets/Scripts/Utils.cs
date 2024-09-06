using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public static class Utils
    {
        public static void CenterCamera(Camera target, VisualPlayfield playfield)
        {
            Vector2 targetPos = playfield.GetTileCenter();
            target.transform.position = new Vector3(targetPos.x, targetPos.y, target.transform.position.z);
        }
    }
}