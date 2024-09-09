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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            int id = -1;
            int moves = 0;

            if(associatedData != null)
            {
                id = associatedData.id;
                moves = associatedData.movementBudget;
            }

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.green;

            UnityEditor.Handles.Label(transform.position + Vector3.left * .4f - Vector3.down * .3f, "id: " + id, style);
            UnityEditor.Handles.Label(transform.position + Vector3.left * .4f - Vector3.down * .4f, "ms: " + moves, style);
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