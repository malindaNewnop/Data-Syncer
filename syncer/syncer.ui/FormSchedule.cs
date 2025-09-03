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
using syncer.core.Services;
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
        
        // Override to allow resizing and full screen
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // Allow the form to be resized normally, set minimum size only
            if (width < 900) width = 900;
            if (height < 650) height = 650;  // Increased to accommodate filter section
            base.SetBoundsCore(x, y, width, height, specified);
        }
        
        // Timer-based upload functionality
        private System.Timers.Timer _uploadTimer;
        private bool _isTimerRunning = false;
        private DateTime _lastUploadTime;
        private string[] _selectedFilesForTimer; // Store files selected for timer upload
        private string _selectedFolderForTimer; // Base folder path for relative path calculation
        private string _timerUploadDestination = "/"; // Default destination path
        private bool _isUploadInProgress = false; // Prevent overlapping uploads
        private DateTime? _uploadStartTime = null; // Track upload duration
        private bool _includeSubfoldersForTimer = true; // Include subfolders in timer uploads (default: true)

        // Download mode fields
        private bool _isDownloadTimerRunning = false; // Track download timer state
        private System.Timers.Timer _downloadTimer; // Timer for automatic downloads
        private string _selectedRemoteFolderForTimer; // Remote folder selected for timer downloads
        private string _selectedRemoteFileForTimer; // Remote file selected for timer downloads  
        private string _timerDownloadDestination; // Local destination for timer downloads
        private bool _isDownloadInProgress = false; // Prevent overlapping downloads
        private DateTime? _downloadStartTime = null; // Track download duration
        private DateTime? _lastDownloadTime = null; // Track last download time

        // Both upload and download modes are supported
        private string _currentTransferMode = "Upload"; // Default to "Upload" mode

        // Filter controls - rebuilt for file extension filtering
        private GroupBox gbFilters;
        private CheckBox chkEnableFilters;
        private Label lblIncludeTypes;
        private TextBox txtIncludeFileTypes;
        private Label lblExcludeTypes;
        private TextBox txtExcludeFileTypes;
        private Label lblFilterHint;
        private CheckedListBox clbFileTypes; // Keep existing for compatibility
        private NumericUpDown numMinFileSize; // Keep existing for compatibility
        private NumericUpDown numMaxFileSize; // Keep existing for compatibility
        private ComboBox cmbFileSizeUnit; // Keep existing for compatibility
        private CheckBox chkIncludeHiddenFiles;
        private CheckBox chkIncludeSystemFiles;
        private CheckBox chkIncludeReadOnlyFiles;
        private TextBox txtExcludePatterns; // Keep existing for compatibility

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
                string timerType = _isTimerRunning ? "upload" : "download";
                DialogResult result = MessageBox.Show(
                    string.Format("You have a {0} timer running but haven't saved the job. The timer will stop when the form closes.\n\n" +
                    "Would you like to save this job before closing?", timerType),
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
            
            // Add download radio button to existing transfer mode group
            AddDownloadRadioButtonToTransferGroup();
            
            SetDefaultValues();
            if (_isEditMode) LoadJobSettings();
            InitializeTransferClient();
            InitializeTimerControls();
            InitializeFilterControls();
            
            // Initialize UI based on current transfer mode
            UpdateUIForTransferMode();
        }

        /*
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
        */

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

        private void AddDownloadRadioButtonToTransferGroup()
        {
            // Find the existing gbTransfer GroupBox and rbUpload radio button
            GroupBox gbTransfer = this.Controls.OfType<GroupBox>().FirstOrDefault(gb => gb.Name == "gbTransfer");
            RadioButton rbUpload = this.Controls.OfType<RadioButton>().FirstOrDefault(rb => rb.Name == "rbUpload");
            
            if (gbTransfer == null)
            {
                ServiceLocator.LogService.LogWarning("gbTransfer GroupBox not found");
                return;
            }

            // Expand the transfer group to accommodate both radio buttons
            gbTransfer.Size = new Size(870, 65); // Make it wider to fit both options
            gbTransfer.Text = "Transfer Mode";
            
            // Adjust existing Upload radio button position and text
            if (rbUpload != null)
            {
                rbUpload.Text = "Upload (Local → Remote)";
                rbUpload.Location = new Point(20, 25);
                rbUpload.Size = new Size(200, 25);
                rbUpload.CheckedChanged += RbUpload_CheckedChanged;
            }

            // Create Download radio button  
            RadioButton rbDownload = new RadioButton();
            rbDownload.Text = "Download (Remote → Local)";
            rbDownload.Location = new Point(240, 25);
            rbDownload.Size = new Size(220, 25);
            rbDownload.Checked = false;
            rbDownload.Font = new Font("Microsoft Sans Serif", 9F);
            rbDownload.CheckedChanged += RbDownload_CheckedChanged;
            rbDownload.Name = "rbDownload";
            
            // Add the download radio button to the existing transfer group
            gbTransfer.Controls.Add(rbDownload);
            
            ServiceLocator.LogService.LogInfo("Added Download radio button to existing Transfer Mode group");
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
            try
            {
                CreateFilterGroupBox();
                SetupFilterEventHandlers();
                UpdateFilterControlsVisibility();
            }
            catch (Exception ex)
            {
                // Log error but don't crash the form
                ServiceLocator.LogService?.LogError("Failed to initialize filter controls: " + ex.Message);
            }
        }

        private void CreateFilterGroupBox()
        {
            // Create the main filter groupbox
            gbFilters = new GroupBox();
            gbFilters.Text = "File Filtering";
            gbFilters.Location = new Point(15, 330);
            gbFilters.Size = new Size(870, 120);
            gbFilters.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(gbFilters);

            // Enable filter checkbox
            chkEnableFilters = new CheckBox();
            chkEnableFilters.Text = "Enable file filtering";
            chkEnableFilters.Location = new Point(10, 20);
            chkEnableFilters.Size = new Size(130, 20);
            chkEnableFilters.CheckedChanged += ChkEnableFilters_CheckedChanged;
            gbFilters.Controls.Add(chkEnableFilters);

            // Include file types
            lblIncludeTypes = new Label();
            lblIncludeTypes.Text = "Include only these file types:";
            lblIncludeTypes.Location = new Point(10, 45);
            lblIncludeTypes.Size = new Size(160, 15);
            gbFilters.Controls.Add(lblIncludeTypes);

            txtIncludeFileTypes = new TextBox();
            txtIncludeFileTypes.Location = new Point(175, 42);
            txtIncludeFileTypes.Size = new Size(300, 22);
            // Add tooltip for help since PlaceholderText is not available in .NET 3.5
            ToolTip toolTip1 = new ToolTip();
            toolTip1.SetToolTip(txtIncludeFileTypes, "Enter file extensions separated by commas (e.g., pdf,jpg,png)");
            gbFilters.Controls.Add(txtIncludeFileTypes);

            // Exclude file types - positioned below include
            lblExcludeTypes = new Label();
            lblExcludeTypes.Text = "Exclude these file types:";
            lblExcludeTypes.Location = new Point(10, 70);
            lblExcludeTypes.Size = new Size(160, 15);
            gbFilters.Controls.Add(lblExcludeTypes);

            txtExcludeFileTypes = new TextBox();
            txtExcludeFileTypes.Location = new Point(175, 67);
            txtExcludeFileTypes.Size = new Size(300, 22);
            // Add tooltip for help since PlaceholderText is not available in .NET 3.5  
            ToolTip toolTip2 = new ToolTip();
            toolTip2.SetToolTip(txtExcludeFileTypes, "Enter file extensions separated by commas (e.g., tmp,bak,log)");
            gbFilters.Controls.Add(txtExcludeFileTypes);

            // Add help text
            lblFilterHint = new Label();
            lblFilterHint.Text = "Tip: Include takes priority. If both are specified, files matching include types will be transferred unless excluded.";
            lblFilterHint.Location = new Point(10, 95);
            lblFilterHint.Size = new Size(850, 15);
            lblFilterHint.ForeColor = Color.Gray;
            gbFilters.Controls.Add(lblFilterHint);
        }

        private void SetupFilterEventHandlers()
        {
            if (txtIncludeFileTypes != null)
                txtIncludeFileTypes.TextChanged += FilterSettings_Changed;
            
            if (txtExcludeFileTypes != null)
                txtExcludeFileTypes.TextChanged += FilterSettings_Changed;
        }

        private void ChkEnableFilters_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFilterControlsVisibility();
            FilterSettings_Changed(sender, e);
        }

        private void FilterSettings_Changed(object sender, EventArgs e)
        {
            // This will be handled when saving the job
            // Could add real-time validation here if needed
        }

        private void UpdateFilterControlsVisibility()
        {
            bool enableFiltering = chkEnableFilters?.Checked ?? false;
            
            if (lblIncludeTypes != null) lblIncludeTypes.Enabled = enableFiltering;
            if (txtIncludeFileTypes != null) txtIncludeFileTypes.Enabled = enableFiltering;
            if (lblExcludeTypes != null) lblExcludeTypes.Enabled = enableFiltering;
            if (txtExcludeFileTypes != null) txtExcludeFileTypes.Enabled = enableFiltering;
            if (lblFilterHint != null) lblFilterHint.Enabled = enableFiltering;
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

        private SearchOption GetSearchOption()
        {
            // Return search option based on subfolder inclusion checkbox
            return chkIncludeSubfolders != null && chkIncludeSubfolders.Checked ? 
                SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        }

 



        private void btnTestFilters_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Advanced filtering has been removed for simplicity.\nAll files in the selected folder will be uploaded automatically.", 
                "Filter Testing Disabled", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        private bool ShouldIncludeSubfolders()
        {
            // Return the checkbox state, default to true if checkbox doesn't exist or isn't checked
            return chkIncludeSubfolders != null && chkIncludeSubfolders.Checked;
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
                
                // Clear download-specific selections when switching to upload mode
                _selectedRemoteFolderForTimer = null;
                _selectedRemoteFileForTimer = null;
                _timerDownloadDestination = null;
                
                UpdateUIForTransferMode();
                
                ServiceLocator.LogService.LogInfo("Switched to Upload mode - cleared download settings");
                
                // Show helpful message to user about what to do next
                if (lblNoFilesSelected != null)
                {
                    lblNoFilesSelected.Text = "Click 'Browse Local Folder' to select files to upload";
                    lblNoFilesSelected.ForeColor = System.Drawing.Color.Blue;
                }
            }
        }
        
        private void RbDownload_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                _currentTransferMode = "Download";
                
                // Clear upload-specific selections when switching to download mode
                _selectedFolderForTimer = null;
                _selectedFilesForTimer = null;
                _timerUploadDestination = null;
                
                UpdateUIForTransferMode();
                
                ServiceLocator.LogService.LogInfo("Switched to Download mode - cleared upload settings");
                
                // Show helpful message to user about what to do next
                if (lblNoFilesSelected != null)
                {
                    lblNoFilesSelected.Text = "Click 'Browse Remote Folder' to select files to download";
                    lblNoFilesSelected.ForeColor = System.Drawing.Color.Green;
                }
            }
        }

        private void UpdateUIForTransferMode()
        {
            // Update timer settings label based on mode
            if (gbTimerSettings != null)
            {
                gbTimerSettings.Text = _currentTransferMode + " Timer Settings";
            }

            // Update file manager groupbox title to be more descriptive
            if (gbFileManager != null)
            {
                if (_currentTransferMode == "Upload")
                {
                    gbFileManager.Text = "File Manager (Local → Remote)";
                }
                else if (_currentTransferMode == "Download")
                {
                    gbFileManager.Text = "File Manager (Remote → Local)";
                }
                else
                {
                    gbFileManager.Text = "File Manager";
                }
            }

            // Show/hide controls based on transfer mode
            if (_currentTransferMode == "Upload")
            {
                // Show upload controls
                if (btnBrowseFilesForTimer != null)
                {
                    btnBrowseFilesForTimer.Visible = true;
                    btnBrowseFilesForTimer.Text = "Browse Local Folder";
                }
                
                // Hide download controls
                if (lblRemotePath != null) lblRemotePath.Visible = false;
                if (txtRemotePath != null) txtRemotePath.Visible = false;
                if (btnBrowseLocalFolder != null) btnBrowseLocalFolder.Visible = false;
                
                // Update file selection label to provide guidance
                if (lblFileSelection != null)
                {
                    lblFileSelection.Text = "File Selection: Select a local folder and configure filters to automatically upload " +
                                           "only specific file types at regular intervals.";
                }
                
                if (lblNoFilesSelected != null)
                {
                    lblNoFilesSelected.Text = "Click 'Browse Local Folder' to select files to upload";
                    lblNoFilesSelected.ForeColor = System.Drawing.Color.Blue;
                }
            }
            else if (_currentTransferMode == "Download")
            {
                // Hide upload controls
                if (btnBrowseFilesForTimer != null)
                {
                    btnBrowseFilesForTimer.Visible = false;
                }
                
                // Show download controls
                if (lblRemotePath != null) lblRemotePath.Visible = true;
                if (txtRemotePath != null) txtRemotePath.Visible = true;
                if (btnBrowseLocalFolder != null) btnBrowseLocalFolder.Visible = true;
                
                // Update file selection label for download mode
                if (lblFileSelection != null)
                {
                    lblFileSelection.Text = "File Selection: Enter the remote path to download from, then select " +
                                           "a local destination folder for automatic downloads.";
                }
                
                if (lblNoFilesSelected != null)
                {
                    lblNoFilesSelected.Text = "Enter remote path above and click 'Browse Local' to set download folder";
                    lblNoFilesSelected.ForeColor = System.Drawing.Color.Blue;
                }
            }

            // Also update via groupbox search as backup (for any dynamically created buttons)
            foreach (Control control in this.Controls)
            {
                if (control is GroupBox && ((GroupBox)control).Text.Contains("File Manager"))
                {
                    foreach (Control innerControl in control.Controls)
                    {
                        if (innerControl is Button)
                        {
                            Button btn = innerControl as Button;
                            if (btn.Text.Contains("Browse") && btn != btnBrowseFilesForTimer && btn != btnBrowseLocalFolder) // Don't double-update
                            {
                                btn.Text = _currentTransferMode == "Upload" ? "Browse Local Folder" : "Browse Remote Folder";
                            }
                        }
                    }
                    break;
                }
            }

            ServiceLocator.LogService.LogInfo(string.Format("Transfer mode changed to: {0}", _currentTransferMode));
        }

        #endregion

        private RadioButton FindDownloadRadioButton()
        {
            // Look for the download radio button in the form controls
            foreach (Control control in this.Controls)
            {
                if (control is RadioButton && ((RadioButton)control).Name == "rbDownload")
                {
                    return (RadioButton)control;
                }
                else if (control is Panel)
                {
                    foreach (Control innerControl in control.Controls)
                    {
                        if (innerControl is RadioButton && ((RadioButton)innerControl).Name == "rbDownload")
                        {
                            return (RadioButton)innerControl;
                        }
                    }
                }
            }
            return null;
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
                
                // Set transfer mode - both upload and download are supported
                _currentTransferMode = _currentJob.TransferMode ?? "Upload";
                ServiceLocator.LogService.LogInfo(string.Format("Loaded job with transfer mode: {0}", _currentTransferMode));
                
                // Set radio button state based on transfer mode
                if (rbUpload != null)
                {
                    rbUpload.Checked = (_currentTransferMode == "Upload");
                }
                
                // Find and set download radio button if it exists
                RadioButton rbDownload = FindDownloadRadioButton();
                if (rbDownload != null)
                {
                    rbDownload.Checked = (_currentTransferMode == "Download");
                }
                
                // Load timer settings (from job data for timer jobs)
                if (chkEnableTimer != null) 
                {
                    // For timer jobs, enable the timer checkbox
                    chkEnableTimer.Checked = _currentJob.TransferMode == "Upload" || _currentJob.TransferMode == "Download" || _currentJob.TransferMode == "Timer";
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
                
                // Load paths based on transfer mode
                if (_currentTransferMode == "Upload")
                {
                    // Upload mode: SourcePath = local, DestinationPath = remote
                    if (!string.IsNullOrEmpty(_currentJob.SourcePath))
                    {
                        _selectedFolderForTimer = _currentJob.SourcePath;
                        if (lblNoFilesSelected != null)
                        {
                            lblNoFilesSelected.Text = "Selected: " + Path.GetFileName(_selectedFolderForTimer);
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(_currentJob.DestinationPath))
                    {
                        _timerUploadDestination = _currentJob.DestinationPath;
                    }
                }
                else if (_currentTransferMode == "Download")
                {
                    // Download mode: SourcePath = remote, DestinationPath = local
                    if (!string.IsNullOrEmpty(_currentJob.SourcePath))
                    {
                        _selectedRemoteFolderForTimer = _currentJob.SourcePath;
                        if (txtRemotePath != null)
                        {
                            txtRemotePath.Text = _selectedRemoteFolderForTimer;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(_currentJob.DestinationPath))
                    {
                        _timerDownloadDestination = _currentJob.DestinationPath;
                        if (lblNoFilesSelected != null)
                        {
                            string remoteDisplay = _selectedRemoteFolderForTimer;
                            string localDisplay = Path.GetFileName(_timerDownloadDestination);
                            lblNoFilesSelected.Text = string.Format("Remote Path: {0} → Local Folder: {1}", remoteDisplay, localDisplay);
                            lblNoFilesSelected.ForeColor = System.Drawing.Color.Green;
                        }
                    }
                }
                

                
                // Load delete source after transfer setting
                if (chkDeleteSourceAfterTransfer != null)
                {
                    chkDeleteSourceAfterTransfer.Checked = _currentJob.DeleteSourceAfterTransfer;
                }
                
                // Load filter settings
                LoadFilterSettings();
                
                // Update UI for the loaded transfer mode
                UpdateUIForTransferMode();
            }
        }

        private void LoadFilterSettings()
        {
            if (_currentJob != null && chkEnableFilters != null)
            {
                // Load filter settings from the current job
                chkEnableFilters.Checked = _currentJob.EnableFilters;
                
                if (txtIncludeFileTypes != null)
                    txtIncludeFileTypes.Text = _currentJob.IncludeFileTypes ?? "";
                
                if (txtExcludeFileTypes != null)
                    txtExcludeFileTypes.Text = _currentJob.ExcludeFileTypes ?? "";
                
                // Update control visibility
                UpdateFilterControlsVisibility();
            }
        }

        private void SaveFilterSettings()
        {
            if (_currentJob != null)
            {
                // Save filter settings to the current job
                _currentJob.EnableFilters = chkEnableFilters?.Checked ?? false;
                _currentJob.IncludeFileTypes = txtIncludeFileTypes?.Text?.Trim() ?? "";
                _currentJob.ExcludeFileTypes = txtExcludeFileTypes?.Text?.Trim() ?? "";
                
                // Add debug logging
                ServiceLocator.LogService.LogInfo($"FILTER DEBUG: SaveFilterSettings - EnableFilters: {_currentJob.EnableFilters}, Include: '{_currentJob.IncludeFileTypes}', Exclude: '{_currentJob.ExcludeFileTypes}'");
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
                    // Update filter settings from UI before saving
                    
                    
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
                // Get remote path from text box
                string remotePath = "/";
                if (txtRemotePath != null && !string.IsNullOrEmpty(txtRemotePath.Text))
                {
                    remotePath = txtRemotePath.Text.Trim();
                }
                
                _currentJob.SourcePath = remotePath;
                
                // Validate that download destination is set
                if (string.IsNullOrEmpty(_timerDownloadDestination))
                {
                    MessageBox.Show("Please select a local destination folder for downloads.", "Download Destination Required", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // Exit early if validation fails
                }
                
                _currentJob.DestinationPath = _timerDownloadDestination;
                _currentJob.TransferMode = "Download"; // Timer-based downloads
                
                // Also update the internal variable for consistency
                _selectedRemoteFolderForTimer = remotePath;
            }
            
            _currentJob.StartTime = DateTime.Now; // Set to current time for timer-based transfers
            
            // For timer functionality, store interval in minutes for consistency
            if (numTimerInterval != null && cmbTimerUnit != null && cmbTimerUnit.SelectedItem != null)
            {
                int intervalValue = (int)numTimerInterval.Value;
                string unit = cmbTimerUnit.SelectedItem.ToString();
                
                // Store the original value and unit without conversion
                _currentJob.IntervalValue = intervalValue;
                _currentJob.IntervalType = unit; // Store the actual selected unit
            }
            else
            {
                _currentJob.IntervalValue = 5; // Default 5 minutes
                _currentJob.IntervalType = "Minutes";
            }
            

            
            // Save delete source after transfer setting
            _currentJob.DeleteSourceAfterTransfer = chkDeleteSourceAfterTransfer.Checked;
            
            // Save filter settings
            SaveFilterSettings();
            
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
                        // Use the stored interval values from _currentJob instead of recalculating from UI
                        // This ensures consistency between what's stored and what's registered
                        double intervalMs;
                        if (_currentJob != null && _currentJob.IntervalValue > 0 && !string.IsNullOrEmpty(_currentJob.IntervalType))
                        {
                            intervalMs = GetIntervalInMilliseconds(_currentJob.IntervalValue, _currentJob.IntervalType);
                        }
                        else
                        {
                            intervalMs = CalculateTimerInterval();
                        }
                        
                        string sourcePath = _currentTransferMode == "Upload" ? _selectedFolderForTimer : _selectedRemoteFolderForTimer;
                        string destPath = _currentTransferMode == "Upload" ? _timerUploadDestination : _timerDownloadDestination;
                        
                        if (_isEditMode && timerJobManager.GetRegisteredTimerJobs().Contains(_currentJob.Id))
                        {
                            // Prepare filter parameters for update
                            List<string> includeExtensions = new List<string>();
                            List<string> excludeExtensions = new List<string>();
                            
                            if (_currentJob.EnableFilters)
                            {
                                // Parse include extensions
                                if (!string.IsNullOrEmpty(_currentJob.IncludeFileTypes))
                                {
                                    string[] includeTypes = _currentJob.IncludeFileTypes.Split(',');
                                    foreach (string type in includeTypes)
                                    {
                                        string cleanType = type.Trim().TrimStart('.');
                                        if (!string.IsNullOrEmpty(cleanType))
                                        {
                                            includeExtensions.Add(cleanType.ToLowerInvariant());
                                        }
                                    }
                                }
                                
                                // Parse exclude extensions
                                if (!string.IsNullOrEmpty(_currentJob.ExcludeFileTypes))
                                {
                                    string[] excludeTypes = _currentJob.ExcludeFileTypes.Split(',');
                                    foreach (string type in excludeTypes)
                                    {
                                        string cleanType = type.Trim().TrimStart('.');
                                        if (!string.IsNullOrEmpty(cleanType))
                                        {
                                            excludeExtensions.Add(cleanType.ToLowerInvariant());
                                        }
                                    }
                                }
                            }
                            
                            ServiceLocator.LogService.LogInfo(string.Format("UPDATING TIMER JOB WITH FILTERS: EnableFilters={0}, Include='{1}', Exclude='{2}'", 
                                _currentJob.EnableFilters, 
                                string.Join(",", includeExtensions.ToArray()), 
                                string.Join(",", excludeExtensions.ToArray())));
                            
                            // Update existing timer job based on transfer mode
                            bool updated;
                            if (_currentTransferMode == "Download")
                            {
                                // For downloads: remote source, local destination
                                updated = timerJobManager.RegisterDownloadTimerJob(_currentJob.Id, _currentJob.Name,
                                    sourcePath, destPath, intervalMs, ShouldIncludeSubfolders(), _currentJob.DeleteSourceAfterTransfer,
                                    _currentJob.EnableFilters, includeExtensions, excludeExtensions);
                            }
                            else
                            {
                                // For uploads: local source, remote destination
                                updated = timerJobManager.UpdateTimerJob(_currentJob.Id, _currentJob.Name,
                                    sourcePath, destPath, intervalMs, ShouldIncludeSubfolders(), _currentJob.DeleteSourceAfterTransfer,
                                    _currentJob.EnableFilters, includeExtensions, excludeExtensions);
                            }
                            
                            if (updated)
                            {
                                ServiceLocator.LogService.LogInfo(string.Format(
                                    "Timer job '{0}' updated successfully with filters", _currentJob.Name));
                            }
                        }
                        else
                        {
                            // Prepare filter parameters
                            List<string> includeExtensions = new List<string>();
                            List<string> excludeExtensions = new List<string>();
                            
                            if (_currentJob.EnableFilters)
                            {
                                // Parse include extensions
                                if (!string.IsNullOrEmpty(_currentJob.IncludeFileTypes))
                                {
                                    string[] includeTypes = _currentJob.IncludeFileTypes.Split(',');
                                    foreach (string type in includeTypes)
                                    {
                                        string cleanType = type.Trim().TrimStart('.');
                                        if (!string.IsNullOrEmpty(cleanType))
                                        {
                                            includeExtensions.Add(cleanType.ToLowerInvariant());
                                        }
                                    }
                                }
                                
                                // Parse exclude extensions
                                if (!string.IsNullOrEmpty(_currentJob.ExcludeFileTypes))
                                {
                                    string[] excludeTypes = _currentJob.ExcludeFileTypes.Split(',');
                                    foreach (string type in excludeTypes)
                                    {
                                        string cleanType = type.Trim().TrimStart('.');
                                        if (!string.IsNullOrEmpty(cleanType))
                                        {
                                            excludeExtensions.Add(cleanType.ToLowerInvariant());
                                        }
                                    }
                                }
                            }
                            
                            ServiceLocator.LogService.LogInfo(string.Format("REGISTERING TIMER JOB WITH FILTERS: EnableFilters={0}, Include='{1}', Exclude='{2}'", 
                                _currentJob.EnableFilters, 
                                string.Join(",", includeExtensions.ToArray()), 
                                string.Join(",", excludeExtensions.ToArray())));
                            
                            // Register new timer job based on transfer mode
                            bool registered;
                            if (_currentTransferMode == "Download")
                            {
                                // For downloads: remote source, local destination
                                registered = timerJobManager.RegisterDownloadTimerJob(_currentJob.Id, _currentJob.Name,
                                    sourcePath, destPath, intervalMs, ShouldIncludeSubfolders(), _currentJob.DeleteSourceAfterTransfer,
                                    _currentJob.EnableFilters, includeExtensions, excludeExtensions);
                            }
                            else
                            {
                                // For uploads: local source, remote destination
                                registered = timerJobManager.RegisterTimerJob(_currentJob.Id, _currentJob.Name,
                                    sourcePath, destPath, intervalMs, ShouldIncludeSubfolders(), _currentJob.DeleteSourceAfterTransfer,
                                    _currentJob.EnableFilters, includeExtensions, excludeExtensions);
                            }
                            
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
            ServiceLocator.LogService.LogInfo(string.Format("Browse button clicked - Current transfer mode: {0}", _currentTransferMode));
            
            // Disable the button during browsing to prevent multiple clicks
            if (btnBrowseFilesForTimer != null)
            {
                btnBrowseFilesForTimer.Enabled = false;
            }
            
            try
            {
                if (_currentTransferMode == "Upload")
                {
                    ServiceLocator.LogService.LogInfo("Routing to local folder browser for upload mode");
                    BrowseLocalFolderForUpload();
                }
                else if (_currentTransferMode == "Download")
                {
                    ServiceLocator.LogService.LogInfo("Routing to remote file browser for download mode");
                    
                    // Verify connection before attempting to browse remote files
                    if (_coreConnectionSettings == null)
                    {
                        MessageBox.Show("No connection settings available. Please configure connection settings first.", 
                            "Connection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    BrowseRemoteFolderForDownloadImproved();
                }
                else
                {
                    ServiceLocator.LogService.LogWarning(string.Format("Unknown transfer mode: {0}", _currentTransferMode));
                    MessageBox.Show(string.Format("Unknown transfer mode: {0}. Please select Upload or Download mode.", _currentTransferMode), 
                        "Invalid Mode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Error during browse operation: {0}", ex.Message));
                MessageBox.Show(string.Format("Error during browse operation: {0}", ex.Message), "Browse Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable the button
                if (btnBrowseFilesForTimer != null)
                {
                    btnBrowseFilesForTimer.Enabled = true;
                }
            }
        }

        private void BrowseLocalFolderForUpload()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select local folder for timed uploads (subfolder inclusion controlled by checkbox)";
                dialog.ShowNewFolderButton = true;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = dialog.SelectedPath;
                    
                    // Get files based on subfolder inclusion checkbox
                    string[] allFiles = Directory.GetFiles(folderPath, "*", GetSearchOption());
                    
                    // Use all files (no filtering)
                    List<string> filteredFiles = new List<string>(allFiles);
                    
                    _selectedFilesForTimer = filteredFiles.ToArray();
                    
                    // Store the base folder path for relative path calculations during upload
                    _selectedFolderForTimer = folderPath;
                    
                    // Update the label to show selected folder and file count
                    if (lblNoFilesSelected != null)
                    {
                        string subfolderInfo = chkIncludeSubfolders.Checked ? " + subfolders" : " (top level only)";
                        
                        if (filteredFiles.Count == 0)
                        {
                            lblNoFilesSelected.Text = Path.GetFileName(folderPath) + " (empty/no matching files - will monitor for new files)" + subfolderInfo;
                        }
                        else if (filteredFiles.Count == 1)
                        {
                            lblNoFilesSelected.Text = Path.GetFileName(folderPath) + " (1 file, including new files added later)" + subfolderInfo;
                        }
                        else
                        {
                            lblNoFilesSelected.Text = Path.GetFileName(folderPath) + " (" + filteredFiles.Count + " files, including new files added later)" + subfolderInfo;
                        }
                    }
                    
                    ServiceLocator.LogService.LogInfo(string.Format("Selected local folder '{0}' with {1} files for timer upload (will also include newly added files)", 
                        folderPath, allFiles.Length));
                    
                    // Ask for upload destination for timer uploads
                    AskForTimerUploadDestination();
                }
            }
        }

        private void BrowseRemoteFolderForDownload()
        {
            ServiceLocator.LogService.LogInfo("BrowseRemoteFolderForDownload method called");
            
            // Debug connection settings
            ServiceLocator.LogService.LogInfo(string.Format("Connection settings null? {0}", _coreConnectionSettings == null));
            if (_coreConnectionSettings != null)
            {
                ServiceLocator.LogService.LogInfo(string.Format("Protocol: {0}", _coreConnectionSettings.Protocol));
            }
            
            // Debug connection service
            ServiceLocator.LogService.LogInfo(string.Format("Connection service null? {0}", _connectionService == null));
            if (_connectionService != null)
            {
                ServiceLocator.LogService.LogInfo(string.Format("Is connected: {0}", _connectionService.IsConnected()));
            }
            
            // Debug transfer client
            ServiceLocator.LogService.LogInfo(string.Format("Transfer client null? {0}", _currentTransferClient == null));
            
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
                string remoteInfo = "";
                if (!string.IsNullOrEmpty(_selectedRemoteFileForTimer))
                {
                    remoteInfo = string.Format(" for remote file: {0}", _selectedRemoteFileForTimer);
                }
                else if (!string.IsNullOrEmpty(_selectedRemoteFolderForTimer))
                {
                    remoteInfo = string.Format(" for remote folder: {0}", System.IO.Path.GetFileName(_selectedRemoteFolderForTimer));
                }
                
                dialog.Description = string.Format("Select local folder to download files{0}", remoteInfo);
                dialog.ShowNewFolderButton = true;
                
                if (!string.IsNullOrEmpty(_timerDownloadDestination))
                {
                    dialog.SelectedPath = _timerDownloadDestination;
                }
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _timerDownloadDestination = dialog.SelectedPath;
                    ServiceLocator.LogService.LogInfo(string.Format("Timer download destination set to: {0}", _timerDownloadDestination));
                    
                    // Update UI to show the complete configuration
                    if (lblNoFilesSelected != null && !string.IsNullOrEmpty(_selectedRemoteFolderForTimer))
                    {
                        string sourceInfo = !string.IsNullOrEmpty(_selectedRemoteFileForTimer) 
                            ? string.Format("Remote File: {0}", _selectedRemoteFileForTimer) 
                            : string.Format("Remote Folder: {0}", System.IO.Path.GetFileName(_selectedRemoteFolderForTimer));
                        
                        lblNoFilesSelected.Text = string.Format("{0} → {1}", sourceInfo, System.IO.Path.GetFileName(_timerDownloadDestination));
                    }
                }
                else
                {
                    ServiceLocator.LogService.LogInfo("Download destination selection cancelled by user");
                }
            }
        }

        private void btnBrowseLocalFolder_Click(object sender, EventArgs e)
        {
            ServiceLocator.LogService.LogInfo("Browse local folder clicked for download mode");
            
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                // Get remote path from text box for better dialog description
                string remotePath = "/";
                if (txtRemotePath != null && !string.IsNullOrEmpty(txtRemotePath.Text))
                {
                    remotePath = txtRemotePath.Text;
                }
                
                dialog.Description = string.Format("Select local folder to download files from remote path: {0}", remotePath);
                dialog.ShowNewFolderButton = true;
                
                if (!string.IsNullOrEmpty(_timerDownloadDestination))
                {
                    dialog.SelectedPath = _timerDownloadDestination;
                }
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _timerDownloadDestination = dialog.SelectedPath;
                    
                    // Also get the remote path from the text box
                    if (txtRemotePath != null && !string.IsNullOrEmpty(txtRemotePath.Text))
                    {
                        _selectedRemoteFolderForTimer = txtRemotePath.Text;
                    }
                    else
                    {
                        _selectedRemoteFolderForTimer = "/";
                    }
                    _selectedRemoteFileForTimer = null; // Clear any specific file selection
                    
                    ServiceLocator.LogService.LogInfo(string.Format("Selected local download destination: {0}", _timerDownloadDestination));
                    ServiceLocator.LogService.LogInfo(string.Format("Remote path from text box: {0}", _selectedRemoteFolderForTimer));
                    
                    // Update UI to show the complete configuration
                    if (lblNoFilesSelected != null)
                    {
                        string remoteDisplay = _selectedRemoteFolderForTimer;
                        string localDisplay = System.IO.Path.GetFileName(_timerDownloadDestination);
                        lblNoFilesSelected.Text = string.Format("Remote Path: {0} → Local Folder: {1}", remoteDisplay, localDisplay);
                        lblNoFilesSelected.ForeColor = System.Drawing.Color.Green;
                    }
                }
                else
                {
                    ServiceLocator.LogService.LogInfo("Local download destination selection cancelled by user");
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
                    MessageBox.Show("Please select a local folder for timer upload using the 'Browse Local Folder' button.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (btnBrowseFilesForTimer != null) btnBrowseFilesForTimer.Focus();
                    return false;
                }
            }
            else if (_currentTransferMode == "Download")
            {
                // For download mode, check if remote path is entered in text box
                string remotePath = "";
                if (txtRemotePath != null && !string.IsNullOrEmpty(txtRemotePath.Text))
                {
                    remotePath = txtRemotePath.Text.Trim();
                }
                
                if (string.IsNullOrEmpty(remotePath) || remotePath == "/")
                {
                    MessageBox.Show("Please enter a remote path in the text box above.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (txtRemotePath != null) txtRemotePath.Focus();
                    return false;
                }
                
                // Update _selectedRemoteFolderForTimer from the text box
                _selectedRemoteFolderForTimer = remotePath;
                _selectedRemoteFileForTimer = null; // Clear file selection for text-based path entry
                
                if (string.IsNullOrEmpty(_timerDownloadDestination))
                {
                    MessageBox.Show("Please select a local destination folder using the 'Browse Local' button.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (btnBrowseLocalFolder != null) btnBrowseLocalFolder.Focus();
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
                // Prevent overlapping uploads
                if (_isUploadInProgress)
                {
                    TimeSpan uploadDuration = DateTime.Now - (_uploadStartTime ?? DateTime.Now);
                    ServiceLocator.LogService.LogWarning(string.Format("Skipping upload timer cycle - previous upload still in progress (running for {0:mm\\:ss})", uploadDuration));
                    return;
                }
                
                ServiceLocator.LogService.LogInfo("Upload timer elapsed - starting automatic upload");
                
                // Mark upload as in progress
                _isUploadInProgress = true;
                _uploadStartTime = DateTime.Now;
                
                // Run upload on UI thread
                this.Invoke(new Action(() =>
                {
                    try
                    {
                        PerformAutomaticUpload();
                        
                        TimeSpan totalDuration = DateTime.Now - _uploadStartTime.Value;
                        ServiceLocator.LogService.LogInfo(string.Format("Upload cycle completed in {0:mm\\:ss}", totalDuration));
                    }
                    catch (Exception ex)
                    {
                        ServiceLocator.LogService.LogError("Automatic upload error: " + ex.Message);
                    }
                    finally
                    {
                        // Always reset the upload in progress flag
                        _isUploadInProgress = false;
                        _uploadStartTime = null;
                    }
                }));
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Upload timer elapsed error: " + ex.Message);
                // Ensure we reset the flag even if there's an error
                _isUploadInProgress = false;
                _uploadStartTime = null;
            }
        }

        private void OnDownloadTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                // Prevent overlapping downloads
                if (_isDownloadInProgress)
                {
                    TimeSpan downloadDuration = DateTime.Now - (_downloadStartTime ?? DateTime.Now);
                    ServiceLocator.LogService.LogWarning(string.Format("Skipping download timer cycle - previous download still in progress (running for {0:mm\\:ss})", downloadDuration));
                    return;
                }
                
                ServiceLocator.LogService.LogInfo("Download timer elapsed - starting automatic download");
                
                // Mark download as in progress
                _isDownloadInProgress = true;
                _downloadStartTime = DateTime.Now;
                
                // Run download on UI thread
                this.Invoke(new Action(() =>
                {
                    try
                    {
                        // Perform automatic download
                        PerformAutomaticDownload();
                        
                        TimeSpan totalDuration = DateTime.Now - _downloadStartTime.Value;
                        ServiceLocator.LogService.LogInfo(string.Format("Download cycle completed in {0:mm\\:ss}", totalDuration));
                    }
                    catch (Exception ex)
                    {
                        ServiceLocator.LogService.LogError("Automatic download error: " + ex.Message);
                    }
                    finally
                    {
                        // Always reset the download in progress flag
                        _isDownloadInProgress = false;
                        _downloadStartTime = null;
                    }
                }));
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Download timer elapsed error: " + ex.Message);
                // Ensure we reset the flag even if there's an error
                _isDownloadInProgress = false;
                _downloadStartTime = null;
            }
        }

        private void PerformAutomaticUpload()
        {
            try
            {
                ServiceLocator.LogService.LogInfo("=== FILTER DEBUG: PerformAutomaticUpload started ===");
                
                // Log current job filter settings
                if (_currentJob != null)
                {
                    ServiceLocator.LogService.LogInfo($"FILTER DEBUG: Current job exists - EnableFilters: {_currentJob.EnableFilters}");
                    ServiceLocator.LogService.LogInfo($"FILTER DEBUG: Current job - Include: '{_currentJob.IncludeFileTypes}', Exclude: '{_currentJob.ExcludeFileTypes}'");
                }
                else
                {
                    ServiceLocator.LogService.LogInfo("FILTER DEBUG: Current job is NULL!");
                }
                
                if (string.IsNullOrEmpty(_selectedFolderForTimer))
                {
                    ServiceLocator.LogService.LogWarning("No folder selected for automatic upload");
                    return;
                }
                
                // Get all files based on subfolder inclusion checkbox setting
                string[] allFiles = Directory.GetFiles(_selectedFolderForTimer, "*", GetSearchOption());
                
                // Apply file filtering if enabled
                List<string> filteredFiles;
                if (_currentJob != null && _currentJob.EnableFilters)
                {
                    ServiceLocator.LogService.LogInfo($"FILTER DEBUG: Starting file filtering - EnableFilters: {_currentJob.EnableFilters}");
                    ServiceLocator.LogService.LogInfo($"FILTER DEBUG: Include types: '{_currentJob.IncludeFileTypes}', Exclude types: '{_currentJob.ExcludeFileTypes}'");
                    ServiceLocator.LogService.LogInfo($"FILTER DEBUG: Total files found: {allFiles.Length}");
                    
                    filteredFiles = new List<string>();
                    ServiceLocator.LogService.LogInfo("Applying file filters for automatic upload");
                    
                    // Parse include and exclude extensions
                    List<string> includeExtensions = new List<string>();
                    List<string> excludeExtensions = new List<string>();
                    
                    ServiceLocator.LogService.LogInfo($"FILTER DEBUG AUTO-UPLOAD: Include types: '{_currentJob.IncludeFileTypes}', Exclude types: '{_currentJob.ExcludeFileTypes}'");
                    
                    if (!string.IsNullOrEmpty(_currentJob.IncludeFileTypes))
                    {
                        foreach (string ext in _currentJob.IncludeFileTypes.Split(','))
                        {
                            string cleanExt = ext.Trim();
                            if (!string.IsNullOrEmpty(cleanExt))
                            {
                                if (!cleanExt.StartsWith("."))
                                    cleanExt = "." + cleanExt;
                                includeExtensions.Add(cleanExt.ToLowerInvariant());
                                ServiceLocator.LogService.LogInfo($"FILTER DEBUG AUTO-UPLOAD: Added include extension: '{cleanExt.ToLowerInvariant()}'");
                            }
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(_currentJob.ExcludeFileTypes))
                    {
                        foreach (string ext in _currentJob.ExcludeFileTypes.Split(','))
                        {
                            string cleanExt = ext.Trim();
                            if (!string.IsNullOrEmpty(cleanExt))
                            {
                                if (!cleanExt.StartsWith("."))
                                    cleanExt = "." + cleanExt;
                                excludeExtensions.Add(cleanExt.ToLowerInvariant());
                                ServiceLocator.LogService.LogInfo($"FILTER DEBUG AUTO-UPLOAD: Added exclude extension: '{cleanExt.ToLowerInvariant()}'");
                            }
                        }
                    }
                    
                    // Filter files
                    foreach (string file in allFiles)
                    {
                        string fileExt = Path.GetExtension(file).ToLowerInvariant();
                        string fileName = Path.GetFileName(file);
                        bool shouldInclude = false;
                        
                        ServiceLocator.LogService.LogInfo($"FILTER DEBUG AUTO-UPLOAD: Checking file '{fileName}' with extension '{fileExt}'");
                        
                        // If no include filters specified, include all by default
                        if (includeExtensions.Count == 0)
                        {
                            shouldInclude = true;
                            ServiceLocator.LogService.LogInfo($"FILTER DEBUG AUTO-UPLOAD: No include filters - including by default");
                        }
                        else
                        {
                            // Check if file matches include filters
                            shouldInclude = includeExtensions.Contains(fileExt);
                            ServiceLocator.LogService.LogInfo($"FILTER DEBUG AUTO-UPLOAD: Include check result: {shouldInclude}");
                        }
                        
                        // Apply exclude filters (exclude takes precedence)
                        if (shouldInclude && excludeExtensions.Count > 0)
                        {
                            if (excludeExtensions.Contains(fileExt))
                            {
                                shouldInclude = false;
                                ServiceLocator.LogService.LogInfo($"FILTER DEBUG AUTO-UPLOAD: File excluded by exclude filter");
                            }
                        }
                        
                        ServiceLocator.LogService.LogInfo($"FILTER DEBUG AUTO-UPLOAD: Final decision for '{fileName}': {(shouldInclude ? "INCLUDE" : "EXCLUDE")}");
                        
                        if (shouldInclude)
                        {
                            filteredFiles.Add(file);
                        }
                    }
                    
                    ServiceLocator.LogService.LogInfo(string.Format("Filter results: {0} files out of {1} total files match the filter criteria", 
                        filteredFiles.Count, allFiles.Length));
                }
                else
                {
                    // No filtering enabled - include all files
                    filteredFiles = new List<string>(allFiles);
                    ServiceLocator.LogService.LogInfo("No file filtering applied - including all files");
                }
                
                string[] currentFiles = filteredFiles.ToArray();
                
                if (currentFiles.Length == 0)
                {
                    ServiceLocator.LogService.LogInfo("No files found (after filtering) in selected folder for automatic upload - will retry on next timer interval");
                    return;
                }
                
                ServiceLocator.LogService.LogInfo(string.Format("Starting automatic upload of folder '{0}' with {1} files (including any newly added)", 
                    _selectedFolderForTimer, currentFiles.Length));
                
                // Upload all filtered files including newly added ones
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
                ServiceLocator.LogService.LogInfo("=== DOWNLOAD DEBUG: PerformAutomaticDownload started ===");
                
                // Log current job filter settings for downloads
                if (_currentJob != null)
                {
                    ServiceLocator.LogService.LogInfo($"DOWNLOAD DEBUG: Current job exists - EnableFilters: {_currentJob.EnableFilters}");
                    ServiceLocator.LogService.LogInfo($"DOWNLOAD DEBUG: Current job - Include: '{_currentJob.IncludeFileTypes}', Exclude: '{_currentJob.ExcludeFileTypes}'");
                }
                else
                {
                    ServiceLocator.LogService.LogInfo("DOWNLOAD DEBUG: Current job is NULL!");
                }
                
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
                
                ServiceLocator.LogService.LogInfo($"Starting automatic download from remote folder '{_selectedRemoteFolderForTimer}' to local destination '{_timerDownloadDestination}'");
                
                // Get all remote files
                List<string> remoteFilesList;
                string listError;
                if (!_currentTransferClient.ListFiles(_coreConnectionSettings, _selectedRemoteFolderForTimer, out remoteFilesList, out listError))
                {
                    ServiceLocator.LogService.LogError($"Failed to list remote files for automatic download: {listError}");
                    return;
                }
                
                ServiceLocator.LogService.LogInfo($"Found {remoteFilesList.Count} remote files");
                
                // Convert to array for compatibility
                string[] remoteFiles = remoteFilesList.ToArray();
                
                // Apply file filtering if enabled
                List<string> filteredFiles = new List<string>();
                if (_currentJob != null && _currentJob.EnableFilters)
                {
                    ServiceLocator.LogService.LogInfo("Applying file filters for automatic download");
                    
                    // Parse include and exclude extensions
                    List<string> includeExtensions = new List<string>();
                    List<string> excludeExtensions = new List<string>();
                    
                    if (!string.IsNullOrEmpty(_currentJob.IncludeFileTypes))
                    {
                        foreach (string ext in _currentJob.IncludeFileTypes.Split(','))
                        {
                            string cleanExt = ext.Trim();
                            if (!string.IsNullOrEmpty(cleanExt))
                            {
                                if (!cleanExt.StartsWith("."))
                                    cleanExt = "." + cleanExt;
                                includeExtensions.Add(cleanExt.ToLowerInvariant());
                            }
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(_currentJob.ExcludeFileTypes))
                    {
                        foreach (string ext in _currentJob.ExcludeFileTypes.Split(','))
                        {
                            string cleanExt = ext.Trim();
                            if (!string.IsNullOrEmpty(cleanExt))
                            {
                                if (!cleanExt.StartsWith("."))
                                    cleanExt = "." + cleanExt;
                                excludeExtensions.Add(cleanExt.ToLowerInvariant());
                            }
                        }
                    }
                    
                    // Filter files
                    foreach (string remoteFile in remoteFiles)
                    {
                        string fileName = Path.GetFileName(remoteFile);
                        string fileExt = Path.GetExtension(fileName).ToLowerInvariant();
                        bool shouldInclude = false;
                        
                        // If no include filters specified, include all by default
                        if (includeExtensions.Count == 0)
                        {
                            shouldInclude = true;
                        }
                        else
                        {
                            // Check if file matches include filters
                            shouldInclude = includeExtensions.Contains(fileExt);
                        }
                        
                        // Apply exclude filters (exclude takes precedence)
                        if (shouldInclude && excludeExtensions.Count > 0)
                        {
                            if (excludeExtensions.Contains(fileExt))
                            {
                                shouldInclude = false;
                            }
                        }
                        
                        ServiceLocator.LogService.LogInfo($"DOWNLOAD FILTER: Final decision for '{fileName}': {(shouldInclude ? "INCLUDE" : "EXCLUDE")}");
                        
                        if (shouldInclude)
                        {
                            filteredFiles.Add(remoteFile);
                        }
                    }
                    
                    ServiceLocator.LogService.LogInfo($"Filter results: {filteredFiles.Count} files out of {remoteFiles.Length} total files match the filter criteria");
                }
                else
                {
                    // No filtering enabled - include all files
                    filteredFiles = new List<string>(remoteFiles);
                    ServiceLocator.LogService.LogInfo("No file filtering applied - including all remote files");
                }
                
                string[] currentFiles = filteredFiles.ToArray();
                
                if (currentFiles.Length == 0)
                {
                    ServiceLocator.LogService.LogInfo("No remote files found (after filtering) for automatic download - will retry on next timer interval");
                    return;
                }
                
                ServiceLocator.LogService.LogInfo($"Starting automatic download of {currentFiles.Length} files from '{_selectedRemoteFolderForTimer}' to '{_timerDownloadDestination}'");
                
                // Download all filtered files using the individual file download method
                PerformDownload(currentFiles, _timerDownloadDestination);
                
                // Update last download time
                _lastDownloadTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Automatic download error: " + ex.Message);
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
                IntervalType = cmbTimerUnit?.SelectedItem?.ToString() ?? "Minutes", // Get actual selected unit from combo box
                TransferMode = "Upload", // This is a timer upload job
                CreatedDate = DateTime.Now,
                LastStatus = "Ready to run",
                ShowTransferProgress = true,
                ValidateTransfer = true,
                MaxRetries = 3,
                RetryDelaySeconds = 5
            };
            

        
            

            return job;
        }

        private void btnLoadConfiguration_Click(object sender, EventArgs e)
        {
            try
            {
                var configService = ServiceLocator.SavedJobConfigurationService;
                
                // Open the Configuration Manager form
                using (var configManager = new Forms.FormSimpleLoadConfiguration(configService))
                {
                    if (configManager.ShowDialog() == DialogResult.OK && configManager.SelectedConfiguration != null)
                    {
                        var config = configManager.SelectedConfiguration;
                        
                        // Apply the loaded configuration to the form
                        ApplyConfigurationToForm(config);
                        
                        // If user chose Load & Start option
                        if (configManager.LoadAndStart)
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
                    // **CRITICAL FIX**: Update _currentJob with loaded configuration values
                    if (_currentJob == null) _currentJob = new SyncJob();
                    _currentJob.Id = config.JobSettings.Id;
                    _currentJob.Name = config.JobSettings.Name;
                    _currentJob.IntervalValue = config.JobSettings.IntervalValue;
                    _currentJob.IntervalType = config.JobSettings.IntervalType;
                    _currentJob.SourcePath = config.JobSettings.SourcePath;
                    _currentJob.DestinationPath = config.JobSettings.DestinationPath;
                    _currentJob.IsEnabled = config.JobSettings.IsEnabled;
                    
                    // Apply job settings to form controls
                    txtJobName.Text = config.JobSettings.Name ?? "";
                    chkEnableJob.Checked = config.JobSettings.IsEnabled;
                    
                    // Set source and destination paths
                    _selectedFolderForTimer = config.JobSettings.SourcePath;
                    _timerUploadDestination = config.JobSettings.DestinationPath;
                    
                    // Set timer interval - now we store original values and units
                    numTimerInterval.Value = Math.Max(1, Math.Min((decimal)numTimerInterval.Maximum, config.JobSettings.IntervalValue));
                    
                    // Set the time unit combo box to match the stored unit
                    if (cmbTimerUnit != null)
                    {
                        string intervalType = config.JobSettings.IntervalType ?? "Minutes";
                        for (int i = 0; i < cmbTimerUnit.Items.Count; i++)
                        {
                            if (cmbTimerUnit.Items[i].ToString().Equals(intervalType, StringComparison.OrdinalIgnoreCase))
                            {
                                cmbTimerUnit.SelectedIndex = i;
                                break;
                            }
                        }
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
                    
                    // **KEY FIX**: Register with TimerJobManager first
                    ITimerJobManager timerJobManager = ServiceLocator.TimerJobManager;
                    if (timerJobManager != null)
                    {
                        // Prepare current job object with correct interval values
                        if (_currentJob == null) _currentJob = new SyncJob();
                        _currentJob.Id = config.JobSettings.Id; // Use the config ID
                        _currentJob.Name = config.JobSettings.Name;
                        _currentJob.IntervalValue = config.JobSettings.IntervalValue;
                        _currentJob.IntervalType = config.JobSettings.IntervalType;
                        _currentJob.SourcePath = config.JobSettings.SourcePath;
                        _currentJob.DestinationPath = config.JobSettings.DestinationPath;
                        _currentJob.IsEnabled = true;
                        
                        // Register with timer job manager using the correct interval
                        List<string> includeExtensions = new List<string>();
                        List<string> excludeExtensions = new List<string>();
                        
                        bool registered = timerJobManager.RegisterTimerJob(
                            _currentJob.Id, 
                            _currentJob.Name,
                            config.JobSettings.SourcePath, 
                            config.JobSettings.DestinationPath, 
                            intervalMs, // Use the calculated intervalMs from config
                            true, // includeSubfolders
                            false, // deleteSourceAfterTransfer
                            false, // enableFilters
                            includeExtensions, 
                            excludeExtensions);
                        
                        if (registered)
                        {
                            timerJobManager.StartTimerJob(_currentJob.Id);
                        }
                    }
                    
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
                            
                            // Delete source file if checkbox is checked
                            if (chkDeleteSourceAfterTransfer.Checked)
                            {
                                try
                                {
                                    File.Delete(localFile);
                                    ServiceLocator.LogService.LogInfo(string.Format("Source file deleted after successful upload: {0}", relativePath));
                                }
                                catch (Exception deleteEx)
                                {
                                    ServiceLocator.LogService.LogWarning(string.Format("Failed to delete source file {0}: {1}", relativePath, deleteEx.Message));
                                    // Don't throw exception here - upload was successful
                                }
                            }
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
                            bool uploadVerified = false;
                            
                            if (_currentTransferClient.FileExists(_coreConnectionSettings, remoteFile, out remoteExists, out verifyError))
                            {
                                if (remoteExists)
                                {
                                    ServiceLocator.LogService.LogInfo(string.Format("Upload verified: {0} exists on remote server", fileName));
                                    uploadVerified = true;
                                }
                                else
                                {
                                    ServiceLocator.LogService.LogWarning(string.Format("Upload completed but file not found on remote: {0}", fileName));
                                }
                            }
                            else
                            {
                                ServiceLocator.LogService.LogWarning(string.Format("Could not verify remote file existence: {0}", verifyError ?? "Unknown error"));
                                // Assume upload was successful if we can't verify (to be safe)
                                uploadVerified = true;
                            }
                            
                            // Delete source file if checkbox is checked and upload was successful
                            if (chkDeleteSourceAfterTransfer.Checked && uploadVerified)
                            {
                                try
                                {
                                    File.Delete(localFile);
                                    ServiceLocator.LogService.LogInfo(string.Format("Source file deleted after successful upload: {0}", fileName));
                                }
                                catch (Exception deleteEx)
                                {
                                    ServiceLocator.LogService.LogWarning(string.Format("Failed to delete source file {0}: {1}", fileName, deleteEx.Message));
                                    // Don't throw exception here - upload was successful
                                }
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

        #endregion

        #region Direct Transfer Methods

        private void btnDirectUpload_Click(object sender, EventArgs e)
        {
            // Same functionality as btnUploadFile_Click
            btnUploadFile_Click(sender, e);
        }

        private void btnDownloadFile_Click(object sender, EventArgs e)
        {
            if (!ValidateConnection()) return;

            // Ask user if they want to download files or a folder
            DialogResult choice = MessageBox.Show(
                "Do you want to download:\n\n" +
                "YES = Individual files from remote server\n" +
                "NO = Entire remote folder\n" +
                "CANCEL = Cancel operation",
                "Download Type Selection",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (choice == DialogResult.Cancel)
                return;

            if (choice == DialogResult.Yes)
            {
                // Download individual files - ask for remote path first
                DownloadIndividualFiles();
            }
            else if (choice == DialogResult.No)
            {
                // Download entire folder
                DownloadEntireFolder();
            }
        }

        private void DownloadIndividualFiles()
        {
            // Ask user for remote file paths
            using (Form inputForm = new Form())
            {
                inputForm.Text = "Remote File Download";
                inputForm.Size = new Size(500, 250);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;
                
                Label lblInfo = new Label(); 
                lblInfo.Left = 10; 
                lblInfo.Top = 20; 
                lblInfo.Text = "Enter remote file paths (one per line):"; 
                lblInfo.AutoSize = true;
                
                TextBox txtRemoteFiles = new TextBox(); 
                txtRemoteFiles.Left = 10; 
                txtRemoteFiles.Top = 50; 
                txtRemoteFiles.Width = 450;
                txtRemoteFiles.Height = 100;
                txtRemoteFiles.Multiline = true;
                txtRemoteFiles.ScrollBars = ScrollBars.Both;
                txtRemoteFiles.Text = "/path/to/file1.txt\r\n/path/to/file2.txt";
                
                Button buttonOk = new Button(); 
                buttonOk.Text = "Download"; 
                buttonOk.Left = 280; 
                buttonOk.Top = 170; 
                buttonOk.DialogResult = DialogResult.OK;
                
                Button buttonCancel = new Button(); 
                buttonCancel.Text = "Cancel"; 
                buttonCancel.Left = 370; 
                buttonCancel.Top = 170; 
                buttonCancel.DialogResult = DialogResult.Cancel;
                
                inputForm.Controls.Add(lblInfo);
                inputForm.Controls.Add(txtRemoteFiles);
                inputForm.Controls.Add(buttonOk);
                inputForm.Controls.Add(buttonCancel);
                inputForm.AcceptButton = buttonOk;
                inputForm.CancelButton = buttonCancel;
                
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    string[] remoteFiles = txtRemoteFiles.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (remoteFiles.Length > 0)
                    {
                        // Ask for local download destination
                        using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                        {
                            folderDialog.Description = "Select local folder to save downloaded files";
                            folderDialog.ShowNewFolderButton = true;
                            
                            if (folderDialog.ShowDialog() == DialogResult.OK)
                            {
                                PerformDownload(remoteFiles, folderDialog.SelectedPath);
                            }
                        }
                    }
                }
            }
        }

        private void DownloadEntireFolder()
        {
            // Ask user for remote folder path
            using (Form inputForm = new Form())
            {
                inputForm.Text = "Remote Folder Download";
                inputForm.Size = new Size(450, 200);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;
                
                Label lblInfo = new Label(); 
                lblInfo.Left = 10; 
                lblInfo.Top = 20; 
                lblInfo.Text = "Enter remote folder path:"; 
                lblInfo.AutoSize = true;
                
                TextBox txtRemoteFolder = new TextBox(); 
                txtRemoteFolder.Left = 10; 
                txtRemoteFolder.Top = 50; 
                txtRemoteFolder.Width = 400;
                txtRemoteFolder.Text = "/";
                
                CheckBox chkIncludeSubfolders = new CheckBox();
                chkIncludeSubfolders.Left = 10;
                chkIncludeSubfolders.Top = 80;
                chkIncludeSubfolders.Text = "Include subfolders";
                chkIncludeSubfolders.Checked = true;
                chkIncludeSubfolders.AutoSize = true;
                
                Button buttonOk = new Button(); 
                buttonOk.Text = "Download"; 
                buttonOk.Left = 220; 
                buttonOk.Top = 120; 
                buttonOk.DialogResult = DialogResult.OK;
                
                Button buttonCancel = new Button(); 
                buttonCancel.Text = "Cancel"; 
                buttonCancel.Left = 320; 
                buttonCancel.Top = 120; 
                buttonCancel.DialogResult = DialogResult.Cancel;
                
                inputForm.Controls.Add(lblInfo);
                inputForm.Controls.Add(txtRemoteFolder);
                inputForm.Controls.Add(chkIncludeSubfolders);
                inputForm.Controls.Add(buttonOk);
                inputForm.Controls.Add(buttonCancel);
                inputForm.AcceptButton = buttonOk;
                inputForm.CancelButton = buttonCancel;
                
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    string remoteFolder = txtRemoteFolder.Text.Trim();
                    bool includeSubfolders = chkIncludeSubfolders.Checked;
                    
                    if (!string.IsNullOrEmpty(remoteFolder))
                    {
                        // Ask for local download destination
                        using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                        {
                            folderDialog.Description = "Select local folder to save downloaded files";
                            folderDialog.ShowNewFolderButton = true;
                            
                            if (folderDialog.ShowDialog() == DialogResult.OK)
                            {
                                PerformFolderDownload(remoteFolder, folderDialog.SelectedPath, includeSubfolders);
                            }
                        }
                    }
                }
            }
        }

        private void PerformDownload(string[] remoteFilePaths, string localDestinationFolder)
        {
            if (remoteFilePaths == null || remoteFilePaths.Length == 0) return;

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            
            worker.DoWork += (sender, e) =>
            {
                try
                {
                    for (int i = 0; i < remoteFilePaths.Length; i++)
                    {
                        string remoteFile = remoteFilePaths[i].Trim();
                        if (string.IsNullOrEmpty(remoteFile)) continue;
                        
                        // Extract filename from remote path
                        string fileName = GetFileNameFromPath(remoteFile);
                        string localFile = Path.Combine(localDestinationFolder, fileName);
                        
                        worker.ReportProgress((i * 100) / remoteFilePaths.Length, 
                            string.Format("Downloading {0}...", fileName));
                        
                        ServiceLocator.LogService.LogInfo(string.Format("Downloading: {0} -> {1}", remoteFile, localFile));
                        
                        // Download the file
                        string downloadError;
                        bool success = _currentTransferClient.DownloadFile(_coreConnectionSettings, remoteFile, localFile, true, out downloadError);
                        
                        if (!success)
                        {
                            string errorMsg = string.Format("Failed to download {0}: {1}", fileName, downloadError ?? "Unknown error");
                            ServiceLocator.LogService.LogError(errorMsg);
                            throw new Exception(errorMsg);
                        }
                        else
                        {
                            ServiceLocator.LogService.LogInfo(string.Format("Successfully downloaded: {0}", fileName));
                            
                            // Delete source file if checkbox is checked and download was successful
                            if (chkDeleteSourceAfterTransfer.Checked)
                            {
                                try
                                {
                                    string deleteError;
                                    bool deleted = _currentTransferClient.DeleteFile(_coreConnectionSettings, remoteFile, out deleteError);
                                    if (deleted)
                                    {
                                        ServiceLocator.LogService.LogInfo(string.Format("Source file deleted after successful download: {0}", fileName));
                                    }
                                    else
                                    {
                                        ServiceLocator.LogService.LogWarning(string.Format("Failed to delete source file {0}: {1}", fileName, deleteError ?? "Unknown error"));
                                    }
                                }
                                catch (Exception deleteEx)
                                {
                                    ServiceLocator.LogService.LogWarning(string.Format("Failed to delete source file {0}: {1}", fileName, deleteEx.Message));
                                    // Don't throw exception here - download was successful
                                }
                            }
                        }
                    }
                    
                    worker.ReportProgress(100, "Download completed successfully!");
                    ServiceLocator.LogService.LogInfo("All downloads completed successfully");
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError("Download failed: " + ex.Message);
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
                    MessageBox.Show("Download failed: " + ex.Message, "Download Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ServiceLocator.LogService.LogError("Download failed: " + ex.Message);
                }
                else
                {
                    MessageBox.Show("Files downloaded successfully!", "Download Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ServiceLocator.LogService.LogInfo("Download completed successfully");
                }
            };
            
            worker.RunWorkerAsync();
        }

        private void PerformFolderDownload(string remoteFolder, string localDestinationFolder, bool includeSubfolders)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            
            worker.DoWork += (sender, e) =>
            {
                try
                {
                    worker.ReportProgress(10, "Getting remote file list...");
                    ServiceLocator.LogService.LogInfo(string.Format("Starting folder download: {0} -> {1}", remoteFolder, localDestinationFolder));
                    
                    // Get list of files from remote folder
                    List<string> remoteFiles;
                    string listError;
                    
                    if (_currentTransferClient.ListFiles(_coreConnectionSettings, remoteFolder, out remoteFiles, out listError))
                    {
                        if (remoteFiles.Count == 0)
                        {
                            worker.ReportProgress(100, "No files found in remote folder");
                            return;
                        }
                        
                        ServiceLocator.LogService.LogInfo(string.Format("Found {0} files to download", remoteFiles.Count));
                        
                        int successCount = 0;
                        int failCount = 0;
                        
                        for (int i = 0; i < remoteFiles.Count; i++)
                        {
                            string remoteFile = remoteFiles[i];
                            string fileName = GetFileNameFromPath(remoteFile);
                            
                            // Create local destination path
                            string localFile = Path.Combine(localDestinationFolder, fileName);
                            
                            worker.ReportProgress((i * 90) / remoteFiles.Count + 10, 
                                string.Format("Downloading {0}...", fileName));
                            
                            ServiceLocator.LogService.LogInfo(string.Format("Downloading: {0} -> {1}", remoteFile, localFile));
                            
                            // Download the file
                            string downloadError;
                            bool success = _currentTransferClient.DownloadFile(_coreConnectionSettings, remoteFile, localFile, true, out downloadError);
                            
                            if (success)
                            {
                                successCount++;
                                ServiceLocator.LogService.LogInfo(string.Format("Successfully downloaded: {0}", fileName));
                                
                                // Delete source file if checkbox is checked
                                if (chkDeleteSourceAfterTransfer.Checked)
                                {
                                    try
                                    {
                                        string deleteError;
                                        bool deleted = _currentTransferClient.DeleteFile(_coreConnectionSettings, remoteFile, out deleteError);
                                        if (deleted)
                                        {
                                            ServiceLocator.LogService.LogInfo(string.Format("Source file deleted after download: {0}", fileName));
                                        }
                                        else
                                        {
                                            ServiceLocator.LogService.LogWarning(string.Format("Failed to delete source file {0}: {1}", fileName, deleteError ?? "Unknown error"));
                                        }
                                    }
                                    catch (Exception deleteEx)
                                    {
                                        ServiceLocator.LogService.LogWarning(string.Format("Failed to delete source file {0}: {1}", fileName, deleteEx.Message));
                                    }
                                }
                            }
                            else
                            {
                                failCount++;
                                ServiceLocator.LogService.LogError(string.Format("Failed to download {0}: {1}", fileName, downloadError ?? "Unknown error"));
                            }
                        }
                        
                        worker.ReportProgress(100, string.Format("Download completed: {0} successful, {1} failed", successCount, failCount));
                        ServiceLocator.LogService.LogInfo(string.Format("Folder download completed: {0} successful, {1} failed", successCount, failCount));
                        
                        if (failCount > 0)
                        {
                            throw new Exception(string.Format("Some downloads failed: {0} successful, {1} failed", successCount, failCount));
                        }
                    }
                    else
                    {
                        string errorMsg = string.Format("Failed to list remote files: {0}", listError ?? "Unknown error");
                        ServiceLocator.LogService.LogError(errorMsg);
                        throw new Exception(errorMsg);
                    }
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError("Folder download failed: " + ex.Message);
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
                    MessageBox.Show("Folder download failed: " + ex.Message, "Download Error", 
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

        private string GetFileNameFromPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "unknown_file";
            
            // Handle both forward and backslashes
            string[] pathParts = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            return pathParts.Length > 0 ? pathParts[pathParts.Length - 1] : "unknown_file";
        }

        private void chkDeleteSourceAfterTransfer_CheckedChanged(object sender, EventArgs e)
        {
            // Optional: Add validation or warning for users when they check this option
            if (chkDeleteSourceAfterTransfer.Checked)
            {
                DialogResult result = MessageBox.Show(
                    "Warning: This option will permanently delete source files after successful transfer.\n\n" +
                    "Local to Remote transfers: Source files will be deleted from your local machine.\n" +
                    "Remote to Local transfers: Source files will be deleted from the remote server.\n\n" +
                    "Are you sure you want to enable this feature?",
                    "Delete Source Files Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.No)
                {
                    chkDeleteSourceAfterTransfer.Checked = false;
                }
                else
                {
                    ServiceLocator.LogService.LogInfo("Delete source after transfer option enabled by user");
                }
            }
        }

        #endregion
    }
}
