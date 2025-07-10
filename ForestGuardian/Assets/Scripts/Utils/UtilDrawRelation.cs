using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class UtilDrawChildRelation : MonoBehaviour
    {
        [SerializeField] private Color color = Color.magenta;
        [SerializeField] private Transform target;

        private void OnDrawGizmos()
        {
            if(target == null)
            {
                return;
            }

            Color prev = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawLine(this.transform.position, target.position);
            Gizmos.color = prev;
        }
    }
}