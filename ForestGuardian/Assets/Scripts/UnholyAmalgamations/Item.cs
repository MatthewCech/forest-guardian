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
        public Vector2Int associatedPos;

        private void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0)) // left 
            {
                MsgItemPrimaryAction msg = new MsgItemPrimaryAction();
                msg.position = associatedPos;
                msg.item = this;
                Postmaster.Instance.Send(msg);
            }
            else if (Input.GetMouseButtonDown(1)) // right
            {
                MsgItemSecondaryAction msg = new MsgItemSecondaryAction();
                msg.position = associatedPos;
                msg.item = this;
                Postmaster.Instance.Send(msg);
            }
        }
    }
}
