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

        // Internal
        private bool didFirstUpdate = false;
        private MessageSubscription subIntroFinished;

        void Start()
        {
            subIntroFinished = Postmaster.Instance.Subscribe<MsgConvoMessage>(ParseConvoMessage);
            StartCoroutine(DelayedStart(waitTime: 1f));
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
                    UnlockTutorial();
                    break;
            }
        }

        private void UnlockTutorial()
        {
            Core.Instance.GameData.UnlockLevel("tutorial");
            Core.Instance.GameData.UnlockLevel("ivy-grove");
        }
    }
}