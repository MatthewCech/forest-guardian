using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
    public class Combat10PortalWarp : CombatState
    {
        private bool firstStep = false;
        private VisualElement resultBanner;
        private Label resultLabel;
        
        public Combat10PortalWarp(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {
            resultBanner = StateMachine.UI.rootVisualElement.Q<VisualElement>("result");
            resultLabel = StateMachine.UI.rootVisualElement.Q<Label>("resultLabel");
        }

        public override void Update()
        {
            if (!firstStep)
            {
                firstStep = true;
                resultBanner.visible = true;
                resultLabel.text = "Deeper into the woods...";
                Loam.CoroutineObject.Instance.StartCoroutine(ScreenDelay());
            }
        }

        private IEnumerator ScreenDelay()
        {
            yield return new WaitForSeconds(StateMachine.resultScreenTime);
            StateMachine.SetState<Combat20Shutdown>();

            UnityEngine.Assertions.Assert.IsTrue(StateMachine.Playfield.portals.Count > 0, "There must be portals to parse this state.");

            // Assume we have a portal, so 
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

            if (StateMachine.TryGetLevelByName(targetPortal.target, out TextAsset levelAsset))
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
