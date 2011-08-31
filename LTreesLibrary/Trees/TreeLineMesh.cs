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
            vbuffer = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, numvertices, BufferUsage.None);
            vbuffer.SetData<VertexPositionColor>(vertices);
            ibuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
            ibuffer.SetData<short>(indices);

            // Create the effect
            effect = new BasicEffect(device);
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            effect.World = world;
            effect.View = view;
            effect.Projection = projection;

            device.SetVertexBuffer(vbuffer);
            device.Indices = ibuffer;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
            	pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, numvertices, 0, numlines);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            vbuffer.Dispose();
            ibuffer.Dispose();
            effect.Dispose();
        }

        #endregion
    }
}
