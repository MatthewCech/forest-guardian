using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class Playfield
    {
        public List<PlayfieldUnit> units;
        public Collection2D<PlayfieldTile> world;
        
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
    }

    public class PlayfieldUtils
    {
        public static string testFile = ""
                + "1,guardian,3,7\n"
                + "[map]\n"
                + "1222222111\n"
                + "1112222211\n"
                + "1122222211\n"
                + "1123222111\n"
                + "1123222111\n"
                + "1122233111\n"
                + "1122233111\n"
                + "1122222111\n"
                + "1112211111\n"
                + "1111111111\n"
            ;


        public static Playfield BuildPlayfield(string toParse)
        {
            Playfield toBuild = new Playfield();

            string[] sections = toParse.Split("[map]");
            string unitsRaw = sections[0];
            string mapRaw = sections[1];

            toBuild.world = Parse2DCollection(toBuild, mapRaw);
            toBuild.units = ParseUnitList(toBuild, unitsRaw);

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
                PlayfieldUnit toAdd = new PlayfieldUnit();

                // Split, left to right.
                string row = rows[i];
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
                toAdd.headIndex = 0;
                toAdd.locations = new List<Vector2Int>
                {
                    new Vector2Int(x, y)
                };

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
            int width = rows[0].Length;
            int height = rows.Length;

            Collection2D<PlayfieldTile> toFill = new Collection2D<PlayfieldTile>(width, height);

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    char cur = rows[y][x];
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
