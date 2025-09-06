using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;
using Loam;
using UnityEditorInternal;

namespace forest
{
    public class PlayfieldCore : MonoBehaviour
    {
        // Inspector
        [Header("General Links")]
        [Tooltip("The text asset that'll be used to build out the playfield. If left empty/null, the game core is checked.")]
        [SerializeField] private TextAsset levelOverride;
        [SerializeField] private Camera mainCam;
        [SerializeField] private PlayfieldUI ui;

        [Header("Links for States")]
        [SerializeField] private PlayfieldLookup playfieldLookup;
        [SerializeField] private VisualLookup visualLookup;
        [SerializeField] private VisualPlayfield visualizerPlayfield;

        // Various delays to space out the feel of the game, all in seconds
        [Header("Artificial Delays")]
        public float turnDelay = 0.2f;
        public float resultScreenTime = 3.0f;

        // Singleton/global access equivalent, exposing these to various states.
        public VisualPlayfield VisualPlayfield { get; private set; }
        public Playfield Playfield { get; private set; }
        public VisualLookup VisualLookup { get; private set; }
        public PlayfieldLookup PlayfieldLookup { get; private set; }
        public PlayfieldUI UI { get; private set; }
        public string PlayfieldName { get; private set; }

        // Internal
        private Playfield playfield;
        private CombatState current = null;

        /// <summary>
        /// Perform initial startup and core setup for systems
        /// </summary>
        void Start()
        {
            Postmaster.Instance.Configure(PostmasterConfig.Default());

            if (!TrySelectLevel(out TextAsset toLoad))
            {
                Debug.LogError("No playfield specified! Attempting to exit.");
                Exit();
                return;
            }

            playfield = Playfield.BuildPlayfield(toLoad.text);
            visualizerPlayfield.Initialize(visualLookup);
            visualizerPlayfield.DisplayAll(playfield);

            Utils.CenterCamera(mainCam, visualizerPlayfield);

            // Internal property setup
            PlayfieldName = toLoad.name;
            Playfield = playfield;
            VisualPlayfield = visualizerPlayfield;
            VisualLookup = visualLookup;
            PlayfieldLookup = playfieldLookup;
            UI = ui;

            // Connect up any global UI
            UI.buttonExit.onClick.AddListener(Exit);
            if (Playfield.portals?.Count == 1)
            {
                UI.buttonJumpToPortal.interactable = true;
                UI.buttonJumpToPortal.onClick.AddListener(() => { SetState<Combat100PortalWarp>(); });
            }

            // Pre-poke coroutine singleton by just doing some guaranteed function to force init.
            Loam.CoroutineObject.Instance.name.ToString();

            // State config
            SetState<Combat010Startup>();
        }

        /// <summary>
        /// Spin down the current playfield by setting the appropriate state and 
        /// </summary>
        private void Exit()
        {
            UI?.buttonJumpToPortal?.onClick.RemoveAllListeners();
            SetState<Combat200Shutdown>();
        }
        private bool TrySelectLevel(out TextAsset selected)
        {
            selected = null;

            // If we were given an override, just do that.
            if (levelOverride != null)
            {
                selected = levelOverride;
                Debug.LogWarning("Hey! You've overridden the level for the playfield, hopefully for testing. This will prevent normal progression otherwise.");
                return true;
            }

            // Try and collect level information from the game instance
            TextAsset coreValue = Core.Instance.gameData.currentPlayfield;
            if (coreValue != null)
            {
                selected = coreValue;

                // Note: Consume this data.
                Core.Instance.gameData.currentPlayfield = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Perform general tick-over of various systems, from current state to postmaster and onward.
        /// </summary>
        private void Update()
        {
            current?.Update();
            Postmaster.Instance.Upkeep();
        }

        /// <summary>
        /// When states are pushed, the starting of the new state occurs before the 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetState<T>() where T : CombatState
        {
            T instance = (T)System.Activator.CreateInstance(typeof(T), args: this);
            instance.Start();
            current?.Shutdown();
            current = instance;

            if (UI != null && UI.currentState != null)
            {
                UI.currentState.text = current?.GetType().Name;
            }
        }
    }
}