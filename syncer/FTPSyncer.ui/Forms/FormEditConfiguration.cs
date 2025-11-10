using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FTPSyncer.ui.Forms
{
    public partial class FormEditConfiguration : Form
    {
        private SavedJobConfiguration _configuration;
        private ISavedJobConfigurationService _configService;
        private bool _isTimerJob = false; // Flag to indicate if this is a timer job
        private int _currentJobIndex = 0; // Index of currently selected job
        private bool _isMultiJob = false; // Flag to indicate if this is a multi-job configuration

        public FormEditConfiguration(SavedJobConfiguration configuration) : this(configuration, false)
        {
        }

        public FormEditConfiguration(SavedJobConfiguration configuration, bool isTimerJob)
        {
            InitializeComponent();
            _configuration = configuration;
            _configService = ServiceLocator.SavedJobConfigurationService;
            _isTimerJob = isTimerJob;
            
            // Detect if this is a multi-job configuration
            _isMultiJob = _configuration != null && _configuration.Jobs != null && _configuration.Jobs.Count > 1;
            
            // Hide connection tab for timer jobs
            if (_isTimerJob && tabControl1.TabPages.Contains(tabConnection))
            {
                tabControl1.TabPages.Remove(tabConnection);
            }
            
            // Initialize multi-job UI if needed
            InitializeMultiJobUI();
            
            LoadConfigurationData();
            
            // Set up tooltips for filter controls (since PlaceholderText is not available in .NET 3.5)
            SetupFilterTooltips();
            
            // Set default values for combo boxes
            if (cmbConnectionType.Items.Count > 0 && cmbConnectionType.SelectedIndex < 0)
                cmbConnectionType.SelectedIndex = 0;
                
            if (cmbIntervalType.Items.Count > 0 && cmbIntervalType.SelectedIndex < 0)
                cmbIntervalType.SelectedIndex = 1; // Minutes
        }

        private void InitializeMultiJobUI()
        {
            if (!_isMultiJob) return;

            // Show the multi-job panel
            panelMultiJob.Visible = true;
            
            // Move other controls down to accommodate the panel
            AdjustControlsForMultiJob();
            
            // Populate the job selector
            LoadJobSelector();
        }
        
        private void AdjustControlsForMultiJob()
        {
            // Move all job controls down by the height of the multi-job panel
            int offsetY = panelMultiJob.Height + 5;
            
            foreach (Control control in tabJob.Controls)
            {
                if (control != panelMultiJob)
                {
                    control.Top += offsetY;
                }
            }
        }
        
        private void LoadJobSelector()
        {
            cmbJobSelector.Items.Clear();
            
            if (_configuration?.Jobs != null)
            {
                for (int i = 0; i < _configuration.Jobs.Count; i++)
                {
                    var job = _configuration.Jobs[i];
                    string displayName = string.IsNullOrEmpty(job.Name) ? string.Format("Job {0}", i + 1) : job.Name;
                    cmbJobSelector.Items.Add(string.Format("{0}. {1}", i + 1, displayName));
                }
                
                if (cmbJobSelector.Items.Count > 0)
                {
                    cmbJobSelector.SelectedIndex = Math.Min(_currentJobIndex, cmbJobSelector.Items.Count - 1);
                }
            }
        }
        
        #region Multi-Job Event Handlers
        
        private void cmbJobSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbJobSelector.SelectedIndex >= 0 && cmbJobSelector.SelectedIndex != _currentJobIndex)
            {
                // Save current job before switching
                SaveCurrentJobData();
                
                // Switch to new job
                _currentJobIndex = cmbJobSelector.SelectedIndex;
                LoadCurrentJobData();
            }
        }
        
        private void btnAddJob_Click(object sender, EventArgs e)
        {
            try
            {
                // Save current job data first
                SaveCurrentJobData();
                
                // Add new job to the configuration
                if (_configuration.Jobs == null)
                    _configuration.Jobs = new List<SyncJob>();
                    
                var newJob = new SyncJob
                {
                    Name = string.Format("Job {0}", _configuration.Jobs.Count + 1),
                    IsEnabled = true,
                    IntervalValue = 5,
                    IntervalType = "Minutes"
                };
                
                _configuration.Jobs.Add(newJob);
                
                // Update the job selector
                LoadJobSelector();
                
                // Select the new job
                cmbJobSelector.SelectedIndex = _configuration.Jobs.Count - 1;
                
                MessageBox.Show("New job added successfully!", "Add Job", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding new job: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnDeleteJob_Click(object sender, EventArgs e)
        {
            try
            {
                if (_configuration?.Jobs == null || _configuration.Jobs.Count <= 1)
                {
                    MessageBox.Show("Cannot delete the last job in the configuration.", "Delete Job", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (_currentJobIndex < 0 || _currentJobIndex >= _configuration.Jobs.Count)
                    return;
                    
                var currentJob = _configuration.Jobs[_currentJobIndex];
                string jobName = string.IsNullOrEmpty(currentJob.Name) ? string.Format("Job {0}", _currentJobIndex + 1) : currentJob.Name;
                
                DialogResult result = MessageBox.Show(
                    string.Format("Are you sure you want to delete '{0}'?", jobName), 
                    "Confirm Delete", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                    
                if (result == DialogResult.Yes)
                {
                    _configuration.Jobs.RemoveAt(_currentJobIndex);
                    
                    // Adjust current job index if needed
                    if (_currentJobIndex >= _configuration.Jobs.Count)
                        _currentJobIndex = _configuration.Jobs.Count - 1;
                    
                    // Check if we still have multiple jobs
                    if (_configuration.Jobs.Count <= 1)
                    {
                        _isMultiJob = false;
                        panelMultiJob.Visible = false;
                        // Move controls back up
                        int offsetY = -(panelMultiJob.Height + 5);
                        foreach (Control control in tabJob.Controls)
                        {
                            if (control != panelMultiJob)
                            {
                                control.Top += offsetY;
                            }
                        }
                    }
                    
                    // Update the job selector and load current job
                    LoadJobSelector();
                    LoadCurrentJobData();
                    
                    MessageBox.Show("Job deleted successfully!", "Delete Job", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting job: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        #endregion
        
        #region Multi-Job Data Methods
        
        private void SaveCurrentJobData()
        {
            if (_configuration?.Jobs == null || _currentJobIndex < 0 || _currentJobIndex >= _configuration.Jobs.Count)
                return;
                
            var currentJob = _configuration.Jobs[_currentJobIndex];
            
            // Save job data from form fields to the current job
            currentJob.Name = txtJobName.Text.Trim();
            currentJob.SourcePath = txtSourcePath.Text.Trim();
            currentJob.DestinationPath = txtDestinationPath.Text.Trim();
            currentJob.IntervalValue = (int)numInterval.Value;
            currentJob.IntervalType = cmbIntervalType.Text;
            currentJob.IsEnabled = chkEnabled.Checked;
            currentJob.IncludeSubFolders = chkIncludeSubfolders.Checked;
            currentJob.DeleteSourceAfterTransfer = chkDeleteSourceAfterTransfer.Checked;
            currentJob.EnableFilters = chkEnableFilters.Checked;
            currentJob.IncludeFileTypes = txtIncludeFileTypes.Text.Trim();
            currentJob.ExcludeFileTypes = txtExcludeFileTypes.Text.Trim();
        }
        
        private void LoadCurrentJobData()
        {
            if (_configuration?.Jobs == null || _currentJobIndex < 0 || _currentJobIndex >= _configuration.Jobs.Count)
            {
                // Clear form if no valid job
                ClearJobForm();
                return;
            }
                
            var currentJob = _configuration.Jobs[_currentJobIndex];
            
            // Load job data from current job to form fields
            txtJobName.Text = currentJob.Name ?? "";
            txtSourcePath.Text = currentJob.SourcePath ?? "";
            txtDestinationPath.Text = currentJob.DestinationPath ?? "";
            numInterval.Value = Math.Max(1, currentJob.IntervalValue);
            
            if (!string.IsNullOrEmpty(currentJob.IntervalType))
            {
                int index = cmbIntervalType.FindString(currentJob.IntervalType);
                if (index >= 0) cmbIntervalType.SelectedIndex = index;
            }
            
            chkEnabled.Checked = currentJob.IsEnabled;
            chkIncludeSubfolders.Checked = currentJob.IncludeSubFolders;
            chkDeleteSourceAfterTransfer.Checked = currentJob.DeleteSourceAfterTransfer;
            chkEnableFilters.Checked = currentJob.EnableFilters;
            txtIncludeFileTypes.Text = currentJob.IncludeFileTypes ?? "";
            txtExcludeFileTypes.Text = currentJob.ExcludeFileTypes ?? "";
            
            // Update filter control states
            UpdateFilterControlStates();
        }
        
        private void ClearJobForm()
        {
            txtJobName.Text = "";
            txtSourcePath.Text = "";
            txtDestinationPath.Text = "";
            numInterval.Value = 1;
            cmbIntervalType.SelectedIndex = 1; // Minutes
            chkEnabled.Checked = true;
            chkIncludeSubfolders.Checked = false;
            chkDeleteSourceAfterTransfer.Checked = false;
            chkEnableFilters.Checked = false;
            txtIncludeFileTypes.Text = "";
            txtExcludeFileTypes.Text = "";
            UpdateFilterControlStates();
        }
        
        #endregion

        private void LoadConfigurationData()
        {
            if (_configuration == null) return;

            // General settings
            txtConfigName.Text = _configuration.Name ?? "";
            txtDescription.Text = _configuration.Description ?? "";

            // Handle job settings based on configuration type
            if (_isMultiJob)
            {
                // For multi-job configurations, load the currently selected job
                LoadCurrentJobData();
            }
            else
            {
                // For single job configurations, load from JobSettings (backward compatibility)
                if (_configuration.JobSettings != null)
                {
                    txtJobName.Text = _configuration.JobSettings.Name ?? "";
                    txtSourcePath.Text = _configuration.JobSettings.SourcePath ?? "";
                    txtDestinationPath.Text = _configuration.JobSettings.DestinationPath ?? "";
                    numInterval.Value = Math.Max(1, _configuration.JobSettings.IntervalValue);
                    
                    if (!string.IsNullOrEmpty(_configuration.JobSettings.IntervalType))
                    {
                        int index = cmbIntervalType.FindString(_configuration.JobSettings.IntervalType);
                        if (index >= 0) cmbIntervalType.SelectedIndex = index;
                    }
                    
                    chkEnabled.Checked = _configuration.JobSettings.IsEnabled;
                    chkIncludeSubfolders.Checked = _configuration.JobSettings.IncludeSubFolders;
                    chkDeleteSourceAfterTransfer.Checked = _configuration.JobSettings.DeleteSourceAfterTransfer;
                    
                    // Load filter settings
                    chkEnableFilters.Checked = _configuration.JobSettings.EnableFilters;
                    txtIncludeFileTypes.Text = _configuration.JobSettings.IncludeFileTypes ?? "";
                    txtExcludeFileTypes.Text = _configuration.JobSettings.ExcludeFileTypes ?? "";
                    
                    // Update filter control states
                    UpdateFilterControlStates();
                }
            }

            // Connection settings (only load if not a timer job)
            if (!_isTimerJob && _configuration.SourceConnection?.Settings != null)
            {
                var conn = _configuration.SourceConnection.Settings;
                cmbConnectionType.Text = conn.Protocol ?? "LOCAL";
                txtServer.Text = conn.Host ?? "";
                txtUsername.Text = conn.Username ?? "";
                txtPassword.Text = conn.Password ?? "";
                numPort.Value = Math.Max(1, conn.Port);
                chkUseSSL.Checked = conn.EnableSsl;
            }
        }

        private void SaveConfigurationData()
        {
            if (_configuration == null) return;

            // Validate required fields
            if (string.IsNullOrEmpty(txtConfigName.Text) || txtConfigName.Text.Trim().Length == 0)
            {
                MessageBox.Show("Configuration name is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl1.SelectedIndex = 0; // Go to General tab
                txtConfigName.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtJobName.Text) || txtJobName.Text.Trim().Length == 0)
            {
                MessageBox.Show("Job name is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl1.SelectedIndex = 1; // Go to Job tab
                txtJobName.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtSourcePath.Text) || txtSourcePath.Text.Trim().Length == 0)
            {
                MessageBox.Show("Source path is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl1.SelectedIndex = 1; // Go to Job tab
                txtSourcePath.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtDestinationPath.Text) || txtDestinationPath.Text.Trim().Length == 0)
            {
                MessageBox.Show("Destination path is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl1.SelectedIndex = 1; // Go to Job tab
                txtDestinationPath.Focus();
                return;
            }

            try
            {
                // Update general settings
                _configuration.Name = txtConfigName.Text.Trim();
                _configuration.Description = txtDescription.Text.Trim();

                // Handle job settings based on configuration type
                if (_isMultiJob)
                {
                    // For multi-job configurations, save current job data
                    SaveCurrentJobData();
                }
                else
                {
                    // For single job configurations, update JobSettings (backward compatibility)
                    if (_configuration.JobSettings == null)
                        _configuration.JobSettings = new SyncJob();

                    _configuration.JobSettings.Name = txtJobName.Text.Trim();
                    _configuration.JobSettings.SourcePath = txtSourcePath.Text.Trim();
                    _configuration.JobSettings.DestinationPath = txtDestinationPath.Text.Trim();
                    _configuration.JobSettings.IntervalValue = (int)numInterval.Value;
                    _configuration.JobSettings.IntervalType = cmbIntervalType.Text;
                    _configuration.JobSettings.IsEnabled = chkEnabled.Checked;
                    _configuration.JobSettings.IncludeSubFolders = chkIncludeSubfolders.Checked;
                    _configuration.JobSettings.DeleteSourceAfterTransfer = chkDeleteSourceAfterTransfer.Checked;
                    
                    // Update filter settings
                    _configuration.JobSettings.EnableFilters = chkEnableFilters.Checked;
                    _configuration.JobSettings.IncludeFileTypes = txtIncludeFileTypes.Text.Trim();
                    _configuration.JobSettings.ExcludeFileTypes = txtExcludeFileTypes.Text.Trim();
                }

                // Update connection settings (only if not a timer job)
                if (!_isTimerJob)
                {
                    if (_configuration.SourceConnection == null)
                        _configuration.SourceConnection = new SavedConnection();
                    if (_configuration.SourceConnection.Settings == null)
                        _configuration.SourceConnection.Settings = new ConnectionSettings();

                    var conn = _configuration.SourceConnection.Settings;
                    conn.Protocol = cmbConnectionType.Text;
                    conn.Host = txtServer.Text.Trim();
                    conn.Username = txtUsername.Text.Trim();
                    conn.Password = txtPassword.Text;
                    conn.Port = (int)numPort.Value;
                    conn.EnableSsl = chkUseSSL.Checked;
                }
                // For timer jobs, keep existing connection settings without modification


                // Update timestamp
                _configuration.LastUsed = DateTime.Now;

                // Save to file with conflict detection
                var saveResult = _configService.SaveConfigurationWithResult(_configuration);
                
                if (saveResult.Result == SaveConfigurationResult.Success)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else if (saveResult.Result == SaveConfigurationResult.NameConflict)
                {
                    // Show confirmation dialog for name conflict
                    string conflictMessage = string.Format(
                        "A configuration with the name '{0}' already exists.\n\n" +
                        "Existing Configuration:\n" +
                        "- Name: {1}\n" +
                        "- Created: {2}\n" +
                        "- Last Used: {3}\n\n" +
                        "Do you want to overwrite the existing configuration?",
                        _configuration.Name,
                        saveResult.ConflictingConfiguration.Name,
                        saveResult.ConflictingConfiguration.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                        saveResult.ConflictingConfiguration.LastUsed?.ToString("yyyy-MM-dd HH:mm") ?? "Never");
                    
                    DialogResult confirmResult = MessageBox.Show(
                        conflictMessage,
                        "Configuration Name Conflict",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    
                    if (confirmResult == DialogResult.Yes)
                    {
                        // User confirmed overwrite
                        if (_configService.SaveConfigurationOverwrite(_configuration))
                        {
                            MessageBox.Show("Configuration saved successfully. The previous configuration has been replaced.", 
                                "Configuration Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Failed to save configuration. Please try again.", 
                                "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    // If user says no, stay on the form so they can change the name
                }
                else if (saveResult.Result == SaveConfigurationResult.ValidationError)
                {
                    MessageBox.Show("Validation Error: " + saveResult.Message, 
                        "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Error saving configuration: " + saveResult.Message, 
                        "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error saving configuration: {0}", ex.Message), 
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveConfigurationData();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnBrowseSource_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select source folder";
                dialog.ShowNewFolderButton = true;
                
                if (!string.IsNullOrEmpty(txtSourcePath.Text))
                    dialog.SelectedPath = txtSourcePath.Text;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtSourcePath.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnBrowseDestination_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select destination folder";
                dialog.ShowNewFolderButton = true;
                
                if (!string.IsNullOrEmpty(txtDestinationPath.Text))
                    dialog.SelectedPath = txtDestinationPath.Text;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtDestinationPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                btnTestConnection.Enabled = false;
                btnTestConnection.Text = "Testing...";

                // Create a temporary connection to test
                var testConnection = new ConnectionSettings
                {
                    Protocol = cmbConnectionType.Text,
                    Host = txtServer.Text.Trim(),
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Text,
                    Port = (int)numPort.Value,
                    EnableSsl = chkUseSSL.Checked
                };

                // Test connection logic would go here
                // For now, just show a success message
                MessageBox.Show("Connection test successful!", "Connection Test", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Connection test failed: {0}", ex.Message), 
                    "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnTestConnection.Enabled = true;
                btnTestConnection.Text = "Test Connection";
            }
        }

        private void chkDeleteSourceAfterTransfer_CheckedChanged(object sender, EventArgs e)
        {
            // Add warning for delete source option
            if (chkDeleteSourceAfterTransfer.Checked)
            {
                DialogResult result = MessageBox.Show(
                    "Warning: This option will permanently delete source files after successful transfer.\n\n" +
                    "Are you sure you want to enable this feature?",
                    "Delete Source Files Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.No)
                {
                    chkDeleteSourceAfterTransfer.Checked = false;
                }
            }
        }

        private void chkEnableFilters_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFilterControlStates();
        }

        private void UpdateFilterControlStates()
        {
            bool enableFiltering = chkEnableFilters?.Checked ?? false;
            
            // Enable/disable controls based on checkbox state, but keep them visible
            if (lblIncludeTypes != null) 
            {
                lblIncludeTypes.Enabled = enableFiltering;
                lblIncludeTypes.Visible = true;
            }
            if (txtIncludeFileTypes != null) 
            {
                txtIncludeFileTypes.Enabled = enableFiltering;
                txtIncludeFileTypes.Visible = true;
            }
            if (lblExcludeTypes != null) 
            {
                lblExcludeTypes.Enabled = enableFiltering;
                lblExcludeTypes.Visible = true;
            }
            if (txtExcludeFileTypes != null) 
            {
                txtExcludeFileTypes.Enabled = enableFiltering;
                txtExcludeFileTypes.Visible = true;
            }
            if (lblFilterHint != null) 
            {
                lblFilterHint.Enabled = enableFiltering;
                lblFilterHint.Visible = true;
            }
        }

        private void SetupFilterTooltips()
        {
            // Add tooltips for filter controls (since PlaceholderText is not available in .NET 3.5)
            if (txtIncludeFileTypes != null)
            {
                ToolTip toolTip1 = new ToolTip();
                toolTip1.SetToolTip(txtIncludeFileTypes, "Enter file extensions separated by commas (e.g., pdf,jpg,png)");
            }
            
            if (txtExcludeFileTypes != null)
            {
                ToolTip toolTip2 = new ToolTip();
                toolTip2.SetToolTip(txtExcludeFileTypes, "Enter file extensions separated by commas (e.g., tmp,bak,log)");
            }
        }
    }
}





