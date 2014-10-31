using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LeafTool
{
    class LeafSprite
    {
        public Vector2 Center { get; set; }
        public float Size { get; set; }
        public float Angle { get; set; }
        public int TextureIndex { get; set; }

        public LeafSprite()
        {
        }
        public LeafSprite(Vector2 center, float size, float angle, int index)
        {
            Center = center;
            Size = size;
            Angle = angle;
            TextureIndex = index;
        }
    }
}
