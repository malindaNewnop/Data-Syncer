using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using syncer.ui.Services;
using syncer.ui.Interfaces;
using syncer.ui.Forms;
using syncer.core.Services;

namespace syncer.ui
{
    public partial class FormMain : Form
    {
        private ISyncJobService _jobService;
        private IServiceManager _serviceManager;
        private IConnectionService _connectionService;
        private ISavedJobConfigurationService _savedJobConfigService;
        
        // System tray components
        private SystemTrayManager _trayManager;
        private NotificationService _notificationService;
        
        // Full screen state management
        private bool _isFullScreen = false;
        private FormWindowState _previousWindowState;
        private FormBorderStyle _previousBorderStyle;
        private bool _previousMenuVisible;
        
        // Bandwidth control
        private BandwidthControlService _bandwidthService;
        private Timer _speedUpdateTimer;

        public FormMain()
        {
            try
            {
                InitializeComponent();
                InitializeServices();
                InitializeSystemTray();
                InitializeCustomComponents();
                InitializeBandwidthControl();
                
                // Update Quick Launch menu based on available configurations
                UpdateQuickLaunchMenuVisibility();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing main form: " + ex.Message + "\n\nThe application may not function correctly.",
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeServices()
        {
            try
            {
                _jobService = ServiceLocator.SyncJobService;
                _serviceManager = ServiceLocator.ServiceManager;
                _connectionService = ServiceLocator.ConnectionService;
                _savedJobConfigService = ServiceLocator.SavedJobConfigurationService;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize services: " + ex.Message + "\n\nSome functionality may be limited.",
                    "Service Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void InitializeSystemTray()
        {
            try
            {
                // Create the system tray manager - it will load its own settings
                _trayManager = new SystemTrayManager(this);
                
                // Create the notification service
                _notificationService = new NotificationService(_trayManager);
                
                // Update the tray icon tooltip with application status
                string serviceStatus = _serviceManager != null ? 
                    (_serviceManager.IsServiceRunning() ? "Running" : "Stopped") : "Unknown";
                _trayManager.UpdateToolTip(string.Format("FTPSyncer - Service: {0}", serviceStatus));
                
                // Load user preference for startup notification
                bool showStartupNotification = true;
                try
                {
                    var configService = ServiceLocator.ConfigurationService;
                    if (configService != null)
                    {
                        showStartupNotification = configService.GetSetting("ShowStartupNotification", true);
                    }
                }
                catch
                {
                    // Ignore configuration errors
                }
                
                // Show a startup notification if enabled
                if (showStartupNotification)
                {
                    _notificationService.ShowNotification(
                        "FTPSyncer Started", 
                        "Application is running and will minimize to the system tray when closed.",
                        ToolTipIcon.Info);
                }
                
                ServiceLocator.LogService.LogInfo("System tray initialized successfully", "UI");
            }
            catch (Exception ex)
            {
                // Log the error but continue - system tray is not critical
                Console.WriteLine("Error initializing system tray: " + ex.Message);
                try { ServiceLocator.LogService.LogError("Error initializing system tray: " + ex.Message, "UI"); } catch { }
            }
        }

        private void InitializeCustomComponents()
        {
            this.Text = "FTPSyncer - Main Dashboard";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.WindowState = FormWindowState.Normal;
            
            // Initialize full screen menu state
            fullScreenToolStripMenuItem.Enabled = true;
            normalViewToolStripMenuItem.Enabled = false;
            
            // Add event handler to refresh timer jobs when form is activated
            this.Activated += FormMain_Activated;
            
            InitializeTimerJobsGrid();
            UpdateServiceStatus();
            UpdateConnectionStatus();
            RefreshTimerJobsGrid();
            
            // Set up auto-refresh timer for timer jobs grid (every 30 seconds)
            Timer refreshTimer = new Timer();
            refreshTimer.Interval = 30000; // 30 seconds
            refreshTimer.Tick += (sender, e) => RefreshTimerJobsGrid();
            refreshTimer.Start();
            
            // Show Quick Launch popup on startup
            Timer startupTimer = new Timer();
            startupTimer.Interval = 1000; // 1 second delay to ensure form is fully loaded
            startupTimer.Tick += (sender, e) => 
            {
                startupTimer.Stop();
                startupTimer.Dispose();
                ShowQuickLaunchPopup();
            };
            startupTimer.Start();
        }

        private void FormMain_Activated(object sender, EventArgs e)
        {
            // Refresh timer jobs grid when form becomes active
            // This ensures the grid is always up-to-date when user returns to main form
            RefreshTimerJobsGrid();
            
            // Update Quick Launch menu visibility in case configurations changed
            UpdateQuickLaunchMenuVisibility();
        }



        private void UpdateServiceStatus()
        {
            try
            {
                string status = _serviceManager != null ? _serviceManager.GetServiceStatus() : "Unknown";
                lblServiceStatus.Text = "Service: " + status;
                lblServiceStatus.ForeColor = status == "Running" ? Color.Green : Color.Red;
                
                // Note: btnStartStop is hidden since service auto-starts with jobs
                // No need to update the button since it's not visible to users
                
                // Update the tray icon tooltip
                if (_trayManager != null)
                {
                    _trayManager.UpdateToolTip($"FTPSyncer - Service: {status}");
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError($"Error updating service status: {ex.Message}", "UI");
            }
        }

        private void UpdateConnectionStatus()
        {
            try
            {
                if (_connectionService != null)
                {
                    var connectionSettings = _connectionService.GetConnectionSettings();
                    if (connectionSettings != null && connectionSettings.IsRemoteConnection)
                    {
                        // Check if any jobs are currently running instead of testing connection directly
                        var timerJobManager = ServiceLocator.TimerJobManager;
                        bool hasActiveJobs = false;
                        
                        if (timerJobManager != null)
                        {
                            try
                            {
                                var runningJobs = timerJobManager.GetRunningJobs();
                                hasActiveJobs = runningJobs != null && runningJobs.Count > 0;
                                
                                // Also check if any jobs are actively uploading/downloading
                                if (!hasActiveJobs)
                                {
                                    var allJobs = timerJobManager.GetAllJobs();
                                    if (allJobs != null)
                                    {
                                        foreach (var jobKvp in allJobs)
                                        {
                                            if (timerJobManager.IsTimerJobUploading(jobKvp.Key) || 
                                                timerJobManager.IsTimerJobDownloading(jobKvp.Key))
                                            {
                                                hasActiveJobs = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                hasActiveJobs = false;
                            }
                        }
                        
                        if (hasActiveJobs)
                        {
                            lblConnectionStatus.Text = $"Connected to {connectionSettings.Host}:{connectionSettings.Port} (Job Running)";
                            lblConnectionStatus.ForeColor = Color.Green;
                        }
                        else
                        {
                            // Only test connection if no jobs are running
                            bool isConnected = false;
                            try
                            {
                                isConnected = _connectionService.TestConnection(connectionSettings);
                            }
                            catch
                            {
                                isConnected = false;
                            }
                            
                            if (isConnected)
                            {
                                lblConnectionStatus.Text = $"Connected to {connectionSettings.Host}:{connectionSettings.Port}";
                                lblConnectionStatus.ForeColor = Color.Green;
                            }
                            else
                            {
                                lblConnectionStatus.Text = $"Connection available to {connectionSettings.Host}:{connectionSettings.Port}";
                                lblConnectionStatus.ForeColor = Color.Orange;
                            }
                        }
                    }
                    else
                    {
                        lblConnectionStatus.Text = "No remote connection configured";
                        lblConnectionStatus.ForeColor = Color.Orange;
                    }
                }
                else
                {
                    lblConnectionStatus.Text = "Connection service unavailable";
                    lblConnectionStatus.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblConnectionStatus.Text = "Connection status check failed";
                lblConnectionStatus.ForeColor = Color.Red;
                ServiceLocator.LogService.LogError($"Error updating connection status: {ex.Message}", "UI");
            }
        }

        private void connectionSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormConnection connForm = new FormConnection())
            {
                if (connForm.ShowDialog() == DialogResult.OK)
                {
                    UpdateConnectionStatus();
                    
                    // Show notification for connection update
                    bool isConnected = _connectionService != null && _connectionService.IsConnected();
                    ConnectionSettings settings = _connectionService != null ? _connectionService.GetConnectionSettings() : null;
                    string serverName = settings != null ? settings.Host : "server";
                    
                    if (_notificationService != null)
                    {
                        _notificationService.ShowConnectionNotification(isConnected, serverName);
                    }
                }
            }
        }

        private void ResetApplicationToNewState()
        {
            try
            {
                ServiceLocator.LogService?.LogInfo("Starting application reset to fresh state...");

                // 1. Stop and remove all timer jobs
                ITimerJobManager timerJobManager = ServiceLocator.TimerJobManager;
                if (timerJobManager != null)
                {
                    List<long> runningJobs = timerJobManager.GetRegisteredTimerJobs();
                    foreach (long jobId in runningJobs)
                    {
                        // Stop the timer job first
                        timerJobManager.StopTimerJob(jobId);
                        // Then remove it completely
                        timerJobManager.RemoveTimerJob(jobId);
                    }
                    ServiceLocator.LogService?.LogInfo($"Removed {runningJobs.Count} timer jobs");
                }

                // 2. Clear all sync jobs from job service
                if (_jobService != null)
                {
                    List<SyncJob> allJobs = _jobService.GetAllJobs();
                    foreach (SyncJob job in allJobs)
                    {
                        _jobService.StopJob(job.Id);
                        _jobService.DeleteJob(job.Id);
                    }
                    ServiceLocator.LogService?.LogInfo($"Cleared {allJobs.Count} sync jobs");
                }

                // 3. Reset connection settings to empty/default
                if (_connectionService != null)
                {
                    ConnectionSettings emptySettings = new ConnectionSettings
                    {
                        Protocol = "LOCAL",
                        ProtocolType = 0, // 0 = Local
                        Host = "",
                        Port = 22,
                        Username = "",
                        SshKeyPath = "",
                        Timeout = 30,
                        IsConnected = false
                    };
                    _connectionService.SaveConnectionSettings(emptySettings);
                    ServiceLocator.LogService?.LogInfo("Reset connection settings to default empty state");
                }

                // 4. Clear logs
                ServiceLocator.LogService?.ClearLogs();
                ServiceLocator.LogService?.LogInfo("Cleared application logs");

                // 5. Clear timer jobs grid
                if (dgvTimerJobs.InvokeRequired)
                {
                    dgvTimerJobs.Invoke(new Action(() => dgvTimerJobs.Rows.Clear()));
                }
                else
                {
                    dgvTimerJobs.Rows.Clear();
                }

                // 6. Clear any logs or status displays
                ClearLogDisplays();
                
                // 7. Update UI status to show fresh state
                UpdateServiceStatus();
                UpdateConnectionStatus();

                // 8. Update running jobs label
                if (lblRunningTimerJobs != null)
                {
                    if (lblRunningTimerJobs.InvokeRequired)
                    {
                        lblRunningTimerJobs.Invoke(new Action(() => lblRunningTimerJobs.Text = "Active Timer Jobs: 0"));
                    }
                    else
                    {
                        lblRunningTimerJobs.Text = "Active Timer Jobs: 0";
                    }
                }

                ServiceLocator.LogService?.LogInfo("Application successfully reset to fresh state - ready for new configuration");
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService?.LogError("Error resetting application state: " + ex.Message);
                throw; // Re-throw to be caught by the calling method
            }
        }

        private void ClearLogDisplays()
        {
            try
            {
                // Clear any log-related controls if they exist
                if (Controls.ContainsKey("richTextBoxLogs"))
                {
                    var logControl = Controls["richTextBoxLogs"] as RichTextBox;
                    if (logControl != null)
                    {
                        if (logControl.InvokeRequired)
                            logControl.Invoke(new Action(() => logControl.Clear()));
                        else
                            logControl.Clear();
                    }
                }

                // Clear status labels
                if (Controls.ContainsKey("labelStatus"))
                {
                    var statusLabel = Controls["labelStatus"] as Label;
                    if (statusLabel != null)
                    {
                        if (statusLabel.InvokeRequired)
                            statusLabel.Invoke(new Action(() => statusLabel.Text = "Ready"));
                        else
                            statusLabel.Text = "Ready";
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService?.LogError("Error clearing log displays: " + ex.Message);
            }
        }

        private void newConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Ask for confirmation before resetting
                DialogResult result = MessageBox.Show(
                    "This will clear all current jobs, connections, and configurations to start fresh.\n\n" +
                    "Are you sure you want to create a new configuration?", 
                    "New Configuration", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Reset application to new instance state
                    ResetApplicationToNewState();

                    // Show confirmation that application has been reset
                    MessageBox.Show("Application has been reset to a fresh start.\n\nYou can now configure new connections and jobs.", 
                        "New Configuration Created", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error resetting application: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService?.LogError("Error resetting application: " + ex.Message);
            }
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Ask user whether to load from saved configurations or from file
                var result = MessageBox.Show(
                    "How would you like to load the configuration?\n\n" +
                    "• Click 'Yes' to browse saved configurations (internal database)\n" +
                    "• Click 'No' to load from a custom file (*.json)\n" +
                    "• Click 'Cancel' to return to main menu",
                    "Load Configuration", 
                    MessageBoxButtons.YesNoCancel, 
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Load from saved configurations (existing functionality)
                    LoadFromSavedConfigurations();
                }
                else if (result == DialogResult.No)
                {
                    // Load from custom file
                    LoadFromCustomFile();
                }
                // If Cancel, just return without doing anything
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening configuration loader: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService?.LogError("Error in load configuration: " + ex.Message);
            }
        }

        private void LoadFromSavedConfigurations()
        {
            try
            {
                // Open the Configuration Manager (FormSimpleLoadConfiguration)
                using (var configManager = new Forms.FormSimpleLoadConfiguration(
                    ServiceLocator.SavedJobConfigurationService))
                {
                    if (configManager.ShowDialog() == DialogResult.OK)
                    {
                        if (configManager.SelectedConfiguration != null)
                        {
                            // Check if user selected Load & Start option
                            if (configManager.LoadAndStart)
                            {
                                // Configuration will be loaded and started
                                ApplyLoadedConfiguration(configManager.SelectedConfiguration, true);
                                
                                // Count jobs for message
                                int jobCount = 0;
                                if (configManager.SelectedConfiguration.Jobs != null)
                                {
                                    jobCount = configManager.SelectedConfiguration.Jobs.Count;
                                }
                                else if (configManager.SelectedConfiguration.JobSettings != null)
                                {
                                    jobCount = 1;
                                }
                                
                                string message = jobCount > 1 
                                    ? $"Configuration loaded and {jobCount} jobs started successfully!"
                                    : "Configuration loaded and job started successfully!";
                                    
                                MessageBox.Show(message, "Success", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                // Configuration was just loaded (not started) - apply without starting
                                ApplyLoadedConfiguration(configManager.SelectedConfiguration, false);
                                
                                // Count jobs for message
                                int jobCount = 0;
                                if (configManager.SelectedConfiguration.Jobs != null)
                                {
                                    jobCount = configManager.SelectedConfiguration.Jobs.Count;
                                }
                                else if (configManager.SelectedConfiguration.JobSettings != null)
                                {
                                    jobCount = 1;
                                }
                                
                                string message = jobCount > 1 
                                    ? $"Configuration loaded successfully! {jobCount} jobs are ready to start."
                                    : "Configuration loaded successfully! Job is ready to start.";
                                    
                                MessageBox.Show(message, "Success", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        
                        // Refresh timer jobs grid to show any loaded/started jobs
                        RefreshTimerJobsGrid();
                        
                        // Update Quick Launch menu visibility in case configurations were loaded
                        UpdateQuickLaunchMenuVisibility();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading from saved configurations: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService?.LogError("Error loading saved configurations: " + ex.Message);
            }
        }

        private void LoadFromCustomFile()
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "Load FTPSyncer Configuration From File";
                    openFileDialog.Filter = "FTPSyncer Configuration Files (*.json)|*.json|All Files (*.*)|*.*";
                    openFileDialog.DefaultExt = "json";
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.CheckPathExists = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Load and apply configuration from selected file
                        LoadConfigurationFromFile(openFileDialog.FileName);
                        
                        MessageBox.Show($"Configuration loaded successfully from:\n{openFileDialog.FileName}", 
                            "Load Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Refresh timer jobs grid to show loaded configuration
                        RefreshTimerJobsGrid();
                        
                        // Update Quick Launch menu visibility
                        UpdateQuickLaunchMenuVisibility();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration from file: {ex.Message}", "Load Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService?.LogError($"Error loading configuration from custom file: {ex.Message}");
            }
        }

        private void LoadConfigurationFromFile(string filePath)
        {
            try
            {
                // Read and parse JSON file
                var json = System.IO.File.ReadAllText(filePath);
                var configDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (configDict == null)
                {
                    throw new Exception("Invalid configuration file format. Unable to parse JSON.");
                }

                // Extract connection settings
                ConnectionSettings connectionSettings = null;
                if (configDict.ContainsKey("Connection"))
                {
                    var connJson = configDict["Connection"].ToString();
                    var connDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(connJson);
                    
                    if (connDict != null)
                    {
                        connectionSettings = new ConnectionSettings
                        {
                            Protocol = GetStringValue(connDict, "Protocol") ?? "FTP",
                            Host = GetStringValue(connDict, "Host") ?? "",
                            Port = GetIntValue(connDict, "Port", 21),
                            Username = GetStringValue(connDict, "Username") ?? "",
                            UsePassiveMode = GetBoolValue(connDict, "UsePassiveMode", true),
                            UseSftp = GetBoolValue(connDict, "UseSftp", false),
                            UseKeyAuthentication = GetBoolValue(connDict, "UseKeyAuthentication", false),
                            SshKeyPath = GetStringValue(connDict, "SshKeyPath") ?? "",
                            EnableSsl = GetBoolValue(connDict, "EnableSsl", false),
                            Timeout = GetIntValue(connDict, "Timeout", 30),
                            OperationTimeout = GetIntValue(connDict, "OperationTimeout", 60)
                        };
                    }
                }

                // Extract job settings
                SyncJob jobSettings = null;
                if (configDict.ContainsKey("Job"))
                {
                    var jobJson = configDict["Job"].ToString();
                    var jobDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(jobJson);
                    
                    if (jobDict != null)
                    {
                        jobSettings = new SyncJob
                        {
                            Name = GetStringValue(jobDict, "Name") ?? "Imported Configuration",
                            Description = GetStringValue(jobDict, "Description") ?? "Configuration loaded from file",
                            SourcePath = GetStringValue(jobDict, "SourcePath") ?? "",
                            DestinationPath = GetStringValue(jobDict, "DestinationPath") ?? "",
                            TransferMode = GetStringValue(jobDict, "TransferMode") ?? "Copy (Keep both files)",
                            IntervalValue = GetIntValue(jobDict, "IntervalValue", 30),
                            IntervalType = GetStringValue(jobDict, "IntervalType") ?? "Minutes",
                            IncludeSubFolders = GetBoolValue(jobDict, "IncludeSubFolders", true),
                            OverwriteExisting = GetBoolValue(jobDict, "OverwriteExisting", true),
                            IsEnabled = GetBoolValue(jobDict, "IsEnabled", true),
                            DeleteSourceAfterTransfer = GetBoolValue(jobDict, "DeleteSourceAfterTransfer", false),
                            StartTime = GetDateTimeValue(jobDict, "StartTime", DateTime.Now)
                        };
                    }
                }

                if (jobSettings == null)
                {
                    throw new Exception("Invalid configuration file format. Missing Job data.");
                }

                // Apply loaded settings to the application
                ApplyLoadedConfigurationFromFile(jobSettings, connectionSettings);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse configuration file: {ex.Message}", ex);
            }
        }

        private void ApplyLoadedConfigurationFromFile(SyncJob job, ConnectionSettings connection)
        {
            try
            {
                var connectionService = ServiceLocator.ConnectionService;
                var timerJobManager = ServiceLocator.TimerJobManager;

                // Save connection settings (equivalent to setting them)
                connectionService.SaveConnectionSettings(connection);
                
                // Test connection
                var isConnected = connectionService.TestConnection(connection);
                if (!isConnected)
                {
                    MessageBox.Show("Warning: Could not establish connection with the loaded settings. " +
                        "Please verify the connection details in the Connection Settings.", 
                        "Connection Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Create and add timer job using RegisterTimerJob method
                long jobId = DateTime.Now.Ticks; // Generate unique job ID
                bool isUpload = job.TransferMode.Contains("Upload") || job.TransferMode.Contains("Local");
                double intervalMs = ConvertToMilliseconds(job.IntervalValue, job.IntervalType);
                
                if (isUpload)
                {
                    timerJobManager.RegisterTimerJob(
                        jobId,
                        job.Name ?? "Loaded Configuration",
                        job.SourcePath,
                        job.DestinationPath,
                        intervalMs,
                        job.IncludeSubFolders,
                        job.DeleteSourceAfterTransfer
                    );
                }
                else
                {
                    // For download jobs, we might need different handling
                    // For now, treat them as upload jobs with reversed paths
                    timerJobManager.RegisterTimerJob(
                        jobId,
                        job.Name ?? "Loaded Configuration",
                        job.SourcePath,
                        job.DestinationPath,
                        intervalMs,
                        job.IncludeSubFolders,
                        job.DeleteSourceAfterTransfer
                    );
                }

                ServiceLocator.LogService?.LogInfo($"Configuration loaded from file: {job.Name}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to apply loaded configuration: {ex.Message}", ex);
            }
        }

        // Helper methods for .NET 3.5 compatibility
        private string GetStringValue(Dictionary<string, object> dict, string key)
        {
            return dict.ContainsKey(key) && dict[key] != null ? dict[key].ToString() : null;
        }

        private int GetIntValue(Dictionary<string, object> dict, string key, int defaultValue)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                int result;
                if (int.TryParse(dict[key].ToString(), out result))
                    return result;
            }
            return defaultValue;
        }

        private bool GetBoolValue(Dictionary<string, object> dict, string key, bool defaultValue)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                bool result;
                if (bool.TryParse(dict[key].ToString(), out result))
                    return result;
            }
            return defaultValue;
        }

        private DateTime GetDateTimeValue(Dictionary<string, object> dict, string key, DateTime defaultValue)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                DateTime result;
                if (DateTime.TryParse(dict[key].ToString(), out result))
                    return result;
            }
            return defaultValue;
        }

        private double ConvertToMilliseconds(int intervalValue, string intervalType)
        {
            switch (intervalType.ToLower())
            {
                case "seconds":
                    return intervalValue * 1000;
                case "minutes":
                    return intervalValue * 60 * 1000;
                case "hours":
                    return intervalValue * 60 * 60 * 1000;
                case "days":
                    return intervalValue * 24 * 60 * 60 * 1000;
                default:
                    return intervalValue * 60 * 1000; // Default to minutes
            }
        }

        private void sshKeyGenerationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var keyGenForm = new Forms.FormComprehensiveConnection())
                {
                    // Set the form to open directly to the SSH Key Generation tab
                    keyGenForm.Text = "SSH Key Generation & Connection Settings";
                    keyGenForm.SetDefaultTab(1); // Tab index 1 is SSH Key Generation
                    keyGenForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening SSH key generation: " + ex.Message, 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void viewLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormLogs logsForm = new FormLogs())
            {
                logsForm.ShowDialog();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // This will close the application completely
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("FTPSyncer v1.0\nFile Synchronization Tool\n\nDeveloped for automated file transfers.", "About FTPSyncer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void jobRecoveryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Show information about automatic job recovery via Windows service
                string message = "Job Recovery is handled automatically by the FTPSyncer Windows Service.\n\n" +
                               "Features:\n" +
                               "• Jobs automatically resume after system restart\n" +
                               "• No manual intervention required\n" +
                               "• Service runs in background\n\n" +
                               "Service Status: " + GetServiceStatus();
                               
                MessageBox.Show(message, "Automatic Job Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking job recovery status: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        

        private void btnAddJob_Click(object sender, EventArgs e)
        {
            using (FormSchedule scheduleForm = new FormSchedule())
            {
                if (scheduleForm.ShowDialog() == DialogResult.OK)
                {
                    // Auto-start service when job is created
                    AutoStartServiceIfNeeded();
                    
                    // Refresh timer jobs grid to show the newly added job
                    RefreshTimerJobsGrid();
                    
                    // Show notification
                    if (_notificationService != null)
                    {
                        _notificationService.ShowNotification(
                            "Job Added",
                            "New synchronization job added and service started automatically.",
                            ToolTipIcon.Info);
                    }
                }
            }
        }

        /// <summary>
        /// Automatically starts the service if it's not running and there are jobs to run
        /// </summary>
        private void AutoStartServiceIfNeeded()
        {
            try
            {
                if (_serviceManager != null)
                {
                    if (!_serviceManager.IsServiceRunning())
                    {
                        ServiceLocator.LogService.LogInfo("Auto-starting service because job was created", "UI");
                        
                        if (_serviceManager.StartService())
                        {
                            ServiceLocator.LogService.LogInfo("Service auto-started successfully", "UI");
                            UpdateServiceStatus();
                            
                            if (_notificationService != null)
                            {
                                _notificationService.ShowNotification(
                                    "Service Auto-Started",
                                    "FTPSyncer service started automatically for new job.",
                                    ToolTipIcon.Info);
                            }
                        }
                        else
                        {
                            ServiceLocator.LogService.LogWarning("Failed to auto-start service", "UI");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError($"Error auto-starting service: {ex.Message}", "UI");
            }
        }





        private void btnStartStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (_serviceManager.IsServiceRunning())
                {
                    if (_serviceManager.StopService())
                    {
                        ServiceLocator.LogService.LogInfo("Service stopped by user");
                        
                        // Show notification
                        if (_notificationService != null)
                        {
                            _notificationService.ShowServiceStatusNotification(false);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to stop service", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    if (_serviceManager.StartService())
                    {
                        ServiceLocator.LogService.LogInfo("Service started by user");
                        
                        // Show notification
                        if (_notificationService != null)
                        {
                            _notificationService.ShowServiceStatusNotification(true);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to start service", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                UpdateServiceStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error managing service: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error managing service: " + ex.Message);
            }
        }








        
        // Handle form closing to minimize to system tray instead of exiting
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // We only want to intercept user-initiated closing
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Check if we have a system tray manager
                if (_trayManager != null)
                {
                    // Cancel the close and minimize instead
                    e.Cancel = true;
                    WindowState = FormWindowState.Minimized;
                }
                else
                {
                    // No system tray support, ask the user if they want to exit
                    DialogResult result = MessageBox.Show(
                        "Are you sure you want to exit FTPSyncer?", 
                        "Confirm Exit", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Question);
                        
                    if (result == DialogResult.No)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        // Stop service and cleanup resources
                        StopServiceAndCleanup();
                    }
                }
            }
            else
            {
                // For non-user closing (like Windows shutdown), cleanup resources
                StopServiceAndCleanup();
                base.OnFormClosing(e);
            }
        }
        
        private void StopServiceAndCleanup()
        {
            try
            {
                // First stop the service
                if (_serviceManager.IsServiceRunning())
                {
                    _serviceManager.StopService();
                }
                
                // Dispose of service manager
                _serviceManager.Dispose();
                
                // Cleanup system tray resources
                if (_trayManager != null)
                {
                    _trayManager.Dispose();
                }
                
                // If needed, stop any timers here
                
                ServiceLocator.LogService.LogInfo("Application closed, all resources cleaned up", "FormMain");
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Error during application shutdown: " + ex.Message, "FormMain");
            }
        }
        
        // Handle minimizing to system tray
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            // When minimized and we have a system tray, hide the form
            if (WindowState == FormWindowState.Minimized && _trayManager != null)
            {
                Hide();
                // Show notification via tray manager instead of here
            }
        }
        
        // Full screen functionality
        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleFullScreen(true);
        }
        
        private void normalViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleFullScreen(false);
        }
        
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Handle F11 for full screen toggle
            if (keyData == Keys.F11)
            {
                ToggleFullScreen(!_isFullScreen);
                return true;
            }
            // Handle Escape to exit full screen
            else if (keyData == Keys.Escape && _isFullScreen)
            {
                ToggleFullScreen(false);
                return true;
            }
            
            return base.ProcessCmdKey(ref msg, keyData);
        }
        
        private void ToggleFullScreen(bool fullScreen)
        {
            if (fullScreen && !_isFullScreen)
            {
                // Save current state
                _previousWindowState = WindowState;
                _previousBorderStyle = FormBorderStyle;
                _previousMenuVisible = menuStrip1.Visible;
                
                // Set full screen properties
                menuStrip1.Visible = false;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                TopMost = true;
                
                _isFullScreen = true;
                
                // Update menu items
                fullScreenToolStripMenuItem.Enabled = false;
                normalViewToolStripMenuItem.Enabled = true;
            }
            else if (!fullScreen && _isFullScreen)
            {
                // Restore previous state
                TopMost = false;
                FormBorderStyle = _previousBorderStyle;
                WindowState = _previousWindowState;
                menuStrip1.Visible = _previousMenuVisible;
                
                _isFullScreen = false;
                
                // Update menu items
                fullScreenToolStripMenuItem.Enabled = true;
                normalViewToolStripMenuItem.Enabled = false;
            }
        }
        
        // Handle application exit
        private void HandleApplicationExit()
        {
            try
            {
                // Cleanup resources
                if (_trayManager != null)
                {
                    _trayManager.Dispose();
                }
                
                // Log the exit
                try
                {
                    ServiceLocator.LogService.LogInfo("Application exiting", "UI");
                }
                catch
                {
                    // Ignore logging errors during shutdown
                }
                
                // Exit the application
                Application.Exit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during application exit: " + ex.Message);
            }
        }
        
        #region Timer Jobs Management
        
        private void InitializeTimerJobsGrid()
        {
            dgvTimerJobs.Columns.Clear();
            dgvTimerJobs.Columns.Add("JobId", "ID");
            dgvTimerJobs.Columns.Add("JobName", "Job Name");
            dgvTimerJobs.Columns.Add("FolderPath", "Folder Path");
            dgvTimerJobs.Columns.Add("RemotePath", "Remote Path");
            dgvTimerJobs.Columns.Add("Interval", "Interval");
            dgvTimerJobs.Columns.Add("LastUpload", "Last Transfer");
            dgvTimerJobs.Columns.Add("Status", "Status");
            
            dgvTimerJobs.Columns["JobId"].Width = 40;
            dgvTimerJobs.Columns["JobName"].Width = 120;
            dgvTimerJobs.Columns["FolderPath"].Width = 250;
            dgvTimerJobs.Columns["RemotePath"].Width = 200;
            dgvTimerJobs.Columns["Interval"].Width = 100;
            dgvTimerJobs.Columns["LastUpload"].Width = 150;
            dgvTimerJobs.Columns["Status"].Width = 80;
            
            // Add selection changed event
            dgvTimerJobs.SelectionChanged += dgvTimerJobs_SelectionChanged;
            
            // Add double-click event for job details popup
            dgvTimerJobs.CellDoubleClick += dgvTimerJobs_CellDoubleClick;
            
            // Initially disable action buttons (job-specific buttons only)
            btnStopTimerJob.Enabled = false;
            btnEditTimerJob.Enabled = false;
            btnDeleteTimerJob.Enabled = false;
            btnResumeTimerJob.Enabled = false;
        }
        
        private void dgvTimerJobs_SelectionChanged(object sender, EventArgs e)
        {
            // Enable or disable buttons based on selection
            bool hasSelection = dgvTimerJobs.SelectedRows.Count > 0;
            btnStopTimerJob.Enabled = hasSelection;
            btnEditTimerJob.Enabled = hasSelection;
            btnDeleteTimerJob.Enabled = hasSelection;
            btnResumeTimerJob.Enabled = hasSelection;
        }
        
        private void dgvTimerJobs_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Check if a valid row was double-clicked (not header)
                if (e.RowIndex < 0) return;
                
                // Get the job ID from the selected row
                DataGridViewRow row = dgvTimerJobs.Rows[e.RowIndex];
                object jobIdObj = row.Cells["JobId"].Value;
                
                if (jobIdObj == null) return;
                
                long jobId;
                if (!long.TryParse(jobIdObj.ToString(), out jobId)) return;
                
                // Check if the job is actually running
                ITimerJobManager timerJobManager = ServiceLocator.TimerJobManager;
                if (timerJobManager == null)
                {
                    MessageBox.Show("Timer job manager is not available.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                bool isRunning = timerJobManager.IsTimerJobRunning(jobId);
                if (!isRunning)
                {
                    MessageBox.Show("This job is not currently running. Job details are only available for running jobs.", 
                        "Job Not Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                // Open the job details popup
                using (var detailsForm = new Forms.JobDetailsPopup(jobId))
                {
                    detailsForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening job details: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Log the error
                ServiceLocator.LogService?.LogError("Error opening job details popup: " + ex.Message, "UI");
            }
        }
        
        private void RefreshTimerJobsGrid()
        {
            try
            {
                // Clear existing rows
                dgvTimerJobs.Rows.Clear();
                
                // Get timer jobs from the manager
                ITimerJobManager timerJobManager = ServiceLocator.TimerJobManager;
                if (timerJobManager == null) return;
                
                List<long> runningJobs = timerJobManager.GetRegisteredTimerJobs();
                lblRunningTimerJobs.Text = string.Format("Active Timer Jobs: {0}", runningJobs.Count);
                
                // Get job details directly from TimerJobManager
                foreach (long jobId in runningJobs)
                {
                    // Get job status and details from TimerJobManager
                    bool isRunning = timerJobManager.IsTimerJobRunning(jobId);
                    bool isDownloadJob = timerJobManager.IsTimerJobDownloadJob(jobId);
                    DateTime? lastUpload = timerJobManager.GetLastUploadTime(jobId);
                    DateTime? lastDownload = timerJobManager.GetLastDownloadTime(jobId);
                    string folderPath = timerJobManager.GetTimerJobFolderPath(jobId);
                    string remotePath = timerJobManager.GetTimerJobRemotePath(jobId);
                    string jobName = timerJobManager.GetTimerJobName(jobId);
                    double intervalMs = timerJobManager.GetTimerJobInterval(jobId);
                    
                    // Convert interval to readable format
                    string interval = "";
                    if (intervalMs > 0)
                    {
                        if (intervalMs < 60000) // Less than a minute
                        {
                            interval = (intervalMs / 1000).ToString("0") + " Seconds";
                        }
                        else if (intervalMs < 3600000) // Less than an hour
                        {
                            interval = (intervalMs / 60000).ToString("0") + " Minutes";
                        }
                        else // Hours
                        {
                            interval = (intervalMs / 3600000).ToString("0.0") + " Hours";
                        }
                    }
                    
                    int rowIndex = dgvTimerJobs.Rows.Add();
                    DataGridViewRow row = dgvTimerJobs.Rows[rowIndex];
                    
                    row.Cells["JobId"].Value = jobId;
                    row.Cells["JobName"].Value = jobName;
                    row.Cells["FolderPath"].Value = folderPath;
                    row.Cells["RemotePath"].Value = remotePath;
                    row.Cells["Interval"].Value = interval;
                    
                    // Show appropriate last transfer time based on job type
                    if (isDownloadJob)
                    {
                        row.Cells["LastUpload"].Value = lastDownload.HasValue ? lastDownload.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Never";
                    }
                    else
                    {
                        row.Cells["LastUpload"].Value = lastUpload.HasValue ? lastUpload.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Never";
                    }
                    
                    // Check if currently transferring and show appropriate status
                    bool isUploading = timerJobManager.IsTimerJobUploading(jobId);
                    bool isDownloading = timerJobManager.IsTimerJobDownloading(jobId);
                    DateTime? uploadStartTime = timerJobManager.GetTimerJobUploadStartTime(jobId);
                    DateTime? downloadStartTime = timerJobManager.GetTimerJobDownloadStartTime(jobId);
                    
                    string status = "Stopped";
                    if (isRunning)
                    {
                        if (isDownloadJob)
                        {
                            if (isDownloading && downloadStartTime.HasValue)
                            {
                                TimeSpan downloadDuration = DateTime.Now - downloadStartTime.Value;
                                status = string.Format("Downloading ({0:mm\\:ss})", downloadDuration);
                            }
                            else
                            {
                                status = "Running";
                            }
                        }
                        else
                        {
                            if (isUploading && uploadStartTime.HasValue)
                            {
                                TimeSpan uploadDuration = DateTime.Now - uploadStartTime.Value;
                                status = string.Format("Uploading ({0:mm\\:ss})", uploadDuration);
                            }
                            else
                            {
                                status = "Running";
                            }
                        }
                    }
                    
                    row.Cells["Status"].Value = status;
                    
                    // Set row color based on status
                    if (isUploading)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightBlue; // Blue for uploading
                    }
                    else if (isDownloading)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCyan; // Light cyan for downloading
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = isRunning ? Color.LightGreen : Color.LightGray;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading timer jobs: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error loading timer jobs: " + ex.Message);
            }
        }
        
        private void btnStopTimerJob_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvTimerJobs.SelectedRows.Count == 0) return;
                
                // Get selected job ID
                DataGridViewRow selectedRow = dgvTimerJobs.SelectedRows[0];
                long jobId = Convert.ToInt64(selectedRow.Cells["JobId"].Value);
                
                // Get the timer job manager
                ITimerJobManager timerJobManager = ServiceLocator.TimerJobManager;
                if (timerJobManager == null) return;
                
                // Stop the job
                if (timerJobManager.StopTimerJob(jobId))
                {
                    MessageBox.Show("Timer job stopped successfully.", "Job Stopped", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshTimerJobsGrid();
                }
                else
                {
                    MessageBox.Show("Failed to stop timer job.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error stopping timer job: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try { ServiceLocator.LogService.LogError("Error stopping timer job: " + ex.Message); } catch { }
            }
        }
        
        private void btnEditTimerJob_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvTimerJobs.SelectedRows.Count == 0) return;
                
                // Get selected job ID and details
                DataGridViewRow selectedRow = dgvTimerJobs.SelectedRows[0];
                long jobId = Convert.ToInt64(selectedRow.Cells["JobId"].Value);
                
                // Get the timer job manager
                ITimerJobManager timerJobManager = ServiceLocator.TimerJobManager;
                if (timerJobManager == null) 
                {
                    MessageBox.Show("Timer job manager is not available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Get job details from TimerJobManager
                string jobName = timerJobManager.GetTimerJobName(jobId);
                string folderPath = timerJobManager.GetTimerJobFolderPath(jobId);
                string remotePath = timerJobManager.GetTimerJobRemotePath(jobId);
                double intervalMs = timerJobManager.GetTimerJobInterval(jobId);
                bool isRunning = timerJobManager.IsTimerJobRunning(jobId);
                DateTime? lastUpload = timerJobManager.GetLastUploadTime(jobId);
                
                // Get additional job settings from TimerJobManager
                bool isDownloadJob = timerJobManager.IsTimerJobDownloadJob(jobId);
                bool includeSubfolders = timerJobManager.GetTimerJobIncludeSubfolders(jobId);
                bool deleteSourceAfterTransfer = timerJobManager.GetTimerJobDeleteSourceAfterTransfer(jobId);
                bool enableFilters = timerJobManager.GetTimerJobEnableFilters(jobId);
                List<string> includeExtensions = timerJobManager.GetTimerJobIncludeExtensions(jobId);
                List<string> excludeExtensions = timerJobManager.GetTimerJobExcludeExtensions(jobId);
                DateTime? lastDownload = timerJobManager.GetLastDownloadTime(jobId);
                
                // Create a SyncJob object to store the job settings
                SyncJob jobSettings = new SyncJob
                {
                    Id = (int)jobId,
                    Name = jobName ?? "Timer Job " + jobId,
                    SourcePath = folderPath ?? "",
                    DestinationPath = remotePath ?? "/",
                    IsEnabled = isRunning,
                    LastRun = isDownloadJob ? lastDownload : lastUpload,
                    TransferMode = isDownloadJob ? "Download" : "Upload",
                    IncludeSubFolders = includeSubfolders,
                    DeleteSourceAfterTransfer = deleteSourceAfterTransfer,
                    EnableFilters = enableFilters,
                    IncludeFileTypes = includeExtensions != null && includeExtensions.Count > 0 ? string.Join(",", includeExtensions.ToArray()) : "",
                    ExcludeFileTypes = excludeExtensions != null && excludeExtensions.Count > 0 ? string.Join(",", excludeExtensions.ToArray()) : ""
                };
                
                // Convert interval from milliseconds to appropriate display unit
                if (intervalMs >= 3600000) // 1 hour or more
                {
                    jobSettings.IntervalValue = (int)(intervalMs / 3600000);
                    jobSettings.IntervalType = "Hours";
                }
                else if (intervalMs >= 60000) // 1 minute or more
                {
                    jobSettings.IntervalValue = (int)(intervalMs / 60000);
                    jobSettings.IntervalType = "Minutes";
                }
                else // Less than 1 minute, use seconds
                {
                    jobSettings.IntervalValue = Math.Max(1, (int)(intervalMs / 1000));
                    jobSettings.IntervalType = "Seconds";
                }
                
                // Create a SavedJobConfiguration object for the FormEditConfiguration
                SavedJobConfiguration configToEdit = new SavedJobConfiguration
                {
                    Id = "timer_job_" + jobId,
                    Name = jobName ?? "Timer Job " + jobId,
                    Description = "Timer job configuration for " + (jobName ?? "Job " + jobId),
                    JobSettings = jobSettings,
                    CreatedDate = DateTime.Now,
                    LastUsed = DateTime.Now,
                    IsDefault = false,
                    Category = "Timer Jobs",
                    EnableQuickLaunch = false,
                    AutoStartOnLoad = false
                };
                
                // Initialize connection settings based on transfer mode
                if (configToEdit.SourceConnection == null)
                    configToEdit.SourceConnection = new SavedConnection();
                if (configToEdit.DestinationConnection == null)
                    configToEdit.DestinationConnection = new SavedConnection();
                
                if (isDownloadJob)
                {
                    // For downloads: source is remote, destination is local
                    configToEdit.SourceConnection.Settings = new ConnectionSettings
                    {
                        Host = "Remote Server", // Placeholder - will be configured in the form
                        Protocol = "SFTP",
                        Port = 22
                    };
                    configToEdit.DestinationConnection.Settings = new ConnectionSettings
                    {
                        Host = "localhost",
                        Protocol = "LOCAL"
                    };
                }
                else
                {
                    // For uploads: source is local, destination is remote
                    configToEdit.SourceConnection.Settings = new ConnectionSettings
                    {
                        Host = "localhost",
                        Protocol = "LOCAL"
                    };
                    configToEdit.DestinationConnection.Settings = new ConnectionSettings
                    {
                        Host = "Remote Server", // Placeholder - will be configured in the form
                        Protocol = "SFTP",
                        Port = 22
                    };
                }
                
                // Open the FormEditConfiguration in edit mode
                using (FormEditConfiguration editForm = new FormEditConfiguration(configToEdit, true)) // true = isTimerJob
                {
                    editForm.Text = "Edit Timer Job - " + jobName;
                    
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        // The job configuration has been updated, now we need to update the timer job
                        // Get the updated configuration from the form
                        var updatedJobSettings = configToEdit.JobSettings;
                        
                        // Convert the interval back to milliseconds for the timer job manager
                        double updatedIntervalMs = GetIntervalInMilliseconds(updatedJobSettings.IntervalValue, updatedJobSettings.IntervalType);
                        
                        // Parse the include/exclude extensions back to lists
                        List<string> updatedIncludeExtensions = new List<string>();
                        if (!string.IsNullOrEmpty(updatedJobSettings.IncludeFileTypes))
                        {
                            foreach (string ext in updatedJobSettings.IncludeFileTypes.Split(','))
                            {
                                string cleanExt = ext.Trim();
                                if (!string.IsNullOrEmpty(cleanExt))
                                    updatedIncludeExtensions.Add(cleanExt);
                            }
                        }
                        
                        List<string> updatedExcludeExtensions = new List<string>();
                        if (!string.IsNullOrEmpty(updatedJobSettings.ExcludeFileTypes))
                        {
                            foreach (string ext in updatedJobSettings.ExcludeFileTypes.Split(','))
                            {
                                string cleanExt = ext.Trim();
                                if (!string.IsNullOrEmpty(cleanExt))
                                    updatedExcludeExtensions.Add(cleanExt);
                            }
                        }
                        
                        // Update the timer job with the new settings
                        bool updateSuccess = timerJobManager.UpdateTimerJob(
                            jobId,
                            updatedJobSettings.Name,
                            updatedJobSettings.SourcePath,
                            updatedJobSettings.DestinationPath,
                            updatedIntervalMs,
                            updatedJobSettings.IncludeSubFolders,
                            updatedJobSettings.DeleteSourceAfterTransfer,
                            updatedJobSettings.EnableFilters,
                            updatedIncludeExtensions,
                            updatedExcludeExtensions
                        );
                        
                        if (updateSuccess)
                        {
                            // Refresh the grid to show the updated values
                            RefreshTimerJobsGrid();
                            
                            ServiceLocator.LogService.LogInfo(string.Format("Timer job '{0}' (ID: {1}) has been updated successfully", updatedJobSettings.Name, jobId));
                            
                            // Show confirmation message
                            MessageBox.Show(
                                "Timer job has been updated successfully!\n\nNote: The changes will take effect after the job is restarted.",
                                "Job Updated",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to update the timer job. Please check the log for details.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error editing timer job: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try { ServiceLocator.LogService.LogError("Error editing timer job: " + ex.Message); } catch { }
            }
        }

        private void btnDeleteTimerJob_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvTimerJobs.SelectedRows.Count == 0) return;

                // Get selected job ID and details
                DataGridViewRow selectedRow = dgvTimerJobs.SelectedRows[0];
                long jobId = Convert.ToInt64(selectedRow.Cells["JobId"].Value);
                string jobName = selectedRow.Cells["JobName"].Value?.ToString() ?? "Timer Job " + jobId;

                // Confirm deletion
                DialogResult result = MessageBox.Show(
                    string.Format("Are you sure you want to delete the timer job '{0}'?\n\nThis action cannot be undone.", jobName),
                    "Confirm Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes) return;

                // Get the timer job manager
                ITimerJobManager timerJobManager = ServiceLocator.TimerJobManager;
                if (timerJobManager == null)
                {
                    MessageBox.Show("Timer job manager is not available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Stop the job first if it's running
                if (timerJobManager.IsTimerJobRunning(jobId))
                {
                    timerJobManager.StopTimerJob(jobId);
                }

                // Delete the job
                if (timerJobManager.RemoveTimerJob(jobId))
                {
                    MessageBox.Show("Timer job deleted successfully.", "Job Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshTimerJobsGrid();
                    ServiceLocator.LogService.LogInfo(string.Format("Timer job '{0}' (ID: {1}) has been deleted", jobName, jobId));
                }
                else
                {
                    MessageBox.Show("Failed to delete timer job.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting timer job: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try { ServiceLocator.LogService.LogError("Error deleting timer job: " + ex.Message); } catch { }
            }
        }

        private void btnResumeTimerJob_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvTimerJobs.SelectedRows.Count == 0) return;

                // Get selected job ID and details
                DataGridViewRow selectedRow = dgvTimerJobs.SelectedRows[0];
                long jobId = Convert.ToInt64(selectedRow.Cells["JobId"].Value);
                string jobName = selectedRow.Cells["JobName"].Value?.ToString() ?? "Timer Job " + jobId;

                // Get the timer job manager
                ITimerJobManager timerJobManager = ServiceLocator.TimerJobManager;
                if (timerJobManager == null)
                {
                    MessageBox.Show("Timer job manager is not available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check if job is already running
                if (timerJobManager.IsTimerJobRunning(jobId))
                {
                    MessageBox.Show(string.Format("Timer job '{0}' is already running.", jobName), 
                        "Job Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Resume/start the job
                if (timerJobManager.StartTimerJob(jobId))
                {
                    MessageBox.Show("Timer job resumed successfully.", "Job Resumed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshTimerJobsGrid();
                    ServiceLocator.LogService.LogInfo(string.Format("Timer job '{0}' (ID: {1}) has been resumed", jobName, jobId));
                }
                else
                {
                    MessageBox.Show("Failed to resume timer job.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error resuming timer job: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try { ServiceLocator.LogService.LogError("Error resuming timer job: " + ex.Message); } catch { }
            }
        }
        
        #endregion

        #region Configuration Management Event Handlers

        private void saveConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we have a current connection to save
                var connectionService = ServiceLocator.ConnectionService;
                var configService = ServiceLocator.SavedJobConfigurationService;
                
                // Get current connection settings
                var currentConnection = connectionService.GetConnectionSettings();
                if (currentConnection == null)
                {
                    MessageBox.Show("No connection settings found. Please configure a connection first.", 
                        "No Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if there are timer jobs to save
                if (dgvTimerJobs.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one timer job before saving configuration.", "No Timer Jobs", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Always save ALL jobs when multiple exist (simplified user experience)
                if (dgvTimerJobs.Rows.Count > 1)
                {
                    // Create multi-job configuration with ALL jobs
                    var multiJobConfig = CreateMultiJobConfigurationFromCurrentState();
                    if (multiJobConfig != null)
                    {
                        
                        // Simple input dialog for multi-job configuration name using built-in method
                        var inputResult = PromptForConfigurationName(
                            $"Enter a name for your multi-job configuration:\n\n" +
                            $"This configuration will contain {multiJobConfig.Jobs.Count} jobs that will run in parallel.\n" +
                            $"Jobs: {string.Join(", ", multiJobConfig.Jobs.Take(3).Select(j => j.Name).ToArray())}" +
                            $"{(multiJobConfig.Jobs.Count > 3 ? "..." : "")}",
                            multiJobConfig.Name);
                            
                        if (string.IsNullOrEmpty(inputResult) || inputResult.Trim().Length == 0)
                        {
                            return; // User cancelled
                        }
                        
                        // Update the configuration name
                        string configName = inputResult.Trim();
                        multiJobConfig.Name = configName;
                        multiJobConfig.Description = $"Multi-job configuration with {multiJobConfig.Jobs.Count} jobs created on {DateTime.Now:yyyy-MM-dd HH:mm}";
                        
                        var saveResult = configService.SaveConfigurationWithResult(multiJobConfig);
                        if (saveResult.Result == SaveConfigurationResult.Success)
                        {
                            MessageBox.Show($"Configuration saved successfully!\n\n" +
                                $"• Name: {multiJobConfig.Name}\n" +
                                $"• Jobs: {multiJobConfig.Jobs.Count} (will run in parallel)\n" +
                                $"• Access from: File → Load Configuration or Quick Launch", 
                                "Multi-Job Configuration Saved", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            // Update Quick Launch menu visibility since we now have configurations
                            UpdateQuickLaunchMenuVisibility();
                            return;
                        }
                        else if (saveResult.Result == SaveConfigurationResult.NameConflict)
                        {
                            var result = MessageBox.Show(
                                $"A configuration with the name '{configName}' already exists.\n\n" +
                                "Do you want to overwrite it?",
                                "Name Conflict",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
                                
                            if (result == DialogResult.Yes)
                            {
                                // Try to overwrite
                                if (configService.SaveConfigurationOverwrite(multiJobConfig))
                                {
                                    MessageBox.Show($"Configuration '{configName}' updated successfully!\n\n" +
                                        $"• Jobs: {multiJobConfig.Jobs.Count} (will run in parallel)",
                                        "Configuration Updated", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    
                                    UpdateQuickLaunchMenuVisibility();
                                    return;
                                }
                                else
                                {
                                    MessageBox.Show("Failed to overwrite configuration. Please try again.", 
                                        "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else
                        {
                            string errorDetails = saveResult.Message ?? "Unknown error";
                            ServiceLocator.LogService?.LogError($"Failed to save multi-job configuration: {errorDetails}", "UI");
                            
                            MessageBox.Show($"Failed to save configuration:\n\n{errorDetails}\n\nCheck the logs for more details.", 
                                "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to create multi-job configuration from current timer jobs.", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                
                // Create single-job configuration (either user chose single-job or only 1 timer job exists)
                var currentJob = CreateJobFromCurrentState();
                
                // Open the save configuration form
                using (var saveForm = new Forms.FormSaveJobConfiguration(
                    currentJob, 
                    currentConnection, 
                    currentConnection, // Using same connection for both source and destination
                    configService,
                    connectionService))
                {
                    if (saveForm.ShowDialog() == DialogResult.OK)
                    {
                        MessageBox.Show("Configuration saved successfully! You can access it from Quick Launch menu.", "Save Successful", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Update Quick Launch menu visibility since we now have configurations
                        UpdateQuickLaunchMenuVisibility();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Save Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError($"Error saving configuration: {ex.Message}", "UI");
            }
        }

        private void saveAsConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we have a current connection to save
                var connectionService = ServiceLocator.ConnectionService;
                var configService = ServiceLocator.SavedJobConfigurationService;
                
                // Get current connection settings
                var currentConnection = connectionService.GetConnectionSettings();
                if (currentConnection == null)
                {
                    MessageBox.Show("No connection settings found. Please configure a connection first.", 
                        "No Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if there are timer jobs to save
                if (dgvTimerJobs.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one timer job before saving configuration.", "No Timer Jobs", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // If user has multiple timer jobs, suggest using the built-in save configuration instead
                if (dgvTimerJobs.Rows.Count > 1)
                {
                    var result = MessageBox.Show(
                        $"You have {dgvTimerJobs.Rows.Count} timer jobs in your list.\n\n" +
                        "For multiple timer jobs, we recommend using 'Save Configuration' instead of 'Save As' to create a proper multi-job configuration that will run all jobs in parallel.\n\n" +
                        "• Click YES to continue with 'Save As' (will only save the first job to file)\n" +
                        "• Click NO to cancel and use 'Save Configuration' instead",
                        "Multiple Timer Jobs - Consider Save Configuration",
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Information);
                        
                    if (result == DialogResult.No)
                        return;
                }
                
                // Create single-job configuration for file export
                var currentJob = CreateJobFromCurrentState();
                
                // Show SaveFileDialog to let user choose location and filename
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Title = "Save FTPSyncer Configuration As";
                    saveFileDialog.Filter = "FTPSyncer Configuration Files (*.json)|*.json|All Files (*.*)|*.*";
                    saveFileDialog.DefaultExt = "json";
                    saveFileDialog.FileName = $"FTPSyncer_Config_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
                    saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Save configuration to selected file
                        SaveConfigurationToFile(currentJob, currentConnection, saveFileDialog.FileName);
                        
                        MessageBox.Show($"Configuration saved successfully to:\n{saveFileDialog.FileName}", 
                            "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Save Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError($"Error saving configuration as file: {ex.Message}", "UI");
            }
        }

        private void SaveConfigurationToFile(SyncJob job, ConnectionSettings connection, string filePath)
        {
            try
            {
                // Create a configuration object similar to what FormSaveJobConfiguration creates
                var config = new
                {
                    Id = Guid.NewGuid().ToString(),
                    DisplayName = job.Name ?? "FTPSyncer Configuration",
                    Description = job.Description ?? "Configuration exported from FTPSyncer",
                    CreatedDate = DateTime.Now,
                    LastModified = DateTime.Now,
                    Version = "1.0",
                    ApplicationName = "FTPSyncer",
                    Job = new
                    {
                        Name = job.Name,
                        Description = job.Description,
                        SourcePath = job.SourcePath,
                        DestinationPath = job.DestinationPath,
                        TransferMode = job.TransferMode,
                        IntervalValue = job.IntervalValue,
                        IntervalType = job.IntervalType,
                        IncludeSubFolders = job.IncludeSubFolders,
                        OverwriteExisting = job.OverwriteExisting,
                        IsEnabled = job.IsEnabled,
                        StartTime = job.StartTime,
                        DeleteSourceAfterTransfer = job.DeleteSourceAfterTransfer
                    },
                    Connection = new
                    {
                        Protocol = connection.Protocol,
                        Host = connection.Host,
                        Port = connection.Port,
                        Username = connection.Username,
                        // Don't save password in plain text for security
                        UsePassiveMode = connection.UsePassiveMode,
                        UseSftp = connection.UseSftp,
                        UseKeyAuthentication = connection.UseKeyAuthentication,
                        SshKeyPath = connection.SshKeyPath,
                        EnableSsl = connection.EnableSsl,
                        Timeout = connection.Timeout,
                        OperationTimeout = connection.OperationTimeout
                    },
                    // Add timestamp and version info
                    ExportInfo = new
                    {
                        ExportedBy = Environment.UserName,
                        ExportedOn = Environment.MachineName,
                        ExportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        FTPSyncerVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                    }
                };
                
                // Convert to JSON and save to file
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save configuration file: {ex.Message}", ex);
            }
        }

        private SyncJob CreateJobFromCurrentState()
        {
            try
            {
                // Get first timer job from grid as primary job (for backward compatibility with single-job saving)
                if (dgvTimerJobs.Rows.Count > 0)
                {
                    var firstRow = dgvTimerJobs.Rows[0];
                    var jobName = firstRow.Cells["JobName"].Value?.ToString() ?? "Saved Configuration";
                    var sourcePath = firstRow.Cells["FolderPath"].Value?.ToString() ?? "";
                    var remotePath = firstRow.Cells["RemotePath"].Value?.ToString() ?? "";
                    var intervalText = firstRow.Cells["Interval"].Value?.ToString() ?? "30 Minutes";
                    
                    // Parse interval
                    string intervalType = "Minutes";
                    int intervalValue = 30;
                    ParseIntervalText(intervalText, out intervalValue, out intervalType);
                    
                    return new SyncJob
                    {
                        Name = $"Configuration - {jobName}",
                        Description = dgvTimerJobs.Rows.Count > 1 
                            ? $"Configuration with {dgvTimerJobs.Rows.Count} timer jobs (first job only - use multi-job configuration for all)" 
                            : $"Saved configuration with 1 timer job",
                        SourcePath = sourcePath,
                        DestinationPath = remotePath,
                        IntervalValue = intervalValue,
                        IntervalType = intervalType,
                        TransferMode = "Copy (Keep both files)",
                        IncludeSubFolders = true,
                        OverwriteExisting = true,
                        IsEnabled = true
                    };
                }
                
                // Fallback if no jobs in grid
                return new SyncJob
                {
                    Name = "Current Configuration",
                    Description = "Configuration saved from main form",
                    SourcePath = "",
                    DestinationPath = "",
                    IntervalValue = 30,
                    IntervalType = "Minutes",
                    TransferMode = "Copy (Keep both files)",
                    IncludeSubFolders = true,
                    OverwriteExisting = true,
                    IsEnabled = true
                };
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService?.LogError("Error creating job from current state: " + ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Creates a multi-job configuration from all timer jobs in the grid
        /// </summary>
        private SavedJobConfiguration CreateMultiJobConfigurationFromCurrentState()
        {
            try
            {
                if (dgvTimerJobs.Rows.Count == 0)
                    return null;

                var jobs = new List<SyncJob>();
                var connectionService = ServiceLocator.ConnectionService;
                var currentConnection = connectionService.GetConnectionSettings();
                
                // Create a job for each row in the timer jobs grid
                for (int i = 0; i < dgvTimerJobs.Rows.Count; i++)
                {
                    var row = dgvTimerJobs.Rows[i];
                    var jobName = row.Cells["JobName"].Value?.ToString() ?? $"Job {i + 1}";
                    var sourcePath = row.Cells["FolderPath"].Value?.ToString() ?? "";
                    var remotePath = row.Cells["RemotePath"].Value?.ToString() ?? "";
                    var intervalText = row.Cells["Interval"].Value?.ToString() ?? "30 Minutes";
                    
                    // Parse interval
                    string intervalType = "Minutes";
                    int intervalValue = 30;
                    ParseIntervalText(intervalText, out intervalValue, out intervalType);
                    
                    var job = new SyncJob
                    {
                        Name = jobName,
                        Description = $"Timer job from main form grid (row {i + 1})",
                        SourcePath = sourcePath,
                        DestinationPath = remotePath,
                        IntervalValue = intervalValue,
                        IntervalType = intervalType,
                        TransferMode = "Copy (Keep both files)",
                        IncludeSubFolders = true,
                        OverwriteExisting = true,
                        IsEnabled = true,
                        CreatedDate = DateTime.Now,
                        
                        // Set connection settings for each individual job
                        SourceConnection = currentConnection,
                        DestinationConnection = currentConnection
                    };
                    
                    jobs.Add(job);
                }
                
                // Create a multi-job configuration
                var config = new SavedJobConfiguration
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"Multi-Job Configuration ({jobs.Count} jobs)",
                    Description = $"Multi-job configuration with {jobs.Count} timer jobs created from main form",
                    Category = "Multi-Job",
                    CreatedDate = DateTime.Now,
                    Jobs = jobs,
                    JobSettings = null, // Explicitly set to null for multi-job configurations
                    
                    // Set up connections
                    SourceConnection = new SavedConnection
                    {
                        Name = "Multi-Job Source Connection",
                        Description = "Source connection for multi-job configuration",
                        Settings = currentConnection,
                        CreatedDate = DateTime.Now
                    },
                    DestinationConnection = new SavedConnection
                    {
                        Name = "Multi-Job Destination Connection", 
                        Description = "Destination connection for multi-job configuration",
                        Settings = currentConnection,
                        CreatedDate = DateTime.Now
                    }
                };
                
                // Ensure compatibility
                config.EnsureJobsCompatibility();
                
                
                return config;
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService?.LogError($"Error creating multi-job configuration from current state: {ex.Message}");
                throw;
            }
        }

        private void ParseIntervalText(string intervalText, out int intervalValue, out string intervalType)
        {
            try
            {
                intervalValue = 30;
                intervalType = "Minutes";
                
                if (string.IsNullOrEmpty(intervalText))
                    return;
                
                var parts = intervalText.Split(' ');
                if (parts.Length >= 2)
                {
                    if (int.TryParse(parts[0], out int value))
                    {
                        intervalValue = value;
                        intervalType = parts[1].TrimEnd('s'); // Remove plural 's' if present
                    }
                }
            }
            catch
            {
                intervalValue = 30;
                intervalType = "Minutes";
            }
        }

        private void ApplyLoadedConfiguration(SavedJobConfiguration config, bool startAfterLoad)
        {
            try
            {
                if (config == null) return;
                
                // Ensure backward compatibility migration
                config.EnsureJobsCompatibility();
                
                var connectionService = ServiceLocator.ConnectionService;
                var timerJobManager = ServiceLocator.TimerJobManager;
                int jobsStarted = 0;
                int totalJobs = 0;

                // Determine which jobs to process
                List<SyncJob> jobsToProcess = new List<SyncJob>();
                
                if (config.Jobs != null && config.Jobs.Count > 0)
                {
                    // Multi-job configuration
                    jobsToProcess.AddRange(config.Jobs);
                    totalJobs = config.Jobs.Count;
                }
                else if (config.JobSettings != null)
                {
                    // Single job configuration (backward compatibility - should be rare after migration)
                    jobsToProcess.Add(config.JobSettings);
                    totalJobs = 1;
                }

                if (jobsToProcess.Count == 0)
                {
                    ServiceLocator.LogService.LogWarning("No jobs found in configuration", "UI");
                    return;
                }
                
                // Auto-start service when loading configuration with jobs
                if (startAfterLoad && totalJobs > 0)
                {
                    AutoStartServiceIfNeeded();
                }
                
                // Apply connection settings (use the first job's connection or the configuration's global connection)
                ConnectionSettings connectionToApply = null;
                
                if (config.SourceConnection?.Settings != null)
                {
                    connectionToApply = config.SourceConnection.Settings;
                }
                else if (jobsToProcess.Count > 0 && jobsToProcess[0].SourceConnection != null)
                {
                    connectionToApply = jobsToProcess[0].SourceConnection;
                }
                
                if (connectionToApply != null)
                {
                    connectionService.SaveConnectionSettings(connectionToApply);
                    ServiceLocator.LogService.LogInfo("Applied connection settings from configuration", "UI");
                    
                    // Small delay to ensure connection is properly applied (.NET 3.5 compatibility)
                    System.Threading.Thread.Sleep(100);
                }
                
                // Process each job
                if (startAfterLoad)
                {
                    // Prepare jobs for parallel registration and starting
                    var jobsToStart = new List<long>();
                    var jobIdMapping = new Dictionary<long, SyncJob>();
                    
                    foreach (var job in jobsToProcess)
                    {
                        try
                        {
                            // Skip disabled jobs
                            if (!job.IsEnabled)
                            {
                                ServiceLocator.LogService.LogInfo($"Skipping disabled job: {job.Name}", "UI");
                                continue;
                            }

                            // Register the timer job first
                            var intervalMs = GetIntervalInMilliseconds(job.IntervalValue, job.IntervalType);
                            var jobId = DateTime.Now.Ticks + jobsToStart.Count; // Use timestamp + counter as unique ID
                            
                            if (timerJobManager.RegisterTimerJob(
                                jobId,
                                job.Name ?? $"Job from {config.Name}",
                                job.SourcePath,
                                job.DestinationPath,
                                intervalMs,
                                job.IncludeSubFolders,
                                job.DeleteSourceAfterTransfer))
                            {
                                jobsToStart.Add(jobId);
                                jobIdMapping[jobId] = job;
                                ServiceLocator.LogService.LogInfo($"Registered job '{job.Name}' for parallel start", "UI");
                            }
                            else
                            {
                                ServiceLocator.LogService.LogError($"Failed to register job '{job.Name}'", "UI");
                            }
                        }
                        catch (Exception jobEx)
                        {
                            ServiceLocator.LogService.LogError($"Error processing job '{job.Name}': {jobEx.Message}", "UI");
                        }
                    }
                    
                    // Start all registered jobs in parallel
                    if (jobsToStart.Count > 0)
                    {
                        ServiceLocator.LogService.LogInfo($"Starting {jobsToStart.Count} jobs in parallel from configuration '{config.Name}'", "UI");
                        
                        var startResults = timerJobManager.StartMultipleTimerJobs(jobsToStart);
                        
                        // Log results for each job
                        foreach (var result in startResults)
                        {
                            var jobId = result.Key;
                            var success = result.Value;
                            
                            if (jobIdMapping.ContainsKey(jobId))
                            {
                                var job = jobIdMapping[jobId];
                                if (success)
                                {
                                    jobsStarted++;
                                    ServiceLocator.LogService.LogInfo($"Auto-started job '{job.Name}' from loaded configuration (parallel)", "UI");
                                }
                                else
                                {
                                    ServiceLocator.LogService.LogError($"Failed to start job '{job.Name}' (parallel)", "UI");
                                }
                            }
                        }
                    }
                    
                    ServiceLocator.LogService.LogInfo($"Parallel job start completed: {jobsStarted} out of {totalJobs} jobs from configuration '{config.Name}'", "UI");
                }
                
                // Update usage statistics
                var configService = ServiceLocator.SavedJobConfigurationService;
                configService.UpdateUsageStatistics(config.Id);
                
                // Show notification if enabled
                if (config.ShowNotificationOnStart && _notificationService != null)
                {
                    string notificationMessage;
                    if (totalJobs > 1)
                    {
                        notificationMessage = $"Configuration '{config.DisplayName}' loaded with {jobsStarted} of {totalJobs} jobs started.";
                    }
                    else
                    {
                        notificationMessage = $"Configuration '{config.DisplayName}' has been loaded successfully.";
                    }
                    
                    _notificationService.ShowNotification(
                        "Configuration Loaded",
                        notificationMessage,
                        ToolTipIcon.Info);
                }
                
                // Refresh the UI and update status displays
                RefreshTimerJobsGrid();
                UpdateConnectionStatus(); // Update connection status display
                UpdateServiceStatus(); // Update service status display
                
                // Force UI refresh for .NET 3.5 compatibility
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying configuration: {ex.Message}", "Apply Configuration Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError($"Error applying configuration: {ex.Message}", "UI");
            }
        }

        private double GetIntervalInMilliseconds(int intervalValue, string intervalType)
        {
            switch (intervalType?.ToLower())
            {
                case "second":
                case "seconds":
                    return intervalValue * 1000;
                case "minute":
                case "minutes":
                    return intervalValue * 60 * 1000;
                case "hour":
                case "hours":
                    return intervalValue * 60 * 60 * 1000;
                default:
                    return intervalValue * 60 * 1000; // Default to minutes
            }
        }

        #region Configuration Button Event Handlers

        private void btnLoadConfiguration_Click(object sender, EventArgs e)
        {
            try
            {
                using (var loadForm = new FormSimpleLoadConfiguration(_savedJobConfigService))
                {
                    if (loadForm.ShowDialog() == DialogResult.OK && loadForm.SelectedConfiguration != null)
                    {
                        var config = loadForm.SelectedConfiguration;
                        
                        // Apply the loaded configuration
                        ApplyLoadedConfiguration(config, loadForm.LoadAndStart);
                        
                        // Refresh any displays if needed
                        UpdateConnectionStatus();
                        UpdateServiceStatus();
                        
                        MessageBox.Show("Configuration '" + config.Name + "' loaded successfully!", "Load Configuration", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error loading configuration: {0}", ex.Message), 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Quick Launch Popup

        private FormQuickLaunch _quickLaunchForm;

        private void showQuickLaunchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowQuickLaunchPopup();
        }

        private void ShowQuickLaunchPopup()
        {
            try
            {
                // Check if there are any saved configurations first
                if (!HasSavedConfigurations())
                {
                    MessageBox.Show(
                        "No saved configurations found!\n\n" +
                        "To use Quick Launch, you need to:\n" +
                        "1. Set up your connection settings\n" +
                        "2. Create and configure timer jobs\n" +
                        "3. Save your configuration using File → Save\n\n" +
                        "After saving configurations, you can quickly access them here.",
                        "No Configurations Available", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);
                    return;
                }

                if (_quickLaunchForm == null || _quickLaunchForm.IsDisposed)
                {
                    _quickLaunchForm = new FormQuickLaunch(this);
                }

                // Position the popup at a nice location relative to the main form
                Point location = new Point(
                    this.Location.X + 50,
                    this.Location.Y + 50
                );

                _quickLaunchForm.ShowAtPosition(location);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing Quick Launch popup: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Checks if there are any saved configurations available
        /// </summary>
        private bool HasSavedConfigurations()
        {
            try
            {
                var configService = ServiceLocator.SavedJobConfigurationService;
                if (configService != null)
                {
                    var configs = configService.GetAllConfigurations();
                    return configs != null && configs.Count > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Updates the visibility and text of Quick Launch menu based on available configurations
        /// </summary>
        private void UpdateQuickLaunchMenuVisibility()
        {
            try
            {
                bool hasConfigs = HasSavedConfigurations();
                
                if (showQuickLaunchToolStripMenuItem != null)
                {
                    showQuickLaunchToolStripMenuItem.Enabled = hasConfigs;
                    
                    if (hasConfigs)
                    {
                        showQuickLaunchToolStripMenuItem.Text = "Quick Launch Panel";
                        showQuickLaunchToolStripMenuItem.ToolTipText = "Access your saved configurations quickly";
                    }
                    else
                    {
                        showQuickLaunchToolStripMenuItem.Text = "Quick Launch Panel (No configurations)";
                        showQuickLaunchToolStripMenuItem.ToolTipText = "Save configurations first to enable Quick Launch";
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService?.LogError("Error updating Quick Launch menu visibility: " + ex.Message);
            }
        }

        public void LoadConfigurationAndStart(SavedJobConfiguration configuration)
        {
            try
            {
                if (configuration == null)
                {
                    MessageBox.Show("Invalid configuration provided.", "Configuration Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Ensure backward compatibility migration
                configuration.EnsureJobsCompatibility();
                
                // Check if configuration has jobs (either single or multiple)
                bool hasJobs = false;
                int jobCount = 0;
                string jobSummary = "";

                if (configuration?.Jobs != null && configuration.Jobs.Count > 0)
                {
                    // Multi-job configuration
                    hasJobs = true;
                    jobCount = configuration.Jobs.Count;
                    
                    // Always show the actual count
                    if (jobCount > 1)
                    {
                        jobSummary = $"Loaded {jobCount} jobs from multi-job configuration '{configuration.Name}' (PARALLEL START)";
                    }
                    else
                    {
                        jobSummary = $"Loaded {jobCount} job from configuration '{configuration.Name}' (To add more jobs: Load Config → Edit → Add Job)";
                    }
                }
                else if (configuration?.JobSettings != null)
                {
                    // Single job configuration (backward compatibility - should be rare after migration)
                    hasJobs = true;
                    jobCount = 1;
                    jobSummary = $"Loaded single job '{configuration.JobSettings.Name}' from configuration '{configuration.Name}'";
                }

                if (hasJobs)
                {
                    ServiceLocator.LogService.LogInfo($"Quick Launch: Loading configuration '{configuration.Name}' with {jobCount} job(s)", "QuickLaunch");
                    
                    // Apply the loaded configuration
                    ApplyLoadedConfiguration(configuration, true);
                    
                    // Show success message with job details
                    string message = jobSummary + "\n\nJobs have been started successfully!";
                    
                    if (configuration.Jobs != null && configuration.Jobs.Count > 1)
                    {
                        message += "\n\n✓ All jobs are now running in PARALLEL";
                        message += "\n\nJob Details:";
                        for (int i = 0; i < Math.Min(configuration.Jobs.Count, 5); i++) // Show first 5 jobs
                        {
                            var job = configuration.Jobs[i];
                            message += $"\n• {job.Name ?? $"Job {i + 1}"}: {job.SourcePath} → {job.DestinationPath}";
                        }
                        if (configuration.Jobs.Count > 5)
                        {
                            message += $"\n... and {configuration.Jobs.Count - 5} more jobs";
                        }
                    }
                    else if (configuration.Jobs != null && configuration.Jobs.Count == 1)
                    {
                        var job = configuration.Jobs[0];
                        message += $"\n\nCurrent Job:";
                        message += $"\n• {job.Name ?? "Job 1"}: {job.SourcePath} → {job.DestinationPath}";
                        message += $"\n• Interval: {job.IntervalValue} {job.IntervalType}";
                        message += "\n\nTo add more jobs for parallel execution:";
                        message += "\n1. Stop current job";
                        message += "\n2. Use 'Load Configuration' (not 'Load & Start')";
                        message += "\n3. Right-click the configuration → 'Edit'";
                        message += "\n4. Click 'Add Job' to create additional jobs";
                        message += "\n5. Save and then 'Load & Start' again";
                    }
                    else if (configuration.JobSettings != null)
                    {
                        message += $"\nSource: {configuration.JobSettings.SourcePath}";
                        message += $"\nDestination: {configuration.JobSettings.DestinationPath}";
                        message += $"\nInterval: {configuration.JobSettings.IntervalValue} {configuration.JobSettings.IntervalType}";
                        message += "\n\nNote: This is a legacy single-job configuration.";
                    }
                    
                    MessageBox.Show(message, "Jobs Started Successfully", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    ServiceLocator.LogService.LogInfo($"Quick Launch: Successfully started {jobCount} job(s) from configuration '{configuration.Name}'", "QuickLaunch");
                }
                else
                {
                    MessageBox.Show("Invalid configuration: No job settings found.", "Configuration Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error loading and starting job configuration: {ex.Message}";
                ServiceLocator.LogService.LogError(errorMsg, "QuickLaunch");
                MessageBox.Show(errorMsg, "Quick Launch Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Bandwidth Control Methods
        
        private void InitializeBandwidthControl()
        {
            try
            {
                _bandwidthService = BandwidthControlService.Instance;
                
                // Load current settings
                LoadBandwidthSettings();
                
                // Set up the speed update timer
                _speedUpdateTimer = new Timer();
                _speedUpdateTimer.Interval = 2000; // Update every 2 seconds
                _speedUpdateTimer.Tick += SpeedUpdateTimer_Tick;
                _speedUpdateTimer.Start();
                
                // Subscribe to bandwidth settings changed event
                _bandwidthService.BandwidthSettingsChanged += BandwidthService_SettingsChanged;
                
                ServiceLocator.LogService.LogInfo("Bandwidth control initialized successfully", "UI");
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError($"Error initializing bandwidth control: {ex.Message}", "UI");
            }
        }
        
        /// <summary>
        /// Apply bandwidth limits to SFTP configuration based on transfer direction
        /// </summary>
        public void ApplyBandwidthLimitsToSftpConfig(syncer.core.Configuration.SftpConfiguration config, bool isUpload)
        {
            try
            {
                if (_bandwidthService != null)
                {
                    _bandwidthService.ApplyLimitsToSftpConfig(config, isUpload);
                    
                    ServiceLocator.LogService.LogInfo($"Applied bandwidth limits to SFTP config - Upload: {isUpload}, " +
                        $"Limit: {(config.BandwidthLimitBytesPerSecond > 0 ? (config.BandwidthLimitBytesPerSecond / 1024) + " KB/s" : "Unlimited")}", "BandwidthControl");
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError($"Error applying bandwidth limits: {ex.Message}", "BandwidthControl");
            }
        }
        
        /// <summary>
        /// Create SFTP configuration with bandwidth limits applied
        /// </summary>
        public syncer.core.Configuration.SftpConfiguration CreateBandwidthLimitedSftpConfig(bool isUpload)
        {
            var config = new syncer.core.Configuration.SftpConfiguration();
            ApplyBandwidthLimitsToSftpConfig(config, isUpload);
            return config;
        }
        
        private void LoadBandwidthSettings()
        {
            try
            {
                chkEnableBandwidthControl.Checked = _bandwidthService.IsBandwidthControlEnabled;
                numUploadLimit.Value = (decimal)_bandwidthService.GetUploadLimitKBps();
                numDownloadLimit.Value = (decimal)_bandwidthService.GetDownloadLimitKBps();
                
                UpdateBandwidthControlsState();
                UpdateSpeedLabels();
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError($"Error loading bandwidth settings: {ex.Message}", "UI");
            }
        }
        
        private void UpdateBandwidthControlsState()
        {
            bool enabled = chkEnableBandwidthControl.Checked;
            lblUploadLimit.Enabled = enabled;
            numUploadLimit.Enabled = enabled;
            lblUploadUnit.Enabled = enabled;
            lblDownloadLimit.Enabled = enabled;
            numDownloadLimit.Enabled = enabled;
            lblDownloadUnit.Enabled = enabled;
        }
        
        private void UpdateSpeedLabels()
        {
            try
            {
                // Get actual current transfer speeds from the bandwidth service
                string currentUploadSpeed = _bandwidthService.GetCurrentUploadSpeedFormatted();
                string currentDownloadSpeed = _bandwidthService.GetCurrentDownloadSpeedFormatted();
                
                lblCurrentUploadSpeed.Text = $"Current Upload: {currentUploadSpeed}";
                lblCurrentDownloadSpeed.Text = $"Current Download: {currentDownloadSpeed}";
                
                // Update color coding based on limits and current speeds
                if (_bandwidthService.IsBandwidthControlEnabled)
                {
                    long uploadLimit = _bandwidthService.GetUploadLimitKBps();
                    long downloadLimit = _bandwidthService.GetDownloadLimitKBps();
                    
                    // Color coding for upload speed
                    if (uploadLimit > 0)
                    {
                        double currentUploadBps = _bandwidthService.GetCurrentUploadSpeedBytesPerSecond();
                        double currentUploadKbps = currentUploadBps / 1024;
                        
                        if (currentUploadKbps > uploadLimit * 0.9) // Within 90% of limit
                        {
                            lblCurrentUploadSpeed.ForeColor = Color.Red;
                        }
                        else if (currentUploadKbps > uploadLimit * 0.7) // Within 70% of limit
                        {
                            lblCurrentUploadSpeed.ForeColor = Color.Orange;
                        }
                        else
                        {
                            lblCurrentUploadSpeed.ForeColor = Color.Green;
                        }
                        
                        lblCurrentUploadSpeed.Text += $" (Limit: {uploadLimit} KB/s)";
                    }
                    else
                    {
                        lblCurrentUploadSpeed.ForeColor = Color.Blue; // Unlimited
                        lblCurrentUploadSpeed.Text += " (Unlimited)";
                    }
                    
                    // Color coding for download speed
                    if (downloadLimit > 0)
                    {
                        double currentDownloadBps = _bandwidthService.GetCurrentDownloadSpeedBytesPerSecond();
                        double currentDownloadKbps = currentDownloadBps / 1024;
                        
                        if (currentDownloadKbps > downloadLimit * 0.9) // Within 90% of limit
                        {
                            lblCurrentDownloadSpeed.ForeColor = Color.Red;
                        }
                        else if (currentDownloadKbps > downloadLimit * 0.7) // Within 70% of limit
                        {
                            lblCurrentDownloadSpeed.ForeColor = Color.Orange;
                        }
                        else
                        {
                            lblCurrentDownloadSpeed.ForeColor = Color.Green;
                        }
                        
                        lblCurrentDownloadSpeed.Text += $" (Limit: {downloadLimit} KB/s)";
                    }
                    else
                    {
                        lblCurrentDownloadSpeed.ForeColor = Color.Blue; // Unlimited
                        lblCurrentDownloadSpeed.Text += " (Unlimited)";
                    }
                }
                else
                {
                    // Bandwidth control disabled
                    lblCurrentUploadSpeed.ForeColor = Color.Black;
                    lblCurrentDownloadSpeed.ForeColor = Color.Black;
                    lblCurrentUploadSpeed.Text += " (Control Disabled)";
                    lblCurrentDownloadSpeed.Text += " (Control Disabled)";
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError($"Error updating speed labels: {ex.Message}", "UI");
                // Fallback display
                lblCurrentUploadSpeed.Text = "Current Upload: Error";
                lblCurrentDownloadSpeed.Text = "Current Download: Error";
                lblCurrentUploadSpeed.ForeColor = Color.Red;
                lblCurrentDownloadSpeed.ForeColor = Color.Red;
            }
        }
        
        private void SpeedUpdateTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Check if there are any active transfers
                var timerJobManager = ServiceLocator.TimerJobManager;
                bool hasActiveTransfers = false;
                
                if (timerJobManager != null)
                {
                    // Check if any timer jobs are currently running transfers
                    var activeJobs = timerJobManager.GetRunningJobs();
                    hasActiveTransfers = activeJobs != null && activeJobs.Count > 0;
                }
                
                // If no active transfers, apply speed decay
                if (!hasActiveTransfers)
                {
                    _bandwidthService.ApplySpeedDecay(0.7); // Decay by 30% each update cycle
                }
                
                UpdateSpeedLabels();
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError($"Error in speed update timer: {ex.Message}", "UI");
                // Fallback to basic update
                UpdateSpeedLabels();
            }
        }
        
        private void BandwidthService_SettingsChanged(object sender, EventArgs e)
        {
            // Update UI when settings change from another source
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler(BandwidthService_SettingsChanged), sender, e);
                return;
            }
            
            LoadBandwidthSettings();
        }
        
        private void chkEnableBandwidthControl_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _bandwidthService.IsBandwidthControlEnabled = chkEnableBandwidthControl.Checked;
                UpdateBandwidthControlsState();
                UpdateSpeedLabels();
                
                ServiceLocator.LogService.LogInfo($"Bandwidth control {(chkEnableBandwidthControl.Checked ? "enabled" : "disabled")}", "UI");
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError($"Error changing bandwidth control state: {ex.Message}", "UI");
                MessageBox.Show($"Error changing bandwidth control state: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnApplyBandwidthSettings_Click(object sender, EventArgs e)
        {
            try
            {
                long uploadLimitKBps = (long)numUploadLimit.Value;
                long downloadLimitKBps = (long)numDownloadLimit.Value;
                
                // Automatically enable bandwidth control if any limits are set
                bool shouldEnable = uploadLimitKBps > 0 || downloadLimitKBps > 0 || chkEnableBandwidthControl.Checked;
                _bandwidthService.IsBandwidthControlEnabled = shouldEnable;
                chkEnableBandwidthControl.Checked = shouldEnable;
                
                _bandwidthService.SetUploadLimitKBps(uploadLimitKBps);
                _bandwidthService.SetDownloadLimitKBps(downloadLimitKBps);
                
                UpdateBandwidthControlsState();
                UpdateSpeedLabels();
                
                string message = "Bandwidth settings applied successfully!\n" +
                               $"Upload limit: {(uploadLimitKBps == 0 ? "Unlimited" : uploadLimitKBps + " KB/s")}\n" +
                               $"Download limit: {(downloadLimitKBps == 0 ? "Unlimited" : downloadLimitKBps + " KB/s")}\n" +
                               $"Bandwidth control: {(shouldEnable ? "Enabled" : "Disabled")}";
                
                MessageBox.Show(message, "Settings Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                ServiceLocator.LogService.LogInfo($"Bandwidth settings applied - Upload: {uploadLimitKBps} KB/s, Download: {downloadLimitKBps} KB/s, Enabled: {shouldEnable}", "UI");
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError($"Error applying bandwidth settings: {ex.Message}", "UI");
                MessageBox.Show($"Error applying bandwidth settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnResetBandwidthSettings_Click(object sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to reset all bandwidth settings to default values?", 
                    "Reset Bandwidth Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    _bandwidthService.ResetToDefaults();
                    LoadBandwidthSettings();
                    
                    MessageBox.Show("Bandwidth settings have been reset to default values.", 
                        "Settings Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    ServiceLocator.LogService.LogInfo("Bandwidth settings reset to defaults", "UI");
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError($"Error resetting bandwidth settings: {ex.Message}", "UI");
                MessageBox.Show($"Error resetting bandwidth settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Get the status of the FTPSyncer Windows service
        /// </summary>
        private string GetServiceStatus()
        {
            try
            {
                using (var service = new System.ServiceProcess.ServiceController("FTPSyncerService"))
                {
                    return service.Status.ToString();
                }
            }
            catch
            {
                return "Not Installed";
            }
        }
        
        #endregion

        #region Helper Methods

        /// <summary>
        /// Simple input dialog for configuration name (compatible with .NET 3.5)
        /// </summary>
        private string PromptForConfigurationName(string message, string defaultValue)
        {
            // Create a simple form for input
            using (var inputForm = new Form())
            {
                inputForm.Text = "Save Multi-Job Configuration";
                inputForm.Size = new System.Drawing.Size(500, 250);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;

                var lblMessage = new Label()
                {
                    Text = message,
                    Location = new System.Drawing.Point(12, 15),
                    Size = new System.Drawing.Size(460, 80),
                    AutoSize = false
                };
                inputForm.Controls.Add(lblMessage);

                var lblName = new Label()
                {
                    Text = "Configuration Name:",
                    Location = new System.Drawing.Point(12, 105),
                    Size = new System.Drawing.Size(120, 23),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                inputForm.Controls.Add(lblName);

                var txtName = new TextBox()
                {
                    Text = defaultValue,
                    Location = new System.Drawing.Point(12, 130),
                    Size = new System.Drawing.Size(460, 23)
                };
                inputForm.Controls.Add(txtName);

                var btnOK = new Button()
                {
                    Text = "Save",
                    DialogResult = DialogResult.OK,
                    Location = new System.Drawing.Point(310, 170),
                    Size = new System.Drawing.Size(80, 30)
                };
                inputForm.Controls.Add(btnOK);

                var btnCancel = new Button()
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new System.Drawing.Point(395, 170),
                    Size = new System.Drawing.Size(80, 30)
                };
                inputForm.Controls.Add(btnCancel);

                inputForm.AcceptButton = btnOK;
                inputForm.CancelButton = btnCancel;

                txtName.Focus();
                txtName.SelectAll();

                return inputForm.ShowDialog(this) == DialogResult.OK ? txtName.Text : null;
            }
        }

        #endregion

        #endregion
    }
}
