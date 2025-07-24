using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    /// <summary>
    /// This is something like stairs in PMD or similar, but can represent other transitory actions within a dungeons.
    /// </summary>
    public class Exit : MonoBehaviour
    {
        [Header("Runtime Visual Association")]
        public PlayfieldExit associatedData; // Note: Could be replaced w/ ID later if needed
        public Vector2Int gridPos;

        private void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0)) // left 
            {
                MsgExitPrimaryAction msg = new MsgExitPrimaryAction();
                msg.position = gridPos;
                msg.item = this;
                Postmaster.Instance.Send(msg);
            }
            else if (Input.GetMouseButtonDown(1)) // right
            {
                MsgExitSecondaryAction msg = new MsgExitSecondaryAction();
                msg.position = gridPos;
                msg.item = this;
                Postmaster.Instance.Send(msg);
            }
        }
    }
}