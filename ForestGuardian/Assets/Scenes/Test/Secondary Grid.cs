using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace forest
{
    public class SecondaryGrid : BaseStuff
    {
        protected override IEnumerator DoSearch(Action<TestGridItem> onComplete)
        {
            pending.Clear();
            didCheck.Clear();
            foreach (TestGridItem item in items)
            {
                item.costCur = item.cost;
            }

            TestGridItem start = GetStart();
            start.parentItem = null;
            TestGridItem target = GetTarget();
            pending.Add(start);

            while (pending.Count > 0)
            {
                TestGridItem item = pending[0];
                pending.RemoveAt(0);

                if (item == target)
                {
                    break;
                }

                didCheck.Add(item);
                if (!items.TryFindLocationOf(item, out Vector2Int pos))
                {
                    continue;
                }

                AddToList(item, pos, Vector2Int.left);
                AddToList(item, pos, Vector2Int.right);
                AddToList(item, pos, Vector2Int.up);
                AddToList(item, pos, Vector2Int.down);

                pending.Sort((a, b) => a.costCur - b.costCur);

                UpdateVisuals();
                yield return new WaitForSeconds(stepTimeMS / 1000.0f);
            }

            onComplete?.Invoke(target);
        }

        private void AddToList(TestGridItem parent, Vector2Int pos, Vector2Int offset)
        {
            Vector2Int curPos = pos + offset;
            if (items.IsPosInGrid(curPos))
            {
                TestGridItem newItem = items.Get(curPos);
                if (didCheck.Contains(newItem))
                {
                    return;
                }

                if(newItem.isWall)
                {
                    return;
                }

                newItem.costCur = int.MaxValue;

                TestGridItem goal = GetTarget();
                TestGridItem start = GetStart();
                items.TryFindLocationOf(goal, out Vector2Int goalPos);
                items.TryFindLocationOf(goal, out Vector2Int startPos);

                int xDif = Mathf.Abs(goalPos.x - curPos.x);
                int yDif = Mathf.Abs(goalPos.y - curPos.y);
                int distTar = xDif + yDif + Mathf.Abs(xDif - yDif);

                newItem.costCur = newItem.cost + distTar;

                newItem.parentItem = parent;
                pending.Add(newItem);
            }
        }
    }
}