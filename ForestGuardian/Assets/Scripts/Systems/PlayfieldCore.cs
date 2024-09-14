using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;
using Loam;

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
            /// Computer play turn.
            /// </summary>
            OpponentMove,

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

            switch(state)
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
            for(int i = 0; i < playfield.units.Count; i++)
            {
                PlayfieldUnit cur = playfield.units[i];
                Unit unit = lookup.GetUnityByTag(cur.tag).unitTemplate;
                cur.curMovementBudget = unit.moveSpeed;
                cur.curMaxSize = unit.maxSize;
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
            yield return new WaitForSeconds(artificalTurnDelay);
            SetState(TurnState.EvaluateTurn);
        }


        private bool HasEnemies()
        {
            for(int i = 0; i < playfield.units.Count; ++i)
            {
                PlayfieldUnit current = playfield.units[i];
                if(current.team != Team.Player)
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
                visualizerPlayfield.ShowMove(playerTmp, playfield);
                playerStateStartup = false;
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

        private void ProcessKeyboardInput(PlayfieldUnit toProcess)
        {
            Vector2Int head = toProcess.locations[PlayfieldUnit.HEAD_INDEX];
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                Vector2Int target = head + Vector2Int.down;
                if (visualizerPlayfield.IsValidImmediatelyPlaceToMoveTo(target))
                {
                    TryMoveUnitToLocation(toProcess, target);
                }
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                Vector2Int target = head + Vector2Int.up;
                if (visualizerPlayfield.IsValidImmediatelyPlaceToMoveTo(target))
                {
                    TryMoveUnitToLocation(toProcess, target);
                }
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Vector2Int target = head + Vector2Int.left;
                if (visualizerPlayfield.IsValidImmediatelyPlaceToMoveTo(target))
                {
                    TryMoveUnitToLocation(toProcess, target);
                }
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                Vector2Int target = head + Vector2Int.right;
                if (visualizerPlayfield.IsValidImmediatelyPlaceToMoveTo(target))
                {
                    TryMoveUnitToLocation(toProcess, target);
                }
            }
        }

        void PlayerMove_MsgMoveTileClicked(Message raw)
        {
            MsgIndicatorClicked msg = raw as MsgIndicatorClicked;

            if (msg.indicator.type != IndicatorType.ImmediateMove)
            {
                return;
            }

            PlayfieldUnit unit = msg.indicator.ownerUnit;
            Vector2Int target = msg.indicator.overlaidPosition;
            TryMoveUnitToLocation(unit, target);
        }

        private void TryMoveUnitToLocation(PlayfieldUnit unit, Vector2Int target)
        {
            // Step the unit to the new place. Ensure this happens before visualizer update.
            Utils.StepUnitTo(unit, playfield, target, moveCost: 1);

            if (playfield.TryGetItemAt(target, out PlayfieldItem item))
            {
                playfield.RemoveItemAt(target);
            }

            visualizerPlayfield.DisplayUnits(playfield);
            visualizerPlayfield.DisplayItems(playfield);
            visualizerPlayfield.ShowMove(unit, playfield);

            // NOTE: Check for going to next friendly movable target or just leave if no moves are allowed.
            // For now, if no moves, we're done.
            if (unit.curMovementBudget <= 0)
            {
                exitPlayerState = true;
            }
        }
    }
}