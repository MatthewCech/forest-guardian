using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace forest
{
    public class Unit : MonoBehaviour
    {
        // TODO: Extract this data to make it visual agnostic
        public int maxSize = 3;
        public int moveSpeed = 2;
        public int attackRange = 2;

        [Header("Runtime Visual Association")]
        public int associatedDataID;

        private void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.left * .4f - Vector3.down * .3f, "id: " + associatedDataID);
        }
    }
}