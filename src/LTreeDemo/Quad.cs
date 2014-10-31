using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LTreeDemo
{
    class Quad : IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        public Quad(GraphicsDevice device, float width, float height)
        {
            GraphicsDevice = device;
            Width = width;
            Height = height;
            Initialize();
        }

        private void Initialize()
        {
            float w = Width / 2.0f;
            float h = Height / 2.0f;
            VertexPositionNormalTexture[] vertices = 
            {
                new VertexPositionNormalTexture(new Vector3(-w, -h, 0), new Vector3(0, 0, 1), new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(-w, h, 0), new Vector3(0, 0, 1), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(w, h, 0), new Vector3(0, 0, 1), new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(w, -h, 0), new Vector3(0, 0, 1), new Vector2(1, 1)),
            };
            short[] indices =
            {
                0, 1, 2,
                2, 3, 0
            };
            vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.None);
            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);

            vertexBuffer.SetData(vertices);
            indexBuffer.SetData(indices);
        }

        /// <summary>
        /// Draws the quad using the specified effect. Sets no parameters on the effect, nor any render states on
        /// the graphics device. Those must be set appropriately before calling Draw.
        /// This is a convenience function, calling Begin, End, and DrawGeometry for you.
        /// </summary>
        /// <param name="effect">The effect to draw with.</param>
        public void Draw(Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                DrawGeometry();
            }
        }

        /// <summary>
        /// Sends vertices and indices to the graphics device. Should be called between the Begin and End call of
        /// an effect pass.
        /// </summary>
        public void DrawGeometry()
        {
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }

        #region IDisposable Members

        public void Dispose()
        {
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        }

        #endregion
    }
}
