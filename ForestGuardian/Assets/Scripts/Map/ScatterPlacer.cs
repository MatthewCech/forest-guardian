using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScatterPlacer : MonoBehaviour
{
    [Header("Object Parameters")]
    [SerializeField] private Scatter scatterTemplate;
    [SerializeField] private Color baseColor;
    [SerializeField, Range(0, 1f)] private float vibranceFlux = 0.05f;

    [Header("Placement Parameters")]
    [SerializeField, Range(0.01f, 3f)] private float scaleMin = 0.9f;
    [SerializeField, Range(0.01f, 3f)] private float scaleMax = 1.1f;
    [SerializeField] private Vector3 placementArea = Vector3.one;
    [SerializeField, Range(1, 100)] private int number = 10;
    [SerializeField] private bool placeLocally = false;
    [SerializeField] private string groupName = "Scatter Group";

    private List<Scatter> instances = new List<Scatter>();
    private GameObject group;

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(ScatterPlacer))]
    public class ScatterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.LabelField("Scatter placement tool");

            base.OnInspectorGUI();
            ScatterPlacer placementTool = (target as ScatterPlacer);
            bool hasPending = placementTool.HasPending();

            string buttonName = hasPending ? "Scramble" : "Place";
            if (GUILayout.Button(buttonName))
            {
                placementTool.Scramble();
            }

            // Conditionally visible writing or clearing 
            GUI.enabled = hasPending;
            if (GUILayout.Button("Write"))
            {
                placementTool.Write();
            }

            if (GUILayout.Button("Clear"))
            {
                placementTool.Clear();
            }
            GUI.enabled = true;
        }
    }

    private void OnValidate()
    {
        if (scatterTemplate != null)
        {
            baseColor = scatterTemplate.GetColor();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(.1f, .9f, .2f, .2f);
        Gizmos.DrawWireCube(this.transform.position, placementArea);
    }
#endif

    private void Clear()
    {
        foreach (Scatter s in instances)
        {
            if (s as Object == null)
            {
                continue;
            }

            DestroyImmediate(s.gameObject);
        }

        instances.Clear();
        DestroyImmediate(group);
    }

    private bool HasPending()
    {
        return instances != null && instances.Count > 0;
    }

    private void Scramble()
    {
        Clear();
        group = new GameObject("[PENDING GROUP] (" + groupName + ")");
        group.transform.position = this.transform.position;

        for (int i = 0; i < number; i++)
        {
            Scatter scatter = GameObject.Instantiate(scatterTemplate);

            // Color set
            Color.RGBToHSV(baseColor, out float h, out float s, out float v);
            v = v + Random.Range(-vibranceFlux, vibranceFlux);
            Color composed = Color.HSVToRGB(h, s, v);
            scatter.SetColor(composed);

            // Position
            float posX = Random.Range(-placementArea.x / 2.0f, placementArea.x / 2.0f);
            float posY = Random.Range(-placementArea.y / 2.0f, placementArea.y / 2.0f);
            float posZ = Random.Range(-placementArea.z / 2.0f, placementArea.z / 2.0f);
            Vector3 pos = new Vector3(posX, posY, posZ);
            if (placeLocally)
            {
                scatter.transform.position = this.transform.position + pos;
            }
            else
            {
                scatter.transform.position = pos;
            }

            // Scale
            float scale = Random.Range(scaleMin, scaleMax);
            scatter.transform.localScale = new Vector3(scale, scale, scale);

            // Track
            scatter.transform.SetParent(group.transform);
            instances.Add(scatter);
        }
    }

    private void Write()
    {
        instances.Clear();
        group.name = groupName;
        group = null;
    }
}
