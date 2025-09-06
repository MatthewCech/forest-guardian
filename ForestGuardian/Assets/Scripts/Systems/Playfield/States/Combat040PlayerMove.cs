using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Hardware;
using UnityEditorInternal;
using UnityEngine;
using System.Linq;
using static UnityEngine.UI.CanvasScaler;
using Codice.Client.BaseCommands.BranchExplorer.Layout;

namespace forest
{
    /// <summary>
    /// USAGE: Per turn
    /// </summary>
    public class Combat040PlayerMove : CombatState
    {
        private MessageSubscription subMoveTileClicked = null;
        private MessageSubscription subPlayerClicked = null;

        private PlayfieldUnit currentUnit = null;

        public Combat040PlayerMove(PlayfieldCore stateMachine) : base(stateMachine) { }

        public bool SelectNextUnitToMove()
        {
            List<PlayfieldUnit> units = GetAvailablePlayerUnits();

            if (units == null || units.Count == 0)
            {
                currentUnit = null;
                return false;
            }

            currentUnit = units[0];
            DisplayPlayerUnitAction(currentUnit);

            return true;
        }

        /// <summary>
        /// Depending on how many moves are allowed for the unit and the state of the unit, display the
        /// appropriate indicator preview. 
        /// 
        /// NOTE: Unsure if this conditional will create edge cases if, say, a player unit is surrounded
        /// and has movement options free but doesn't have have space to do so.
        /// </summary>
        /// <param name="toShowActionOf"></param>
        private void DisplayPlayerUnitAction(PlayfieldUnit toShowActionOf)
        {
            if (toShowActionOf.curMovementBudget <= 0)
            {
                // Can't move or no movement? Try and attack.
                StateMachine.VisualPlayfield.DisplayIndicatorAttackPreview(toShowActionOf, StateMachine.Playfield);
            }
            else
            {
                // If we have the movement available, try and keep moving
                StateMachine.VisualPlayfield.DisplayIndicatorMovePreview(toShowActionOf, StateMachine.Playfield);
            }
        }

        public List<PlayfieldUnit> GetAvailablePlayerUnits()
        {
            return StateMachine.Playfield.units.Where((unit) => 
                unit.team == Team.Player && 
                unit.curHasPerformedActions == false).ToList();
        }

        public override void Start()
        {
            UnityEngine.Assertions.Assert.IsTrue(GetAvailablePlayerUnits().Count > 0, "It's impossible to start a player turn without units");

            subMoveTileClicked = Postmaster.Instance.Subscribe<MsgIndicatorClicked>(PlayerMoveAttack_MsgInteractionClicked);
            subPlayerClicked = Postmaster.Instance.Subscribe<MsgUnitPrimaryAction>(PlayerSelect_MsgUnitClicked);

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

        void PlayerSelect_MsgUnitClicked(Message raw)
        {
            MsgUnitPrimaryAction msg = raw as MsgUnitPrimaryAction;

            PlayfieldUnit unit = msg.unit.associatedData;
            Vector2Int gridPosClicked = msg.position;
            Vector2Int clickedUnitHeadPos = msg.unit.associatedData.locations[PlayfieldUnit.HEAD_INDEX];

            bool isHead = gridPosClicked == clickedUnitHeadPos;
            if (isHead)
            {
                if (unit.team == Team.Player)
                {
                    if (!unit.curHasPerformedActions)
                    {
                        StateMachine.VisualPlayfield.HideIndicators();
                        DisplayPlayerUnitAction(unit);
                    }
                }
                else
                {
                    StateMachine.VisualPlayfield.HideIndicators();
                    StateMachine.VisualPlayfield.DisplayIndicatorAttackPreview(unit, StateMachine.Playfield);
                }

                return;
            }
        }

        /// <summary>
        /// Receive a specific tile click event, handling immediate movement indicators specifically.
        /// In the event of an attack, we then handle damage and impact of the unit.
        /// </summary>
        /// <param name="raw"></param>
        void PlayerMoveAttack_MsgInteractionClicked(Message raw)
        {
            MsgIndicatorClicked msg = raw as MsgIndicatorClicked;
            PlayfieldUnit indicatorOwnerUnit = msg.indicator.ownerUnit;
            Unit visualUnit = StateMachine.VisualPlayfield.FindUnit(indicatorOwnerUnit);
            Vector2Int target = msg.indicator.overlaidPosition;

            // We're probably just previewing an attack, but either way we won't
            // allow any interaction here so just hide and reselect next player unit.
            if(indicatorOwnerUnit.team != Team.Player)
            {
                StateMachine.VisualPlayfield.HideIndicators();
                SelectNextUnitToMove();
                return;
            }

            // Move the unit about, but don't do an explicit clear of the preview - Instead just redraw.
            if (msg.indicator.type == IndicatorType.ImmediateMove)
            {
                Utils.MoveUnitToLocation(StateMachine.Playfield, StateMachine.VisualPlayfield, indicatorOwnerUnit, target);

                Postmaster.Instance.Send(new MsgUnitMoved { unit = visualUnit });

                CheckForShortCircuitStateJump(indicatorOwnerUnit);

                if (indicatorOwnerUnit.curMovementBudget == 0)
                {
                    StateMachine.VisualPlayfield.DisplayIndicatorAttackPreview(indicatorOwnerUnit, StateMachine.Playfield);
                }

                return;
            }

            // Attacking, friend or foe. Indicator no longer needed, but mark that we're done with the turn.
            if (msg.indicator.type == IndicatorType.Attack)
            {
                void WrapUpAttack()
                {
                    Postmaster.Instance.Send(new MsgUnitAttack { unit = visualUnit });
                    StateMachine.VisualPlayfield.HideIndicators();
                    indicatorOwnerUnit.curHasPerformedActions = true;

                    if (!SelectNextUnitToMove())
                    {
                        StateMachine.SetState<Combat050OpponentMove>();
                    }
                }

                bool isValidTarget = StateMachine.Playfield.TryGetUnitAt(target, out PlayfieldUnit targetUnit);
                if (isValidTarget)
                {
                    if (targetUnit.team == Team.Player)
                    {
                        Core.Instance.uiCore.DisplayCoDA("fr?", () =>
                        {
                            StateMachine.VisualPlayfield.DamageUnit(indicatorOwnerUnit, targetUnit, StateMachine.Playfield);
                            WrapUpAttack();
                        });

                        return;
                    }
                    else
                    {
                        StateMachine.VisualPlayfield.DamageUnit(indicatorOwnerUnit, targetUnit, StateMachine.Playfield);
                        WrapUpAttack();

                        return;
                    }
                }

                WrapUpAttack();

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