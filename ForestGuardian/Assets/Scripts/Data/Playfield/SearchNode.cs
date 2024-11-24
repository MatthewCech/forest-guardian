using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class SearchNode<T> where T : class
    {
        public T data = null;

        public SearchNode<T> parent = null;
        public int heuristic = 0;
        public int curNodeCost = 0;

        public int StartingCost { get; private set; }

        public SearchNode(T data, int baseCost)
        {
            this.data = data;
            this.curNodeCost = baseCost;
            this.StartingCost = baseCost;
        }
    }
}
