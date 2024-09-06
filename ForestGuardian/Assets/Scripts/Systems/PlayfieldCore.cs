using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.CanvasScaler;

namespace forest
{
    public class PlayfieldCore : MonoBehaviour
    {
        [SerializeField] private Lookup lookup;
        [SerializeField] private Playfield playfield;
        [SerializeField] private VisualPlayfield visualizerPlayfield;
        [SerializeField] private Camera mainCam;
        [SerializeField] private UIDocument ui;

        // Internal
        private TurnState turnState = TurnState.Startup;
        private bool executeState = false;

        void Start()
        {
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
                    StartCoroutine(PlayerMove());
                    executeState = false; 
                    break;

                case TurnState.OpponentMove: break;
                case TurnState.Victory: break;
                case TurnState.Defeat: break;
            }
        }

        private IEnumerator Startup()
        {
            yield return new WaitForSeconds(1);
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

        private IEnumerator PlayerMove()
        {
            PlayfieldUnit playerTmp = playfield.units[0];
            visualizerPlayfield.ShowMove(playerTmp);
            yield return null;
        }

        private void SetState(TurnState newState)
        {
            turnState = newState;
            ui.rootVisualElement.Q<Label>("bannerLabel").text = turnState.ToString();
            executeState = true;
        }
    }
}