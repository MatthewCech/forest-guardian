using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class LoadLevelOnClick : MonoBehaviour
    {
        [SerializeField] private TextAsset levelData;

        private void OnMouseUpAsButton()
        {
            UnityEngine.Assertions.Assert.IsNotNull(levelData, "Level data must be specified!");
            Debug.Log($"Attempting to load {levelData.name}...");
            Core.Instance.LoadLevelPlayfield(levelData);
        }
    }
}
