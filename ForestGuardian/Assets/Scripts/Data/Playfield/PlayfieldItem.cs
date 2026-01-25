using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class PlayfieldItem
    {
        [JsonProperty] public string tag = null;        // Unique tag for template
        [JsonProperty] public int id = Playfield.NO_ID; // PLAYFIELD-SPECIFIC id.
        [JsonProperty] public Vector2Int location = Vector2Int.zero;

        // Serialized values cloned only.
        public PlayfieldItem Clone(int newID)
        {
            return new PlayfieldItem()
            {
                tag = this.tag,
                id = newID,
                location = this.location,
            };
        }
    }
}
