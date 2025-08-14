using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{
    public class PlayfieldUI : MonoBehaviour
    {
        [Header("Lookups")]
        [SerializeField] private VisualLookup visualLookup;

        [Header("Public Links")]
        public Button buttonExit;
        public Button buttonJumpToPortal;
        public TMPro.TextMeshProUGUI currentState;
        public TMPro.TextMeshProUGUI result;

        [Header("Entry Handling")]
        public CanvasGroup placementGroup;
        public PlayfieldUISelectionEntry selectionEntryTemplate;
        public Transform selectionParent;
        public PlayfieldUIUnitDetails unitDetails;
        public Button startFloor;

        private List<PlayfieldUISelectionEntry> trackedEntries;

        private void Awake()
        {
            trackedEntries = new List<PlayfieldUISelectionEntry>();
            selectionEntryTemplate.gameObject.SetActive(false);
        }

        public void SetSelectorVisibility(bool isVisible)
        {
            if(isVisible)
            {
                placementGroup.alpha = 1;
                placementGroup.interactable = true;
                placementGroup.blocksRaycasts = true;
            }
            else
            {
                placementGroup.alpha = 0;
                placementGroup.interactable = false;
                placementGroup.blocksRaycasts = false;
            }
        }

        void Start()
        {
            List<UnitData> roster = Core.Instance.gameData.roster;
            for(int i = 0; i < roster.Count; ++i)
            {
                UnitData rosterEntry = roster[i];

                PlayfieldUISelectionEntry entry = Instantiate(selectionEntryTemplate, selectionParent);
                entry.gameObject.SetActive(true);

                Unit unitVisual = visualLookup.GetUnitTemplateByName(rosterEntry.unitName);
                entry.DisplayData(i, rosterEntry, unitVisual);

                trackedEntries.Add(entry);
            }
        }
    }
}