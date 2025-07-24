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
    friendlyName: "Unit Segment Destroyed",
    description: "A segment of a unit was destroyed",
    isVisible: true)]
        public class MsgUnitSegmentDestroyed : Loam.Message
        {
            public int attackingUnitID = Playfield.NO_ID;
            public int defendingUnitID = Playfield.NO_ID;
            public Vector2Int destroyedPosition;
        }



    // Direct input style interactions with playfield stuff
    // -------------------------------------------------------------------------------------------------------------

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
    friendlyName: "Tile Secondary Action",
    description: "A secondary action, such as a right-click, was performed over a playfield tile",
    isVisible: true)]
    public class MsgTileSecondaryAction : Loam.Message
    {
        public Tile tile;
        public Vector2Int position;
    }

    [MessageMetadata(
    friendlyName: "Unit Primary Action",
    description: "A primary action, such as a left-click, was performed over a playfield unit. This takes precedence over a playfield tile.",
    isVisible: true)]
    public class MsgUnitPrimaryAction : Loam.Message
    {
        public Unit unit;
        public Vector2Int position;
    }

    [MessageMetadata(
    friendlyName: "Unit Secondary Action",
    description: "A secondary action, such as a right-click, was performed over a playfield unit. This takes precedence over a playfield tile.",
    isVisible: true)]
    public class MsgUnitSecondaryAction : Loam.Message
    {
        public Unit unit;
        public Vector2Int position;
    }

    [MessageMetadata(
    friendlyName: "Item Primary Action",
    description: "A primary action, such as a left-click, was performed over a playfield item. This takes precedence over a playfield tile.",
    isVisible: true)]
    public class MsgItemPrimaryAction : Loam.Message
    {
        public Item item;
        public Vector2Int position;
    }

    [MessageMetadata(
    friendlyName: "Item Secondary Action",
    description: "A secondary action, such as a right-click, was performed over a playfield item. This takes precedence over a playfield tile.",
    isVisible: true)]
    public class MsgItemSecondaryAction : Loam.Message
    {
        public Item item;
        public Vector2Int position;
    }

    [MessageMetadata(
    friendlyName: "Portal Primary Action",
    description: "A primary action, such as a left-click, was performed over playfield portal. This takes precedence over a playfield tile.",
    isVisible: true)]
    public class MsgPortalPrimaryAction : Loam.Message
    {
        public Portal item;
        public Vector2Int position;
    }

    [MessageMetadata(
    friendlyName: "Portal Secondary Action",
    description: "A secondary action, such as a right-click, was performed over a playfield portal. This takes precedence over a playfield tile.",
    isVisible: true)]
    public class MsgPortalSecondaryAction : Loam.Message
    {
        public Portal item;
        public Vector2Int position;
    }

    [MessageMetadata(
    friendlyName: "Exit Primary Action",
    description: "A primary action, such as a left-click, was performed over playfield exit. This takes precedence over a playfield tile.",
    isVisible: true)]
    public class MsgExitPrimaryAction : Loam.Message
    {
        public Exit item;
        public Vector2Int position;
    }

    [MessageMetadata(
    friendlyName: "Exit Secondary Action",
    description: "A secondary action, such as a right-click, was performed over a playfield exit. This takes precedence over a playfield tile.",
    isVisible: true)]
    public class MsgExitSecondaryAction : Loam.Message
    {
        public Exit item;
        public Vector2Int position;
    }
}