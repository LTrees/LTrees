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

namespace LTreeDemo
{
    /// <summary>
    /// Perspective camera using a position, target, and up-vector to calculate its view matrix.
    /// </summary>
    /// <remarks>
    /// The View and Projection properties are rebuilt on a when-needed basis when one of the other properties they
    /// depend on have been changed. This is very convenient, but in a multithreaded environment you should to call
    /// CalculateAll() before entering a section with multiple threads accessing the camera. Otherwise, several threads
    /// might try to write to the projection or view matrix, even though you thought you were only reading it.
    /// </remarks>
    public class Camera
    {
        #region Fields
        private Matrix view;
        private Matrix projection;
        private BoundingFrustum boundingFrustum = new BoundingFrustum(Matrix.Identity);
        private Vector3 target = Vector3.Zero;
        private Vector3 position = new Vector3(0, 0, 10);
        private Vector3 up = Vector3.Up;
        private float fieldOfView = MathHelper.ToRadians(45.0f);
        private float aspectRatio = 4.0f / 3.0f;
        private float nearZ = 1.0f;
        private float farZ = 100000.0f;
        private bool obsoleteView = true;
        private bool obsoleteProjection = true;
        private bool obsoleteBoundingFrustum = true;

        private const float DefaultDistance = 100.0f;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the camera's view matrix. It will be recalculated if one of the following properties has changed:
        /// Position, Target, or Up.
        /// The view matrix transforms points from world space into view space. In view space, the X axis points
        /// to the right of the camera, Y points upwards, and Z is backwards into the screen.
        /// It is generally unwanted to use the standard properties of the Matrix class on a View matrix, because it
        /// is actually the inverse transformation of the camera. If for instance the camera's forward vector is wanted,
        /// using Matrix.Forward will not give the desired value. Instead, [-View.M13, -View.M23, -View.M33] will be the correct
        /// forward vector.
        /// </summary>
        public Matrix View
        {
            get
            {
                if (obsoleteView)
                    UpdateViewMatrix();
                return view;
            }
        }

        /// <summary>
        /// Gets the camera's projection matrix. It will be recalculated if one of the following properties has changed:
        /// FieldOfView, AspectRatio, NearZ, or FarZ.
        /// The projection matrix transforms points from view space into clip space (ie. screen coordinates).
        /// </summary>
        public Matrix Projection
        {
            get
            {
                if (obsoleteProjection)
                    UpdateProjectionMatrix();
                return projection;
            }
        }

        /// <summary>
        /// Gets of sets the target point viewed by the camera. The camera will always orientate itself to look directly towards
        /// this point.
        /// </summary>
        public Vector3 Target
        {
            get { return target; }
            set { target = value; obsoleteView = true; }
        }

        /// <summary>
        /// Gets of sets the position of the camera.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; obsoleteView = true; }
        }

        /// <summary>
        /// Gets or sets the camera's up vector, which determines which direction should be considered "upwards" by the camera.
        /// In most cases, you want to leave this as [0,1,0] (its default), but if you want the camera to roll around you have
        /// to change it.
        /// If you want the Z-axis to be the up-vector you can set this to [0,0,1].
        /// </summary>
        public Vector3 Up
        {
            get { return up; }
            set { up = value; obsoleteView = true; }
        }

        /// <summary>
        /// Gets or sets the camera's field of view, in radians. This is the angle between the leftmost visible point, and the rightmost visible
        /// point. A value of PI/4 radians (~45 degrees) is the default, and is typically a good value.
        /// You can decrease it to simulate a telescope-effect.
        /// </summary>
        public float FieldOfView
        {
            get { return fieldOfView; }
            set { fieldOfView = value; obsoleteProjection = true; }
        }

        /// <summary>
        /// Gets or sets the aspect ratio used by the camera's projection matrix. This should always match the aspect ratio of the viewport.
        /// The aspect ratio is defined as Viewport.Width / Viewport.Height. You can get the Viewport's size using the GraphicsDevice.
        /// The default value is 4.0f / 3.0f, but you probably want to set it manually to be sure it is correct.
        /// </summary>
        public float AspectRatio
        {
            get { return aspectRatio; }
            set { aspectRatio = value; obsoleteProjection = true; }
        }

