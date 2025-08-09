using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{

    public class MapUIMenu : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI instanceState;
        [SerializeField] private float timeBetweenUpdates = 0.25f;

        private float timeSoFar = 0;

        private void Start()
        {
            timeSoFar = timeBetweenUpdates;
        }

        // Update is called once per frame
        void Update()
        {
            timeSoFar += Time.deltaTime;

            if (timeSoFar > timeBetweenUpdates)
            {
                if (Core.Instance != null)
                {
                    instanceState.text = JsonUtility.ToJson(Core.Instance.gameData, true);
                }

                timeSoFar = 0;
            }
        }
    }
}
