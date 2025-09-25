using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Loam;

namespace forest
{
    public class MapInteractionPoint : MonoBehaviour
    {
        [Header("Links")]
        [SerializeField] private TextAsset levelData;
        [SerializeField] private SpriteRenderer highlight;
        [SerializeField] private SpriteRenderer shadow;

        // TODO: Much easier to do automatically with addressables, for proof of concept for now just provide it manually.
        [Header("Should be Auto-populated probably")]
        [Tooltip("Player-facing name of the level")][SerializeField] private string visibleName = "UNNAMED";
        [Tooltip("Number of associated floors, the size of the dungeon")][SerializeField] private int depth = 1;

        [Header("Auto-populated")]
        [Tooltip("Level name, used for unlocking")][SerializeField] private string tagLabel;
        [Tooltip("the unlock provided by finishing this dungeon")][SerializeField] private List<string> tagsBestowed;

        // Internal
        private MessageSubscription handleMsgShowLevelInfo;
        private MessageSubscription handleMsgHideLevelInfo;

        // Properties
        public string TagLabel { get { return tagLabel; } }
        public List<string> TagsBestowed { get { return tagsBestowed; } }
        public TextAsset LevelData { get { return levelData; } }
        public string LevelName { get { return visibleName; } }
        public int LevelDepth { get { return depth; } }

#if UNITY_EDITOR
        /// <summary>
        /// Do some editor-time preprocessing and 
        /// </summary>
        private void OnValidate()
        {
            if (levelData != null)
            {
                Playfield pf = JsonUtility.FromJson<Playfield>(levelData.text);
                this.gameObject.name = $"Dungeon '{pf.tagLabel}' (unlocks '{pf.GetBestowedList()}')";
                this.tagLabel = pf.tagLabel;
                this.tagsBestowed = pf.tagsBestowed;
            }
        }
#endif

        private void OnMouseUpAsButton()
        {
            Loam.Postmaster.Instance.Send(new MsgShowLevelInfo() { mapInteractionPoint = this });
        }

        private void Start()
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

            handleMsgShowLevelInfo = Postmaster.Instance.Subscribe<MsgShowLevelInfo>(LevelShow);
            handleMsgHideLevelInfo = Postmaster.Instance.Subscribe<MsgHideLevelInfo>(LevelHide);
        }

        private void OnDestroy()
        {
            handleMsgShowLevelInfo.Dispose();
            handleMsgHideLevelInfo.Dispose();
        }

        private void LevelShow(Message raw)
        {
            MsgShowLevelInfo msg = raw as MsgShowLevelInfo;
            MapInteractionPoint interactable = msg.mapInteractionPoint;

            SetHighlight(interactable == this);
        }

        private void LevelHide(Message raw)
        {
            MsgHideLevelInfo msg = raw as MsgHideLevelInfo;
            MapInteractionPoint interactable = msg.mapInteractionPoint;

            SetHighlight(false);
        }

        private void SetHighlight(bool isHighlighted)
        {
            highlight.gameObject.SetActive(isHighlighted);
            shadow.gameObject.SetActive(!isHighlighted);
        }
    }
}
