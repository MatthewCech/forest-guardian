using forest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class CombatState
    {
        public PlayfieldCore StateMachine { get; private set; }

        public CombatState(PlayfieldCore stateMachine)
        {
            StateMachine = stateMachine;
        }

        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void Shutdown() { }

        protected bool HasEnemies()
        {
            for (int i = 0; i < StateMachine.Playfield.units.Count; ++i)
            {
                PlayfieldUnit current = StateMachine.Playfield.units[i];
                if (current.team != Team.Player)
                {
                    return true;
                }
            }

            return false;
        }

        protected bool HasWinCondition()
        {
            return StateMachine.Playfield.portals.Count == 0 && StateMachine.Playfield.items.Count == 0 && !HasEnemies();
        }
    }
}