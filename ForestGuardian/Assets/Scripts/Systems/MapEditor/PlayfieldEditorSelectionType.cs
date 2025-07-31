using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    /// <summary>
    /// Within the playfield editor itself, this represents the 'layer' or 'type' that's being edited.
    /// </summary>
    public enum PlayfieldEditorSelectionType
    {
        NONE = 0,
        Tile,
        Unit,
        Item,
        Portal,
        Exit
    }
}