using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
    /// <summary>
    /// USAGE: One time
    /// </summary>
    public class Combat010Startup : CombatState
    {
        private bool firstStep = false;


        public Combat010Startup(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {
            StateMachine.UI.SetSelectorVisibility(false);
            StateMachine.UI.result.gameObject.SetActive(false);
        }

        public override void Update()
        {
            if(!firstStep)
            {
                firstStep = true;
                Loam.CoroutineObject.Instance.StartCoroutine(QueueUpNext());
            }
        }

        private IEnumerator QueueUpNext()
        {
            // Handle initial playfield configuration
            yield return new WaitForSeconds(StateMachine.turnDelay);

            // Determine if we're going to place or can go straight into simulating
            yield return new WaitForSeconds(StateMachine.turnDelay);

            int originsNeedingAssignment = 0;
            
            
            if (StateMachine.Playfield.origins != null)
            {
                foreach (PlayfieldOrigin origin in StateMachine.Playfield.origins)
                {
                    if (origin.partyIndex == PlayfieldOrigin.NO_INDEX_SELECTED)
                    {
                        // No index is explicitly requiring the player to say.
                        originsNeedingAssignment++;
                    }
                    else if(Core.Instance.GameData.lastFloor == null)
                    {
                        // Doesn't matter if we have a specific party index ask, we have no last floor data.
                        originsNeedingAssignment++;
                    }
                }
            }

            if (originsNeedingAssignment > 0)
            {
                StateMachine.UI.SetSelectorVisibility(true);
                StateMachine.SetState<Combat015OptionalPlace>();
            }
            else
            {
                StateMachine.UI.SetSelectorVisibility(false);
                StateMachine.SetState<Combat020BuildPlayfield>();
            }
        }
    }
}