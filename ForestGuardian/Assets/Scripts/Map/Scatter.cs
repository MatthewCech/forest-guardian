using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scatter : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> visuals;

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
}
