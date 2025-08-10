using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    public class UnitData
    {
        public string unitName = "UNNAMED";
        public int maxSize = 3;
        public int speed = 2;

        public List<AttackData> attacks;
    }
}