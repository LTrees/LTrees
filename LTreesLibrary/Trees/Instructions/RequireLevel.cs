using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LTreesLibrary.Trees.Instructions
{
    public enum CompareType
    {
        Less,
        Greater
    }

    public class RequireLevel : TreeCrayonInstruction
    {
        public List<TreeCrayonInstruction> Instructions { get; private set; }
        public int Level { get; set; }
        public CompareType Type { get; set; }

        public RequireLevel(int level, CompareType type)
        {
            Instructions = new List<TreeCrayonInstruction>();
            Level = level;
            Type = type;
        }
        
        public void Execute(TreeCrayon crayon, Random rnd)
        {
            if (crayon.Level >= Level && Type == CompareType.Greater || crayon.Level <= Level && Type == CompareType.Less)
            {
                foreach (TreeCrayonInstruction instruction in Instructions)
                {
                    instruction.Execute(crayon, rnd);
                }
            }
        }
    }
}
