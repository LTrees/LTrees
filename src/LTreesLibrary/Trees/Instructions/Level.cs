using System;
using System.Collections.Generic;
using System.Text;

namespace LTreesLibrary.Trees.Instructions
{
    public class Level : TreeCrayonInstruction
    {
        private int deltaLevel;

        public int DeltaLevel
        {
            get { return deltaLevel; }
            set { deltaLevel = value; }
        }

        public Level(int deltaLevel)
        {
            this.deltaLevel = deltaLevel;
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.Level = crayon.Level + deltaLevel;
        }

        #endregion
    }
}
