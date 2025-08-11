using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Loam;

namespace forest
{
    public class PlayfieldUISelectionEntryMove : MonoBehaviour
    {
        [SerializeField] private Button move;
        [SerializeField] private TMPro.TextMeshProUGUI moveText;

        private MoveData associatedMoveData;

        public MoveData AssociatedMove { get { return associatedMoveData; } }

        public void AssignMove(MoveData moveData)
        {
            moveText.text = moveData.moveName.ToString();
            associatedMoveData = moveData;

            //Loam.Postmaster.Instance.
        }
    }
}