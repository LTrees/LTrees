using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Removes branches too close to the ground. Branches that point upwards are never affected by this constraint.
    /// </summary>
    public class ConstrainUndergroundBranches : TreeContraints
    {
        private float limit = 256.0f;

        /// <summary>
        /// Minimum allowed Y-coordinate for branches pointing downwards.
        /// </summary>
        public float Limit
        {
            get { return limit; }
            set { limit = value; }
        }

        public ConstrainUndergroundBranches()
        {
        }
        public ConstrainUndergroundBranches(float limit)
        {
            this.limit = limit;
        }

        #region TreeContraints Members

        public bool ConstrainForward(TreeCrayon crayon, ref float distance, ref float radiusEndScale)
        {
            Matrix m = crayon.GetTransform();
            if (m.Up.Y < 0.0f && m.Translation.Y + m.Up.Y * distance < limit)
                return false;// distance *= 100; // Use distance * 100 to visualize which branches will be cut.
            return true;
        }

        #endregion
    }
}
