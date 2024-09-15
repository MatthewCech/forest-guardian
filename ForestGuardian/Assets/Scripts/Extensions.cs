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
    }
}
