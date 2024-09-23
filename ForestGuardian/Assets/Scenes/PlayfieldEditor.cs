using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Loam;
using System.IO;
using Newtonsoft.Json;

namespace forest
{
    public class PlayfieldEditor : MonoBehaviour
    {
        [Header("Editor Settings")]
        [SerializeField] private KeyCode mainModifier = KeyCode.LeftShift;
        [SerializeField] private KeyCode extraModifier = KeyCode.LeftControl;

        [Header("Required Links")]
        [SerializeField] private VisualLookup lookup;
        [SerializeField] private UIDocument document;
        [SerializeField] private VisualPlayfield visuals;
        [SerializeField] private Camera displayingCamera;

        private Playfield workingPlayfield;

        public void Start()
        {
            visuals.Initialize(lookup);
            document.rootVisualElement.Q<Button>("buttonSave").clicked += SaveCreatedPlayfield;
            SliderInt width = document.rootVisualElement.Q<SliderInt>("sliderWidth");
            SliderInt height = document.rootVisualElement.Q<SliderInt>("sliderHeight");

            width.RegisterValueChangedCallback(SizeChange);
            height.RegisterValueChangedCallback(SizeChange);

            workingPlayfield = CreatePlayfield();
            visuals.DisplayAll(workingPlayfield);
            Utils.CenterCamera(displayingCamera, visuals);

            Postmaster.Instance.Subscribe<MsgTilePrimaryAction>(TilePrimaryAction);
            Postmaster.Instance.Subscribe<MsgTileSecondaryAction>(TileSecondaryAction);
            Postmaster.Instance.Subscribe<MsgUnitPrimaryAction>(UnitPrimaryAction);
            Postmaster.Instance.Subscribe<MsgUnitSecondaryAction>(UnitSecondaryAction);
            Postmaster.Instance.Subscribe<MsgItemPrimaryAction>(ItemPrimaryAction);
            Postmaster.Instance.Subscribe<MsgItemSecondaryAction>(ItemSecondaryAction);
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

            document.rootVisualElement.Q<Label>("editorStatus").text = status;
        }

        /// <summary>
        /// Processes an action that 
        /// </summary>
        /// <param name="raw"></param>
        private void TilePrimaryAction(Message raw)
        {
            MsgTilePrimaryAction msg = raw as MsgTilePrimaryAction;

            if (IsMainInputModifierDown())
            {
                PlayfieldUnit unit = new PlayfieldUnit();
                unit.tag = lookup.unitTemplates[0].name;
                unit.team = lookup.unitTemplates[0].defaultTeam;
                unit.id = workingPlayfield.GetNextID();
                unit.locations.Add(msg.tilePosition);

                workingPlayfield.units.Add(unit);
            }
            else if (IsExtraInputModifierDown())
            {
                PlayfieldItem item = new PlayfieldItem();
                item.tag = lookup.itemTemplates[0].name;
                item.id = workingPlayfield.GetNextID();
                item.location = msg.tilePosition;

                workingPlayfield.items.Add(item);
            }
            else
            {
                // No longer using enums, we now follow the pattern of units by cycling through available tiles by tag
                PlayfieldTile tile = workingPlayfield.world.Get(msg.tilePosition);
                int index = GetTemplateIndex(lookup.tileTemplates, tile.tag);

                index++;
                if (index >= lookup.tileTemplates.Count)
                {
                    index = 0;
                }

                string newTag = lookup.tileTemplates[index].name;
                tile.tag = newTag;
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
            SliderInt width = document.rootVisualElement.Q<SliderInt>("sliderWidth");
            SliderInt height = document.rootVisualElement.Q<SliderInt>("sliderHeight");

            int newWidth = width.value;
            int newHeight = height.value;

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

        private void SizeChange(ChangeEvent<int> e)
        {
            workingPlayfield = CreatePlayfield(workingPlayfield);
            visuals.DisplayAll(workingPlayfield);
            Utils.CenterCamera(displayingCamera, visuals);
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
