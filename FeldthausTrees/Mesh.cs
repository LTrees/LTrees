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
using System.Diagnostics;

namespace Feldthaus.Xna
{
    public class Mesh : IDisposable
    {
        #region Fields
        private VertexDeclaration vertexDeclaration;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private Effect effect;
        private EffectParameter worldParam;
        private EffectParameter viewParam;
        private EffectParameter projectionParam;
        private EffectParameter worldViewParam;
        private EffectParameter viewProjectionParam;
        private EffectParameter worldViewProjectionParam;
        private GraphicsDevice device;
        private VertexPositionNormalTexture[] vertices;
        private int[] indices;
        private bool obsoleteVertexBuffer;
        private bool obsoleteIndexBuffer;
        private bool obsoleteProjection;
        private Matrix projection;
        private bool initialized = false;
        private bool projectionSet = false;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the array of vertices. If you change anything, you must call ObsoleteVertexBuffer or the
        /// changes will not take effect.
        /// </summary>
        public VertexPositionNormalTexture[] Vertices
        {
            get { return vertices; }
        }

        /// <summary>
        /// Gets the array of indices. If you change anything, you must call ObsoleteIndexBuffer or the
        /// changes will not take effect.
        /// </summary>
        public int[] Indices
        {
            get { return indices; }
        }

        /// <summary>
        /// Gets or sets the effect used to draw the mesh. The World, View, Projection, WorldView, ViewProjection, and WorldViewProjection parameters
        /// are automatically assigned if their semantics are used in the effect. Any other parameter must be set manually.
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set
            {
                effect = value;
                FetchEffectParameters();
            }
        }

