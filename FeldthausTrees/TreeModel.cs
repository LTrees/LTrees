/* 
 * Copyright (c) 2007 Asger Feldthaus
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
 * and associated documentation files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:  
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
 * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using Microsoft.Xna.Framework;

namespace Feldthaus.Xna
{
    /// <summary>
    /// Holds the mesh of a tree's trunk, its leaves, and its bounding box.
    /// </summary>
    public class TreeModel
    {
        /// <summary>
        /// Holds the trunk's mesh. You need this to draw the tree's trunk.
        /// </summary>
        public Mesh Trunk;

        /// <summary>
        /// Holds the leaves on the tree, as a particle cloud. You need this to draw the leaves.
        /// </summary>
        public ParticleCloud Leaves;

        /// <summary>
        /// A bounding box containing all the vertices and leaves in the tree.
        /// </summary>
        public BoundingBox BoundingBox;
    }
}
