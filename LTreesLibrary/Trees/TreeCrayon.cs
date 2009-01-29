/*
 * Copyright (c) 2009 Asger Feldthaus
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// The object being controlled by the L-system. Each instruction resolves to one or more
    /// operations on the crayon.
    /// A crayon knows its current position, rotation, and scale in 3D space, and paints tree
    /// branches as it moves around, like a pencil drawing lines on a piece of paper.
    /// </summary>
    /// <remarks>
    /// Each crayon has one skeleton which it contiuously build upon. One crayon cannot be
    /// used to create more than one skeleton.
    /// </remarks>
    public class TreeCrayon
    {
        private struct State
        {
            public int ParentIndex;
            public float ParentPosition;
            public float Scale;
            public Quaternion Rotation;
            public int Level;
            public float RadiusScale;
            public int ParentBoneIndex;
            public int BoneLevel;
        }
        
        private State state;
        private Stack<State> stack = new Stack<State>();
        private TreeSkeleton skeleton = new TreeSkeleton();
        private List<Matrix> branchTransforms = new List<Matrix>();
        private List<int> boneEndings = new List<int>(); // Map from bone index to branch where it was created

        private TreeContraints constraints;

        private int boneLevels = 3;
        private const int MaxBones = 20; // Should match the number in Trunk.fx and Leaves.fx

        /// <summary>
        /// How long a chain of bones is allowed to be. Default is 3.
        /// </summary>
        public int BoneLevels
        {
            get { return boneLevels; }
            set { boneLevels = value; }
        }

        /// <summary>
        /// Contraints to apply in the <see cref="Forward"/> operation.
        /// </summary>
        public TreeContraints Constraints
        {
            get { return constraints; }
            set { constraints = value; }
        }

        /// <summary>
        /// The skeleton being built by the crayon.
        /// </summary>
        public TreeSkeleton Skeleton
        {
            get { return skeleton; }
        }

        /// <summary>
        /// The current branch level of the crayon. This will typically decrease
        /// in recursive production calls to prevent it from running forever.
        /// It it drops negative, production calls are ignored.
        /// </summary>
        public int Level
        {
            get { return state.Level; }
            set { state.Level = value; }
        }

        /// <summary>
        /// The current scale of the crayon.
        /// </summary>
        public float CurrentScale
        {
            get { return state.Scale; }
        }



        /// <summary>
        /// Calculates and returns the absolute transform of the crayon.
        /// </summary>
        /// <returns>Absolute transformation of the crayon.</returns>
        public Matrix GetTransform()
        {
            Matrix m = Matrix.CreateFromQuaternion(state.Rotation);
            if (state.ParentIndex == -1)
                return m;

            m.M42 = state.ParentPosition * skeleton.Branches[state.ParentIndex].Length;
            return m * branchTransforms[state.ParentIndex];
        }


        public TreeCrayon()
        {
            state.ParentIndex = -1;
            state.ParentPosition = 1.0f;
            state.Scale = 1.0f;
            state.Rotation = Quaternion.Identity;
            state.Level = 1;
            state.RadiusScale = 1.0f;
            state.ParentBoneIndex = -1;
            state.BoneLevel = 0;
        }

        /// <summary>
        /// Moves forward while painting a branch here.
        /// </summary>
        /// <param name="length">Length of the new branch. The current scale will be applied to this.</param>
        /// <param name="radiusEndScale">How much smaller the ending radius should be. The equation is: StartRadius * RadiusEndScale = EndRadius.</param>
        /// <remarks>
        /// The crayon always moves along its local Y-axis, which is initially upwards.
        /// </remarks>
        public void Forward(float length, float radiusEndScale)
        {
            // Run the constraints
            if (constraints != null && !constraints.ConstrainForward(this, ref length, ref radiusEndScale))
                return;

            // Create the branch
            TreeBranch branch = new TreeBranch(
                state.Rotation,
                length * state.Scale,
                GetRadiusAt(state.ParentIndex, state.ParentPosition) * state.RadiusScale,
                GetRadiusAt(state.ParentIndex, state.ParentPosition) * state.RadiusScale * radiusEndScale,
                state.ParentIndex,
                state.ParentPosition);
            branch.BoneIndex = state.ParentBoneIndex;
            skeleton.Branches.Add(branch);
            branchTransforms.Add(GetTransform());

            // Set newest branch to parent
            state.ParentIndex = skeleton.Branches.Count - 1;

            // Rotation is relative to the current parent, so set to identity
            // to maintain original orientation
            state.Rotation = Quaternion.Identity;

            // Move to the end of the branch
            state.ParentPosition = 1.0f;

            // Move radius scale back to one, since the radius will now be relative to the new parent
            state.RadiusScale = 1.0f;
        }

        /// <summary>
        /// Moves backwards without drawing any branches.
        /// </summary>
        /// <param name="distance">Distance to move backwards.</param>
        /// <remarks>
        /// This follows the hiarchy of branches towards the root, so it may not
        /// move in a straight line.
        /// </remarks>
        public void Backward(float distance)
        {
            distance = distance * state.Scale;
            while (state.ParentIndex != -1 && distance > 0.0f)
            {
                float distanceOnBranch = state.ParentPosition * skeleton.Branches[state.ParentIndex].Length;
                if (distance > distanceOnBranch)
                {
                    state.ParentIndex = skeleton.Branches[state.ParentIndex].ParentIndex;
                    state.ParentPosition = 1.0f;
                    state.Rotation = Quaternion.Identity;
                }
                else
                {
                    state.ParentPosition -= distance / skeleton.Branches[state.ParentIndex].Length;
                }
                distance -= distanceOnBranch;
            }
            state.BoneLevel = 99;
        }

        /// <summary>
        /// Rotates the crayon around its local Y-axis.
        /// </summary>
        public void Twist(float angleRadians)
        {
            state.Rotation = state.Rotation * Quaternion.CreateFromYawPitchRoll(angleRadians, 0, 0);
            if (float.IsNaN(state.Rotation.X))
            {
            }
        }

        /// <summary>
        /// Rotates the crayon around its local X-axis.
        /// </summary>
        public void Pitch(float angleRadians)
        {
            state.Rotation = state.Rotation * Quaternion.CreateFromYawPitchRoll(0, angleRadians, 0);
            if (float.IsNaN(state.Rotation.X))
            {
            }
        }

        /// <summary>
        /// Scales the length of following branches.
        /// </summary>
        public void Scale(float scale)
        {
            state.Scale = state.Scale * scale;
        }

        /// <summary>
        /// Scales the radius of following branches.
        /// Note that radius is proportional to the radius at the parent's branch where the branch sprouts.
        /// </summary>
        /// <param name="scale"></param>
        public void ScaleRadius(float scale)
        {
            state.RadiusScale = state.RadiusScale * scale;
        }

        /// <summary>
        /// Remembers the crayon's current state, by putting it on top of the state stack.
        /// </summary>
        public void PushState()
        {
            stack.Push(state);
        }

        /// <summary>
        /// Restores the previously stored state and removes it from the top of the state stack.
        /// </summary>
        public void PopState()
        {
            state = stack.Pop();
        }

        /// <summary>
        /// Places a leaf here.
        /// </summary>
        /// <param name="rotationRadians">The leaf's rotation relative to the camera. For axis-aligned leaves, this better be 0.</param>
        /// <param name="size">Width and height of the leaf, in view space.</param>
        /// <param name="color">Color tint of the leaf.</param>
        /// <param name="axisOffset">Position offset along the leaf axis, if it exists.</param>
        /// <seealso cref="TreeLeaf"/>
        public void Leaf(float rotationRadians, Vector2 size, Vector4 color, float axisOffset)
        {
            skeleton.Leaves.Add(new TreeLeaf(state.ParentIndex, color, rotationRadians, size, state.ParentBoneIndex, axisOffset));
        }

        /// <summary>
        /// Places the end of a bone here, unless the bone level would become too high, or too many bones have been added already.
        /// The bone's origin is at the previously added bone (as seen from the current state).
        /// </summary>
        /// <param name="delta">Amount to adjust the current bone level by</param>
        /// <remarks>
        /// Branches added between this and the previously added bone will be controlled by the new bone.
        /// 
        /// The current bone is part of the crayon's state, like position and rotation, and is affected by PushState and PopState.
        /// </remarks>
        public void Bone(int delta)
        {
            if (state.BoneLevel > boneLevels + delta || skeleton.Bones.Count == MaxBones)
                return;

            // Get index of the parent
            int parent = state.ParentBoneIndex;

            // Get the parent's absolute transform
            Matrix parentTransform = Matrix.Identity;
            Matrix parentInverseTransform = Matrix.Identity;
            float parentLength = 0.0f;
            if (parent != -1)
            {
                parentTransform = skeleton.Bones[parent].ReferenceTransform;
                parentInverseTransform = skeleton.Bones[parent].InverseReferenceTransform;
                parentLength = skeleton.Bones[parent].Length;
            }

            // Find the starting and ending point of the new bone
            Vector3 targetLocation = GetTransform().Translation;
            Vector3 fromLocation = parentTransform.Translation + parentTransform.Up * parentLength;

            // Direction of the bone's Y-axis
            Vector3 directionY = Vector3.Normalize(targetLocation - fromLocation);

            // Choose arbitrary perpendicular X and Z axes
            Vector3 directionX;
            Vector3 directionZ;
            if (directionY.Y < 0.50f)
            {
                directionX = Vector3.Normalize(Vector3.Cross(directionY, Vector3.Up));
                directionZ = Vector3.Normalize(Vector3.Cross(directionX, directionY));
            }
            else
            {
                directionX = Vector3.Normalize(Vector3.Cross(directionY, Vector3.Backward));
                directionZ = Vector3.Normalize(Vector3.Cross(directionX, directionY));
            }

            // Construct the absolute rotation of the child
            Matrix childAbsoluteTransform = Matrix.Identity;
            childAbsoluteTransform.Right = directionX;
            childAbsoluteTransform.Up = directionY;
            childAbsoluteTransform.Backward = directionZ;
            childAbsoluteTransform.Translation = fromLocation;

            // Calculate the relative transformation
            Matrix relativeTransformation = childAbsoluteTransform * parentInverseTransform;
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(relativeTransformation);

            // Create the new bone
            TreeBone bone = new TreeBone();
            bone.ReferenceTransform = childAbsoluteTransform;
            bone.InverseReferenceTransform = Matrix.Invert(bone.ReferenceTransform);
            bone.Length = Vector3.Distance(fromLocation, targetLocation);
            bone.Rotation = rotation;
            bone.ParentIndex = parent;
            bone.Stiffness = skeleton.Branches[state.ParentIndex].StartRadius; // 1.0f; // TODO: Set stiffness according to radius
            bone.EndBranchIndex = state.ParentIndex;

            // Add the bone to the skeleton
            skeleton.Bones.Add(bone);

            // Set this bone as the parent
            int endIndex = (state.ParentBoneIndex == -1? -1 : skeleton.Bones[state.ParentBoneIndex].EndBranchIndex);
            int boneIndex = state.ParentBoneIndex = skeleton.Bones.Count - 1;
            state.BoneLevel -= delta;

            // Update the bone index on branches
            int branchIndex = state.ParentIndex;
            while (branchIndex != endIndex)
            {
                TreeBranch branch = skeleton.Branches[branchIndex];
                branch.BoneIndex = boneIndex;
                skeleton.Branches[branchIndex] = branch;
                branchIndex = branch.ParentIndex;
            }
        }
        
        /// <summary>
        /// Returns the radius of a given branch at the height.
        /// </summary>
        private float GetRadiusAt(int parentIndex, float f)
        {
            if (parentIndex == -1)
                return 128.0f;

            TreeBranch branch = skeleton.Branches[parentIndex];

            return branch.StartRadius + f * (branch.EndRadius - branch.StartRadius);
        }
    }
}
