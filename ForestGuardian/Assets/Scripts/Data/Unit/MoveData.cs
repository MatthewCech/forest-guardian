using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    public class MoveData
    {
        public string moveName = "UNNAMED";
        public int moveDamage = 1;
        public int moveRange = 1;

        [Space]
        public FXTag moveFX = FXTag.NONE;
        public string moveDescription = "";

        public MoveData Clone()
        {
            MoveData clone = new MoveData();
            clone.moveName = moveName;
            clone.moveDamage = moveDamage;
            clone.moveRange = moveRange;
            clone.moveDescription = moveDescription;

            return clone;
        }
    }
}
