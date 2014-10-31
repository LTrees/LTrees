using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LTreesLibrary.Trees.Wind
{
    public class WindStrengthSin : WindSource
    {
        private int time = 0;

        public void Update(GameTime t)
        {
            time += t.ElapsedGameTime.Milliseconds;
        }

        #region WindSource Members

        public Vector3 GetWindStrength(Vector3 position)
        {
            float seconds = time / 1000.0f;
            return 10.0f * Vector3.Right * (float)Math.Sin(seconds * 3)
                + 15.0f * Vector3.Backward * (float)Math.Sin(seconds * 5 + 1)
                + 1.5f * Vector3.Backward * (float)Math.Sin(seconds * 11 + 3)
                + 1.5f * Vector3.Right * (float)Math.Sin(seconds * 11 + 3) * (float)Math.Sin(seconds * 1 + 3);
        }

        #endregion
    }
}
