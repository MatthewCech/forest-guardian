using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Loam;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using SimpleFileBrowser;

namespace forest
{
    public class PlayfieldEditor : MonoBehaviour
    {
        public const string LEVEL_FILE_EXTENSION = "json"; // do not include the dot

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
        [SerializeField] private TMPro.TMP_InputField uiDescriptionField;
        [SerializeField] private TMPro.TMP_InputField uiTagBestowedEntryField;
        [SerializeField] private TMPro.TextMeshProUGUI uiTagLabelValue;
        [SerializeField] private TMPro.TextMeshProUGUI uiDescriptionValue;
        [SerializeField] private TMPro.TextMeshProUGUI uiTagBestowedValue;
        [SerializeField] private Toggle uiIsPlayerTeam;
        [SerializeField] private Toggle uiIsEnemyTeam;
        [SerializeField] private Toggle uiIsForcePrimary;
        [SerializeField] private Toggle uiIsForceSecondary;
        [SerializeField] private Toggle uiIsForceAlternative;

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

        private List<MessageSubscription> subs = new List<MessageSubscription>();

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
            uiDescriptionField.onValueChanged.AddListener(DescriptionChanged);
            uiTagLabelEntryField.onValueChanged.AddListener(TagLabelChange);

            inputMetadataPanel.onValueChanged.AddListener(PlayfieldObjectMetadataModified);

            uiWidthEntryField.text = uiWidthSlider.value.ToString();
            uiHeightEntryField.text = uiHeightSlider.value.ToString();

            int newWidth = Mathf.RoundToInt(uiWidthSlider.value);
            int newHeight = Mathf.RoundToInt(uiHeightSlider.value);

            workingPlayfield = Utils.CreatePlayfield(newWidth, newHeight);
            visuals.DisplayAll(workingPlayfield);
            Utils.CenterCamera(displayingCamera, visuals);

            subs.Add(Postmaster.Instance.Subscribe<MsgTilePrimaryAction>(TilePrimaryAction));
            subs.Add(Postmaster.Instance.Subscribe<MsgItemPrimaryAction>(ItemPrimaryAction));
            subs.Add(Postmaster.Instance.Subscribe<MsgUnitPrimaryAction>(UnitPrimaryAction));
            subs.Add(Postmaster.Instance.Subscribe<MsgPortalPrimaryAction>(PortalPrimaryAction));
            subs.Add(Postmaster.Instance.Subscribe<MsgExitPrimaryAction>(ExitPrimaryAction));
            subs.Add(Postmaster.Instance.Subscribe<MsgOriginPrimaryAction>(OriginPrimaryAction));

            subs.Add(Postmaster.Instance.Subscribe<MsgUnitSecondaryAction>(UnitSecondaryAction));
            subs.Add(Postmaster.Instance.Subscribe<MsgTileSecondaryAction>(TileSecondaryAction));
            subs.Add(Postmaster.Instance.Subscribe<MsgItemSecondaryAction>(ItemSecondaryAction));
            subs.Add(Postmaster.Instance.Subscribe<MsgPortalSecondaryAction>(PortalSecondaryAction));
            subs.Add(Postmaster.Instance.Subscribe<MsgExitSecondaryAction>(ExitSecondaryAction));
            subs.Add(Postmaster.Instance.Subscribe<MsgOriginSecondaryAction>(OriginSecondaryAction));

            CreateSelectableButtons();

            // Set tags as default
            TagLabelChange("");
            TagBestowedChange("");
            DescriptionChanged("");

            // Basic tile
            Tile tile = lookup.tileTemplates[1];
            SetPreview(tile.gameObject, tile.name, PlayfieldEditorSelectionType.Tile);

        }

