using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using System.Timers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
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
        
        // Override to prevent unwanted resizing
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // Maintain the designed size
            base.SetBoundsCore(x, y, 800, 470, specified);
        }
        
        // Timer-based upload functionality
        private System.Timers.Timer _uploadTimer;
        private bool _isTimerRunning = false;
        private DateTime _lastUploadTime;
        private string[] _selectedFilesForTimer; // Store files selected for timer upload
        private string _selectedFolderForTimer; // Base folder path for relative path calculation
        private string _timerUploadDestination = "/"; // Default destination path

        // Timer-based download functionality
        private System.Timers.Timer _downloadTimer;
        private bool _isDownloadTimerRunning = false;
        private DateTime _lastDownloadTime;
        private string _selectedRemoteFolderForTimer; // Remote folder path for download monitoring
        private string _selectedRemoteFileForTimer; // Specific remote file for download (if any)
        private string _timerDownloadDestination; // Local destination path for downloads
        private string _currentTransferMode = "Upload"; // "Upload" or "Download"

        // Filter controls - simplified filtering (most advanced filtering features removed)
        // private GroupBox gbFilters; // Unused - filtering simplified
        // private CheckBox chkEnableFilters; // Unused - filtering simplified
        private CheckedListBox clbFileTypes;
        private NumericUpDown numMinFileSize;
        private NumericUpDown numMaxFileSize;
        private ComboBox cmbFileSizeUnit;
        private CheckBox chkIncludeHiddenFiles;
        private CheckBox chkIncludeSystemFiles;
        private CheckBox chkIncludeReadOnlyFiles;
        private TextBox txtExcludePatterns;
        private FilterSettings _jobFilterSettings;

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
            if ((_isTimerRunning || _isDownloadTimerRunning) && _currentJob?.Id == null)
            {
                string timerType = _isTimerRunning ? "upload" : "download";
                DialogResult result = MessageBox.Show(
                    $"You have a {timerType} timer running but haven't saved the job. The timer will stop when the form closes.\n\n" +
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
            
            // Always stop and clean up both timers when form is closing
            if (_uploadTimer != null)
            {
                try
                {
                    _uploadTimer.Stop();
                    _uploadTimer.Elapsed -= OnTimerElapsed;
                    _uploadTimer.Dispose();
                    _isTimerRunning = false;
                    ServiceLocator.LogService.LogInfo("Upload timer stopped and disposed during form closing");
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError("Error stopping upload timer during form closing: " + ex.Message);
                }
            }
            
            if (_downloadTimer != null)
            {
                try
                {
                    _downloadTimer.Stop();
                    _downloadTimer.Elapsed -= OnDownloadTimerElapsed;
                    _downloadTimer.Dispose();
                    _isDownloadTimerRunning = false;
                    ServiceLocator.LogService.LogInfo("Download timer stopped and disposed during form closing");
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError("Error stopping download timer during form closing: " + ex.Message);
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
            this.Text = _isEditMode ? "Edit Timer Job" : "Add Timer Settings";
            // Remove manual size setting - let the designer handle the size
            // this.Size = new Size(800, 400); // This was causing the layout issue
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Don't reorganize layout - let the designer layout remain
            // ReorganizeFormLayout();
            
            SetDefaultValues();
            if (_isEditMode) LoadJobSettings();
            InitializeTransferClient();
            InitializeTimerControls();
            InitializeFilterControls();
        }

        private void ReorganizeFormLayout()
        {
            // Add a header section
            CreateHeaderSection();
            
            // Create transfer mode selection section
            CreateTransferModeSection();
            
            // Position timer settings at the top left with better spacing
            if (gbTimerSettings != null)
            {
                gbTimerSettings.Location = new Point(12, 125); // Moved down to accommodate transfer mode
                gbTimerSettings.Size = new Size(370, 120);
                gbTimerSettings.Text = "Timer Settings";
                
                // Improve internal layout of timer controls
                AdjustTimerControlsLayout();
            }
            
            // Position file manager to the right of timer settings with proper spacing
            if (gbFileManager != null)
            {
                gbFileManager.Location = new Point(398, 125);
                gbFileManager.Size = new Size(370, 120);
                gbFileManager.Text = "File Manager";
                
                // Ensure proper spacing within the File Manager group
                AdjustFileManagerLayout();
            }
        }

        private void AdjustFileManagerLayout()
        {
            if (gbFileManager == null) return;
            
            // Find and adjust controls within the File Manager group box
            foreach (Control control in gbFileManager.Controls)
            {
                if (control is Label)
                {
                    Label lbl = control as Label;
                    if (lbl.Text.Contains("Manual file transfer operations") || lbl.Text.Contains("files selected"))
                    {
                        lbl.Location = new Point(15, 25);
                        lbl.Size = new Size(400, 20);
                        lbl.AutoSize = false;
                    }
                }
                else if (control is Button)
                {
                    Button btn = control as Button;
                    if (btn.Text.Contains("Browse") || btn.Text.Contains("Select"))
                    {
                        btn.Location = new Point(15, 50);
                        btn.Size = new Size(120, 30);
                        btn.Text = "Browse Folder";
                    }
                    else if (btn.Text.Contains("Upload File(s)"))
                    {
                        btn.Location = new Point(145, 50);
                        btn.Size = new Size(100, 30);
                    }
                    else if (btn.Text.Contains("Download"))
                    {
                        btn.Location = new Point(255, 50);
                        btn.Size = new Size(100, 30);
                    }
                }
            }
        }

        private void AdjustTimerControlsLayout()
        {
            if (gbTimerSettings == null) return;
            
            // Find and adjust controls within the Timer Settings group box
            foreach (Control control in gbTimerSettings.Controls)
            {
                if (control is CheckBox)
                {
                    CheckBox chk = control as CheckBox;
                    if (chk.Text.Contains("Enable Timer"))
                    {
                        chk.Location = new Point(15, 25);
                        chk.Size = new Size(120, 20);
                    }
                }
                else if (control is Label)
                {
                    Label lbl = control as Label;
                    if (lbl.Text.Contains("Upload Every"))
                    {
                        lbl.Location = new Point(15, 55);
                        lbl.Size = new Size(80, 20);
                    }
                    else if (lbl.Text.Contains("Timer") && lbl.Text.Contains("stopped"))
                    {
                        lbl.Location = new Point(300, 25);
                        lbl.Size = new Size(120, 20);
                    }
                    else if (lbl.Text.Contains("Never") || lbl.Text.Contains("Last"))
                    {
                        lbl.Location = new Point(300, 55);
                        lbl.Size = new Size(120, 20);
                    }
                }
                else if (control is NumericUpDown)
                {
                    NumericUpDown num = control as NumericUpDown;
                    num.Location = new Point(100, 53);
                    num.Size = new Size(60, 20);
                }
                else if (control is ComboBox)
                {
                    ComboBox cmb = control as ComboBox;
                    cmb.Location = new Point(170, 53);
                    cmb.Size = new Size(80, 20);
                }
                else if (control is Button)
                {
                    Button btn = control as Button;
                    if (btn.Text.Contains("Start"))
                    {
                        btn.Location = new Point(15, 85);
                        btn.Size = new Size(80, 30);
                        btn.BackColor = Color.LightGreen;
                    }
                    else if (btn.Text.Contains("Stop"))
                    {
                        btn.Location = new Point(105, 85);
                        btn.Size = new Size(80, 30);
                        btn.BackColor = Color.LightCoral;
                    }
                }
            }
        }

        private void CreateHeaderSection()
        {
            // Create header panel
            Panel headerPanel = new Panel();
            headerPanel.Location = new Point(0, 0);
            headerPanel.Size = new Size(950, 70);
            headerPanel.BackColor = Color.FromArgb(240, 248, 255); // Light blue background

            // Main title
            Label lblTitle = new Label();
            lblTitle.Text = _isEditMode ? "Edit Timer Job" : "Create New Timer Job";
            lblTitle.Location = new Point(20, 15);
            lblTitle.Size = new Size(400, 25);
            lblTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.DarkBlue;

            // Subtitle
            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Configure automatic file uploads/downloads and manual transfers - Simple and reliable";
            lblSubtitle.Location = new Point(20, 40);
            lblSubtitle.Size = new Size(600, 20);
            lblSubtitle.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
            lblSubtitle.ForeColor = Color.DarkSlateGray;

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            this.Controls.Add(headerPanel);
        }

        private void CreateTransferModeSection()
        {
            // Create transfer mode panel
            Panel transferModePanel = new Panel();
            transferModePanel.Location = new Point(12, 75);
            transferModePanel.Size = new Size(756, 45);
            transferModePanel.BackColor = Color.FromArgb(250, 250, 250);
            transferModePanel.BorderStyle = BorderStyle.FixedSingle;

            // Transfer mode label
            Label lblTransferMode = new Label();
            lblTransferMode.Text = "Transfer Mode:";
            lblTransferMode.Location = new Point(10, 15);
            lblTransferMode.Size = new Size(100, 20);
            lblTransferMode.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);

            // Upload radio button
            RadioButton rbUpload = new RadioButton();
            rbUpload.Text = "Upload (Local → Remote)";
            rbUpload.Location = new Point(120, 12);
            rbUpload.Size = new Size(180, 25);
            rbUpload.Checked = true; // Default selection
            rbUpload.Font = new Font("Microsoft Sans Serif", 9F);
            rbUpload.CheckedChanged += RbUpload_CheckedChanged;
            rbUpload.Name = "rbUpload";

            // Download radio button  
            RadioButton rbDownload = new RadioButton();
            rbDownload.Text = "Download (Remote → Local)";
            rbDownload.Location = new Point(320, 12);
            rbDownload.Size = new Size(200, 25);
            rbDownload.Font = new Font("Microsoft Sans Serif", 9F);
            rbDownload.CheckedChanged += RbDownload_CheckedChanged;
            rbDownload.Name = "rbDownload";

            transferModePanel.Controls.Add(lblTransferMode);
            transferModePanel.Controls.Add(rbUpload);
            transferModePanel.Controls.Add(rbDownload);
            this.Controls.Add(transferModePanel);
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
            
            // Initialize button states
            if (btnStartTimer != null) btnStartTimer.Enabled = false;
            if (btnStopTimer != null) btnStopTimer.Enabled = false;
        }

        private void InitializeFilterControls()
        {
            // Note: Advanced filtering has been simplified for easier job creation
            // Basic file selection will be done through folder browsing only
            
            // Initialize filter settings as minimal/disabled by default
            if (_jobFilterSettings == null)
            {
                _jobFilterSettings = new FilterSettings();
                _jobFilterSettings.FiltersEnabled = false; // Disabled by default for simplicity
                _jobFilterSettings.IncludeHiddenFiles = false;
                _jobFilterSettings.IncludeSystemFiles = false; 
                _jobFilterSettings.IncludeReadOnlyFiles = true;
                _jobFilterSettings.MinFileSize = 0;
                _jobFilterSettings.MaxFileSize = 1000; // 1GB default max
            }

            // Filter controls are now handled in the designer file
            // No need to create additional labels here
        }

        private void CreateFileTypesPanel()
        {
            // Advanced filtering simplified - this method no longer creates complex UI
            // File type filtering is handled automatically
            return;
        }

        private void CreateFileSizePanel()
        {
            // Advanced filtering simplified - this method no longer creates complex UI
            return;
        }

        private void CreateAttributesPanel()
        {
            // Advanced filtering simplified - this method no longer creates complex UI
            return;
        }

        private void CreateActionButtons()
        {
            // Advanced filtering simplified - complex filter UI removed
            // Main action buttons are handled by the main form designer
            return;
        }

        private void chkEnableFilters_CheckedChanged(object sender, EventArgs e)
        {
            // Advanced filtering simplified - this method no longer needed
            return;
        }

        private void UpdateFilterControlStates()
        {
            // Filtering simplified - advanced filter controls removed
            // All files will be uploaded automatically
            return;
        }

        private FilterSettings GetCurrentFilterSettings()
        {
            FilterSettings settings = new FilterSettings();
            
            // Basic filter enable/disable - now always disabled for simplicity
            settings.FiltersEnabled = false;
            
            // Set basic defaults for all files
            settings.AllowedFileTypes = null; // Allow all file types
            settings.MinFileSize = 0; // No minimum
            settings.MaxFileSize = 0; // No maximum
            settings.IncludeHiddenFiles = false;
            settings.IncludeSystemFiles = false;
            settings.IncludeReadOnlyFiles = true;
            settings.ExcludePatterns = null;
            
            return settings;
        }

        private void LoadFilterSettings()
        {
            if (_currentJob?.FilterSettings != null)
            {
                _jobFilterSettings = _currentJob.FilterSettings;
                
                // Simplified filtering - most UI controls removed
                // Only basic filter settings are maintained for compatibility
                
                if (clbFileTypes != null && _jobFilterSettings.AllowedFileTypes != null)
                {
                    // Clear all selections first
                    for (int i = 0; i < clbFileTypes.Items.Count; i++)
                        clbFileTypes.SetItemChecked(i, false);
                    
                    // Check the items that match saved settings
                    foreach (string allowedType in _jobFilterSettings.AllowedFileTypes)
                    {
                        int index = clbFileTypes.Items.IndexOf(allowedType);
                        if (index >= 0)
                            clbFileTypes.SetItemChecked(index, true);
                    }
                }
                
                if (numMinFileSize != null)
                {
                    // Values are stored in MB, display as MB by default
                    numMinFileSize.Value = _jobFilterSettings.MinFileSize;
                }
                if (numMaxFileSize != null)
                {
                    // Values are stored in MB, display as MB by default
                    numMaxFileSize.Value = _jobFilterSettings.MaxFileSize;
                }
                if (cmbFileSizeUnit != null)
                {
                    cmbFileSizeUnit.SelectedIndex = 1; // Default to MB since values are stored in MB
                }
                
                if (chkIncludeHiddenFiles != null)
                    chkIncludeHiddenFiles.Checked = _jobFilterSettings.IncludeHiddenFiles;
                if (chkIncludeSystemFiles != null)
                    chkIncludeSystemFiles.Checked = _jobFilterSettings.IncludeSystemFiles;
                if (chkIncludeReadOnlyFiles != null)
                    chkIncludeReadOnlyFiles.Checked = _jobFilterSettings.IncludeReadOnlyFiles;
                
                if (txtExcludePatterns != null)
                    txtExcludePatterns.Text = _jobFilterSettings.ExcludePatterns ?? "";
                
                UpdateFilterControlStates();
            }
        }

        private void btnTestFilters_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Advanced filtering has been removed for simplicity.\nAll files in the selected folder will be uploaded automatically.", 
                "Filter Testing Disabled", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool ShouldIncludeFileForTimer(string filePath, FilterSettings filterSettings)
        {
            // Simplified: Always include files since advanced filtering is removed
            return true;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Clean up timers when form is closed
            if (_uploadTimer != null)
            {
                _uploadTimer.Stop();
                _uploadTimer.Elapsed -= OnTimerElapsed;
                _uploadTimer.Dispose();
                _uploadTimer = null;
            }
            
            if (_downloadTimer != null)
            {
                _downloadTimer.Stop();
                _downloadTimer.Elapsed -= OnDownloadTimerElapsed;
                _downloadTimer.Dispose();
                _downloadTimer = null;
            }
            
            base.OnFormClosed(e);
        }

        #region Transfer Mode Event Handlers

        private void RbUpload_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                _currentTransferMode = "Upload";
                UpdateUIForTransferMode();
            }
        }

        private void RbDownload_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                _currentTransferMode = "Download";
                UpdateUIForTransferMode();
            }
        }

        private void UpdateUIForTransferMode()
        {
            // Update timer settings label based on mode
            if (gbTimerSettings != null)
            {
                gbTimerSettings.Text = _currentTransferMode + " Timer Settings";
            }

            // Update button text in File Manager
            foreach (Control control in this.Controls)
            {
                if (control is GroupBox && ((GroupBox)control).Text == "File Manager")
                {
                    foreach (Control innerControl in control.Controls)
                    {
                        if (innerControl is Button)
                        {
                            Button btn = innerControl as Button;
                            if (btn.Text.Contains("Browse"))
                            {
                                btn.Text = _currentTransferMode == "Upload" ? "Browse Local Folder" : "Browse Remote Folder";
                            }
                        }
                    }
                    break;
                }
            }

            ServiceLocator.LogService.LogInfo($"Transfer mode changed to: {_currentTransferMode}");
        }

        #endregion

        private void SetDefaultValues()
        {
            if (chkEnableJob != null) chkEnableJob.Checked = true;
            if (chkEnableTimer != null) chkEnableTimer.Checked = false;
            if (numTimerInterval != null) numTimerInterval.Value = 5;
            if (cmbTimerUnit != null) cmbTimerUnit.SelectedIndex = 1; // Minutes
            if (lblTimerStatus != null) lblTimerStatus.Text = "Timer stopped";
            if (lblNoFilesSelected != null) lblNoFilesSelected.Text = "No files selected";
            
            // Initialize timer upload settings
            _selectedFilesForTimer = null;
            _timerUploadDestination = "/";
        }

        private void LoadJobSettings()
        {
            if (_currentJob != null)
            {
                if (txtJobName != null) txtJobName.Text = _currentJob.Name;
                if (chkEnableJob != null) chkEnableJob.Checked = _currentJob.IsEnabled;
                
                // Load timer settings (from job data for timer jobs)
                if (chkEnableTimer != null) 
                {
                    // For timer jobs, enable the timer checkbox
                    chkEnableTimer.Checked = _currentJob.TransferMode == "Upload" || _currentJob.TransferMode == "Timer";
                }
                
                // Load interval settings
                if (numTimerInterval != null && cmbTimerUnit != null) 
                {
                    // Convert from minutes (stored format) to appropriate display unit
                    int intervalInMinutes = _currentJob.IntervalValue;
                    
                    if (intervalInMinutes >= 60 && intervalInMinutes % 60 == 0)
                    {
                        // Use hours if it's a whole number of hours
                        numTimerInterval.Value = intervalInMinutes / 60;
                        cmbTimerUnit.SelectedIndex = 2; // Hours
                    }
                    else if (intervalInMinutes < 1)
                    {
                        // Use seconds for sub-minute intervals
                        numTimerInterval.Value = intervalInMinutes * 60;
                        cmbTimerUnit.SelectedIndex = 0; // Seconds
                    }
                    else
                    {
                        // Use minutes
                        numTimerInterval.Value = intervalInMinutes;
                        cmbTimerUnit.SelectedIndex = 1; // Minutes
                    }
                }
                
                // Load source folder for timer jobs
                if (!string.IsNullOrEmpty(_currentJob.SourcePath))
                {
                    _selectedFolderForTimer = _currentJob.SourcePath;
                    if (lblNoFilesSelected != null)
                    {
                        lblNoFilesSelected.Text = "Selected: " + Path.GetFileName(_selectedFolderForTimer);
                    }
                }
                
                // Load destination path for timer jobs
                if (!string.IsNullOrEmpty(_currentJob.DestinationPath))
                {
                    _timerUploadDestination = _currentJob.DestinationPath;
                }
                
                // Load filter settings
                LoadFilterSettings();
            }
        }

        // Removed - btnBrowseSource_Click method - no longer needed in simplified timer form
        // Removed - btnBrowseDestination_Click method - no longer needed in simplified timer form

        // Removed - BrowseRemoteFolder method - no longer needed in simplified timer form
        // Removed - ShowRemotePathInputDialog method - no longer needed in simplified timer form

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
            _currentJob.IsEnabled = chkEnableJob.Checked;
            
            // Set paths based on transfer mode
            if (_currentTransferMode == "Upload")
            {
                // For upload jobs: local source, remote destination
                _currentJob.SourcePath = _selectedFolderForTimer ?? "";
                _currentJob.DestinationPath = _timerUploadDestination ?? "/";
                _currentJob.TransferMode = "Upload"; // Timer-based uploads
            }
            else if (_currentTransferMode == "Download")
            {
                // For download jobs: remote source, local destination
                _currentJob.SourcePath = _selectedRemoteFolderForTimer ?? "/";
                _currentJob.DestinationPath = _timerDownloadDestination ?? "";
                _currentJob.TransferMode = "Download"; // Timer-based downloads
            }
            
            _currentJob.StartTime = DateTime.Now; // Set to current time for timer-based transfers
            
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
            
            // Save filter settings to the job
            _currentJob.FilterSettings = GetCurrentFilterSettings();
            
            // Set connection settings for source and destination based on transfer mode
            var currentConnection = _connectionService.GetConnectionSettings();
            if (currentConnection != null && currentConnection.IsRemoteConnection)
            {
                if (_currentTransferMode == "Upload")
                {
                    // Upload: local source, remote destination
                    _currentJob.SourceConnection = new ConnectionSettings(); // Local
                    _currentJob.DestinationConnection = currentConnection; // Remote
                }
                else if (_currentTransferMode == "Download")
                {
                    // Download: remote source, local destination
                    _currentJob.SourceConnection = currentConnection; // Remote
                    _currentJob.DestinationConnection = new ConnectionSettings(); // Local
                }
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
            
            // If the timer is enabled and we have the required paths, register/update the job with the timer job manager
            bool hasRequiredPaths = (_currentTransferMode == "Upload" && !string.IsNullOrEmpty(_selectedFolderForTimer)) ||
                                   (_currentTransferMode == "Download" && !string.IsNullOrEmpty(_selectedRemoteFolderForTimer));
                                   
            if (chkEnableTimer != null && chkEnableTimer.Checked && hasRequiredPaths)
            {
                try 
                {
                    // First, try to get the timer job manager from the service locator
                    ITimerJobManager timerJobManager = ServiceLocator.TimerJobManager;
                    
                    // Register or update the job with the timer job manager
                    if (timerJobManager != null)
                    {
                        double intervalMs = CalculateTimerInterval();
                        
                        string sourcePath = _currentTransferMode == "Upload" ? _selectedFolderForTimer : _selectedRemoteFolderForTimer;
                        string destPath = _currentTransferMode == "Upload" ? _timerUploadDestination : _timerDownloadDestination;
                        
                        if (_isEditMode && timerJobManager.GetRegisteredTimerJobs().Contains(_currentJob.Id))
                        {
                            // Update existing timer job
                            bool updated = timerJobManager.UpdateTimerJob(_currentJob.Id, _currentJob.Name,
                                sourcePath, destPath, intervalMs);
                            
                            if (updated)
                            {
                                ServiceLocator.LogService.LogInfo(string.Format(
                                    "Timer job '{0}' updated successfully", _currentJob.Name));
                            }
                        }
                        else
                        {
                            // Register new timer job
                            bool registered = timerJobManager.RegisterTimerJob(_currentJob.Id, _currentJob.Name,
                                sourcePath, destPath, intervalMs);
                            
                            bool isTimerCurrentlyRunning = (_currentTransferMode == "Upload" && _isTimerRunning) ||
                                                         (_currentTransferMode == "Download" && _isDownloadTimerRunning);
                            
                            if (registered && isTimerCurrentlyRunning)
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
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError(string.Format(
                        "Failed to register/update job '{0}' with timer service: {1}", _currentJob.Name, ex.Message));
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
            ServiceLocator.LogService.LogInfo($"Browse button clicked - Current transfer mode: {_currentTransferMode}");
            
            if (_currentTransferMode == "Upload")
            {
                ServiceLocator.LogService.LogInfo("Routing to local folder browser for upload mode");
                BrowseLocalFolderForUpload();
            }
            else if (_currentTransferMode == "Download")
            {
                ServiceLocator.LogService.LogInfo("Routing to remote file browser for download mode");
                BrowseRemoteFolderForDownloadImproved();
            }
            else
            {
                ServiceLocator.LogService.LogWarning($"Unknown transfer mode: {_currentTransferMode}");
            }
        }

        private void BrowseLocalFolderForUpload()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select local folder for timed uploads (all files will be monitored)";
                dialog.ShowNewFolderButton = true;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = dialog.SelectedPath;
                    
                    // Get all files in the selected folder (including subfolders)
                    string[] allFiles = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                    
                    // Apply filters to get realistic count
                    List<string> filteredFiles = new List<string>();
                    FilterSettings currentFilters = GetCurrentFilterSettings();
                    
                    if (currentFilters != null && currentFilters.FiltersEnabled)
                    {
                        foreach (string file in allFiles)
                        {
                            if (ShouldIncludeFileForTimer(file, currentFilters))
                            {
                                filteredFiles.Add(file);
                            }
                        }
                    }
                    else
                    {
                        filteredFiles.AddRange(allFiles);
                    }
                    
                    _selectedFilesForTimer = filteredFiles.ToArray();
                    
                    // Store the base folder path for relative path calculations during upload
                    _selectedFolderForTimer = folderPath;
                    
                    // Update the label to show selected folder and file count
                    if (lblNoFilesSelected != null)
                    {
                        string filterInfo = (currentFilters != null && currentFilters.FiltersEnabled) ? " (filtered)" : "";
                        
                        if (filteredFiles.Count == 0)
                        {
                            lblNoFilesSelected.Text = Path.GetFileName(folderPath) + " (empty/no matching files - will monitor for new files)" + filterInfo;
                        }
                        else if (filteredFiles.Count == 1)
                        {
                            lblNoFilesSelected.Text = Path.GetFileName(folderPath) + " (1 file, including new files added later)" + filterInfo;
                        }
                        else
                        {
                            lblNoFilesSelected.Text = Path.GetFileName(folderPath) + " (" + filteredFiles.Count + " files, including new files added later)" + filterInfo;
                        }
                    }
                    
                    string filterMessage = (currentFilters != null && currentFilters.FiltersEnabled) ? 
                        string.Format(" ({0} files after applying filters)", filteredFiles.Count) : "";
                    
                    ServiceLocator.LogService.LogInfo(string.Format("Selected local folder '{0}' with {1} files{2} for timer upload (will also include newly added files)", 
                        folderPath, allFiles.Length, filterMessage));
                    
                    // Ask for upload destination for timer uploads
                    AskForTimerUploadDestination();
                }
            }
        }

        private void BrowseRemoteFolderForDownload()
        {
            ServiceLocator.LogService.LogInfo("BrowseRemoteFolderForDownload method called");
            
            // Debug connection settings
            ServiceLocator.LogService.LogInfo($"Connection settings null? {_coreConnectionSettings == null}");
            if (_coreConnectionSettings != null)
            {
                ServiceLocator.LogService.LogInfo($"Protocol: {_coreConnectionSettings.Protocol}");
            }
            
            // Debug connection service
            ServiceLocator.LogService.LogInfo($"Connection service null? {_connectionService == null}");
            if (_connectionService != null)
            {
                ServiceLocator.LogService.LogInfo($"Is connected: {_connectionService.IsConnected()}");
            }
            
            // Debug transfer client
            ServiceLocator.LogService.LogInfo($"Transfer client null? {_currentTransferClient == null}");
            
            if (!ValidateConnection()) 
            {
                ServiceLocator.LogService.LogWarning("Connection validation failed in BrowseRemoteFolderForDownload");
                return;
            }

            try
            {
                ServiceLocator.LogService.LogInfo("Creating FormSimpleDirectoryBrowser for remote browsing");
                using (FormSimpleDirectoryBrowser remoteBrowser = new FormSimpleDirectoryBrowser(_coreConnectionSettings))
                {
                    remoteBrowser.Text = "Select Remote Files/Folder for Download Timer";
                    remoteBrowser.IsRemoteMode = true;
                    remoteBrowser.IsDownloadMode = true; // Enable file selection for download mode
                    remoteBrowser.AllowFileSelection = true; // Allow file selection
                    
                    ServiceLocator.LogService.LogInfo($"Remote browser configured - IsRemoteMode: {remoteBrowser.IsRemoteMode}, IsDownloadMode: {remoteBrowser.IsDownloadMode}, AllowFileSelection: {remoteBrowser.AllowFileSelection}");
                    
                    if (remoteBrowser.ShowDialog() == DialogResult.OK)
                    {
                        if (!string.IsNullOrEmpty(remoteBrowser.SelectedPath))
                        {
                            // Check if a specific file was selected
                            if (remoteBrowser.IsFileSelected && !string.IsNullOrEmpty(remoteBrowser.SelectedFileName))
                            {
                                // Store the selected remote file information
                                _selectedRemoteFolderForTimer = remoteBrowser.SelectedPath;
                                _selectedRemoteFileForTimer = remoteBrowser.SelectedFileName;
                                string selectedFilePath = _selectedRemoteFolderForTimer.EndsWith("/") ? 
                                    _selectedRemoteFolderForTimer + remoteBrowser.SelectedFileName : 
                                    _selectedRemoteFolderForTimer + "/" + remoteBrowser.SelectedFileName;
                                
                                // Update the label to show selected remote file
                                if (lblNoFilesSelected != null)
                                {
                                    lblNoFilesSelected.Text = "Remote File: " + remoteBrowser.SelectedFileName + 
                                        " (will download periodically)";
                                }
                                
                                ServiceLocator.LogService.LogInfo($"Selected remote file '{selectedFilePath}' for timer download");
                            }
                            else
                            {
                                // Store the selected remote folder (existing behavior)
                                _selectedRemoteFolderForTimer = remoteBrowser.SelectedPath;
                                _selectedRemoteFileForTimer = null; // Clear any previously selected file
                                
                                // Update the label to show selected remote folder
                                if (lblNoFilesSelected != null)
                                {
                                    lblNoFilesSelected.Text = "Remote Folder: " + System.IO.Path.GetFileName(_selectedRemoteFolderForTimer) + 
                                        " (will monitor for new files)";
                                }
                                
                                ServiceLocator.LogService.LogInfo($"Selected remote folder '{_selectedRemoteFolderForTimer}' for timer download");
                            }
                            
                            // Ask for local download destination
                            AskForTimerDownloadDestination();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening remote file browser: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error browsing remote files: " + ex.Message);
            }
        }

        private void BrowseRemoteFolderForDownloadImproved()
        {
            ServiceLocator.LogService.LogInfo("BrowseRemoteFolderForDownloadImproved started");
            
            // Check if connection settings exist
            if (_coreConnectionSettings == null)
            {
                MessageBox.Show("No connection settings available. Please configure connection settings first.", 
                    "Connection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // For local protocol, redirect to local browser
            if (_coreConnectionSettings.Protocol == syncer.core.ProtocolType.Local)
            {
                ServiceLocator.LogService.LogInfo("Local protocol detected, using local folder browser");
                BrowseLocalFolderForUpload(); // Use same logic but for download destination
                return;
            }

            // Attempt to connect if not already connected
            if (!_connectionService.IsConnected())
            {
                ServiceLocator.LogService.LogInfo("Not connected, attempting to test connection");
                try
                {
                    // Try to test connection using the current connection settings
                    // Get UI connection settings since TestConnection expects UI ConnectionSettings
                    var uiConnectionSettings = _connectionService.GetConnectionSettings();
                    if (_connectionService.TestConnection(uiConnectionSettings))
                    {
                        ServiceLocator.LogService.LogInfo("Connection test successful");
                        
                        // Initialize transfer client after successful connection test
                        UpdateTransferClient();
                        
                        if (_currentTransferClient == null)
                        {
                            ServiceLocator.LogService.LogError("Failed to initialize transfer client after connection test");
                            MessageBox.Show("Failed to initialize transfer client.", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        ServiceLocator.LogService.LogError("Connection test failed");
                        MessageBox.Show("Could not connect to remote server. Please check your connection settings.", 
                            "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError($"Exception during connection test: {ex.Message}");
                    MessageBox.Show($"Error testing connection: {ex.Message}", "Connection Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Now proceed with remote browsing
            try
            {
                ServiceLocator.LogService.LogInfo("Opening remote file browser");
                using (FormSimpleDirectoryBrowser remoteBrowser = new FormSimpleDirectoryBrowser(_coreConnectionSettings))
                {
                    remoteBrowser.Text = "Select Remote Files/Folder for Download Timer";
                    remoteBrowser.IsRemoteMode = true;
                    remoteBrowser.IsDownloadMode = true; // Enable file selection for download mode
                    remoteBrowser.AllowFileSelection = true; // Allow file selection
                    
                    ServiceLocator.LogService.LogInfo($"Remote browser configured - IsRemoteMode: {remoteBrowser.IsRemoteMode}, IsDownloadMode: {remoteBrowser.IsDownloadMode}, AllowFileSelection: {remoteBrowser.AllowFileSelection}");
                    
                    if (remoteBrowser.ShowDialog() == DialogResult.OK)
                    {
                        if (!string.IsNullOrEmpty(remoteBrowser.SelectedPath))
                        {
                            // Check if a specific file was selected
                            if (remoteBrowser.IsFileSelected && !string.IsNullOrEmpty(remoteBrowser.SelectedFileName))
                            {
                                // Store the selected remote file information
                                _selectedRemoteFolderForTimer = remoteBrowser.SelectedPath;
                                _selectedRemoteFileForTimer = remoteBrowser.SelectedFileName;
                                string selectedFilePath = _selectedRemoteFolderForTimer.EndsWith("/") ? 
                                    _selectedRemoteFolderForTimer + remoteBrowser.SelectedFileName : 
                                    _selectedRemoteFolderForTimer + "/" + remoteBrowser.SelectedFileName;
                                
                                // Update the label to show selected remote file
                                if (lblNoFilesSelected != null)
                                {
                                    lblNoFilesSelected.Text = "Remote File: " + remoteBrowser.SelectedFileName + 
                                        " (will download periodically)";
                                }
                                
                                ServiceLocator.LogService.LogInfo($"Selected remote file '{selectedFilePath}' for timer download");
                            }
                            else
                            {
                                // Store the selected remote folder (existing behavior)
                                _selectedRemoteFolderForTimer = remoteBrowser.SelectedPath;
                                _selectedRemoteFileForTimer = null; // Clear any previously selected file
                                
                                // Update the label to show selected remote folder
                                if (lblNoFilesSelected != null)
                                {
                                    lblNoFilesSelected.Text = "Remote Folder: " + System.IO.Path.GetFileName(_selectedRemoteFolderForTimer) + 
                                        " (will monitor for new files)";
                                }
                                
                                ServiceLocator.LogService.LogInfo($"Selected remote folder '{_selectedRemoteFolderForTimer}' for timer download");
                            }
                            
                            // Ask for local download destination
                            AskForTimerDownloadDestination();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening remote file browser: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error browsing remote files: " + ex.Message);
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

        private void AskForTimerDownloadDestination()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select local folder for timer downloads";
                dialog.ShowNewFolderButton = true;
                
                if (!string.IsNullOrEmpty(_timerDownloadDestination))
                {
                    dialog.SelectedPath = _timerDownloadDestination;
                }
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _timerDownloadDestination = dialog.SelectedPath;
                    ServiceLocator.LogService.LogInfo("Timer download destination set to: " + _timerDownloadDestination);
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
                
                if (_currentTransferMode == "Upload")
                {
                    StartUploadTimer(intervalMs);
                }
                else if (_currentTransferMode == "Download")
                {
                    StartDownloadTimer(intervalMs);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting timer: " + ex.Message, "Timer Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error starting timer: " + ex.Message);
            }
        }

        private void StartUploadTimer(double intervalMs)
        {
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
            if (lblTimerStatus != null) lblTimerStatus.Text = "Upload timer running";
            if (btnStartTimer != null) btnStartTimer.Enabled = false;
            if (btnStopTimer != null) btnStopTimer.Enabled = true;
            
            // Ask if user wants to save this job configuration
            DialogResult saveResult = MessageBox.Show(
                "Upload timer started successfully! Do you want to save this job configuration for future use?", 
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

        private void StartDownloadTimer(double intervalMs)
        {
            if (_downloadTimer == null)
            {
                _downloadTimer = new System.Timers.Timer();
                _downloadTimer.Elapsed += OnDownloadTimerElapsed;
                _downloadTimer.AutoReset = true;
            }
            
            _downloadTimer.Interval = intervalMs;
            _downloadTimer.Start();
            _isDownloadTimerRunning = true;
            
            // Update UI
            if (lblTimerStatus != null) lblTimerStatus.Text = "Download timer running";
            if (btnStartTimer != null) btnStartTimer.Enabled = false;
            if (btnStopTimer != null) btnStopTimer.Enabled = true;
            
            // Ask if user wants to save this job configuration
            DialogResult saveResult = MessageBox.Show(
                "Download timer started successfully! Do you want to save this job configuration for future use?", 
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
                    "Download timer started successfully! Note: This timer will stop if the application is closed.",
                    "Timer Started",
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
            }
            
            ServiceLocator.LogService.LogInfo(string.Format("Download timer started with interval: {0} ms", intervalMs));
        }

        private void btnStopTimer_Click(object sender, EventArgs e)
        {
            try
            {
                bool timerStopped = false;
                string timerType = "";
                
                if (_uploadTimer != null && _isTimerRunning)
                {
                    _uploadTimer.Stop();
                    _isTimerRunning = false;
                    timerType = "Upload";
                    timerStopped = true;
                }
                
                if (_downloadTimer != null && _isDownloadTimerRunning)
                {
                    _downloadTimer.Stop();
                    _isDownloadTimerRunning = false;
                    timerType = "Download";
                    timerStopped = true;
                }
                
                if (timerStopped)
                {
                    // Update UI
                    if (lblTimerStatus != null) lblTimerStatus.Text = "Timer stopped";
                    if (btnStartTimer != null) btnStartTimer.Enabled = chkEnableTimer != null && chkEnableTimer.Checked;
                    if (btnStopTimer != null) btnStopTimer.Enabled = false;
                    
                    MessageBox.Show($"{timerType} timer stopped.", "Timer Stopped", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    ServiceLocator.LogService.LogInfo($"{timerType} timer stopped");
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
            
            if (_currentTransferMode == "Upload")
            {
                if (string.IsNullOrEmpty(_selectedFolderForTimer))
                {
                    MessageBox.Show("Please select a local folder for timer upload using 'Browse Files' button.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (btnBrowseFilesForTimer != null) btnBrowseFilesForTimer.Focus();
                    return false;
                }
            }
            else if (_currentTransferMode == "Download")
            {
                if (string.IsNullOrEmpty(_selectedRemoteFolderForTimer))
                {
                    MessageBox.Show("Please select a remote folder for timer download using 'Browse Files' button.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (btnBrowseFilesForTimer != null) btnBrowseFilesForTimer.Focus();
                    return false;
                }
                
                if (string.IsNullOrEmpty(_timerDownloadDestination))
                {
                    MessageBox.Show("Please select a local destination folder for downloads.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
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
                ServiceLocator.LogService.LogInfo("Upload timer elapsed - starting automatic upload");
                
                // Run upload on UI thread
                this.Invoke(new Action(() =>
                {
                    PerformAutomaticUpload();
                }));
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Upload timer elapsed error: " + ex.Message);
            }
        }

        private void OnDownloadTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                ServiceLocator.LogService.LogInfo("Download timer elapsed - starting automatic download");
                
                // Run download on UI thread
                this.Invoke(new Action(() =>
                {
                    PerformAutomaticDownload();
                }));
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Download timer elapsed error: " + ex.Message);
            }
        }

        private void PerformAutomaticUpload()
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedFolderForTimer))
                {
                    ServiceLocator.LogService.LogWarning("No folder selected for automatic upload");
                    return;
                }
                
                // Get all files in the directory, including any newly added files
                string[] allFiles = Directory.GetFiles(_selectedFolderForTimer, "*", SearchOption.AllDirectories);
                
                // Apply filters if they are configured
                List<string> filteredFiles = new List<string>();
                FilterSettings currentFilters = GetCurrentFilterSettings();
                
                if (currentFilters != null && currentFilters.FiltersEnabled)
                {
                    foreach (string file in allFiles)
                    {
                        // Use the same filtering logic as the service
                        if (ShouldIncludeFileForTimer(file, currentFilters))
                        {
                            filteredFiles.Add(file);
                        }
                    }
                }
                else
                {
                    // No filters, include all files
                    filteredFiles.AddRange(allFiles);
                }
                
                string[] currentFiles = filteredFiles.ToArray();
                
                if (currentFiles.Length == 0)
                {
                    ServiceLocator.LogService.LogInfo("No files found (after filtering) in selected folder for automatic upload - will retry on next timer interval");
                    return;
                }
                
                ServiceLocator.LogService.LogInfo(string.Format("Starting automatic upload of folder '{0}' with {1} files (including any newly added)", 
                    _selectedFolderForTimer, currentFiles.Length));
                
                // Upload all files including newly added ones
                PerformFolderUpload(_selectedFolderForTimer, currentFiles, _timerUploadDestination);
                
                // Update last upload time
                _lastUploadTime = DateTime.Now;
                // Note: Last upload time display removed for simplified interface
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Automatic upload error: " + ex.Message);
            }
        }

        private void PerformAutomaticDownload()
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedRemoteFolderForTimer))
                {
                    ServiceLocator.LogService.LogWarning("No remote folder selected for automatic download");
                    return;
                }
                
                if (string.IsNullOrEmpty(_timerDownloadDestination))
                {
                    ServiceLocator.LogService.LogWarning("No local destination selected for automatic download");
                    return;
                }
                
                ServiceLocator.LogService.LogInfo($"Starting automatic download from remote location '{_selectedRemoteFolderForTimer}' to local folder '{_timerDownloadDestination}'");
                
                // Check if we're monitoring a specific file
                bool isSpecificFile = !string.IsNullOrEmpty(_selectedRemoteFileForTimer) ||
                    (lblNoFilesSelected != null && lblNoFilesSelected.Text.StartsWith("Remote File:"));
                
                if (isSpecificFile)
                {
                    // Download specific file
                    PerformSpecificFileDownload();
                }
                else
                {
                    // Monitor and download from folder
                    PerformFolderMonitoringDownload();
                }
                
                // Update last download time
                _lastDownloadTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Automatic download error: " + ex.Message);
            }
        }
        
        private void PerformSpecificFileDownload()
        {
            try
            {
                // Use stored filename if available, otherwise extract from label
                string fileName = _selectedRemoteFileForTimer;
                
                if (string.IsNullOrEmpty(fileName) && lblNoFilesSelected != null && lblNoFilesSelected.Text.StartsWith("Remote File:"))
                {
                    // Extract filename from label: "Remote File: filename.ext (will download periodically)"
                    string labelText = lblNoFilesSelected.Text;
                    int start = labelText.IndexOf(": ") + 2;
                    int end = labelText.IndexOf(" (will download");
                    if (start > 1 && end > start)
                    {
                        fileName = labelText.Substring(start, end - start);
                    }
                }
                
                if (string.IsNullOrEmpty(fileName))
                {
                    ServiceLocator.LogService.LogError("Could not determine specific file name for download");
                    return;
                }
                
                // Construct full remote file path
                string remoteFilePath = _selectedRemoteFolderForTimer.EndsWith("/") ? 
                    _selectedRemoteFolderForTimer + fileName : 
                    _selectedRemoteFolderForTimer + "/" + fileName;
                
                // Construct local file path
                string localFilePath = Path.Combine(_timerDownloadDestination, fileName);
                
                ServiceLocator.LogService.LogInfo($"Downloading specific file '{remoteFilePath}' to '{localFilePath}'");
                
                // Download the specific file
                string error;
                bool success = _currentTransferClient.DownloadFile(_coreConnectionSettings, remoteFilePath, localFilePath, false, out error);
                
                if (success)
                {
                    ServiceLocator.LogService.LogInfo($"Successfully downloaded file '{fileName}'");
                }
                else
                {
                    ServiceLocator.LogService.LogError($"Failed to download file '{fileName}': {error}");
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError($"Error downloading specific file: {ex.Message}");
            }
        }
        
        private void PerformFolderMonitoringDownload()
        {
            try
            {
                // Get list of files from remote directory
                List<string> remoteFiles;
                string error;
                bool success = _currentTransferClient.ListFiles(_coreConnectionSettings, _selectedRemoteFolderForTimer, out remoteFiles, out error);
                
                if (!success)
                {
                    ServiceLocator.LogService.LogError($"Failed to list remote files: {error}");
                    return;
                }
                
                if (remoteFiles == null || remoteFiles.Count == 0)
                {
                    ServiceLocator.LogService.LogInfo("No files found in remote folder for automatic download - will retry on next timer interval");
                    return;
                }
                
                ServiceLocator.LogService.LogInfo($"Found {remoteFiles.Count} files in remote folder for download");
                
                // Download all files
                PerformFolderDownload(remoteFiles.ToArray(), _selectedRemoteFolderForTimer, _timerDownloadDestination);
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError($"Error in folder monitoring download: {ex.Message}");
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
            string enabled = chkEnableJob.Checked ? "Enabled" : "Disabled";
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
                   "Source: " + (_selectedFolderForTimer ?? "No folder selected") + "\n" +
                   "Destination: " + (_timerUploadDestination ?? "/") + "\n" +
                   "Timer: " + timerEnabled + "\n" +
                   "Upload Interval: " + interval + "\n" +
                   "Timer Status: " + timerStatus + "\n" +
                   "Last Upload: " + lastUpload;
        }
        
        private void btnSaveTimerJob_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFolderForTimer))
            {
                MessageBox.Show("Please select a folder for timer upload first.", 
                    "No Folder Selected", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                return;
            }
            
            if (UIStringExtensions.IsNullOrWhiteSpace(txtJobName.Text))
            {
                MessageBox.Show("Please enter a job name.", 
                    "Job Name Required", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                txtJobName.Focus();
                return;
            }
            
            try
            {
                // Save the job using traditional method
                SaveJob();
                
                // Also save as a reusable configuration
                SaveAsConfiguration();
                
                MessageBox.Show(
                    "Timer job saved successfully! The job configuration has also been saved for future use.\n\nYou can load this configuration later from the File menu.",
                    "Timer Job Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                
                ServiceLocator.LogService.LogInfo(string.Format(
                    "Timer job '{0}' saved for folder: {1}", 
                    txtJobName.Text, 
                    _selectedFolderForTimer));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error saving timer job: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error saving timer job: " + ex.Message);
            }
        }

        private void SaveAsConfiguration()
        {
            try
            {
                var configService = ServiceLocator.SavedJobConfigurationService;
                var connectionService = ServiceLocator.ConnectionService;
                
                // Get current connection settings
                var currentConnection = connectionService.GetConnectionSettings();
                if (currentConnection == null)
                {
                    // Create a default connection if none exists
                    currentConnection = new ConnectionSettings();
                }
                
                // Create a SyncJob from current form data
                var currentJob = CreateSyncJobFromForm();
                
                // Open the save configuration form
                using (var saveForm = new Forms.FormSaveJobConfiguration(
                    currentJob, 
                    currentConnection, 
                    currentConnection, // Using same connection for both source and destination
                    configService,
                    connectionService))
                {
                    // Pre-fill with current job name and auto-generated description
                    if (saveForm.ShowDialog() == DialogResult.OK)
                    {
                        ServiceLocator.LogService.LogInfo($"Configuration saved for job '{txtJobName.Text}'", "UI");
                    }
                }
            }
            catch (Exception ex)
            {
                // Don't throw exception if configuration saving fails - the main job save should still work
                ServiceLocator.LogService.LogError($"Error saving configuration: {ex.Message}", "UI");
            }
        }

        private SyncJob CreateSyncJobFromForm()
        {
            var job = new SyncJob
            {
                Name = txtJobName.Text.Trim(),
                Description = $"Timer job for uploading from {_selectedFolderForTimer}",
                SourcePath = _selectedFolderForTimer ?? "",
                DestinationPath = _timerUploadDestination ?? "/",
                IsEnabled = chkEnableJob.Checked,
                IncludeSubFolders = true, // Default value
                OverwriteExisting = true, // Default value
                IntervalValue = (int)numTimerInterval.Value,
                IntervalType = "Minutes", // Based on the form's timer setting
                TransferMode = "Upload", // This is a timer upload job
                CreatedDate = DateTime.Now,
                LastStatus = "Ready to run",
                ShowTransferProgress = true,
                ValidateTransfer = true,
                MaxRetries = 3,
                RetryDelaySeconds = 5
            };
            
            // Apply filter settings if available
            if (_jobFilterSettings != null)
            {
                job.FilterSettings = _jobFilterSettings;
            }
            
            return job;
        }

        private void btnLoadConfiguration_Click(object sender, EventArgs e)
        {
            try
            {
                var configService = ServiceLocator.SavedJobConfigurationService;
                var connectionService = ServiceLocator.ConnectionService;
                var timerJobManager = ServiceLocator.TimerJobManager;
                
                // Open the load configuration form
                using (var loadForm = new Forms.FormLoadJobConfiguration(
                    configService, 
                    connectionService, 
                    timerJobManager))
                {
                    if (loadForm.ShowDialog() == DialogResult.OK && loadForm.LoadedConfiguration != null)
                    {
                        var config = loadForm.LoadedConfiguration;
                        
                        // Apply the loaded configuration to the form
                        ApplyConfigurationToForm(config);
                        
                        // If user chose to start the job automatically
                        if (loadForm.StartJobAfterLoad)
                        {
                            StartTimerJobFromConfiguration(config);
                        }
                        
                        MessageBox.Show($"Configuration '{config.DisplayName}' loaded successfully!", 
                            "Configuration Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                        ServiceLocator.LogService.LogInfo($"Configuration '{config.DisplayName}' loaded in FormSchedule", "UI");
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

        private void ApplyConfigurationToForm(SavedJobConfiguration config)
        {
            try
            {
                if (config.JobSettings != null)
                {
                    // Apply job settings to form controls
                    txtJobName.Text = config.JobSettings.Name ?? "";
                    chkEnableJob.Checked = config.JobSettings.IsEnabled;
                    
                    // Set source and destination paths
                    _selectedFolderForTimer = config.JobSettings.SourcePath;
                    _timerUploadDestination = config.JobSettings.DestinationPath;
                    
                    // Set timer interval
                    if (config.JobSettings.IntervalType?.ToLower() == "minutes")
                    {
                        numTimerInterval.Value = Math.Max(1, Math.Min((decimal)numTimerInterval.Maximum, config.JobSettings.IntervalValue));
                    }
                    else if (config.JobSettings.IntervalType?.ToLower() == "seconds")
                    {
                        // Convert seconds to minutes
                        var minutes = Math.Max(1, config.JobSettings.IntervalValue / 60);
                        numTimerInterval.Value = Math.Max(1, Math.Min((decimal)numTimerInterval.Maximum, minutes));
                    }
                    
                    // Apply filter settings if available
                    if (config.JobSettings.FilterSettings != null)
                    {
                        _jobFilterSettings = config.JobSettings.FilterSettings;
                    }
                }
                
                // Apply connection settings
                if (config.SourceConnection?.Settings != null)
                {
                    var connectionService = ServiceLocator.ConnectionService;
                    connectionService.SaveConnectionSettings(config.SourceConnection.Settings);
                    
                    // Refresh connection display or status if needed
                    InitializeTransferClient();
                }
                
                // Update form display
                UpdateFormDisplayForLoadedConfiguration(config);
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError($"Error applying configuration to form: {ex.Message}", "UI");
                throw;
            }
        }

        private void UpdateFormDisplayForLoadedConfiguration(SavedJobConfiguration config)
        {
            // Update the form title to show loaded configuration
            if (!string.IsNullOrEmpty(config.Name))
            {
                this.Text = $"Timer Job - {config.Name} (Loaded Configuration)";
            }
            
            // Enable/disable controls based on configuration
            if (!string.IsNullOrEmpty(_selectedFolderForTimer))
            {
                // Update any status labels or displays to show the selected folder
                ServiceLocator.LogService.LogInfo($"Folder selected from configuration: {_selectedFolderForTimer}", "UI");
            }
        }

        private void StartTimerJobFromConfiguration(SavedJobConfiguration config)
        {
            try
            {
                if (config.JobSettings != null && !string.IsNullOrEmpty(_selectedFolderForTimer))
                {
                    var intervalMs = GetIntervalInMilliseconds(config.JobSettings.IntervalValue, config.JobSettings.IntervalType);
                    
                    // Start the timer job similar to btnStartTimer_Click
                    if (_uploadTimer == null)
                    {
                        _uploadTimer = new System.Timers.Timer();
                        _uploadTimer.Elapsed += OnTimerElapsed;
                        _uploadTimer.AutoReset = true;
                    }
                    
                    _uploadTimer.Interval = intervalMs;
                    _uploadTimer.Start();
                    _isTimerRunning = true;
                    
                    // Update UI if controls exist
                    try
                    {
                        if (lblTimerStatus != null) lblTimerStatus.Text = "Timer running (from loaded config)";
                        if (btnStartTimer != null) btnStartTimer.Enabled = false;
                        if (btnStopTimer != null) btnStopTimer.Enabled = true;
                    }
                    catch
                    {
                        // Ignore UI update errors
                    }
                    
                    ServiceLocator.LogService.LogInfo($"Timer job started automatically from configuration: {config.DisplayName}", "UI");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting timer job: {ex.Message}", "Start Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ServiceLocator.LogService.LogError($"Error starting timer job from configuration: {ex.Message}", "UI");
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

            // Ask user if they want to upload files or a folder
            DialogResult choice = MessageBox.Show(
                "Do you want to upload:\n\n" +
                "YES = Individual files\n" +
                "NO = Entire folder\n" +
                "CANCEL = Cancel operation",
                "Upload Type Selection",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (choice == DialogResult.Cancel)
                return;

            if (choice == DialogResult.Yes)
            {
                // Upload individual files
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
            else if (choice == DialogResult.No)
            {
                // Upload entire folder
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Select folder to upload (all files and subfolders will be uploaded)";
                    dialog.ShowNewFolderButton = false;
                    
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string folderPath = dialog.SelectedPath;
                        
                        // Get all files in the folder including subfolders
                        string[] allFiles = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                        
                        if (allFiles.Length == 0)
                        {
                            MessageBox.Show("The selected folder is empty. No files to upload.", 
                                "Empty Folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        
                        DialogResult confirmResult = MessageBox.Show(
                            string.Format("Found {0} files in the selected folder.\n\nDo you want to upload all files?", allFiles.Length),
                            "Confirm Folder Upload",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                            
                        if (confirmResult == DialogResult.Yes)
                        {
                            // Ask for upload destination
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
                                buttonOk.Text = "OK"; 
                                buttonOk.Left = 220; 
                                buttonOk.Top = 90; 
                                buttonOk.DialogResult = DialogResult.OK;
                                
                                Button buttonCancel = new Button(); 
                                buttonCancel.Text = "Cancel"; 
                                buttonCancel.Left = 320; 
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
                                    if (string.IsNullOrEmpty(remotePath))
                                        remotePath = "/";
                                        
                                    PerformFolderUpload(folderPath, allFiles, remotePath);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnDownloadFile_Click(object sender, EventArgs e)
        {
            if (!ValidateConnection()) return;

            // Open file manager in download mode
            try
            {
                using (FormSimpleDirectoryBrowser fileManager = new FormSimpleDirectoryBrowser(_coreConnectionSettings))
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
                using (FormSimpleDirectoryBrowser fileManager = new FormSimpleDirectoryBrowser(_coreConnectionSettings))
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

        private void PerformFolderDownload(string[] remoteFilePaths, string baseRemotePath, string localDestinationPath)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (sender, e) =>
            {
                try
                {
                    ServiceLocator.LogService.LogInfo($"Starting download of {remoteFilePaths.Length} files from {baseRemotePath} to {localDestinationPath}");
                    ServiceLocator.LogService.LogInfo($"Connection: {_coreConnectionSettings.Username}@{_coreConnectionSettings.Host}:{_coreConnectionSettings.Port}");
                    
                    // Ensure local destination directory exists
                    if (!Directory.Exists(localDestinationPath))
                    {
                        Directory.CreateDirectory(localDestinationPath);
                        ServiceLocator.LogService.LogInfo($"Created local destination directory: {localDestinationPath}");
                    }
                    
                    int successCount = 0;
                    int failureCount = 0;
                    long totalBytes = 0;
                    
                    for (int i = 0; i < remoteFilePaths.Length; i++)
                    {
                        string remoteFile = remoteFilePaths[i];
                        string fileName = Path.GetFileName(remoteFile);
                        
                        // Calculate local file path
                        string localFile = Path.Combine(localDestinationPath, fileName);
                        
                        worker.ReportProgress((i * 100) / remoteFilePaths.Length, 
                            $"Downloading {fileName}...");
                        
                        ServiceLocator.LogService.LogInfo($"Downloading: {remoteFile} -> {localFile}");
                        
                        try
                        {
                            string downloadError;
                            bool success = _currentTransferClient.DownloadFile(_coreConnectionSettings, remoteFile, localFile, true, out downloadError);
                            
                            if (!success)
                            {
                                string errorMsg = $"Failed to download {fileName}: {downloadError ?? "Unknown error"}";
                                ServiceLocator.LogService.LogError(errorMsg);
                                failureCount++;
                            }
                            else
                            {
                                ServiceLocator.LogService.LogInfo($"Successfully downloaded: {fileName}");
                                successCount++;
                                
                                // Get file size for statistics
                                if (File.Exists(localFile))
                                {
                                    FileInfo fileInfo = new FileInfo(localFile);
                                    totalBytes += fileInfo.Length;
                                }
                            }
                        }
                        catch (Exception fileEx)
                        {
                            ServiceLocator.LogService.LogError($"Error downloading {fileName}: {fileEx.Message}");
                            failureCount++;
                        }
                    }
                    
                    worker.ReportProgress(100, "Download completed");
                    ServiceLocator.LogService.LogInfo($"Folder download completed. Success: {successCount}, Failed: {failureCount}, Total bytes: {totalBytes}");
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError("Error during folder download: " + ex.Message);
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
                }
                else
                {
                    MessageBox.Show("Folder downloaded successfully!", "Download Complete", 
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

        #region Direct Transfer Methods

        private void btnDirectUpload_Click(object sender, EventArgs e)
        {
            // Same functionality as btnUploadFile_Click
            btnUploadFile_Click(sender, e);
        }

        private void btnDirectDownload_Click(object sender, EventArgs e)
        {
            // Same functionality as btnDownloadFile_Click
            btnDownloadFile_Click(sender, e);
        }

        #endregion
    }
}
