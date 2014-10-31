using System;

namespace LeafTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (LeafToolGame game = new LeafToolGame())
            {
                game.Run();
            }
        }
    }
}

