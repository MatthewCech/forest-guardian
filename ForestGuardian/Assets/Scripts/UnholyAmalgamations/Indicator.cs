using forest;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Loam;

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
        ImmediateMove = 2
    }

    public class Indicator : MonoBehaviour
    {
        // TODO: Extract this data to make it visual agnostic
        public IndicatorType type = IndicatorType.DEFAULT;

        [Header("Runtime Visual Association")]
        public PlayfieldTile associatedTile = null; // Also could be ID?
        public PlayfieldUnit ownerUnit = null; // Eh? ID?
        public Vector2Int overlaidPosition = Vector2Int.zero; // The location we're targeting

        private void OnMouseDown()
        {
            MsgIndicatorClicked msg = new MsgIndicatorClicked();
            msg.indicator = this;
            Postmaster.Instance.Send(msg);
            /*
            if (associatedTile != null)
            {
                Debug.Log("Tile " + associatedTile.id + ", unit ID: " + associatedTile.associatedUnitID);
            }
            */
        }

        private void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.left * .35f + Vector3.down * .2f, "id->" + associatedTile.id);
        }
    }
}