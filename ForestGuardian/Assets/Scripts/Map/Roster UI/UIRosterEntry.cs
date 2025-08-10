using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{
    public class UIRosterEntry : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI unitName;
        public TMPro.TextMeshProUGUI unitLevel;

        public TMPro.TextMeshProUGUI unitSize;
        public Button unitSizeIncrease;
        public TMPro.TextMeshProUGUI unitSpeed;
        public Button unitSpeedIncrease;

        public Image unitIcon;

        public UIRosterMoveEntry moveEntryTemplate;
        public Transform moveEntryParent;

        private string unitInternalName;
        private List<UIRosterMoveEntry> trackedMoveEntries = new List<UIRosterMoveEntry>();

        private void Awake()
        {
            moveEntryTemplate.gameObject.SetActive(false);
        }

        public void UpdateAll(UnitData data, Unit visual)
        {
            UpdateData(data);
            UpdateVisuals(visual);
        }

        // Strip and track data. We can't assume there's nothing here, so clear anything that exists already.
        private void UpdateData(UnitData data)
        {
            unitInternalName = data.unitName;
            unitName.text = data.unitName;
            unitSize.text = data.maxSize.ToString();
            unitSpeed.text = data.speed.ToString();
            unitLevel.text = data.level.ToString();

            // Clear anything that might exist before populating moves. This is perf inefficient
            // and may cause visual stutter, but is guaranteed to be accurate at least. It can be improved by
            // more closely following events that change move stats.
            foreach (UIRosterMoveEntry entry in trackedMoveEntries)
            {
                Destroy(entry.gameObject);
            }
            trackedMoveEntries.Clear();

            foreach (MoveData move in data.moves)
            {
                UIRosterMoveEntry moveEntry = Instantiate(moveEntryTemplate, moveEntryParent);
                moveEntry.moveName.text = move.moveName;
                moveEntry.moveDamage.text = move.moveDamage.ToString();
                moveEntry.moveRange.text = move.moveRange.ToString();

                moveEntry.gameObject.SetActive(true);
                trackedMoveEntries.Add(moveEntry);
            }
        }

        // Strip and track visuals. While the visual may have some data, it's not guaranteed to be
        // up to date (and often is only the initial values) so we should use it only for visual info.
        private void UpdateVisuals(Unit visual)
        {
            unitIcon.sprite = visual.uiIcon;
        }

        private void OnEnable()
        {
            unitSizeIncrease.onClick.AddListener(IncreaseUnitSize);
            unitSpeedIncrease.onClick.AddListener(IncreaseUnitSpeed);
        }

        private void OnDisable()
        {
            unitSpeedIncrease.onClick.RemoveListener(IncreaseUnitSpeed);
            unitSizeIncrease.onClick.RemoveListener(IncreaseUnitSize);
        }

        private void IncreaseUnitSize()
        {
            UnitData unit = Core.Instance.gameData.GetRosterEntry(unitInternalName);
            ++unit.maxSize;
            UpdateData(unit);
        }

        private void IncreaseUnitSpeed()
        {
            UnitData unit = Core.Instance.gameData.GetRosterEntry(unitInternalName);
            ++unit.speed;
            UpdateData(unit);
        }
    }
}