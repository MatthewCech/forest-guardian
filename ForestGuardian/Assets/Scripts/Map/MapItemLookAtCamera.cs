using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapItemLookAtCamera : MonoBehaviour
{
    Camera mainCam;
    [SerializeField] private bool lookOnX = false;
    [SerializeField] private bool lookOnY = true;
    [SerializeField] private bool lookOnZ = false;
    [SerializeField] private bool flipX = false;
    [SerializeField] private bool flipY = true;
    [SerializeField] private bool flipZ = false;

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
        this.transform.rotation = Quaternion.Euler(lookOnX ? looking.x : 0, lookOnY ? looking.y : 0, lookOnZ ? looking.z : 0); // This will be away by default

        if (flipY)
        {
            this.transform.Rotate(flipX ? 180 : 0, flipY ? 180 : 0, flipZ ? 180 : 0);
        }
    }
}
