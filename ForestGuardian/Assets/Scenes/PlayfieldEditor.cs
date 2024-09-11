using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Loam;
using System.IO;
using static UnityEditor.Progress;

namespace forest
{
    public class PlayfieldEditor : MonoBehaviour
    {
        public VisualLookup lookup;
        public UIDocument document;
        public VisualPlayfield visuals;
        public Camera displayingCamera;

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
        }

        /// <summary>
        /// Processes an action that 
        /// </summary>
        /// <param name="raw"></param>
        private void TilePrimaryAction(Message raw)
        {
            MsgTilePrimaryAction msg = raw as MsgTilePrimaryAction;
            PlayfieldTile tile = workingPlayfield.world.Get(msg.tilePosition);

            // We're making an assumption that values here are sequential and spaced by 1.
            ++tile.tileType;
            if(tile.tileType >= TileType.COUNT)
            {
                tile.tileType = TileType.DEFAULT + 1;
            }

            workingPlayfield.world.Set(msg.tilePosition, tile);
            visuals.DisplayAll(workingPlayfield);
        }

        private void TileSecondaryAction(Message raw)
        {
            MsgTileSecondaryAction msg = raw as MsgTileSecondaryAction;
            PlayfieldTile tile = workingPlayfield.world.Get(msg.tilePosition);

            --tile.tileType;
            if (tile.tileType <= TileType.DEFAULT)
            {
                tile.tileType = TileType.COUNT - 1;
            }

            workingPlayfield.world.Set(msg.tilePosition, tile);
            visuals.DisplayAll(workingPlayfield);
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
                    PlayfieldTile tile = new PlayfieldTile(TileType.Nothing);
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
                bool inFirst = pos.x < existing.world.GetWidth() || pos.y < existing.world.GetHeight();
                bool inSecond = pos.x < newPlayfield.world.GetWidth() || pos.y < newPlayfield.world.GetHeight();
                return inFirst && inSecond;
            }

            newPlayfield.world.ScrapeDataFrom(existing.world);

            // Back to front move over units that are still in the new bounds of the playfield.
            // The entire location/body of the unit must be in the new resize playfield or it all gets removed.
            for (int i = existing.units.Count - 1; i >= 0; --i)
            {
                PlayfieldUnit unit = existing.units[i];

                bool shouldRemove = false;
                for (int bodyPos = 0; i < unit.locations.Count; ++bodyPos)
                {
                    Vector2Int cur = unit.locations[i];

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

            StreamWriter writer = new StreamWriter(path);

            const char SEPARATOR = ',';

            // Units
            for(int i = 0; i < workingPlayfield.units.Count; ++i)
            {
                PlayfieldUnit unit = workingPlayfield.units[i];

                StringWriter unitSW = new StringWriter();
                unitSW.Write(unit.team);
                unitSW.Write(SEPARATOR);
                unitSW.Write(unit.tag);
                unitSW.Write(SEPARATOR);
                unitSW.Write(unit.locations[0].x);
                unitSW.Write(SEPARATOR);
                unitSW.Write(unit.locations[1].y);

                writer.WriteLine(unitSW.ToString());
            }

            // Items
            for (int i = 0; i < workingPlayfield.items.Count; ++i)
            {
                PlayfieldItem item = workingPlayfield.items[i];

                StringWriter itemSW = new StringWriter();
                itemSW.Write('i');
                itemSW.Write(SEPARATOR);
                itemSW.Write(item.tag);
                itemSW.Write(SEPARATOR);
                itemSW.Write(item.location.x);
                itemSW.Write(SEPARATOR);
                itemSW.Write(item.location.y);

                writer.WriteLine(itemSW.ToString());
            }

            // Map / Tiles
            writer.WriteLine("[map]");

            StringWriter sw = new StringWriter();
            int width = workingPlayfield.world.GetWidth();
            int height = workingPlayfield.world.GetHeight();

            for (int y = 0; y < height; ++y)
            {
                for(int x = 0; x < width; ++x)
                {
                    // Fencepost
                    if(x != 0)
                    {
                        writer.Write(SEPARATOR);
                    }

                    writer.Write((int)workingPlayfield.world.Get(x, y).tileType);
                }

                writer.WriteLine();
            }

            writer.Close();
#endif
        }
    }
}
