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
                UnitData data = Core.Instance.GameData.roster[origin.curRosterIndex];
                Unit visual = StateMachine.VisualLookup.GetUnitTemplateByName(data.unitName);
                StateMachine.UI.unitDetails.ShowDetails(data, visual, ShowExtras.Hide); // Roster has to be player

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

            // List tracking removals done to avoid back to front, otherwise origin population is inverted with each floor.
            List<int> toRemove = new List<int>(); 

            for (int i = 0; i < StateMachine.Playfield.origins.Count; ++i)
            {
                PlayfieldOrigin origin = StateMachine.Playfield.origins[i];

                // If a player didn't specify someone to use, we're good.
                if (origin.curRosterIndex == PlayfieldOrigin.NO_INDEX_SELECTED)
                {
                    continue;
                }

                UnitData unit = Core.Instance.GameData.roster[origin.curRosterIndex];

                PlayfieldUnit unitToAdd = new PlayfieldUnit();
                unitToAdd.tag = unit.unitName;
                unitToAdd.id = StateMachine.Playfield.GetNextID();
                unitToAdd.team = Team.Player;
                unitToAdd.locations = new List<Vector2Int>() { origin.location };
                unitToAdd.rosterOverride = unit;

                StateMachine.Playfield.units.Add(unitToAdd);
                toRemove.Add(i);
            }

            // Ok, so this is still back to front because we know the list is populated front to back, but if we
            // go through and remove indexes from front to back we still get the same issue as if we did it in the
            // initial loop. So we have to still do that count offset here.
            for(int i = toRemove.Count - 1; i >= 0; --i)
            {
                StateMachine.Playfield.origins.RemoveAt(i);
            }
            
            StateMachine.VisualPlayfield.DisplayUnits(StateMachine.Playfield);
            StateMachine.VisualPlayfield.DisplayOrigins(StateMachine.Playfield);
        }

        private void TryStart()
        {
            WriteOriginUnitsToPlayfield();
            Loam.Postmaster.Instance.Send(new MsgFloorStarted());
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