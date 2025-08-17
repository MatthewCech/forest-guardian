using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Hardware;
using UnityEditorInternal;
using UnityEngine;
using System.Linq;
using static UnityEngine.UI.CanvasScaler;

namespace forest
{
    /// <summary>
    /// USAGE: Per turn
    /// </summary>
    public class Combat040PlayerMove : CombatState
    {

        private MessageSubscription subMoveTileClicked = null;
        private MessageSubscription subPlayerClicked = null;

        private List<PlayfieldUnit> pendingUnits = null;
        private PlayfieldUnit currentUnit = null;

        public Combat040PlayerMove(PlayfieldCore stateMachine) : base(stateMachine) { }

        // Pop the head to get to the next available unit
        public bool SelectNextUnitToMove()
        {
            if (pendingUnits == null || pendingUnits.Count == 0)
            {
                currentUnit = null;
                return false;
            }

            currentUnit = pendingUnits[0];
            pendingUnits.RemoveAt(0);

            if (!CheckForShortCircuitStateJump(currentUnit))
            {
                StateMachine.VisualPlayfield.DisplayIndicatorMovePreview(currentUnit, StateMachine.Playfield);
            }
            return true;
        }

        public override void Start()
        {
            subMoveTileClicked = Postmaster.Instance.Subscribe<MsgIndicatorClicked>(PlayerMove_MsgMoveTileClicked);
            subPlayerClicked = Postmaster.Instance.Subscribe<MsgUnitPrimaryAction>(PlayerMove_MsgFriendlyUnitClicked);

            // Collect player units
            pendingUnits = StateMachine.Playfield.units.Where((unit) => unit.team == Team.Player).ToList();
            UnityEngine.Assertions.Assert.IsTrue(pendingUnits.Count > 0, "It's impossible to start a player turn without units");

            SelectNextUnitToMove();
        }

        public override void Update()
        {
            PlayfieldUnit toProcess = StateMachine.Playfield.units[0];
            ProcessKeyboardInput(toProcess);
        }

        public override void Shutdown()
        {
            subMoveTileClicked?.Dispose();
            subPlayerClicked?.Dispose();
        }

        void PlayerMove_MsgFriendlyUnitClicked(Message raw)
        {
            MsgUnitPrimaryAction msg = raw as MsgUnitPrimaryAction;

            PlayfieldUnit unit = msg.unit.associatedData;
            Vector2Int gridPosClicked = msg.position;
            Vector2Int clickedUnitHeadPos = msg.unit.associatedData.locations[PlayfieldUnit.HEAD_INDEX];

            bool isHead = gridPosClicked == clickedUnitHeadPos;
            if (isHead)
            {
                unit.curMovementBudget = 0;
                StateMachine.VisualPlayfield.HideIndicators();
                StateMachine.VisualPlayfield.DisplayIndicatorAttackPreview(unit, StateMachine.Playfield);
                return;
            }
        }

        /// <summary>
        /// Receive a specific tile click event, handling immediate movement indicators specifically.
        /// In the event of an attack, we then handle damage and impact of the unit.
        /// </summary>
        /// <param name="raw"></param>
        void PlayerMove_MsgMoveTileClicked(Message raw)
        {
            MsgIndicatorClicked msg = raw as MsgIndicatorClicked;
            PlayfieldUnit unit = msg.indicator.ownerUnit;
            Unit visualUnit = StateMachine.VisualPlayfield.FindUnit(unit);
            Vector2Int target = msg.indicator.overlaidPosition;

            if (msg.indicator.type == IndicatorType.ImmediateMove)
            {
                Utils.MoveUnitToLocation(StateMachine.Playfield, StateMachine.VisualPlayfield, unit, target);

                Postmaster.Instance.Send(new MsgUnitMoved { unit = visualUnit });

                CheckForShortCircuitStateJump(unit);
                if (unit.curMovementBudget == 0)
                {
                    StateMachine.VisualPlayfield.DisplayIndicatorAttackPreview(unit, StateMachine.Playfield);
                }
            }

            if (msg.indicator.type == IndicatorType.Attack)
            {
                if (StateMachine.Playfield.TryGetUnitAt(target, out PlayfieldUnit targetUnit))
                {
                    StateMachine.VisualPlayfield.DamageUnit(unit, targetUnit, StateMachine.Playfield);
                }

                Postmaster.Instance.Send(new MsgUnitAttack { unit = visualUnit });

                StateMachine.VisualPlayfield.HideIndicators();

                if(!SelectNextUnitToMove())
                {
                    StateMachine.SetState<Combat050OpponentMove>();
                }

                return;
            }
        }

