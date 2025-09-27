using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
    /// <summary>
    /// USAGE: One time
    /// </summary>
    public class Combat100PortalWarp : CombatState
    {
        private bool firstStep = false;


        public Combat100PortalWarp(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {

        }

        public override void Update()
        {
            if (!firstStep)
            {
                firstStep = true;
                StateMachine.UI.result.gameObject.SetActive(true);
                StateMachine.UI.result.text = "Deeper into the woods...";
                Loam.CoroutineObject.Instance.StartCoroutine(ScreenDelay());
            }
        }

        private IEnumerator ScreenDelay()
        {
            yield return new WaitForSeconds(StateMachine.resultScreenTime);
            UnityEngine.Assertions.Assert.IsTrue(StateMachine.Playfield.portals.Count > 0, "There must be portals to parse this state.");
            
            StateMachine.SetState<Combat200Shutdown>();
            Core.Instance.gameData.lastFloor = StateMachine.Playfield;

            // Assume we have a portal
            PlayfieldPortal targetPortal = null;

            if (StateMachine.Playfield.portals.Count == 1)
            {
                targetPortal = StateMachine.Playfield.portals[0];
            }
            else
            {
                foreach (PlayfieldUnit unit in StateMachine.Playfield.units)
                {
                    foreach (PlayfieldPortal portal in StateMachine.Playfield.portals)
                    {
                        if (portal.location == unit.locations[PlayfieldUnit.HEAD_INDEX])
                        {
                            targetPortal = portal;
                            break;
                        }
                    }

                    if (targetPortal != null)
                    {
                        break;
                    }
                }
            }

            if (StateMachine.PlayfieldLookup.TryGetPlayfieldByName(targetPortal.target, out TextAsset levelAsset))
            {
                Core.Instance.SetPlayfieldAndLoad(levelAsset);
            }
            else
            {
                Debug.LogError($"Attempted to find level named '{targetPortal.target}' but couldn't!");
            }
        }
    }
}
