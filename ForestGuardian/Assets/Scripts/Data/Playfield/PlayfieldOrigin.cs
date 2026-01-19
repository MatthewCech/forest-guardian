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
        public const int NO_INDEX_SELECTED = -1;

        [JsonProperty] public int id = Playfield.NO_ID; // The playfield-unique ID for this.
        [JsonProperty] public Vector2Int location = Vector2Int.zero;
        [JsonProperty] public int partyIndex = NO_INDEX_SELECTED; // Index to draw from automatically for previous units

        // Serialized values cloned only.
        public PlayfieldOrigin Clone(int newID)
        {
            return new PlayfieldOrigin()
            {
                id = newID,
                location = this.location,
                partyIndex = this.partyIndex
            };
        }

        // Per turn variables
        // Tunable tile data based on lookup information, etc
        public int curRosterIndex = NO_INDEX_SELECTED;

        // If we are pre-writing the contents or indicating we're going to potentially skip player choice,
        // we can set this to true. If every origin is flagged true, then we skip the placement phase.
        public bool curSelectionPreComplete = false;
    }
}