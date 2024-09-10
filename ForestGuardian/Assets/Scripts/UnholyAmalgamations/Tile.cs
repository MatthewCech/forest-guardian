using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace forest
{
    public class Tile : MonoBehaviour
    {
        [Header("Runtime Visual Association")]
        public int associatedDataID;

        private void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.left * .3f, "id: " + associatedDataID);
        }
    }
}