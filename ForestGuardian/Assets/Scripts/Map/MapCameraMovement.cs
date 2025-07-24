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

    [SerializeField] private float boundaryRadius = 10;
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

            // Apply XY mouse delta on XZ plane, which is why there's some 
            float newX = startTransform.x + diff.x;
            float newZ = startTransform.z + diff.y;
            Vector2 flat = new Vector2(newX, newZ);
            
            if (flat.magnitude > boundaryRadius)
            {
                flat = flat.normalized * boundaryRadius;
            }

            this.transform.position = new Vector3(flat.x, this.transform.position.y, flat.y);
        }
    }
}
