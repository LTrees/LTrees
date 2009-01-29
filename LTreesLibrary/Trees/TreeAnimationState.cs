using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Animation state for one particular instance of a tree. One tree mesh can be rendered
    /// in several differently deformed positions, when each position has its own animation state.
    /// An animation state contains the local rotation for each bone in the tree.
    /// </summary>
    /// <remarks>
    /// The bone rotations can conveniently as the argument for <see cref="TreeSkeleton.CopyBindingMatricesTo"/>.
    /// </remarks>
    public class TreeAnimationState
    {
        private Quaternion[] rotations;

        /// <summary>
        /// Rotation of each bone in the tree, relative to the bone's parent.
        /// </summary>
        public Quaternion[] BoneRotations
        {
            get { return rotations; }
            set { rotations = value; }
        }

        /// <summary>
        /// Creates an empty animation state. Cannot be used for rendering.
        /// The bone rotations are set to an array of length zero.
        /// </summary>
        public TreeAnimationState()
        {
            rotations = new Quaternion[0];
        }
        
        /// <summary>
        /// Creates an animation state initialized with a number of bones, but
        /// each bone will initially have an invalid rotation associated with it.
        /// </summary>
        public TreeAnimationState(int numBones)
        {
            rotations = new Quaternion[numBones];
        }

        /// <summary>
        /// Creates an animation state for the given tree skeleton, initially setting
        /// each bone's rotation to its rotation at the frame of reference.
        /// </summary>
        /// <param name="skeleton">The skeleton to create an animation state for.</param>
        public TreeAnimationState(TreeSkeleton skeleton)
        {
            rotations = new Quaternion[skeleton.Bones.Count];
            for (int i = 0; i < skeleton.Bones.Count; i++)
            {
                rotations[i] = skeleton.Bones[i].Rotation;
            }
        }

        /// <summary>
        /// Rotates a bone around a given axis.
        /// </summary>
        /// <param name="index">Index of the bone to rotate.</param>
        /// <param name="axis">Axis to rotate the bone around.</param>
        /// <param name="radians">Angle to rotate by, in radians.</param>
        public void RotateBoneBy(int index, Vector3 axis, float radians)
        {
            rotations[index] = rotations[index] * Quaternion.CreateFromAxisAngle(axis, radians);
        }

        /// <summary>
        /// Interpolates between two animation states and stores the result in a third state.
        /// </summary>
        /// <param name="state1">The first animation state.</param>
        /// <param name="state2">The second animation state.</param>
        /// <param name="destinationState">Where to store the interpolated result. This may be the same as either of the two input states.</param>
        /// <param name="f">Interpolation factor, between 0 and 1. 0 weights towards the first state, and 1 weights towards the second state.</param>
        /// <remarks>The three animation states must have the same number of bones.</remarks>
        public static void Interpolate(TreeAnimationState state1, TreeAnimationState state2, TreeAnimationState destinationState, float f)
        {
            if (state1.BoneRotations.Length != state2.BoneRotations.Length || state1.BoneRotations.Length != destinationState.BoneRotations.Length)
                throw new ArgumentException("All three animation states must have the same number of bones");

            if (f < 0.0f || f > 1.0f)
                throw new ArgumentException("Interpolation factor must be between 0 and 1.", "f");

            for (int i = 0; i < destinationState.BoneRotations.Length; i++)
            {
                destinationState.BoneRotations[i] = Quaternion.Slerp(state1.BoneRotations[i], state2.BoneRotations[i], f);
            }
        }
    }
}
