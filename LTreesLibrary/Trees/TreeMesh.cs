/*
 * Copyright (c) 2007-2009 Asger Feldthaus
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
    /// Contains vertex and index buffers for the branches of a tree.
    /// The mesh is composed of uncapped conish cylinders. Each branch is modelled as a single cylinder.
    /// The number of radial segments in a cylinder is proportional to its radius, so thin branches have fewer polygons.
    /// </summary>
    public class TreeMesh : IDisposable
    {
        private GraphicsDevice device;
        private VertexBuffer vbuffer;
        private IndexBuffer ibuffer;
        private VertexDeclaration vdeclaration;
        private int numvertices;
        private int numtriangles;
        private int maxRadialSegments = 8;
        private BoundingSphere boundingSphere;

        /// <summary>
        /// Bounding sphere of the mesh.
        /// </summary>
        /// <remarks>
        /// Not likely to be optimal (it is calculated from the bounding box).
        /// 
        /// Provided for convenience; the mesh itself does not use the bounding sphere for anything.
        /// </remarks>
        public BoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
            set { boundingSphere = value; }
        }

        /// <summary>
        /// Number of triangles in the mesh.
        /// </summary>
        public int NumberOfTriangles
        {
            get { return numtriangles; }
        }

        /// <summary>
        /// Number of vertices in the mesh.
        /// </summary>
        public int NumberOfVertices
        {
            get { return numvertices; }
        }

        /// <summary>
        /// Gets the vertex buffer.
        /// </summary>
        public VertexBuffer VertexBuffer
        {
            get { return vbuffer; }
        }
        /// <summary>
        /// Gets the index buffer.
        /// </summary>
        public IndexBuffer IndexBuffer
        {
            get { return ibuffer; }
        }

        /// <summary>
        /// Creates a mesh of a tree.
        /// </summary>
        /// <param name="device">Graphics device.</param>
        /// <param name="skeleton">The tree to create a mesh for.</param>
        /// <remarks>
        /// The mesh does not remember the skeleton that generated it. The skeleton may be changed
        /// without affecting previously generated meshes.
        /// </remarks>
        public TreeMesh(GraphicsDevice device, TreeSkeleton skeleton)
        {
            this.device = device;
            this.maxRadialSegments = 8;

            Init(skeleton);
        }

        /// <summary>
        /// Creates a mesh of a tree.
        /// </summary>
        /// <param name="device">Graphics device.</param>
        /// <param name="skeleton">The tree to create a mesh for. This reference is not used after creation, and can be used independently afterwards.</param>
        /// <param name="numberOfRadialSegments">Number of radial segments on the trunk. Default value is 8. Higher values gives more polygons. Radial segments will always decrease towards 3 in the smaller branches.</param>
        public TreeMesh(GraphicsDevice device, TreeSkeleton skeleton, int numberOfRadialSegments)
        {
            this.device = device;
            this.maxRadialSegments = numberOfRadialSegments;

            Init(skeleton);
        }

        /// <summary>
        /// Draws the tree mesh using a given effect.
        /// You should set the proper parameters on the effect first, such as World, View, and Projection matrices.
        /// </summary>
        /// <param name="effect">Effect to draw with.</param>
        public void Draw(Effect effect)
        {
            device.VertexDeclaration = vdeclaration;
            device.Vertices[0].SetSource(vbuffer, 0, TreeVertex.SizeInBytes);
            device.Indices = ibuffer;

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numvertices, 0, numtriangles);
                pass.End();
            }
            effect.End();
        }

        #region Mesh Creation
        private int GetRadialSegmentsBottom(int index, TreeSkeleton skeleton)
        {
            float ratio = skeleton.Branches[index].StartRadius / skeleton.Branches[0].StartRadius;
            return 3 + (int)(ratio * (maxRadialSegments - 3) + 0.50f);
        }

        private int GetRadialSegmentsTop(int index, TreeSkeleton skeleton)
        {
            float ratio = skeleton.Branches[index].EndRadius / skeleton.Branches[0].StartRadius;
            return 3 + (int)(ratio * (maxRadialSegments - 3) + 0.50f);
        }

        private void Init(TreeSkeleton skeleton)
        {
            if (skeleton.Branches.Count == 0)
                throw new ArgumentException("Tree skeleton had no branches");

            if (maxRadialSegments < 3)
                throw new ArgumentException("Tree must have at least 3 radial segments");

            // min and max keep track of the mesh's bounding box.
            Vector3 min = new Vector3(10000, 10000, 10000);
            Vector3 max = new Vector3(-10000, -10000, -10000);

            // Create lists for vertices and indices
            List<TreeVertex> vertices = new List<TreeVertex>();
            List<int> indices = new List<int>();

            // Absolute transformation of branches
            Matrix[] transforms = new Matrix[skeleton.Branches.Count];
            skeleton.CopyAbsoluteBranchTransformsTo(transforms);

            // Branch topological distances from root
            float[] distances = new float[skeleton.Branches.Count];
            skeleton.GetLongestBranching(distances);

            //
            //  Create vertices and indices
            //
            for (int i = 0; i < skeleton.Branches.Count; i++)
            {
                int bottomRadials = GetRadialSegmentsBottom(i, skeleton);

                // Add bottom vertices
                int parentIndex = skeleton.Branches[i].ParentIndex;
                int bottomIndex = vertices.Count;
                Matrix bottomTransform = transforms[i];
                if (parentIndex != -1 && skeleton.Branches[i].ParentPosition > 0.99f && Vector3.Dot(transforms[i].Up, transforms[parentIndex].Up) > 0.7f)
                {
                    bottomTransform = transforms[parentIndex];
                    bottomTransform.Translation += bottomTransform.Up * skeleton.Branches[parentIndex].Length;

                    // Rotate bottomTransform to the best fit to avoid twisting
                    Vector3 childDir = Vector3.Transform(Vector3.Right, skeleton.Branches[i].Rotation);

                    float maxdot = -2.0f;
                    double bestangle = 0.0;
                    for (int j = 0; j < bottomRadials; j++)
                    {
                        double angle = j / (double)bottomRadials * Math.PI * 2.0;
                        Vector3 vec = new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));

                        float dot = Vector3.Dot(childDir, vec);
                        if (dot > maxdot)
                        {
                            maxdot = dot;
                            bestangle = angle;
                        }
                    }

                    float cos = (float)Math.Cos(bestangle);
                    float sin = (float)Math.Sin(bestangle);
                    Vector3 right = bottomTransform.Right * cos + bottomTransform.Backward * sin;
                    Vector3 back = -bottomTransform.Right * sin + bottomTransform.Backward * cos;

                    bottomTransform.Right = right;
                    bottomTransform.Backward = back;
                }

                // Texture coordinates
                float ty = (distances[i] - skeleton.Branches[i].Length) / skeleton.TextureHeight;
                float txspan = 0.25f + 0.75f * skeleton.Branches[i].StartRadius / skeleton.TrunkRadius;

                // Bones
                int parentBoneIndex = (parentIndex == -1? skeleton.Branches[i].BoneIndex : skeleton.Branches[parentIndex].BoneIndex);
                int branchBoneIndex = skeleton.Branches[i].BoneIndex;

                AddCircleVertices(ref bottomTransform, skeleton.Branches[i].StartRadius, bottomRadials, ty, 0.0f, txspan, vertices, parentBoneIndex, parentBoneIndex);
                
                // Add top vertices
                int topRadials = GetRadialSegmentsTop(i, skeleton);
                int topIndex = vertices.Count;
                Matrix topTransform = transforms[i];
                topTransform.Translation += topTransform.Up * skeleton.Branches[i].Length;

                ty = ty + skeleton.Branches[i].Length / skeleton.TextureHeight;
                txspan = 0.25f + 0.75f * skeleton.Branches[i].EndRadius / skeleton.TrunkRadius;

                AddCircleVertices(ref topTransform, skeleton.Branches[i].EndRadius, topRadials, ty, 0.0f, txspan, vertices, branchBoneIndex, branchBoneIndex);

                // Add indices
                AddCylinderIndices(bottomIndex, bottomRadials, topIndex, topRadials, indices);

                // Updates bounds
                SetMin(ref min, bottomTransform.Translation);
                SetMin(ref min, topTransform.Translation);
                SetMax(ref max, bottomTransform.Translation);
                SetMax(ref max, topTransform.Translation);
            }
            
            numvertices = vertices.Count;
            numtriangles = indices.Count / 3;

            // Create the buffers
            vbuffer = new VertexBuffer(device, vertices.Count * TreeVertex.SizeInBytes, BufferUsage.None);
            vbuffer.SetData<TreeVertex>(vertices.ToArray());

            if (vertices.Count > 0xFFFF)
            {
                ibuffer = new IndexBuffer(device, indices.Count * sizeof(int), BufferUsage.None, IndexElementSize.ThirtyTwoBits);
                ibuffer.SetData<int>(indices.ToArray());
            }
            else
            {
                ibuffer = new IndexBuffer(device, indices.Count * sizeof(short), BufferUsage.None, IndexElementSize.SixteenBits);
                ibuffer.SetData<short>(Create16BitArray(indices));
            }
            
            // Create the vertex declaration
            vdeclaration = new VertexDeclaration(device, TreeVertex.VertexElements);

            // Set the bounding sphere
            boundingSphere.Center = (min + max) / 2.0f;
            boundingSphere.Radius = (max - min).Length() / 2.0f;
        }

        private short[] Create16BitArray(List<int> list)
        {
            short[] array = new short[list.Count];
            for (int i = 0; i < list.Count; i++)
                array[i] = (short) list[i];
            return array;
        }

        private static void SetMin(ref Vector3 min, Vector3 point)
        {
            min.X = Math.Min(min.X, point.X);
            min.Y = Math.Min(min.Y, point.Y);
            min.Z = Math.Min(min.Z, point.Z);
        }
        private static void SetMax(ref Vector3 max, Vector3 point)
        {
            max.X = Math.Max(max.X, point.X);
            max.Y = Math.Max(max.Y, point.Y);
            max.Z = Math.Max(max.Z, point.Z);
        }

        private static void AddCircleVertices(ref Matrix transform,
            float radius,
            int segments,
            float textureY,
            float textureStartX,
            float textureSpanX,
            List<TreeVertex> vertices,
            int bone1,
            int bone2)
        {
            for (int i = 0; i < segments + 1; i++)
            {
                double angle = i / (double)(segments) * Math.PI * 2.0;
                Vector3 dir = new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));

                Vector3.TransformNormal(ref dir, ref transform, out dir);

                float tx = textureStartX + (i / (float)(segments)) * textureSpanX;

                vertices.Add(new TreeVertex(transform.Translation + dir * radius, dir, new Vector2(tx, textureY), bone1, bone2));
            }
        }

        private static void AddCylinderIndices(int bottomIndex,
            int numBottomVertices,
            int topIndex,
            int numTopVertices,
            List<int> indices)
        {
            int bi = 0; // Bottom index
            int ti = 0; // Top index
            while (bi < numBottomVertices || ti < numTopVertices)
            {
                if (bi * numTopVertices < ti * numBottomVertices)
                {
                    // Move bottom index forward
                    indices.Add(bottomIndex + bi + 1);
                    indices.Add(topIndex + ti);
                    indices.Add(bottomIndex + bi);

                    bi++;
                }
                else
                {
                    // Move top index forward
                    indices.Add(bottomIndex + bi);
                    indices.Add(topIndex + ti + 1);
                    indices.Add(topIndex + ti);

                    ti++;
                }
            }
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            vbuffer.Dispose();
            ibuffer.Dispose();
            vdeclaration.Dispose();
        }

        #endregion
    }
}
