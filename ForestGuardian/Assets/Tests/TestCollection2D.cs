using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace forest
{
    public class TestCollection2D
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestCollection2DSimplePasses()
        {
            const int width = 20;
            const int height = 10;
            Collection2D<int> coll = new Collection2D<int>(width, height);

            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestCollection2DWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
