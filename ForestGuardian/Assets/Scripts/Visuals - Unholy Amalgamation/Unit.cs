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
        public int attackRange = 2;

        [Header("General Visuals")]
        [SerializeField] private GameObject head = null;

        // CONSIDER: Replace with int ID if the reference creates issues.
        [Header("Runtime Association")]
        public PlayfieldUnit associatedData;
        public Vector2Int gridPos;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            int id = -1;
            int moves = 0;

            if(associatedData != null)
            {
                id = associatedData.id;
                moves = associatedData.curMovementBudget;
            }

            GUIStyle style = new GUIStyle();
            style.normal.textColor = new Color(.2f, .8f, .2f, 1);

            UnityEditor.Handles.Label(transform.position + Vector3.left * .4f - Vector3.down * .2f, "id: " + id, style);
            UnityEditor.Handles.Label(transform.position + Vector3.left * .4f - Vector3.down * .3f, "mov: " + moves, style);

            int locIndex = -1;
            for(int i = 0; i < associatedData.locations.Count; ++i)
            {
                Vector2Int cur = associatedData.locations[i];
                if(cur == gridPos)
                {
                    locIndex = i;
                    break;
                }
            }

            UnityEditor.Handles.Label(transform.position + Vector3.left * .4f - Vector3.down * .4f, "#: " + locIndex, style);
        }
#endif 

        public void SetBodyVisibility(bool visible)
        {
            head.SetActive(visible);
        }

        private void OnMouseDown()
        {
            Debug.Log("A unit has been clicked!");
        }
    }
}