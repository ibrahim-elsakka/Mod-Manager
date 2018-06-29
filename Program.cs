using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mod_Manager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
           /* Process[] p = Process.GetProcessesByName("modmgrupdater.exe");
            
            if (p.Length == 0)
            {
                MessageBox.Show("Improper Launch Sequence", "Error", MessageBoxButtons.OK);
                Environment.Exit(0);
            }*/
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
