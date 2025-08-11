using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static forest.VisualPlayfield;

namespace forest
{
    /// <summary>
    /// NOTE: This can be turned into addressables long term.
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
        public Tile defaultTileTemplate; // The tile used when there's nothing present
        public Portal portalTemplate;    // "Stairs" in a traditional rogue/PMD sense, dungeon advancement
        public Exit exitTemplate;        // "Exit" in a "Go back to the map" sense
        public Origin originTemplate;    // A start/placement location

        [Header("Template Lists")]
        public List<Tile> tileTemplates;
        public List<Unit> unitTemplates;
        public List<Item> itemTemplates;


        public Tile DefaultTileTemplate { get { return defaultTileTemplate; } }
        public Portal PortalTemplate { get { return portalTemplate; } }
        public Exit ExitTemplate { get { return exitTemplate; } }
        public Origin OriginTemplate { get { return originTemplate; } }

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

        /// <summary>
        /// Access to the unit that's associated with with the specified tag.
        /// This lookup ignores case.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public Unit GetUnitTemplateByName(string tag)
        {
            bool FindTag(Unit unit)
            {
                return unit.data.unitName.Equals(tag, StringComparison.InvariantCultureIgnoreCase);
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
    }
}