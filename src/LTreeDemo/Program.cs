using System;
using System.Windows.Forms;
using System.Threading;

namespace LTreeDemo
{
    static class Program
    {
        /*/// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (LTreeDemoGame game = new LTreeDemoGame())
            {
                game.Run();
            }
        }*/
        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}

