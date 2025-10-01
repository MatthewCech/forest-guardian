using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace forest
{
    public class Core : MonoBehaviour
    {
        /// <summary>
        /// Refers to a menu. While related to scene layout, this refers to conceptual levels whereas
        /// scenes refer more to memory. Since this is a small game, the venn diagram is near-circle here.
        /// </summary>
        public enum ForestLevel
        {
            DEFAULT = 0,

            MainMenu,
            Map,
            Playfield,
            Editor
        }

        public const string SCENE_NAME_PLAYFIELD = "Playfield";
        public const string SCENE_NAME_MAP = "Map";
        public const string SCENE_NAME_MAINMENU = "MainMenu";
        public const string SCENE_NAME_PLAYFIELDEDITOR = "PlayfieldEditor";

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

        public GameInstance GameData { get; private set; }
        public AudioCore AudioCore { get; private set; }
        public UICore UICore { get; private set; }
        public VisualLookup VisualLookup { get; private set; }

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
            GameData = new GameInstance();
            GameData.PopulateDefaults(VisualLookup);
        }

        public void TryRegisterVisualLookup(VisualLookup lookup)
        {
            if(VisualLookup != null)
            {
                Debug.Log("Ignoring visual lookup registration attempt. Not a huge deal, just noted.");
                return;
            }

            VisualLookup = lookup;

            OnGameInstanceInitConditionsMet();
        }

        public void TryRegisterAudioCore(AudioCore coreToRegister)
        {
            if(AudioCore != null)
            {
                Debug.Log("Ignoring audio core registration attempt. Not necessarily anything wrong, just noting it.");
                return;
            }

            DontDestroyOnLoad(coreToRegister);
            coreToRegister.transform.SetParent(this.transform);
            coreToRegister.transform.position = Vector3.zero;

            AudioCore = coreToRegister;
        }

        public void TryRegisterUICore(UICore coreToRegister)
        {
            if (UICore != null)
            {
                Debug.Log("Ignoring extra UI core registration attempt. Not necessarily anything wrong, just noting it.");
                return;
            }

            DontDestroyOnLoad(coreToRegister);
            coreToRegister.transform.SetParent(this.transform);
            coreToRegister.transform.position = Vector3.zero;

            UICore = coreToRegister;
        }

        public void SetPlayfieldAndLoad(TextAsset levelToLoad)
        {
            GameData.currentPlayfield = levelToLoad;
            LoadLevel(ForestLevel.Playfield);
        }

        public void LoadLevel(ForestLevel level)
        {
            switch (level)
            {
                case ForestLevel.MainMenu:
                    SceneManager.LoadScene(SCENE_NAME_MAINMENU);
                    break;

                case ForestLevel.Map:
                    SceneManager.LoadScene(SCENE_NAME_MAP);
                    break;

                case ForestLevel.Playfield:
                    SceneManager.LoadScene(SCENE_NAME_PLAYFIELD);
                    break;

                case ForestLevel.Editor:
                    SceneManager.LoadScene(SCENE_NAME_PLAYFIELDEDITOR);
                    break;

                case ForestLevel.DEFAULT:
                default:
                    throw new System.Exception("No level specified to load for this button! You must select a NON-DEFAULT from the enum in the inspector.");
            }
        }
    }
}