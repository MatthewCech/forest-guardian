using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class PlayfieldPortal
    {
        [JsonProperty] public int id = Playfield.NO_ID; // Playfield specific id.
        [JsonProperty] public string target = null;     // Names being targeted for portal location
        [JsonProperty] public Vector2Int location = Vector2Int.zero;

        // Serialized values cloned only.
        public PlayfieldPortal Clone(int newID)
        {
            return new PlayfieldPortal()
            {
                id = newID,
                target = this.target,
                location = this.location,
            };
        }
    }
}
