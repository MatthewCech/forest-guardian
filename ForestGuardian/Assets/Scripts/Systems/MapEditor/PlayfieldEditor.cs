using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.UI;
using Loam;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Events;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace forest
{
    public class PlayfieldEditor : MonoBehaviour
    {
        public const string LEVEL_FILE_EXTENSION = "txt";

        [Header("Editor Settings")]
        [SerializeField] private KeyCode mainModifier = KeyCode.LeftShift;
        [SerializeField] private KeyCode extraModifier = KeyCode.LeftControl;

        [Header("Required Links")]
        [SerializeField] private VisualLookup lookup;
        [SerializeField] private VisualPlayfield visuals;
        [SerializeField] private Camera displayingCamera;
        [SerializeField] PlayfieldEditorCamera playfieldEditorCamera;

        [Header("General Config")]
        [SerializeField] private Button uiSave;
        [SerializeField] private Button uiLoad;
        [SerializeField] private Slider uiWidthSlider;
        [SerializeField] private Slider uiHeightSlider;
        [SerializeField] private TMPro.TMP_InputField uiWidthEntryField;
        [SerializeField] private TMPro.TMP_InputField uiHeightEntryField;
        [SerializeField] private TMPro.TextMeshProUGUI uiStatus;
        [SerializeField] private TMPro.TextMeshProUGUI uiLayer;
        [SerializeField] private TMPro.TMP_InputField uiTagLabelEntryField;
        [SerializeField] private TMPro.TMP_InputField uiTagBestowedEntryField;
        [SerializeField] private TMPro.TextMeshProUGUI uiTagLabelValue;
        [SerializeField] private TMPro.TextMeshProUGUI uiTagBestowedValue;
        [SerializeField] private Toggle uiIsPlayerTeam;
        [SerializeField] private Toggle uiIsEnemyTeam;

        [Header("Metadata Panel")]
        [SerializeField] private TMPro.TMP_InputField inputMetadataPanel;
        [SerializeField] private CanvasGroup metadataPanel;
        [SerializeField] private TMPro.TextMeshProUGUI metadataPanelLabel;
        [SerializeField] private Button metadataPanelCloseButton;

        [Header("Selectable Entries")]
        [SerializeField] private PlayfieldEditorUISelectable selectableEntryTemplate;
        [SerializeField] private PlayfieldEditorUISelectableLabel selectableEntryLabelTemplate;
        [SerializeField] private Transform previewTarget;

        
        // Internal
        private string nothingTileTag;
        private Playfield workingPlayfield;
        private Transform selectableEntryParent;

        private GameObject previewObject;
        private string previewTag;
        private PlayfieldEditorSelectionType previewType;

        // NOTE: Can combine these if there's a common base class.
        private PlayfieldPortal selectedPortal;
        private PlayfieldOrigin selectedOrigin;
        private bool isShowingMetadataPanel = false;

        public void Start()
        {
            previewObject = null;
            previewTag = "";
            previewType = PlayfieldEditorSelectionType.NONE;
            nothingTileTag = lookup.tileTemplates[0].name; // Set nothing tile tag

            selectableEntryParent = selectableEntryTemplate.transform.parent;
            selectableEntryTemplate.gameObject.SetActive(false);
            selectableEntryLabelTemplate.gameObject.SetActive(false);

            visuals.Initialize(lookup);

            uiSave.onClick.AddListener(SaveCreatedPlayfield);
            uiLoad.onClick.AddListener(LoadCreatedPlayfield);
            metadataPanelCloseButton.onClick.AddListener(HideMetadataPanel);

            uiWidthSlider.onValueChanged.AddListener(SizeChange);
            uiHeightSlider.onValueChanged.AddListener(SizeChange);
            uiTagBestowedEntryField.onValueChanged.AddListener(TagBestowedChange);
            uiTagLabelEntryField.onValueChanged.AddListener(TagLabelChange);

            inputMetadataPanel.onValueChanged.AddListener(PlayfieldObjectMetadataModified);

            uiWidthEntryField.text = uiWidthSlider.value.ToString();
            uiHeightEntryField.text = uiHeightSlider.value.ToString();

            workingPlayfield = CreatePlayfield();
            visuals.DisplayAll(workingPlayfield);
            Utils.CenterCamera(displayingCamera, visuals);

            Postmaster.Instance.Subscribe<MsgTilePrimaryAction>(TilePrimaryAction);
            Postmaster.Instance.Subscribe<MsgItemPrimaryAction>(ItemPrimaryAction);
            Postmaster.Instance.Subscribe<MsgUnitPrimaryAction>(UnitPrimaryAction);
            Postmaster.Instance.Subscribe<MsgPortalPrimaryAction>(PortalPrimaryAction);
            Postmaster.Instance.Subscribe<MsgExitPrimaryAction>(ExitPrimaryAction);
            Postmaster.Instance.Subscribe<MsgOriginPrimaryAction>(OriginPrimaryAction);

            Postmaster.Instance.Subscribe<MsgUnitSecondaryAction>(UnitSecondaryAction);
            Postmaster.Instance.Subscribe<MsgTileSecondaryAction>(TileSecondaryAction);
            Postmaster.Instance.Subscribe<MsgItemSecondaryAction>(ItemSecondaryAction);
            Postmaster.Instance.Subscribe<MsgPortalSecondaryAction>(PortalSecondaryAction);
            Postmaster.Instance.Subscribe<MsgExitSecondaryAction>(ExitSecondaryAction);
            Postmaster.Instance.Subscribe<MsgOriginSecondaryAction>(OriginSecondaryAction);

            CreateSelectableButtons();

            // Set tags as default
            TagLabelChange("");
            TagBestowedChange("");

            // Basic tile
            Tile tile = lookup.tileTemplates[1];
            SetPreview(tile.gameObject, tile.name, PlayfieldEditorSelectionType.Tile);

        }

        private void TilePrimaryAction(Message raw) { ProcessPrimaryAction((raw as MsgTilePrimaryAction).tilePosition); }
        private void UnitPrimaryAction(Message raw) { ProcessPrimaryAction((raw as MsgUnitPrimaryAction).position); }
        private void ItemPrimaryAction(Message raw) { ProcessPrimaryAction((raw as MsgItemPrimaryAction).position); }
        private void PortalPrimaryAction(Message raw) { ProcessPrimaryAction((raw as MsgPortalPrimaryAction).position); }
        private void ExitPrimaryAction(Message raw) { ProcessPrimaryAction((raw as MsgExitPrimaryAction).position); }
        private void OriginPrimaryAction(Message raw) { ProcessPrimaryAction((raw as MsgOriginPrimaryAction).position); }

        private void TileSecondaryAction(Message raw) { ProcessSecondaryAction((raw as MsgTileSecondaryAction).position); }
        private void UnitSecondaryAction(Message raw) { ProcessSecondaryAction((raw as MsgUnitSecondaryAction).position); }
        private void ItemSecondaryAction(Message raw) { ProcessSecondaryAction((raw as MsgItemSecondaryAction).position); }
        private void PortalSecondaryAction(Message raw) { ProcessSecondaryAction((raw as MsgPortalSecondaryAction).position); }
        private void ExitSecondaryAction(Message raw) { ProcessSecondaryAction((raw as MsgExitSecondaryAction).position); }
        private void OriginSecondaryAction(Message raw) { ProcessSecondaryAction((raw as MsgOriginSecondaryAction).position); }

        private void AddSelectableLabel(string label)
        {
            PlayfieldEditorUISelectableLabel labelObj = GameObject.Instantiate(selectableEntryLabelTemplate, selectableEntryParent);
            labelObj.label.text = label;
            labelObj.gameObject.SetActive(true);
        }

        private void PlayfieldObjectMetadataModified(string value)
        {
            if(selectedPortal != null)
            {
                selectedPortal.target = value;
                return;
            }
            else if(selectedOrigin != null)
            {
                if(int.TryParse(value.Trim(), out int parsed))
                {
                    selectedOrigin.partyIndex = parsed;
                }
                else
                {
                    Debug.LogWarning("Invalid number provided, but that's fine if typing was still happening");
                }
                return;
            }
        }

        /// <summary>
        /// Create UI for items like tools in a general editor toolbox
        /// </summary>
        private void CreateSelectableButtons()
        {
            AddSelectableLabel("Tiles");
            foreach (Tile tileToCreate in lookup.tileTemplates)
            {
                if(tileToCreate.name == nothingTileTag)
                {
                    continue;
                }

                PlayfieldEditorUISelectable tile = GameObject.Instantiate(selectableEntryTemplate, selectableEntryParent);
                tile.SetData(tileToCreate.gameObject, PlayfieldEditorSelectionType.Tile, tileToCreate.name, ProcessedSelectableClick);
                
                tile.gameObject.SetActive(true);
            }

            AddSelectableLabel("Units");
            foreach (Unit unitToCreate in lookup.unitTemplates)
            {
                PlayfieldEditorUISelectable unit = GameObject.Instantiate(selectableEntryTemplate, selectableEntryParent);
                unit.SetData(unitToCreate.gameObject, PlayfieldEditorSelectionType.Unit, unitToCreate.name, ProcessedSelectableClick);

                unit.gameObject.SetActive(true);
            }

            AddSelectableLabel("Items");
            foreach (Item itemToCreate in lookup.itemTemplates)
            {
                PlayfieldEditorUISelectable item = GameObject.Instantiate(selectableEntryTemplate, selectableEntryParent);
                item.SetData(itemToCreate.gameObject, PlayfieldEditorSelectionType.Item, itemToCreate.name, ProcessedSelectableClick);

                item.gameObject.SetActive(true);
            }

            AddSelectableLabel("Portal");
            PlayfieldEditorUISelectable portal = GameObject.Instantiate(selectableEntryTemplate, selectableEntryParent);
            portal.SetData(lookup.portalTemplate.gameObject, PlayfieldEditorSelectionType.Portal, lookup.PortalTemplate.name, ProcessedSelectableClick);
            portal.gameObject.SetActive(true);

            AddSelectableLabel("Origin");
            PlayfieldEditorUISelectable origin = GameObject.Instantiate(selectableEntryTemplate, selectableEntryParent);
            origin.SetData(lookup.originTemplate.gameObject, PlayfieldEditorSelectionType.Origin, lookup.OriginTemplate.name, ProcessedSelectableClick);
            origin.gameObject.SetActive(true);

            AddSelectableLabel("Exit");
            PlayfieldEditorUISelectable exit = GameObject.Instantiate(selectableEntryTemplate, selectableEntryParent);
            exit.SetData(lookup.exitTemplate.gameObject, PlayfieldEditorSelectionType.Exit, lookup.ExitTemplate.name, ProcessedSelectableClick);
            exit.gameObject.SetActive(true);
        }

        private void ProcessedSelectableClick(PlayfieldEditorUISelectable previewClicked)
        {
            SetPreview(previewClicked.Visual, previewClicked.SelectableTag, previewClicked.SelectableType);
        }

        private void SetPreview(GameObject toPreview, string tag, PlayfieldEditorSelectionType type)
        {
            previewTag = tag;
            previewType = type;

            if (previewObject != null)
            {
                DestroyImmediate(previewObject);
            }

            if(previewType != PlayfieldEditorSelectionType.Portal)
            {
                HideMetadataPanel();
            }

            // Strip any trail lines
            previewObject = GameObject.Instantiate(toPreview, previewTarget);
            LineRenderer[] renderers = previewObject.GetComponentsInChildren<LineRenderer>();
            foreach(LineRenderer renderer in renderers)
            {
                Destroy(renderer);
            }

            previewObject.gameObject.SetActive(true);
        }

        private void Update()
        {
            uiLayer.text = previewType.ToString();
            string action = "Placing";
            if(IsMainInputModifierDown())
            {
                action = "Removing";
            }

            if(IsExtraInputModifierDown())
            {
                action = "Writing";
            }

            uiStatus.text = $"{action} {previewType.ToString()}";

            playfieldEditorCamera.isEnabledWASD = !isShowingMetadataPanel;
        }

        private void HideMetadataPanel()
        {
            metadataPanel.alpha = 0;
            metadataPanel.interactable = false;
            metadataPanel.blocksRaycasts = false;

            selectedPortal = null;
            selectedOrigin = null;

            isShowingMetadataPanel = false;
            visuals.DisplayAll(workingPlayfield);
        }

        private void ShowMetadataPanel(PlayfieldOrigin origin)
        {
            selectedOrigin = origin;
            selectedPortal = null;
            BaseShowMetadataPanel("Origin", selectedOrigin.location, origin.partyIndex.ToString());
        }

        private void ShowMetadataPanel(PlayfieldPortal portal)
        { 
            selectedOrigin = null;
            selectedPortal = portal;
            BaseShowMetadataPanel("Portal", selectedPortal.location, selectedPortal.target);
        }

        private void BaseShowMetadataPanel(string label, Vector2Int highlightPos, string startValue)
        {
            metadataPanel.alpha = 1;
            metadataPanel.interactable = true;
            metadataPanel.blocksRaycasts = true;

            inputMetadataPanel.text = startValue;

            metadataPanelLabel.text = $"{label} @ ({highlightPos.x},{highlightPos.y})";
            visuals.DisplayIndicatorAt(highlightPos.x, highlightPos.y, workingPlayfield, lookup.movePreviewTemplate);

            isShowingMetadataPanel = true;
        }

        private void ProcessPrimaryAction(Vector2Int position)
        {
            if(IsMainInputModifierDown())
            {
                ProcessSecondaryAction(position);
                return;
            }

            if(IsExtraInputModifierDown())
            {
                if(previewType == PlayfieldEditorSelectionType.Portal)
                {
                    if(workingPlayfield.TryGetPortalAt(position, out PlayfieldPortal portal))
                    {
                        if(isShowingMetadataPanel)
                        {
                            HideMetadataPanel();
                        }
                        else
                        {
                            ShowMetadataPanel(portal);
                        }
                    }
                }
                else if (previewType == PlayfieldEditorSelectionType.Origin)
                {
                    if(workingPlayfield.TryGetOriginAt(position, out PlayfieldOrigin origin))
                    {
                        if(isShowingMetadataPanel)
                        {
                            HideMetadataPanel();
                        }
                        else
                        {
                            ShowMetadataPanel(origin);
                        }
                    }
                }

                return;
            }

            if (previewType == PlayfieldEditorSelectionType.Tile)
            {
                PlayfieldTile tile = workingPlayfield.world.Get(position);
                tile.tag = previewTag;
            }
            else if (previewType == PlayfieldEditorSelectionType.Unit)
            {
                if (workingPlayfield.TryGetUnitAt(position, out PlayfieldUnit unit))
                {
                    unit.tag = previewTag;
                }
                else
                {
                    PlayfieldUnit newUnit = new PlayfieldUnit();
                    newUnit.locations.Add(position);
                    newUnit.tag = previewTag;
                    newUnit.id = workingPlayfield.GetNextID();

                    // Map checkboxes to team
                    newUnit.team = Team.DEFAULT;
                    if(uiIsPlayerTeam.isOn)
                    {
                        newUnit.team = Team.Player;
                    }
                    else if (uiIsEnemyTeam.isOn)
                    {
                        newUnit.team = Team.Opponent;
                    }
                    
                    workingPlayfield.units.Add(newUnit);
                }
            }
            else if (previewType == PlayfieldEditorSelectionType.Item)
            {
                if (workingPlayfield.TryGetItemAt(position, out PlayfieldItem item))
                {
                    item.tag = previewTag;
                }
                else
                {
                    PlayfieldItem newItem = new PlayfieldItem();
                    newItem.location = position;
                    newItem.tag = previewTag;
                    newItem.id = workingPlayfield.GetNextID();

                    workingPlayfield.items.Add(newItem);
                }
            }
            else if (previewType == PlayfieldEditorSelectionType.Portal)
            {
                // No placing at the same spot as an Origin or Exit
                if (workingPlayfield.TryGetOriginAt(position, out PlayfieldOrigin _)
                    || workingPlayfield.TryGetExitAt(position, out PlayfieldExit _))
                {
                    return;
                }

                if (!workingPlayfield.TryGetPortalAt(position, out PlayfieldPortal portal))
                {
                    PlayfieldPortal newPortal = new PlayfieldPortal();
                    newPortal.location = position;
                    newPortal.id = workingPlayfield.GetNextID();

                    workingPlayfield.portals.Add(newPortal);
                }

                HideMetadataPanel();
            }
            else if (previewType == PlayfieldEditorSelectionType.Origin)
            {
                // No placing at the same spot as a Portal or Exit
                if(workingPlayfield.TryGetPortalAt(position, out PlayfieldPortal _)
                    || workingPlayfield.TryGetExitAt(position, out PlayfieldExit _))
                {
                    return;
                }

                if (!workingPlayfield.TryGetOriginAt(position, out PlayfieldOrigin origin))
                {
                    PlayfieldOrigin newOrigin = new PlayfieldOrigin();
                    newOrigin.location = position;
                    newOrigin.id = workingPlayfield.GetNextID();

                    workingPlayfield.origins.Add(newOrigin);
                }

                HideMetadataPanel();
            }
            else if (previewType == PlayfieldEditorSelectionType.Exit)
            {
                // No placing at the same spot as a Portal or Origin
                if (workingPlayfield.TryGetPortalAt(position, out PlayfieldPortal _)
                    || workingPlayfield.TryGetOriginAt(position, out PlayfieldOrigin _))
                {
                    return;
                }

                if (workingPlayfield.exit != null)
                {
                    workingPlayfield.exit = null;
                }

                PlayfieldExit newExit = new PlayfieldExit();
                newExit.location = position;
                newExit.id = workingPlayfield.GetNextID();
                workingPlayfield.exit = newExit;
            }

            visuals.DisplayAll(workingPlayfield);
        }

        private void ProcessSecondaryAction(Vector2Int position)
        {
            if (previewType == PlayfieldEditorSelectionType.Tile)
            {
                PlayfieldTile tile = workingPlayfield.world.Get(position);
                tile.tag = nothingTileTag;
            }
            else if (previewType == PlayfieldEditorSelectionType.Unit)
            {
                workingPlayfield.RemoveUnitAt(position);
            }
            else if (previewType == PlayfieldEditorSelectionType.Item)
            {
                workingPlayfield.RemoveItemAt(position);
            }
            else if (previewType == PlayfieldEditorSelectionType.Portal)
            {
                workingPlayfield.RemovePortalAt(position);
            }
            else if (previewType == PlayfieldEditorSelectionType.Origin)
            {
                workingPlayfield.RemoveOriginAt(position);
            }
            else if (previewType == PlayfieldEditorSelectionType.Exit)
            {
                workingPlayfield.RemoveExitAt(position);
            }

            visuals.DisplayAll(workingPlayfield);
        }

        /// <summary>
        /// Get index of existing element by name/tag within the lookup list
        /// </summary>
        private int GetTemplateIndex<T>(List<T> tempaltes, string nameToFind) where T : MonoBehaviour
        {
            for (int i = 0; i < tempaltes.Count; ++i)
            {
                MonoBehaviour template = tempaltes[i];
                if (template.name.Equals(nameToFind, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private bool IsMainInputModifierDown()
        {
            return Input.GetKey(mainModifier);
        }

        private bool IsExtraInputModifierDown()
        {
            return Input.GetKey(extraModifier);
        }


        private Playfield CreatePlayfield(Playfield existing = null)
        {
            // Get world size
            int newWidth = Mathf.RoundToInt(uiWidthSlider.value);
            int newHeight = Mathf.RoundToInt(uiHeightSlider.value);

            // Create and configure blank playfield
            Playfield newPlayfield = new Playfield();
            newPlayfield.items = new List<PlayfieldItem>();
            newPlayfield.units = new List<PlayfieldUnit>();
            newPlayfield.world = new Collection2D<PlayfieldTile>(newWidth, newHeight);
            newPlayfield.portals = new List<PlayfieldPortal>();
            newPlayfield.origins = new List<PlayfieldOrigin>();
            newPlayfield.exit = null;

            for (int x = 0; x < newWidth; ++x)
            {
                for (int y = 0; y < newHeight; ++y)
                {
                    PlayfieldTile tile = new PlayfieldTile();
                    tile.tag = lookup.defaultTileTemplate.name;
                    tile.id = newPlayfield.GetNextID();
                    newPlayfield.world.Set(x, y, tile);
                }
            }

            if(existing != null)
            {
                MoveDataToNewPlayfield(existing, newPlayfield);
            }

            // TODO: Separated like this to allow moving existing data over.
            return newPlayfield;
        }

        private void MoveDataToNewPlayfield(Playfield existing, Playfield newPlayfield)
        {
            // Local function to check if we're within bounds.
            bool InCombinedBounds(Vector2Int pos)
            {
                bool inFirst = pos.x < existing.world.GetWidth() && pos.y < existing.world.GetHeight();
                bool inSecond = pos.x < newPlayfield.world.GetWidth() && pos.y < newPlayfield.world.GetHeight();
                return inFirst && inSecond;
            }

            newPlayfield.world.ScrapeDataFrom(existing.world);

            // Back to front move over units that are still in the new bounds of the playfield.
            // The entire location/body of the unit must be in the new resize playfield or it all gets removed.
            for (int i = existing.units.Count - 1; i >= 0; --i)
            {
                PlayfieldUnit unit = existing.units[i];

                bool shouldRemove = false;
                for (int bodyPos = 0; bodyPos < unit.locations.Count; ++bodyPos)
                {
                    Vector2Int cur = unit.locations[bodyPos];

                    if (!InCombinedBounds(cur))
                    {
                        shouldRemove = true;
                        break;
                    }
                }

                if (shouldRemove)
                {
                    existing.units.RemoveAt(i);
                    continue;
                }
            }
            newPlayfield.units = existing.units;

            for (int i = existing.items.Count - 1; i >= 0; --i)
            {
                PlayfieldItem item = existing.items[i];
                Vector2Int cur = item.location;

                if (!InCombinedBounds(cur))
                {
                    existing.units.RemoveAt(i);
                    continue;
                }
            }
            newPlayfield.items = existing.items;

            for(int i = existing.portals.Count - 1; i >= 0; --i)
            {
                PlayfieldPortal portal = existing.portals[i];
                Vector2Int cur = portal.location;

                if(!InCombinedBounds(cur))
                {
                    existing.portals.RemoveAt(i);
                    continue;
                }
            }
            newPlayfield.portals = existing.portals;

            for (int i = existing.origins.Count - 1; i >= 0; --i)
            {
                PlayfieldOrigin origin = existing.origins[i];
                Vector2Int cur = origin.location;

                if (!InCombinedBounds(cur))
                {
                    existing.origins.RemoveAt(i);
                    continue;
                }
            }
            newPlayfield.origins = existing.origins;

            if(existing.exit != null)
            {
                Vector2Int curExitPos = existing.exit.location;
                if(!InCombinedBounds(curExitPos))
                {
                    existing.exit = null;
                }
            }
            newPlayfield.exit = existing.exit;

            // Other data
            newPlayfield.tagLabel = existing.tagLabel;
            newPlayfield.tagsBestowed = existing.tagsBestowed;
        }

        private void SizeChange(float e)
        {
            workingPlayfield = CreatePlayfield(workingPlayfield);
            visuals.DisplayAll(workingPlayfield);
            Utils.CenterCamera(displayingCamera, visuals);

            // Readouts
            uiWidthEntryField.text = uiWidthSlider.value.ToString();
            uiHeightEntryField.text = uiHeightSlider.value.ToString();

            workingPlayfield.SetBestowedList(uiTagBestowedValue.text);
            workingPlayfield.tagLabel = uiTagLabelValue.text;
        }

        private void TagBestowedChange(string value)
        {
            workingPlayfield.SetBestowedList(value);
            uiTagBestowedValue.text = workingPlayfield.GetBestowedList();
        }

        private void TagLabelChange(string value)
        {
            workingPlayfield.tagLabel = value;
            uiTagLabelValue.text = workingPlayfield.tagLabel;
        }

        private void SaveCreatedPlayfield()
        {

#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.SaveFilePanel("Select a place to save the level", Application.dataPath, "NewPlayfield", LEVEL_FILE_EXTENSION);
            if(path == null || path.Length == 0)
            {
                return;
            }

            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter writer = new StreamWriter(path))
            {
                string toPrint = JsonConvert.SerializeObject(workingPlayfield, Formatting.Indented);
                writer.Write(toPrint);
            }
#endif
        }

        private void LoadCreatedPlayfield()
        {
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.OpenFilePanel("Select a level to load", Application.dataPath, LEVEL_FILE_EXTENSION);
            if(path == null || path.Length == 0)
            {
                Debug.LogWarning("Undefined path selected");
                return;
            }

            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader reader = new StreamReader(path))
            {
                string all = reader.ReadToEnd();
                Playfield field = JsonConvert.DeserializeObject<Playfield>(all);

                workingPlayfield = field;
                if(!workingPlayfield.Validate())
                {
                    Debug.LogError("Mis-configured playfield");
                }

                uiTagBestowedValue.text = field.GetBestowedList();
                uiTagBestowedEntryField.text = uiTagBestowedValue.text;
                uiTagLabelValue.text = field.tagLabel;
                uiTagLabelEntryField.text = uiTagLabelValue.text;

                uiWidthSlider.SetValueWithoutNotify(workingPlayfield.world.GetWidth());
                uiWidthEntryField.SetTextWithoutNotify(workingPlayfield.world.GetWidth().ToString());
                uiHeightSlider.SetValueWithoutNotify(workingPlayfield.world.GetHeight());
                uiHeightEntryField.SetTextWithoutNotify(workingPlayfield.world.GetHeight().ToString());

                visuals.DisplayAll(workingPlayfield);
            }
#endif
        }
    }
}
