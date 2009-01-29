using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LTreeDemo
{
    class Grid
    {
        private GraphicsDevice device;
        private VertexBuffer vbuffer;
        private IndexBuffer ibuffer;
        private VertexDeclaration vdeclaration;
        private int numrows;
        private int numcolumns;
        private float size;
        private int numlines;
        private BasicEffect effect;

        public Grid(GraphicsDevice device)
        {
            this.device = device;

            numrows = 10;
            numcolumns = 10;
            size = 10000.0f;

            Init();
        }

        private void Init()
        {
            // Create vertices and indices
            VertexPositionColor[] vertices = new VertexPositionColor[(numrows + numcolumns) * 2 + 4];
            short[] indices = new short[(numrows + numcolumns) * 2 + 4];

            int iidx = 0;
            int vidx = 0;

            for (int x = 0; x <= numcolumns; x++)
            {
                int index = vidx;
                float px = (x - numcolumns / 2.0f) * (size / numcolumns);
                vertices[vidx++] = new VertexPositionColor(new Vector3(px, 0, -size / 2.0f), Color.White);
                vertices[vidx++] = new VertexPositionColor(new Vector3(px, 0, size / 2.0f), Color.White);

                indices[iidx++] = (short)index;
                indices[iidx++] = (short)(index + 1);
            }
            for (int z = 0; z <= numrows; z++)
            {
                int index = vidx;
                float pz = (z - numrows / 2.0f) * (size / numrows);
                vertices[vidx++] = new VertexPositionColor(new Vector3(-size / 2.0f, 0, pz), Color.White);
                vertices[vidx++] = new VertexPositionColor(new Vector3(size / 2.0f, 0, pz), Color.White);

                indices[iidx++] = (short)index;
                indices[iidx++] = (short)(index + 1);
            }

            // Create buffers
            vbuffer = new VertexBuffer(device, vertices.Length * VertexPositionColor.SizeInBytes, BufferUsage.WriteOnly);
            vbuffer.SetData<VertexPositionColor>(vertices);

            ibuffer = new IndexBuffer(device, indices.Length * sizeof(short), BufferUsage.WriteOnly, IndexElementSize.SixteenBits);
            ibuffer.SetData<short>(indices);

            // Create vertex declaration
            vdeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);

            numlines = vertices.Length / 2;

            // Create the effect
            effect = new BasicEffect(device, new EffectPool());
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            device.Vertices[0].SetSource(vbuffer, 0, VertexPositionColor.SizeInBytes);
            device.VertexDeclaration = vdeclaration;

            effect.World = world;
            effect.View = view;
            effect.Projection = projection;

            device.Indices = ibuffer;
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, numlines * 2, 0, numlines);

                pass.End();
            }
            effect.End();
        }
    }
}
