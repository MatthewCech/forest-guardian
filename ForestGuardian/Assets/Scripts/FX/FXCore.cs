using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class FXCore : MonoBehaviour
    {
        [SerializeField] private FXLookup fxLookup;

        [Header("Debug")]
        [SerializeField] private bool DEBUG_dontParent;
        [SerializeField] public FXTag DEBUG_fxTag;
        [SerializeField] public Transform DEBUG_fxOrigin;
        [SerializeField] public Transform DEBUG_fxTarget;

        private List<FXPerformer> playing = new List<FXPerformer>();

        void Awake()
        {
            if (!DEBUG_dontParent && !Core.Instance.TryRegisterFXCore(this))
            {
                return;
            }

            fxLookup.Initialize();
        }

        public void Play(FXTag tag, Transform origin, Transform target, System.Action<bool> onComplete = null)
        {
            if(!fxLookup.TryGetFXTemplateByTag(tag, out FXPerformer identified))
            {
                Debug.LogWarning($"Couldn't find FX with the template {tag.ToString()}");
                return;
            }

            if(origin == null || target == null )
            {
                return;
            }

            FXPerformer fx = Instantiate(identified);
            fx.transform.position = identified.transform.position;
            fx.FxStart(origin, target, onComplete);
            playing.Add(fx);
        }

        private void Update()
        {
            for(int i = playing.Count - 1; i >= 0; i--)
            {
                FXPerformer cur = playing[i];
                cur.FXUpdate();

                if(cur.HasFinished)
                {
                    playing.RemoveAt(i);
                }
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(FXCore))]
    [UnityEditor.CanEditMultipleObjects] // If safe, this is nice. 
    public class YOUR_MONOBEHAVIOR_CLASS_NAMEEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.LabelField("I'm a custom added label!");
            
            base.OnInspectorGUI();

            if (GUILayout.Button("Perform Debug FX"))
            {
                FXCore fxCore = target as FXCore;
                (fxCore).Play(fxCore.DEBUG_fxTag, fxCore.DEBUG_fxOrigin, fxCore.DEBUG_fxTarget, (fxFinishedSpecifically) =>
                {
                    Debug.Log($"Debug FX with tag {fxCore.DEBUG_fxTag} finished, with argument {fxFinishedSpecifically}");
                });
            }
        }
    }
#endif
}