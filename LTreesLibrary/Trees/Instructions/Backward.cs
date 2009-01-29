using System;
using System.Collections.Generic;
using System.Text;

namespace LTreesLibrary.Trees.Instructions
{
    public class Backward : TreeCrayonInstruction
    {
        private float distance;
        private float distanceVariation;

        public float DistanceVariation
        {
            get { return distanceVariation; }
            set { distanceVariation = value; }
        }


        public float Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        public Backward(float distance, float variation)
        {
            this.distance = distance;
            this.distanceVariation = variation;
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.Backward(Util.Random(rnd, distance, distanceVariation));
        }

        #endregion
    }
}
