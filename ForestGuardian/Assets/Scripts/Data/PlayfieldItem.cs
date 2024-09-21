using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    public class PlayfieldItem
    {
        public string tag = null;        // Unique tag for template
        public int id = Playfield.NO_ID; // PLAYFIELD-SPECIFIC id.
        public Vector2Int location = Vector2Int.zero;
    }
}
