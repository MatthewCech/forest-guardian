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
        [Tooltip("the unlock provided by finishing this dungeon")][SerializeField] private List<string> tagsBestowed;

        public string TagLabel { get { return tagLabel; } }
        public List<string> TagsBestowed { get { return tagsBestowed; } }

#if UNITY_EDITOR
        /// <summary>
        /// Do some editor-time preprocessing and 
        /// </summary>
        private void OnValidate()
        {
            if(levelData != null)
            {
                Playfield pf = JsonUtility.FromJson<Playfield>(levelData.text);
                this.gameObject.name = $"Dungeon '{pf.tagLabel}' (unlocks '{pf.GetBestowedList()}')";
                this.tagLabel = pf.tagLabel;
                this.tagsBestowed = pf.tagsBestowed;
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
            if (Core.Instance.gameData.unlockedTags == null)
            {
                this.gameObject.SetActive(false);
                return;
            }

            for (int i = 0; i < Core.Instance.gameData.unlockedTags.Count; ++i)
            {
                string curTag = Core.Instance.gameData.unlockedTags[i];
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
