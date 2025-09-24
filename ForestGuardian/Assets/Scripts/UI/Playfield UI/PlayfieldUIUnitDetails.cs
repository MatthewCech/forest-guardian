using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace forest
{
    public enum ShowExtras
    {
        Show,
        ViewOnly,
        Hide
    }

    public class PlayfieldUIUnitDetails : MonoBehaviour
    {
        [SerializeField] public Image unitIcon;
        [SerializeField] public TMPro.TextMeshProUGUI label;
        [SerializeField] public TMPro.TextMeshProUGUI moveName;
        [SerializeField] public TMPro.TextMeshProUGUI moveRange;
        [SerializeField] public TMPro.TextMeshProUGUI moveDamage;
        [SerializeField] public TMPro.TextMeshProUGUI moveDescription;
        [SerializeField] public Button move;
        [SerializeField] public Button noAction;

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

        public void ShowDetailsOfInstance(Unit instance, ShowExtras show)
        {
            ShowDetails(instance.data, instance, show);
        }

        public void ShowDetails(UnitData unitData, Unit unitVisual, ShowExtras show)
        {
            Clear();

            label.text = unitData.unitName + " moves:";
            unitIcon.sprite = unitVisual.uiIcon;

            SetExtrasVisibility(unitVisual, show);

            foreach (MoveData move in unitData.moves)
            {
                PlayfieldUIUnitDetailMove moveButton = Instantiate(moveButtonTemplate, moveButtonParent);
                moveButton.gameObject.SetActive(true);
                moveButton.AssignMove(move);
                moveButton.move.onClick.AddListener(() => ShowMoveData(moveButton));
                moveButton.move.onClick.AddListener(() => MessageMoveSelected(move));

                trackedMoveButtons.Add(moveButton);
            }
        }

        /// <summary>
        /// Show a reasonable enabled, disabled, and visible state for extra controls on the playfield panel, taking
        /// into account the current state of the specified unit on the playfield
        /// </summary>
        private void SetExtrasVisibility(Unit unitVisual, ShowExtras show)
        {
            if (show == ShowExtras.Hide)
            {
                noAction.gameObject.SetActive(false);
                move.gameObject.SetActive(false);
            }
            else
            {
                noAction.gameObject.SetActive(true);
                move.gameObject.SetActive(true);

                if (show == ShowExtras.Show)
                {
                    noAction.interactable = true;

                    if (unitVisual.associatedData.curMovementBudget > 0)
                    {
                        move.interactable = true;
                    }
                    else
                    {
                        move.interactable = false;
                    }
                }
                else
                {
                    noAction.interactable = false;
                    move.interactable = false;
                }
            }
        }

        /// <summary>
        /// Attempt to select the specified move if it's present.
        /// </summary>
        /// <param name="move"></param>
        public void PhantomSelectMove(MoveData move)
        {
            foreach (PlayfieldUIUnitDetailMove moveButton in trackedMoveButtons)
            {
                if(move.moveName.Equals(moveButton.AssociatedMove.moveName, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    ShowMoveData(moveButton);
                    return;
                }
            }
        }

        private void ShowMoveData(PlayfieldUIUnitDetailMove currentButton)
        {
            foreach(PlayfieldUIUnitDetailMove btn in trackedMoveButtons)
            {
                btn.highlight.gameObject.SetActive(false);
            }

            currentButton.highlight.gameObject.SetActive(true);
            MoveData moveData = currentButton.AssociatedMove;

            moveName.text = moveData.moveName;
            moveRange.text = moveData.moveRange.ToString();
            moveDamage.text = moveData.moveDamage.ToString();
            moveDescription.text = moveData.moveDescription;
        }

        private void MessageMoveSelected(MoveData moveSelected)
        {
            MsgMoveSelected_UI msgMove = new MsgMoveSelected_UI()
            {
                moveSelected = moveSelected.Clone()
            };

            Loam.Postmaster.Instance.Send(msgMove);
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