using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace syncer.service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller serviceProcessInstaller;
        private ServiceInstaller serviceInstaller;

        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.serviceProcessInstaller = new ServiceProcessInstaller();
            this.serviceInstaller = new ServiceInstaller();

            // Service Process Installer - LocalSystem for registry access
            this.serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            this.serviceProcessInstaller.Password = null;
            this.serviceProcessInstaller.Username = null;

            // Service Installer - configured for job recovery
            this.serviceInstaller.ServiceName = "FTPSyncerService";
            this.serviceInstaller.DisplayName = "FTPSyncer Service";
            this.serviceInstaller.Description = "Automated FTP/SFTP file synchronization service with job recovery support. Resumes interrupted sync jobs after system restart.";
            this.serviceInstaller.StartType = ServiceStartMode.Automatic;

            // Add installers to collection
            this.Installers.AddRange(new Installer[] {
                this.serviceProcessInstaller,
                this.serviceInstaller
            });
        }

        /// <summary>
        /// Configure service recovery options after installation
        /// </summary>
        protected override void OnAfterInstall(System.Collections.IDictionary savedState)
        {
            base.OnAfterInstall(savedState);
            
            try
            {
                // Configure service recovery using sc command
                // This will restart the service automatically if it crashes
                string serviceName = this.serviceInstaller.ServiceName;
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = string.Format("failure {0} reset= 86400 actions= restart/5000/restart/10000/restart/30000", serviceName),
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                });
            }
            catch
            {
                // Ignore errors in recovery configuration
            }
        }
    }
}