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
        public TMPro.TextMeshProUGUI currentState;
        public TMPro.TextMeshProUGUI result;

        [Header("Entry Handling")]
        public PlayfieldUISelectionEntry selectionEntryTemplate;
        public Transform selectionParent;
        public PlayfieldUIUnitDetails unitDetails;

        private List<PlayfieldUISelectionEntry> trackedEntries;

        private void Awake()
        {
            trackedEntries = new List<PlayfieldUISelectionEntry>();
            selectionEntryTemplate.gameObject.SetActive(false);
        }

        void Start()
        {
            List<UnitData> roster = Core.Instance.gameData.roster;
            foreach(UnitData rosterEntry in roster)
            {
                PlayfieldUISelectionEntry entry = Instantiate(selectionEntryTemplate, selectionParent);
                entry.gameObject.SetActive(true);

                Unit unitVisual = visualLookup.GetUnitTemplateByName(rosterEntry.unitName);
                entry.DisplayData(rosterEntry, unitVisual);

                trackedEntries.Add(entry);
            }
        }
    }
}