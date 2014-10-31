/*
 * Copyright (c) 2007-2009 Asger Feldthaus
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Describes the topology of a tree. Does not contain any vertex data or similar information required to render the tree.
    /// A mesh should be created using the tree skeleton.
    /// </summary>
    /// <see cref="TreeCylMesh"/>
    public class TreeSkeleton
    {
        private List<TreeBranch> branches = new List<TreeBranch>();
        private List<TreeLeaf> leaves = new List<TreeLeaf>();
        private List<TreeBone> bones = new List<TreeBone>();
        private float textureHeight;

        /// <summary>
        /// If null, the leaves are free billboards. Otherwise, they are axis-aligned
        /// around this axis. Must be a normalized vector or null.
        /// </summary>
        public Vector3? LeafAxis { get; set; }

        /// <summary>
        /// How far to stretch the texture Y coordinates along the tree.
        /// A typical value is 512.0f.
        /// </summary>
        public float TextureHeight
        {
            get { return textureHeight; }
            set { textureHeight = value; }
        }
        
        /// <summary>
        /// The list of leaves on the tree.
        /// </summary>
        public List<TreeLeaf> Leaves
        {
            get { return leaves; }
            set { leaves = value; }
        }


        /// <summary>
        /// The list of branches in the tree. The branches must be topologically sorted, that is, a branch must
        /// never appear before its own parent in the array. The root branch must therefore be the first branch.
        /// </summary>
        public List<TreeBranch> Branches
        {
            get { return branches; }
        }

        /// <summary>
        /// Gets the list of bones.
        /// </summary>
        public List<TreeBone> Bones
        {
            get { return bones; }
        }

        /// <summary>
        /// Radius of the trunk. Same as Branches[0].StartRadius. Useful for simple collision detection.
        /// </summary>
        public float TrunkRadius
        {
            get { return branches[0].StartRadius; }
        }

        /// <summary>
        /// Calculates the absolute transform of each branch, and copies it into a given matrix array.
        /// </summary>
        /// <param name="destinationMatrices">Where to store the branch transforms. The matrix at index N corresponds to the branch at index N.</param>
        public void CopyAbsoluteBranchTransformsTo(Matrix[] destinationMatrices)
        {
            if (destinationMatrices == null)
                throw new ArgumentNullException("destinationMatrices");
            
            if (destinationMatrices.Length < branches.Count)
                throw new ArgumentException("Destination array is too small.");

            for (int i = 0; i < branches.Count; i++)
            {
                // Get rotation matrix relative to parent (if any)
                Matrix rotationMatrix = Matrix.CreateFromQuaternion(branches[i].Rotation);
                
                int parent = branches[i].ParentIndex;
                if (parent == -1)
                {
                    // This is the root branch
                    destinationMatrices[i] = rotationMatrix;
                }
                else
                {
                    // Translate the branch along its parent. (M42 is the Y-coordinate of the translation vector)
                    rotationMatrix.M42 = branches[i].ParentPosition * branches[parent].Length;
                    
                    // Transform by parent's absolute matrix
                    destinationMatrices[i] = rotationMatrix * destinationMatrices[parent];
                }
            }
        }

        /// <summary>
        /// Finds the distance from the root to the tip of each branch, and writes it in an array.
        /// Then returns the longest of those distances found.
        /// </summary>
        /// <param name="destinationArray">Destination array to put distances into.</param>
        /// <returns>Longest root-to-branch-tip distance found</returns>
        /// <exception cref="ArgumentException">If the destination array is too short.</exception>
        public float GetLongestBranching(float[] destinationArray)
        {
            if (destinationArray.Length < branches.Count)
                throw new ArgumentException("Destination array is too small.");

            float maxdist = 0.0f;
            for (int i = 0; i < branches.Count; i++)
            {
                float dist = branches[i].Length;
                if (branches[i].ParentIndex != -1)
                    dist += destinationArray[branches[i].ParentIndex] - branches[branches[i].ParentIndex].Length * (1.0f - branches[i].ParentPosition);
                if (dist > maxdist)
                    maxdist = dist;
                destinationArray[i] = dist;
            }

            return maxdist;
        }

        /// <summary>
        /// Sets the EndRadius to 0.0f on all branches without children.
        /// This is automatically called by the TreeGenerator.
        /// </summary>
        public void CloseEdgeBranches()
        {
            // Create a map of all the branches to remember if it is a parent or not
            bool[] parentmap = new bool[branches.Count];

            for (int i = branches.Count - 1; i >= 0; --i)
            {
                int parent = branches[i].ParentIndex;
                if (parent != -1)
                    parentmap[parent] = true;
                if (!parentmap[i])
                {
                    TreeBranch branch = branches[i];
                    branch.EndRadius = 0.0f;
                    branches[i] = branch;
                }
            }
        }

        /// <summary>
        /// For each branch, finds the number of branches topologically between itself and the root, including itself but not the root.
        /// The root has level 0.
        /// </summary>
        /// <param name="destinationArray">Array to put branch levels into.</param>
        public void CopyBranchLevelsTo(int[] destinationArray)
        {
            for (int i = 0; i < branches.Count; i++)
            {
                if (Branches[i].ParentIndex == -1)
                    destinationArray[i] = 0;
                else
                    destinationArray[i] = 1 + destinationArray[Branches[i].ParentIndex];
            }
        }

        /// <summary>
        /// Returns the number of children a given branch has.
        /// </summary>
        /// <param name="index">Index of a branch.</param>
        /// <returns>The number of children the given branch has.</returns>
        public int GetNumberOfBranchChildren(int index)
        {
            int count = 0;
            for (int i = index + 1; i < Branches.Count; i++)
            {
                if (Branches[i].ParentIndex == index)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Calculates the absolute transform of each bone, using the specified bone rotations. The results are put into the specified matrix array.
        /// </summary>
        /// <param name="destinationArray">Array to store the bone transforms in. Length must be at least the number of bones in the tree.</param>
        /// <param name="boneRotations">Rotations of each bone. Length must be at least the number of bones in the tree.</param>
        /// <seealso cref="TreeAnimationState"/>
        public void CopyAbsoluteBoneTranformsTo(Matrix[] destinationArray, Quaternion[] boneRotations)
        {
            if (destinationArray.Length < bones.Count)
                throw new ArgumentException("Destination array is too small.");

            if (boneRotations.Length < bones.Count)
                throw new ArgumentException("Rotations array is too small to be a proper animation state.");

            for (int i = 0; i < bones.Count; i++)
            {
                destinationArray[i] = Matrix.CreateFromQuaternion(boneRotations[i]);
                if (bones[i].ParentIndex != -1)
                {
                    destinationArray[i].M42 = bones[bones[i].ParentIndex].Length;
                    destinationArray[i] = destinationArray[i] * destinationArray[bones[i].ParentIndex];
                }
            }
        }

        /// <summary>
        /// Calculates the binding matrix for each bone, using the specified rotations. The results are put into the specified matrix array.
        /// </summary>
        /// <param name="destinationArray">Array to store the bone transforms in. Length must be at least the number of bones in the tree.</param>
        /// <param name="boneRotations">Rotations of each bone. Length must be at least the number of bones in the tree.</param>
        /// <remarks>
        /// A binding matrix is the bone's inverse absolute reference transform multiplied by its absolute transform using the current rotation.
        /// It is used in hardware skinning when the vertices are in object space rather than bone space.
        /// </remarks>
        /// <seealso cref="TreeAnimationState"/>
        public void CopyBoneBindingMatricesTo(Matrix[] destinationArray, Quaternion[] boneRotations)
        {
            CopyAbsoluteBoneTranformsTo(destinationArray, boneRotations);

            for (int i = 0; i < bones.Count; i++)
            {
                destinationArray[i] = bones[i].InverseReferenceTransform * destinationArray[i];
            }
        }
    }
}
