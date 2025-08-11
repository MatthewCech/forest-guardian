using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class PlayfieldUIUnitDetails : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;
        public TMPro.TextMeshProUGUI moveName;
        public TMPro.TextMeshProUGUI moveRange;
        public TMPro.TextMeshProUGUI moveDamage;
        public TMPro.TextMeshProUGUI moveDescription;

        public void ShowDetails(UnitData unitData, MoveData moveData)
        {
            label.text = unitData.unitName + " moves:";
            moveName.text = moveData.moveName;
            moveRange.text = moveData.moveRange.ToString();
            moveDamage.text = moveData.moveDamage.ToString();
            moveDescription.text = moveData.moveDescription;
        }
    }
}