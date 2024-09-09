using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    [System.Serializable]
    public class Collection2D<T>
    {
        protected int width;
        protected int height;

        // Internal access only.
        protected T[,] tiles;

        /// <summary>
        /// Hey future self: I really don't recommend using this, but you
        /// may have to for a private constructor.
        /// </summary>
        protected Collection2D() { }

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

            this.width = width;
            this.height = height;

            tiles = new T[width, height];
        }

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

            return tiles[x, y];
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

            tiles[x, y] = newValue;
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }
    }
}
 