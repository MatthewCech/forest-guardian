using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
    public class Combat010Startup : CombatState
    {
        private bool firstStep = false;


        public Combat010Startup(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {

        }

        public override void Update()
        {
            if(!firstStep)
            {
                firstStep = true;
                StateMachine.UI.result.gameObject.SetActive(false);
                Loam.CoroutineObject.Instance.StartCoroutine(QueueUpNext());
            }
        }

        private IEnumerator QueueUpNext()
        {
            yield return new WaitForSeconds(StateMachine.turnDelay);

            int originCount = StateMachine.Playfield.origins.Count;
            foreach (PlayfieldOrigin origin in StateMachine.Playfield.origins)
            {
                if(origin.curSelectionPreComplete)
                {
                    originCount--;
                }
            }

            if (originCount > 0)
            {
                StateMachine.SetState<Combat020Place>();
            }
            else
            {
                StateMachine.SetState<Combat030PrepareTurn>();
            }
        }
    }
}