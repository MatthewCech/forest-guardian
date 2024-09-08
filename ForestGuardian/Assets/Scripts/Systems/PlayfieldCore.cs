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
        [SerializeField] private VisualLookup lookup;
        [SerializeField] private Playfield playfield;
        [SerializeField] private VisualPlayfield visualizerPlayfield;
        [SerializeField] private Camera mainCam;
        [SerializeField] private UIDocument ui;

        // Internal
        private TurnState turnState = TurnState.Startup;
        private bool executeState = false;

        void Start()
        {
            Postmaster.Instance.Configure(PostmasterConfig.Default());
            


            playfield = PlayfieldUtils.BuildPlayfield(PlayfieldUtils.testFile);
            visualizerPlayfield.Initialize(lookup);
            visualizerPlayfield.DisplayAll(playfield);

            Utils.CenterCamera(mainCam, visualizerPlayfield);
            SetState(TurnState.Startup);
        }

        public enum TurnState
        {
            Startup,
            PrepareTurn,
            PlayerMove,
            OpponentMove,
            Victory,
            Defeat,
        }

        private void Update()
        {
            if (executeState)
            {
                ProcessState(turnState);
            }
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
                    executeState = false;
                    break;
                case TurnState.Victory: break;
                case TurnState.Defeat: break;
            }
        }

        private IEnumerator Startup()
        {
            yield return new WaitForSeconds(0.33f);
            SetState(TurnState.PrepareTurn);
        }

        private IEnumerator PrepareTurn()
        {
            for(int i = 0; i < playfield.units.Count; i++)
            {
                PlayfieldUnit cur = playfield.units[i];
                Unit unit = lookup.GetUnityByTag(cur.tag).unitTemplate;
                cur.movesRemaining = unit.moveSpeed;
            }

            yield return null;
            SetState(TurnState.PlayerMove);
        }

        // Lmao
        private bool exitPlayerState = false;
        private bool playerStateStartup = true;
        private void PlayerMove()
        {
            // Eww eww ewwwwww get a proper state system, you - and do this on state enter
            MessageSubscription sub = null;
            if (playerStateStartup)
            {
                sub = Postmaster.Instance.Subscribe<MsgIndicatorClicked>(PlayerMove_MsgMoveTileClicked);
                PlayfieldUnit playerTmp = playfield.units[0];
                visualizerPlayfield.ShowMove(playerTmp, playfield);
                playerStateStartup = false;
            }

            if(exitPlayerState)
            {
                sub?.Dispose();
                exitPlayerState = false;
                playerStateStartup = true;
                SetState(TurnState.OpponentMove);
            }
        }
        void PlayerMove_MsgMoveTileClicked(Message raw)
        {
            MsgIndicatorClicked msg = raw as MsgIndicatorClicked;
            PlayfieldUnit unit = msg.indicator.ownerUnit;

            --unit.movesRemaining;
        }

        private void SetState(TurnState newState)
        {
            turnState = newState;
            ui.rootVisualElement.Q<Label>("bannerLabel").text = turnState.ToString();
            executeState = true;
        }
    }
}