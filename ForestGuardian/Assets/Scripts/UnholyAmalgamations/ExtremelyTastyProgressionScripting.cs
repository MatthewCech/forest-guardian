using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Loam;

namespace forest
{
    public class ExtremelyTastyProgressionScripting : MonoBehaviour
    {
        // Inspector
        [SerializeField] private string initialDialog = "intro";
        [SerializeField] private string campDialog = "camp";

        // Internal
        private MessageSubscription subIntroFinished;
        private bool hasFoundFlag = false;
        private float messageStartDelay = 0.25f;

        void Start()
        {
            subIntroFinished = Postmaster.Instance.Subscribe<MsgConvoMessage>(ParseConvoMessage);

            TryRunAll();
        }

        private void TryRunAll()
        {
            StartFirstAvailable(GameInstance.FLAG_STORY_INTRO);
        }

        /// <summary>
        /// Run this so long as no other run to this point has found a not-yet-flipped flag.
        /// </summary>
        private void StartFirstAvailable(int flagToCheck)
        {
            if (hasFoundFlag)
            {
                return;
            }

            if (!Core.Instance.GameData.GetFlag(flagToCheck))
            {
                hasFoundFlag = true;
                StartCoroutine(DelayedStart(waitTime: messageStartDelay));
            }
        }

        private IEnumerator DelayedStart(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            Postmaster.Instance.Send(new MsgConvoStart() { convoName = initialDialog });
        }

        void OnDestroy()
        {
            subIntroFinished.Dispose();
        }

        private void ParseConvoMessage(Message raw)
        {
            MsgConvoMessage msg = raw as MsgConvoMessage;

            string convoMessage = msg.message.ToLower();
            Debug.Log("Checking message: " + convoMessage);
            switch (convoMessage)
            {
                case "introdone":
                    UnlockTutorialAndInitialLevels();
                    break;
            }
        }

        private void UnlockTutorialAndInitialLevels()
        {
            Core.Instance.GameData.UnlockLevel(GameInstance.LEVEL_TUTORIAL);
            Core.Instance.GameData.SetFlag(GameInstance.FLAG_STORY_INTRO);
        }
    }
}