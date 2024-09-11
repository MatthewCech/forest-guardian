using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Loam;

namespace forest
{

    [MessageMetadata(
    friendlyName: "Indicator Hit",
    description: "A movement indicator tile was pressed. This message contains the associated indicator.",
    isVisible: true)]
    public class MsgIndicatorClicked : Loam.Message
    {
        public Indicator indicator;
    }

    [MessageMetadata(
    friendlyName: "Collectable Acquired",
    description: "Indicates that a collectable was interacted with",
    isVisible: true)]
    public class MsgCollectableGrabbed : Loam.Message
    {
        public Item collectable;
    }


    [MessageMetadata(
    friendlyName: "Tile Primary Action",
    description: "A secondary action, such as a right-click, was performed over a playfield tile",
    isVisible: true)]
    public class MsgTilePrimaryAction : Loam.Message
    {
        public Tile tileTweaked;
        public Vector2Int tilePosition;
    }

    [MessageMetadata(
    friendlyName: "Tile  Secondary Action",
    description: "A secondary action, such as a right-click, was performed over a playfield tile",
    isVisible: true)]
    public class MsgTileSecondaryAction : Loam.Message
    {
        public Tile tileTweaked;
        public Vector2Int tilePosition;
    }
}