/* 
 * Copyright (c) 2007 Asger Feldthaus
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
 * and associated documentation files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:  
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
 * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Feldthaus.Xna
{
    /// <summary>
    /// Contains all the particles and settings of a particle cloud. A particle cloud system is required to create a particle cloud.
    /// The particles will be invisible until you specify a texture.
    /// </summary>
    /// <remarks>
    /// To add particles to the cloud, use AddParticle or set the NumberOfParticles property manually. The properties of the particles
    /// can be modified through the properties: Positions, Colors, and RollAngles. This class does not simulate any kind of particle
    /// movement at all, and is not designed for such use.
    /// </remarks>
    /// <seealso cref="ParticleCloudSystem"/>
    public sealed class ParticleCloud : IDisposable
    {
        #region Fields
        private const int InitialArrayLength = 8;
        private Vector3[] particlePositions = new Vector3[InitialArrayLength];
        private Vector4[] particleColors = new Vector4[InitialArrayLength];
        private Vector4[] particleOrientations = new Vector4[InitialArrayLength];
        private int numParticles = 0;
        private ParticleCloudSystem system;
        private Vector3 axis = Vector3.Up;
        private bool hasAxis = false;
        private Texture texture;
        private bool sortingEnabled;
        #endregion

        #region Properties
        /// <summary>
        /// Positions of all the particles, local to the particle cloud. Note that the length of the array may be greater than the actual number of particles.
        /// To add more particles, call AddParticle, or set NumberOfParticles to any number you like.
        /// </summary>
        public Vector3[] Positions
        {
            get { return particlePositions; }
        }

        /// <summary>
        /// Colors of all the particles. Note that the length of the array may be greater than the actual number of particles.
        /// To add more particles, call AddParticle, or set NumberOfParticles to any number you like.
        /// </summary>
        public Vector4[] Colors
        {
            get { return particleColors; }
        }

        /// <summary>
        /// The orientation vector is a bit tricky, because it is actually composed to two 2D-vectors.
        /// It specifies both the size and orientation of a particle.
        /// The XY part makes up the particle's right-vector, and the ZW part makes up its up-vector.
        /// </summary>
        public Vector4[] Orientations
        {
            get { return particleOrientations; }
        }

        /// <summary>
        /// Gets or sets the number of particles in the cloud.
        /// </summary>
        public int NumberOfParticles
        {
            get { return numParticles; }
            set { numParticles = value; ResizeArrays(numParticles); }
        }

        /// <summary>
        /// Gets or sets the billboard rotation axis used by the particles in the cloud.
        /// AxisEnabled must be set to true to enable axis-aligned particles.
        /// </summary>
        public Vector3 Axis
        {
            get { return axis; }
            set { axis = value; }
        }

        /// <summary>
        /// Gets or sets whether the billboards should be restricted to a rotation axis, or rotate
        /// as free billboards. Default is false.
        /// </summary>
        public bool AxisEnabled
        {
            get { return hasAxis; }
            set { hasAxis = value; }
        }

        /// <summary>
        /// Gets or sets the texture applied to the particles of the cloud. A texture must be specified
        /// or the particles will be invisible.
        /// </summary>
        public Texture Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        /// <summary>
        /// Gets or sets whether the particles should be sorted before rendering. Enabling sorting increases the
        /// visual quality of the particles, but requires more CPU time.
        /// </summary>
        /// <remarks>
        /// If you want semi-transparent leaves, you have to enable sorting. Otherwise, each pixel will either be completely
        /// transparent or opaque.
        /// The depth buffer is not written to when rendering sorted particles; therefore, you should draw anything that is 
        /// behind the cloud, before drawing the cloud itself. Drawing solid geometry first, and then drawing transparent geometry
        /// ordered by furthest-to-nearest is a good way to resolve this issue.
        /// </remarks>
        public bool SortingEnabled
        {
            get { return sortingEnabled; }
            set { sortingEnabled = value; }
        }
        #endregion
        
        /// <summary>
        /// Creates an empty particle cloud.
        /// </summary>
        /// <param name="system">The particle cloud system used to draw the particles. You only need one system for all your clouds.</param>
        public ParticleCloud(ParticleCloudSystem system)
        {
            Debug.Assert(system != null, "A null ParticleCloudSystem was passed to a ParticleCloud. It must be non-null.");
            this.system = system;
        }

        #region Drawing
        private void InvertTransform(ref Matrix mat)
        {
            // Invert the scale
            float sx = 1.0f / new Vector3(mat.M11, mat.M21, mat.M31).LengthSquared();
            float sy = 1.0f / new Vector3(mat.M12, mat.M22, mat.M32).LengthSquared();
            float sz = 1.0f / new Vector3(mat.M13, mat.M23, mat.M33).LengthSquared();

            mat.M11 *= sx;
            mat.M21 *= sx;
            mat.M31 *= sx;
            mat.M12 *= sy;
            mat.M22 *= sy;
            mat.M32 *= sy;
            mat.M13 *= sz;
            mat.M23 *= sz;
            mat.M33 *= sz;

            // Invert the rotation
            Swap(ref mat.M12, ref mat.M21);
            Swap(ref mat.M13, ref mat.M31);
            Swap(ref mat.M23, ref mat.M32);

            // Invert the translation
            Vector3 translation = -mat.Translation;
            Vector3.Transform(ref translation, ref mat, out translation);
            mat.Translation = translation;
        }

        /// <summary>
        /// Draws the particle cloud. If sorting is enabled, this is where the particles get sorted.
        /// Make sure the ParticleCloudSystem is properly initialized before calling this.
        /// </summary>
        /// <param name="world">The world matrix.</param>
        /// <param name="view">The view matrix.</param>
        /// <param name="cameraPositionWorld">The camera's position in world space. This is used to sort the particles, if sorting is enabled.</param>
        public void Draw(Matrix world, Matrix view, Vector3 cameraPositionWorld)
        {
            if (sortingEnabled)
            {
                Matrix worldInv = world;
                Matrix.Invert(ref worldInv, out worldInv);
                Vector3 look = -new Vector3(view.M13, view.M23, view.M33);

                Vector3 align;
                if (hasAxis)
                {
                    Vector3 right = Vector3.Cross(look, axis);
                    align = -Vector3.Cross(right, axis);
                }
                else
                {
                    align = look;
                }

                SortParticles(Vector3.Transform(cameraPositionWorld, worldInv), Vector3.TransformNormal(align, worldInv));
            }
            system.DrawParticleCloud(this, world, view);
        }
        #endregion

        #region Particle Sorting
        private static void Swap<T>(ref T a, ref T b)
        {
            T c = a;
            a = b;
            b = c;
        }

        // NOTE: QuickSort is not currently used since it causes flickering (due to rounding errors)
        private void QSortParticles(ref Vector3 cameraPosition, ref Vector3 align, int low, int high)
        {
            if (low >= high)
                return;
            
            int mid = (low + high) >> 1;

            float midDist = Vector3.Dot(particlePositions[mid] - cameraPosition, align);
            float dist;

            int i = low;
            int j = high;
            while (i < j)
            {
                // Move 'i' to the right until we find a particle to swap
                while (i < j)
                {
                    dist = Vector3.Dot(particlePositions[i] - cameraPosition, align);
                    if (dist <= midDist)
                    {
                        break; // This particle needs to be swapped
                    }
                    i++;
                }

                // Move 'j' to the left until we find a particle to swap
                while (i < j)
                {
                    dist = Vector3.Dot(particlePositions[j] - cameraPosition, align);
                    if (dist >= midDist)
                    {
                        break;
                    }
                    j--;
                }
                
                // Did we find two particles to swap?
                if (i < j)
                {
                    Swap(ref particlePositions[i], ref particlePositions[j]);
                    Swap(ref particleColors[i], ref particleColors[j]);
                    Swap(ref particleOrientations[i], ref particleOrientations[j]);

                    i++;
                    j--;
                }
            }

            QSortParticles(ref cameraPosition, ref align, low, i - 1);
            QSortParticles(ref cameraPosition, ref align, i + 1, high);
        }

        private void BubbleSortParticles(ref Vector3 cameraPosition, ref Vector3 align)
        {
            bool moved = false;
            do
            {
                moved = false;
                float dist = Vector3.Dot(particlePositions[0] - cameraPosition, align);

                for (int i = 0; i < numParticles - 1; i++)
                {
                    float otherDist = Vector3.Dot(particlePositions[i+1] - cameraPosition, align);
                    if (dist < otherDist)
                    {
                        Swap(ref particlePositions[i], ref particlePositions[i + 1]);
                        Swap(ref particleColors[i], ref particleColors[i + 1]);
                        Swap(ref particleOrientations[i], ref particleOrientations[i + 1]);
                        moved = true;
                    }
                    else
                    {
                        dist = otherDist;
                    }
                }
            }
            while (moved);
        }

        /// <summary>
        /// Sorts the particles based on their distance from the camera's far plane. Note that the camera's position
        /// must be given relative to the cloud's position.
        /// </summary>
        /// <remarks>
        /// Distant particles will appear first in the resulting arrays.
        /// Note that this is a slow function; use it sparingly for large particle clouds.
        /// This method is automatically called by the Draw method if SortingEnabled is set to true.
        /// </remarks>
        /// <param name="cameraPosition">Position of the camera, in local cloud space.</param>
        /// <param name="align">Complicated stuff...</param>
        private void SortParticles(Vector3 cameraPosition, Vector3 align)
        {
            // Use Bubble Sort to avoid flickering. Most of the time the particles are already sorted,
            // so the average running time will not be that bad, even.
            BubbleSortParticles(ref cameraPosition, ref align);

            // QuickSort is not used due to flickering. Uncomment the line to try it.
            //// QSortParticles(ref cameraPosition, ref align, 0, numParticles - 1);
        }
        #endregion

        #region Particle Add/Removal
        /// <summary>
        /// Adds a particle to the cloud. The particle's twist angle is set to 0, and its size is set to 1x1.
        /// </summary>
        /// <param name="position">Position of the particle, relative to the cloud.</param>
        /// <param name="color">Color of the particle. XYZ are the RGB parts, and W is the Alpha part.</param>
        public void AddParticle(Vector3 position, Vector4 color)
        {
            AddParticle(position, color, 0f, new Vector2(1, 1));
        }

        /// <summary>
        /// Adds a particle to the cloud. The particle's twist angle is set to 0, and its size is set to 1x1.
        /// </summary>
        /// <param name="position">Position of the particle, relative to the cloud.</param>
        /// <param name="color">Color of the particle.</param>
        public void AddParticle(Vector3 position, Color color)
        {
            AddParticle(position, color.ToVector4(), 0f, new Vector2(1, 1));
        }

        /// <summary>
        /// Adds a particle to the cloud.
        /// </summary>
        /// <param name="position">Position of the particle, relative to the cloud.</param>
        /// <param name="color">Color of the particle.</param>
        /// <param name="twistRadians">Twisting angle of the particle, indicating how it is rotated relative to the screen, in radians.</param>
        /// <param name="size">Size of the particle. X is the width, and Y is the height.</param>
        public void AddParticle(Vector3 position, Color color, float twistRadians, Vector2 size)
        {
            AddParticle(position, color.ToVector4(), twistRadians, size);
        }

        /// <summary>
        /// Adds a particle to the cloud.
        /// </summary>
        /// <param name="position">Position of the particle, relative to the cloud.</param>
        /// <param name="color">Color of the particle. XYZ are the RGB parts, and W is the Alpha part.</param>
        /// <param name="twistRadians">Twisting angle of the particle, indicating how it is rotated relative to the screen, in radians.</param>
        /// <param name="size">Size of the particle. X is the width, and Y is the height.</param>
        public void AddParticle(Vector3 position, Vector4 color, float twistRadians, Vector2 size)
        {
            // Increase number of particles.
            numParticles++;

            // Make sure we have enough space for the extra particle.
            ResizeArrays(numParticles);

            int index = numParticles - 1;

            float cosTwist = (float)Math.Cos(twistRadians);
            float sinTwist = (float)Math.Sin(twistRadians);

            size *= 0.5f;

            particlePositions[index] = position;
            particleColors[index] = color;
            particleOrientations[index] = new Vector4(size.X * cosTwist, size.Y * sinTwist, -size.X * sinTwist, size.Y * cosTwist);
        }

        /// <summary>
        /// Removes a particle from the cloud.
        /// </summary>
        /// <param name="index">Index of the particle to remove. Must be at least zero, and must be less than the number of particles in the cloud.</param>
        public void KillParticle(int index)
        {
            // TODO: Test KillParticle.
            // Verify that the index is valid
            if (index >= numParticles)
                throw new ArgumentOutOfRangeException("Particle " + index + " can not be killed because it does not exist. There are only " + numParticles + " particles in the cloud.");
            if (index < 0)
                throw new ArgumentOutOfRangeException("Particle index must not be negative (" + index + ")");

            // Decrease the particle counter
            numParticles--;

            // Move the last particle down to this position, to keep the array free of holes.
            if (index != numParticles)
            {
                particlePositions[index] = particlePositions[numParticles];
                particleColors[index] = particleColors[numParticles];
                particleOrientations[index] = particleOrientations[numParticles];
            }
        }

        private void ResizeArrays(int requiredSize)
        {
            // Get the current size of the arrays.
            // Just use the positions as reference, since they all must have equal length.
            int currentSize = particlePositions.Length;

            // If we already have enough space, just quit here.
            if (requiredSize <= currentSize)
                return;

            // Find the smallest power of two we can use to satisfy the space requirements.
            while (requiredSize > currentSize)
            {
                currentSize = currentSize * 2;
            }

            // Resize the arrays to the new size
            Array.Resize<Vector3>(ref particlePositions, currentSize);
            Array.Resize<Vector4>(ref particleColors, currentSize);
            Array.Resize<Vector4>(ref particleOrientations, currentSize);
        }
        #endregion

        #region Testing
        /// <summary>
        /// Unit-test for loosely verifying the InvertTransform method.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="view"></param>
        /// <param name="cameraPositionWorld"></param>
        public void TestInverseMatrix(Matrix world, Matrix view, Vector3 cameraPositionWorld)
        {
            Matrix inv = world;
            InvertTransform(ref inv);
            Vector3 v = cameraPositionWorld;
            Vector3.Transform(ref v, ref inv, out v);
            Vector3.Transform(ref v, ref inv, out v);
            float f = Vector3.Distance(v, cameraPositionWorld);
            Console.WriteLine("Distance after transform = " + f);

            v = view.Translation;
            f = Vector3.Distance(v, cameraPositionWorld);
            Console.WriteLine("Distance between cameras = " + f);
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// Removes the arrays so the garbage collector may recycle the memory. This is the only way to
        /// eliminate the memory used by the particle cloud. Number of particles is also reduced to 0.
        /// </summary>
        public void FreeMem()
        {
            numParticles = 0;
            particlePositions = new Vector3[1];
            particleColors = new Vector4[1];
            particleOrientations = new Vector4[1];
        }

        /// <summary>
        /// Calls FreeMem().
        /// </summary>
        public void Dispose()
        {
            FreeMem();
        }
        #endregion
    }
}
