using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class PlayfieldTile
    {
        [JsonProperty] public string tag;
        [JsonProperty] public int id = Playfield.NO_ID; // The playfield-unique ID for this. 0 means no ID has been assigned.

        // Tunable tile data based on lookup information, etc
        public bool curIsImpassable = false; // Can you even more though this tile?
        public int curMoveDifficulty = 1;    // How expensive it is to travel through this tile - ignored if impassable is true.
    }
}