using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Loam;

public class MapCameraMovement : MonoBehaviour
{
    private bool isDown;
    private Vector2 startMousePos;
    private Vector3 startTransform;

    [SerializeField] private Camera cam;

    Vector2 GetInputPos()
    {
        return Input.mousePosition;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDown = true;
            startMousePos = GetInputPos();
            startTransform = this.transform.position;
        };

        if(Input.GetMouseButtonUp(0))
        {
            isDown = false;
        }

        if(isDown)
        {
            Vector2 currentPos = GetInputPos();
            Vector2 diff = currentPos - startMousePos;

            diff /= Screen.dpi;
            diff *= -1;

            // Apply XY mouse delta on XZ plane
            this.transform.position = new Vector3(startTransform.x + diff.x, this.transform.position.y, startTransform.z + diff.y);
        }
    }
}
