using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
    /// <summary>
    /// USAGE: One time
    /// </summary>
    public class Combat200Shutdown : CombatState
    {
        private bool firstStep = false;


        public Combat200Shutdown(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {

        }

        public override void Update()
        {
            if (!firstStep)
            {
                firstStep = true;

                if (StateMachine.UI != null && StateMachine.UI.result != null)
                {
                    StateMachine.UI.result.gameObject.SetActive(false);
                }

                Core.Instance.LoadLevelMap();
            }
        }
    }
}