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
        [SerializeField] private TextAsset levelData;
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

        // Internal
        private CombatState current = null;

        /// <summary>
        /// Perform initial startup and core setup for systems
        /// </summary>
        void Start()
        {
            Postmaster.Instance.Configure(PostmasterConfig.Default());

            playfield = Playfield.BuildPlayfield(levelData.text);
            visualizerPlayfield.Initialize(lookup);
            visualizerPlayfield.DisplayAll(playfield);

            Utils.CenterCamera(mainCam, visualizerPlayfield);

            ConfigureInitialState();
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

            // Pre-poke coroutine singleton by just doing some guaranteed function to force init.
            Loam.CoroutineObject.Instance.name.ToString();

            // State config
            SetState<Combat01Startup>();
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