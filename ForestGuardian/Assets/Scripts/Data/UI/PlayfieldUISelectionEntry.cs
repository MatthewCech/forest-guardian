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
        [SerializeField] private Image unitIcon;

        [SerializeField] private PlayfieldUISelectionEntryMove move0;
        [SerializeField] private PlayfieldUISelectionEntryMove move1;
        [SerializeField] private PlayfieldUISelectionEntryMove move2;
        [SerializeField] private PlayfieldUISelectionEntryMove move3;

        public void DisplayData(UnitData data, Unit visual)
        {
            unitName.text = data.unitName;
            unitSpeed.text = data.speed.ToString();
            unitSize.text = data.maxSize.ToString();

            unitIcon.sprite = visual.uiIcon;

            int moves = data.moves.Count;
            if (moves > 3)
            {
                move3.AssignMove(data.moves[3]);
            }
            if (moves > 2)
            {
                move2.AssignMove(data.moves[2]);
            }
            if (moves > 1)
            {
                move1.AssignMove(data.moves[1]);
            }
            if (moves > 0)
            {
                move0.AssignMove(data.moves[0]);
            }
        }
    }
}