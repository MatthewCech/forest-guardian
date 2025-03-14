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

namespace forest
{
    public class PlayfieldEditor : MonoBehaviour
    {
        [Header("Editor Settings")]
        [SerializeField] private KeyCode mainModifier = KeyCode.LeftShift;
        [SerializeField] private KeyCode extraModifier = KeyCode.LeftControl;

        [Header("Required Links")]
        [SerializeField] private VisualLookup lookup;
        [SerializeField] private VisualPlayfield visuals;
        [SerializeField] private Camera displayingCamera;

        [Header("Old UI Style links")]
        [SerializeField] private Button uiSave;
        [SerializeField] private Slider uiWidthSlider;
        [SerializeField] private Slider uiHeightSlider;
        [SerializeField] private TMPro.TMP_InputField uiWidthEntryField;
        [SerializeField] private TMPro.TMP_InputField uiHeightEntryField;
        [SerializeField] private TMPro.TextMeshProUGUI uiStatus;
        [SerializeField] private TMPro.TextMeshProUGUI uiLayer;

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
        private SelectionType previewType;

        public void Start()
        {
            previewObject = null;
            previewTag = "";
            previewType = SelectionType.NONE;
            nothingTileTag = lookup.tileTemplates[0].name; // Set nothing tile tag

            selectableEntryParent = selectableEntryTemplate.transform.parent;
            selectableEntryTemplate.gameObject.SetActive(false);
            selectableEntryLabelTemplate.gameObject.SetActive(false);

            visuals.Initialize(lookup);

            uiSave.onClick.AddListener(SaveCreatedPlayfield);

            uiWidthSlider.onValueChanged.AddListener(SizeChange);
            uiHeightSlider.onValueChanged.AddListener(SizeChange);

            uiWidthEntryField.text = uiWidthSlider.value.ToString();
            uiHeightEntryField.text = uiHeightSlider.value.ToString();

            workingPlayfield = CreatePlayfield();
            visuals.DisplayAll(workingPlayfield);
            Utils.CenterCamera(displayingCamera, visuals);

            Postmaster.Instance.Subscribe<MsgTilePrimaryAction>(TilePrimaryAction);
            Postmaster.Instance.Subscribe<MsgTileSecondaryAction>(TileSecondaryAction);
            Postmaster.Instance.Subscribe<MsgUnitPrimaryAction>(UnitPrimaryAction);
            Postmaster.Instance.Subscribe<MsgUnitSecondaryAction>(UnitSecondaryAction);
            Postmaster.Instance.Subscribe<MsgItemPrimaryAction>(ItemPrimaryAction);
            Postmaster.Instance.Subscribe<MsgItemSecondaryAction>(ItemSecondaryAction);

            CreateSelectableButtons();

            // Basic tile
            Tile tile = lookup.tileTemplates[1];
            SetPreview(tile.gameObject, tile.name, SelectionType.Tile);
        }

        private void TilePrimaryAction(Message raw) { ProcessPrimaryAction((raw as MsgTilePrimaryAction).tilePosition); }
        private void UnitPrimaryAction(Message raw) { ProcessPrimaryAction((raw as MsgUnitPrimaryAction).position); }
        private void ItemPrimaryAction(Message raw) { ProcessPrimaryAction((raw as MsgItemPrimaryAction).position); }
        private void TileSecondaryAction(Message raw) { ProcessSecondaryAction((raw as MsgTileSecondaryAction).position); }
        private void UnitSecondaryAction(Message raw) { ProcessSecondaryAction((raw as MsgUnitSecondaryAction).position); }
        private void ItemSecondaryAction(Message raw) { ProcessSecondaryAction((raw as MsgItemSecondaryAction).position); }

        private void AddSelectableLabel(string label)
        {
            PlayfieldEditorUISelectableLabel labelObj = GameObject.Instantiate(selectableEntryLabelTemplate, selectableEntryParent);
            labelObj.label.text = label;
            labelObj.gameObject.SetActive(true);
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
                tile.SetData(tileToCreate.gameObject, SelectionType.Tile, tileToCreate.name, ProcessedSelectableClick);
                
                tile.gameObject.SetActive(true);
            }

            AddSelectableLabel("Units");
            foreach (Unit unitToCreate in lookup.unitTemplates)
            {
                PlayfieldEditorUISelectable unit = GameObject.Instantiate(selectableEntryTemplate, selectableEntryParent);
                unit.SetData(unitToCreate.gameObject, SelectionType.Unit, unitToCreate.name, ProcessedSelectableClick);

                unit.gameObject.SetActive(true);
            }

            AddSelectableLabel("Items");
            foreach (Item itemToCreate in lookup.itemTemplates)
            {
                PlayfieldEditorUISelectable item = GameObject.Instantiate(selectableEntryTemplate, selectableEntryParent);
                item.SetData(itemToCreate.gameObject, SelectionType.Item, itemToCreate.name, ProcessedSelectableClick);

                item.gameObject.SetActive(true);
            }
        }

        private void ProcessedSelectableClick(PlayfieldEditorUISelectable previewClicked)
        {
            SetPreview(previewClicked.Visual, previewClicked.SelectableTag, previewClicked.SelectableType);
        }

        private void SetPreview(GameObject toPreview, string tag, SelectionType type)
        {
            previewTag = tag;
            previewType = type;

            if (previewObject != null)
            {
                DestroyImmediate(previewObject);
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

            uiStatus.text = $"{action} {previewType.ToString()}";
        }

        private void ProcessPrimaryAction(Vector2Int position)
        {
            if(IsMainInputModifierDown())
            {
                ProcessSecondaryAction(position);
                return;
            }

            if (previewType == SelectionType.Tile)
            {
                PlayfieldTile tile = workingPlayfield.world.Get(position);
                tile.tag = previewTag;
            }
            else if (previewType == SelectionType.Unit)
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

                    workingPlayfield.units.Add(newUnit);
                }
            }
            else if (previewType == SelectionType.Item)
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

            visuals.DisplayAll(workingPlayfield);
        }

        private void ProcessSecondaryAction(Vector2Int position)
        {
            if (previewType == SelectionType.Tile)
            {
                PlayfieldTile tile = workingPlayfield.world.Get(position);
                tile.tag = nothingTileTag;
            }
            else if (previewType == SelectionType.Unit)
            {
                workingPlayfield.RemoveUnitAt(position);
            }
            else if (previewType == SelectionType.Item)
            {
                workingPlayfield.RemoveItemAt(position);
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
        }

        private void SizeChange(float e)
        {
            workingPlayfield = CreatePlayfield(workingPlayfield);
            visuals.DisplayAll(workingPlayfield);
            Utils.CenterCamera(displayingCamera, visuals);

            // Readouts
            uiWidthEntryField.text = uiWidthSlider.value.ToString();
            uiHeightEntryField.text = uiHeightSlider.value.ToString();
        }

        private void SaveCreatedPlayfield()
        {

#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.SaveFilePanel("Select a place to save the level", Application.dataPath, "NewPlayfield", "txt");
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
    }
}
