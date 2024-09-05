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
        Nothing = 1,

        /// <summary>
        /// Nothing fancy, a generic run-of-the-mill default tile with no special implications
        /// </summary>
        Basic = 2,

        /// <summary>
        /// Any tile acting as a wall.
        /// </summary>
        Impassable = 3
    }

    [System.Serializable]
    public class Tile
    {
        public TileType tileType = TileType.DEFAULT;

        [System.NonSerialized] public ulong id;

        public Tile(TileType tileType)
        {
            this.tileType = tileType;
            id = 0;
        }
    }
}