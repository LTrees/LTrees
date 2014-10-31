using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LTreesLibrary.Trees.Wind
{
    public interface WindSource
    {
        /// <summary>
        /// Returns the direction and strength of the wind, in a given position in the tree.
        /// </summary>
        /// <param name="position">Position local to the tree.</param>
        /// <returns>Wind strength. 1 is a light wind, 10 is medium, 50 is strong, and 100 is hurricane.</returns>
        Vector3 GetWindStrength(Vector3 position);
    }
}
