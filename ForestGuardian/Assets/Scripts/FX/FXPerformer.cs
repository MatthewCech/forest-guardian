using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class FXPerformer : MonoBehaviour
    {
        [SerializeField] private FXTag effectTag;
        [SerializeField] protected float maxTime = 2; // Backup max time the effect can take
        
        // Populated by effect caller
        [NonSerialized] protected Transform target; // target of the effect for use mostly in the effect. 
        [NonSerialized] protected Transform source; // origin location for the effect (where this prefab gets placed)
        [NonSerialized] protected Action<bool> OnComplete; // pass if normally executed or not.

        // Internal tracking
        [NonSerialized] private float timeSoFar = 0;
        [NonSerialized] protected bool hasFinished = false;
        [NonSerialized] protected bool hasStarted = false;

        public virtual void FxStart(Transform source, Transform target, Action<bool>OnComplete = null)
        {
            this.OnComplete = OnComplete;
            this.source = source;
            this.target = target;

            hasStarted = true;
        }

        public virtual void FXUpdate()
        {
            if (!hasStarted || !hasFinished)
            {
                return;
            }

            timeSoFar += Time.deltaTime;
            if (timeSoFar >= maxTime && !hasFinished)
            {
                OnComplete?.Invoke(false);
                hasFinished = true;
                Destroy(this);
                return;
            }
        }

        public virtual void FXDone()
        {
            OnComplete?.Invoke(true);
            Destroy(this.gameObject);
        }
    }
}