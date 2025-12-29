using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class FXPerformer : MonoBehaviour
    {
        [Header("Base FX")]
        [SerializeField] private FXTag effectTag;
        [SerializeField] protected double maxTime = 2; // Backup max time the effect can take

        // Populated by effect caller
        [NonSerialized] protected Transform target; // target of the effect for use mostly in the effect. 
        [NonSerialized] protected Transform source; // origin location for the effect (where this prefab gets placed)
        [NonSerialized] protected Action<bool> OnComplete; // pass if the effect noted itself as done (true), or if it hit a timeout (false).

        // Internal tracking
        [NonSerialized] private double timeSoFar = 0;
        [NonSerialized] protected bool hasFinished = false;
        [NonSerialized] protected bool hasStarted = false;

        /// <summary>
        /// Begins the effect playback
        /// </summary>
        /// <param name="source">Beginning location for the effect</param>
        /// <param name="target">Ending location for the effect. May be the same as the source.</param>
        /// <param name="OnComplete">Optional callback</param>
        public virtual void FxStart(Transform source, Transform target, Action<bool>OnComplete = null)
        {
            this.OnComplete = OnComplete;
            this.source = source;
            this.target = target;

            hasStarted = true;
        }

        public virtual void FXUpdate()
        {
            if (!hasStarted || hasFinished)
            {
                return;
            }

            timeSoFar += Time.deltaTime;
            if (timeSoFar >= maxTime && !hasFinished)
            {
                OnComplete?.Invoke(false);
                hasFinished = true;
                Destroy(this.gameObject);
                return;
            }
        }

        public virtual void FXDone()
        {
            OnComplete?.Invoke(true);
            hasFinished = true;
            Destroy(this.gameObject);
        }

        public bool HasFinished { get { return hasFinished; } }

        public FXTag FXTag { get { return effectTag; } }
    }
}