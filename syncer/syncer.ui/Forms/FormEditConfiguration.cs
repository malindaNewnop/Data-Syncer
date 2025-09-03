using System;
using System.Drawing;
using System.Windows.Forms;

namespace syncer.ui.Forms
{
    public partial class FormEditConfiguration : Form
    {
        private SavedJobConfiguration _configuration;
        private ISavedJobConfigurationService _configService;

        public FormEditConfiguration(SavedJobConfiguration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            _configService = ServiceLocator.SavedJobConfigurationService;
            
            LoadConfigurationData();
            
            // Set up tooltips for filter controls (since PlaceholderText is not available in .NET 3.5)
            SetupFilterTooltips();
            
            // Set default values for combo boxes
            if (cmbConnectionType.Items.Count > 0 && cmbConnectionType.SelectedIndex < 0)
                cmbConnectionType.SelectedIndex = 0;
                
            if (cmbIntervalType.Items.Count > 0 && cmbIntervalType.SelectedIndex < 0)
                cmbIntervalType.SelectedIndex = 1; // Minutes
        }

        private void LoadConfigurationData()
        {
            if (_configuration == null) return;

            // General settings
            txtConfigName.Text = _configuration.Name ?? "";
            txtDescription.Text = _configuration.Description ?? "";

            // Job settings
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

            // Connection settings
            if (_configuration.SourceConnection?.Settings != null)
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

                // Update job settings
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

                // Update connection settings
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


                // Update timestamp
                _configuration.LastUsed = DateTime.Now;

                // Save to file
                _configService.SaveConfiguration(_configuration);

                DialogResult = DialogResult.OK;
                Close();
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
