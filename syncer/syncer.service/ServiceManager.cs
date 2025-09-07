using System;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace syncer.service
{
    /// <summary>
    /// Enhanced service installer and manager for deployment
    /// Provides both GUI and command-line installation options
    /// </summary>
    public class ServiceManager
    {
        private const string SERVICE_NAME = "FTPSyncerService";
        private const string SERVICE_DISPLAY_NAME = "FTPSyncer Service";
        
        public static void HandleServiceManagement(string[] args)
        {
            if (args.Length > 0)
            {
                // Command line mode
                HandleCommandLine(args);
            }
            else
            {
                // GUI mode or service mode
                if (Environment.UserInteractive)
                {
                    // Running in console/GUI mode
                    ShowServiceManagerGUI();
                }
                else
                {
                    // Running as Windows service
                    ServiceBase[] servicesToRun = new ServiceBase[] { new Service1() };
                    ServiceBase.Run(servicesToRun);
                }
            }
        }

        private static void HandleCommandLine(string[] args)
        {
            string command = args[0].ToLower();
            
            switch (command)
            {
                case "install":
                case "/install":
                case "-install":
                    InstallService();
                    break;
                    
                case "uninstall":
                case "/uninstall":
                case "-uninstall":
                    UninstallService();
                    break;
                    
                case "start":
                case "/start":
                case "-start":
                    StartService();
                    break;
                    
                case "stop":
                case "/stop":
                case "-stop":
                    StopService();
                    break;
                    
                case "status":
                case "/status":
                case "-status":
                    ShowServiceStatus();
                    break;
                    
                default:
                    ShowCommandLineHelp();
                    break;
            }
        }

        private static void ShowServiceManagerGUI()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            ServiceManagerForm form = new ServiceManagerForm();
            Application.Run(form);
        }

        private static void InstallService()
        {
            try
            {
                Console.WriteLine("Installing FTPSyncer Service...");
                
                // Use ManagedInstallerClass to install the service
                System.Configuration.Install.ManagedInstallerClass.InstallHelper(
                    new string[] { System.Reflection.Assembly.GetExecutingAssembly().Location });
                
                Console.WriteLine("Service installed successfully!");
                
                // Start the service
                StartService();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error installing service: " + ex.Message);
                Environment.ExitCode = 1;
            }
        }

        private static void UninstallService()
        {
            try
            {
                Console.WriteLine("Uninstalling FTPSyncer Service...");
                
                // Stop service first
                StopService();
                
                // Uninstall the service
                System.Configuration.Install.ManagedInstallerClass.InstallHelper(
                    new string[] { "/u", System.Reflection.Assembly.GetExecutingAssembly().Location });
                
                Console.WriteLine("Service uninstalled successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uninstalling service: " + ex.Message);
                Environment.ExitCode = 1;
            }
        }

        private static void StartService()
        {
            try
            {
                using (ServiceController service = new ServiceController(SERVICE_NAME))
                {
                    if (service.Status == ServiceControllerStatus.Stopped)
                    {
                        Console.WriteLine("Starting FTPSyncer Service...");
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                        Console.WriteLine("Service started successfully!");
                    }
                    else
                    {
                        Console.WriteLine("Service is already running.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting service: " + ex.Message);
                Environment.ExitCode = 1;
            }
        }

        private static void StopService()
        {
            try
            {
                using (ServiceController service = new ServiceController(SERVICE_NAME))
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        Console.WriteLine("Stopping FTPSyncer Service...");
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        Console.WriteLine("Service stopped successfully!");
                    }
                    else
                    {
                        Console.WriteLine("Service is not running.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error stopping service: " + ex.Message);
                Environment.ExitCode = 1;
            }
        }

        private static void ShowServiceStatus()
        {
            try
            {
                using (ServiceController service = new ServiceController(SERVICE_NAME))
                {
                    Console.WriteLine("=== FTPSyncer Service Status ===");
                    Console.WriteLine("Service Name: " + SERVICE_NAME);
                    Console.WriteLine("Display Name: " + SERVICE_DISPLAY_NAME);
                    Console.WriteLine("Current Status: " + service.Status);
                    Console.WriteLine("Can Stop: " + service.CanStop);
                    Console.WriteLine("Can Pause: " + service.CanPauseAndContinue);
                    Console.WriteLine("Service Type: " + service.ServiceType);
                    Console.WriteLine("================================");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting service status: " + ex.Message);
                Console.WriteLine("Service may not be installed.");
                Environment.ExitCode = 1;
            }
        }

        private static void ShowCommandLineHelp()
        {
            Console.WriteLine();
            Console.WriteLine("FTPSyncer Service Manager");
            Console.WriteLine("=========================");
            Console.WriteLine();
            Console.WriteLine("Usage: syncer.service.exe [command]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  install    - Install the Windows service");
            Console.WriteLine("  uninstall  - Uninstall the Windows service");
            Console.WriteLine("  start      - Start the service");
            Console.WriteLine("  stop       - Stop the service");
            Console.WriteLine("  status     - Show service status");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  syncer.service.exe install");
            Console.WriteLine("  syncer.service.exe start");
            Console.WriteLine("  syncer.service.exe status");
            Console.WriteLine();
            Console.WriteLine("Note: Installation and uninstallation require Administrator privileges.");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Simple GUI form for service management
    /// </summary>
    public partial class ServiceManagerForm : Form
    {
        private Button btnInstall;
        private Button btnUninstall;
        private Button btnStart;
        private Button btnStop;
        private Button btnStatus;
        private TextBox txtStatus;
        private System.Windows.Forms.Timer statusTimer;

        public ServiceManagerForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "FTPSyncer Service Manager";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Create controls
            btnInstall = new Button { Text = "Install Service", Location = new System.Drawing.Point(20, 20), Size = new System.Drawing.Size(100, 30) };
            btnUninstall = new Button { Text = "Uninstall Service", Location = new System.Drawing.Point(130, 20), Size = new System.Drawing.Size(100, 30) };
            btnStart = new Button { Text = "Start Service", Location = new System.Drawing.Point(20, 60), Size = new System.Drawing.Size(100, 30) };
            btnStop = new Button { Text = "Stop Service", Location = new System.Drawing.Point(130, 60), Size = new System.Drawing.Size(100, 30) };
            btnStatus = new Button { Text = "Refresh Status", Location = new System.Drawing.Point(240, 60), Size = new System.Drawing.Size(100, 30) };

            txtStatus = new TextBox 
            { 
                Location = new System.Drawing.Point(20, 100), 
                Size = new System.Drawing.Size(450, 250), 
                Multiline = true, 
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };

            // Add event handlers
            btnInstall.Click += BtnInstall_Click;
            btnUninstall.Click += BtnUninstall_Click;
            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnStatus.Click += BtnStatus_Click;

            // Add controls to form
            this.Controls.AddRange(new Control[] { btnInstall, btnUninstall, btnStart, btnStop, btnStatus, txtStatus });

            // Set up status timer
            statusTimer = new System.Windows.Forms.Timer { Interval = 5000 }; // Update every 5 seconds
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();

            // Initial status update
            UpdateServiceStatus();
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            AppendStatus("Installing service...");
            // Implementation would call ServiceManager.InstallService() in background thread
        }

        private void BtnUninstall_Click(object sender, EventArgs e)
        {
            AppendStatus("Uninstalling service...");
            // Implementation would call ServiceManager.UninstallService() in background thread
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            AppendStatus("Starting service...");
            // Implementation would call ServiceManager.StartService() in background thread
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            AppendStatus("Stopping service...");
            // Implementation would call ServiceManager.StopService() in background thread
        }

        private void BtnStatus_Click(object sender, EventArgs e)
        {
            UpdateServiceStatus();
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            UpdateServiceStatus();
        }

        private void UpdateServiceStatus()
        {
            try
            {
                using (ServiceController service = new ServiceController("FTPSyncerService"))
                {
                    string statusText = string.Format("Service Status: {0}\r\nLast Updated: {1}\r\n", 
                        service.Status, DateTime.Now);
                    
                    if (txtStatus.InvokeRequired)
                    {
                        txtStatus.Invoke(new Action(() => { txtStatus.Text = statusText; }));
                    }
                    else
                    {
                        txtStatus.Text = statusText;
                    }
                }
            }
            catch (Exception ex)
            {
                AppendStatus("Error getting status: " + ex.Message);
            }
        }

        private void AppendStatus(string message)
        {
            string statusMessage = string.Format("{0}: {1}\r\n", DateTime.Now, message);
            if (txtStatus.InvokeRequired)
            {
                txtStatus.Invoke(new Action(() => { txtStatus.AppendText(statusMessage); }));
            }
            else
            {
                txtStatus.AppendText(statusMessage);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && statusTimer != null)
            {
                statusTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
