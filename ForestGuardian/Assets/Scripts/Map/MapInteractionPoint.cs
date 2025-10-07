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
        [SerializeField] private GameObject visibilityRoot;
        [SerializeField] private SpriteRenderer highlight;
        [SerializeField] private SpriteRenderer shadow;
        [SerializeField] private SpriteRenderer unfinished;

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
        private MessageSubscription handleMsgLevelFinished;
        private MessageSubscription handleMsgRefreshVisibility;

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
            // Prevent clicking through UI to change map level info
            if(UICore.IsMouseOverUIElement())
            {
                return;
            }

            // If not visible, don't allow interactions
            if(!visibilityRoot.activeInHierarchy)
            {
                return;
            }

            Loam.Postmaster.Instance.Send(new MsgShowLevelInfo() { mapInteractionPoint = this });
        }

        private void Start()
        {
            CheckLevelVisibility();

            handleMsgShowLevelInfo = Postmaster.Instance.Subscribe<MsgShowLevelInfo>(LevelShow);
            handleMsgHideLevelInfo = Postmaster.Instance.Subscribe<MsgHideLevelInfo>(LevelHide);
            handleMsgLevelFinished = Postmaster.Instance.Subscribe<MsgLevelFinishedAdded>(LevelFinished);
            handleMsgRefreshVisibility = Postmaster.Instance.Subscribe<MsgLevelUnlockAdded>((_) => { CheckLevelVisibility(); });
        }

        public void CheckLevelVisibility()
        {
            // Don't proceed if we have no level tags.
            if (Core.Instance.GameData.unlockedTags == null)
            {
                visibilityRoot.SetActive(false);
                return;
            }

            bool didSetActive = UpdateVisible();
            if (didSetActive)
            {
                UpdateFinished();
            }
            else
            {
                visibilityRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            handleMsgShowLevelInfo.Dispose();
            handleMsgHideLevelInfo.Dispose();
            handleMsgLevelFinished.Dispose();
            handleMsgRefreshVisibility.Dispose();
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

        private bool UpdateVisible()
        {
            for (int i = 0; i < Core.Instance.GameData.unlockedTags.Count; ++i)
            {
                string curTag = Core.Instance.GameData.unlockedTags[i];
                if (string.Equals(tagLabel, curTag, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    visibilityRoot.SetActive(true);
                    return true;
                }
            }

            return false;
        }

        private bool UpdateFinished()
        {
            for (int i = 0; i < Core.Instance.GameData.finishedTags.Count; ++i)
            {
                string curTag = Core.Instance.GameData.finishedTags[i];
                if (string.Equals(tagLabel, curTag, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    SetFinished(true);
                    return true;
                }
            }

            return false;
        }

        private void LevelFinished(Message raw)
        {
            MsgLevelFinishedAdded msg = raw as MsgLevelFinishedAdded;
            string interactable = msg.newFinishedLevel;

            CheckLevelVisibility();

            SetHighlight(false);
        }

        private void SetHighlight(bool isHighlighted)
        {
            highlight.gameObject.SetActive(isHighlighted);
            shadow.gameObject.SetActive(!isHighlighted);
        }

        private void SetFinished(bool isUnlocked)
        {
            unfinished.gameObject.SetActive(!isUnlocked);
        }
    }
}
