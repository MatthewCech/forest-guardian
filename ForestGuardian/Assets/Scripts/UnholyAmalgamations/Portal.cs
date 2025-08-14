using forest;
using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace forest
{
    /// <summary>
    /// This is something like stairs in PMD or similar, but can represent other transitory actions within a dungeons.
    /// </summary>
    public class Portal : MonoBehaviour
    {
        [Header("Runtime Visual Association")]
        public PlayfieldPortal associatedData; // Note: Could be replaced w/ ID later if needed
        public Vector2Int associatedPos;

        private void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0)) // left 
            {
                MsgPortalPrimaryAction msg = new MsgPortalPrimaryAction();
                msg.position = associatedPos;
                msg.item = this;
                Postmaster.Instance.Send(msg);
            }
            else if (Input.GetMouseButtonDown(1)) // right
            {
                MsgPortalSecondaryAction msg = new MsgPortalSecondaryAction();
                msg.position = associatedPos;
                msg.item = this;
                Postmaster.Instance.Send(msg);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (associatedData == null)
            {
                return;
            }

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.magenta;
            style.alignment = TextAnchor.MiddleCenter;

            Handles.Label(this.transform.position + new Vector3(0, 0.1f, 0), $"Target: \"{associatedData.target}\"", style);
        }
#endif
    }
}