using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;
using Loam;
using static UnityEngine.UI.CanvasScaler;

namespace forest
{
    public class PlayfieldCore : MonoBehaviour
    {
        [SerializeField] private TextAsset levelData;
        [SerializeField] private VisualLookup lookup;
        [SerializeField] private Playfield playfield;
        [SerializeField] private VisualPlayfield visualizerPlayfield;
        [SerializeField] private Camera mainCam;
        [SerializeField] private UIDocument ui;

        // Internal
        private TurnState turnState = TurnState.Startup;
        private bool executeState = false;
        private float artificalTurnDelay = 0.2f; // seconds
        private float winLoseScreenShowTime = 3f; // seconds

        void Start()
        {
            Postmaster.Instance.Configure(PostmasterConfig.Default());

            playfield = PlayfieldUtils.BuildPlayfield(levelData.text);
            visualizerPlayfield.Initialize(lookup);
            visualizerPlayfield.DisplayAll(playfield);

            Utils.CenterCamera(mainCam, visualizerPlayfield);
            SetState(TurnState.Startup);
        }

        public enum TurnState
        {
            Startup,

            /// <summary>
            /// Get everything ready for a turn
            /// </summary>
            PrepareTurn,

            /// <summary>
            /// Player-controlled input! Yay!
            /// </summary>
            PlayerMove,

            /// <summary>
            /// Player-controlled attacking input
            /// </summary>
            PlayerAttack,

            /// <summary>
            /// Computer play turn.
            /// </summary>
            OpponentMove,

            /// <summary>
            /// Computer attach turn.
            /// </summary>
            OpponentAttack,

            /// <summary>
            /// Did someone win? Someone lose? something go on that needs correcting?
            /// Some sort of end-of-turn logic? This is the place!
            /// </summary>
            EvaluateTurn,

            /// <summary>
            /// If the player won
            /// </summary>
            Victory,

            /// <summary>
            /// If the player lost
            /// </summary>
            Defeat,

            /// <summary>
            /// Clean up, we're done. Shut the scene down, recording and tucking
            /// anything we need to away.
            /// </summary>
            Shutdown
        }

        private void Update()
        {
            if (executeState)
            {
                ProcessState(turnState);
            }

            Postmaster.Instance.Upkeep();
        }

        public void ProcessState(TurnState state)
        {
            switch (state)
            {
                case TurnState.Startup:
                    StartCoroutine(Startup());
                    executeState = false;
                    break;

                case TurnState.PrepareTurn:
                    StartCoroutine(PrepareTurn());
                    executeState = false;
                    break;

                case TurnState.PlayerMove:
                    PlayerMove();
                    break;

                case TurnState.OpponentMove:
                    StartCoroutine(OpponentMove());
                    executeState = false;
                    break;

                case TurnState.EvaluateTurn:
                    StartCoroutine(EvaluateTurn());
                    executeState = false;
                    break;

                case TurnState.Victory:
                    StartCoroutine(Victory());
                    executeState = false;
                    break;

                case TurnState.Defeat:
                    StartCoroutine(Defeat());
                    executeState = false;
                    break;

                case TurnState.Shutdown:
                    StartCoroutine(Shutdown());
                    executeState = false;
                    break;
            }
        }

        private void SetState(TurnState newState)
        {
            turnState = newState;
            ui.rootVisualElement.Q<Label>("bannerLabel").text = turnState.ToString();
            executeState = true;
        }

        private IEnumerator Startup()
        {
            yield return new WaitForSeconds(artificalTurnDelay);
            SetState(TurnState.PrepareTurn);
        }

        private IEnumerator PrepareTurn()
        {
            for (int i = 0; i < playfield.units.Count; i++)
            {
                PlayfieldUnit cur = playfield.units[i];
                Unit template = lookup.GetUnityByTag(cur.tag).unitTemplate;
                cur.curMovementBudget = template.moveSpeed;
                cur.curMaxSize = template.maxSize;
            }

            yield return null;
            SetState(TurnState.PlayerMove);
        }

        /// <summary>
        /// Want to see PlayerMove? Scroll down for the real one, it's horrid lol
        /// </summary>
        /// <returns></returns>
        private IEnumerator PlayerMoveStub() { yield return null; }

