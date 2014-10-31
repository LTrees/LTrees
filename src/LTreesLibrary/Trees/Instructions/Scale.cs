using System;
using System.Collections.Generic;
using System.Text;

namespace LTreesLibrary.Trees.Instructions
{
    public class Scale : TreeCrayonInstruction
    {
        private float scale;

        private float variation;

        public float Variation
        {
            get { return variation; }
            set { variation = value; }
        }


        public float Value
        {
            get { return scale; }
            set { scale = value; }
        }

        public Scale(float scale, float variation)
        {
            this.scale = scale;
            this.variation = variation;
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.Scale(Util.Random(rnd, scale, variation));
        }

        #endregion
    }
}
