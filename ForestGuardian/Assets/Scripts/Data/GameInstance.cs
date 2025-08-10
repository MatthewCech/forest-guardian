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
        [JsonProperty] public int currency = 0;

        [JsonProperty] public List<string> completedLevelTags = new List<string>()
        { 
            "test",
            "tutorial 1"
        };

        [JsonProperty] public List<UnitData> roster = new List<UnitData>()
        { 
            new UnitData()
            { 
                unitName = "Guardian", 
                maxSize = 3, 
                speed = 2, 
                attacks = new List<AttackData>() 
                { 
                    new AttackData() 
                    { 
                        attackName = "Swipe", 
                        attackDamage = 2, 
                        attackRange = 2
                    } 
                } 
            } 
        };


        // Runtime only
        // ---------------------------------------------------------------------------
        [System.NonSerialized] public TextAsset currentPlayfield = null;
    }
}