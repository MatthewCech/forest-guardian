using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace forest
{
    public class Core : MonoBehaviour
    {
        public const string SCENE_NAME_PLAYFIELD = "Playfield";
        public const string SCENE_NAME_MAP = "Map";

        // Singleton setup
        private static Core instance = null;
        public static Core Instance
        {
            get
            {
                UnityEngine.Assertions.Assert.IsNotNull(instance);
                return instance;
            }
        }

        public static bool HasInstance
        {
            get
            {
                return instance != null;
            }
        }

        public GameInstance gameData { get; private set; }
        public AudioCore audioCore { get; private set; }
        public VisualLookup visualLookup { get; private set; }

        /// <summary>
        /// Pre-awake initialization and singleton setup
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeInstance()
        {
            if (instance != null)
            {
                Debug.LogError("Stop that.");
                return;
            }

            // Internal tracking
            GameObject coreObj = new GameObject("Game Core");
            Core core = coreObj.AddComponent<Core>();
            DontDestroyOnLoad(coreObj);
            instance = core;

        }

        private void OnGameInstanceInitConditionsMet()
        {
            // Data Initialization
            gameData = new GameInstance();
            gameData.PopulateDefaults(visualLookup);
        }

        public void TryRegisterVisualLookup(VisualLookup lookup)
        {
            if(visualLookup != null)
            {
                Debug.Log("Ignoring visual lookup registration attempt. Not a huge deal, just noted.");
                return;
            }

            visualLookup = lookup;

            OnGameInstanceInitConditionsMet();
        }

        public void TryRegisterAudioCore(AudioCore coreToRegister)
        {
            if(audioCore != null)
            {
                Debug.Log("Ignoring audio core registration attempt. Not necessarily anything wrong, just noting it.");
                return;
            }

            DontDestroyOnLoad(coreToRegister);
            coreToRegister.transform.SetParent(this.transform);
            coreToRegister.transform.position = Vector3.zero;

            audioCore = coreToRegister;
        }

        public void LoadLevelPlayfield()
        {
            SceneManager.LoadScene(SCENE_NAME_PLAYFIELD);
        }

        public void LoadLevelPlayfield(TextAsset levelToLoad)
        {
            gameData.currentPlayfield = levelToLoad;
            SceneManager.LoadScene(SCENE_NAME_PLAYFIELD);
        }

        public void LoadLevelMap()
        {
            SceneManager.LoadScene(SCENE_NAME_MAP);
        }
    }
}