using System;
using System.Collections.Generic;
using System.Text;

namespace LTreesLibrary.Trees.Instructions
{
    public class Util
    {
        public static float Random(Random rnd, float value, float variation)
        {
            return value + (float)(rnd.NextDouble() * 2.0 - 1.0) * variation;
        }
    }
}
