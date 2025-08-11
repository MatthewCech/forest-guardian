using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
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
            StateMachine.SetState<Combat200Shutdown>();

            UnityEngine.Assertions.Assert.IsTrue(StateMachine.Playfield.portals.Count > 0, "There must be portals to parse this state.");

            // Assume we have a portal
            PlayfieldPortal targetPortal = null;
            foreach(PlayfieldUnit unit in StateMachine.Playfield.units)
            {
                foreach(PlayfieldPortal portal in StateMachine.Playfield.portals)
                {
                    if (portal.location == unit.locations[PlayfieldUnit.HEAD_INDEX])
                    {
                        targetPortal = portal;
                        break;
                    }
                }

                if(targetPortal != null)
                {
                    break;
                }
            }

            if (StateMachine.PlayfieldLookup.TryGetPlayfieldByName(targetPortal.target, out TextAsset levelAsset))
            {
                Core.Instance.LoadLevelPlayfield(levelAsset);
            }
            else
            {
                Debug.LogError($"Attempted to find level named '{targetPortal.target}' but couldn't!");
            }
        }
    }
}
