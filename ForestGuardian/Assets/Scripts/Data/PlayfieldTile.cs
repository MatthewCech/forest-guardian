using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    public enum TileType
    {
        /// <summary>
        /// Testable error state 
        /// </summary>
        DEFAULT = 0,

        /// <summary>
        /// There is a ghost of a tile.
        /// A tile was.
        /// There is no tile but for data reasons we consider this a tile of type nothing.
        /// </summary>
        Nothing,

        /// <summary>
        /// Nothing fancy, a generic run-of-the-mill default tile with no special implications
        /// </summary>
        Basic,

        /// <summary>
        /// Any tile acting as a wall.
        /// </summary>
        Impassable,

        /// <summary>
        /// Meta COUNT value for enum math
        /// </summary>
        COUNT
    }

    [System.Serializable]
    public class PlayfieldTile
    {
        public TileType tileType = TileType.DEFAULT;

        public int id = Playfield.NO_ID;                // The ID for this. 0 means no ID has been assigned.
        public int associatedUnitID = Playfield.NO_ID;  // 0 means no ID association with anything
    }
}