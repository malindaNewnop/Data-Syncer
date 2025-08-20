using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using System.Timers;
using System.Collections.Generic;
using syncer.core;
using syncer.core.Configuration;
using syncer.ui.Forms;
using syncer.ui.Interfaces;

namespace syncer.ui
{
    public partial class FormSchedule : Form
    {
        private ISyncJobService _jobService;
        private IConnectionService _connectionService;
        private SyncJob _currentJob;
        private bool _isEditMode;
        private ITransferClient _currentTransferClient;
        private syncer.core.ConnectionSettings _coreConnectionSettings;
        
        // Timer-based upload functionality
        private System.Timers.Timer _uploadTimer;
        private bool _isTimerRunning = false;
        private DateTime _lastUploadTime;
        private string[] _selectedFilesForTimer; // Store files selected for timer upload
        private string _selectedFolderForTimer; // Base folder path for relative path calculation
        private string _timerUploadDestination = "/"; // Default destination path

        public FormSchedule() : this(null) { }

        public FormSchedule(SyncJob jobToEdit)
        {
            InitializeComponent();
            InitializeServices();
            _currentJob = jobToEdit;
            _isEditMode = jobToEdit != null;
            InitializeCustomComponents();
            
            // Subscribe to form closing event to handle timers
            this.FormClosing += FormSchedule_FormClosing;
        }
        
