// Written by Asger Feldthaus
// Septemper 2007

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Feldthaus.Xna
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
        private Vector3 target = Vector3.Zero;
        private Vector3 position = new Vector3(0, 0, 10);
        private Vector3 up = Vector3.Up;
        private float fieldOfView = MathHelper.ToRadians(45.0f);
        private float aspectRatio = 4.0f / 3.0f;
        private float nearZ = 1.0f;
        private float farZ = 100000.0f;
        private bool obsoleteView = true;
        private bool obsoleteProjection = true;
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
        /// Distance to the most distance visible point from the camera. Points further away than this will be invisible.
        /// The default value i 100000.0f (one hundred thousand units) which is enough for a general-purpose camera.
        /// If you experience zbuffer-fighting (seen as flickering surfaces) consider decreasing the FarZ value, as the
        /// depth buffe's accuracy decreases the greater the FarZ value is.
        /// </summary>
        public float FarZ
        {
            get { return farZ; }
            set { farZ = value; obsoleteProjection = true; }
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
        #endregion
    }
}
