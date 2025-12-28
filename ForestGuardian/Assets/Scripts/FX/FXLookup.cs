using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    public enum FXTag : int
    {
        NONE = 0, 

        FX_ATTACK_GENERIC = 1,
    }

    [System.Serializable]
    public class FXPair
    {
        public FXTag tag;
        public FXPerformer performer;
    }

    /// <summary>
    /// NOTE: This can be turned into addressables long term.
    /// </summary>
    [CreateAssetMenu(fileName = "FX Data", menuName = "ScriptableObjects/FX Lookup Data", order = 3)]
    public class FXLookup : ScriptableObject
    {
        [SerializeField] private List<FXPair> fxPairs = new List<FXPair>();

        private Dictionary<FXTag, FXPerformer> tagLookup = new Dictionary<FXTag, FXPerformer>();

        public void Initialize()
        {
            tagLookup.Clear();

            foreach (var item in fxPairs)
            {
                tagLookup.Add(item.tag, item.performer);
            }
        }

        public bool TryGetFXTemplateByTag(FXTag tag, out FXPerformer performer)
        {
            return tagLookup.TryGetValue(tag, out performer);
        }
    }
}
