using forest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class VisualRegistration : MonoBehaviour
    {
        [SerializeField] private VisualLookup visualLookup;

        public void Awake()
        {
            Core.Instance.TryRegisterVisualLookup(visualLookup);
        }
    }
}