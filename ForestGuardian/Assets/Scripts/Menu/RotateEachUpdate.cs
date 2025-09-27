using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class RotateEachUpdate : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationSpeed;

        // Update is called once per frame
        void Update()
        {
            const float singleRotation = 360f;

            this.transform.Rotate(rotationSpeed * Time.deltaTime);
            Vector3 wrapped = this.transform.rotation.eulerAngles;

            bool didWrap = false;
            if (wrapped.x > singleRotation) { wrapped.x -= singleRotation; didWrap = true; }
            else if (wrapped.x < -singleRotation) { wrapped.x += singleRotation; didWrap = true; }

            if (wrapped.y > singleRotation) { wrapped.y -= singleRotation; didWrap = true; }
            else if (wrapped.y < -singleRotation) { wrapped.y += singleRotation; didWrap = true; }

            if (wrapped.z > singleRotation) { wrapped.z -= singleRotation; didWrap = true; }
            else if (wrapped.z < -singleRotation) { wrapped.z += singleRotation; didWrap = true; }

            if (didWrap)
            {
                this.transform.rotation = Quaternion.Euler(wrapped);
            }
        }
    }
}