        private IEnumerator OpponentMove()
        {
            const float visualDisplayDelay = .3f;
            const float visualMoveDelay = .1f;

            if (!TryGetOpponentsTarget(out PlayfieldUnit targeted))
            {
                // If we're here, we're not gonna get anything done
                // since there are no players yet.
                SetState(TurnState.EvaluateTurn);
                yield break;
            }

            yield return null;
            for (int unitIndex = 0; unitIndex < playfield.units.Count; ++unitIndex)
            {
                PlayfieldUnit curOpponentToMove = playfield.units[unitIndex];
                if (curOpponentToMove.team != Team.Opponent)
                {
                    continue;
                }

                visualizerPlayfield.DisplayIndicatorMovePreview(curOpponentToMove, playfield);

                yield return new WaitForSeconds(visualDisplayDelay);

                while (curOpponentToMove.curMovementBudget > 0)
                {
                    if(!TryStepOnceTowardsPlayerUnit(targeted, curOpponentToMove))
                    {
                        break;
                    }


                    yield return new WaitForSeconds(visualMoveDelay);
                }

                visualizerPlayfield.DisplayIndicatorAttackPreview(curOpponentToMove, playfield);

                yield return new WaitForSeconds(visualDisplayDelay);

                Vector2Int head = curOpponentToMove.locations[PlayfieldUnit.HEAD_INDEX];
                Vector2Int closest = GetClosestOpponentPosition(curOpponentToMove);

                if (head.GridDistance(closest) <= curOpponentToMove.curAttackRange)
                {
                    if (playfield.TryGetUnitAt(closest, out PlayfieldUnit targetPlayerUnit))
                    {
                        visualizerPlayfield.DamageUnit(curOpponentToMove, targetPlayerUnit, playfield);
                    }
                }

                visualizerPlayfield.HideIndicators();


                yield return new WaitForSeconds(visualDisplayDelay);
            }

            SetState(TurnState.EvaluateTurn);
        }

        private Vector2Int GetClosestOpponentPosition(PlayfieldUnit curOpponent)
        {
            Vector2Int closestPos = new Vector2Int(-short.MaxValue, -short.MaxValue); // A wild distance
            Vector2Int head = curOpponent.locations[PlayfieldUnit.HEAD_INDEX];

            for (int i = 0; i < playfield.units.Count; ++i)
            {
                PlayfieldUnit unit = playfield.units[i];
                if(unit.team == Team.Player)
                {
                    for(int loc = 0; loc < unit.locations.Count; ++loc)
                    {
                        Vector2Int curLoc = unit.locations[loc];
                        if(head.GridDistance(curLoc) < head.GridDistance(closestPos))
                        {
                            closestPos = curLoc;
                        }
                    }
                }
            }

            return closestPos;
        }

