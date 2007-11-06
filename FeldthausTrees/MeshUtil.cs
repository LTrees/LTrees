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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Feldthaus.Xna
{
    class MeshUtil
    {
        /// <summary>
        /// Constructs a triangle-based cylinder. It calculates vertex position, normals, and texture coordinates, indices and bounding box.
        /// A transformation matrix is sent along to allow arbitrary placement of the cylinder in the mesh. Before transform, the cylinder
        /// will be placed with its bottom centered at [0,0,0] and its top placed somewhere on positive Y-axis.
        /// Note that the cylinder is not closed, as there are no top and bottom caps.
        /// </summary>
        /// <param name="vertices">The array where the cylinder's vertices will be added. The existing vertices will not be affected.</param>
        /// <param name="indices">The index array where the indices for the cylinder's geometry will be added. The indices will be relative to the vertex array given.</param>
        /// <param name="boundingBox">Reference to a bounding box that will be updated to contain the entire cylinder (after transform).</param>
        /// <param name="height">The height of the cylinder (before transform).</param>
        /// <param name="startRadius">Radius at the base (bottom) of the cylinder (before transform).</param>
        /// <param name="endRadius">Radius at the end (top) of the cylinder (before transform).</param>
        /// <param name="radialSegments">Number of radial segments to add to the cylinder. Low values make the cylinder appear chunky. Higher values make it appear more round, but require more vertices.</param>
        /// <param name="transform">Matrix indicating the position, rotation, and scale of the cylinder. Each vertex will be transformed by this matrix. </param>
        /// <param name="startTCoordY">The texture Y-coordinate to use at the bottom of the cylinder. 0.0f is a typical value.</param>
        /// <param name="endTCoordY">The texture Y-coordinate to use at the top of the cylinder. 1.0f is a typical value.</param>
        public static void BuildCylinder(
            List<VertexPositionNormalTexture> vertices,
            List<int> indices,
            ref BoundingBox boundingBox,
            float height,
            float startRadius,
            float endRadius,
            int radialSegments,
            Matrix transform,
            float startTCoordY,
            float endTCoordY)
        {
            // Remember the first index, so we can index our own vertices correctly.
            int vertexIndex = vertices.Count;

            // There is one additional radial vertex to properly wrap the texture.
            int radialVertices = radialSegments + 1;

            // Place bottom vertices.
            for (int i = 0; i < radialVertices; i++)
            {
                // Get the angle from the cylinder's center.
                double angle = 2.0 * Math.PI * i / (double)(radialSegments);

                // Get a vector pointing in the direction of our point.
                Vector3 dir = Vector3.Zero;
                dir.X = (float)Math.Cos(angle);
                dir.Z = (float)Math.Sin(angle);

                // The normal points away from the cylinder.
                Vector3 normal = dir;

                // Caluclate the position, relative to the cylinder's base.
                Vector3 pos = dir * startRadius;

                // Transform position by the matrix.
                pos = Vector3.Transform(pos, transform);

                // Rotate the normal vector.
                normal = Vector3.TransformNormal(normal, transform);

                // Get the texture coordinates for this vertex.
                Vector2 tcoord;
                tcoord.X = (float)(i) / (float)(radialSegments);
                tcoord.Y = startTCoordY;

                // Finally add the vertex to the vertex list.
                vertices.Add(new VertexPositionNormalTexture(
                    pos,
                    normal,
                    tcoord));

                // Add the vertex position to the bounding box.
                AddPointToBoundingBox(pos, ref boundingBox);
            }
            
            // Place top vertices and indices.
            for (int i = 0; i < radialVertices; i++)
            {
                // Get the angle from the cylinder's center.
                double angle = 2.0 * Math.PI * i / (double)(radialSegments);

                // Get a vector pointing in the direction of our point.
                Vector3 dir = Vector3.Zero;
                dir.X = (float)Math.Cos(angle);
                dir.Z = (float)Math.Sin(angle);

                // The normal points away from the cylinder.
                Vector3 normal = dir;

                // Caluclate the position, relative to the cylinder's base.
                Vector3 pos = dir * endRadius;

                // Move position to the top of the cylinder.
                pos.Y = height;

                // Transform position by the matrix.
                pos = Vector3.Transform(pos, transform);
                
                // Rotate the normal vector.
                normal = Vector3.TransformNormal(normal, transform);
                
                // Get the texture coordinates for this vertex.
                Vector2 tcoord;
                tcoord.X = (float)(i) / (float)(radialSegments);
                tcoord.Y = endTCoordY;
                
                // Finally add the vertex to the vertex list.
                vertices.Add(new VertexPositionNormalTexture(
                    pos,
                    normal,
                    tcoord));

                // Add the vertex position to the bounding box.
                AddPointToBoundingBox(pos, ref boundingBox);

                // Add indices
                if (i != radialVertices - 1)
                {
                    int i2 = (i + 1) % radialVertices;

                    // Place the indices
                    indices.Add(vertexIndex + i2);
                    indices.Add(vertexIndex + radialVertices + i);
                    indices.Add(vertexIndex + i);

                    indices.Add(vertexIndex + i2);
                    indices.Add(vertexIndex + radialVertices + i2);
                    indices.Add(vertexIndex + radialVertices + i);
                }
            }
        }

        /// <summary>
        /// Expands a bounding box to contain a specfied point.
        /// </summary>
        /// <param name="point">Point you want the bounding box to contain.</param>
        /// <param name="box">Reference to the bounding box to expand.</param>
        public static void AddPointToBoundingBox(Vector3 point, ref BoundingBox box)
        {
            if (point.X < box.Min.X)
                box.Min.X = point.X;
            else if (point.X > box.Max.X)
                box.Max.X = point.X;

            if (point.Y < box.Min.Y)
                box.Min.Y = point.Y;
            else if (point.Y > box.Max.Y)
                box.Max.Y = point.Y;

            if (point.Z < box.Min.Z)
                box.Min.Z = point.Z;
            else if (point.Z > box.Max.Z)
                box.Max.Z = point.Z;
        }
    }
}
