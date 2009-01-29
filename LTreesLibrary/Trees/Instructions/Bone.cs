using System;
using System.Collections.Generic;
using System.Text;

namespace LTreesLibrary.Trees.Instructions
{
    public class Bone : TreeCrayonInstruction
    {
        public int Delta { get; set; }

        public Bone(int delta)
        {
            Delta = delta;
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.Bone(Delta);
        }

        #endregion
    }
}
