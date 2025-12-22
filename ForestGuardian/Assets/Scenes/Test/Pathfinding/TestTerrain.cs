using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    public class CostItem
    {
        public Vector2Int pos;
        [Range(0, 50)] public int cost;
        public bool isWall = false;
        public bool isStart = false;
        public bool isTarget = false;
    }

    public class TestTerrain : MonoBehaviour
    {
        public List<CostItem> terrain;
    }
}
