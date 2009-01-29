using System;
using System.Collections.Generic;
using System.Text;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Contains a list of tree constraints to apply while generating a tree, and one user-defined
    /// constraint that the TreeGenerator will automatically assign.
    /// </summary>
    public class CompositeTreeConstraints : TreeContraints
    {
        private List<TreeContraints> constaints = new List<TreeContraints>();

        private TreeContraints userConstraint;

        /// <summary>
        /// User-constraint assigned to by the TreeGenerator. Do not assign this manually.
        /// This constraint is applied as the last constraint.
        /// </summary>
        public TreeContraints UserConstraint
        {
            get { return userConstraint; }
            set { userConstraint = value; }
        }
        
        /// <summary>
        /// List of constraints.
        /// </summary>
        public List<TreeContraints> Constaints
        {
            get { return constaints; }
        }


        #region TreeContraints Members

        public bool ConstrainForward(TreeCrayon crayon, ref float distance, ref float radiusEndScale)
        {
            foreach (TreeContraints c in constaints)
            {
                if (!c.ConstrainForward(crayon, ref distance, ref radiusEndScale))
                    return false;
            }
            if (userConstraint != null)
                return userConstraint.ConstrainForward(crayon, ref distance, ref radiusEndScale);
            return true;
        }

        #endregion
    }
}
