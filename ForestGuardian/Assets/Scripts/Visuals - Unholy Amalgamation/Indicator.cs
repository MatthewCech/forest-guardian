using forest;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace forest
{
    public class Indicator : MonoBehaviour
    {
        [Header("Runtime Visual Association")]
        public PlayfieldTile associatedTile = null;
        public Vector2Int OverlaidPosition = Vector2Int.zero;

        private void OnMouseDown()
        {
            if (associatedTile != null)
            {
                Debug.Log("Tile " + associatedTile.id + ", unit ID: " + associatedTile.associatedUnitID);
            }
        }

        private void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.left * .35f + Vector3.down * .2f, "id->" + associatedTile.id);
        }
    }
}