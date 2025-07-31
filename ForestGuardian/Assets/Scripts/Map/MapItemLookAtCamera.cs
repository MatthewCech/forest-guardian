using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapItemLookAtCamera : MonoBehaviour
{
    Camera mainCam;
    [SerializeField] private bool lookOnX = false;
    [SerializeField] private bool lookOnY = true;
    [SerializeField] private bool lookOnZ = false;
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
        this.transform.Rotate(0, 180, 0); 
    }
}
