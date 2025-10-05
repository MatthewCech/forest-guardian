using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
 
namespace forest
{
    public class TestStuff : BaseStuff
    {
        [Space]
        public int maxDistance = 12;

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

                --item.curNodeCost;
                item.data.DEBUG_primaryDisplayNum = item.curNodeCost;
                if (item.curNodeCost > 0)
                {
                    pending.Add(item);
                    UpdateVisuals();
                    yield return new WaitForSeconds(stepTimeMS / 1000.0f);
                    continue;
                }


                visited.Add(item);
                if (!items.TryFindLocationOf(item.data, out Vector2Int pos))
                {
                    continue;
                }

                TryAdd(item, pos, Vector2Int.left, maxDistance);
                TryAdd(item, pos, Vector2Int.right, maxDistance);
                TryAdd(item, pos, Vector2Int.up, maxDistance);
                TryAdd(item, pos, Vector2Int.down, maxDistance);

                UpdateVisuals();
                yield return new WaitForSeconds(stepTimeMS / 1000.0f);
            }
        }

        private void TryAdd(SearchNode<TestGridItem> parent, Vector2Int pos, Vector2Int offset, int maxDistance)
        {
            Vector2Int target = pos + offset;

            if (items.IsPosInGrid(target))
            {
                TestGridItem toAdd = items.Get(target);
                if (VisitedContains(toAdd))
                {
                    return;
                }

                if (toAdd.isWall)
                {
                    return;
                }

                if (!PendingContains(toAdd))
                {
                    SearchNode<TestGridItem> node = new SearchNode<TestGridItem>(toAdd, toAdd.cost);
                    node.parent = parent;

                    int dist = GetDistance(node);
                    if(dist > maxDistance)
                    {
                        return;
                    }

                    pending.Add(node);
                }
            }
        }

        private int GetDistance(SearchNode<TestGridItem> item)
        {
            int distance = 0;
            while(item.parent != null)
            {
                distance += item.StartingCost;
                item = item.parent;
            }

            return distance;
        }
    }
}
