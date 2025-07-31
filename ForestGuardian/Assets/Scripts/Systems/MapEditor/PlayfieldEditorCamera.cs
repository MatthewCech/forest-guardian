using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class PlayfieldEditorCamera : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 1;

        // Update is called once per frame
        void Update()
        {
            Vector2 moveInput = Vector2.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                moveInput.y -= moveSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                moveInput.x += moveSpeed * Time.deltaTime;
            }

            if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                moveInput.y += moveSpeed * Time.deltaTime;
            }

            if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                moveInput.x -= moveSpeed * Time.deltaTime;
            }

            float z = this.transform.position.z;
            this.transform.position = new Vector3(this.transform.position.x + moveInput.x, this.transform.position.y + moveInput.y, z);
        }
    }
}