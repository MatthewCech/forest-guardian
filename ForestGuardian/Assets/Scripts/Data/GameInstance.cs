using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [JsonProperty] public List<string> unlockedTags = new List<string>();
        [JsonProperty] public List<UnitData> roster = new List<UnitData>();

        // Runtime Data Only
        [System.NonSerialized] public TextAsset currentPlayfield = null;

        /// <summary>
        /// Provides defaults. Arguments and construction can and should be reworked 
        /// </summary>
        public void PopulateDefaults(VisualLookup lookup)
        {
            currency = 0;

            unlockedTags = new List<string>()
            {
                "tutorial",
                "ivy-grove"
            };


            AddToRoster(lookup, "Guardian");
            AddToRoster(lookup, "BogWisp");
        }

        /// <summary>
        /// Look up specified name and attempt to add the default data to our roster
        /// </summary>
        private void AddToRoster(VisualLookup lookup, string name)
        {
            UnitData toAdd = lookup.GetUnitTemplateByName(name).data;
            UnityEngine.Assertions.Assert.IsNotNull(toAdd, $"Unit template named '{name}' was not found! Check visual lookup.");
            roster.Add(toAdd);
        }

        /// <summary>
        /// Look up the specified unit name in the roster, ignoring case.
        /// </summary>
        /// <returns>Data associated with the requested name, or null if not found.</returns>
        public UnitData GetRosterEntry(string name)
        {
            return roster.Find(unit => string.Equals(unit.unitName, name, System.StringComparison.CurrentCultureIgnoreCase));
        }
    }
}