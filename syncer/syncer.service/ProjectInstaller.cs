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

            // Service Process Installer
            this.serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            this.serviceProcessInstaller.Password = null;
            this.serviceProcessInstaller.Username = null;

            // Service Installer
            this.serviceInstaller.ServiceName = "DataSyncerService";
            this.serviceInstaller.DisplayName = "Data Syncer Service";
            this.serviceInstaller.Description = "Automated file synchronization service";
            this.serviceInstaller.StartType = ServiceStartMode.Automatic;

            // Add installers to collection
            this.Installers.AddRange(new Installer[] {
                this.serviceProcessInstaller,
                this.serviceInstaller
            });
        }
    }
}