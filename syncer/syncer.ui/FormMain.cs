using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using syncer.ui.Services;
using syncer.ui.Interfaces;
using syncer.ui.Forms;

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

        public FormMain()
        {
            try
            {
                InitializeComponent();
                InitializeServices();
                InitializeSystemTray();
                InitializeCustomComponents();
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
                _trayManager.UpdateToolTip(string.Format("Data Syncer - Service: {0}", serviceStatus));
                
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
                        "Data Syncer Started", 
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
            this.Text = "DataSyncer - Main Dashboard";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 500);
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
                    _trayManager.UpdateToolTip($"Data Syncer - Service: {status}");
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
                        // Test the connection
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
                            lblConnectionStatus.Text = $"Connection failed to {connectionSettings.Host}:{connectionSettings.Port}";
                            lblConnectionStatus.ForeColor = Color.Red;
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

        private void connectionManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var connectionManager = new FormConnectionManager())
                {
                    connectionManager.ShowDialog(this);
                    
                    // Update connection status after using connection manager
                    UpdateConnectionStatus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening connection manager: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService?.LogError($"Error opening connection manager: {ex.Message}");
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
            MessageBox.Show("DataSyncer v1.0\nFile Synchronization Tool\n\nDeveloped for automated file transfers.", "About DataSyncer", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                                    "Data Syncer service started automatically for new job.",
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
                        "Are you sure you want to exit Data Syncer?", 
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
            dgvTimerJobs.Columns.Add("Interval", "Upload Interval");
            dgvTimerJobs.Columns.Add("LastUpload", "Last Upload");
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
            
            // Initially disable action buttons
            btnStopTimerJob.Enabled = false;
            btnEditTimerJob.Enabled = false;
        }
        
        private void dgvTimerJobs_SelectionChanged(object sender, EventArgs e)
        {
            // Enable or disable buttons based on selection
            bool hasSelection = dgvTimerJobs.SelectedRows.Count > 0;
            btnStopTimerJob.Enabled = hasSelection;
            btnEditTimerJob.Enabled = hasSelection;
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
                lblRunningTimerJobs.Text = string.Format("Running Timer Jobs: {0}", runningJobs.Count);
                
                // Get job details directly from TimerJobManager
                foreach (long jobId in runningJobs)
                {
                    // Get job status and details from TimerJobManager
                    bool isRunning = timerJobManager.IsTimerJobRunning(jobId);
                    DateTime? lastUpload = timerJobManager.GetLastUploadTime(jobId);
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
                    row.Cells["LastUpload"].Value = lastUpload.HasValue ? lastUpload.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Never";
                    row.Cells["Status"].Value = isRunning ? "Running" : "Stopped";
                    
                    // Set row color based on status
                    row.DefaultCellStyle.BackColor = isRunning ? Color.LightGreen : Color.LightGray;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading timer jobs: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error loading timer jobs: " + ex.Message);
            }
        }
        
        private void btnRefreshTimerJobs_Click(object sender, EventArgs e)
        {
            RefreshTimerJobsGrid();
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
                
                // Create a SyncJob object to pass to the FormSchedule
                SyncJob jobToEdit = new SyncJob
                {
                    Id = (int)jobId,
                    Name = jobName ?? "Timer Job " + jobId,
                    SourcePath = folderPath ?? "",
                    DestinationPath = remotePath ?? "/",
                    IsEnabled = isRunning,
                    LastRun = lastUpload,
                    TransferMode = "Upload",
                    // Convert interval from milliseconds to minutes for display
                    IntervalValue = (int)(intervalMs / 60000), // Convert ms to minutes
                    IntervalType = "Minutes"
                };
                
                // Open the FormSchedule in edit mode
                using (FormSchedule editForm = new FormSchedule(jobToEdit))
                {
                    editForm.Text = "Edit Timer Job - " + jobName;
                    
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        // The job has been updated, refresh the grid
                        RefreshTimerJobsGrid();
                        
                        ServiceLocator.LogService.LogInfo(string.Format("Timer job '{0}' (ID: {1}) has been edited", jobName, jobId));
                        
                        // Show confirmation message
                        MessageBox.Show(
                            "Timer job has been updated successfully!\n\nNote: The changes will take effect after the job is restarted.",
                            "Job Updated",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error editing timer job: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try { ServiceLocator.LogService.LogError("Error editing timer job: " + ex.Message); } catch { }
            }
        }
        
        #endregion

        #region Configuration Management Event Handlers

        private void saveConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we have a current job and connection to save
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
                
                // Create a sample job if none exists (user would normally have created one)
                var currentJob = new SyncJob
                {
                    Name = "Current Job Configuration",
                    Description = "Job configuration saved from main form",
                    SourcePath = "",
                    DestinationPath = "",
                    IntervalValue = 60,
                    IntervalType = "Minutes",
                    TransferMode = "Copy (Keep both files)",
                    IncludeSubFolders = true,
                    OverwriteExisting = true,
                    IsEnabled = true
                };
                
                // Open the save configuration form
                using (var saveForm = new Forms.FormSaveJobConfiguration(
                    currentJob, 
                    currentConnection, 
                    currentConnection, // Using same connection for both source and destination as example
                    configService,
                    connectionService))
                {
                    if (saveForm.ShowDialog() == DialogResult.OK)
                    {
                        MessageBox.Show("Configuration saved successfully!", "Save Successful", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var configService = ServiceLocator.SavedJobConfigurationService;
                
                // Open the simplified load configuration form
                using (var loadForm = new Forms.FormSimpleLoadConfiguration(configService))
                {
                    if (loadForm.ShowDialog() == DialogResult.OK && loadForm.SelectedConfiguration != null)
                    {
                        var config = loadForm.SelectedConfiguration;
                        
                        // Apply the loaded configuration
                        ApplyLoadedConfiguration(config, loadForm.LoadAndStart);
                        
                        MessageBox.Show("Configuration '" + config.Name + "' loaded successfully!", 
                            "Load Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Load Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError($"Error loading configuration: {ex.Message}", "UI");
            }
        }

        private void ApplyLoadedConfiguration(SavedJobConfiguration config, bool startAfterLoad)
        {
            try
            {
                var connectionService = ServiceLocator.ConnectionService;
                var timerJobManager = ServiceLocator.TimerJobManager;
                
                // Auto-start service when loading configuration with jobs
                if (startAfterLoad && config.JobSettings != null)
                {
                    AutoStartServiceIfNeeded();
                }
                
                // Apply connection settings
                if (config.SourceConnection?.Settings != null)
                {
                    connectionService.SaveConnectionSettings(config.SourceConnection.Settings);
                    
                    // Small delay to ensure connection is properly applied (.NET 3.5 compatibility)
                    System.Threading.Thread.Sleep(100);
                }
                
                // If the job should be started automatically
                if (startAfterLoad && config.JobSettings != null)
                {
                    // Register and start the timer job
                    var intervalMs = GetIntervalInMilliseconds(config.JobSettings.IntervalValue, config.JobSettings.IntervalType);
                    
                    var jobId = DateTime.Now.Ticks; // Use timestamp as unique ID
                    
                    if (timerJobManager.RegisterTimerJob(
                        jobId,
                        config.JobSettings.Name,
                        config.JobSettings.SourcePath,
                        config.JobSettings.DestinationPath,
                        intervalMs))
                    {
                        if (timerJobManager.StartTimerJob(jobId))
                        {
                            ServiceLocator.LogService.LogInfo($"Auto-started job '{config.JobSettings.Name}' from loaded configuration", "UI");
                        }
                    }
                }
                
                // Update usage statistics
                var configService = ServiceLocator.SavedJobConfigurationService;
                configService.UpdateUsageStatistics(config.Id);
                
                // Show notification if enabled
                if (config.ShowNotificationOnStart && _notificationService != null)
                {
                    _notificationService.ShowNotification(
                        "Configuration Loaded",
                        $"Configuration '{config.DisplayName}' has been loaded successfully.",
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
                case "seconds":
                    return intervalValue * 1000;
                case "minutes":
                    return intervalValue * 60 * 1000;
                case "hours":
                    return intervalValue * 60 * 60 * 1000;
                default:
                    return intervalValue * 60 * 1000; // Default to minutes
            }
        }

        #region Configuration Button Event Handlers

        private void btnSaveCurrentConfig_Click(object sender, EventArgs e)
        {
            try
            {
                // Create a basic job configuration from current settings
                var currentJob = new SyncJob();
                var sourceConn = new ConnectionSettings();
                var destConn = new ConnectionSettings();
                
                // Open the save dialog to let user enter details
                using (var saveForm = new FormSaveJobConfiguration(currentJob, sourceConn, destConn, 
                    _savedJobConfigService, _connectionService))
                {
                    if (saveForm.ShowDialog() == DialogResult.OK)
                    {
                        MessageBox.Show("Configuration saved successfully!", "Save Configuration", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error saving configuration: {0}", ex.Message), 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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

        public void LoadConfigurationAndStart(SavedJobConfiguration configuration)
        {
            try
            {
                // Load the job configuration and start it
                if (configuration?.JobSettings != null)
                {
                    ServiceLocator.LogService.LogInfo($"Quick Launch: Loading configuration '{configuration.Name}'", "QuickLaunch");
                    
                    // Apply the loaded configuration
                    ApplyLoadedConfiguration(configuration, true);
                    
                    // Show success message
                    string message = $"Job '{configuration.Name}' started successfully!\n" +
                                   $"Source: {configuration.JobSettings.SourcePath}\n" +
                                   $"Destination: {configuration.JobSettings.DestinationPath}\n" +
                                   $"Interval: {configuration.JobSettings.IntervalValue} {configuration.JobSettings.IntervalType}";
                    
                    MessageBox.Show(message, "Job Started Successfully", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    ServiceLocator.LogService.LogInfo($"Quick Launch: Job '{configuration.Name}' started successfully", "QuickLaunch");
                }
                else
                {
                    MessageBox.Show("Invalid configuration: Missing job settings.", "Configuration Error", 
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

        #endregion
    }
}
