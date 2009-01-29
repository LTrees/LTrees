using System;
using System.Collections.Generic;
using System.Text;

namespace LTreesLibrary.Trees.Instructions
{
    public class ScaleRadius : TreeCrayonInstruction
    {
        private float scale;
        private float variation;

        public float Variation
        {
            get { return variation; }
            set { variation = value; }
        }


        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public ScaleRadius(float scale, float variation)
        {
            this.scale = scale;
            this.variation = variation;
        }


        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.ScaleRadius(Util.Random(rnd, scale, variation));
        }

        #endregion
    }
}
