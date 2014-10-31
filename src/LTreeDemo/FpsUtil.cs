using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LTreeDemo
{
    public class FpsUtil
    {
        private int framesSoFar;
        private int millisecondsSoFar;
        private float fps;
        private int interval = 1000;

        public float Fps
        {
            get { return fps; }
        }

        public int Interval
        {
            get { return interval; }
            set { interval = value; }
        }

        public bool NewFrame(GameTime time)
        {
            framesSoFar++;
            millisecondsSoFar += time.ElapsedGameTime.Milliseconds;
            if (millisecondsSoFar >= interval)
            {
                fps = framesSoFar * 1000.0f / (float)millisecondsSoFar;
                framesSoFar = 0;
                millisecondsSoFar = 0;
                return true;
            }
            return false;
        }
    }
}
