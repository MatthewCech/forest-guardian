using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    /// <summary>
    /// NOTE: This can be turned into addressables long term.
    /// </summary>
    [CreateAssetMenu(fileName = "Playfield Data", menuName = "ScriptableObjects/Playfield Lookup Data", order = 3)]
    public class PlayfieldLookup : ScriptableObject
    {
        [SerializeField] private List<TextAsset> playfields;

        /// <summary>
        /// Thumb through all available level assets and attempt to get one by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="levelAsset"></param>
        /// <returns></returns>
        public bool TryGetPlayfieldByName(string name, out TextAsset levelAsset)
        {
            foreach (TextAsset asset in playfields)
            {
                if (string.Equals(asset.name, name, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    levelAsset = asset;
                    return true;
                }
            }

            levelAsset = null;
            return false;
        }
    }
}