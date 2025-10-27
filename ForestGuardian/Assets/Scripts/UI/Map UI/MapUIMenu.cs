using Newtonsoft.Json;
using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{

    public class MapUIMenu : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private Button buttonDumpGameInstanceState;
        [SerializeField] private Button buttonSelectAndLoadLevel;

        [Header("Links")]
        [SerializeField] private TMPro.TextMeshProUGUI currencyDisplay;
        [SerializeField] public Transform cameraPivot;

        [Header("Roster")]
        [SerializeField] private Button buttonRoster;
        [SerializeField] private UIRoster roster;

        private void OnEnable()
        {
            buttonRoster.onClick.AddListener(ToggleRosterVisibility);
            buttonDumpGameInstanceState.onClick.AddListener(DEBUG_DumpInstanceStateToConsole);
            buttonSelectAndLoadLevel.onClick.AddListener(DEBUG_SelectAndLoadLevel);
        }

        private void OnDisable()
        {
            buttonSelectAndLoadLevel.onClick.RemoveListener(DEBUG_SelectAndLoadLevel);
            buttonDumpGameInstanceState.onClick.RemoveListener(DEBUG_DumpInstanceStateToConsole);
            buttonRoster.onClick.RemoveListener(ToggleRosterVisibility);
        }

        private void ToggleRosterVisibility()
        {
            roster.SetVisibility(!roster.IsVisible);
        }

        private void DEBUG_SelectAndLoadLevel()
        {
            FileBrowser.ShowLoadDialog((paths) =>
            {
                string path = paths[0];
                JsonSerializer serializer = new JsonSerializer();
                using (StreamReader reader = new StreamReader(path))
                {
                    string parsedLevel = reader.ReadToEnd();
                    Core.Instance.SetPlayfieldAndLoad(parsedLevel);
                    Loam.Postmaster.Instance.Send(new MsgConvoEnd());
                }
            }, () => { }, FileBrowser.PickMode.Files, allowMultiSelection: false, Application.dataPath);
        }

        private void DEBUG_DumpInstanceStateToConsole()
        {
            Debug.Log("Game Instance State:\n" + JsonUtility.ToJson(Core.Instance.GameData, true));
        }
    }
}
