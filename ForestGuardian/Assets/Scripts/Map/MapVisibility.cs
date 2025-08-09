using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class MapVisibility : MonoBehaviour
    {
        [SerializeField] private TextAsset asset = null;

        private string label;
        void Start()
        {
            label = JsonUtility.FromJson<Playfield>(asset.text).tagLabel;
            bool didSetActive = false;

            // Don't proceed if we have no level tags.
            if(Core.Instance.gameData.completedLevelTags == null)
            {
                this.gameObject.SetActive(false);
                return;
            }

            for (int i = 0; i < Core.Instance.gameData.completedLevelTags.Count; ++i)
            {
                string curTag = Core.Instance.gameData.completedLevelTags[i];
                if (string.Equals(label, curTag, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    this.gameObject.SetActive(true);
                    didSetActive = true;
                    break;
                }
            }

            if(!didSetActive)
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}