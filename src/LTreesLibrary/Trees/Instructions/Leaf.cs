using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LTreesLibrary.Trees.Instructions
{
    public class Leaf : TreeCrayonInstruction
    {
        private Vector4 color;
        private Vector4 colorVariation;
        private Vector2 size;
        private Vector2 sizeVariation;

        public Vector2 SizeVariation
        {
            get { return sizeVariation; }
            set { sizeVariation = value; }
        }

        
        public Vector2 Size
        {
            get { return size; }
            set { size = value; }
        }


        public Vector4 ColorVariation
        {
            get { return colorVariation; }
            set { colorVariation = value; }
        }

        public Vector4 Color
        {
            get { return color; }
            set { color = value; }
        }

        public float AxisOffset { get; set; }

        public Leaf(Vector4 color, Vector4 colorVariation)
        {
            this.color = color;
            this.colorVariation = colorVariation;
            this.size = new Vector2(128, 128);
        }
        public Leaf()
        {
            this.color = new Vector4(1, 1, 1, 1);
            this.colorVariation = Vector4.Zero;
            this.Size = new Vector2(128, 128);
        }


        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            if (crayon.Level == 0)
            {
                float rotation = 0.0f;
                if (crayon.Skeleton.LeafAxis == null)
                    rotation = (float)rnd.NextDouble() * MathHelper.TwoPi;
                crayon.Leaf(rotation, 
                    size + sizeVariation * (2.0f * (float)rnd.NextDouble() - 1.0f), 
                    color + colorVariation * (2.0f * (float)rnd.NextDouble() - 1.0f),
                    AxisOffset);
            }
        }

        #endregion
    }
}
