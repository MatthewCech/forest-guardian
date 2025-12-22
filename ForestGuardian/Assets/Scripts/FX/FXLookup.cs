using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public enum FXTag : int
    {
        NONE = 0, 

        FX_ATTACK_GENERIC = 1,
    }

    /// <summary>
    /// NOTE: This can be turned into addressables long term.
    /// </summary>
    [CreateAssetMenu(fileName = "FX Data", menuName = "ScriptableObjects/FX Lookup Data", order = 3)]
    public class FXLookup : ScriptableObject
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
