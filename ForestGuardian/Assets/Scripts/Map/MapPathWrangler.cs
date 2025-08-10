using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class ByHierarchyPosition : IComparer
    {
        public int Compare(object x, object y)
        {
            int xVal = (x as MapVisibility).transform.GetSiblingIndex();
            int yVal = (y as MapVisibility).transform.GetSiblingIndex();
            return xVal.CompareTo(yVal);
        }
    }

    public class MapPathWrangler : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;

        void Start()
        {
            StartCoroutine(Delay());
        }

        IEnumerator Delay()
        {
            yield return new WaitForEndOfFrame();
            RedrawConnectionLines();
        }

        void RedrawConnectionLines()
        {
            // Collect all MapVisibility objects and then sort them by order in hierarchy
            Object[] obj = FindObjectsByType(typeof(MapVisibility), FindObjectsSortMode.InstanceID);
            IComparer byHierarchyPosition = new ByHierarchyPosition();
            System.Array.Sort(obj, byHierarchyPosition);

            // Convert components to positions
            List<Vector3> pos = new List<Vector3>();
            foreach (Object o in obj)
            {
                if(!((MapVisibility)o).gameObject.activeInHierarchy)
                {
                    continue;
                }

                Vector3 curPos = ((MapVisibility)o).gameObject.transform.position;
                pos.Add(new Vector3(curPos.x, 0, curPos.z));
            }

            // Configure
            lineRenderer.positionCount = pos.Count;
            lineRenderer.SetPositions(pos.ToArray());
        }
    }
}