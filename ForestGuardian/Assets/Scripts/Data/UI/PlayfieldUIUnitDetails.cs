using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{
    public class PlayfieldUIUnitDetails : MonoBehaviour
    {
        [SerializeField] public Image unitIcon;
        [SerializeField] public TMPro.TextMeshProUGUI label;
        [SerializeField] public TMPro.TextMeshProUGUI moveName;
        [SerializeField] public TMPro.TextMeshProUGUI moveRange;
        [SerializeField] public TMPro.TextMeshProUGUI moveDamage;
        [SerializeField] public TMPro.TextMeshProUGUI moveDescription;

        [SerializeField] private PlayfieldUIUnitDetailMove moveButtonTemplate;
        [SerializeField] private Transform moveButtonParent;

        private List<PlayfieldUIUnitDetailMove> trackedMoveButtons;
        private string descriptionPlaceholder;

        private void Awake()
        {
            trackedMoveButtons = new List<PlayfieldUIUnitDetailMove>();
            descriptionPlaceholder = moveDescription.text;
            moveButtonTemplate.gameObject.SetActive(false);
        }

        public void ShowDetails(UnitData unitData, Unit unitVisual)
        {
            Clear();

            label.text = unitData.unitName + " moves:";
            unitIcon.sprite = unitVisual.uiIcon;

            foreach(MoveData move in unitData.moves)
            {
                PlayfieldUIUnitDetailMove moveButton = Instantiate(moveButtonTemplate, moveButtonParent);
                moveButton.gameObject.SetActive(true);
                moveButton.AssignMove(move);
                moveButton.move.onClick.AddListener(() => ShowMoveData(moveButton.AssociatedMove));

                trackedMoveButtons.Add(moveButton);
            }
        }

        private void ShowMoveData(MoveData moveData)
        {
            moveName.text = moveData.moveName;
            moveRange.text = moveData.moveRange.ToString();
            moveDamage.text = moveData.moveDamage.ToString();
            moveDescription.text = moveData.moveDescription;
        }

        private void Clear()
        {
            foreach(PlayfieldUIUnitDetailMove move in trackedMoveButtons)
            {
                Destroy(move.gameObject);
            }
            trackedMoveButtons.Clear();
            
            label.text = "";
            moveName.text = "";
            moveRange.text = "";
            moveDamage.text = "";
            moveDescription.text = descriptionPlaceholder;
        }
    }
}