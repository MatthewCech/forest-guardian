using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace forest
{
    public class BaseStuff : MonoBehaviour
    {
        public TestGridItem itemTemplate;
        public float stepTimeMS = 50;
        public TestTerrain terrain;

        protected Collection2D<TestGridItem> items;
        protected float maxCost = 1;

        // Start is called before the first frame update
        void Start()
        {
            itemTemplate.gameObject.SetActive(false);
            items = new Collection2D<TestGridItem>(20, 10);
            for (int x = 0; x < items.GetWidth(); ++x)
            {
                for (int y = 0; y < items.GetHeight(); ++y)
                {
                    float xPosHalf = items.GetWidth() / 2f;
                    float yPosHalf = items.GetHeight() / 2f;
                    TestGridItem item = GameObject.Instantiate(itemTemplate, this.transform);
                    item.transform.localPosition = new Vector2(x - xPosHalf, y - yPosHalf);
                    item.gameObject.SetActive(true);
                    items.Set(x, y, item);
                }
            }

            foreach (CostItem item in terrain.terrain)
            {
                TestGridItem i = items.Get(item.pos);
                i.cost = item.cost;
                i.isWall = item.isWall;
                i.isStart = item.isStart;
                i.isTarget = item.isTarget;
            }

            foreach (TestGridItem item in items)
            {
                if (item.cost > maxCost)
                {
                    maxCost = item.cost;
                }
            }

            UpdateVisuals();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(DoSearch(OnComplete));
            }
        }

        protected void OnComplete(TestGridItem end)
        {
            StartCoroutine(Completed(end));
        }

        protected IEnumerator Completed(TestGridItem end)
        {
            while (end != null)
            {
                end.SetColor(Color.magenta);
                end = end.parentItem;
                yield return new WaitForSeconds(stepTimeMS / 1000.0f);
            }

            yield return null;
        }

        protected TestGridItem GetStart()
        {
            foreach (TestGridItem item in items)
            {
                if (item.isStart == true)
                {
                    return item;
                }
            }

            throw new System.Exception("No start found");
        }

        protected TestGridItem GetTarget()
        {
            foreach (TestGridItem item in items)
            {
                if (item.isTarget == true)
                {
                    return item;
                }
            }

            throw new System.Exception("No start found");
        }

        protected void UpdateVisuals()
        {
            foreach (TestGridItem item in items)
            {
                if (item.isStart)
                {
                    item.SetColor(Color.cyan);
                }
                else if (item.isTarget)
                {
                    item.SetColor(Color.blue);
                }
                else if (item.cost > 1)
                {
                    Color c = Color.Lerp(Color.white, Color.gray, item.cost / (float)maxCost);
                    item.SetColor(c);
                }
                else if (item.isWall)
                {
                    item.SetColor(Color.black);
                }
                else if (pending.Contains(item))
                {
                    item.SetColor(new Color(1f, .8f, .5f));
                }
                else if (didCheck.Contains(item))
                {
                    item.SetColor(new Color(1f, .2f, .5f));
                }
                else
                {
                    item.SetColor(Color.white);
                }
            }
        }


        protected List<TestGridItem> pending = new List<TestGridItem>();
        protected List<TestGridItem> didCheck = new List<TestGridItem>();

        protected virtual IEnumerator DoSearch(Action<TestGridItem> onComplete) { yield return null; }
    }

}
