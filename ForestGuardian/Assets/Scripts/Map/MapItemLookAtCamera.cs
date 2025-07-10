using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapItemLookAtCamera : MonoBehaviour
{
    Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (mainCam == null)
        {
            Debug.LogWarning("No main camera found?");
            return;
        }

        this.transform.LookAt(mainCam.transform.position);
        Vector3 looking = this.transform.rotation.eulerAngles;
        this.transform.rotation = Quaternion.Euler(0, looking.y, 0); // This will be away by default
        this.transform.Rotate(0, 180, 0); 
    }
}
