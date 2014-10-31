using System;
using System.Collections.Generic;
using System.Text;

namespace LTreesLibrary.Trees.Instructions
{
    /// <summary>
    /// Has a chance of performing a group of instructions; otherwise does nothing.
    /// </summary>
    public class Maybe : TreeCrayonInstruction
    {
        private List<TreeCrayonInstruction> instructions = new List<TreeCrayonInstruction>();
        private float chance;

        /// <summary>
        /// Probability that the instructions will be executed. Should be between 0.0f and 1.0f.
        /// </summary>
        public float Chance
        {
            get { return chance; }
            set { chance = value; }
        }


        public List<TreeCrayonInstruction> Instructions
        {
            get { return instructions; }
            set { instructions = value; }
        }

        public Maybe(float chance)
        {
            this.chance = chance;
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            if (rnd.NextDouble() < chance)
            {
                foreach (TreeCrayonInstruction child in instructions)
                {
                    child.Execute(crayon, rnd);
                }
            }
        }

        #endregion
    }
}
