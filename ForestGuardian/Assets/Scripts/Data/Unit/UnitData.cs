using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    public class UnitData
    {
        public string unitName = "UNNAMED";
        public int maxSize = 1;
        public int speed = 1;
        public int level = 0; // This is effectively a track for the number of modifications made to the base unit experience.

        public List<MoveData> moves;

        public UnitData Clone()
        {
            UnitData clone = new UnitData();
            clone.unitName = unitName;
            clone.maxSize = maxSize;
            clone.speed = speed;
            clone.level = level;

            clone.moves = new List<MoveData>();
            foreach (MoveData move in moves)
            {
                clone.moves.Add(move.Clone());
            }

            return clone;
        }
    }
}