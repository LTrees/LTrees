using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Describes one branch segment in a tree.
    /// </summary>
    /// <remarks>
    /// A branch's origin is always somewhere on its parent branch (except for the root branch).
    /// </remarks>
    /// <see cref="TreeSkeleton"/>
    public struct TreeBranch
    {
        /// <summary>
        /// Rotation relative to the branch's parent.
        /// </summary>
        public Quaternion Rotation;
        
        /// <summary>
        /// Length of the branch.
        /// </summary>
        public float Length;

        /// <summary>
        /// Radius at the start of the branch.
        /// </summary>
        public float StartRadius;

        /// <summary>
        /// Radius at the end of the branch.
        /// </summary>
        public float EndRadius;

        /// <summary>
        /// Index of the parent branch, or -1 if this is the root branch.
        /// </summary>
        public int ParentIndex;

        /// <summary>
        /// Where on the parent the branch is located. 0.0f is at the start, 1.0f is at the end.
        /// </summary>
        public float ParentPosition;

        /// <summary>
        /// Index of the bone controlling this branch.
        /// </summary>
        public int BoneIndex;

        public TreeBranch(Quaternion rotation,
            float length,
            float startRadius,
            float endRadius,
            int parentIndex,
            float parentPosition)
        {
            Rotation = rotation;
            Length = length;
            StartRadius = startRadius;
            EndRadius = endRadius;
            ParentIndex = parentIndex;
            ParentPosition = parentPosition;
            BoneIndex = -1;
        }
    }
}
