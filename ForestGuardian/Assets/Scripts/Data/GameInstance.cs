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
        // Defines
        [System.NonSerialized] public const string PLAYER_UNIT_DEFAULT = "Guardian";

        // Data
        [JsonProperty] public int currency = 0;
        [JsonProperty] public List<string> completedLevelTags = new List<string>();
        [JsonProperty] public List<UnitData> roster = new List<UnitData>();

        // Runtime Data Only
        [System.NonSerialized] public TextAsset currentPlayfield = null;

        /// <summary>
        /// Provides defaults. Arguments and construction can and should be reworked 
        /// </summary>
        public void PopulateDefaults(VisualLookup lookup)
        {
            currency = 0;

            completedLevelTags = new List<string>()
            {
                "test",
                "tutorial 1"
            };

            UnitData guardian = lookup.GetUnityByTag("Guardian").data;
            if (guardian == null)
            {
                throw new System.Exception("OI! Where's the guardian?");
            }

            roster.Add(guardian);
        }

        public UnitData GetRosterEntry(string name)
        {
            return roster.Find(unit => string.Equals(unit.unitName, name, System.StringComparison.CurrentCultureIgnoreCase));
        }
    }
}