        /// <summary>
        /// Gets or sets the projection matrix used when drawing the mesh. It is important that you set this
        /// correctly before drawing the mesh, or it will be invisible (at best).
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; obsoleteProjection = true; projectionSet = true; }
        }

        /// <summary>
        /// Returns true if the mesh has been initialized.
        /// </summary>
        public bool IsInitialized
        {
            get { return initialized; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an empty uninitialized mesh. Call Initialize() to set the mesh data.
        /// </summary>
        public Mesh()
        {
        }

        /// <summary>
        /// Creates a new mesh and initializes it using the graphics device and the data from the specified arrays.
        /// <remarks>
        /// Please note that the arrays are NOT copied, and are stored as references.
        /// If you plan to change the mesh individually, clone the input them before calling this.
        /// </remarks>
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="vertices">Array of vertices to use in the mesh. The array is not copied, and will be stored as a reference.</param>
        /// <param name="indices">Array of indices to use in the mesh. The array is not copied, and will be stored as a reference.</param>
        public Mesh(GraphicsDevice device, VertexPositionNormalTexture[] vertices, int[] indices)
        {
            Initialize(device, vertices, indices);
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the mesh by creating the vertex and index buffers.
        /// </summary>
        /// Please note that the arrays are NOT copied, and are stored as references.
        /// If you plan to change the mesh individually, clone the input them before calling this.
        /// </remarks>
        /// <param name="device">The graphics device.</param>
        /// <param name="vertices">Array of vertices to use in the mesh. The array is not copied, and will be stored as a reference.</param>
        /// <param name="indices">A triangle index list to use in the mesh. The array is not copied, and will be stored as a reference.</param>
        /// <param name="vertexDeclaration">Optional parameter for supplying the vertex declaration, if you already have it. If you neglect it, a new one will be created for you.</param>
        public void Initialize(GraphicsDevice device, VertexPositionNormalTexture[] vertices, int[] indices)
        {
            Debug.Assert(!initialized, "Mesh is already initialized. It cannot be initialized more than once.");

            this.device = device;
            this.vertices = vertices;
            this.indices = indices;

            // Create the vertex declaration
            vertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);

            // Create the vertex buffer
            vertexBuffer = new VertexBuffer(device, vertices.Length * VertexPositionNormalTexture.SizeInBytes, ResourceUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

            // Create the index buffer
            indexBuffer = new IndexBuffer(device, indices.Length * sizeof(int), ResourceUsage.WriteOnly, IndexElementSize.ThirtyTwoBits);
            indexBuffer.SetData<int>(indices);

            // We are now initialized
            initialized = true;

            obsoleteVertexBuffer = false;
            obsoleteIndexBuffer = false;
        }
        #endregion

        #region Effect Management
        /// <summary>
        /// Iterates all effect parameters, and, according to their semantics, assigns the parameters we recognize.
        /// </summary>
        private void FetchEffectParameters()
        {
            // Null all the parameters
            worldParam = viewParam = projectionParam = worldViewParam = viewProjectionParam = worldViewProjectionParam = null;

            // If we have no effect, just stop here.
            if (effect == null)
                return;

            // Iterate all parameters, and look for something we recognize
            for (int i = 0; i < effect.Parameters.Count; i++)
            {
                EffectParameter param = effect.Parameters[i];
                string semantic = param.Semantic;

                // If we have no semantic, just use the name instead.
                if (semantic == null || semantic == "")
                    semantic = param.Name;

                semantic = semantic.ToLower();

                switch (semantic)
                {
                    case "world":
                        worldParam = param;
                        break;

                    case "view":
                        viewParam = param;
                        break;

                    case "projection":
                        projectionParam = param;
                        break;

                    case "worldview":
                        worldViewParam = param;
                        break;

                    case "viewprojection":
                    case "viewproj":
                        viewProjectionParam = param;
                        break;

                    case "worldviewprojection":
                    case "worldviewproj":
                        worldViewProjectionParam = param;
                        break;
                }
            }
        }
        #endregion

        #region Drawing
        public void Draw(Matrix world, Matrix view)
        {
            // Are we initialized?
            if (!initialized)
                throw new Exception("Mesh has not been initialized. Please call Initialize before drawing it.");

            // Save a few headaches for people who would otherwise get invisible meshes.
            // Forgot to set an effect?
            if (effect == null)
                throw new Exception("Cannot draw a mesh without an effect (effect was null).");

            // Forgot to set a projection matrix?
            if (!projectionSet)
                throw new Exception("Cannot draw mesh, because its projection matrix has not been set. Please set Projection before drawing.");

            // Update vertex and index buffers if necessary
            if (obsoleteVertexBuffer)
            {
                vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);
                obsoleteVertexBuffer = false;
            }

            if (obsoleteIndexBuffer)
            {
                indexBuffer.SetData<int>(indices);
                obsoleteIndexBuffer = true;
            }

            // Assign the proper parameters
            if (worldParam != null)
                worldParam.SetValue(world);

            if (viewParam != null)
                viewParam.SetValue(view);

            // Only assign projection if it has changed (it doesn't change very often)
            if (projectionParam != null && obsoleteProjection)
            {
                projectionParam.SetValue(projection);
                obsoleteProjection = false;
            }
            
            // Cache worldView and viewProj in case we need worldViewProjection also.
            Matrix? worldView = null;
            Matrix? viewProj = null;
            if (worldViewParam != null)
            {
                worldView = world * view;
                worldViewParam.SetValue((Matrix)worldView);
            }
            if (viewProjectionParam != null)
            {
                viewProj = view * projection;
                viewProjectionParam.SetValue((Matrix)viewProj);
            }
            if (worldViewProjectionParam != null)
            {
                Matrix worldViewProj;
                if (worldView != null)
                    worldViewProj = (Matrix)worldView * projection;
                else if (viewProj != null)
                    worldViewProj = world * (Matrix)viewProj;
                else
                    worldViewProj = world * view * projection;
                worldViewProjectionParam.SetValue(worldViewProj);
            }

            // Prepare the graphics device
            device.VertexDeclaration = vertexDeclaration;
            device.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            device.Indices = indexBuffer;

            // Start drawing
            effect.Begin();
            for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
            {
                EffectPass pass = effect.CurrentTechnique.Passes[i];
                pass.Begin();
                device.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    vertices.Length,
                    0,
                    indices.Length / 3);
                pass.End();
            }
            effect.End();
        }
        #endregion

        #region Buffer management
        /// <summary>
        /// Marks the vertex buffer obsolete, so it will be re-uploaded next time the mesh is about to be drawn.
        /// </summary>
        public void ObsoleteVertexBuffer()
        {
            obsoleteVertexBuffer = true;
        }

        /// <summary>
        /// Marks the index buffer obsolete, so it will be re-uploaded next time to the mesh is about to be drawn.
        /// </summary>
        public void ObsoleteIndexBuffer()
        {
            obsoleteIndexBuffer = true;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (initialized)
            {
                vertexBuffer.Dispose();
                indexBuffer.Dispose();
                vertexDeclaration.Dispose();
            }
        }
        #endregion

    }
}
