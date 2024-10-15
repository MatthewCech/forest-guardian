using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class Combat10Shutdown : CombatState
    {
        public Combat10Shutdown(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Update()
        {
            throw new System.Exception("Signal for scene unload or something");
        }
    }
}