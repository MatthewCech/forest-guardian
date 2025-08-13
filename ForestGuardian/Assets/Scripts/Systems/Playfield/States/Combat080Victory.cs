using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
    public class Combat080Victory : CombatState
    {
        private bool firstStep = false;

        public Combat080Victory(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {

        }

        public override void Update()
        {
            if(!firstStep)
            {
                firstStep = true;

                TryBestowTags();

                StateMachine.UI.result.gameObject.SetActive(true);
                StateMachine.UI.result.text = "Area Cleared - Victory!";
                Loam.CoroutineObject.Instance.StartCoroutine(ScreenDelay());
            }
        }

        /// <summary>
        /// Perform any unlocks we're expecting if this playfield has them
        /// </summary>
        private void TryBestowTags()
        {
            if (StateMachine.Playfield.tagsBestowed == null)
            {
                return;
            }

            for (int i = 0; i < StateMachine.Playfield.tagsBestowed.Count; ++i)
            {
                string toUnlock = StateMachine.Playfield.tagsBestowed[i];
                if (!string.IsNullOrWhiteSpace(toUnlock))
                {
                    bool tagAlreadyUnlocked = Core.Instance.gameData.unlockedTags.Contains(toUnlock);
                    if (!tagAlreadyUnlocked)
                    {
                        Core.Instance.gameData.unlockedTags.Add(toUnlock);
                    }
                }
            }
        }

        private IEnumerator ScreenDelay()
        {
            yield return new WaitForSeconds(StateMachine.resultScreenTime);
            StateMachine.SetState<Combat200Shutdown>();
        }
    }
}