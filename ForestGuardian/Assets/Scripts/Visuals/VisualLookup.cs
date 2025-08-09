using System;
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
    [CreateAssetMenu(fileName = "Lookup Data", menuName = "ScriptableObjects/Visual Lookup Data", order = 1)]
    public class VisualLookup : ScriptableObject
    {
        public float interactionZPriority = 0.4f; // Used as -z in practice to move towards camera
        public float unitZPriority = 0.2f; // Used as -z in practice to move towards camera

        [Header("Indicators")]
        public Indicator movePreviewTemplate;
        public Indicator moveInteractionTemplate;
        public Indicator attackPreview;
        public Indicator movePathTemplate;

        [Header("Various Templates")]
        public Tile defaultTileTemplate;
        public Portal portalTemplate; // "Stairs" in a traditional rogue/PMD sense, dungeon advancement
        public Exit exitTemplate;     // "Exit" in a "Go back to the map" sense
        public List<Tile> tileTemplates;
        public List<Unit> unitTemplates;
        public List<Item> itemTemplates;

        /// <summary>
        /// Access to the unit that's associated with with the specified tag.
        /// This lookup ignores case.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public Unit GetUnityByTag(string tag)
        {
            bool FindTag(Unit info)
            {
                return info.name.Equals(tag, StringComparison.InvariantCultureIgnoreCase);
            }

            return unitTemplates.Find(FindTag);
        }

        /// <summary>
        /// Access the lookup entry associated with with the specified tag.
        /// This lookup ignores case.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public Item GetItemByTag(string tag)
        {
            bool FindInfo(Item info)
            {
                return info.name.Equals(tag, StringComparison.InvariantCultureIgnoreCase);
            }

            return itemTemplates.Find(FindInfo);
        }

        /// <summary>
        /// Access to the tile associated with the specific tile type laid out in the TileType enum.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Tile GetTileByTag(string tag)
        {
            bool FindTile(Tile info)
            {
                return info.name.Equals(tag, StringComparison.InvariantCultureIgnoreCase);
            }

            return tileTemplates.Find(FindTile);
        }

        public Portal GetPortalByTag(string tag)
        {
            if (portalTemplate.name.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
            {
                return portalTemplate;
            }
            else
            {
                Debug.LogWarning($"You're trying to look up a portal by a different name? {tag} was asked for, {portalTemplate.name} exists.");
                return portalTemplate;
            }
        }

        public Exit GetExitByTag(string tag)
        {
            if (exitTemplate.name.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
            {
                return exitTemplate;
            }
            else
            {
                Debug.LogWarning($"You're trying to look up an exit by a different name? {tag} was asked for, {exitTemplate.name} exists.");
                return exitTemplate;
            }
        }
    }
}