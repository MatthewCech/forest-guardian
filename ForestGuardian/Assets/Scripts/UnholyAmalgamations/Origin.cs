using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class Origin : MonoBehaviour
    {
        [Header("Runtime Visual Association")]
        public PlayfieldOrigin associatedData; // Note: Could be replaced w/ ID later if needed
        public Vector2Int associatedPos;
        public GameObject highlight;
        public SpriteRenderer unitIcon;

        public void SetHighlight(bool isShowing)
        {
            highlight.SetActive(isShowing);
        }

        private void Awake()
        {
            highlight.SetActive(false);
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0)) // left 
            {
                MsgOriginPrimaryAction msg = new MsgOriginPrimaryAction();
                msg.position = associatedPos;
                msg.origin = this;
                Postmaster.Instance.Send(msg);
            }
            else if (Input.GetMouseButtonDown(1)) // right
            {
                MsgOriginSecondaryAction msg = new MsgOriginSecondaryAction();
                msg.position = associatedPos;
                msg.origin = this;
                Postmaster.Instance.Send(msg);
            }
        }
    }
}
