using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class Playfield
    {
        public List<PlayfieldUnit> units;
        public Collection2D<PlayfieldTile> world;
        public List<PlayfieldItem> items;

        // Internal tracking for playfield.
        // This is not unique across the game, just the specific playfield.
        private int nextID;

        public Playfield()
        {
            nextID = 1;
        }

        public int GetNextID()
        {
            return nextID++;
        }

        /// <summary>
        /// Returns the first Unit with the specified ID, or null if no unit was found.
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

        public PlayfieldUnit GetUnit(int id)
        {
            bool didFind = TryGetUnit(id, out PlayfieldUnit unit);
            UnityEngine.Assertions.Assert.IsTrue(didFind, "A unit is expected.");
            return unit;
        }

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
    }

    public class PlayfieldUtils
    {
        public static Playfield BuildPlayfield(string toParse)
        {
            Playfield toBuild = new Playfield();

            string[] splitOnmap = toParse.Split("[map]");
            string mixedMapEntities = splitOnmap[0];
            string mapRaw = splitOnmap[1];

            toBuild.world = Parse2DCollection(toBuild, mapRaw);
            toBuild.units = ParseUnitList(toBuild, mixedMapEntities);
            toBuild.items = ParseItemList(toBuild, mixedMapEntities);

            AssignUnits(toBuild.world, toBuild.units);

            return toBuild;
        }


        /// <summary>
        /// Takes input of a list of newline separated 
        /// unit indicators and allows 
        /// 
        /// delimiters
        ///  |    | |
        /// 1,tree,4,2     (SAMPLE LINE)
        /// ^   ^  ^
        /// ^   ^  ^ x,y pos from lower left
        /// ^   ^unit tag
        /// ^team 1
        ///

        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        private static List<PlayfieldUnit> ParseUnitList(Playfield playfield, string unitSection)
        {
            string[] rows = unitSection.Trim().Split('\n');

            List<PlayfieldUnit> toFill = new List<PlayfieldUnit>();

            for(int i = 0; i < rows.Length; ++i)
            {
                // Current operating string. See if we need to leave early.
                string row = rows[i];
                if(row[0] == 'i')
                {
                    continue;
                }

                PlayfieldUnit toAdd = new PlayfieldUnit();

                // Split, left to right.
                string[] split = row.Split(',');

                // Team info
                Team team = (Team)int.Parse(split[0]);

                // Icon info
                string tag = split[1];

                // Starting location
                int x = int.Parse(split[2]);
                int y = int.Parse(split[3]);

                // Apply information
                toAdd.tag = tag;
                toAdd.id = playfield.GetNextID();
                toAdd.team = team;
                toAdd.locations = new List<Vector2Int>
                {
                    new Vector2Int(x, y)
                };

                toFill.Add(toAdd);
            }

            return toFill;
        }

        // This is super duped code. Convert input type to something like JSON or similar to reduce this noise.
        private static List<PlayfieldItem> ParseItemList(Playfield playfield, string unitSection)
        {
            string[] rows = unitSection.Trim().Split('\n');

            List<PlayfieldItem> toFill = new List<PlayfieldItem>();

            for (int i = 0; i < rows.Length; ++i)
            {
                string row = rows[i];
                if (row[0] != 'i')
                {
                    continue;
                }

                PlayfieldItem toAdd = new PlayfieldItem();

                string[] split = row.Split(',');
                string tag = split[1];

                int x = int.Parse(split[2]);
                int y = int.Parse(split[3]);

                toAdd.id = playfield.GetNextID();
                toAdd.tag = tag;

                toFill.Add(toAdd);
            }

            return toFill;
        }


        /// <summary>
        /// Assumes inpute 
        /// </summary>
        /// <param name="mapSection"></param>
        /// <returns></returns>
        private static Collection2D<PlayfieldTile> Parse2DCollection(Playfield playfield, string mapSection)
        {
            string[] rows = mapSection.Trim().Split('\n');
            int width = rows[0].Trim().Length;
            int height = rows.Length;

            Collection2D<PlayfieldTile> toFill = new Collection2D<PlayfieldTile>(width, height);

            for (int y = 0; y < height; ++y)
            {
                string row = rows[y].Trim();
                for (int x = 0; x < width; ++x)
                {
                    char cur = row[x];
                    int num = cur - '0';
                    PlayfieldTile newTile = new PlayfieldTile((TileType)num);
                    newTile.id = playfield.GetNextID();
                    toFill.Set(x, y, newTile);
                }
            }

            return toFill;
        }

        private static void AssignUnits(Collection2D<PlayfieldTile> toCorrect, List<PlayfieldUnit> units)
        {
            for(int unit = 0; unit < units.Count; ++unit)
            {
                PlayfieldUnit cur = units[unit];
                for (int j = 0; j < cur.locations.Count; ++j)
                {
                    Vector2Int curLocation = cur.locations[j];
                    toCorrect.Get(curLocation.x, curLocation.y).associatedUnitID = cur.id;
                }
            }
        }
    }
}
