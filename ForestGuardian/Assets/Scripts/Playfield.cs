using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class Playfield
    {
        public Collection2D<Tile> world;

        public Playfield()
        {
            world = CreateTest2DCollection();
        }

        public static Collection2D<Tile> CreateTest2DCollection()
        {
            string testLayout = "";
            testLayout += "1222222111\n";
            testLayout += "1112222211\n";
            testLayout += "1122222211\n";
            testLayout += "1123222111\n";
            testLayout += "1123222111\n";
            testLayout += "1122233111\n";
            testLayout += "1122233111\n";
            testLayout += "1122222111\n";
            testLayout += "1112211111\n";
            testLayout += "1111111111";

            return Parse2DCollection(testLayout);
        }

        private static Collection2D<Tile> Parse2DCollection(string testLayout)
        {
            string[] rows = testLayout.Trim().Split('\n');
            int width = rows[0].Length;
            int height = rows.Length;

            Collection2D<Tile> toFill = new Collection2D<Tile>(width, height);

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    char cur = rows[y][x];
                    int num = cur - '0';
                    Tile newTile = new Tile((TileType)num);
                    toFill.Set(x, y, newTile);
                }
            }

            return toFill;
        }
    }
}
