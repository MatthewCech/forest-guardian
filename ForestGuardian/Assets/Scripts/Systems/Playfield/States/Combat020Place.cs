using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

namespace forest
{
    public class Combat020Place : CombatState
    {
        private bool firstStep = false;

        private Origin selected = null;

        public Combat020Place(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {
            StateMachine.UI.SetSelectorVisibility(true);

            Postmaster.Instance.Subscribe<MsgOriginPrimaryAction>(SelectOrigin);
            Postmaster.Instance.Subscribe<MsgRosterUnitIndicated>(UnitIndicated);
            StateMachine.UI.startFloor.onClick.AddListener(TryStart);
            StateMachine.UI.startFloor.interactable = false;
        }

        private void UnitIndicated(Message raw)
        {
            if(selected == null)
            {
                return;
            }

            MsgRosterUnitIndicated msg = raw as MsgRosterUnitIndicated;

            if(StateMachine.Playfield.TryGetOriginAt(selected.associatedPos, out PlayfieldOrigin origin))
            {
                origin.curRosterIndex = msg.rosterIndex;
                UnitData data = Core.Instance.gameData.roster[origin.curRosterIndex];
                Unit visual = StateMachine.VisualLookup.GetUnitTemplateByName(data.unitName);
                StateMachine.UI.unitDetails.ShowDetails(data, visual);

                StateMachine.UI.startFloor.interactable = true;
            }

            StateMachine.VisualPlayfield.DisplayOrigins(StateMachine.Playfield);
        }

        private void SelectOrigin(Message raw)
        {
            MsgOriginPrimaryAction msg = raw as MsgOriginPrimaryAction;

            foreach(PlayfieldOrigin origin in StateMachine.Playfield.origins)
            {
                if(origin.location == msg.origin.associatedData.location)
                {
                    msg.origin.SetHighlight(true);
                    selected = msg.origin;
                }
                else
                {
                    StateMachine.VisualPlayfield.FindOrigin(origin.location).SetHighlight(false);
                }
            }
        }

        private void TryStart()
        {
            Loam.CoroutineObject.Instance.StartCoroutine(QueueUpNext());
        }

        public override void Update()
        {
            if(!firstStep)
            {
                firstStep = true;
            }
        }

        private IEnumerator QueueUpNext()
        {
            StateMachine.UI.startFloor.onClick.RemoveListener(TryStart);
            StateMachine.UI.SetSelectorVisibility(false);

            yield return new WaitForSeconds(StateMachine.turnDelay);
            StateMachine.SetState<Combat030PrepareTurn>();
        }
    }
}