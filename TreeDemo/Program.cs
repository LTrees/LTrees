using System;

namespace TreeDemo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (TreeDemo game = new TreeDemo())
            {
                game.Run();
            }
        }
    }
}

