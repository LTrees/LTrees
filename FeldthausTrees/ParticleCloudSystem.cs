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
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

namespace Feldthaus.Xna
{
    /// <summary>
    /// Responsible for rendering all particle clouds. Particle clouds are groups of billboards with a texture, that may be axis-aligned
    /// or free.
    /// </summary>
    /// <remarks>
    /// To render particle clouds you must have the ParticleCloud.fx shader in your content pipeline, and supply its asset name (with path)
    /// to its constructor.
    /// You should only have one instance of the system; and a good place to create it is the LoadContent method. You must call
    /// Initialize(), and set the ProjectionMatrix property, before it can be used to render anything.
    /// Once the system is properly initialized, you can create particle clouds using the ParticleCloud class.
    /// </remarks>
    public sealed class ParticleCloudSystem
    {
        #region Fields
        private Effect particleEffect;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private VertexDeclaration vertexDeclaration;
        private GraphicsDevice device;
        private ContentManager content;
        private string particleEffectPath;
        private bool isInitialized = false;
        private bool isProjectionMatrixSet = false;
        private Matrix projection;
        private EffectParameter positionsParam;
        private EffectParameter colorsParam;
        private EffectParameter orientationsParam;
        private EffectParameter worldViewParam;
        private EffectParameter worldParam;
        private EffectParameter projectionParam;
        private EffectParameter billboardRightParam;
        private EffectParameter billboardUpParam;
        private EffectParameter textureParam;
        private EffectTechnique sortedTechnique;
        private EffectTechnique unsortedTechnique;
        private bool obsoleteProjection;
        
        private Vector3[] positionBuffer = new Vector3[MaxParticlesPerRender];
        private Vector4[] colorBuffer = new Vector4[MaxParticlesPerRender];
        private Vector4[] orientationBuffer = new Vector4[MaxParticlesPerRender];
        #endregion

        /// <summary>
        /// Number of cloud particles we can render in one render call. If a particle cloud has more than this number,
        /// it will simply be split into several render calls by the particle cloud system.
        /// This should match the value of MAX_PARTICLES defined in ParticleCloud.fx.
        /// </summary>
        public const int MaxParticlesPerRender = 80;

        #region Properties
        /// <summary>
        /// Gets the effect used to render particles.
        /// </summary>
        public Effect ParticleEffect
        {
            get { return particleEffect; }
        }

        /// <summary>
        /// True if Initialize() has been called.
        /// </summary>
        public bool IsInitialized
        {
            get { return isInitialized; }
        }

