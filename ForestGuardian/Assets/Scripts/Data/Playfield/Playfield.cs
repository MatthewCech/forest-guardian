using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using static UnityEditor.FilePathAttribute;
using static UnityEngine.UI.CanvasScaler;

namespace forest
{
    [System.Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Playfield
    {
        public const int NO_ID = -1;

        // Data about the playfield
        [JsonProperty] public string tagLabel;
        [JsonProperty] public string tagBestowed;

        // Contents of the playfield
        [JsonProperty] public List<PlayfieldUnit> units;
        [JsonProperty] public Collection2D<PlayfieldTile> world;
        [JsonProperty] public List<PlayfieldItem> items;
        [JsonProperty] public List<PlayfieldPortal> portals;
        [JsonProperty] public List<PlayfieldOrigin> origins;
        [JsonProperty] public PlayfieldExit exit;

        // Internal tracking for playfield.
        // This is not unique across the game, just the specific playfield.
        [JsonProperty] private int nextID = 1;

        /// <summary>
        /// Provide a playfield-unique ID for use when creating a new tile/unit/item within the playfield.
        /// </summary>
        /// <returns>an unused integer to be used as an ID when identifying things in the playfield.</returns>
        public int GetNextID()
        {
            return nextID++;
        }

        /// <summary>
        /// Determine if the data makes sense
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            bool isValid = true;

            isValid &= units != null;
            isValid &= world != null;
            isValid &= items != null;
            isValid &= portals != null;
            isValid &= nextID > 0;

            // Note: Exit is allowed to be null.
            return isValid;
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
                for (int locs = 0; locs < current.locations.Count; ++locs)
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
            for (int i = 0; i < items.Count; ++i)
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
        /// Removes first item found at the specified location.
        /// </summary>
        /// <param name="pos">X,Y world location to try and remove tiles at.</param>
        public bool RemoveItemAt(Vector2Int pos)
        {
            for (int i = items.Count - 1; i >= 0; --i)
            {
                if (items[i].location == pos)
                {
                    items.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes first unit found at the specified location - tail location will work.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool RemoveUnitAt(Vector2Int pos)
        {
            for (int unitIndex = 0; unitIndex < units.Count; ++unitIndex)
            {
                PlayfieldUnit current = units[unitIndex];
                for (int locs = 0; locs < current.locations.Count; ++locs)
                {
                    Vector2Int curLoc = current.locations[locs];
                    if (curLoc == pos)
                    {
                        units.RemoveAt(unitIndex);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the exit at the specified position 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool RemoveExitAt(Vector2Int pos)
        {
            if (exit == null)
            {
                return false;
            }
            
            if (exit.location == pos)
            {
                exit = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try and get the exit at the specified location
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="theExit"></param>
        /// <returns></returns>
        public bool TryGetExitAt(Vector2Int pos, out PlayfieldExit theExit)
        {
            if (exit == null)
            {
                theExit = null;
                return false;
            }

            if (exit.location != pos)
            {
                theExit = null;
                Debug.LogError("Exit location is not at the specified position.");
                return false;
            }

            theExit = exit;
            return true;
        }

        /// <summary>
        /// See if we have an item at the specified location, and return it if so.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryGetPortalAt(Vector2Int pos, out PlayfieldPortal portal)
        {
            if (portals == null)
            {
                portal = null;
                return false;
            }

            foreach(PlayfieldPortal current in portals)
            {
                if (current.location == pos)
                {
                    portal = current;
                    return true;
                }
            }

            portal = null;
            return false;
        }

        /// <summary>
        /// Removes first portal found at the specified location.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool RemovePortalAt(Vector2Int pos)
        {
            if(portals == null)
            {
                return false;
            }

            return portals.RemoveAll(portal => portal.location == pos) > 0;
        }


        public bool TryGetOriginAt(Vector2Int pos, out PlayfieldOrigin origin)
        {
            if (origins == null)
            {
                origin = null;
                return false;
            }

            foreach (PlayfieldOrigin cur in origins)
            {
                if (cur.location == pos)
                {
                    origin = cur;
                    return true;
                }
            }

            origin = null;
            return false;
        }

        /// <summary>
        /// Removes first portal found at the specified location.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool RemoveOriginAt(Vector2Int pos)
        {
            if (origins == null)
            {
                return false;
            }

            return origins.RemoveAll(origin => origin.location == pos) > 0;
        }

        /// <summary>
        /// Attempt to retrieve the location of the tile with the specified ID.
        /// </summary>
        /// <param name="id">The id of the tile to look up.</param>
        /// <param name="location">The position of the specified tile if found, zero otherwise.</param>
        /// <returns>If a tile was successfully found.</returns>
        public bool TryGetTileXY(int id, out Vector2Int location)
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
                        location = new Vector2Int(x, y);
                        return true;
                    }
                }
            }

            location = Vector2Int.zero;
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
