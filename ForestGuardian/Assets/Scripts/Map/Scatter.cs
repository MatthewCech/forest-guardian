using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scatter : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> visuals;
    [Space]
    [SerializeField] private Sprite newSprite;

    public Color GetColor()
    {
        UnityEngine.Assertions.Assert.IsNotNull(visuals, "There must be visuals associated with scatter");
        UnityEngine.Assertions.Assert.IsTrue(visuals.Count >= 1, "There must be at least one visual");
        return visuals[0].color;
    }

    public void SetColor(Color color)
    {
        foreach(var renderer in visuals)
        {
            renderer.color = color;
        }
    }

    public void ReplaceVisual()
    {
        SpriteRenderer visual = visuals[0];
        visual.sprite = newSprite;
        Transform toSave = visual.transform;
        Transform[] transforms = GetComponentsInChildren<Transform>();
        for(int i = transforms.Length - 1; i >= 0; --i)
        {
            Transform cur = transforms[i];
            if(cur != toSave && cur != this.transform)
            {
                GameObject.DestroyImmediate(cur.gameObject);
            }
        }
        visuals.Clear();
        visuals.Add(visual);
        visual.transform.localPosition = new Vector3(0, .975f, 0);
        this.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Add some custom refresh options
    /// </summary>
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(Scatter))]
    public class ScatterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Replace"))
            {
                (target as Scatter).ReplaceVisual();
            }
            base.OnInspectorGUI();
        }
    }
#endif
}
