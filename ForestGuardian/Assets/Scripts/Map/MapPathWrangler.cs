using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class ByHierarchyPosition : IComparer
    {
        public int Compare(object x, object y)
        {
            int xVal = (x as MapInteractionPoint).transform.GetSiblingIndex();
            int yVal = (y as MapInteractionPoint).transform.GetSiblingIndex();
            return xVal.CompareTo(yVal);
        }
    }

    public class MapPathWrangler : MonoBehaviour
    {
        [SerializeField] private LineRenderer referenceLineRenderer;
        [SerializeField] private Transform lineParent;

        List<LineRenderer> trackedLines = new List<LineRenderer>();

        private void Awake()
        {
            referenceLineRenderer.gameObject.SetActive(false);
        }

        void Start()
        {
            RedrawConnectionLines();
        }

        void RedrawConnectionLines()
        {
            // For now, one line though we'll use the list since we'll need it later for branching unlocks.
            foreach(LineRenderer line in trackedLines)
            {
                GameObject.Destroy(line.gameObject);
            }
            trackedLines.Clear();

            // Collect all MapVisibility objects and then sort them by order in hierarchy
            Object[] obj = FindObjectsByType(typeof(MapInteractionPoint), FindObjectsSortMode.InstanceID);
            IComparer byHierarchyPosition = new ByHierarchyPosition();
            System.Array.Sort(obj, byHierarchyPosition);

            // Convert components to positions
            List<Vector3> pos = new List<Vector3>();
            foreach (Object o in obj)
            {
                if(!((MapInteractionPoint)o).gameObject.activeInHierarchy)
                {
                    continue;
                }

                Vector3 curPos = ((MapInteractionPoint)o).gameObject.transform.position;
                pos.Add(new Vector3(curPos.x, 0, curPos.z));
            }

            // Configure line as a single long item, this will need to be done differently once branching is added.
            LineRenderer renderer = Instantiate(referenceLineRenderer, lineParent);
            renderer.gameObject.SetActive(true);
            renderer.positionCount = pos.Count;
            renderer.SetPositions(pos.ToArray());
            trackedLines.Add(renderer);
        }
    }
}