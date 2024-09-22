using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace forest
{
    [System.Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Collection2D<T> : IEnumerable<T>
    {
        // Internal access only.
        [JsonProperty] protected T[,] data;

        public Collection2D(){ }

        /// <summary>
        /// 2D Collection with a few guards in place for easier looping access or similar.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public Collection2D(int width, int height)
        {
            if(width <=0 || height <= 0)
            {
                throw new System.ArgumentOutOfRangeException($"You cannot use a negative width or negative height in {this.GetType().Name}!");
            }

            data = new T[width, height];
        }

        private int width => data.GetLength(0);
        private int height => data.GetLength(1);

        /// <summary>
        /// Retrieve data at a given position from within the structure.
        /// </summary>
        /// <param name="x">The horizontal position within the 2D Collection</param>
        /// <param name="y">The vertical position within the 2D Colleciton</param>
        /// <returns>A valid X,Y position. Out of bounds requests are looped back onto the 2D structure.</returns>
        public T Get(int x, int y)
        {
            x %= width;
            y %= height;

            return data[x, y];
        }
        public T Get(Vector2Int pos)
        {
            return Get(pos.x, pos.y);
        }

        /// <summary>
        /// Configures data at a given X,Y position in the structure.
        /// </summary>
        /// <param name="x">The horizontal position within the 2D Collection</param>
        /// <param name="y">The vertical position within the 2D Colleciton</param>
        /// <param name="newValue">The new value that will be directly assigned to the contents at the specified X,Y position</param>
        public void Set(int x, int y, T newValue)
        {
            x %= width;
            y %= height;

            data[x, y] = newValue;
        }
        public void Set(Vector2Int pos, T newValue)
        {
            Set(pos.x, pos.y, newValue);
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        /// <summary>
        /// Superimpose the data from target onto ourselves, with no regard for size compatibility.
        /// The collection with the smaller dimension provides the dimension bound.
        /// This is done WITHOUT regard for shallow vs deep copying - direct assignment is used.
        /// </summary>
        /// <param name="target">The other collection to scrape data from</param>
        public void ScrapeDataFrom(Collection2D<T> target)
        {
            for(int x = 0; x < target.width && x < width; ++x)
            {
                for(int y = 0; y < target.height && y < height; ++y)
                {
                    data[x, y] = target.data[x, y];
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int x = 0; x < width && x < width; ++x)
            {
                for (int y = 0; y < height && y < height; ++y)
                {
                    yield return data[x, y];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
 