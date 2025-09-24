using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{
    public class PlayfieldUISelectionEntry : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI unitName;
        [SerializeField] private TMPro.TextMeshProUGUI unitSpeed;
        [SerializeField] private TMPro.TextMeshProUGUI unitSize;
        [SerializeField] private TMPro.TextMeshProUGUI unitMoves;
        [SerializeField] private Image unitIcon;
        [SerializeField] private Button unitButton;

        public void OnEnable()
        {
            unitButton.onClick.AddListener(NotifyUnitClicked);
        }

        public void OnDisable()
        {
            unitButton.onClick.RemoveListener(NotifyUnitClicked);
        }

        private void NotifyUnitClicked()
        {
            Loam.Postmaster.Instance.Send(new MsgRosterUnitIndicated() { rosterIndex = readOnlyRosterIndex });
        }

        private int readOnlyRosterIndex;

        public void DisplayData(int rosterIndex, UnitData data, Unit visual)
        {
            readOnlyRosterIndex = rosterIndex;

            unitName.text = data.unitName;
            unitSpeed.text = data.speed.ToString();
            unitSize.text = data.maxSize.ToString();
            unitMoves.text = "";
            unitIcon.sprite = visual.uiIcon;

            foreach(MoveData move in data.moves)
            {
                unitMoves.text += $"{move.moveName}\n";
            }
        }
    }
}