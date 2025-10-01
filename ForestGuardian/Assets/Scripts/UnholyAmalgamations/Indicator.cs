using forest;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Loam;
using UnityEngine.UIElements;

namespace forest
{
    public enum IndicatorType
    {
        DEFAULT = 0,

        /// <summary>
        /// A preview of a move that could be made
        /// </summary>
        Preview = 1,

        /// <summary>
        /// Expected to be clicked by the user, and represents a very narrow movement option set in the 4 cardinal directions
        /// </summary>
        ImmediateMove = 2,

        /// <summary>
        /// A preview for attacking range, any of which can be interacted with to imply an attack
        /// </summary>
        Attack = 3
    }

    public class Indicator : MonoBehaviour
    {
        // TODO: Extract this data to make it visual agnostic
        public IndicatorType type = IndicatorType.DEFAULT;

        [Header("Runtime Visual Association")]
        public PlayfieldTile associatedTile = null;           // Note: Can be replaced w/ ID later if needed
        public PlayfieldUnit ownerUnit = null;                // Note: Can also be replaced w/ ID later if needed
        public Vector2Int overlaidPosition = Vector2Int.zero; // The location we're targeting

        private void OnMouseDown()
        {
            // MOVE TO INTERNAL CHECKING FOR UNSUB/RESUB OR EVENT PAUSING?
            if (!Core.HasInstance || !Core.Instance.UICore.IsWorldInteractable)
            {
                return;
            }

            MsgIndicatorClicked msg = new MsgIndicatorClicked();
            msg.indicator = this;
            Postmaster.Instance.Send(msg);
        }

        private void OnDrawGizmos()
        {
            if(associatedTile == null)
            {
                return;
            }

            int id = associatedTile.id;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = type == IndicatorType.Attack ? Color.red : Color.cyan;
            style.alignment = TextAnchor.MiddleLeft;

            float lOffset = 0.05f;
            //float vSpace = -0.15f;
            float vOffset = -0.1f;

            Handles.Label(transform.position + Vector3.left * lOffset + Vector3.up * (vOffset), "id@>" + id, style);
        }
    }
}