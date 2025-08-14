using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

namespace forest
{
    /// <summary>
    /// USAGE: One time
    /// </summary>
    public class Combat015OptionalPlace : CombatState
    {
        private bool firstStep = false;

        private Origin selected = null;

        public Combat015OptionalPlace(PlayfieldCore stateMachine) : base(stateMachine) { }

        private MessageSubscription selectOrigin; 
        private MessageSubscription selectIndicated;

        public override void Start()
        {
            StateMachine.UI.SetSelectorVisibility(true);

            selectOrigin = Postmaster.Instance.Subscribe<MsgOriginPrimaryAction>(SelectOrigin);
            selectIndicated = Postmaster.Instance.Subscribe<MsgRosterUnitIndicated>(UnitFromRosterSelected);

            StateMachine.UI.startFloor.onClick.AddListener(TryStart);
            StateMachine.UI.startFloor.interactable = false;

            StateMachine.VisualPlayfield.DisplayAll(StateMachine.Playfield);
        }

        public override void Shutdown()
        {
            StateMachine.UI.startFloor.onClick.RemoveListener(TryStart);

            selectIndicated.Dispose();
            selectOrigin.Dispose();
        }

        private void UnitFromRosterSelected(Message raw)
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
                    Origin toHide = StateMachine.VisualPlayfield.FindOrigin(origin.location);
                    toHide.SetHighlight(false);
                }
            }
        }

        /// <summary>
        /// Writes and displays the origin units to the playfield
        /// </summary>
        private void WriteOriginUnitsToPlayfield()
        {
            if (StateMachine.Playfield.origins == null || StateMachine.Playfield.origins.Count == 0)
            {
                return;
            }

            foreach (PlayfieldOrigin origin in StateMachine.Playfield.origins)
            {
                if (origin.curRosterIndex == PlayfieldOrigin.NO_INDEX_SELECTED)
                {
                    continue;
                }

                UnitData unit = Core.Instance.gameData.roster[origin.curRosterIndex];

                PlayfieldUnit unitToAdd = new PlayfieldUnit();
                unitToAdd.tag = unit.unitName;
                unitToAdd.id = StateMachine.Playfield.GetNextID();
                unitToAdd.team = Team.Player;
                unitToAdd.locations = new List<Vector2Int>() { origin.location };
                unitToAdd.rosterOverride = unit;

                StateMachine.Playfield.units.Add(unitToAdd);
            }

            StateMachine.Playfield.origins.Clear();

            StateMachine.VisualPlayfield.DisplayUnits(StateMachine.Playfield);
            StateMachine.VisualPlayfield.DisplayOrigins(StateMachine.Playfield);
        }

        private void TryStart()
        {
            WriteOriginUnitsToPlayfield();
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
            StateMachine.UI.SetSelectorVisibility(false);

            yield return new WaitForSeconds(StateMachine.turnDelay);
            StateMachine.SetState<Combat020BuildPlayfield>();
        }
    }
}