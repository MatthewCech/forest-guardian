using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{

    public class MapUIMenu : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private TMPro.TextMeshProUGUI instanceState;
        [SerializeField] private float timeBetweenUpdates = 0.25f;

        [Header("Links")]
        [SerializeField] private TMPro.TextMeshProUGUI currencyDisplay;

        [Header("Roster")]
        [SerializeField] private Button rosterButton;
        [SerializeField] private UIRoster roster;


        // Internal
        private float timeSoFar = 0;

        private void Start()
        {
            timeSoFar = timeBetweenUpdates;
        }

        // Update is called once per frame
        void Update()
        {
            timeSoFar += Time.deltaTime;

            if (timeSoFar > timeBetweenUpdates)
            {
                if (Core.Instance != null)
                {
                    instanceState.text = JsonUtility.ToJson(Core.Instance.gameData, true);
                }

                timeSoFar = 0;
            }
        }

        private void OnEnable()
        {
            rosterButton.onClick.AddListener(ToggleRosterVisibility);
        }

        private void OnDisable()
        {
            rosterButton.onClick.RemoveListener(ToggleRosterVisibility);
        }

        private void ToggleRosterVisibility()
        {
            roster.SetVisibility(!roster.IsVisible);
        }
    }
}
