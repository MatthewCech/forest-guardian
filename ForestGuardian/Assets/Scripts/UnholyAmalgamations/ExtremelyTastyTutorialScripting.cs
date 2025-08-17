using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace forest
{

    public class ExtremelyTastyTutorialScripting : MonoBehaviour
    {
        public enum TutorialState
        {
            // Error state
            NONE = 0,

            // Lore stuff
            Intro_01,
            Intro_02,

            // Walking through setup
            SelectFirstOrigin,
            SelectGuardianInRoster,
            PostPlaceFirstUnit,
            PlaceSecondUnit,
            SelectStartButton,

            // Walking through a regular player turn
            // #TODO: Fill out

            // General
            Startup,
            Waiting,
            Disable
        }

        [Header("Things to stare at and reach into")]
        [SerializeField] private TextAsset levelToMatch;
        [SerializeField] private PlayfieldCore playfieldCore; // Used just for name really

        [Header("UI")]
        [SerializeField] private CanvasGroup tutorialDisplay;
        [SerializeField] private TMPro.TextMeshProUGUI messageText;
        [SerializeField] private UnityEngine.UI.Button next;
        [SerializeField] private GameObject playfieldIndicator;
        [SerializeField] private GameObject uiIndicator;

        [Header("Debug")]
        [SerializeField] private TutorialState curState = TutorialState.NONE;
        [SerializeField] private TutorialState prevState = TutorialState.NONE;

        // Internal only
        private bool isNextStateRequested;
        private bool isStateStart;
        private Loam.MessageSubscription stateOnlyMsgHandle_01 = null;

        void Awake()
        {
            isNextStateRequested = false;
            isStateStart = false;

            DisableMessageBox();
            SetState(TutorialState.Startup);
        }

        private void NextPressed()
        {
            isNextStateRequested = true;
        }

        private void OnEnable()
        {
            next.onClick.AddListener(NextPressed);
        }

        private void OnDisable()
        {
            next.onClick.RemoveListener(NextPressed);
        }

        /// <summary>
        /// Set the next and previous state variables and reset the state of various tutorial elements.
        /// </summary>
        /// <param name="target">State to move to</param>
        void SetState(TutorialState target)
        {
            prevState = curState;
            curState = target;
            isStateStart = true;

            if(playfieldCore != null && playfieldCore.UI != null)
            {
                uiIndicator.gameObject.transform.SetParent(playfieldCore.UI.transform);
                uiIndicator.gameObject.transform.position = Vector3.zero;
            }

            next.gameObject.SetActive(true);
            uiIndicator.gameObject.SetActive(false);
            playfieldIndicator.gameObject.SetActive(false);

            if (stateOnlyMsgHandle_01 != null)
            {
                stateOnlyMsgHandle_01.Dispose();
                stateOnlyMsgHandle_01 = null;
            }
        }

        void EnableMessageBox()
        {
            tutorialDisplay.alpha = 1;
            tutorialDisplay.interactable = true;
            tutorialDisplay.blocksRaycasts = true;
        }

        void DisableMessageBox()
        {
            tutorialDisplay.alpha = 0;
            tutorialDisplay.interactable = false;
            tutorialDisplay.blocksRaycasts = false;
        }

        void ShowMessage(string message)
        {
            messageText.text = message;
            EnableMessageBox();
        }

        /// <summary>
        /// Provide a conditional check that only returns true when we've indicated we want to proceed
        /// to the next state.
        /// </summary>
        bool TryConsumeProceedToNextRequest()
        {
            bool toReturn = isNextStateRequested;
            isNextStateRequested = false;

            return toReturn;
        }

        /// <summary>
        /// Provide a conditional check that only returns true once per state change.
        /// </summary>
        bool TryConsumeStateInit()
        {
            bool toReturn = isStateStart;
            isStateStart = false;

            return toReturn;
        }

        // Update is called once per frame
        void Update()
        {
            switch (curState)
            {
                case TutorialState.Startup:
                    if (string.Equals(playfieldCore.PlayfieldName, levelToMatch.name))
                    {
                        SetState(TutorialState.Intro_01);
                    }
                    else
                    {
                        SetState(TutorialState.Disable);
                    }

                    break;

                case TutorialState.Intro_01:
                    if (TryConsumeStateInit())
                    {
                        ShowMessage("Welcome to your first <forest battle>?! What you see here is an <overgrown grove>?\n\n(Press next to continue...)");
                    }
                    if (TryConsumeProceedToNextRequest())
                    {
                        SetState(TutorialState.Intro_02);
                    }
                    break;

                case TutorialState.Intro_02:
                    if (TryConsumeStateInit())
                    {
                        ShowMessage("To <grow into and rebalance>? a space, we first need to select what <plants/units>? to use.");
                    }
                    if (TryConsumeProceedToNextRequest())
                    {
                        SetState(TutorialState.SelectFirstOrigin);
                    }
                    break;

                case TutorialState.SelectFirstOrigin:
                    const int ORIGIN_INDEX = 0;

                    if (TryConsumeStateInit())
                    {
                        void OriginSelected(Loam.Message raw)
                        {
                            MsgOriginPrimaryAction msg = raw as MsgOriginPrimaryAction;
                            bool indicatedOriginSelected = msg.position == playfieldCore.Playfield.origins[ORIGIN_INDEX].location;

                            if (indicatedOriginSelected)
                            {
                                isNextStateRequested = true;
                            }
                        }

                        next.gameObject.SetActive(false);
                        playfieldIndicator.gameObject.SetActive(true);
                        playfieldIndicator.gameObject.transform.position =
                            playfieldCore.VisualPlayfield.FindOrigin(
                                playfieldCore.Playfield.origins[ORIGIN_INDEX].location).gameObject.transform.position;

                        stateOnlyMsgHandle_01 = Loam.Postmaster.Instance.Subscribe<MsgOriginPrimaryAction>(OriginSelected);

                        ShowMessage("You can do this by selecting the <space>?...");
                    }

                    if (TryConsumeProceedToNextRequest())
                    {
                        SetState(TutorialState.SelectGuardianInRoster);
                    }

                    break;

                case TutorialState.SelectGuardianInRoster:
                    if (TryConsumeStateInit())
                    {
                        const int ROSTER_INDEX = 0;

                        void RosterEntrySelected(Loam.Message raw)
                        {
                            MsgRosterUnitIndicated msg = raw as MsgRosterUnitIndicated;
                            bool rosterIndexMatches = msg.rosterIndex == ROSTER_INDEX;

                            if (rosterIndexMatches)
                            {
                                isNextStateRequested = true;
                            }
                        }

                        next.gameObject.SetActive(false);
                        uiIndicator.gameObject.SetActive(true);
                        uiIndicator.gameObject.transform.SetParent(playfieldCore.UI.RosterEntries[ROSTER_INDEX].transform);
                        uiIndicator.gameObject.transform.localPosition = new Vector3(35, -70, 0);
                        stateOnlyMsgHandle_01 = Loam.Postmaster.Instance.Subscribe<MsgRosterUnitIndicated>(RosterEntrySelected);

                        ShowMessage("...Then by selecting the <unit>? we want to place.");
                    }

                    if (TryConsumeProceedToNextRequest())
                    {
                        SetState(TutorialState.PostPlaceFirstUnit);
                    }

                    break;

                case TutorialState.PostPlaceFirstUnit:
                    if (TryConsumeStateInit())
                    {
                        ShowMessage("Good work!\n\nYou can change around <plants/units>? as much as you want before starting the <battle>?.\n\nJust know that you, the <b>Guardian</b> MUST be on the field to command other <plants/units>? !.");
                    }

                    if (TryConsumeProceedToNextRequest())
                    {
                        SetState(TutorialState.PlaceSecondUnit);
                    }

                    break;

                case TutorialState.PlaceSecondUnit:

                    const int SECOND_SELECT_ORIGIN_INDEX = 1;
                    const int SECOND_SELECT_ROSTER_INDEX = 1;

                    if (TryConsumeStateInit())
                    {
                        ShowMessage("Lets add a second <unit>?, just in case.\n\nJust because we're filling all <start spots>? here doesn't mean you have to in levels though!");

                        next.gameObject.SetActive(false);
                        playfieldIndicator.gameObject.SetActive(true);
                        uiIndicator.gameObject.SetActive(true);

                        playfieldIndicator.gameObject.transform.position =
                            playfieldCore.VisualPlayfield.FindOrigin(
                                playfieldCore.Playfield.origins[SECOND_SELECT_ORIGIN_INDEX].location).gameObject.transform.position;

                        uiIndicator.gameObject.transform.SetParent(playfieldCore.UI.RosterEntries[SECOND_SELECT_ROSTER_INDEX].transform);
                        uiIndicator.gameObject.transform.localPosition = new Vector3(35, -70, 0);
                    }

                    if (playfieldCore.Playfield.origins[SECOND_SELECT_ORIGIN_INDEX].curRosterIndex != PlayfieldOrigin.NO_INDEX_SELECTED)
                    {
                        SetState(TutorialState.SelectStartButton);
                    }
                    break;

                case TutorialState.SelectStartButton:
                    if (TryConsumeStateInit())
                    {
                        void FloorStartRequest(Loam.Message raw)
                        {
                            isNextStateRequested = true;
                        }

                        next.gameObject.SetActive(false);
                        uiIndicator.gameObject.SetActive(true);

                        stateOnlyMsgHandle_01 = Loam.Postmaster.Instance.Subscribe<MsgFloorStarted>(FloorStartRequest);
                        uiIndicator.gameObject.transform.SetParent(playfieldCore.UI.startFloor.transform);
                        uiIndicator.gameObject.transform.localPosition = new Vector3(70, -10, 0);
                        ShowMessage("Now that we're ready, lets go ahead and start!");
                    }

                    if (TryConsumeProceedToNextRequest())
                    {
                        // #TODO: Fill out
                        SetState(TutorialState.Waiting);
                    }

                    break;

                case TutorialState.Waiting:
                    // This area intentionally left blank
                    break;

                case TutorialState.Disable:
                default:
                    if (TryConsumeStateInit())
                    {
                        playfieldIndicator.gameObject.SetActive(false);
                        uiIndicator.gameObject.SetActive(false);
                        this.gameObject.SetActive(false);
                    }
                    break;
            }
        }
    }
}