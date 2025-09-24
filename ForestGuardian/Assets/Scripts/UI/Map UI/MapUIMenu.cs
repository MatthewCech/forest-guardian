using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{

    public class MapUIMenu : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private Button dumpGameInstanceState;

        [Header("Links")]
        [SerializeField] private TMPro.TextMeshProUGUI currencyDisplay;
        [SerializeField] public Transform cameraPivot;

        [Header("Roster")]
        [SerializeField] private Button rosterButton;
        [SerializeField] private UIRoster roster;

        private void OnEnable()
        {
            rosterButton.onClick.AddListener(ToggleRosterVisibility);
            dumpGameInstanceState.onClick.AddListener(DumpInstanceStateToConsole);
        }

        private void OnDisable()
        {
            dumpGameInstanceState.onClick.RemoveListener(DumpInstanceStateToConsole);
            rosterButton.onClick.RemoveListener(ToggleRosterVisibility);
        }

        private void ToggleRosterVisibility()
        {
            roster.SetVisibility(!roster.IsVisible);
        }

        private void DumpInstanceStateToConsole()
        {
            Debug.Log("Game Instance State:\n" + JsonUtility.ToJson(Core.Instance.gameData, true));
        }
    }
}
