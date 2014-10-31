using System;
using System.Collections.Generic;
using System.Text;

namespace LTreesLibrary.Trees.Instructions
{
    public class Production
    {
        private List<TreeCrayonInstruction> instructions = new List<TreeCrayonInstruction>();

        public List<TreeCrayonInstruction> Instructions
        {
            get { return instructions; }
            set { instructions = value; }
        }

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            foreach (TreeCrayonInstruction instruction in instructions)
            {
                instruction.Execute(crayon, rnd);
            }
        }
    }
}
