using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace forest
{
    public class SecondaryGrid : BaseStuff
    {
        protected override IEnumerator DoSearch(Action<SearchNode<TestGridItem>> onComplete)
        {
            pending.Clear();
            visited.Clear();

            TestGridItem start = GetStart();
            TestGridItem target = GetTarget();
            pending.Add(new SearchNode<TestGridItem>(start, start.cost));

            while (pending.Count > 0)
            {
                SearchNode<TestGridItem> item = pending[0];
                pending.RemoveAt(0);

                if (item.data == target)
                {
                    onComplete?.Invoke(item);
                    break;
                }

                visited.Add(item);
                if (!items.TryFindLocationOf(item.data, out Vector2Int pos))
                {
                    continue;
                }

                AddToList(item, pos, Vector2Int.left);
                AddToList(item, pos, Vector2Int.right);
                AddToList(item, pos, Vector2Int.up);
                AddToList(item, pos, Vector2Int.down);

                pending.Sort((a, b) => a.heuristic - b.heuristic);

                UpdateVisuals();
                yield return new WaitForSeconds(stepTimeMS / 1000.0f);
            }
        }

        private void AddToList(SearchNode<TestGridItem> parent, Vector2Int pos, Vector2Int offset)
        {
            Vector2Int curPos = pos + offset;
            if (items.IsPosInGrid(curPos))
            {
                TestGridItem newItem = items.Get(curPos);
                if (VisitedContains(newItem))
                {
                    return;
                }

                if(newItem.isWall)
                {
                    return;
                }

                SearchNode<TestGridItem> node = new SearchNode<TestGridItem>(newItem, newItem.cost);
                node.curNodeCost = int.MaxValue;

                TestGridItem goal = GetTarget();
                TestGridItem start = GetStart();
                items.TryFindLocationOf(goal, out Vector2Int goalPos);
                items.TryFindLocationOf(goal, out Vector2Int startPos);

                int xDif = Mathf.Abs(goalPos.x - curPos.x);
                int yDif = Mathf.Abs(goalPos.y - curPos.y);
                int distTar = xDif + yDif + Mathf.Abs(xDif - yDif);

                node.heuristic = newItem.cost + distTar;
                node.parent = parent;

                pending.Add(node);
            }
        }
    }
}