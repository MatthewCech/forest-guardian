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

                if(!string.IsNullOrWhiteSpace(StateMachine.Playfield.tagBestowed))
                {
                    bool has = Core.Instance.gameData.completedLevelTags.Contains(StateMachine.Playfield.tagBestowed);
                    if (!has)
                    {
                        Core.Instance.gameData.completedLevelTags.Add(StateMachine.Playfield.tagBestowed);
                    }
                }

                StateMachine.UI.result.gameObject.SetActive(true);
                StateMachine.UI.result.text = "Area Cleared - Victory!";
                Loam.CoroutineObject.Instance.StartCoroutine(ScreenDelay());
            }
        }

        private IEnumerator ScreenDelay()
        {
            yield return new WaitForSeconds(StateMachine.resultScreenTime);
            StateMachine.SetState<Combat200Shutdown>();
        }
    }
}