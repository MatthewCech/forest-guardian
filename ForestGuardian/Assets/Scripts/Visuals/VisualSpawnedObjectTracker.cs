using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    public class VisualSpawnedObjectTracker
    {
        public GameObject spawned;
        public int DataID { get; private set; }

        public VisualSpawnedObjectTracker(int id)
        {
            DataID = id;
        }
    }
}