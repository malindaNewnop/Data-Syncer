using System;
using System.ServiceProcess;

namespace syncer.service
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Supports both service mode and installation/management commands.
        /// </summary>
        static void Main(string[] args)
        {
            // Use ServiceManager for enhanced installation and management
            ServiceManager.HandleServiceManagement(args);
        }
    }
}
