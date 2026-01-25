using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public static class Extensions 
    {
        public static Vector2Int Tail(this List<Vector2Int> @this)
        {
            return @this[@this.Count - 1];
        }

        public static int GridDistance(this Vector2Int @this, Vector2Int other)
        {
            return Mathf.Abs(@this.x - other.x) + Mathf.Abs(@this.y - other.y);
        }

        /// <summary>
        /// Random int within a range
        /// </summary>
        /// <param name="this"></param>
        /// <param name="min">Inclusive</param>
        /// <param name="max">Exclusive</param>
        /// <returns></returns>
        public static int Range(this WHRandom @this, int min, int max)
        {
            float val = (float)@this.Next();
            int range = max - min;
            return min + Mathf.FloorToInt(val * range);
        }
    }
}
