using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// A static tree mesh composed of simple lines. Each branch is displayed as a line.
    /// Useful for testing and debugging.
    /// </summary>
    public class TreeLineMesh : IDisposable
    {
        private GraphicsDevice device;
        private VertexBuffer vbuffer;
        private IndexBuffer ibuffer;
        private int numvertices;
        private int numlines;
        private BasicEffect effect;
        private VertexDeclaration declaration;

        public TreeLineMesh(GraphicsDevice device, TreeSkeleton skeleton)
        {
            this.device = device;

            Init(skeleton);
        }

        private void Init(TreeSkeleton skeleton)
        {
            // Get branch transforms
            Matrix[] transforms = new Matrix[skeleton.Branches.Count];
            skeleton.CopyAbsoluteBranchTransformsTo(transforms);

            // Create the vertices and indices
            numlines = skeleton.Branches.Count;
            numvertices = numlines * 2;
            VertexPositionColor[] vertices = new VertexPositionColor[numvertices];
            short[] indices = new short[numlines * 2];

            int vidx = 0;
            int iidx = 0;

            for (int i = 0; i < skeleton.Branches.Count; i++)
            {
                TreeBranch branch = skeleton.Branches[i];

                indices[iidx++] = (short)vidx;
                indices[iidx++] = (short)(vidx + 1);

                vertices[vidx++] = new VertexPositionColor(transforms[i].Translation, Color.White);
                vertices[vidx++] = new VertexPositionColor(Vector3.Transform(new Vector3(0, branch.Length, 0), transforms[i]), Color.White);
            }

            // Create buffers
            vbuffer = new VertexBuffer(device, numvertices * VertexPositionColor.SizeInBytes, BufferUsage.None);
            vbuffer.SetData<VertexPositionColor>(vertices);
            ibuffer = new IndexBuffer(device, indices.Length * sizeof(short), BufferUsage.None, IndexElementSize.SixteenBits);
            ibuffer.SetData<short>(indices);

            // Create vertex declaration
            declaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);

            // Create the effect
            effect = new BasicEffect(device, new EffectPool());
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            effect.World = world;
            effect.View = view;
            effect.Projection = projection;

            device.VertexDeclaration = declaration;
            device.Vertices[0].SetSource(vbuffer, 0, VertexPositionColor.SizeInBytes);
            device.Indices = ibuffer;

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, numvertices, 0, numlines);
                pass.End();
            }
            effect.End();
        }

        #region IDisposable Members

        public void Dispose()
        {
            vbuffer.Dispose();
            ibuffer.Dispose();
            declaration.Dispose();
            effect.Dispose();
        }

        #endregion
    }
}