        private void FormSchedule_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If a timer is running and no job has been saved, warn the user
            if (_isTimerRunning && _currentJob?.Id == null)
            {
                DialogResult result = MessageBox.Show(
                    "You have a timer running but haven't saved the job. The timer will stop when the form closes.\n\n" +
                    "Would you like to save this job before closing?",
                    "Timer Running",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);
                
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        SaveJob();
                        MessageBox.Show(
                            "Job saved successfully. The timer will continue running in the background.",
                            "Job Saved",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Failed to save job: " + ex.Message,
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        
                        // Ask if they want to continue closing
                        DialogResult continueResult = MessageBox.Show(
                            "Do you still want to close this form? The timer will stop.",
                            "Confirm Close",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        
                        if (continueResult == DialogResult.No)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                // For "No" we'll just continue and close
            }
            
            // Always stop and clean up the timer when form is closing
            if (_uploadTimer != null)
            {
                try
                {
                    _uploadTimer.Stop();
                    _uploadTimer.Elapsed -= OnTimerElapsed;
                    _uploadTimer.Dispose();
                    _isTimerRunning = false;
                    ServiceLocator.LogService.LogInfo("Timer stopped and disposed during form closing");
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError("Error stopping timer during form closing: " + ex.Message);
                }
            }
        }

        private void InitializeServices()
        {
            _jobService = ServiceLocator.SyncJobService;
            _connectionService = ServiceLocator.ConnectionService;
        }

        private void InitializeCustomComponents()
        {
            this.Text = _isEditMode ? "Edit Upload Timer Settings" : "Add Upload Timer Settings";
            this.Size = new Size(779, 440); // Reduced height since we're removing source/destination
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Hide source and destination section - we don't need it anymore
            if (gbPaths != null) gbPaths.Visible = false;
            
            // Move timer controls up since we're hiding source/destination section
            if (gbTimerSettings != null)
            {
                gbTimerSettings.Location = new Point(gbTimerSettings.Location.X, 97);
            }
            
            // Move file manager controls up too
            if (gbFileManager != null)
            {
                gbFileManager.Location = new Point(gbFileManager.Location.X, 
                    gbTimerSettings != null ? gbTimerSettings.Location.Y + gbTimerSettings.Height + 10 : 300);
            }
            
            SetDefaultValues();
            if (_isEditMode) LoadJobSettings();
            InitializeTransferClient();
            InitializeTimerControls();
        }

        private void InitializeTimerControls()
        {
            // Set up timer unit dropdown
            if (cmbTimerUnit != null)
            {
                cmbTimerUnit.Items.Clear();
                cmbTimerUnit.Items.AddRange(new string[] { "Seconds", "Minutes", "Hours" });
                cmbTimerUnit.SelectedIndex = 1; // Default to Minutes
            }

            // Set up timer interval limits
            if (numTimerInterval != null)
            {
                numTimerInterval.Minimum = 1;
                numTimerInterval.Maximum = 9999;
                numTimerInterval.Value = 5; // Default 5 minutes
            }

            // Initialize timer status
            if (lblTimerStatus != null) lblTimerStatus.Text = "Timer stopped";
            if (lblLastUpload != null) lblLastUpload.Text = "Never";
            
            // Initialize button states
            if (btnStartTimer != null) btnStartTimer.Enabled = false;
            if (btnStopTimer != null) btnStopTimer.Enabled = false;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Clean up timer when form is closed
            if (_uploadTimer != null)
            {
                _uploadTimer.Stop();
                _uploadTimer.Elapsed -= OnTimerElapsed;
                _uploadTimer.Dispose();
                _uploadTimer = null;
            }
            
            base.OnFormClosed(e);
        }

        private void SetDefaultValues()
        {
            if (chkEnabled != null) chkEnabled.Checked = true;
            if (chkEnableTimer != null) chkEnableTimer.Checked = false;
            if (numTimerInterval != null) numTimerInterval.Value = 5;
            if (cmbTimerUnit != null) cmbTimerUnit.SelectedIndex = 1; // Minutes
            if (lblTimerStatus != null) lblTimerStatus.Text = "Timer stopped";
            if (lblLastUpload != null) lblLastUpload.Text = "Never";
            if (lblSelectedFiles != null) lblSelectedFiles.Text = "No files selected";
            
            // Initialize timer upload settings
            _selectedFilesForTimer = null;
            _timerUploadDestination = "/";
        }

        private void LoadJobSettings()
        {
            if (_currentJob != null)
            {
                if (txtJobName != null) txtJobName.Text = _currentJob.Name;
                if (chkEnabled != null) chkEnabled.Checked = _currentJob.IsEnabled;
                if (txtSourcePath != null) txtSourcePath.Text = _currentJob.SourcePath;
                if (txtDestinationPath != null) txtDestinationPath.Text = _currentJob.DestinationPath;
                
                // Load timer settings (if available from job data)
                if (chkEnableTimer != null) chkEnableTimer.Checked = false; // Default for new timer feature
                if (numTimerInterval != null) numTimerInterval.Value = 5; // Default 5 minute interval
                if (cmbTimerUnit != null) cmbTimerUnit.SelectedIndex = 1; // Minutes
            }
        }

        private void btnBrowseSource_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select source folder to sync";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtSourcePath.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnBrowseDestination_Click(object sender, EventArgs e)
        {
            ConnectionSettings connectionSettings = _connectionService.GetConnectionSettings();
            if (connectionSettings != null && connectionSettings.Protocol == "LOCAL")
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Select destination folder for local sync";
                    dialog.ShowNewFolderButton = true;
                    if (txtDestinationPath != null && !UIStringExtensions.IsNullOrWhiteSpace(txtDestinationPath.Text))
                        dialog.SelectedPath = txtDestinationPath.Text;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (txtDestinationPath != null) txtDestinationPath.Text = dialog.SelectedPath;
                    }
                }
            }
            else if (connectionSettings != null && (connectionSettings.Protocol == "FTP" || connectionSettings.Protocol == "SFTP"))
            {
                if (_connectionService.IsConnected())
                {
                    // Convert UI ConnectionSettings to Core ConnectionSettings
                    var coreSettings = new syncer.core.ConnectionSettings
                    {
                        Protocol = connectionSettings.Protocol == "SFTP" ? 
                            syncer.core.ProtocolType.Sftp : 
                            connectionSettings.Protocol == "FTP" ? 
                                syncer.core.ProtocolType.Ftp : 
                                syncer.core.ProtocolType.Local,
                        Host = connectionSettings.Host,
                        Port = connectionSettings.Port,
                        Username = connectionSettings.Username,
                        Password = connectionSettings.Password,
                        SshKeyPath = connectionSettings.SshKeyPath,
                        Timeout = connectionSettings.Timeout
                    };
                    
                    // Use the new FileZilla-like file manager
                    try
                    {
                        using (FormRemoteDirectoryBrowser fileManager = new FormRemoteDirectoryBrowser(coreSettings))
                        {
                            fileManager.IsUploadMode = false; // We want to select destination path
                            if (fileManager.ShowDialog() == DialogResult.OK)
                            {
                                if (!string.IsNullOrEmpty(fileManager.SelectedRemotePath))
                                {
                                    if (txtDestinationPath != null) 
                                        txtDestinationPath.Text = fileManager.SelectedRemotePath;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error opening file manager: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ShowRemotePathInputDialog();
                    }
                }
                else
                {
                    MessageBox.Show("Not connected to a remote server. Please connect first.", 
                        "Connection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    
                    // Show remote path input dialog for now
                    ShowRemotePathInputDialog();
                }
            }
            else
            {
                ShowRemotePathInputDialog();
            }
        }

        private void ShowRemotePathInputDialog()
        {
            using (Form inputForm = new Form())
            {
                ConnectionSettings connectionSettings = _connectionService.GetConnectionSettings();
                string protocol = connectionSettings != null ? connectionSettings.Protocol : "Unknown";
                string host = connectionSettings != null ? connectionSettings.Host : "";
                
                inputForm.Text = $"Enter Remote Destination Path ({protocol})";
                inputForm.Size = new Size(450, 180);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;
                
                Label labelInfo = new Label(); 
                labelInfo.Left = 10; 
                labelInfo.Top = 10; 
                labelInfo.Width = 420;
                labelInfo.Text = $"Enter the remote path on {protocol} server: {host}"; 
                labelInfo.AutoSize = false;
                
                Label label = new Label(); 
                label.Left = 10; 
                label.Top = 40; 
                label.Text = "Remote path (e.g., /remote/folder):"; 
                label.AutoSize = true;
                
                TextBox textBox = new TextBox(); 
                textBox.Left = 10; 
                textBox.Top = 65; 
                textBox.Width = 410; 
                textBox.Text = txtDestinationPath != null ? txtDestinationPath.Text : string.Empty;
                
                Label tipLabel = new Label();
                tipLabel.Left = 10;
                tipLabel.Top = 95;
                tipLabel.Width = 420;
                tipLabel.Height = 30;
                tipLabel.Text = "Tip: For FTP/SFTP, paths typically start with / and use forward slashes.";
                tipLabel.ForeColor = Color.DarkBlue;
                tipLabel.AutoSize = false;
                
                Button okButton = new Button(); 
                okButton.Text = "OK"; 
                okButton.Left = 255; 
                okButton.Width = 75; 
                okButton.Top = 130; 
                okButton.DialogResult = DialogResult.OK;
                
                Button cancelButton = new Button(); 
                cancelButton.Text = "Cancel"; 
                cancelButton.Left = 345; 
                cancelButton.Width = 75; 
                cancelButton.Top = 130; 
                cancelButton.DialogResult = DialogResult.Cancel;
                
                inputForm.Controls.Add(labelInfo);
                inputForm.Controls.Add(label);
                inputForm.Controls.Add(textBox);
                inputForm.Controls.Add(tipLabel);
                inputForm.Controls.Add(okButton);
                inputForm.Controls.Add(cancelButton);
                inputForm.AcceptButton = okButton;
                inputForm.CancelButton = cancelButton;
                
                if (inputForm.ShowDialog() == DialogResult.OK && txtDestinationPath != null)
                {
                    txtDestinationPath.Text = textBox.Text.Trim();
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                try
                {
                    SaveJob();
                    MessageBox.Show(_isEditMode ? "Job updated successfully!" : "Job created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving job: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ServiceLocator.LogService.LogError("Error saving job: " + ex.Message);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateInputs()
        {
            if (UIStringExtensions.IsNullOrWhiteSpace(txtJobName.Text))
            {
                MessageBox.Show("Please enter a job name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtJobName.Focus();
                return false;
            }
            
            // Skip source and destination validation as we're using files/folders selected directly
            
            if (chkEnableTimer != null && chkEnableTimer.Checked && numTimerInterval != null && numTimerInterval.Value <= 0)
            {
                MessageBox.Show("Please enter a valid timer interval.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numTimerInterval.Focus();
                return false;
            }
            return true;
        }

        private void SaveJob()
        {
            if (_currentJob == null) _currentJob = new SyncJob();
            _currentJob.Name = txtJobName.Text.Trim();
            _currentJob.IsEnabled = chkEnabled.Checked;
            _currentJob.SourcePath = txtSourcePath.Text.Trim();
            _currentJob.DestinationPath = txtDestinationPath.Text.Trim();
            _currentJob.StartTime = DateTime.Now; // Set to current time for timer-based uploads
            
            // For timer functionality, store interval in minutes for consistency
            if (numTimerInterval != null && cmbTimerUnit != null && cmbTimerUnit.SelectedItem != null)
            {
                int intervalValue = (int)numTimerInterval.Value;
                string unit = cmbTimerUnit.SelectedItem.ToString();
                
                // Convert to minutes for storage
                if (unit == "Seconds")
                    _currentJob.IntervalValue = intervalValue / 60; // Convert seconds to minutes
                else if (unit == "Minutes")
                    _currentJob.IntervalValue = intervalValue;
                else if (unit == "Hours")
                    _currentJob.IntervalValue = intervalValue * 60;
                
                _currentJob.IntervalType = "Minutes"; // Always store as minutes internally
            }
            else
            {
                _currentJob.IntervalValue = 5; // Default 5 minutes
                _currentJob.IntervalType = "Minutes";
            }
            
            _currentJob.TransferMode = "Upload"; // Timer-based uploads
            
            // Set connection settings for source and destination
            var currentConnection = _connectionService.GetConnectionSettings();
            if (currentConnection != null && currentConnection.IsRemoteConnection)
            {
                // Timer upload scenario: local source, remote destination
                _currentJob.SourceConnection = new ConnectionSettings(); // Local
                _currentJob.DestinationConnection = currentConnection; // Remote
            }
            else
            {
                // Local to local transfer
                _currentJob.SourceConnection = new ConnectionSettings(); // Local
                _currentJob.DestinationConnection = new ConnectionSettings(); // Local
            }
            
            if (_isEditMode)
            {
                _jobService.UpdateJob(_currentJob);
                ServiceLocator.LogService.LogInfo("Job '" + _currentJob.Name + "' updated");
            }
            else
            {
                _currentJob.Id = _jobService.CreateJob(_currentJob);
                ServiceLocator.LogService.LogInfo("Job '" + _currentJob.Name + "' created");
            }
            
            // If the timer is enabled and we have a folder selected, register the job with the timer job manager
            if (chkEnableTimer != null && chkEnableTimer.Checked && !string.IsNullOrEmpty(_selectedFolderForTimer))
            {
                try 
                {
                    // First, try to get the timer job manager from the service locator
                    ITimerJobManager timerJobManager = ServiceLocator.TimerJobManager;
                    
                    // Register the job with the timer job manager
                    if (timerJobManager != null)
                    {
                        double intervalMs = CalculateTimerInterval();
                        bool registered = timerJobManager.RegisterTimerJob(_currentJob.Id, 
                            _selectedFolderForTimer, _timerUploadDestination, intervalMs);
                        
                        if (registered && _isTimerRunning)
                        {
                            // If the timer is already running, start the job in the manager
                            timerJobManager.StartTimerJob(_currentJob.Id);
                            ServiceLocator.LogService.LogInfo(string.Format(
                                "Job '{0}' registered and started in background timer service", _currentJob.Name));
                        }
                        else if (registered)
                        {
                            ServiceLocator.LogService.LogInfo(string.Format(
                                "Job '{0}' registered in background timer service", _currentJob.Name));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError(string.Format(
                        "Failed to register job '{0}' with timer service: {1}", _currentJob.Name, ex.Message));
                }
            }
        }

        private void chkEnabled_CheckedChanged(object sender, EventArgs e)
        {
            // Enable/disable the main form functionality
        }

        private void chkEnableTimer_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEnableTimer != null)
            {
                bool enabled = chkEnableTimer.Checked;
                
                // Enable/disable timer controls
                if (numTimerInterval != null) numTimerInterval.Enabled = enabled;
                if (cmbTimerUnit != null) cmbTimerUnit.Enabled = enabled;
                if (btnStartTimer != null) btnStartTimer.Enabled = enabled && !_isTimerRunning;
                if (btnStopTimer != null) btnStopTimer.Enabled = enabled && _isTimerRunning;
                if (btnBrowseFilesForTimer != null) btnBrowseFilesForTimer.Enabled = enabled;
            }
        }

        private void btnBrowseFilesForTimer_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select folder for timed uploads (all files will be monitored)";
                dialog.ShowNewFolderButton = true;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = dialog.SelectedPath;
                    
                    // Get all files in the selected folder (including subfolders)
                    string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                    _selectedFilesForTimer = files;
                    
                    // Store the base folder path for relative path calculations during upload
                    _selectedFolderForTimer = folderPath;
                    
                    // Update the label to show selected folder and file count
                    if (lblSelectedFiles != null)
                    {
                        if (files.Length == 1)
                        {
                            lblSelectedFiles.Text = Path.GetFileName(folderPath) + " (1 file, including new files added later)";
                        }
                        else
                        {
                            lblSelectedFiles.Text = Path.GetFileName(folderPath) + " (" + files.Length + " files, including new files added later)";
                        }
                    }
                    
                    ServiceLocator.LogService.LogInfo(string.Format("Selected folder '{0}' with {1} files for timer upload (will also include newly added files)", 
                        folderPath, files.Length));
                    
                    // Ask for upload destination for timer uploads
                    AskForTimerUploadDestination();
                }
            }
        }

        private void AskForTimerUploadDestination()
        {
            using (Form inputForm = new Form())
            {
                inputForm.Text = "Timer Upload Destination";
                inputForm.Size = new Size(450, 180);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;
                
                Label label = new Label(); 
                label.Left = 10; 
                label.Top = 20; 
                label.Text = "Enter remote destination path for timer uploads:"; 
                label.AutoSize = true;
                
                TextBox textBox = new TextBox(); 
                textBox.Left = 10; 
                textBox.Top = 50; 
                textBox.Width = 400;
                textBox.Text = _timerUploadDestination;
                
                Button buttonOk = new Button(); 
                buttonOk.Text = "Set Destination"; 
                buttonOk.Left = 220; 
                buttonOk.Top = 90; 
                buttonOk.DialogResult = DialogResult.OK;
                
                Button buttonCancel = new Button(); 
                buttonCancel.Text = "Cancel"; 
                buttonCancel.Left = 330; 
                buttonCancel.Top = 90; 
                buttonCancel.DialogResult = DialogResult.Cancel;
                
                inputForm.Controls.Add(label);
                inputForm.Controls.Add(textBox);
                inputForm.Controls.Add(buttonOk);
                inputForm.Controls.Add(buttonCancel);
                inputForm.AcceptButton = buttonOk;
                inputForm.CancelButton = buttonCancel;
                
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    string destination = textBox.Text.Trim();
                    if (!string.IsNullOrEmpty(destination))
                    {
                        _timerUploadDestination = destination;
                        ServiceLocator.LogService.LogInfo("Timer upload destination set to: " + _timerUploadDestination);
                    }
                }
            }
        }

        private void btnStartTimer_Click(object sender, EventArgs e)
        {
            if (!ValidateConnection() || !ValidateTimerSettings()) return;

            try
            {
                // Calculate interval in milliseconds
                double intervalMs = CalculateTimerInterval();
                
                if (_uploadTimer == null)
                {
                    _uploadTimer = new System.Timers.Timer();
                    _uploadTimer.Elapsed += OnTimerElapsed;
                    _uploadTimer.AutoReset = true;
                }
                
                _uploadTimer.Interval = intervalMs;
                _uploadTimer.Start();
                _isTimerRunning = true;
                
                // Update UI
                if (lblTimerStatus != null) lblTimerStatus.Text = "Timer running";
                if (btnStartTimer != null) btnStartTimer.Enabled = false;
                if (btnStopTimer != null) btnStopTimer.Enabled = true;
                
                // Ask if user wants to save this job configuration
                DialogResult saveResult = MessageBox.Show(
                    "Timer started successfully! Do you want to save this job configuration for future use?", 
                    "Save Job Configuration", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (saveResult == DialogResult.Yes)
                {
                    SaveJob();
                    MessageBox.Show(
                        "Job configuration saved. This timer will continue to run even if this window is closed.", 
                        "Job Saved", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Upload timer started successfully! Note: This timer will stop if the application is closed.",
                        "Timer Started",
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);
                }
                
                ServiceLocator.LogService.LogInfo(string.Format("Upload timer started with interval: {0} ms", intervalMs));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting timer: " + ex.Message, "Timer Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error starting timer: " + ex.Message);
            }
        }

        private void btnStopTimer_Click(object sender, EventArgs e)
        {
            try
            {
                if (_uploadTimer != null)
                {
                    _uploadTimer.Stop();
                    _isTimerRunning = false;
                    
                    // Update UI
                    if (lblTimerStatus != null) lblTimerStatus.Text = "Timer stopped";
                    if (btnStartTimer != null) btnStartTimer.Enabled = chkEnableTimer != null && chkEnableTimer.Checked;
                    if (btnStopTimer != null) btnStopTimer.Enabled = false;
                    
                    MessageBox.Show("Upload timer stopped.", "Timer Stopped", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    ServiceLocator.LogService.LogInfo("Upload timer stopped");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error stopping timer: " + ex.Message, "Timer Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error stopping timer: " + ex.Message);
            }
        }

        private bool ValidateTimerSettings()
        {
            if (numTimerInterval == null || numTimerInterval.Value <= 0)
            {
                MessageBox.Show("Please enter a valid timer interval.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (numTimerInterval != null) numTimerInterval.Focus();
                return false;
            }
            
            if (_selectedFilesForTimer == null || _selectedFilesForTimer.Length == 0)
            {
                MessageBox.Show("Please select files for timer upload using 'Browse Files' button.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (btnBrowseFilesForTimer != null) btnBrowseFilesForTimer.Focus();
                return false;
            }
            
            return true;
        }

        private double CalculateTimerInterval()
        {
            if (numTimerInterval == null || cmbTimerUnit == null || cmbTimerUnit.SelectedItem == null)
                return 300000; // Default 5 minutes

            double value = (double)numTimerInterval.Value;
            string unit = cmbTimerUnit.SelectedItem.ToString();
            
            switch (unit)
            {
                case "Seconds":
                    return value * 1000;
                case "Minutes":
                    return value * 60 * 1000;
                case "Hours":
                    return value * 60 * 60 * 1000;
                default:
                    return value * 60 * 1000; // Default to minutes
            }
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                ServiceLocator.LogService.LogInfo("Timer elapsed - starting automatic upload");
                
                // Run upload on UI thread
                this.Invoke(new Action(() =>
                {
                    PerformAutomaticUpload();
                }));
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Timer elapsed error: " + ex.Message);
            }
        }

        private void PerformAutomaticUpload()
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedFolderForTimer))
                {
                    ServiceLocator.LogService.LogError("No folder selected for automatic upload");
                    return;
                }
                
                // Get all files in the directory, including any newly added files
                string[] currentFiles = Directory.GetFiles(_selectedFolderForTimer, "*", SearchOption.AllDirectories);
                
                if (currentFiles.Length == 0)
                {
                    ServiceLocator.LogService.LogError("No files found in selected folder for automatic upload");
                    return;
                }
                
                ServiceLocator.LogService.LogInfo(string.Format("Starting automatic upload of folder '{0}' with {1} files (including any newly added)", 
                    _selectedFolderForTimer, currentFiles.Length));
                
                // Upload all files including newly added ones
                PerformFolderUpload(_selectedFolderForTimer, currentFiles, _timerUploadDestination);
                
                // Update last upload time
                _lastUploadTime = DateTime.Now;
                if (lblLastUpload != null)
                {
                    lblLastUpload.Text = _lastUploadTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Automatic upload error: " + ex.Message);
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                string previewInfo = GenerateTimerPreview();
                MessageBox.Show(previewInfo, "Timer Settings Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string GenerateTimerPreview()
        {
            string enabled = chkEnabled.Checked ? "Enabled" : "Disabled";
            string timerEnabled = (chkEnableTimer != null && chkEnableTimer.Checked) ? "Enabled" : "Disabled";
            
            string interval = "Not set";
            if (numTimerInterval != null && cmbTimerUnit != null && cmbTimerUnit.SelectedItem != null)
            {
                interval = string.Format("Every {0} {1}", numTimerInterval.Value, cmbTimerUnit.SelectedItem.ToString());
            }
            
            string timerStatus = _isTimerRunning ? "Running" : "Stopped";
            string lastUpload = _lastUploadTime == DateTime.MinValue ? "Never" : _lastUploadTime.ToString("yyyy-MM-dd HH:mm:ss");
            
            return "Job Name: " + txtJobName.Text + "\n" +
                   "Status: " + enabled + "\n" +
                   "Source: " + txtSourcePath.Text + "\n" +
                   "Destination: " + txtDestinationPath.Text + "\n" +
                   "Timer: " + timerEnabled + "\n" +
                   "Upload Interval: " + interval + "\n" +
                   "Timer Status: " + timerStatus + "\n" +
                   "Last Upload: " + lastUpload;
        }

        #region File Manager and Transfer Operations

        private void InitializeTransferClient()
        {
            try
            {
                // Get connection settings and initialize transfer client
                ConnectionSettings uiSettings = _connectionService.GetConnectionSettings();
                if (uiSettings != null)
                {
                    _coreConnectionSettings = ConvertToCore(uiSettings);
                    UpdateTransferClient();
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to initialize transfer client: " + ex.Message);
            }
        }

        private void UpdateTransferClient()
        {
            if (_currentTransferClient != null)
            {
                // ITransferClient doesn't implement IDisposable, no need to dispose
                _currentTransferClient = null;
            }

            if (_coreConnectionSettings == null) return;

            try
            {
                var factory = syncer.core.ServiceFactory.CreateTransferClientFactory();
                _currentTransferClient = factory.Create(_coreConnectionSettings.Protocol);
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to create transfer client: " + ex.Message);
                MessageBox.Show("Failed to create transfer client: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private syncer.core.ConnectionSettings ConvertToCore(ConnectionSettings uiSettings)
        {
            return new syncer.core.ConnectionSettings
            {
                Protocol = uiSettings.Protocol == "SFTP" ? syncer.core.ProtocolType.Sftp :
                          uiSettings.Protocol == "FTP" ? syncer.core.ProtocolType.Ftp :
                          syncer.core.ProtocolType.Local,
                Host = uiSettings.Host,
                Port = uiSettings.Port,
                Username = uiSettings.Username,
                Password = uiSettings.Password,
                SshKeyPath = uiSettings.SshKeyPath,
                Timeout = uiSettings.Timeout
            };
        }

        private void btnUploadFile_Click(object sender, EventArgs e)
        {
            if (!ValidateConnection()) return;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select File(s) to Upload";
                dialog.Filter = "All Files (*.*)|*.*";
                dialog.Multiselect = true;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    UploadFiles(dialog.FileNames);
                }
            }
        }

        private void btnDownloadFile_Click(object sender, EventArgs e)
        {
            if (!ValidateConnection()) return;

            // Open file manager in download mode
            try
            {
                using (FormRemoteDirectoryBrowser fileManager = new FormRemoteDirectoryBrowser(_coreConnectionSettings))
                {
                    fileManager.IsUploadMode = false;
                    fileManager.Text = "Download Files from Remote Server";
                    
                    if (fileManager.ShowDialog() == DialogResult.OK)
                    {
                        if (!string.IsNullOrEmpty(fileManager.SelectedRemotePath))
                        {
                            // Ask user for local download location
                            using (FolderBrowserDialog localDialog = new FolderBrowserDialog())
                            {
                                localDialog.Description = "Select local folder to download files to";
                                localDialog.ShowNewFolderButton = true;
                                
                                if (localDialog.ShowDialog() == DialogResult.OK)
                                {
                                    DownloadFile(fileManager.SelectedRemotePath, localDialog.SelectedPath);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening download dialog: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenFileManager_Click(object sender, EventArgs e)
        {
            if (!ValidateConnection()) return;

            try
            {
                using (FormRemoteDirectoryBrowser fileManager = new FormRemoteDirectoryBrowser(_coreConnectionSettings))
                {
                    fileManager.Text = "FileZilla-like File Manager";
                    fileManager.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening file manager: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateConnection()
        {
            if (_coreConnectionSettings == null)
            {
                MessageBox.Show("No connection settings available. Please configure connection settings first.", 
                    "Connection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_coreConnectionSettings.Protocol == syncer.core.ProtocolType.Local)
            {
                return true; // Local operations don't need validation
            }

            if (!_connectionService.IsConnected())
            {
                MessageBox.Show("Not connected to remote server. Please connect first using Connection Settings.", 
                    "Connection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_currentTransferClient == null)
            {
                UpdateTransferClient();
                if (_currentTransferClient == null)
                {
                    MessageBox.Show("Failed to initialize transfer client.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            return true;
        }

        private void UploadFiles(string[] localFilePaths)
        {
            if (localFilePaths == null || localFilePaths.Length == 0) return;

            // Ask for remote destination folder
            using (Form inputForm = new Form())
            {
                inputForm.Text = "Upload Destination";
                inputForm.Size = new Size(450, 180);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;
                
                Label label = new Label(); 
                label.Left = 10; 
                label.Top = 20; 
                label.Text = "Enter remote destination path:"; 
                label.AutoSize = true;
                
                TextBox textBox = new TextBox(); 
                textBox.Left = 10; 
                textBox.Top = 50; 
                textBox.Width = 400;
                textBox.Text = "/";
                
                Button buttonOk = new Button(); 
                buttonOk.Text = "Upload"; 
                buttonOk.Left = 250; 
                buttonOk.Top = 90; 
                buttonOk.DialogResult = DialogResult.OK;
                
                Button buttonCancel = new Button(); 
                buttonCancel.Text = "Cancel"; 
                buttonCancel.Left = 330; 
                buttonCancel.Top = 90; 
                buttonCancel.DialogResult = DialogResult.Cancel;
                
                inputForm.Controls.Add(label);
                inputForm.Controls.Add(textBox);
                inputForm.Controls.Add(buttonOk);
                inputForm.Controls.Add(buttonCancel);
                inputForm.AcceptButton = buttonOk;
                inputForm.CancelButton = buttonCancel;
                
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    string remotePath = textBox.Text.Trim();
                    if (!string.IsNullOrEmpty(remotePath))
                    {
                        PerformUpload(localFilePaths, remotePath);
                    }
                }
            }
        }

        private void PerformFolderUpload(string baseLocalFolder, string[] localFilePaths, string baseRemotePath)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (sender, e) =>
            {
                try
                {
                    ServiceLocator.LogService.LogInfo(string.Format("Starting upload of folder '{0}' with {1} files to {2}", 
                        baseLocalFolder, localFilePaths.Length, baseRemotePath));
                    ServiceLocator.LogService.LogInfo(string.Format("Connection: {0}@{1}:{2}", 
                        _coreConnectionSettings.Username, _coreConnectionSettings.Host, _coreConnectionSettings.Port));
                        
                    // Create dictionary to track created remote folders
                    Dictionary<string, bool> createdRemoteFolders = new Dictionary<string, bool>();
                    
                    for (int i = 0; i < localFilePaths.Length; i++)
                    {
                        string localFile = localFilePaths[i];
                        
                        // Calculate relative path from base folder
                        string relativePath = localFile.Substring(baseLocalFolder.Length).TrimStart('\\', '/');
                        string relativeDirectory = Path.GetDirectoryName(relativePath);
                        
                        // Fix path separators for remote path
                        string normalizedRelativePath = relativePath.Replace('\\', '/');
                        string normalizedBaseRemotePath = baseRemotePath.Replace('\\', '/');
                        if (!normalizedBaseRemotePath.EndsWith("/")) normalizedBaseRemotePath += "/";
                        
                        // Construct full remote path (preserving folder structure)
                        string remoteFile = normalizedBaseRemotePath + normalizedRelativePath;
                        
                        // Create remote directory structure if needed
                        if (!string.IsNullOrEmpty(relativeDirectory))
                        {
                            string[] pathParts = relativeDirectory.Split('\\', '/');
                            string currentPath = normalizedBaseRemotePath;
                            
                            foreach (string part in pathParts)
                            {
                                currentPath += part + "/";
                                
                                // Only create directory if we haven't already done so
                                if (!createdRemoteFolders.ContainsKey(currentPath))
                                {
                                    // Try to ensure remote directory exists
                                    string error;
                                    if (!_currentTransferClient.EnsureDirectory(_coreConnectionSettings, currentPath, out error))
                                    {
                                        ServiceLocator.LogService.LogWarning(string.Format("Could not create directory: {0} - {1}", 
                                            currentPath, error ?? "Unknown error"));
                                    }
                                    
                                    createdRemoteFolders[currentPath] = true;
                                }
                            }
                        }
                        
                        worker.ReportProgress((i * 100) / localFilePaths.Length, 
                            string.Format("Uploading {0}...", relativePath));
                        
                        ServiceLocator.LogService.LogInfo(string.Format("Uploading: {0} -> {1}", localFile, remoteFile));
                        
                        // Continue with normal file upload
                        string uploadError;
                        bool success = _currentTransferClient.UploadFile(_coreConnectionSettings, localFile, remoteFile, true, out uploadError);
                        
                        if (!success)
                        {
                            ServiceLocator.LogService.LogError(string.Format("Failed to upload: {0} - {1}", 
                                relativePath, uploadError ?? "Unknown error"));
                        }
                        else
                        {
                            ServiceLocator.LogService.LogInfo(string.Format("Successfully uploaded: {0}", relativePath));
                        }
                    }
                    
                    worker.ReportProgress(100, "Folder upload completed");
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError("Error during folder upload: " + ex.Message);
                    e.Result = ex;
                }
            };
            
            worker.ProgressChanged += (sender, e) =>
            {
                if (e.UserState != null)
                {
                    ServiceLocator.LogService.LogInfo(e.UserState.ToString());
                }
            };
            
            worker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Result is Exception)
                {
                    Exception ex = (Exception)e.Result;
                    MessageBox.Show("Upload failed: " + ex.Message, "Upload Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Folder uploaded successfully!", "Upload Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            
            worker.RunWorkerAsync();
        }

        private void PerformUpload(string[] localFilePaths, string remotePath)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (sender, e) =>
            {
                try
                {
                    ServiceLocator.LogService.LogInfo(string.Format("Starting upload of {0} files to {1}", localFilePaths.Length, remotePath));
                    ServiceLocator.LogService.LogInfo(string.Format("Connection: {0}@{1}:{2}", _coreConnectionSettings.Username, _coreConnectionSettings.Host, _coreConnectionSettings.Port));
                    
                    for (int i = 0; i < localFilePaths.Length; i++)
                    {
                        string localFile = localFilePaths[i];
                        string fileName = Path.GetFileName(localFile);
                        
                        // Fix path separator - ensure remote path uses forward slashes
                        string normalizedRemotePath = remotePath.Replace('\\', '/');
                        if (!normalizedRemotePath.EndsWith("/")) normalizedRemotePath += "/";
                        string remoteFile = normalizedRemotePath + fileName;
                        
                        worker.ReportProgress((i * 100) / localFilePaths.Length, 
                            string.Format("Uploading {0}...", fileName));
                        
                        ServiceLocator.LogService.LogInfo(string.Format("Uploading: {0} -> {1}", localFile, remoteFile));
                        
                        // Check if local file exists and is accessible
                        if (!File.Exists(localFile))
                        {
                            throw new Exception(string.Format("Local file does not exist: {0}", localFile));
                        }
                        
                        FileInfo fileInfo = new FileInfo(localFile);
                        ServiceLocator.LogService.LogInfo(string.Format("Local file size: {0} bytes", fileInfo.Length));
                        
                        string error;
                        bool success = _currentTransferClient.UploadFile(_coreConnectionSettings, localFile, remoteFile, true, out error);
                        
                        if (!success)
                        {
                            string errorMsg = string.Format("Failed to upload {0}: {1}", fileName, error ?? "Unknown error");
                            ServiceLocator.LogService.LogError(errorMsg);
                            throw new Exception(errorMsg);
                        }
                        else
                        {
                            ServiceLocator.LogService.LogInfo(string.Format("Successfully uploaded: {0}", fileName));
                            
                            // Try to verify the upload by checking if file exists on remote
                            bool remoteExists;
                            string verifyError;
                            if (_currentTransferClient.FileExists(_coreConnectionSettings, remoteFile, out remoteExists, out verifyError))
                            {
                                if (remoteExists)
                                {
                                    ServiceLocator.LogService.LogInfo(string.Format("Upload verified: {0} exists on remote server", fileName));
                                }
                                else
                                {
                                    ServiceLocator.LogService.LogWarning(string.Format("Upload completed but file not found on remote: {0}", fileName));
                                }
                            }
                            else
                            {
                                ServiceLocator.LogService.LogWarning(string.Format("Could not verify remote file existence: {0}", verifyError ?? "Unknown error"));
                            }
                        }
                    }
                    
                    worker.ReportProgress(100, "Upload completed successfully!");
                    ServiceLocator.LogService.LogInfo("All uploads completed successfully");
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError("Upload failed: " + ex.Message);
                    e.Result = ex;
                }
            };
            
            worker.ProgressChanged += (sender, e) =>
            {
                // Could show progress dialog here
                if (e.UserState != null)
                {
                    ServiceLocator.LogService.LogInfo(e.UserState.ToString());
                }
            };
            
            worker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Result is Exception)
                {
                    Exception ex = (Exception)e.Result;
                    MessageBox.Show("Upload failed: " + ex.Message, "Upload Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ServiceLocator.LogService.LogError("Upload failed: " + ex.Message);
                }
                else
                {
                    MessageBox.Show("Files uploaded successfully!", "Upload Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ServiceLocator.LogService.LogInfo("Upload completed successfully");
                }
            };
            
            worker.RunWorkerAsync();
        }

        private void DownloadFile(string remotePath, string localFolderPath)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (sender, e) =>
            {
                try
                {
                    string fileName = Path.GetFileName(remotePath);
                    string localFilePath = Path.Combine(localFolderPath, fileName);
                    
                    worker.ReportProgress(50, string.Format("Downloading {0}...", fileName));
                    
                    string error;
                    bool success = _currentTransferClient.DownloadFile(_coreConnectionSettings, remotePath, localFilePath, true, out error);
                    
                    if (!success)
                    {
                        throw new Exception(string.Format("Failed to download {0}: {1}", fileName, error));
                    }
                    
                    worker.ReportProgress(100, "Download completed successfully!");
                }
                catch (Exception ex)
                {
                    e.Result = ex;
                }
            };
            
            worker.ProgressChanged += (sender, e) =>
            {
                if (e.UserState != null)
                {
                    ServiceLocator.LogService.LogInfo(e.UserState.ToString());
                }
            };
            
            worker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Result is Exception)
                {
                    Exception ex = (Exception)e.Result;
                    MessageBox.Show("Download failed: " + ex.Message, "Download Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ServiceLocator.LogService.LogError("Download failed: " + ex.Message);
                }
                else
                {
                    MessageBox.Show("File downloaded successfully!", "Download Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ServiceLocator.LogService.LogInfo("Download completed successfully");
                }
            };
            
            worker.RunWorkerAsync();
        }

        #endregion
    }
}
