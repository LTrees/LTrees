/*
 * Copyright (c) 2009 Asger Feldthaus
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Contains vertex and index buffers for drawing a group of leaves. This only works
    /// as a buffer, and does not set any effect parameters.
    /// </summary>
    public class TreeLeafCloud : IDisposable
    {
        private GraphicsDevice device;
        private VertexBuffer vbuffer;
        private IndexBuffer ibuffer;
        private int numleaves;
        private BoundingSphere boundingSphere;

        /// <summary>
        /// A bounding sphere enclosing all the leaves at any camera angle.
        /// </summary>
        public BoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
            set { boundingSphere = value; }
        }

        /// <summary>
        /// Creates a leaf cloud displaying the leaves on the specified tree skeleton.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="skeleton">The tree skeleton whose leaves you want to display.</param>
        /// <remarks>
        /// The leaf cloud does not remember the skeleton that generated it. The skeleton may be changed
        /// without affecting previously generated leaf clouds.
        /// </remarks>
        public TreeLeafCloud(GraphicsDevice device, TreeSkeleton skeleton)
        {
            this.device = device;

            Init(skeleton);
        }

        private void Init(TreeSkeleton skeleton)
        {
            if (skeleton.Leaves.Count == 0)
                return;

            Matrix[] transforms = new Matrix[skeleton.Branches.Count];
            skeleton.CopyAbsoluteBranchTransformsTo(transforms);

            Vector3 center = Vector3.Zero;
            for (int i = 0; i < skeleton.Leaves.Count; i++)
            {
                center += transforms[skeleton.Leaves[i].ParentIndex].Translation;
            }
            center = center / (float)skeleton.Leaves.Count;

            LeafVertex[] vertices = new LeafVertex[skeleton.Leaves.Count * 4];
            short[] indices = new short[skeleton.Leaves.Count * 6];

            int vindex = 0;
            int iindex = 0;

            boundingSphere.Center = center;
            boundingSphere.Radius = 0.0f;

            foreach (TreeLeaf leaf in skeleton.Leaves)
            {
                // Get the position of the leaf
                Vector3 position = transforms[leaf.ParentIndex].Translation + transforms[leaf.ParentIndex].Up * skeleton.Branches[leaf.ParentIndex].Length;
                if (skeleton.LeafAxis != null)
                {
                    position += skeleton.LeafAxis.Value * leaf.AxisOffset;
                }

                // Orientation
                Vector2 right = new Vector2((float)Math.Cos(leaf.Rotation), (float)Math.Sin(leaf.Rotation));
                Vector2 up = new Vector2(-right.Y, right.X);

                // Scale vectors by size
                right = leaf.Size.X * right;
                up = leaf.Size.Y * up;

                // Choose a normal vector for lighting calculations
                float distanceFromCenter = Vector3.Distance(position, center);
                Vector3 normal = (position - center) / distanceFromCenter; // normalize the normal

                //                    0---1
                // Vertex positions:  | \ |
                //                    3---2
                int vidx = vindex;
                vertices[vindex++] = new LeafVertex(position, new Vector2(0, 0), -right + up, leaf.Color, leaf.BoneIndex, normal);
                vertices[vindex++] = new LeafVertex(position, new Vector2(1, 0), right + up, leaf.Color, leaf.BoneIndex, normal);
                vertices[vindex++] = new LeafVertex(position, new Vector2(1, 1), right - up, leaf.Color, leaf.BoneIndex, normal);
                vertices[vindex++] = new LeafVertex(position, new Vector2(0, 1), -right - up, leaf.Color, leaf.BoneIndex, normal);

                // Add indices
                indices[iindex++] = (short)(vidx);
                indices[iindex++] = (short)(vidx + 1);
                indices[iindex++] = (short)(vidx + 2);

                indices[iindex++] = (short)(vidx);
                indices[iindex++] = (short)(vidx + 2);
                indices[iindex++] = (short)(vidx + 3);

                // Update the bounding sphere
                float size = leaf.Size.Length() / 2.0f;
                boundingSphere.Radius = Math.Max(boundingSphere.Radius, distanceFromCenter + size);
            }

            // Create the buffers
            vbuffer = new VertexBuffer(device, LeafVertex.VertexDeclaration, vertices.Length, BufferUsage.None);
            vbuffer.SetData<LeafVertex>(vertices);

            ibuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
            ibuffer.SetData<short>(indices);

            // Remember the number of leaves
            numleaves = skeleton.Leaves.Count;
        }

        /// <summary>
        /// Draws the tree leaf cloud using a given effect.
        /// All parameters, such as World, View, Projection, and Bones should already be set on the effect.
        /// See the <see cref="LeafVertex"/> description for how the effect's vertex program should receive input vertices.
        /// </summary>
        /// <param name="effect">Effect to draw the cloud with.</param>
        public void Draw(Effect effect)
        {
            if (numleaves == 0)
                return;

            device.SetVertexBuffer(vbuffer);
            device.Indices = ibuffer;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
            	pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numleaves * 4, 0, numleaves * 2);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            vbuffer.Dispose();
            ibuffer.Dispose();
        }

        #endregion
    }
}
