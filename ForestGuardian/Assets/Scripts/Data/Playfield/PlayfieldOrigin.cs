using forest;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    /// <summary>
    /// A location that the player can select a unit to start in
    /// </summary>
    [System.Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class PlayfieldOrigin
    {
        public const int ROSTER_NONE_SELECTED = -1;

        [JsonProperty] public int id = Playfield.NO_ID; // The playfield-unique ID for this.
        [JsonProperty] public Vector2Int location = Vector2Int.zero;

        // Per turn variables
        // Tunable tile data based on lookup information, etc
        public int curRosterIndex = ROSTER_NONE_SELECTED;

        // If we are pre-writing the contents or indicating we're going to potentially skip player choice,
        // we can set this to true. If every origin is flagged true, then we skip the placement phase.
        public bool curSelectionPreComplete = false;
    }
}