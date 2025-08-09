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

        public GameInstance game;

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

            // Initialization
            core.game = new GameInstance();
        }

        public void LoadLevelPlayfield()
        {
            SceneManager.LoadScene(SCENE_NAME_PLAYFIELD);
        }

        public void LoadLevelPlayfield(TextAsset levelToLoad)
        {
            game.currentPlayfield = levelToLoad;
            SceneManager.LoadScene(SCENE_NAME_PLAYFIELD);
        }

        public void LoadLevelMap()
        {
            SceneManager.LoadScene(SCENE_NAME_MAP);
        }
    }
}