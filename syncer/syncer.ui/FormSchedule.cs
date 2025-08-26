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
        
        // Timer-based upload functionality
        private System.Timers.Timer _uploadTimer;
        private bool _isTimerRunning = false;
        private DateTime _lastUploadTime;
        private string[] _selectedFilesForTimer; // Store files selected for timer upload
        private string _selectedFolderForTimer; // Base folder path for relative path calculation
        private string _timerUploadDestination = "/"; // Default destination path

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
            this.Text = _isEditMode ? "Edit Upload Timer Job" : "Add Upload Timer Settings";
            this.Size = new Size(800, 320); // Wide enough to show both Timer Settings and File Manager sections
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Reorganize layout with better positioning
            ReorganizeFormLayout();
            
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
            
            // Position timer settings at the top left with better spacing
            if (gbTimerSettings != null)
            {
                gbTimerSettings.Location = new Point(12, 85);
                gbTimerSettings.Size = new Size(370, 120);
                gbTimerSettings.Text = "Upload Timer Settings";
                
                // Improve internal layout of timer controls
                AdjustTimerControlsLayout();
            }
            
            // Position file manager to the right of timer settings with proper spacing
            if (gbFileManager != null)
            {
                gbFileManager.Location = new Point(398, 85);
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
            lblTitle.Text = _isEditMode ? "Edit Upload Timer Job" : "Create New Upload Timer Job";
            lblTitle.Location = new Point(20, 15);
            lblTitle.Size = new Size(400, 25);
            lblTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.DarkBlue;

            // Subtitle
            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Configure automatic file uploads and manual transfers - Simple and reliable";
            lblSubtitle.Location = new Point(20, 40);
            lblSubtitle.Size = new Size(500, 20);
            lblSubtitle.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
            lblSubtitle.ForeColor = Color.DarkSlateGray;

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            this.Controls.Add(headerPanel);
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
                
                // Load timer settings (if available from job data)
                if (chkEnableTimer != null) chkEnableTimer.Checked = false; // Default for new timer feature
                if (numTimerInterval != null) numTimerInterval.Value = 5; // Default 5 minute interval
                if (cmbTimerUnit != null) cmbTimerUnit.SelectedIndex = 1; // Minutes
                
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
            
            // For timer jobs, use selected folder path
            _currentJob.SourcePath = _selectedFolderForTimer ?? "";
            _currentJob.DestinationPath = _timerUploadDestination ?? "/";
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
            
            // Save filter settings to the job
            _currentJob.FilterSettings = GetCurrentFilterSettings();
            
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
                    
                    ServiceLocator.LogService.LogInfo(string.Format("Selected folder '{0}' with {1} files{2} for timer upload (will also include newly added files)", 
                        folderPath, allFiles.Length, filterMessage));
                    
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
            
            if (string.IsNullOrEmpty(_selectedFolderForTimer))
            {
                MessageBox.Show("Please select a folder for timer upload using 'Browse Files' button.", "Validation Error", 
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
                // Save the job without closing the form
                SaveJob();
                
                MessageBox.Show(
                    "Timer job saved successfully! The job will continue running even if you close this window.",
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
