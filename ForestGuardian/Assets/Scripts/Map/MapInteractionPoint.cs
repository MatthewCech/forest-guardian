using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class MapInteractionPoint : MonoBehaviour
    {
        [SerializeField] private TextAsset levelData;

        [Header("Auto-populated")]
        [Tooltip("Level name, used for unlocking")] [SerializeField] private string tagLabel;
        [Tooltip("the unlock provided by finishing this dungeon")][SerializeField] private string tagBestowed;

        public string TagLabel { get { return tagLabel; } }
        public string TagBestowed { get { return tagBestowed; } }

#if UNITY_EDITOR
        /// <summary>
        /// Do some editor-time preprocessing and 
        /// </summary>
        private void OnValidate()
        {
            if(levelData != null)
            {
                Playfield pf = JsonUtility.FromJson<Playfield>(levelData.text);
                this.gameObject.name = $"Dungeon '{pf.tagLabel}' (unlocks '{pf.tagBestowed}')";
                this.tagLabel = pf.tagLabel;
                this.tagBestowed = pf.tagBestowed;
            }
        }
#endif

        private string label;

        private void OnMouseUpAsButton()
        {
            UnityEngine.Assertions.Assert.IsNotNull(levelData, "Level data must be specified!");
            Debug.Log($"Attempting to load {levelData.name}...");
            Core.Instance.LoadLevelPlayfield(levelData);
        }

        void Start()
        {
            bool didSetActive = false;

            // Don't proceed if we have no level tags.
            if (Core.Instance.gameData.completedLevelTags == null)
            {
                this.gameObject.SetActive(false);
                return;
            }

            for (int i = 0; i < Core.Instance.gameData.completedLevelTags.Count; ++i)
            {
                string curTag = Core.Instance.gameData.completedLevelTags[i];
                if (string.Equals(tagLabel, curTag, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    this.gameObject.SetActive(true);
                    didSetActive = true;
                    break;
                }
            }

            if (!didSetActive)
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}
