using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static forest.BaseStuff;

namespace forest
{
    public class TestGridItem : MonoBehaviour
    {
        public int cost = 1;
        public SpriteRenderer spriteRenderer;
        [SerializeField] private TMPro.TextMeshPro displayText;
        [SerializeField] private TMPro.TextMeshPro displayTextSmall;
        public bool isTarget = false;
        public bool isStart = false;
        public bool isWall = false;

        public int DEBUG_primaryDisplayNum = 0;

        private void Update()
        {
            displayTextSmall.text = cost.ToString();
            displayText.text = DEBUG_primaryDisplayNum.ToString();
        }

        public void SetColor(Color color)
        {
            float a = spriteRenderer.color.a;
            color.a = a;
            spriteRenderer.color = color;
        }
    }
}
