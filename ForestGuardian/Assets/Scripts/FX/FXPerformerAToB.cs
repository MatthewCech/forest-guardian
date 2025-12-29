using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class FXPerformerAToB : FXPerformer
    {
        [Header("Derived")]
        [SerializeField] private AnimationCurve interpolationCurve;
        [Min(0.01f)][SerializeField] private float interpolationDuration = 1;

        private float iterpoationSoFar = 0;

        private void OnValidate()
        {
            if(interpolationDuration < 0)
            {
                interpolationDuration = 0;
            }

            if (interpolationDuration > maxTime)
            {
                maxTime = interpolationDuration;
            }
        }

        public override void FxStart(Transform source, Transform target, Action<bool> OnComplete = null)
        {
            base.FxStart(source, target, OnComplete);

            this.transform.position = source.position;
        }

        public override void FXUpdate()
        {
            base.FXUpdate();

            iterpoationSoFar += Time.deltaTime;
            if(iterpoationSoFar > interpolationDuration)
            {
                iterpoationSoFar = interpolationDuration;
            }

            float t = Mathf.Clamp01(iterpoationSoFar / interpolationDuration);
            Vector3 newPos = Vector3.Lerp(source.position, target.position, interpolationCurve.Evaluate(t));
            this.transform.position = newPos;
        }
    }
}