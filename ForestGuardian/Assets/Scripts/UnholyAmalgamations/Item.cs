using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Loam;

namespace forest
{
    public class Item : MonoBehaviour
    {
        [Header("Runtime Visual Association")]
        public PlayfieldItem associatedData; // Note: Could be replaced w/ ID later if needed
        public Vector2Int gridPos;

        public void Collect()
        {
            MsgCollectableGrabbed msg = new MsgCollectableGrabbed();
            msg.collectable = this;
            Postmaster.Instance.Send<MsgCollectableGrabbed>(msg);

            // Note: Destroying from within itself is dangerous, and should
            // be handled by the visualization system.
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0)) // left 
            {
                MsgItemPrimaryAction msg = new MsgItemPrimaryAction();
                msg.position = gridPos;
                msg.item = this;
                Postmaster.Instance.Send(msg);
            }
            else if (Input.GetMouseButtonDown(1)) // right
            {
                MsgItemSecondaryAction msg = new MsgItemSecondaryAction();
                msg.position = gridPos;
                msg.item = this;
                Postmaster.Instance.Send(msg);
            }
        }

    }
}