        /// <summary>
        /// Distance to the closest visible point from the camera. Points closer than this will be invisible. The default
        /// value is 1.0f, which usually is small enough. The value must be strictly greater than zero.
        /// Do not confuse this with a clipping plane; changing this between render calls will mess up the depth buffer and
        /// cause unwanted behaviour.
        /// </summary>
        public float NearZ
        {
            get { return nearZ; }
            set { nearZ = value; obsoleteProjection = true; }
        }

        /// <summary>
        /// Distance to the most distant visible point from the camera. Points further away than this will be invisible.
        /// The default value is 100000.0f (one hundred thousand units) which is enough for a general-purpose camera.
        /// If you experience zbuffer-fighting (seen as flickering surfaces) consider decreasing the FarZ value, as the
        /// depth buffe's accuracy decreases the greater the FarZ value is.
        /// </summary>
        public float FarZ
        {
            get { return farZ; }
            set { farZ = value; obsoleteProjection = true; }
        }

        /// <summary>
        /// Returns a reference to the bounding frustum of the camera. Will be recalculated if needed.
        /// The bounding frustum defines the area that is visible to the camera, and is useful for culling.
        /// </summary>
        /// <remarks>
        /// Please note that this is a <i>reference</i> to the frustum -- you should clone it if you need to store
        /// it for later use.
        /// </remarks>
        public BoundingFrustum BoundingFrustum
        {
            get
            {
                if (obsoleteBoundingFrustum || obsoleteProjection || obsoleteView)
                    UpdateBoundingFrustum();
                return boundingFrustum;
            }
        }

        /// <summary>
        /// Gets a vector pointing in the direction the camera is looking.
        /// </summary>
        public Vector3 ForwardDir
        {
            get
            {
                if (obsoleteView)
                    UpdateViewMatrix();
                return new Vector3(-view.M13, -view.M23, -view.M33);
            }
        }

        /// <summary>
        /// Gets a vector pointing to the left of the camera.
        /// </summary>
        public Vector3 LeftDir
        {
            get
            {
                if (obsoleteView)
                    UpdateViewMatrix();
                return new Vector3(-view.M11, -view.M21, -view.M31);
            }
        }

        /// <summary>
        /// Gets a vector pointing to the right of the camera.
        /// </summary>
        public Vector3 RightDir
        {
            get
            {
                return -LeftDir;
            }
        }

        #endregion
        
        /// <summary>
        /// Creates a new camera at [0,0,10] looking towards [0,0,0].
        /// </summary>
        public Camera()
        {
        }

        /// <summary>
        /// Creates a new camera.
        /// </summary>
        /// <param name="position">Position of the camera.</param>
        /// <param name="target">Target of the camera.</param>
        public Camera(Vector3 position, Vector3 target)
        {
            this.position = position;
            this.target = target;
        }

        /// <summary>
        /// Calculates the view and projection matrix, if needed.
        /// </summary>
        /// <remarks>
        /// The matrices are automatically calculated (if needed) when View or Projection is accessed, so in single-threaded 
        /// program this method does not need to be called. In a multithreaded program, you should call this before entering 
        /// a section where multiple threads might read the camera's projection or view matrix, to prevent several threads from
        /// writing to the cached values simultaneously.
        /// </remarks>
        public void CalculateAll()
        {
            if (obsoleteView)
                UpdateViewMatrix();
            if (obsoleteProjection)
                UpdateProjectionMatrix();
        }
        
        #region Helpful methods
        /// <summary>
        /// Changes the camera's target and position to visualize a third person's perspective of a point, using specified
        /// direction and angle-of-attack.
        /// </summary>
        /// <remarks>
        /// Note that this function assumes that +Y is your up-vector (as by default). If you use another up-vector this will
        /// probably give unexpected results.
        /// </remarks>
        /// <param name="target">The new camera target.</param>
        /// <param name="orbitRadians">Orbital angle, in radians. 0 will place the camera to the "east" (+X) of the point.
        ///     PI/2 will place it to the "south" (+Z), PI will place it to the "west" (-X) and so on.</param>
        /// <param name="pitchRadians">Pitch angle, in radians. 0 will give a flat horizontal perspective, while PI/2 gives a
        ///     complete top-down perspective of the target.</param>
        /// <param name="distanceToTarget">Desired distance between the camera and the target.</param>
        public void SetThirdPersonView(Vector3 target, float orbitRadians, float pitchRadians, float distanceToTarget)
        {
            this.target = target;
            SetThirdPersonView(orbitRadians, pitchRadians, distanceToTarget);
        }

