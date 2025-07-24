using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    /// <summary>
    /// A literal exit point, taking the player to the map regardless (or rather, to the end location)
    /// </summary>
    [System.Serializable]
    public class PlayfieldExit
    {
        public string tag = null;
        public int id = Playfield.NO_ID; // PLAYFIELD-SPECIFIC id.
        public Vector2Int location = Vector2Int.zero;
    }
}