using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class PlayfieldEditorCamera : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 1;

        private bool isDown;
        private Vector2 startMousePos;
        private Vector2 startDragPos;
        public bool isEnabledWASD;

        // Update is called once per frame
        void Update()
        {
            MoveMiddleMouse();
            MoveWASD();
        }

        private void MoveMiddleMouse()
        {
            // Middle mouse button
            if (Input.GetMouseButtonDown(2))
            {
                startMousePos = Input.mousePosition;
                startDragPos = transform.position;

                isDown = true;
            }

            if (isDown)
            {
                Vector2 currentMousePos = Input.mousePosition;
                Vector2 pos = currentMousePos - startMousePos;

                pos /= Screen.dpi;
                pos *= -1;

                this.transform.position = new Vector3(startDragPos.x + pos.x, startDragPos.y + pos.y, this.transform.position.z);
            }

            if (Input.GetMouseButtonUp(2))
            {
                isDown = false;
            }
        }

        private void MoveWASD()
        {
            if(!isEnabledWASD)
            {
                return;
            }

            Vector2 moveInput = Vector2.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                moveInput.y += moveSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                moveInput.x -= moveSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                moveInput.y -= moveSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                moveInput.x += moveSpeed * Time.deltaTime;
            }

            float z = this.transform.position.z;
            this.transform.position = new Vector3(this.transform.position.x + moveInput.x, this.transform.position.y + moveInput.y, z);
        }
    }
}