        /// <summary>
        /// Gets or sets the projection matrix used when rendering particle clouds.
        /// You should assign this to your projection matrix before rendering any particle clouds.
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; isProjectionMatrixSet = true; obsoleteProjection = true; }
        }
        #endregion

        /// <summary>
        /// Creates the particle cloud system.
        /// Before you can draw anything, you must:
        ///     a) Call Initialize().
        ///     b) Set the ProjectionMatrix property.
        /// Failure to do this will raise an exception kindly reminding you to do so.
        /// </summary>
        /// <param name="device">Graphics device; required to create the vertex and index buffers.</param>
        /// <param name="content">Content manager; required to load the particle cloud effect.</param>
        /// <param name="particleEffectPath">Path to the ParticleCloud.fx file (without the .fx extension). For example: "Content/Shaders/ParticleCloud".</param>
        public ParticleCloudSystem(GraphicsDevice device, ContentManager content, string particleEffectPath)
        {
            Debug.Assert(device != null, "ParticleCloudSystem was given a null graphics device. Did you create this in the game's constructor? You should do it in LoadContent instead.");
            Debug.Assert(content != null, "ParticleCloudSystem was given a null content manager.");
            this.device = device;
            this.content = content;
            this.particleEffectPath = particleEffectPath;
        }

        /// <summary>
        /// Loads the particle effect from the .fx file, and initializes the vertex and index buffer used for rendering particle clouds.
        /// This must be called before any particle clouds can be drawn.
        /// An exception will be thrown if the shader does not conform to the requirements of the particle cloud system
        /// (ie. if it was missing a parameter).
        /// </summary>
        public void Initialize()
        {
            // Verify that we have not initialized already.
            Debug.Assert(!isInitialized, "ParticleCloudSystem is already initialized. Initialize() cannot be called more than once.");

            // Load the particle cloud effect.
            particleEffect = content.Load<Effect>(particleEffectPath);

            // Get all the parameter handles.
            positionsParam = particleEffect.Parameters["Positions"];
            colorsParam = particleEffect.Parameters["Colors"];
            orientationsParam = particleEffect.Parameters["Orientations"];
            worldParam = particleEffect.Parameters["World"];
            projectionParam = particleEffect.Parameters["Projection"];
            worldViewParam = particleEffect.Parameters["WorldView"];
            billboardRightParam = particleEffect.Parameters["BillboardRight"];
            billboardUpParam = particleEffect.Parameters["BillboardUp"];
            textureParam = particleEffect.Parameters["Texture"];

            // Verify that the parameters we need are indeed there.
            if (positionsParam == null)
                throw new Exception(particleEffectPath + " is not a valid shader for particle clouds. float3 Positions[] array is undefined");
            if (colorsParam == null)
                throw new Exception(particleEffectPath + " is not a valid shader for particle clouds. float4 Colors[] array is undefined");
            if (orientationsParam == null)
                throw new Exception(particleEffectPath + " is not a valid shader for particle clouds. float4 Orientations[] array is undefined");
            if (worldParam == null)
                throw new Exception(particleEffectPath + " is not a valid shader for particle clouds. float4x4 World : WORLD, is undefined");
            if (projectionParam == null)
                throw new Exception(particleEffectPath + " is not a valid shader for particle clouds. float4x4 Projection : PROJECTION, is undefined");
            if (worldViewParam == null)
                throw new Exception(particleEffectPath + " is not a valid shader for particle clouds. float4x4 WorldView : WORLDVIEW, is undefined");
            if (billboardRightParam == null)
                throw new Exception(particleEffectPath + " is not a valid shader for particle clouds. float3 BillboardRight, is undefined");
            if (billboardUpParam == null)
                throw new Exception(particleEffectPath + " is not a valid shader for particle clouds. float3 BillboardUp, is undefined");
            if (textureParam == null)
                throw new Exception(particleEffectPath + " is not a valid shader for particle clouds. texture Texture, is undefined");

            // Get the technique handles
            sortedTechnique = particleEffect.Techniques["SortedParticles"];
            unsortedTechnique = particleEffect.Techniques["UnsortedParticles"];

            // Verify that the required techniques are indeed there.
            if (sortedTechnique == null)
                throw new Exception(particleEffectPath + " is not a valid shader for particle clouds. SortedParticles technique is not defined.");
            if (unsortedTechnique == null)
                throw new Exception(particleEffectPath + " is not a valid shader for particle clouds. UnsortedParticles technique is not defined.");
            
            // Initialize vertex buffer.
            ParticleCloudVertex[] vertices = new ParticleCloudVertex[MaxParticlesPerRender * 4];
            for (short i = 0; i < MaxParticlesPerRender; i++)
            {
                vertices[4 * i    ] = new ParticleCloudVertex(new Vector2(-1,  1), i);
                vertices[4 * i + 1] = new ParticleCloudVertex(new Vector2( 1,  1), i);
                vertices[4 * i + 2] = new ParticleCloudVertex(new Vector2( 1, -1), i);
                vertices[4 * i + 3] = new ParticleCloudVertex(new Vector2(-1, -1), i);
            }
            vertexBuffer = new VertexBuffer(device, 4 * MaxParticlesPerRender * ParticleCloudVertex.SizeInBytes, ResourceUsage.WriteOnly);
            vertexBuffer.SetData<ParticleCloudVertex>(vertices);

            // Initialize index buffer
            int[] indices = new int[MaxParticlesPerRender * 6];
            int idx = 0;
            for (int i = 0; i < MaxParticlesPerRender; i++)
            {
                indices[idx++] = 4 * i;
                indices[idx++] = 4 * i + 1;
                indices[idx++] = 4 * i + 2;

                indices[idx++] = 4 * i + 2;
                indices[idx++] = 4 * i + 3;
                indices[idx++] = 4 * i;
            }
            indexBuffer = new IndexBuffer(device, 6 * MaxParticlesPerRender * sizeof(int), ResourceUsage.WriteOnly, IndexElementSize.ThirtyTwoBits);
            indexBuffer.SetData<int>(indices);

            // Create the vertex declaration
            vertexDeclaration = new VertexDeclaration(device, ParticleCloudVertex.VertexElements);

            // Initialization successful
            isInitialized = true;
        }

        /// <summary>
        /// This method is here because we can only send a limited number of particles at a time, and the current API
        /// insists that the source of the variables are at the beginning of an array. We must therefore copy a slice
        /// of the array into a buffer and use the buffer instead.
        /// The EffectParameter.SetArrayRange sets array slice written to, but there seems to be no such function for
        /// setting the array slice read from.
        /// </summary>
        /// <param name="cloud">The particle cloud to get particle data from.</param>
        /// <param name="renderN">The current render pass.</param>
        private void PrepareParticleBuffers(ParticleCloud cloud, int renderN)
        {
            int numParticlesToRender = cloud.NumberOfParticles - MaxParticlesPerRender * renderN;

            if (numParticlesToRender > MaxParticlesPerRender)
                numParticlesToRender = MaxParticlesPerRender;

            for (int i = 0; i < numParticlesToRender; i++)
			{
                int cloudIndex = renderN * MaxParticlesPerRender + i;
                positionBuffer[i] = cloud.Positions[cloudIndex];
                colorBuffer[i] = cloud.Colors[cloudIndex];
                orientationBuffer[i] = cloud.Orientations[cloudIndex];
			}
        }

        /// <summary>
        /// Draws all the particles in a cloud. The cloud system must be initialized, and its projection matrix must be set
        /// before this is called.
        /// The texture used to draw the particles is specified by cloud.Texture.
        /// </summary>
        /// <param name="cloud">The particle cloud to render.</param>
        /// <param name="world">The world matrix, indicating the position, rotation, and scale of the particle cloud.</param>
        /// <param name="view">The view matrix, indicating the camera's current point of view.</param>
        public void DrawParticleCloud(ParticleCloud cloud, Matrix world, Matrix view)
        {
            // Verify that we have been properly initialized.
            Debug.Assert(isInitialized, "ParticleCloudSystem has not been initialized. "
                + "You must call Initialize() before it can draw anything.");
            Debug.Assert(isProjectionMatrixSet, "ParticleCloudSystem has not been given a projection matrix. "
                + "You must write to Projection before it can draw anything.");

            // Remember if ZWrite was enabled.
            bool wasZWriteEnabled = device.RenderState.DepthBufferWriteEnable;
            
            // Get the number of particles to draw
            int numParticles = cloud.NumberOfParticles;

            // If there is nothing to draw, just skip right now.
            if (numParticles == 0)
                return;

            // How many render calls must be use to draw these particles?
            int numRenders = 1 + numParticles / (MaxParticlesPerRender + 1);

            // Set the correct effect technique
            particleEffect.CurrentTechnique = cloud.SortingEnabled ? sortedTechnique : unsortedTechnique;

            // Set vertex and index stuff
            device.VertexDeclaration = vertexDeclaration;
            device.Vertices[0].SetSource(vertexBuffer, 0, ParticleCloudVertex.SizeInBytes);
            device.Indices = indexBuffer;

            // Assign shader uniforms
            if (obsoleteProjection)
            {
                projectionParam.SetValue(projection);
                obsoleteProjection = false;
            }
            worldParam.SetValue(world);
            worldViewParam.SetValue(world * view);
            textureParam.SetValue(cloud.Texture);

            // Are the particle billboards axis-aligned?
            if (cloud.AxisEnabled)
            {
                // Get the forward vector from the view matrix. Remember that the view matrix's rotation is
                // inverted, so we cannot use view.Forward.
                Vector3 forward = new Vector3(view.M13, view.M23, view.M33);

                Vector3 right = Vector3.Cross(forward, cloud.Axis);
                right.Normalize();
                Vector3 up = cloud.Axis;

                Vector3.TransformNormal(ref right, ref view, out right);
                Vector3.TransformNormal(ref up, ref view, out up);

                billboardRightParam.SetValue(right);
                billboardUpParam.SetValue(up);
            }
            else
            {
                billboardRightParam.SetValue(Vector3.Right);
                billboardUpParam.SetValue(Vector3.Up);
            }

            // Start rendering the particles
            particleEffect.Begin();            
            for (int i = 0; i < particleEffect.CurrentTechnique.Passes.Count; i++)
            {
                EffectPass pass = particleEffect.CurrentTechnique.Passes[i];
                
                for (int renderN = 0; renderN < numRenders; renderN++)
                {
                    // Calculate the number of particles to render in this render call
                    int numParticlesToRender = numParticles - renderN * MaxParticlesPerRender;
                    if (numParticlesToRender > MaxParticlesPerRender)
                        numParticlesToRender = MaxParticlesPerRender;

                    // Get the array start and end indices
                    int arrayStart = renderN * MaxParticlesPerRender;
                    int arrayEnd = arrayStart + numParticlesToRender - 1;

                    // Assign the parameters accordingly
                    if (renderN == 0)
                    {
                        // The first render pass can use the arrays directly. No need to waste time setting up the buffers.
                        positionsParam.SetValue(cloud.Positions);
                        colorsParam.SetValue(cloud.Colors);
                        orientationsParam.SetValue(cloud.Orientations);
                    }
                    else
                    {
                        // We need to construct a new array where the next 80 particles are first in the array.
                        PrepareParticleBuffers(cloud, renderN);
                        positionsParam.SetValue(positionBuffer);
                        colorsParam.SetValue(colorBuffer);
                        orientationsParam.SetValue(orientationBuffer);
                    }

                    pass.Begin();
                    device.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        4 * numParticlesToRender,
                        0,
                        2 * numParticlesToRender);
                    pass.End();
                }
            }
            particleEffect.End();

            // It seems that ZWriteEnable propagates to other drawing calls in XNA, so we better clean up
            // after ourselves.
            if (cloud.SortingEnabled && wasZWriteEnabled)
            {
                device.RenderState.DepthBufferWriteEnable = true;
            }
        }
    }
}
