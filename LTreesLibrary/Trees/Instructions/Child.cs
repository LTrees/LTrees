using System;
using System.Collections.Generic;
using System.Text;

namespace LTreesLibrary.Trees.Instructions
{
    public class Child : TreeCrayonInstruction
    {
        private List<TreeCrayonInstruction> instructions = new List<TreeCrayonInstruction>();

        public List<TreeCrayonInstruction> Instructions
        {
            get { return instructions; }
            set { instructions = value; }
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.PushState();
            foreach (TreeCrayonInstruction instruction in instructions)
            {
                instruction.Execute(crayon, rnd);
            }
            crayon.PopState();
        }

        #endregion
    }
}
