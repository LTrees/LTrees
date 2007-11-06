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

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Feldthaus.Xna;
#endregion

namespace TreeDemo
{
    /// <summary>
    /// Program that demonstrates how to use the TreeGenerator class. It is also useful for viewing trees when designing
    /// tree profiles.
    /// </summary>
    public class TreeDemo : Microsoft.Xna.Framework.Game
    {
        struct TreeFile
        {
            public string Name;
            public string Profile;
            public string BarkTexture;
            public string LeafTexture;

            public TreeFile(string name, string profile, string bark, string leaf)
            {
                Name = name;
                Profile = profile;
                BarkTexture = bark;
                LeafTexture = leaf;
            }
        }

        static readonly TreeFile[] treeFiles =
            {
                new TreeFile("Oak", "Oak.xml", "OakBark", "OakLeaf"),
                new TreeFile("Willow", "Willow.xml", "WillowBark", "WillowLeaf"),
                new TreeFile("Aspen", "Aspen.xml", "AspenBark", "AspenLeaf"),
                new TreeFile("Pine", "Pine.xml", "PineBark", "PineLeaf")
            };
        
        const string treePath = "Content/Trees/";
        const string texturePath = "Content/Textures/";

        GraphicsDeviceManager graphics;
        ContentManager content;
        ParticleCloudSystem cloudSystem;
        Camera camera = new Camera();
        Matrix treeTransform = Matrix.Identity;

        TreeGenerator[] treeGenerators;
        TreeModel tree;

        int currentTreeType = 0;

        float pitch = MathHelper.ToRadians(30f);
        float orbit = 0f;
        float cameraDistance = 500f;

        KeyboardState keyboardState = new KeyboardState();
        KeyboardState lastKeyboardState = new KeyboardState();

        Random random = new Random();
        int seed = 1234;

        SpriteBatch textBatch;
        SpriteFont font;

        int numFramesRendered;
        int timeFrames;
        int currentFps;
        
        public TreeDemo()
        {
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(Services);
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            camera.SetThirdPersonView(new Vector3(0, 120, 0), MathHelper.ToRadians(45f), MathHelper.ToRadians(60f), 500f);

            base.Initialize();
        }

        private void GenerateTree()
        {
            bool sorting = tree == null? true : tree.Leaves.SortingEnabled;

            // Generate a tree.
            tree = treeGenerators[currentTreeType].GenerateTreeMesh(seed, 12, true, 0);

            // Enable/disable leaf sorting
            tree.Leaves.SortingEnabled = sorting;

            // Set the trunk's projection matrix
            tree.Trunk.Projection = camera.Projection;
        }

        /// <summary>
        /// Load your graphics content.  If loadAllContent is true, you should
        /// load content from both ResourceManagementMode pools.  Otherwise, just
        /// load ResourceManagementMode.Manual content.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                // Initialize the particle cloud system.
                cloudSystem = new ParticleCloudSystem(graphics.GraphicsDevice, content, "Content/Shaders/ParticleCloud");
                cloudSystem.Initialize();

                treeGenerators = new TreeGenerator[treeFiles.Length];

                // Create the tree generators.
                for (int i = 0; i < treeGenerators.Length; i++)
                {
                    treeGenerators[i] = new TreeGenerator(graphics.GraphicsDevice, cloudSystem);
                    treeGenerators[i].LoadFromFile(treePath + treeFiles[i].Profile);

                    // Set the texture assigned to newly generated trees.
                    treeGenerators[i].TrunkEffect.Texture = content.Load<Texture2D>(texturePath + treeFiles[i].BarkTexture);
                    treeGenerators[i].TrunkEffect.TextureEnabled = true;
                    treeGenerators[i].TrunkEffect.EnableDefaultLighting();
                    treeGenerators[i].LeafTexture = content.Load<Texture>(texturePath + treeFiles[i].LeafTexture);
                }

                GenerateTree();

                // Load fonts
                textBatch = new SpriteBatch(graphics.GraphicsDevice);
                font = content.Load<SpriteFont>("Content/Fonts/Font1");
            }

            // Calculate the aspect ratio
            camera.AspectRatio = graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height;

            // Give the projection matrix to the particle cloud system
            cloudSystem.Projection = camera.Projection;

