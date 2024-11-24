using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace forest
{
    [System.Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Playfield
    {
        public const int NO_ID = -1;

        [JsonProperty] public List<PlayfieldUnit> units;
        [JsonProperty] public Collection2D<PlayfieldTile> world;
        [JsonProperty] public List<PlayfieldItem> items;

        // Internal tracking for playfield.
        // This is not unique across the game, just the specific playfield.
        [JsonProperty] private int nextID = 1;

        /// <summary>
        /// Provide a playfield-unique ID for use when creating a new item within the playfield.
        /// </summary>
        /// <returns>an unused integer to be used as an ID when identifying things in the playfield.</returns>
        public int GetNextID()
        {
            return nextID++;
        }

        /// <summary>
        /// Returns the first unit with the specified ID, or null if no unit was found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool TryGetUnit(int id, out PlayfieldUnit unit)
        {
            for (int i = 0; i < units.Count; i++)
            {
                PlayfieldUnit cur = units[i];
                if (units[i].id == id)
                {
                    unit = cur;
                    return true;
                }
            }

            unit = null;
            return false;
        }

        /// <summary>
        /// Try and find the unit at the specified location if one is present.
        /// </summary>
        /// <param name="location">The location to check.</param>
        /// <param name="unit">The unit, if found. If not found, null is assigned.</param>
        /// <returns>Whether or not a unit was found at the specified location.</returns>
        public bool TryGetUnitAt(Vector2Int location, out PlayfieldUnit unit)
        {
            for (int i = 0; i < units.Count; ++i)
            {
                PlayfieldUnit current = units[i];
                for(int locs = 0; locs < current.locations.Count; ++locs)
                {
                    Vector2Int curLoc = current.locations[locs];
                    if (curLoc == location)
                    {
                        unit = current;
                        return true;
                    }
                }
            }

            unit = null;
            return false;
        }

        /// <summary>
        /// See if we have an item at the specified location, and return it if so.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryGetItemAt(Vector2Int pos, out PlayfieldItem item)
        {
            for(int i = 0; i < items.Count; ++i)
            {
                PlayfieldItem current = items[i];
                if (current.location == pos)
                {
                    item = current;
                    return true;
                }
            }

            item = null;
            return false;
        }

        /// <summary>
        /// Removes any item found at the specified location.
        /// </summary>
        /// <param name="pos">X,Y world location to try and remove tiles at.</param>
        public void RemoveItemAt(Vector2Int pos)
        {
            for(int i = items.Count - 1; i >= 0; --i)
            {
                if (items[i].location == pos)
                {
                    items.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Attempt to retrieve the location of the tile with the specified ID.
        /// </summary>
        /// <param name="id">The id of the tile to look up.</param>
        /// <param name="loc">The position of the specified tile if found, zero otherwise.</param>
        /// <returns>If a tile was successfully found.</returns>
        public bool TryGetTileXY(int id, out Vector2Int loc)
        {
            int w = world.GetWidth();
            int h = world.GetHeight();

            for(int x = 0; x < w; ++x)
            {
                for(int y = 0; y < h; ++y)
                {
                    PlayfieldTile t = world.Get(x, y);
                    if(t.id == id)
                    {
                        loc = new Vector2Int(x, y);
                        return true;
                    }
                }
            }

            loc = Vector2Int.zero;
            return false;
        }

        /// <summary>
        /// Given a raw string in the supported format, parses out a playfield
        /// </summary>
        /// <returns>A new playfield object representing the serialized data provided</returns>
        public static Playfield BuildPlayfield(string toParse)
        {
            Playfield parsed = JsonConvert.DeserializeObject<Playfield>(toParse);
            return parsed;
        }
    }
}