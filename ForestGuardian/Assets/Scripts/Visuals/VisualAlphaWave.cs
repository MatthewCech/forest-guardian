using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualAlphaWave : MonoBehaviour
{
    [SerializeField] private float speed = 1;
    [SerializeField] private float offset = 1;
    [SerializeField] private SpriteRenderer target;
    [SerializeField] private float minimumAlpha = 0.3f;

    private float timeSoFar = 0;
    // Update is called once per frame
    void Update()
    {
        timeSoFar += Time.deltaTime;

        float normalizedSin = (Mathf.Sin((timeSoFar + offset) * speed) + 1) / 2.0f;
        float value = minimumAlpha + ((1.0f - minimumAlpha) * normalizedSin);
        Color tweaked = target.color;
        tweaked.a = value;
        target.color = tweaked;
    }
}
