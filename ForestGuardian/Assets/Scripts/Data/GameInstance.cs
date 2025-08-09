using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    /// <summary>
    /// Data-only class. Filled with defaults.
    /// </summary>
    [System.Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class GameInstance
    {
        // Between saves
        [JsonProperty] public List<string> completedLevelTags = new List<string>() { "test", "tutorial 1"};
        [JsonProperty] public int currency = 0;

        // Runtime only
        [System.NonSerialized] public TextAsset currentPlayfield = null;
    }
}