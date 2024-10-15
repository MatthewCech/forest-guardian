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
    }
}