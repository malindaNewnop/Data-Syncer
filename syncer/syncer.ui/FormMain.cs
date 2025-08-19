using System;
using System.Drawing;
using System.Windows.Forms;
using syncer.ui.Services;

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
            InitializeJobsGrid();
            UpdateServiceStatus();
            UpdateConnectionStatus();
            RefreshJobsGrid();
        }

        private void InitializeJobsGrid()
        {
            dgvJobs.Columns.Clear();
            dgvJobs.Columns.Add("JobName", "Job Name");
            dgvJobs.Columns.Add("Status", "Status");
            dgvJobs.Columns.Add("SourcePath", "Source Path");
            dgvJobs.Columns.Add("DestinationPath", "Destination Path");
            dgvJobs.Columns.Add("Schedule", "Schedule");
            dgvJobs.Columns.Add("LastRun", "Last Run");
            dgvJobs.Columns.Add("NextRun", "Next Run");
            dgvJobs.Columns["JobName"].Width = 120;
            dgvJobs.Columns["Status"].Width = 80;
            dgvJobs.Columns["SourcePath"].Width = 200;
            dgvJobs.Columns["DestinationPath"].Width = 200;
            dgvJobs.Columns["Schedule"].Width = 100;
            dgvJobs.Columns["LastRun"].Width = 130;
            dgvJobs.Columns["NextRun"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            
            // Add selection changed event to update button states
            dgvJobs.SelectionChanged += dgvJobs_SelectionChanged;
        }

        private void dgvJobs_SelectionChanged(object sender, EventArgs e)
        {
            UpdateJobControlButtons();
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
                if (scheduleForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshJobsGrid();
                    
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
                    RefreshJobsGrid();
                    
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

        private void btnPauseJob_Click(object sender, EventArgs e)
        {
            if (dgvJobs.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a job to pause/resume.", "No Job Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int jobId = (int)dgvJobs.SelectedRows[0].Tag;
                SyncJob job = _jobService.GetJobById(jobId);
                
                if (job != null)
                {
                    string currentStatus = _jobService.GetJobStatus(jobId);
                    bool isRunning = currentStatus.ToLower().Contains("running") || currentStatus.ToLower().Contains("active");
                    
                    if (isRunning)
                    {
                        // Pause the job
                        if (_jobService.StopJob(jobId))
                        {
                            RefreshJobsGrid();
                            
                            if (_notificationService != null)
                            {
                                _notificationService.ShowNotification(
                                    "Job Paused",
                                    $"Job '{job.Name}' has been paused successfully.",
                                    ToolTipIcon.Info);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Failed to pause the job. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        // Resume the job
                        if (_jobService.StartJob(jobId))
                        {
                            RefreshJobsGrid();
                            
                            if (_notificationService != null)
                            {
                                _notificationService.ShowNotification(
                                    "Job Resumed",
                                    $"Job '{job.Name}' has been resumed successfully.",
                                    ToolTipIcon.Info);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Failed to resume the job. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error managing job: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error managing job: " + ex.Message);
            }
        }

        private void btnDeleteJob_Click(object sender, EventArgs e)
        {
            if (dgvJobs.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a job to delete.", "No Job Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int jobId = (int)dgvJobs.SelectedRows[0].Tag;
                SyncJob job = _jobService.GetJobById(jobId);
                
                if (job != null)
                {
                    DialogResult result = MessageBox.Show(
                        $"Are you sure you want to delete the job '{job.Name}'?\n\nThis action cannot be undone.",
                        "Confirm Deletion",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        if (_jobService.DeleteJob(jobId))
                        {
                            RefreshJobsGrid();
                            
                            if (_notificationService != null)
                            {
                                _notificationService.ShowNotification(
                                    "Job Deleted",
                                    $"Job '{job.Name}' has been deleted successfully.",
                                    ToolTipIcon.Info);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete the job. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting job: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error deleting job: " + ex.Message);
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshJobsGrid();
            UpdateServiceStatus();
            UpdateConnectionStatus();
        }

        private void RefreshJobsGrid()
        {
            try
            {
                dgvJobs.Rows.Clear();
                System.Collections.Generic.List<SyncJob> jobs = null;
                
                try 
                {
                    jobs = _jobService.GetAllJobs();
                }
                catch (Exception ex)
                {
                    // Handle the specific case where jobs can't be loaded
                    MessageBox.Show("Cannot load jobs: " + ex.Message + "\n\nThe application will continue with an empty job list.", 
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    jobs = new System.Collections.Generic.List<SyncJob>();
                }
                
                for (int i = 0; i < jobs.Count; i++)
                {
                    SyncJob job = jobs[i];
                    string schedule = "Every " + job.IntervalValue + " " + job.IntervalType;
                    
                    // Improve LastRun display - show "Never" for both null and MinValue dates
                    string lastRun = "Never";
                    if (job.LastRun.HasValue && job.LastRun.Value != DateTime.MinValue)
                    {
                        lastRun = job.LastRun.Value.ToString("yyyy-MM-dd HH:mm");
                    }
                    
                    string nextRun = job.GetNextRunTime();
                    string status = job.IsEnabled ? "Enabled" : "Disabled";
                    int rowIndex = dgvJobs.Rows.Add(new object[] { job.Name, status, job.SourcePath, job.DestinationPath, schedule, lastRun, nextRun });
                    dgvJobs.Rows[rowIndex].Tag = job.Id;
                }
                
                // Update button states based on selection
                UpdateJobControlButtons();
                
                try
                {
                    ServiceLocator.LogService.LogInfo("Jobs grid refreshed. Found " + jobs.Count + " jobs.");
                }
                catch 
                {
                    // Ignore logging errors
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading jobs: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try 
                {
                    ServiceLocator.LogService.LogError("Error loading jobs: " + ex.Message);
                }
                catch 
                {
                    // Ignore logging errors
                }
            }
        }

        private void UpdateJobControlButtons()
        {
            // Safely check if we have selected rows
            bool hasSelectedJob = dgvJobs != null && dgvJobs.SelectedRows != null && dgvJobs.SelectedRows.Count > 0;
            
            // Make sure the buttons exist before trying to access them
            if (btnPauseJob != null) btnPauseJob.Enabled = hasSelectedJob;
            if (btnDeleteJob != null) btnDeleteJob.Enabled = hasSelectedJob;
            
            if (hasSelectedJob)
            {
                try
                {
                    // Check if Tag exists and is an integer
                    if (dgvJobs.SelectedRows[0].Tag == null)
                    {
                        if (btnPauseJob != null)
                        {
                            btnPauseJob.Text = "Pause Job";
                            btnPauseJob.BackColor = Color.Orange;
                        }
                        return;
                    }
                    
                    int jobId = (int)dgvJobs.SelectedRows[0].Tag;
                    
                    // Check if job service is available
                    if (_jobService == null)
                    {
                        if (btnPauseJob != null)
                        {
                            btnPauseJob.Text = "Pause Job";
                            btnPauseJob.BackColor = Color.Orange;
                        }
                        return;
                    }
                    
                    string currentStatus = _jobService.GetJobStatus(jobId);
                    
                    // Check if status is null or empty
                    if (string.IsNullOrEmpty(currentStatus))
                    {
                        if (btnPauseJob != null)
                        {
                            btnPauseJob.Text = "Pause Job";
                            btnPauseJob.BackColor = Color.Orange;
                        }
                        return;
                    }
                    
                    bool isRunning = currentStatus.ToLower().Contains("running") || currentStatus.ToLower().Contains("active");
                    
                    if (btnPauseJob != null)
                    {
                        btnPauseJob.Text = isRunning ? "Pause Job" : "Resume Job";
                        btnPauseJob.BackColor = isRunning ? Color.Orange : Color.LightGreen;
                    }
                }
                catch (Exception ex)
                {
                    // Log the error for debugging
                    Console.WriteLine($"Error in UpdateJobControlButtons: {ex.Message}");
                    
                    if (btnPauseJob != null)
                    {
                        btnPauseJob.Text = "Pause Job";
                        btnPauseJob.BackColor = Color.Orange;
                    }
                }
            }
            else
            {
                if (btnPauseJob != null)
                {
                    btnPauseJob.Text = "Pause Job";
                    btnPauseJob.BackColor = Color.Orange;
                }
            }
        }

        private void dgvJobs_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    int jobId = (int)dgvJobs.Rows[e.RowIndex].Tag;
                    SyncJob job = _jobService.GetJobById(jobId);
                    if (job != null)
                    {
                        using (FormSchedule scheduleForm = new FormSchedule(job))
                        {
                            if (scheduleForm.ShowDialog() == DialogResult.OK)
                            {
                                RefreshJobsGrid();
                                
                                // Show notification
                                if (_notificationService != null)
                                {
                                    _notificationService.ShowNotification(
                                        "Job Updated",
                                        $"Job '{job.Name}' has been updated successfully.",
                                        ToolTipIcon.Info);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening job for editing: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ServiceLocator.LogService.LogError("Error opening job for editing: " + ex.Message);
                }
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
    }
}
