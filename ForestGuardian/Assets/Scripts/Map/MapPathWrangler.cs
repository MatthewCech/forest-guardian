using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
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

        /// <summary>
        /// Walk through all levels and see what is and isn't unlocked, then draw the unlock relationships accordingly.
        /// NOTE: This is done with lists and nested loops instead of a hashset lookup or similar because I want to 
        /// be able to ignore case and culture when comparing the level tags/names with the unlock tags/names. 
        /// </summary>
        void RedrawConnectionLines()
        {
            // For now, one line though we'll use the list since we'll need it later for branching unlocks.
            foreach (LineRenderer line in trackedLines)
            {
                GameObject.Destroy(line.gameObject);
            }
            trackedLines.Clear();

            // Collect all visible and active interaction points on the map 
            Object[] obj = FindObjectsByType(typeof(MapInteractionPoint), FindObjectsSortMode.InstanceID);
            List<MapInteractionPoint> activeInteractionPoints = new List<MapInteractionPoint>();
            foreach (Object o in obj)
            {
                MapInteractionPoint cur = o as MapInteractionPoint;
                foreach (string unlock in Core.Instance.gameData.unlockedTags)
                {
                    if (string.Equals(cur.TagLabel, unlock, System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        activeInteractionPoints.Add(cur);
                        continue;
                    }
                }
            }

            // Determine positions between active sections
            List<KeyValuePair<Vector3, Vector3>> toDraw = new List<KeyValuePair<Vector3, Vector3>>();
            foreach(MapInteractionPoint mapIP in activeInteractionPoints)
            {
                foreach (MapInteractionPoint source in activeInteractionPoints)
                {
                    foreach (string bestowedTag in source.TagsBestowed)
                    {
                        if (string.Equals(mapIP.TagLabel, bestowedTag, System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Line starts from parent position, goes to child
                            toDraw.Add(new KeyValuePair<Vector3, Vector3>(
                                new Vector3(source.transform.position.x, 0, source.transform.position.z),
                                new Vector3(mapIP.transform.position.x, 0, mapIP.transform.position.z)));
                        }
                    }
                }
            }

            // Build out lines by connecting our pairs of positions
            foreach(var pair in toDraw)
            {
                LineRenderer renderer = Instantiate(referenceLineRenderer, lineParent);
                renderer.gameObject.SetActive(true);
                renderer.positionCount = 2;
                renderer.SetPositions(new Vector3[2] { pair.Key, pair.Value });

                trackedLines.Add(renderer);
            }
        }
    }
}