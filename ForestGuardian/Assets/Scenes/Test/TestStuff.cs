using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
 
namespace forest
{
    public class TestStuff : BaseStuff
    {
        protected override IEnumerator DoSearch(Action<TestGridItem> onComplete)
        {
            pending.Clear();
            didCheck.Clear();
            foreach(TestGridItem item in items)
            {
                item.costCur = item.cost;
            }

            TestGridItem start = GetStart();
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

                --item.costCur;
                if (item.costCur > 0)
                {
                    pending.Add(item);
                    UpdateVisuals();
                    yield return new WaitForSeconds(stepTimeMS / 1000.0f);
                    continue;
                }


                didCheck.Add(item);
                if (!items.TryFindLocationOf(item, out Vector2Int pos))
                {
                    continue;
                }

                TryAdd(item, pos, Vector2Int.left);
                TryAdd(item, pos, Vector2Int.right);
                TryAdd(item, pos, Vector2Int.up);
                TryAdd(item, pos, Vector2Int.down);

                UpdateVisuals();
                yield return new WaitForSeconds(stepTimeMS / 1000.0f);
            }

            onComplete?.Invoke(target);
        }

        private void TryAdd(TestGridItem parent, Vector2Int pos, Vector2Int offset)
        {
            Vector2Int target = pos + offset;

            if (items.IsPosInGrid(target))
            {
                TestGridItem toAdd = items.Get(target);
                if (didCheck.Contains(toAdd))
                {
                    return;
                }

                if (toAdd.isWall)
                {
                    return;
                }

                if (!pending.Contains(toAdd))
                {
                    toAdd.parentItem = parent;
                    pending.Add(toAdd);
                }
            }
        }
    }
}
