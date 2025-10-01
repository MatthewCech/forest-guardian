using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class UIRoster : MonoBehaviour
    {
        [Header("General Links")]
        [SerializeField] private VisualLookup visualLookup;

        [Header("UI Links")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private UIRosterEntry rosterEntryTemplate;
        [SerializeField] private Transform rosterEntryParent;
        
        // Internal only
        private List<UIRosterEntry> trackedRosterEntries;

        // Properties
        public bool IsVisible { get; private set; }

        private void Awake()
        {
            rosterEntryTemplate.gameObject.SetActive(false);
            trackedRosterEntries = new List<UIRosterEntry>();
            Hide();
        }

        private void Start()
        {
            List<UnitData> roster = Core.Instance.GameData.roster;

            foreach (UnitData data in roster)
            {
                UIRosterEntry rosterEntry = Instantiate(rosterEntryTemplate, rosterEntryParent);
                Unit unitVisual = visualLookup.GetUnitTemplateByName(data.unitName);
                rosterEntry.UpdateAll(data, unitVisual);

                rosterEntry.gameObject.SetActive(true);
                trackedRosterEntries.Add(rosterEntry); 
            }
        }

        public void SetVisibility(bool isVisible)
        {
            if (isVisible)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void Hide()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            IsVisible = false;
        }

        public void Show()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            IsVisible = true;
        }
    }
}