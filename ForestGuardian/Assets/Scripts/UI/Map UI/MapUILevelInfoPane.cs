using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Loam;
using UnityEngine.UI;



namespace forest
{
    public class MapUILevelInfoPane : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private MapUIMenu mapUIMenu;

        [SerializeField] private TMPro.TextMeshProUGUI levelTitle;
        [SerializeField] private TMPro.TextMeshProUGUI levelFloors;
        [SerializeField] private TMPro.TextMeshProUGUI levelDescription;
        [SerializeField] private Button buttonExit;
        [SerializeField] private Button buttonStart;

        [Header("lerp")]
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float lerpTime = 2f;
        [SerializeField] private float exitRange = 0.5f;

        private MapInteractionPoint interactionPoint;
        private float timeSoFar = 0f;
        private bool isMoving = false;
        private Vector3 startPosition;
        private Vector3 targetPosition;

        private MessageSubscription handleMsgShowLevelInfo;

        private void OnEnable()
        {
            buttonStart.onClick.AddListener(StartLevel);
            buttonExit.onClick.AddListener(ExitPanel);
        }

        private void OnDisable()
        {
            buttonExit.onClick.AddListener(ExitPanel);
            buttonStart.onClick.AddListener(StartLevel);
        }

        private void Start()
        {
            canvasGroup.SetCanvasActive(false);
            handleMsgShowLevelInfo = Postmaster.Instance.Subscribe<MsgShowLevelInfo>(ShowLevelData);
        }

        private void OnDestroy()
        {
            handleMsgShowLevelInfo.Dispose();
        }

        private void ShowLevelData(Loam.Message raw)
        {
            MsgShowLevelInfo msg = raw as MsgShowLevelInfo;
            MapInteractionPoint interactable = msg.mapInteractionPoint;
            canvasGroup.SetCanvasActive(true);

            levelTitle.text = interactable.LevelName;
            levelFloors.text = interactable.LevelDepth.ToString();

            // LERP content
            timeSoFar = 0;
            interactionPoint = interactable;
            targetPosition = interactable.transform.position;
            startPosition = mapUIMenu.cameraPivot.position;
            isMoving = true;
        }

        private void Update()
        {
            if (isMoving)
            {
                timeSoFar += Time.deltaTime;
                float t = timeSoFar / lerpTime;
                mapUIMenu.cameraPivot.position = Vector3.Lerp(startPosition, targetPosition, curve.Evaluate(t));

                if(t >= 1)
                {
                    isMoving = false;
                }
            }

            if(interactionPoint != null && isMoving == false)
            {
                if(Vector3.Distance(mapUIMenu.cameraPivot.position, interactionPoint.transform.position) > exitRange)
                {
                    canvasGroup.SetCanvasActive(false);
                    Postmaster.Instance.Send(new MsgHideLevelInfo() { mapInteractionPoint = interactionPoint });
                    interactionPoint = null;
                }
            }
        }

        // Start is called before the first frame update
        void StartLevel()
        {
            ExitPanel();

            TextAsset levelData = interactionPoint.LevelData;
            UnityEngine.Assertions.Assert.IsNotNull(levelData, "Level data must be specified!");
            Debug.Log($"Attempting to load {levelData.name}...");
            Core.Instance.SetPlayfieldAndLoad(levelData);
        }

        // Update is called once per frame
        void ExitPanel()
        {
            canvasGroup.SetCanvasActive(false);
            Postmaster.Instance.Send(new MsgHideLevelInfo() { mapInteractionPoint = interactionPoint });
        }
    }
}