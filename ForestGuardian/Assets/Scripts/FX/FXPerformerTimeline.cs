using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace forest
{
    public class FXPerformerTimeline : FXPerformer
    {
        private const double FX_DURATION_PADDING = 0.5; // seconds

        [SerializeField] private PlayableDirector director;

        private void OnValidate()
        {
            if (director != null)
            {
                maxTime = director.duration + FX_DURATION_PADDING;
            }
        }

        public override void FxStart(Transform source, Transform target, Action<bool> OnComplete = null)
        {
            base.FxStart(source, target, OnComplete);
            maxTime = director.duration + FX_DURATION_PADDING;
            director.Play();
            director.stopped += Director_stopped;
        }

        private void Director_stopped(PlayableDirector obj)
        {
            base.FXDone();
        }
    }
}