using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;


namespace forest
{
    public class Tile : MonoBehaviour
    {
        // TODO: Extract this data to make it visual agnostic
        public bool isImpassable = false;
        public int moveDifficulty = 1;

        [Header("Runtime Visual Association")]
        public PlayfieldTile associatedData; // Note: Could be replaced w/ ID later if needed
        public Vector2Int associatedPos;

        private void OnDrawGizmos()
        {
            if (associatedData == null)
            {
                return;
            }

            int id = associatedData.id;
            int movCost = associatedData.curMoveDifficulty;
            bool isImpass = associatedData.curIsImpassable;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.green;
            style.alignment = TextAnchor.MiddleLeft;

            float lOffset = 0.45f;
            float vSpace = -0.15f;
            float vOffset = -0.1f;

            Handles.Label(transform.position + Vector3.left * lOffset + Vector3.up * (vOffset), "id: " + id);
            Handles.Label(transform.position + Vector3.left * lOffset + Vector3.up * (vOffset + vSpace), "mov$: " + movCost);
            Handles.Label(transform.position + Vector3.left * lOffset + Vector3.up * (vOffset + vSpace * 2), "imp?: " + (isImpass ? "Y" : "N"));
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0)) // left 
            {
                MsgTilePrimaryAction msg = new MsgTilePrimaryAction();
                msg.tilePosition = associatedPos;
                msg.tileTweaked = this;
                Postmaster.Instance.Send(msg);
            }
            else if (Input.GetMouseButtonDown(1)) // right
            {
                MsgTileSecondaryAction msg = new MsgTileSecondaryAction();
                msg.position = associatedPos;
                msg.tile = this;
                Postmaster.Instance.Send(msg);
            }
        }
    }
}