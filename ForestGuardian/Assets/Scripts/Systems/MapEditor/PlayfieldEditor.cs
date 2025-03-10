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

        [Header("Selectables")]
        [SerializeField] private PlayfieldEditorUISelectable tileEntryTemplate;
        [SerializeField] private PlayfieldEditorUISelectable unitEntryTemplate;
        [SerializeField] private PlayfieldEditorUISelectable itemEntryTemplate;
        [SerializeField] private Transform previewTarget;

        // Internal
        private Playfield workingPlayfield;
        private Transform tileEntryParent;
        private Transform unitEntryParent;
        private Transform itemEntryParent;

        private GameObject previewObject;
        private string previewTag;
        private SelectionType previewType;

        public void Start()
        {
            previewObject = null;
            previewTag = "";
            previewType = SelectionType.NONE;

            tileEntryParent = tileEntryTemplate.transform.parent;
            tileEntryTemplate.gameObject.SetActive(false);

            unitEntryParent = unitEntryTemplate.transform.parent;
            unitEntryTemplate.gameObject.SetActive(false);

            itemEntryParent = itemEntryTemplate.transform.parent;
            itemEntryTemplate.gameObject.SetActive(false);

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

            Tile tile = lookup.tileTemplates[0];
            SetPreview(tile.gameObject, tile.name, SelectionType.Tile);
        }

        private void CreateSelectableButtons()
        {
            foreach(Tile tileToCreate in lookup.tileTemplates)
            {
                PlayfieldEditorUISelectable tile = GameObject.Instantiate(tileEntryTemplate, tileEntryParent);
                tile.SetData(tileToCreate.gameObject, SelectionType.Tile, tileToCreate.name, ProcessTileClicked);
                
                tile.gameObject.SetActive(true);
            }
        }

        private void ProcessTileClicked(PlayfieldEditorUISelectable previewClicked)
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

            previewObject = GameObject.Instantiate(toPreview, previewTarget);
            previewObject.gameObject.SetActive(true);
        }

        private void Update()
        {
            string status = "Placing Tile";
            if (Input.GetKey(mainModifier))
            {
                status = "Placing Unit";
            }
            else if (Input.GetKey(extraModifier))
            {
                status = "Placing Item";
            }

            uiStatus.text = status;
        }

        /// <summary>
        /// Processes an action that 
        /// </summary>
        /// <param name="raw"></param>
        private void TilePrimaryAction(Message raw)
        {
            MsgTilePrimaryAction msg = raw as MsgTilePrimaryAction;

            if (previewType == SelectionType.Tile)
            {
                PlayfieldTile tile = workingPlayfield.world.Get(msg.tilePosition);
                tile.tag = previewTag;
            }

            visuals.DisplayAll(workingPlayfield);
        }
        private void TileSecondaryAction(Message raw)
        {
            MsgTileSecondaryAction msg = raw as MsgTileSecondaryAction;
            PlayfieldTile tile = workingPlayfield.world.Get(msg.position);

            int index = GetTemplateIndex(lookup.tileTemplates, tile.tag);

            index--;
            if (index < 0)
            {
                index = lookup.tileTemplates.Count - 1;
            }

            string newTag = lookup.tileTemplates[index].name;
            tile.tag = newTag;

            visuals.DisplayAll(workingPlayfield);
        }

        private void UnitPrimaryAction(Message raw)
        {
            MsgUnitPrimaryAction msg = raw as MsgUnitPrimaryAction;
            PlayfieldUnit data = msg.unit.associatedData;

            if (IsMainInputModifierDown())
            {
                workingPlayfield.units.Remove(data);
            }
            else
            {
                // display the next unit template, looping back if needed.
                int index = GetTemplateIndex(lookup.unitTemplates, data.tag);
                index++;
                if (index >= lookup.unitTemplates.Count)
                {
                    index = 0;
                }

                string newTag = lookup.unitTemplates[index].name;
                Team newTeam = lookup.unitTemplates[index].defaultTeam;
                data.tag = newTag;
                data.team = newTeam;
            }

            visuals.DisplayAll(workingPlayfield);
        }

        private void UnitSecondaryAction(Message raw)
        {
            MsgUnitSecondaryAction msg = raw as MsgUnitSecondaryAction;
            PlayfieldUnit data = msg.unit.associatedData;

            if (IsMainInputModifierDown())
            {
                workingPlayfield.units.Remove(data);
            }
            else
            {
                // display previous unit template, looping back if needed.
                int index = GetTemplateIndex(lookup.unitTemplates, data.tag);
                index--;
                if (index < 0)
                {
                    index = lookup.unitTemplates.Count - 1;
                }

                string newTag = lookup.unitTemplates[index].name;
                Team newTeam = lookup.unitTemplates[index].defaultTeam;
                data.tag = newTag;
                data.team = newTeam;
            }

            visuals.DisplayAll(workingPlayfield);
        }

        private void ItemPrimaryAction(Message raw)
        {
            MsgItemPrimaryAction msg = raw as MsgItemPrimaryAction;
            PlayfieldItem data = msg.item.associatedData;

            if (IsExtraInputModifierDown())
            {
                workingPlayfield.items.Remove(data);
            }
            else
            {
                // display the next unit template, looping back if needed.
                int index = GetTemplateIndex(lookup.itemTemplates, data.tag);
                index++;
                if (index >= lookup.itemTemplates.Count)
                {
                    index = 0;
                }

                string newTag = lookup.itemTemplates[index].name;
                data.tag = newTag;
            }

            visuals.DisplayAll(workingPlayfield);
        }

        private void ItemSecondaryAction(Message raw)
        {
            MsgItemSecondaryAction msg = raw as MsgItemSecondaryAction;
            PlayfieldItem data = msg.item.associatedData;

            if (IsExtraInputModifierDown())
            {
                workingPlayfield.items.Remove(data);
            }
            else
            {
                // display previous unit template, looping back if needed.
                int index = GetTemplateIndex(lookup.itemTemplates, data.tag);
                index--;
                if (index < 0)
                {
                    index = lookup.itemTemplates.Count - 1;
                }

                string newTag = lookup.itemTemplates[index].name;
                data.tag = newTag;
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