        private bool TryStepOnceTowardsPlayerUnit(PlayfieldUnit targeted, PlayfieldUnit curOpponentToMove)
        {
            Vector2Int curHead = curOpponentToMove.locations[PlayfieldUnit.HEAD_INDEX];
            Vector2Int targetHead = targeted.locations[PlayfieldUnit.HEAD_INDEX];

            int xDif = curHead.x - targetHead.x;
            int yDif = curHead.y - targetHead.y;

            // Recall that upper-left is 0, 0
            Vector2Int yDir = Vector2Int.zero;
            Vector2Int xDir = Vector2Int.zero;

            if (yDif > 0) { yDir = Vector2Int.down; }
            else if (yDif < 0) { yDir = Vector2Int.up; }

            if (xDif > 0) { xDir = Vector2Int.left; }
            else if (xDif < 0) { xDir = Vector2Int.right; }

            Vector2Int toUse = Mathf.Abs(yDif) > Mathf.Abs(xDif) ? yDir : xDir;

            Vector2Int targetPos = curHead + toUse;
            if (Utils.CanUnitMoveTo(playfield, curOpponentToMove, targetPos))
            {
                MoveUnitToLocation(curOpponentToMove, targetPos);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryGetOpponentsTarget(out PlayfieldUnit targeted)
        {
            for (int i = 0; i < playfield.units.Count; ++i)
            {
                PlayfieldUnit cur = playfield.units[i];
                if (cur.team == Team.Player)
                {
                    targeted = playfield.units[i];
                    return true;
                }
            }

            targeted = null;
            return false;
        }

        private bool HasEnemies()
        {
            for (int i = 0; i < playfield.units.Count; ++i)
            {
                PlayfieldUnit current = playfield.units[i];
                if (current.team != Team.Player)
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerator EvaluateTurn()
        {
            yield return new WaitForSeconds(artificalTurnDelay);

            // Win value, in this case I've hardcoded to be "No items left"
            if (playfield.items.Count == 0 && !HasEnemies())
            {
                SetState(TurnState.Victory);
            }
            else
            {
                SetState(TurnState.PrepareTurn);
            }
        }

        private IEnumerator Victory()
        {
            yield return new WaitForSeconds(3);
            SetState(TurnState.Shutdown);
        }

        private IEnumerator Defeat()
        {
            yield return new WaitForSeconds(3);
            SetState(TurnState.Shutdown);
        }

        private IEnumerator Shutdown()
        {
            yield return new WaitForSeconds(artificalTurnDelay);
            throw new System.Exception("Signal for scene unload or something");
        }





        // Lol, lmao even.
        private bool exitPlayerState = false;
        private bool playerStateStartup = true;
        MessageSubscription sub = null;

        private void PlayerMove()
        {
            // TODO: Ewwww make a proper class based state system and do this on state enter
            // Not the highest priority but a real important thing long term for sanity.
            if (playerStateStartup)
            {
                sub = Postmaster.Instance.Subscribe<MsgIndicatorClicked>(PlayerMove_MsgMoveTileClicked);
                // NOTE: Currently hardcoded. Need to select players piece by piece in the future.
                PlayfieldUnit playerTmp = playfield.units[0];
                visualizerPlayfield.DisplayIndicatorMovePreview(playerTmp, playfield);
                playerStateStartup = false;
                exitPlayerState = false;
            }

            PlayfieldUnit toProcess = playfield.units[0];

            ProcessKeyboardInput(toProcess);

            // TODO: See above TODO on class-based state system. 
            if (exitPlayerState)
            {
                sub.Dispose();
                exitPlayerState = false;
                playerStateStartup = true;
                SetState(TurnState.OpponentMove);
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
                if (Utils.CanUnitMoveTo(playfield, controlledUnit, target))
                {
                    MoveUnitToLocation(controlledUnit, target);
                }
            }
        }

        private void ProcessKeyboardInput(PlayfieldUnit controlledUnit)
        {
            TryMoveOnKeyInput(controlledUnit, Vector2Int.down, KeyCode.W, KeyCode.UpArrow);
            TryMoveOnKeyInput(controlledUnit, Vector2Int.up, KeyCode.S, KeyCode.DownArrow);
            TryMoveOnKeyInput(controlledUnit, Vector2Int.left, KeyCode.A, KeyCode.LeftArrow);
            TryMoveOnKeyInput(controlledUnit, Vector2Int.right, KeyCode.D, KeyCode.RightArrow);
        }

        void PlayerMove_MsgMoveTileClicked(Message raw)
        {
            MsgIndicatorClicked msg = raw as MsgIndicatorClicked;
            PlayfieldUnit unit = msg.indicator.ownerUnit;
            Vector2Int target = msg.indicator.overlaidPosition;

            if (msg.indicator.type == IndicatorType.ImmediateMove)
            {
                MoveUnitToLocation(unit, target);
                if (unit.curMovementBudget == 0)
                {
                    visualizerPlayfield.DisplayIndicatorAttackPreview(unit, playfield);
                }
            }

            if (msg.indicator.type == IndicatorType.Attack)
            {
                PlayfieldTile clickedTile = playfield.world.Get(target);
                if (playfield.TryGetUnit(clickedTile.associatedUnitID, out PlayfieldUnit targetUnit))
                {
                    visualizerPlayfield.DamageUnit(unit, targetUnit, playfield);
                    exitPlayerState = true;
                }
                else
                {
                    exitPlayerState = true;
                }

                visualizerPlayfield.HideIndicators();
            }
        }

        private void MoveUnitToLocation(PlayfieldUnit unit, Vector2Int target)
        {
            // Step the unit to the new place. Ensure this happens before visualizer update.
            Utils.StepUnitTo(unit, playfield, target, moveCost: 1);

            if (playfield.TryGetItemAt(target, out PlayfieldItem item))
            {
                playfield.RemoveItemAt(target);
            }

            visualizerPlayfield.DisplayUnits(playfield);
            visualizerPlayfield.DisplayItems(playfield);
            visualizerPlayfield.DisplayIndicatorMovePreview(unit, playfield);
        }
    }
}