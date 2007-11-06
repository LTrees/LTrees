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
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Feldthaus.Xna
{
    /// <summary>
    /// The vertex type used by particle clouds.
    /// </summary>
    /// <remarks>
    /// It is a very simple vertex, containing only the index of the its particle, and
    /// its offset from the particle's center. See the ParticleCloud.fx shader source to see how the vertex works.
    /// </remarks>
    public struct ParticleCloudVertex
    {
        public struct IdStruct
        {
            public short id;
            public short _unused; // Not used. Only here because VertexElementFormat.Int does not exist, so we use .Short2 instead.
        }

        /// <summary>
        /// Specifies the position of the vertex relative to the particle's position. This is used to calculate the vertex's
        /// final position as well as its texture coordinates. The value should be either [-1,-1], [-1,1], [1,-1] or [1,1].
        /// </summary>
        public Vector2 Offset;

        /// <summary>
        /// Specifies which particle this vertex belongs to. There are four vertices per particle. This index is used to address
        /// the properties of the particle being rendered.
        /// </summary>
        public IdStruct Id;
        
        public ParticleCloudVertex(Vector2 offset, short id)
        {
            Offset = offset;
            Id.id = id;
            Id._unused = 0;
        }

        public const int SizeInBytes = 12;

        public static readonly VertexElement[] VertexElements = 
            {
                new VertexElement(0, 0, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                new VertexElement(0, 8, VertexElementFormat.Short2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0)
            };
    }
}
