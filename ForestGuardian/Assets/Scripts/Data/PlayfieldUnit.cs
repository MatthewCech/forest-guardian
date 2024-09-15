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
        public string tag = null;
        public int id = Playfield.NO_ID; // PLAYFIELD-SPECIFIC id.
        public Team team = Team.DEFAULT;

        public const int HEAD_INDEX = 0;
        public List<Vector2Int> locations = new List<Vector2Int>();

        // Per-turn variables... These don't need serialization in practice,
        // and aren't related to data representation meaningfully. Curious philosophical coding question,
        // because removing this wouldn't impact state saving meaningfully but yeah. Like this is
        // information that all units have, and you should be able to interact with between units but it
        // also could be argued that it's only related to the specific visual+controller implementation.
        public int curMovementBudget = 0;
        
        // Like this one for example. You'd expect it to be serialized, but it's entirely reasonable to
        // buffer or tweak the max length based on various gameplay elements. 
        public int curMaxSize = 1;

        // This is basically the same move situation, but rehashed.
        public int curAttackRange = 1;
    }
}

