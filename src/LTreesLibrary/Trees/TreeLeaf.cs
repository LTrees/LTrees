using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Describes a leaf on a tree. Leaves are always placed at the end of a branch.
    /// </summary>
    public struct TreeLeaf
    {
        /// <summary>
        /// Index of the branch carrying the leaf.
        /// </summary>
        public int ParentIndex;

        /// <summary>
        /// Color tint of the leaf.
        /// </summary>
        public Vector4 Color;
        
        /// <summary>
        /// Rotation of the leaf, in radians.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// Width and height of the leaf.
        /// </summary>
        public Vector2 Size;

        /// <summary>
        /// Index of the bone controlling this leaf.
        /// </summary>
        public int BoneIndex;

        /// <summary>
        /// Leaf's position offset along the leaf axis. Only used when the leaf axis is non-null on the tree skeleton.
        /// LeafAxis * AxisOffset will be added to the leaf's position on the branch.
        /// </summary>
        public float AxisOffset;

        public TreeLeaf(int parentIndex, Vector4 color, float rotation, Vector2 size, int boneIndex, float axisOffset)
        {
            this.ParentIndex = parentIndex;
            this.Color = color;
            this.Rotation = rotation;
            this.Size = size;
            this.BoneIndex = boneIndex;
            this.AxisOffset = axisOffset;
        }
    }
}
