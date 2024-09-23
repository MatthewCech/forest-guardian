using Loam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class Unit : MonoBehaviour
    {
        // TODO: Extract this data to make it visual agnostic
        public int maxSize = 3;
        public int moveSpeed = 2;
        public int attackRange = 1;
        public Team defaultTeam = Team.DEFAULT;

        // ALSO TODO: collect this kinda stuff into objects for multi-attack
        public int attackDamage = 2;

        [Header("General Visuals")]
        [SerializeField] private GameObject head = null;

        // CONSIDER: Replace with int ID if the reference creates issues.
        [Header("Runtime Association")]
        public PlayfieldUnit associatedData; // Note: Could be replaced w/ ID later if needed
        public Vector2Int gridPos;


        private void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0)) // left 
            {
                MsgUnitPrimaryAction msg = new MsgUnitPrimaryAction();
                msg.position = gridPos;
                msg.unit = this;
                Postmaster.Instance.Send(msg);
            }
            else if (Input.GetMouseButtonDown(1)) // right
            {
                MsgUnitSecondaryAction msg = new MsgUnitSecondaryAction();
                msg.position = gridPos;
                msg.unit = this;
                Postmaster.Instance.Send(msg);
            }
        }

        public void SetBodyVisibility(bool visible)
        {
            head.SetActive(visible);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (associatedData == null)
            {
                return;
            }

            int id = associatedData.id;
            int moves = associatedData.curMovementBudget; ;
            int locIndex = -1;

            for (int i = 0; i < associatedData.locations.Count; ++i)
            {
                Vector2Int cur = associatedData.locations[i];
                if (cur == gridPos)
                {
                    locIndex = i;
                    break;
                }
            }

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.green;
            style.alignment = TextAnchor.MiddleLeft;

            float lOffset = 0.45f;
            float vSpace = 0.15f;
            float vOffset = 0.1f;

            UnityEditor.Handles.Label(transform.position + Vector3.left * lOffset + Vector3.up * (vOffset), "id: " + id, style);
            UnityEditor.Handles.Label(transform.position + Vector3.left * lOffset + Vector3.up * (vOffset + vSpace), "mov: " + moves, style);
            UnityEditor.Handles.Label(transform.position + Vector3.left * lOffset + Vector3.up * (vOffset + vSpace * 2), "loc#: " + locIndex, style);
        }
#endif 
    }
}