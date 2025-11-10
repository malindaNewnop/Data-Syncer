using System;
using System.Drawing;
using System.Windows.Forms;
using FTPSyncer.ui.Models;
using FTPSyncer.ui.Services;

namespace FTPSyncer.ui.Forms
{
    public partial class FormEditConfiguration : Form
    {
        private SavedJobConfiguration _configuration;
        private ISavedJobConfigurationService _configService;
        
        public SavedJobConfiguration Configuration 
        { 
            get { return _configuration; } 
        }
        
        public bool ConfigurationChanged { get; private set; }

        public FormEditConfiguration(SavedJobConfiguration config)
        {
            InitializeComponent();
            _configuration = config;
            _configService = ServiceLocator.SavedJobConfigurationService;
            LoadConfigurationData();
        }

        private void LoadConfigurationData()
        {
            if (_configuration == null) return;

            // Load basic configuration info
            txtConfigName.Text = _configuration.DisplayName;
            txtDescription.Text = _configuration.Description;
            
            // Load job settings
            if (_configuration.JobSettings != null)
            {
                txtJobName.Text = _configuration.JobSettings.Name;
                txtSourcePath.Text = _configuration.JobSettings.SourcePath;
                txtDestinationPath.Text = _configuration.JobSettings.DestinationPath;
                numInterval.Value = _configuration.JobSettings.IntervalValue;
                cmbIntervalType.Text = _configuration.JobSettings.IntervalType;
                chkEnabled.Checked = _configuration.JobSettings.IsEnabled;
            }
            
            // Load connection settings
            if (_configuration.SourceConnection?.Settings != null)
            {
                var conn = _configuration.SourceConnection.Settings;
                cmbConnectionType.Text = conn.ConnectionType;
                txtServer.Text = conn.Server ?? "";
                txtUsername.Text = conn.Username ?? "";
                txtPassword.Text = conn.Password ?? "";
                numPort.Value = conn.Port;
                chkUseSSL.Checked = conn.UseSSL;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidateInput())
                {
                    SaveConfiguration();
                    ConfigurationChanged = true;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error saving configuration: {0}", ex.Message), 
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(txtConfigName.Text.Trim()))
            {
                MessageBox.Show("Please enter a configuration name.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfigName.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtJobName.Text.Trim()))
            {
                MessageBox.Show("Please enter a job name.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtJobName.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtSourcePath.Text.Trim()))
            {
                MessageBox.Show("Please enter a source path.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSourcePath.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtDestinationPath.Text.Trim()))
            {
                MessageBox.Show("Please enter a destination path.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDestinationPath.Focus();
                return false;
            }

            return true;
        }

        private void SaveConfiguration()
        {
            // Update configuration name and description
            _configuration.DisplayName = txtConfigName.Text.Trim();
            _configuration.Description = txtDescription.Text.Trim();
            
            // Update job settings
            if (_configuration.JobSettings == null)
                _configuration.JobSettings = new JobSettings();
                
            _configuration.JobSettings.Name = txtJobName.Text.Trim();
            _configuration.JobSettings.SourcePath = txtSourcePath.Text.Trim();
            _configuration.JobSettings.DestinationPath = txtDestinationPath.Text.Trim();
            _configuration.JobSettings.IntervalValue = (int)numInterval.Value;
            _configuration.JobSettings.IntervalType = cmbIntervalType.Text;
            _configuration.JobSettings.IsEnabled = chkEnabled.Checked;
            
            // Update connection settings
            if (_configuration.SourceConnection == null)
            {
                _configuration.SourceConnection = new ConnectionInfo();
                _configuration.SourceConnection.Settings = new ConnectionSettings();
            }
            
            var conn = _configuration.SourceConnection.Settings;
            conn.ConnectionType = cmbConnectionType.Text;
            conn.Server = txtServer.Text.Trim();
            conn.Username = txtUsername.Text.Trim();
            conn.Password = txtPassword.Text;
            conn.Port = (int)numPort.Value;
            conn.UseSSL = chkUseSSL.Checked;
            
            // Update modification time
            _configuration.ModifiedDate = DateTime.Now;
            
            // Save to service
            _configService.UpdateConfiguration(_configuration);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnBrowseSource_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select Source Folder";
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
                dialog.Description = "Select Destination Folder";
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
                // Basic validation
                if (cmbConnectionType.Text == "FTP" || cmbConnectionType.Text == "SFTP")
                {
                    if (string.IsNullOrEmpty(txtServer.Text.Trim()))
                    {
                        MessageBox.Show("Please enter server address for FTP/SFTP connection.", 
                            "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                
                MessageBox.Show(string.Format("Connection test for {0} connection would be performed here.\n\nServer: {1}\nPort: {2}\nUsername: {3}", 
                    cmbConnectionType.Text, txtServer.Text, numPort.Value, txtUsername.Text), 
                    "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error testing connection: {0}", ex.Message), 
                    "Test Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}





