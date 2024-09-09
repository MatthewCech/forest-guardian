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

        // Per-turn variables... These don't need serialization in practice,
        // and aren't related to data representation meaningfully. Curious philosophical coding question,
        // because removing this wouldn't impact state saving meaningfully but yeah. Like this is
        // information that all units have, and you should be able to interact with between units but it
        // also could be argued that it's only related to the specific visual+controller implementation.
        public int movementBudget = 0;
    }
}

