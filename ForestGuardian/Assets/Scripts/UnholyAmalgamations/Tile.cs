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
        [Header("Runtime Visual Association")]
        public int associatedDataID;
        public Vector2Int associatedPos;

        private void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.left * .3f, "id: " + associatedDataID);
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
                msg.tilePosition = associatedPos;
                msg.tileTweaked = this;

                Postmaster.Instance.Send(msg);
            }
        }
    }
}