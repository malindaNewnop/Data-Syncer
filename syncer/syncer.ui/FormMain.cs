using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
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
        
        // System tray components
        private SystemTrayManager _trayManager;
        private NotificationService _notificationService;

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
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            InitializeTimerJobsGrid();
            UpdateServiceStatus();
            UpdateConnectionStatus();
            RefreshTimerJobsGrid();
        }



        private void UpdateServiceStatus()
        {
            try
            {
                string status = _serviceManager != null ? _serviceManager.GetServiceStatus() : "Unknown";
                lblServiceStatus.Text = "Service: " + status;
                lblServiceStatus.ForeColor = status == "Running" ? Color.Green : Color.Red;
                if (status == "Running")
                {
                    btnStartStop.Text = "Stop Service";
                    btnStartStop.BackColor = Color.LightCoral;
                }
                else
                {
                    btnStartStop.Text = "Start Service";
                    btnStartStop.BackColor = Color.LightGreen;
                }
                
                // Update the tray icon tooltip
                if (_trayManager != null)
                {
                    _trayManager.UpdateToolTip($"Data Syncer - Service: {status}");
                }
            }
            catch (Exception)
            {
                lblServiceStatus.Text = "Service: Unknown";
                lblServiceStatus.ForeColor = Color.Gray;
                btnStartStop.Text = "Start Service";
                btnStartStop.BackColor = Color.LightGray;
                btnStartStop.Enabled = false;
            }
        }

        private void UpdateConnectionStatus()
        {
            try
            {
                bool isConnected = _connectionService != null && _connectionService.IsConnected();
                ConnectionSettings connectionSettings = _connectionService != null ? _connectionService.GetConnectionSettings() : null;
                string status = isConnected ? "Connected (" + (connectionSettings != null ? connectionSettings.ConnectionTypeDisplay : "Unknown") + ")" : "Disconnected";
                if (!isConnected && connectionSettings != null)
                {
                    status += " (" + connectionSettings.ConnectionTypeDisplay + ")";
                }
                lblConnectionStatus.Text = "Connection: " + status;
                lblConnectionStatus.ForeColor = isConnected ? Color.Green : Color.Red;
            }
            catch (Exception)
            {
                lblConnectionStatus.Text = "Connection: Unknown";
                lblConnectionStatus.ForeColor = Color.Gray;
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

        private void enhancedSftpSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var enhancedForm = new Forms.FormComprehensiveConnection())
                {
                    if (enhancedForm.ShowDialog() == DialogResult.OK)
                    {
                        UpdateConnectionStatus();
                        
                        // Show notification for SFTP connection update
                        if (_notificationService != null)
                        {
                            _notificationService.ShowNotification(
                                "SFTP Connection Updated", 
                                "Enhanced SFTP settings have been configured successfully.",
                                ToolTipIcon.Info);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening enhanced SFTP settings: " + ex.Message, 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void scheduleSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormSchedule scheduleForm = new FormSchedule())
            {
                // Set form title to reflect it's now for timer jobs
                scheduleForm.Text = "Timer Job Settings";
                
                if (scheduleForm.ShowDialog() == DialogResult.OK)
                {
                    // Show notification
                    if (_notificationService != null)
                    {
                        _notificationService.ShowNotification(
                            "Schedule Updated",
                            "Job schedules have been updated successfully.",
                            ToolTipIcon.Info);
                    }
                }
            }
        }

        private void filterSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormFilters filtersForm = new FormFilters())
            {
                filtersForm.ShowDialog();
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
                    // Show notification
                    if (_notificationService != null)
                    {
                        _notificationService.ShowNotification(
                            "Job Added",
                            "New synchronization job has been added successfully.",
                            ToolTipIcon.Info);
                    }
                }
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
        }
        
        private void dgvTimerJobs_SelectionChanged(object sender, EventArgs e)
        {
            // Enable or disable the stop button based on selection
            btnStopTimerJob.Enabled = dgvTimerJobs.SelectedRows.Count > 0;
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
        
        #endregion
    }
}