        private void OnDestroy()
        {
            foreach(var sub in subs)
            {
                sub.Dispose();
            }
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

            if (IsMainInputModifierDown())
            {
                action = "Removing";
            }

            if(IsExtraInputModifierDown())
            {
                action = "Writing";
            }

            if(uiIsForceSecondary.isOn)
            {
                action = "Removing (BY FORCE)";
            }

            if (uiIsForceAlternative.isOn)
            {
                action = "Writing (BY FORCE)";
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
            if(UICore.IsMouseOverUIElement())
            {
                Debug.Log("Skipping primary action to avoid duped UI interaction");
                return;
            }

            if(IsMainInputModifierDown() || uiIsForceSecondary.isOn)
            {
                ProcessSecondaryAction(position);
                return;
            }

            if(IsExtraInputModifierDown() || uiIsForceAlternative.isOn)
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
                if (workingPlayfield.TryGetOriginAt(position, out PlayfieldOrigin _))
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
                if(workingPlayfield.TryGetPortalAt(position, out PlayfieldPortal _))
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

            visuals.DisplayAll(workingPlayfield);
        }

        private void ProcessSecondaryAction(Vector2Int position)
        {
            if (UICore.IsMouseOverUIElement())
            {
                Debug.Log("Skipping secondary action to avoid duped UI interaction");
                return;
            }

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

        private void SizeChange(float e)
        {
            int newWidth = Mathf.RoundToInt(uiWidthSlider.value);
            int newHeight = Mathf.RoundToInt(uiHeightSlider.value);

            workingPlayfield = Utils.CreatePlayfield(newWidth, newHeight, workingPlayfield);
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

        private void DescriptionChanged(string value)
        {
            workingPlayfield.description = value;
            uiDescriptionValue.text = workingPlayfield.description;
        }

        private void TagLabelChange(string value)
        {
            workingPlayfield.tagLabel = value;
            uiTagLabelValue.text = workingPlayfield.tagLabel;
        }

        private void SaveCreatedPlayfield()
        {
            FileBrowser.ShowSaveDialog((paths) =>
            {
                string path = paths[0];
                JsonSerializer serializer = new JsonSerializer();
                using (StreamWriter writer = new StreamWriter(path))
                {
                    string toPrint = JsonConvert.SerializeObject(workingPlayfield, Formatting.Indented);
                    writer.Write(toPrint);
                }
            }, () => { }, FileBrowser.PickMode.Files, allowMultiSelection: false, Application.dataPath, $"level.{LEVEL_FILE_EXTENSION}", "Save Level JSON");
        }

        private void LoadCreatedPlayfield()
        {
            FileBrowser.ShowLoadDialog((paths) =>
            {
                string path = paths[0];
                JsonSerializer serializer = new JsonSerializer();
                using (StreamReader reader = new StreamReader(path))
                {
                    string all = reader.ReadToEnd();
                    Playfield field = JsonConvert.DeserializeObject<Playfield>(all);

                    AssignPlayfield(field);
                }
            }, () => { }, FileBrowser.PickMode.Files, allowMultiSelection: false, Application.dataPath);
        }

        /// <summary>
        /// Assign and redraw the content of the provided playfield. Useful for loading, etc.
        /// </summary>
        /// <param name="field"></param>
        private void AssignPlayfield(Playfield field)
        {
            workingPlayfield = field;
            if (!workingPlayfield.Validate())
            {
                Debug.LogError("Mis-configured playfield");
            }

            uiTagBestowedValue.text = field.GetBestowedList();
            uiTagBestowedEntryField.text = uiTagBestowedValue.text;
            uiTagLabelValue.text = field.tagLabel;
            uiTagLabelEntryField.text = uiTagLabelValue.text;
            uiDescriptionValue.text = field.description;
            uiDescriptionField.text = uiDescriptionValue.text;

            uiWidthSlider.SetValueWithoutNotify(workingPlayfield.world.GetWidth());
            uiWidthEntryField.SetTextWithoutNotify(workingPlayfield.world.GetWidth().ToString());
            uiHeightSlider.SetValueWithoutNotify(workingPlayfield.world.GetHeight());
            uiHeightEntryField.SetTextWithoutNotify(workingPlayfield.world.GetHeight().ToString());

            visuals.DisplayAll(workingPlayfield);
        }
    }
}
