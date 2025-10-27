using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Loam.Convo;
using Loam;

namespace forest
{
    public class DialogueUICore : MonoBehaviour
    {
        [Header("Dialog UI")]
        [SerializeField] private ConvoBasicDialogUI _dialog;

        [Header("Data")]
        [SerializeField] private List<TextAsset> _conversations;

        private MessageSubscription _subscriptionStart;
        private MessageSubscription _subscriptionEnd;

        public void Initialize()
        {
            _dialog.gameObject.SetActive(true);
            _dialog.Hide();
        }

        public void Start()
        {
            _dialog.ConversationSystem.OnEnd += EndConversation;
            _dialog.ConversationSystem.OnMessage += ProcessMessage;

            EndConversation();
        }

        private void ProcessMessage(string input)
        {
            Postmaster.Instance.Send(new MsgConvoMessage() { message = input });
            Debug.Log(input);
        }

        private void OnEnable()
        {
            _subscriptionStart = Postmaster.Instance.Subscribe<MsgConvoStart>(StartConvoFromAssetMessage);
            _subscriptionEnd = Postmaster.Instance.Subscribe<MsgConvoEnd>(EndConvoMessage);
        }

        private void OnDisable()
        {
            _subscriptionEnd.Dispose();
            _subscriptionStart.Dispose();
        }

        private void StartConvoFromAssetMessage(Message raw)
        {
            MsgConvoStart msg = raw as MsgConvoStart;

            StartByName(msg.convoName);
        }

        private void EndConvoMessage(Message raw)
        {
            MsgConvoEnd msg = raw as MsgConvoEnd;

            EndConversation();
        }

        private void StartByName(string convoName)
        {
            for (int i = 0; i < _conversations.Count; ++i)
            {
                string name = _conversations[i].name;
                if (convoName.Equals(name))
                {
                    StartConversation(_conversations[i].text);
                    return;
                }
            }
        }

        public void StartConversation(string conversationJSON)
        {
            _dialog.Show(conversationJSON);
        }

        private void EndConversation()
        {
            _dialog.Hide();
        }
    }
}