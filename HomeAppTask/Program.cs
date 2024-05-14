using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeAppTask
{
    static class Program
    {
        static int exitCode = 0;


        public static void ExitApplication(int exitCode)
        {
            Program.exitCode = exitCode;
            Application.Exit();
        }

        /// <summary>
        /// The Main Starting-Point
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0)
            {
                Application.Run(new Form1(args));
                return exitCode;
            }
                
            else
            {
                Application.Run(new Form1(null));
                return exitCode;
            }
                



        }
    }
}
