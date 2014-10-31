using System;
using System.Collections.Generic;
using System.Text;

namespace LTreesLibrary.Trees.Instructions
{
    public class Forward : TreeCrayonInstruction
    {
        private float distance;
        private float variation;
        private float radiusScale;

        public float RadiusScale
        {
            get { return radiusScale; }
            set { radiusScale = value; }
        }

        public float Variation
        {
            get { return variation; }
            set { variation = value; }
        }


        public float Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        public Forward(float distance, float variation, float radiusScale)
        {
            this.distance = distance;
            this.variation = variation;
            this.radiusScale = radiusScale;
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.Forward(distance + variation * ((float)rnd.NextDouble() * 2 - 1), radiusScale);
        }

        #endregion
    }
}