        /// <summary>
        /// Changes the camera's position to visualize a third person's perspective of a point, using specified
        /// direction and angle-of-attack. The camera will keep its existing target.
        /// Note that this function assumes that +Y is your up-vector (as by default). If you use another up-vector this will
        /// probably give unexpected results.
        /// </summary>
        /// <param name="orbitRadians">Orbital angle, in radians. 0 will place the camera to the "east" (+X) of the point.
        ///     PI/2 will place it to the "south" (+Z), PI will place it to the "west" (-X) and so on.</param>
        /// <param name="pitchRadians">Pitch angle, in radians. 0 will give a flat horizontal perspective, while PI/2 gives a
        ///     complete top-down perspective of the target.</param>
        /// <param name="distanceToTarget">Desired distance between the camera and the target.</param>
        public void SetThirdPersonView(float orbitRadians, float pitchRadians, float distanceToTarget)
        {
            float co = (float)Math.Cos(orbitRadians);
            float so = (float)Math.Sin(orbitRadians);
            float cp = (float)Math.Cos(pitchRadians);
            float sp = (float)Math.Sin(pitchRadians);

            position = target + distanceToTarget * new Vector3(
                co * cp,
                sp,
                so * cp);

            obsoleteView = true;
        }

        /// <summary>
        /// Changes the camera target to fit a first person's view.
        /// </summary>
        /// <param name="facingRadians">Direction of the viewer, 0 being east (+X), Pi/2 being north (-Z), and so on.</param>
        /// <param name="pitchRadians">Up/down viewing angle. 0 is horizontal (flat), Pi/2 is straight upwards, and -Pi/2 is straight downwards.</param>
        public void SetFirstPersonView(float facingRadians, float pitchRadians)
        {
            SetFirstPersonView(facingRadians, pitchRadians, DefaultDistance);
        }

        /// <summary>
        /// Changes the camera target to fit a first person's view.
        /// </summary>
        /// <param name="facingRadians">Direction of the viewer, 0 being east (+X), Pi/2 being north (-Z), and so on.</param>
        /// <param name="pitchRadians">Up/down viewing angle. 0 is horizontal (flat), Pi/2 is straight upwards, and -Pi/2 is straight downwards.</param>
        /// <param name="distanceToTarget">Distance to the target point. This does not affect the view, but the Target property will be affected by it.</param>
        public void SetFirstPersonView(float facingRadians, float pitchRadians, float distanceToTarget)
        {
            float cf = (float)Math.Cos(facingRadians);
            float sf = (float)Math.Sin(facingRadians);
            float cp = (float)Math.Cos(pitchRadians);
            float sp = (float)Math.Sin(pitchRadians);

            target = position + distanceToTarget * new Vector3(
                cf * cp,
                sp,
                sf * cp);

            obsoleteView = true;
        }

        /// <summary>
        /// Moves the camera position and target to a new location, maintaining the original viewing angle.
        /// </summary>
        /// <param name="newPosition">New position, in world space.</param>
        public void PanTo(Vector3 newPosition)
        {
            Vector3 delta = newPosition - position;
            position = newPosition;
            target += delta;
            obsoleteView = true;
        }
        #endregion

        #region Private updates
        private void UpdateViewMatrix()
        {
            view = Matrix.CreateLookAt(position, target, up);
            obsoleteView = false;
        }

        private void UpdateProjectionMatrix()
        {
            projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearZ, farZ);
            obsoleteProjection = false;
        }

        private void UpdateBoundingFrustum()
        {
            if (obsoleteView)
                UpdateViewMatrix();
            if (obsoleteProjection)
                UpdateProjectionMatrix();
            boundingFrustum.Matrix = view * projection;
            obsoleteBoundingFrustum = false;
        }
        #endregion
    }
}
