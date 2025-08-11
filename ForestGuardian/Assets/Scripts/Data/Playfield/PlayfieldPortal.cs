using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    public class PlayfieldPortal
    {
        public int id = Playfield.NO_ID; // Playfield specific id.
        public string target = null;     // Names being targeted for portal location
        public Vector2Int location = Vector2Int.zero;
    }
}
