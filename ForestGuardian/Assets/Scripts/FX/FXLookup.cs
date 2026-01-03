using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    public enum FXTag : int
    {
        NONE = 0, 

        FX_ATT_BASIC_CLOSE = 1,
        FX_ATT_BASIC_FAR = 2
    }

    /// <summary>
    /// NOTE: This can be turned into addressables long term.
    /// </summary>
    [CreateAssetMenu(fileName = "FX Data", menuName = "ScriptableObjects/FX Lookup Data", order = 3)]
    public class FXLookup : ScriptableObject
    {
        [SerializeField] private List<FXPerformer> fxPairs = new List<FXPerformer>();

        private Dictionary<FXTag, FXPerformer> tagLookup = new Dictionary<FXTag, FXPerformer>();

        public void Initialize()
        {
            tagLookup.Clear();

            foreach (var item in fxPairs)
            {
                tagLookup.Add(item.FXTag, item);
            }
        }

        public bool TryGetFXTemplateByTag(FXTag tag, out FXPerformer performer)
        {
            return tagLookup.TryGetValue(tag, out performer);
        }
    }
}
