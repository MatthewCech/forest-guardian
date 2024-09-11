using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Loam;

namespace forest
{
    public class Item : MonoBehaviour
    {
        public void Collect()
        {
            MsgCollectableGrabbed msg = new MsgCollectableGrabbed();
            msg.collectable = this;
            Postmaster.Instance.Send<MsgCollectableGrabbed>(msg);

            // Note: Destroying from within itself is dangerous, and should
            // be handled by the visualization system.
        }
    }
}
