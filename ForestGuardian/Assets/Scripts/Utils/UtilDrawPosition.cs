using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace forest
{
    public class UtilDrawPosition : MonoBehaviour
    {
        [SerializeField] private Color color = Color.white;

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(this.transform.position, 0.05f);
            Gizmos.DrawWireSphere(this.transform.position, 0.5f);
        }
    }
}