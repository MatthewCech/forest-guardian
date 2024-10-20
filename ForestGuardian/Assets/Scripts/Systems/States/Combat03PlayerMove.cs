using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Hardware;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

namespace forest
{
    public class Combat03PlayerMove : CombatState
    {
        private MessageSubscription sub = null;

        public Combat03PlayerMove(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Start()
        {
            sub = Postmaster.Instance.Subscribe<MsgIndicatorClicked>(PlayerMove_MsgMoveTileClicked);

            // NOTE: Currently hard-coded. Need to select players piece by piece in the future.
            PlayfieldUnit playerTmp = StateMachine.Playfield.units[0];
            StateMachine.VisualPlayfield.DisplayIndicatorMovePreview(playerTmp, StateMachine.Playfield);
        }

        public override void Update()
        {
            PlayfieldUnit toProcess = StateMachine.Playfield.units[0];
            ProcessKeyboardInput(toProcess);
        }

        public override void Shutdown()
        {
            sub?.Dispose();
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
            Vector2Int target = msg.indicator.overlaidPosition;

            if (msg.indicator.type == IndicatorType.ImmediateMove)
            {
                Utils.MoveUnitToLocation(StateMachine.Playfield, StateMachine.VisualPlayfield, unit, target);
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

                StateMachine.VisualPlayfield.HideIndicators();
                StateMachine.SetState<Combat05OpponentMove>();
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
    }
}