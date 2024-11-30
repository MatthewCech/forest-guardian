using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;
using Loam;
using static UnityEngine.UI.CanvasScaler;

namespace forest
{
    public class PlayfieldCore : MonoBehaviour
    {
        // Inspector
        [Header("General Links")]
        [Tooltip("The text asset that'll be used to build out the playfield. If left empty/null, the game core is checked.")]
        [SerializeField] private TextAsset levelOverride;
        [SerializeField] private Camera mainCam;
        [SerializeField] private UIDocument ui;

        [Header("Links for States")]
        [SerializeField] private VisualLookup lookup;
        [SerializeField] private Playfield playfield;
        [SerializeField] private VisualPlayfield visualizerPlayfield;      

        // Various delays to space out the feel of the game, all in seconds
        [Header("Artificial Delays")] 
        public float turnDelay = 0.2f;        
        public float resultScreenTime = 3.0f;

        // Singleton/global access equivalent, exposing these to various states.
        public VisualPlayfield VisualPlayfield { get; private set; }
        public Playfield Playfield { get; private set; }
        public VisualLookup Lookup { get; private set; }
        public UIDocument UI { get; private set; }

        // Internal
        private CombatState current = null;

        /// <summary>
        /// Perform initial startup and core setup for systems
        /// </summary>
        void Start()
        {
            Postmaster.Instance.Configure(PostmasterConfig.Default());

            if(!TrySelectLevel(out TextAsset toLoad))
            {
                Debug.LogError("No playfield specified! Attempting to exit.");
                SetState<Combat10Shutdown>();
                return;
            }

            playfield = Playfield.BuildPlayfield(toLoad.text);
            visualizerPlayfield.Initialize(lookup);
            visualizerPlayfield.DisplayAll(playfield);

            Utils.CenterCamera(mainCam, visualizerPlayfield);

            ConfigureInitialState();
        }

        private bool TrySelectLevel(out TextAsset selected)
        {
            selected = null;

            // If we were given an override, just do that.
            if (levelOverride != null)
            {
                selected = levelOverride;
                return true;
            }

            // Try and collect level information from the game instance
            TextAsset coreValue = Core.Instance.game.currentPlayfield;
            if(coreValue != null) 
            {
                selected = coreValue;

                // Note: Consume this data.
                Core.Instance.game.currentPlayfield = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set properties and configure state for playing
        /// </summary>
        private void ConfigureInitialState()
        {
            // Internal links
            Playfield = playfield;
            VisualPlayfield = visualizerPlayfield;
            Lookup = lookup;
            UI = ui;

            // Connect relevant UI.
            ui.rootVisualElement.Q<Button>("buttonMap").clicked += Exit;

            // Pre-poke coroutine singleton by just doing some guaranteed function to force init.
            Loam.CoroutineObject.Instance.name.ToString();

            // State config
            SetState<Combat01Startup>();
        }

        private void Exit()
        {
            SetState<Combat10Shutdown>();
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
            T instance = (T)System.Activator.CreateInstance(typeof(T), args:this);
            instance.Start();
            current?.Shutdown();
            ui.rootVisualElement.Q<Label>("bannerLabel").text = current?.GetType().Name;
            current = instance;
        }
    }
}