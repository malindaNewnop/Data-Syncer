using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace syncer.ui
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Initialize service locator with stub services so the UI runs
            ServiceLocator.Initialize();
            // Run main dashboard form
            Application.Run(new FormMain());
        }
    }
}
