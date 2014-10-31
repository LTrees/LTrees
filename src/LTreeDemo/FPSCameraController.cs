using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace LTreeDemo
{
    public class FPSCameraController
    {
        private float heading = 0.0f;
        private float pitch = 0.0f;
        private Camera camera;
        private KeyboardState keystate;
        private KeyboardState lastKeystate;
        private MouseState mouse;
        private GraphicsDevice device;

        private float cameraMoveSpeed = 10000.0f;
        private float cameraMoveSpeedSlowed = 500.0f;
        private float cameraTurnSpeed = MathHelper.PiOver2;
        private float cameraPitchSpeed = MathHelper.PiOver2;

        /// <summary>
        /// Radians to pitch when the mouse has moved one whole screen's height.
        /// Default is 2 Pi.
        /// </summary>
        public float CameraPitchSpeed
        {
            get { return cameraPitchSpeed; }
            set { cameraPitchSpeed = value; }
        }


        /// <summary>
        /// Radians to turn when the mouse has moved one whole screen's width.
        /// Default is 2 Pi.
        /// </summary>
        public float CameraTurnSpeed
        {
            get { return cameraTurnSpeed; }
            set { cameraTurnSpeed = value; }
        }

        /// <summary>
        /// Moving speed, in units per second. Default is 10000.0f (ten thousand).
        /// </summary>
        public float CameraMoveSpeed
        {
            get { return cameraMoveSpeed; }
            set { cameraMoveSpeed = value; }
        }

        /// <summary>
        /// Moving speed while holding SHIFT, in units per second. Default is 500.0f (five hundred).
        /// </summary>
        public float CameraMoveSpeedSlowed
        {
            get { return cameraMoveSpeedSlowed; }
            set { cameraMoveSpeedSlowed = value; }
        }

        /// <summary>
        /// Gets or sets the controlled camera.
        /// </summary>
        public Camera Camera
        {
            get { return camera; }
            set { camera = value; }
        }

        public FPSCameraController(Camera camera, GraphicsDevice device)
        {
            this.camera = camera;
            this.device = device;

            // Calculate heading and pitch from camera's current position and target
            Vector3 delta = camera.Target - camera.Position;
            
            this.heading = (float)Math.Atan2(delta.Z, delta.X);
            this.pitch = (float)Math.Atan2(delta.Y, Math.Sqrt(delta.X * delta.X + delta.Z * delta.Z)); 
        }

        public void Update(GameTime time)
        {
            if (camera == null)
                return;

            lastKeystate = keystate;
            keystate = Keyboard.GetState();

            int width = device.Viewport.Width;
            int height = device.Viewport.Height;

            mouse = Mouse.GetState();
            Mouse.SetPosition(width / 2, height / 2);
            float mx = (mouse.X - width / 2) / (float)width;
            float my = (mouse.Y - height / 2) / (float)height;

            float movespeed = keystate.IsKeyDown(Keys.LeftShift) ? cameraMoveSpeedSlowed : cameraMoveSpeed;

            if (keystate.IsKeyDown(Keys.W))
                camera.PanTo(camera.Position + camera.ForwardDir * movespeed * time.ElapsedGameTime.Milliseconds / 1000.0f);
            if (keystate.IsKeyDown(Keys.S))
                camera.PanTo(camera.Position - camera.ForwardDir * movespeed * time.ElapsedGameTime.Milliseconds / 1000.0f);
            if (keystate.IsKeyDown(Keys.A))
                camera.PanTo(camera.Position + camera.LeftDir * movespeed * time.ElapsedGameTime.Milliseconds / 1000.0f);
            if (keystate.IsKeyDown(Keys.D))
                camera.PanTo(camera.Position + camera.RightDir * movespeed * time.ElapsedGameTime.Milliseconds / 1000.0f);

            heading += mx * cameraTurnSpeed;
            pitch -= my * cameraPitchSpeed;

            if (pitch < -MathHelper.Pi / 2 + 0.0001f)
                pitch = -MathHelper.Pi / 2 + 0.0001f;
            if (pitch > MathHelper.Pi / 2 - 0.0001f)
                pitch = MathHelper.Pi / 2 - 0.0001f;

            camera.SetFirstPersonView(heading, pitch);
        }
    }
}
