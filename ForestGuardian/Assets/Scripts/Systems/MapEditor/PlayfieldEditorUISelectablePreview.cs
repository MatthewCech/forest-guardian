using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlayfieldEditorUITilePreview : MonoBehaviour
{
    [SerializeField] bool executeInEdit = false;

    // Update is called once per frame
    void Update()
    {
        if(!Application.isPlaying && !executeInEdit)
        {
            return;
        }

        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        targetPosition.z = 0;
        this.transform.position = targetPosition;
    }
}
