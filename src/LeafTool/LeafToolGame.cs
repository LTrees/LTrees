/*
 * Copyright (c) 2007-2009 Asger Feldthaus
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using DialogResult = System.Windows.Forms.DialogResult;

namespace LeafTool
{
    /// <summary>
    /// A crude but useful tool for creating leaf textures.
    /// </summary>
    /// <remarks>
    /// CONTROLS
    /// Left button:    Place sprite
    /// Right button:   Place pivot
    /// Mouse wheel:    Change the size of the sprite
    /// Q,W:            Change sprite's rotation relative to pivot
    /// E:              Reset sprite's rotation
    /// A,S:            Cycle available sprite textures
    /// D:              Delete a sprite near the cursor
    /// Ctrl+S:         Save the texture to a file
    /// Shift:          Hold shift while rotating or scaling the sprite to fine-tine rotation and scale.
    /// Escape:         Exit program
    /// 
    /// HOW TO ADD MORE SPRITE TEXTURES
    /// Place a texture with just one leaf and transparent background in the Textures folder,
    /// add it to the 'textureFilenames' array in LeafToolGame, and add it to the content project.
    /// 
    /// HINTS
    /// - Make sure no leaf overlaps the edge of the texture. It produces very obvious artifacts.
    /// - Leave holes in the texture instead of filling it all out with leaves.
    /// - Avoid perfect shapes like squares and circles, and try to place the leaves more irregularly.
    /// - The tool may seem crude, but it IS easier than using photoshop and the leaves WILL look better.
    /// </remarks>
    public class LeafToolGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        RenderTarget2D renderTarget;

        String textureFilenameFormat = "Textures/{0}";
        String[] textureFilenames =
        {
            "BirchA",
            "RedWoodLeaf",
            "willow-leaf",
            "OakLeaf",
            "PineLeaf",
        };
        Texture2D[] textures;

        // Index of the leaf texture currently being painted
        int textureIndex = 0;


        // Center of rotation for leaves
        Vector2 pivot = Vector2.Zero;
        float angleOffset = 0.0f;
        Texture2D pivotTexture;

        // List of sprites we have added
        List<LeafSprite> sprites = new List<LeafSprite>();

        LeafSprite mouseSprite;
        Vector2 imageSize;

        KeyboardState keyboard;
        KeyboardState lastKeyboard;
        MouseState mouse;
        MouseState lastMouse;

        public Vector2 MousePos
        {
            get { return new Vector2(mouse.X, mouse.Y); }
        }

        public LeafToolGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 256;
            graphics.PreferredBackBufferHeight = 256;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            textures = new Texture2D[textureFilenames.Length];
            for (int i = 0; i < textures.Length; i++)
			{
                textures[i] = Content.Load<Texture2D>(String.Format(textureFilenameFormat, textureFilenames[i]));
			}

            renderTarget = new RenderTarget2D(GraphicsDevice, 256, 256, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            mouseSprite = new LeafSprite(Vector2.Zero, 32, 0, 0);

            imageSize = new Vector2(256, 256);
            pivot = imageSize / 2.0f;

            pivotTexture = Content.Load<Texture2D>("Textures/Pivot");
        }

        protected override void UnloadContent()
        {
        }

        private void UpdateMouseSprite()
        {
            mouseSprite.Center = MousePos;
            mouseSprite.Angle = (float)Math.Atan2(MousePos.Y - pivot.Y, MousePos.X - pivot.X) + MathHelper.PiOver2 + angleOffset;
            mouseSprite.TextureIndex = textureIndex;
        }

        private bool IsKeyTriggered(Keys key)
        {
            return keyboard.IsKeyDown(key) && !lastKeyboard.IsKeyDown(key);
        }
        public bool ControlPressed
        { 
            get { return keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl); }
        }
        public bool ShiftPressed
        {
            get { return keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift); }
        }

        protected override void Update(GameTime gameTime)
        {
            lastKeyboard = keyboard;
            keyboard = Keyboard.GetState();
            lastMouse = mouse;
            mouse = Mouse.GetState();

            float seconds = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            float shiftHalf = ShiftPressed ? 0.5f : 1.0f;
            
            // Escape: Exit
            if (keyboard.IsKeyDown(Keys.Escape))
                this.Exit();

            // Q,W,E: Rotate
            if (keyboard.IsKeyDown(Keys.Q))
                angleOffset += MathHelper.Pi * shiftHalf * seconds;
            if (keyboard.IsKeyDown(Keys.W))
                angleOffset -= MathHelper.Pi * shiftHalf * seconds;
            if (keyboard.IsKeyDown(Keys.E))
                angleOffset = 0.0f;

            // A,S: Change leaf texture
            if (IsKeyTriggered(Keys.A))
                textureIndex = (textureIndex + 1) % textures.Length;
            if (IsKeyTriggered(Keys.S))
                textureIndex = (textureIndex - 1 + textures.Length) % textures.Length;

            // Mouse wheel: Size
            float scroll = mouse.ScrollWheelValue - lastMouse.ScrollWheelValue;
            mouseSprite.Size += scroll * shiftHalf * 0.1f;

            // D: Delete sprite
            if (IsKeyTriggered(Keys.D))
            {
                DeleteSpriteAtCursor();
            }
            // Ctrl+S: Save image
            if (IsKeyTriggered(Keys.S) && ControlPressed)
            {
                OpenSaveDialog();
            }

            UpdateMouseSprite();

            // Left click: Place sprite
            if (mouse.LeftButton == ButtonState.Pressed && lastMouse.LeftButton == ButtonState.Released)
            {
                sprites.Add(new LeafSprite(mouseSprite.Center, mouseSprite.Size, mouseSprite.Angle, mouseSprite.TextureIndex));
            }
            // Right click: Move pivot
            if (mouse.RightButton == ButtonState.Pressed && lastMouse.RightButton == ButtonState.Released)
            {
                pivot = MousePos;
            }

            base.Update(gameTime);
        }

        SaveFileDialog saveDialog = new SaveFileDialog();
        private void OpenSaveDialog()
        {
            saveDialog.DefaultExt = ".png";
            saveDialog.AddExtension = true;
            saveDialog.Filter = "Image files (*.png, *.jpg)|*.png;*.jpg|All files (*.*)|*.*";
            DialogResult result = saveDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                Texture2D texture = renderTarget;
				using (Stream stream = File.OpenWrite(saveDialog.FileName))
				{
					if (IsImageFileFormatFromFilenamePng(saveDialog.FileName))
						texture.SaveAsPng(stream, texture.Width, texture.Height);
					else
						texture.SaveAsJpeg(stream, texture.Width, texture.Height);
				}
            }
        }

        private bool IsImageFileFormatFromFilenamePng(string filename)
        {
            String ext = filename.Substring(filename.LastIndexOf(".")).ToLowerInvariant();
            switch (ext)
            {
                case ".png":
					return true;
                case ".jpg":
					return false;
                default:
                    return true; // Just use PNG this as default
            }
        }

        private void DeleteSpriteAtCursor()
        {
            LeafSprite sprite = FindClosestSpriteUnderPoint(MousePos);
            if (sprite == null)
                return;

            sprites.Remove(sprite);
        }

        private LeafSprite FindClosestSpriteUnderPoint(Vector2 searchPoint)
        {
            float closestDistanceSQ = 1e20f;
            LeafSprite closest = null;
            for (int i = 0; i < sprites.Count; i++)
            {
                LeafSprite sprite = sprites[i];

                // Determine if cursor is inside the sprite's rectangle
                Vector2 leafToCursor = searchPoint - sprite.Center;
                Vector2 localX = new Vector2((float)Math.Cos(sprite.Angle), (float)Math.Sin(sprite.Angle));
                Vector2 localY = new Vector2(-localX.Y, localX.X);
                Vector2 localCoords = new Vector2(
                    Vector2.Dot(leafToCursor, localX),
                    Vector2.Dot(leafToCursor, localY)
                    );

                // If outside rectangle, skip this sprite
                if (Math.Abs(localCoords.X) > sprite.Size / 2 || Math.Abs(localCoords.Y) > sprite.Size / 2)
                    continue;

                float distanceSQ = localCoords.LengthSquared();
                if (distanceSQ < closestDistanceSQ)
                {
                    closestDistanceSQ = distanceSQ;
                    closest = sprite;
                }
            }
            return closest;
        }

        

        protected override void Draw(GameTime gameTime)
        {
            // Draw on the render target in case we want to save the image
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(new Color(0.0f, 0.0f, 0.0f, 0.0f));
            DrawBackgroundLeaves();
            DrawForegroundLeaves();
            GraphicsDevice.SetRenderTarget(null);

			GraphicsDevice.Clear(new Color(0.0f, 1.0f, 1.0f, 1.0f));
            DrawLeafSprites();
            DrawMouseSprite();
            DrawPivot();

            base.Draw(gameTime);
        }

        private void DrawBackgroundLeaves()
        {
			BlendState savedBlendState = GraphicsDevice.BlendState;
        	GraphicsDevice.BlendState = ChangeColorWriteChannels(savedBlendState,
				ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            for (int i = 0; i < sprites.Count; i++)
            {
                LeafSprite sprite = sprites[sprites.Count - i - 1];
                BatchLeafSprite(sprite, 2.0f);
            }
            spriteBatch.End();
            GraphicsDevice.BlendState = ChangeColorWriteChannels(savedBlendState,
				ColorWriteChannels.All);
        }

		private static BlendState ChangeColorWriteChannels(BlendState blendState, ColorWriteChannels colorWriteChannels)
		{
			return new BlendState
			{
				AlphaBlendFunction = blendState.AlphaBlendFunction,
				AlphaDestinationBlend = blendState.AlphaDestinationBlend,
				AlphaSourceBlend = blendState.AlphaSourceBlend,
				BlendFactor = blendState.BlendFactor,
				ColorBlendFunction = blendState.ColorBlendFunction,
				ColorDestinationBlend = blendState.ColorDestinationBlend,
				ColorSourceBlend = blendState.ColorSourceBlend,
				ColorWriteChannels = colorWriteChannels,
				ColorWriteChannels1 = blendState.ColorWriteChannels1,
				ColorWriteChannels2 = blendState.ColorWriteChannels2,
				ColorWriteChannels3 = blendState.ColorWriteChannels3,
				MultiSampleMask = blendState.MultiSampleMask,
				Name = blendState.Name
			};
		}

        private void DrawForegroundLeaves()
        {
            DrawLeafSprites();
        }

        private void DrawPivot()
        {
            int pivotSize = 10;
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.Draw(pivotTexture, new Rectangle((int)pivot.X - pivotSize / 2, (int)pivot.Y - pivotSize / 2, pivotSize, pivotSize), Color.White);
            spriteBatch.End();
        }

        private void DrawMouseSprite()
        {
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            BatchLeafSprite(mouseSprite);
            spriteBatch.End();
        }

        private void DrawLeafSprites()
        {
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            foreach (LeafSprite sprite in sprites)
            {
                BatchLeafSprite(sprite);
            }
            spriteBatch.End();
        }

        private void BatchLeafSprite(LeafSprite sprite)
        {
            BatchLeafSprite(sprite, 1.0f);
        }
        private void BatchLeafSprite(LeafSprite sprite, float sizeMultiplier)
        {
            float scale = sprite.Size / (float)textures[sprite.TextureIndex].Width * sizeMultiplier;
            Vector2 textureCenter = new Vector2(textures[sprite.TextureIndex].Width, textures[sprite.TextureIndex].Height) / 2.0f;
            spriteBatch.Draw(textures[sprite.TextureIndex], sprite.Center, null, Color.White, sprite.Angle, textureCenter, scale, SpriteEffects.None, 0.0f);
        }
    }
}
