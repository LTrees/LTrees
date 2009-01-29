using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Renders a tree mesh and corresponding leaves. Contains a tree skeleton and the corresponding
    /// tree mesh, leaf cloud, animation state, effects, and textures.
    /// </summary>
    /// <remarks>
    /// Because effects should be loaded by the content manager, they must be set manually before the
    /// can be rendered.
    /// 
    /// This is a good place to get started, but to gain more flexibility and performance, you will
    /// eventually need to use the TreeMesh, TreeLeafCloud, TreeAnimationState and TreeSkeleton classes directly.
    /// In a serious application, it is recommended that you write your own application-specific substitute for this class.
    /// </remarks>
    public class SimpleTree
    {
        private GraphicsDevice device;
        private TreeSkeleton skeleton;
        private TreeMesh trunk;
        private TreeLeafCloud leaves;
        private TreeAnimationState animationState;
        private Effect trunkEffect;
        private Effect leafEffect;
        private Texture2D trunkTexture;
        private Texture2D leafTexture;
        private Matrix[] bindingMatrices;
        private BasicEffect boneEffect;

        public GraphicsDevice GraphicsDevice
        {
            get { return device; }
        }

        /// <summary>
        /// The tree structure displayed.
        /// Setting this will reset the current animation state and result in new meshes being generated.
        /// </summary>
        public TreeSkeleton Skeleton
        {
            get { return skeleton; }
            set { skeleton = value; UpdateSkeleton(); }
        }

        public TreeMesh TrunkMesh
        {
            get { return trunk; }
        }

        public TreeLeafCloud LeafCloud
        {
            get { return leaves; }
        }

        /// <summary>
        /// The current position of all the bones.
        /// Setting this to a new animation state has no performance hit.
        /// </summary>
        public TreeAnimationState AnimationState
        {
            get { return animationState; }
            set { animationState = value; }
        }

        /// <summary>
        /// Effect used to draw the trunk.
        /// </summary>
        public Effect TrunkEffect
        {
            get { return trunkEffect; }
            set { trunkEffect = value; }
        }

        /// <summary>
        /// Effect used to draw the leaves.
        /// </summary>
        public Effect LeafEffect
        {
            get { return leafEffect; }
            set { leafEffect = value; }
        }

        /// <summary>
        /// Texture on the trunk.
        /// </summary>
        public Texture2D TrunkTexture
        {
            get { return trunkTexture; }
            set { trunkTexture = value; }
        }

        /// <summary>
        /// Leaves on the trunk.
        /// </summary>
        public Texture2D LeafTexture
        {
            get { return leafTexture; }
            set { leafTexture = value; }
        }

        private void UpdateSkeleton()
        {
            this.trunk = new TreeMesh(device, skeleton);
            this.leaves = new TreeLeafCloud(device, skeleton);
            this.animationState = new TreeAnimationState(skeleton);
            this.bindingMatrices = new Matrix[skeleton.Bones.Count];
        }

        public SimpleTree(GraphicsDevice device)
        {
            this.device = device;
            this.boneEffect = new BasicEffect(device, new EffectPool());
        }
        public SimpleTree(GraphicsDevice device, TreeSkeleton skeleton)
        {
            this.device = device;
            this.skeleton = skeleton;
            this.boneEffect = new BasicEffect(device, new EffectPool());
            UpdateSkeleton();
        }

        /// <summary>
        /// Draws the trunk using the specified world, view, and projection matrices.
        /// </summary>
        /// <param name="world">World matrix.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        /// <exception cref="InvalidOperationException">If no trunk effect is set.</exception>
        /// <exception cref="InvalidOperationException">If no skeleton is set.</exception>
        /// <remarks>
        /// This method sets all the appropriate effect parameters.
        /// </remarks>
        public void DrawTrunk(Matrix world, Matrix view, Matrix projection)
        {
            if (skeleton == null)
                throw new InvalidOperationException("A skeleton must be set before trunk can be rendered.");

            if (trunkEffect == null)
                throw new InvalidOperationException("TrunkEffect must be set before trunk can be rendered.");

            trunkEffect.Parameters["World"].SetValue(world);
            trunkEffect.Parameters["View"].SetValue(view);
            trunkEffect.Parameters["Projection"].SetValue(projection);

            skeleton.CopyBoneBindingMatricesTo(bindingMatrices, animationState.BoneRotations);
            trunkEffect.Parameters["Bones"].SetValue(bindingMatrices);

            trunkEffect.Parameters["Texture"].SetValue(trunkTexture);

            trunk.Draw(trunkEffect);
        }

        /// <summary>
        /// Draws the leaves using the specified world, view, and projection matrices.
        /// </summary>
        /// <param name="world">World matrix.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        /// <exception cref="InvalidOperationException">If no leaf effect is set.</exception>
        /// <exception cref="InvalidOperationException">If no skeleton is set.</exception>
        /// <remarks>
        /// This method sets all the appropriate effect parameters.
        /// </remarks>
        public void DrawLeaves(Matrix world, Matrix view, Matrix projection)
        {
            if (skeleton == null)
                throw new InvalidOperationException("A skeleton must be set before leaves can be rendered.");

            if (leafEffect == null)
                throw new InvalidOperationException("LeafEffect must be set before leaves can be rendered.");

            leafEffect.Parameters["WorldView"].SetValue(world * view);
            leafEffect.Parameters["View"].SetValue(view);
            leafEffect.Parameters["Projection"].SetValue(projection);
            leafEffect.Parameters["LeafScale"].SetValue(world.Right.Length());

            if (skeleton.LeafAxis == null)
            {
                leafEffect.Parameters["BillboardRight"].SetValue(Vector3.Right);
                leafEffect.Parameters["BillboardUp"].SetValue(Vector3.Up);
            }
            else
            {
                Vector3 axis = skeleton.LeafAxis.Value;
                Vector3 forward = new Vector3(view.M13, view.M23, view.M33);

                Vector3 right = Vector3.Cross(forward, axis);
                right.Normalize();
                Vector3 up = axis;

                Vector3.TransformNormal(ref right, ref view, out right);
                Vector3.TransformNormal(ref up, ref view, out up);

                leafEffect.Parameters["BillboardRight"].SetValue(right);
                leafEffect.Parameters["BillboardUp"].SetValue(up);
            }

            skeleton.CopyBoneBindingMatricesTo(bindingMatrices, animationState.BoneRotations);
            leafEffect.Parameters["Bones"].SetValue(bindingMatrices);

            leafEffect.Parameters["Texture"].SetValue(leafTexture);

            leaves.Draw(leafEffect);
        }

        /// <summary>
        /// Draws the tree's bones as lines. Useful for testing and debugging.
        /// </summary>
        /// <param name="world">World matrix.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        public void DrawBonesAsLines(Matrix world, Matrix view, Matrix projection)
        {
            if (skeleton == null)
                throw new InvalidOperationException("A skeleton must be set before bones can be rendered.");

            if (leafEffect == null)
                throw new InvalidOperationException("LeafEffect must be set before leaves can be rendered.");

            if (skeleton.Bones.Count == 0)
                return;

            boneEffect.World = world;
            boneEffect.View = view;
            boneEffect.Projection = projection;

            bool wasDepthBufferOn = device.RenderState.DepthBufferEnable;
            device.RenderState.DepthBufferEnable = false;
            device.RenderState.AlphaTestEnable = false;
            device.RenderState.AlphaBlendEnable = false;

            Matrix[] transforms = new Matrix[skeleton.Bones.Count];
            skeleton.CopyAbsoluteBoneTranformsTo(transforms, animationState.BoneRotations);

            VertexPositionColor[] vertices = new VertexPositionColor[skeleton.Bones.Count * 2];
            for (int i = 0; i < skeleton.Bones.Count; i++)
            {
                vertices[2 * i] = new VertexPositionColor(transforms[i].Translation, Color.Red);
                vertices[2 * i + 1] = new VertexPositionColor(transforms[i].Translation + transforms[i].Up * skeleton.Bones[i].Length, Color.Red);
            }

            boneEffect.Begin();
            foreach (EffectPass pass in boneEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, skeleton.Bones.Count);
                pass.End();
            }
            boneEffect.End();

            device.RenderState.DepthBufferEnable = wasDepthBufferOn;
        }
    }
}
