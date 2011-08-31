/*
 * Copyright (c) 2007-2009 Asger Feldthaus
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using LTreesLibrary.Trees;
using System.Xml;
using LTreesLibrary.Trees.Wind;

namespace LTreeDemo
{
    /// <summary>
    /// An example of trees can be loaded and rendered from an XNA Game.
    /// This class is currently unused because we use a windows form instead. To see it
    /// in action modify Program.cs accordingly.
    /// </summary>
    public class LTreeDemoGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Camera camera;
        FPSCameraController camerafps;
        SpriteFont font;

        Matrix gridworld = Matrix.Identity;
        Grid grid;

        Matrix meshworld = Matrix.Identity;

        FpsUtil fpsutil = new FpsUtil();

        String profileAssetFormat = "Trees/{0}";
        
        String[] profileNames = new String[]
        {
            "Birch",
            "Pine",
            "Gardenwood",
            "Graywood",
            "Rug",
            "Willow",
        };
        TreeProfile[] profiles;

        TreeLineMesh linemesh;

        int currentTree = 0;

        SimpleTree tree;

        bool drawTree = true;
        bool drawLeaves = true;
        bool drawBones = false;
        bool drawTreeLines = false;

        TreeWindAnimator animator;
        WindStrengthSin wind;

        KeyboardState lastKeyboard;
        KeyboardState keyboard;

        public LTreeDemoGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            camera = new Camera();

            base.Initialize();
        }

        void LoadTreeGenerators()
        {
            // Here we load all the tree profiles using the content manager
            // The tree profiles contain information about how to generate a tree
            profiles = new TreeProfile[profileNames.Length];
            for (int i = 0; i < profiles.Length; i++)
            {
                profiles[i] = Content.Load<TreeProfile>(String.Format(profileAssetFormat, profileNames[i]));
            }
        }

        void NewTree()
        {
            // Generates a new tree using the currently selected tree profile
            // We call TreeProfile.GenerateSimpleTree() which does three things for us:
            // 1. Generates a tree skeleton
            // 2. Creates a mesh for the branches
            // 3. Creates a particle cloud (TreeLeafCloud) for the leaves
            // The line mesh is just for testing and debugging
            tree = profiles[currentTree].GenerateSimpleTree();
            linemesh = new TreeLineMesh(GraphicsDevice, tree.Skeleton);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            camerafps = new FPSCameraController(camera, GraphicsDevice);
            grid = new Grid(GraphicsDevice);

            LoadTreeGenerators();
            NewTree();

            font = Content.Load<SpriteFont>("Fonts/Font");

            // Create a wind animation. The WindStrengthSin class is a rather crude but simple wind generator
            wind = new WindStrengthSin();
            animator = new TreeWindAnimator(wind);
        }
        
        protected override void UnloadContent()
        {
        }


        public bool IsKeyTriggered(Keys key)
        {
            return keyboard.IsKeyDown(key) && !lastKeyboard.IsKeyDown(key);
        }

        protected override void Update(GameTime gameTime)
        {
            lastKeyboard = keyboard;
            keyboard = Keyboard.GetState();

            if (!base.IsActive)
                return;

            if (keyboard.IsKeyDown(Keys.Escape))
                this.Exit();

            camerafps.Update(gameTime);

            if (IsKeyTriggered(Keys.F5))
            {
                NewTree();
            }
            if (IsKeyTriggered(Keys.F1))
            {
                currentTree = (currentTree + profiles.Length - 1) % profiles.Length;
                NewTree();
            }
            if (IsKeyTriggered(Keys.F2))
            {
                currentTree = (currentTree + 1) % profiles.Length;
                NewTree();
            }

            if (IsKeyTriggered(Keys.T))
                drawTree = !drawTree;

            if (IsKeyTriggered(Keys.L))
                drawLeaves = !drawLeaves;

            if (IsKeyTriggered(Keys.B))
                drawBones = !drawBones;

            if (IsKeyTriggered(Keys.V))
                drawTreeLines = !drawTreeLines;

            wind.Update(gameTime);
            animator.Animate(tree.Skeleton, tree.AnimationState, gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            grid.Draw(gridworld, camera.View, camera.Projection);

        	GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            // We call SimpleTree.DrawTrunk to draw the trunk.
            // This sets a lot of effect parameters for us, although not all of them
            // For example, the direction of light 0 can be set manually using tree.TrunkEffect.Parameters["DirLight0Direction"]
            if (drawTree)
                tree.DrawTrunk(meshworld, camera.View, camera.Projection);

            if (drawTreeLines)
                linemesh.Draw(meshworld, camera.View, camera.Projection);

            // We draw the leaves using SimpleTree.DrawLeaves.
            // There is no method to render both trunk and leaves, because the leaves are transparent,
            // so in practice you want to render them at different times
            if (drawLeaves)
                tree.DrawLeaves(meshworld, camera.View, camera.Projection);

			GraphicsDevice.DepthStencilState = DepthStencilState.None;
            if (drawBones)
                tree.DrawBonesAsLines(meshworld, camera.View, camera.Projection);

            fpsutil.NewFrame(gameTime);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, "FPS: " + fpsutil.Fps, new Vector2(50, 50), Color.White);
            spriteBatch.DrawString(font, "Tris: " + tree.TrunkMesh.NumberOfTriangles, new Vector2(50, 75), Color.White);
            spriteBatch.DrawString(font, "Bones: " + tree.Skeleton.Bones.Count, new Vector2(50, 100), Color.White);
            spriteBatch.DrawString(font, "Leaves: " + tree.Skeleton.Leaves.Count, new Vector2(50, 125), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
