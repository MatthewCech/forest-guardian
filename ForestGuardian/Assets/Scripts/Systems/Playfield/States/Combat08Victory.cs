using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace forest
{
    public class Combat08Victory : CombatState
    {
        private bool firstStep = false;
        private VisualElement resultBanner;
        private Label resultLabel; 

        public Combat08Victory(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {
            resultBanner = StateMachine.UI.rootVisualElement.Q<VisualElement>("result");
            resultLabel = StateMachine.UI.rootVisualElement.Q<Label>("resultLabel");
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

                resultBanner.visible = true;
                resultLabel.text = "Area Cleared - Victory!";
                Loam.CoroutineObject.Instance.StartCoroutine(ScreenDelay());
            }
        }

        private IEnumerator ScreenDelay()
        {
            yield return new WaitForSeconds(StateMachine.resultScreenTime);
            StateMachine.SetState<Combat20Shutdown>();
        }
    }
}