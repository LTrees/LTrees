using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LTreesLibrary.Trees.Instructions
{
    public class Pitch : TreeCrayonInstruction
    {
        private float angle;

        private float variation;

        public float Variation
        {
            get { return variation; }
            set { variation = value; }
        }


        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        public Pitch(float angle, float variation)
        {
            this.angle = MathHelper.ToRadians(angle);
            this.variation = MathHelper.ToRadians(variation);
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.Pitch(Util.Random(rnd, angle, variation));
        }

        #endregion
    }
}
