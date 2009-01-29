using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LTreesLibrary.Trees.Instructions
{
    /// <summary>
    /// Twists the crayon such that its local X-axis becomes parrallel to the ground plane.
    /// (The ground plane is the XZ-plane).
    /// The X-axis is the rotation axis used when pitching the crayon, which makes this
    /// useful when one wants to curb a branch towards or away from the ground.
    /// </summary>
    public class Align : TreeCrayonInstruction
    {
        public void Execute(TreeCrayon crayon, Random rnd)
        {
            Matrix transform = crayon.GetTransform();

            Vector3 branchDir = transform.Up;
            Vector3 axis = Vector3.Up;
            float dot = Vector3.Dot(axis, branchDir);

            // If branch is almost perpendicular to alignment axis,
            // just do nothing
            if (Math.Abs(dot) > 0.999f)
                return;

            // project axis onto the crayon's XZ plane
            Vector3 axisXZ = axis - dot * branchDir;
            axisXZ.Normalize();

            float cosAngle = Vector3.Dot(transform.Backward, axisXZ);

            // The dot product of two normalized vectors is always in range [-1,1],
            // but to account for rounding errors, we have to clamp it.
            // Otherwise, Acos will return NaN in some cases.
            cosAngle = MathHelper.Clamp(cosAngle, -1, 1);

            // calculate the angle between the old Z-axis and axisXZ
            // this is also the twist angle required to align the crayon
            float rotation = (float)Math.Acos(cosAngle);

            // finally, perform the twist
            crayon.Twist(rotation);
        }
    }
}
