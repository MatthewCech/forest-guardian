using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static forest.VisualPlayfield;

namespace forest
{
    /// <summary>
    /// NOTE: This can be turned into addressables long term if desired.
    /// </summary>
    [CreateAssetMenu(fileName = "Lookup Data", menuName = "ScriptableObjects/Lookup Data", order = 1)]
    public class VisualLookup : ScriptableObject
    {
        public float interactionZPriority = 0.5f; // Used as -z in practice to move towards camera
        public float unitZPriority = 0.2f; // Used as -z in practice to move towards camera

        public Indicator movePreviewTemplate;
        public Indicator moveInteractionTemplate;

        public List<TileInfo> tileTemplates;
        public List<UnitInfo> unitTemplates;
        public List<ItemInfo> itemTemplates;

        /// <summary>
        /// Access to the unit that's associated with with the specified tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public UnitInfo GetUnityByTag(string tag)
        {
            for (int i = 0; i < unitTemplates.Count; ++i)
            {
                UnitInfo cur = unitTemplates[i];
                if (cur.unitTemplate.name.Equals(tag, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    return cur;
                }
            }

            return null;
        }


        /// <summary>
        /// Access to the tile associated with the specific tile type laid out in the TileType enum.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public TileInfo GetTileByType(TileType type)
        {
            for (int i = 0; i < tileTemplates.Count; ++i)
            {
                TileInfo cur = tileTemplates[i];
                if (cur.tileType == type)
                {
                    return cur;
                }
            }

            return null;
        }


        [System.Serializable]
        public class TileInfo
        {
            public Tile tileTemplate;
            public TileType tileType;
        }

        [System.Serializable]
        public class UnitInfo
        {
            public Unit unitTemplate;
        }

        [System.Serializable]
        public class ItemInfo
        {
            public Item itemTemplate;
        }
    }
}