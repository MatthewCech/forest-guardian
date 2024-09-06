using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class Unit : MonoBehaviour
    {
        public int maxSize = 3;
        public int moveSpeed = 2;
        public int attackRange = 2;

        private void OnMouseDown()
        {
            Debug.Log("Ayy");
        }
    }
}