        /// <summary>
        /// General catch all variadic function that, given a controlled unit, attempt to 
        /// move it to the target if any of the following keys as parameters are met.
        /// </summary>
        /// <param name="controlledUnit">The unit that we're attempting to move.</param>
        /// <param name="target">A target position to try and move to. This is absolute, not relative.</param>
        /// <param name="keyList">Any number of keys to consider</param>
        private void TryMoveOnKeyInput(PlayfieldUnit controlledUnit, Vector2Int target, params KeyCode[] keyList)
        {
            Vector2Int head = controlledUnit.locations[PlayfieldUnit.HEAD_INDEX];
            Vector2Int newMovement = head + target;

            bool relevantKeyDown = false;
            for (int i = 0; i < keyList.Length; i++)
            {
                KeyCode keyCode = keyList[i];
                if (Input.GetKeyDown(keyCode))
                {
                    relevantKeyDown = true;
                }
            }

            if (relevantKeyDown)
            {
                if (Utils.CanMovePlayfieldUnitTo(StateMachine.Playfield, controlledUnit, newMovement))
                {
                    Utils.MoveUnitToLocation(StateMachine.Playfield, StateMachine.VisualPlayfield, controlledUnit, newMovement);
                    CheckForShortCircuitStateJump(controlledUnit);
                }
            }
        }

        /// <summary>
        /// Handles specific inputs all associated with the input.
        /// </summary>
        /// <param name="controlledUnit"></param>
        private void ProcessKeyboardInput(PlayfieldUnit controlledUnit)
        {
            if (controlledUnit.curMovementBudget == 0)
            {
                return;
            }

            TryMoveOnKeyInput(controlledUnit, Vector2Int.down, KeyCode.W, KeyCode.UpArrow);
            TryMoveOnKeyInput(controlledUnit, Vector2Int.up, KeyCode.S, KeyCode.DownArrow);
            TryMoveOnKeyInput(controlledUnit, Vector2Int.left, KeyCode.A, KeyCode.LeftArrow);
            TryMoveOnKeyInput(controlledUnit, Vector2Int.right, KeyCode.D, KeyCode.RightArrow);

            if (controlledUnit.curMovementBudget == 0)
            {
                StateMachine.VisualPlayfield.DisplayIndicatorAttackPreview(controlledUnit, StateMachine.Playfield);
            }
        }

        /// <summary>
        /// Determine if we're in a situation where we need to make a jump to either an exit or to another level.
        /// In these situations, we're done here so we don't want to bother with enemy turns, etc.
        /// </summary>
        private bool CheckForShortCircuitStateJump(PlayfieldUnit controlledUnit)
        {
            Vector2Int head = controlledUnit.locations[PlayfieldUnit.HEAD_INDEX];

            if (StateMachine.Playfield.exit != null)
            {
                if (head == StateMachine.Playfield.exit.location)
                {
                    StateMachine.SetState<Combat090Defeat>();
                    return true;
                }
            }

            if (StateMachine.Playfield.portals.Count > 0)
            {
                if (StateMachine.Playfield.TryGetPortalAt(head, out PlayfieldPortal portal))
                {
                    StateMachine.SetState<Combat100PortalWarp>();
                    return true;
                }
            }
            else
            {
                if (HasWinCondition())
                {
                    StateMachine.SetState<Combat080Victory>();
                    return true;
                }
            }

            return false;
        }
    }
}