            // Give the projection matrix to the tree's trunk
            tree.Trunk.Projection = camera.Projection;
        }

        /// <summary>
        /// Unload your graphics content.  If unloadAllContent is true, you should
        /// unload content from both ResourceManagementMode pools.  Otherwise, just
        /// unload ResourceManagementMode.Manual content.  Manual content will get
        /// Disposed by the GraphicsDevice during a Reset.
        /// </summary>
        /// <param name="unloadAllContent">Which type of content to unload.</param>
        protected override void UnloadGraphicsContent(bool unloadAllContent)
        {
            if (unloadAllContent)
            {
                // TODO: Unload any ResourceManagementMode.Automatic content
                content.Unload();
            }

            // TODO: Unload any ResourceManagementMode.Manual content
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Remember the previous keyboard state
            lastKeyboardState = keyboardState;

            // Get the new keyboard state
            keyboardState = Keyboard.GetState();

            // Allows the game to exit
            if (keyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            // Circle the camera left
            if (keyboardState.IsKeyDown(Keys.Left))
                orbit += gameTime.ElapsedGameTime.Milliseconds * MathHelper.PiOver2 / 1000.0f;

            // Circle the camera right
            if (keyboardState.IsKeyDown(Keys.Right))
                orbit -= gameTime.ElapsedGameTime.Milliseconds * MathHelper.PiOver2 / 1000.0f;

            // Move the camera down
            if (keyboardState.IsKeyDown(Keys.Up))
                pitch = Math.Max(-MathHelper.PiOver4, pitch - gameTime.ElapsedGameTime.Milliseconds * MathHelper.PiOver2 / 1000.0f);

            // Move the camera up
            if (keyboardState.IsKeyDown(Keys.Down))
                pitch = Math.Min(MathHelper.PiOver2-0.1f, pitch + gameTime.ElapsedGameTime.Milliseconds * MathHelper.PiOver2 / 1000.0f);

            // Zoom in
            if (keyboardState.IsKeyDown(Keys.A))
                cameraDistance = Math.Max(100f, cameraDistance - gameTime.ElapsedGameTime.Milliseconds * 400f / 1000f);

            // Zoom out
            if (keyboardState.IsKeyDown(Keys.Z))
                cameraDistance = Math.Min(1000f, cameraDistance + gameTime.ElapsedGameTime.Milliseconds * 400f / 1000f);

            // Toggle leaf sorting
            if (keyboardState.IsKeyDown(Keys.S) && !lastKeyboardState.IsKeyDown(Keys.S))
                tree.Leaves.SortingEnabled = !tree.Leaves.SortingEnabled;

            // Previous tree profile
            if (keyboardState.IsKeyDown(Keys.Q) && !lastKeyboardState.IsKeyDown(Keys.Q))
            {
                currentTreeType--;
                if (currentTreeType < 0)
                    currentTreeType = treeFiles.Length - 1;
                GenerateTree();
            }

            // Next tree profile
            if (keyboardState.IsKeyDown(Keys.W) && !lastKeyboardState.IsKeyDown(Keys.W))
            {
                currentTreeType++;
                if (currentTreeType >= treeFiles.Length)
                    currentTreeType = 0;
                GenerateTree();
            }

            // New seed
            if (keyboardState.IsKeyDown(Keys.D) && !lastKeyboardState.IsKeyDown(Keys.D))
            {
                seed = random.Next();
                GenerateTree();
            }
            
            // Update camera position
            camera.SetThirdPersonView(orbit, pitch, cameraDistance);
            
            base.Update(gameTime);

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Update fps?
            numFramesRendered++;
            timeFrames += gameTime.ElapsedRealTime.Milliseconds;
            if (timeFrames >= 1000)
            {
                currentFps = (1000 * numFramesRendered) / timeFrames;
                numFramesRendered = 0;
                timeFrames = 0;
            }

            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the tree's trunk
            tree.Trunk.Draw(treeTransform, camera.View);

            // Draw the tree's leaves (this has its own effect)
            tree.Leaves.Draw(treeTransform, camera.View, camera.Position);

            // Draw the help text
            textBatch.Begin();
            textBatch.DrawString(font, treeFiles[currentTreeType].Name, new Vector2(40, 40), Color.PapayaWhip);
            textBatch.DrawString(font, "Arrow keys - Rotate", new Vector2(40, 440), Color.PapayaWhip);
            textBatch.DrawString(font, "A, Z - Zoom", new Vector2(40, 460), Color.PapayaWhip);
            textBatch.DrawString(font, "Q, W - Change tree profile", new Vector2(40, 480), Color.PapayaWhip);
            textBatch.DrawString(font, "S - Toggle sorted leaves " + (tree.Leaves.SortingEnabled? "(ON)":"(OFF)"), new Vector2(40, 500), Color.PapayaWhip);
            textBatch.DrawString(font, "D - New seed", new Vector2(40, 520), Color.PapayaWhip);
            textBatch.DrawString(font, "FPS: " + currentFps, new Vector2(500, 40), Color.PapayaWhip);
            textBatch.End();

            // Naughty SpriteBatch pollutes my pretty render state. Enable ZBuffering again.
            graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;

            base.Draw(gameTime);
        }
    }
}
