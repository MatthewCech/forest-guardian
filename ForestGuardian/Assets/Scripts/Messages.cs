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
}