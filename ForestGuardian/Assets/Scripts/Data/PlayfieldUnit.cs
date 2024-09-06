using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public enum Team
    {
        DEFAULT = 0,
        Player = 1,
        Opponent = 2,
    }

    public class PlayfieldUnit
    {
        public string tag;
        public int id; // PLAYFIELD-SPECIFIC id.
        public Team team = Team.DEFAULT;
        public int headIndex = 0;
        public List<Vector2Int> locations = new List<Vector2Int>();

        // Per-turn variables
        public int movesRemaining = 0;
    }
}

