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
    friendlyName: "Collectible Acquired",
    description: "Indicates that a Collectible was interacted with",
    isVisible: true)]
    public class MsgCollectibleGrabbed : Loam.Message
    {
        public Item collectible;
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

    [MessageMetadata(
    friendlyName: "Roster Unit Clicked",
    description: "The user is indicating they wish to select this unit for the specified spot on the playfield",
    isVisible: true)]
    public class MsgRosterUnitIndicated : Loam.Message
    {
        public int rosterIndex;
    }

    [MessageMetadata(
    friendlyName: "Player Unit Moved",
    description: "When a player indicates a unit has moved",
    isVisible: true)]
    public class MsgUnitMoved : Loam.Message
    {
        public Unit unit;
    }

    [MessageMetadata(
    friendlyName: "Player Unit Attacking",
    description: "When a player has a unit attack or use a move",
    isVisible: true)]
    public class MsgUnitAttack : Loam.Message
    {
        public Unit unit;
    }


    [MessageMetadata(
    friendlyName: "Start Floor Requested",
    description: "A player is entering the main loop of the playfield via an explicit request (button, etc)",
    isVisible: true)]
    public class MsgFloorStarted : Loam.Message
    {
    }

    [MessageMetadata(
    friendlyName: "Do No Action",
    description: "A 'no action' has been selected during combat",
    isVisible: true)]
    public class MsgNoAction_UI : Loam.Message
    {
    }

    [MessageMetadata(
    friendlyName: "Move Selected",
    description: "The specified following move was selected from the specified unit, clone of the data",
    isVisible: true)]
    public class MsgMoveSelected_UI : Loam.Message
    {
        public MoveData moveSelected;
    }

    [MessageMetadata(
    friendlyName: "Movement button Selected",
    description: "Swap to try and movements and continue moving",
    isVisible: true)]
    public class MsgMove_UI : Loam.Message
    {
    }

    [MessageMetadata(
    friendlyName: "Show Level Info",
    description: "Request showing the specified level info",
    isVisible: true)]
    public class MsgShowLevelInfo : Loam.Message
    {
        public MapInteractionPoint mapInteractionPoint;
    }

    [MessageMetadata(
    friendlyName: "Hide Level Info",
    description: "Request hiding the specified level",
    isVisible: true)]
    public class MsgHideLevelInfo : Loam.Message
    {
        public MapInteractionPoint mapInteractionPoint;
    }

    [MessageMetadata(
    friendlyName: "Start Convo",
    description: "Attempts to display the specified conversation dialog",
    isVisible: true)]
    public class MsgConvoStart : Loam.Message
    {
        // The name of the text asset
        public string convoName;
    }

    [MessageMetadata(
    friendlyName: "End Convo",
    description: "Attempts to end any active dialogue and hide the conversation dialog",
    isVisible: true)]
    public class MsgConvoEnd : Loam.Message
    {
    }

    [MessageMetadata(
    friendlyName: "Convo Message",
    description: "An interaction in a dialog sending a message",
    isVisible: true)]
    public class MsgConvoMessage : Loam.Message
    {
        public string message;
    }

    [MessageMetadata(
    friendlyName: "Add unlock tag",
    description: "Add a tag for unlocking",
    isVisible: true)]
    public class MsgUnlockLevel : Loam.Message
    {
        public string tagToAdd;
    }

    [MessageMetadata(
    friendlyName: "New level unlocked",
    description: "Adds the tag internally to the list of unlocked levels. Imacts map visibility.",
    isVisible: true)]
    public class MsgLevelUnlockAdded : Loam.Message
    {
        public string newUnlock;
    }

    [MessageMetadata(
    friendlyName: "New level Finished",
    description: "Adds the level tag internally to the list of finished levels. Imacts map highlights.",
    isVisible: true)]
    public class MsgLevelFinishedAdded : Loam.Message
    {
        public string newFinishedLevel;
    }

    [MessageMetadata(
    friendlyName: "Save the game",
    description: "Requests that the game get saved",
    isVisible: true)]
    public class MsgSaveGame : Loam.Message
    {
    }
}