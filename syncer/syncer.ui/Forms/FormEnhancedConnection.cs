using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using syncer.core;
using syncer.core.Configuration;

namespace syncer.ui.Forms
{
    /// <summary>
    /// Enhanced connection form with support for SFTP advanced features
    /// </summary>
    public partial class FormEnhancedConnection : Form
    {
        private ConnectionSettings _settings;
        private SftpConfigurationManager _configManager;
        private SftpConfiguration _sftpConfig;
        private bool _isEditingProfile = false;
        private string _currentProfileName = null;

        public ConnectionSettings ConnectionSettings => _settings;

        // .NET 3.5 compatibility helper
        private static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim().Length == 0;
        }

        public FormEnhancedConnection()
        {
            InitializeComponent();
            InitializeServices();
            InitializeForm();
        }

        public FormEnhancedConnection(string profileName) : this()
        {
            LoadProfile(profileName);
        }

        private void InitializeServices()
        {
            _configManager = new SftpConfigurationManager();
            _settings = new ConnectionSettings();
            _sftpConfig = new SftpConfiguration();
        }

        private void InitializeForm()
        {
            this.Text = "Enhanced SFTP Connection Settings";
            this.Size = new Size(650, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            LoadProfiles();
            LoadDefaultSettings();
            UpdateUIForProtocol();
        }

        private void LoadProfiles()
        {
            cmbProfiles.Items.Clear();
            cmbProfiles.Items.Add("(New Profile)");
            
            foreach (var profile in _configManager.GetAllProfiles())
            {
                cmbProfiles.Items.Add(profile.Name);
            }
            
            cmbProfiles.SelectedIndex = 0;
        }

        private void LoadProfile(string profileName)
        {
            var profile = _configManager.GetProfile(profileName);
            if (profile != null)
            {
                _settings = profile.ConnectionSettings;
                _sftpConfig = profile.TransferConfiguration;
                _currentProfileName = profileName;
                _isEditingProfile = true;
                
                LoadSettingsToUI();
                cmbProfiles.Text = profileName;
            }
        }

        private void LoadDefaultSettings()
        {
            // Basic connection settings
            cmbProtocol.SelectedIndex = 1; // SFTP
            txtPort.Text = "22";
            
            // Advanced SFTP settings
            numBandwidthLimit.Value = 0; // Unlimited
            chkEnableResumption.Checked = true;
            chkVerifyIntegrity.Checked = false;
            numRetryAttempts.Value = 3;
            numRetryDelay.Value = 1000;
            chkExponentialBackoff.Checked = true;
            numConnectionTimeout.Value = 30000;
            numOperationTimeout.Value = 60000;
        }

        private void LoadSettingsToUI()
        {
            // Basic settings
            cmbProtocol.SelectedIndex = (int)_settings.Protocol;
            txtHost.Text = _settings.Host;
            txtPort.Text = _settings.Port.ToString();
            txtUsername.Text = _settings.Username;
            txtPassword.Text = _settings.Password;
            txtSshKeyPath.Text = _settings.SshKeyPath;
            numTimeout.Value = _settings.Timeout;

            // Advanced SFTP settings
            numBandwidthLimit.Value = _sftpConfig.BandwidthLimitBytesPerSecond / 1024; // Convert to KB/s
            chkEnableResumption.Checked = _sftpConfig.EnableTransferResumption;
            chkVerifyIntegrity.Checked = _sftpConfig.VerifyTransferIntegrity;
            numRetryAttempts.Value = _sftpConfig.MaxRetryAttempts;
            numRetryDelay.Value = _sftpConfig.RetryDelayMs;
            chkExponentialBackoff.Checked = _sftpConfig.UseExponentialBackoff;
            numConnectionTimeout.Value = _sftpConfig.ConnectionTimeoutMs;
            numOperationTimeout.Value = _sftpConfig.OperationTimeoutMs;
            chkPreserveTimestamps.Checked = _sftpConfig.PreserveTimestamps;
            chkEnableCompression.Checked = _sftpConfig.EnableCompression;
        }

        private void SaveSettingsFromUI()
        {
            // Basic settings
            _settings.Protocol = (ProtocolType)cmbProtocol.SelectedIndex;
            _settings.Host = txtHost.Text.Trim();
            if (int.TryParse(txtPort.Text, out int port))
                _settings.Port = port;
            _settings.Username = txtUsername.Text.Trim();
            _settings.Password = txtPassword.Text;
            _settings.SshKeyPath = txtSshKeyPath.Text.Trim();
            if (int.TryParse(numTimeout.Value.ToString(), out int timeout))
                _settings.Timeout = timeout;

            // Advanced SFTP settings
            _sftpConfig.BandwidthLimitBytesPerSecond = (long)numBandwidthLimit.Value * 1024; // Convert from KB/s
            _sftpConfig.EnableTransferResumption = chkEnableResumption.Checked;
            _sftpConfig.VerifyTransferIntegrity = chkVerifyIntegrity.Checked;
            _sftpConfig.MaxRetryAttempts = (int)numRetryAttempts.Value;
            _sftpConfig.RetryDelayMs = (int)numRetryDelay.Value;
            _sftpConfig.UseExponentialBackoff = chkExponentialBackoff.Checked;
            _sftpConfig.ConnectionTimeoutMs = (int)numConnectionTimeout.Value;
            _sftpConfig.OperationTimeoutMs = (int)numOperationTimeout.Value;
            _sftpConfig.PreserveTimestamps = chkPreserveTimestamps.Checked;
            _sftpConfig.EnableCompression = chkEnableCompression.Checked;
        }

        private void cmbProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUIForProtocol();
        }

        private void UpdateUIForProtocol()
        {
            bool isSftp = cmbProtocol.SelectedIndex == 2; // SFTP
            bool isFtp = cmbProtocol.SelectedIndex == 1;  // FTP
            bool isLocal = cmbProtocol.SelectedIndex == 0; // LOCAL
            
            // Enable/disable protocol-specific controls
            gbSftpAdvanced.Enabled = isSftp;
            gbAuthentication.Enabled = !isLocal;
            
            // SSH key-related controls (SFTP only)
            txtSshKeyPath.Enabled = isSftp;
            btnBrowseKey.Enabled = isSftp;
            btnGenerateKey.Enabled = isSftp;
            btnTestAuthMethods.Enabled = isSftp;
            
            // Set appropriate default ports
            if (isSftp)
            {
                txtPort.Text = "22";
                lblPassword.Text = "Password/Passphrase:";
            }
            else if (isFtp)
            {
                txtPort.Text = "21";
                lblPassword.Text = "Password:";
            }
            
            // For local transfers, no connection credentials needed
            if (isLocal)
            {
                txtHost.Text = "localhost";
                txtUsername.Text = "local";
                txtPassword.Text = "";
                txtPort.Text = "0";
            }
        }

        private void btnBrowseKey_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select SSH Private Key";
                openFileDialog.Filter = "SSH Keys (*.pem;*.ppk;*)|*.pem;*.ppk;*.*|All Files (*.*)|*.*";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtSshKeyPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnGenerateKey_Click(object sender, EventArgs e)
        {
            using (var keyGenForm = new FormKeyGeneration())
            {
                if (keyGenForm.ShowDialog() == DialogResult.OK)
                {
                    txtSshKeyPath.Text = keyGenForm.GeneratedKeyPath;
                }
            }
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            SaveSettingsFromUI();
            
            btnTestConnection.Enabled = false;
            btnTestConnection.Text = "Testing...";
            progressBarTest.Visible = true;
            
            try
            {
                ITransferClient client;
                
                // Use the appropriate client based on protocol
                switch (_settings.Protocol)
                {
                    case ProtocolType.Sftp:
                        client = new EnhancedSftpTransferClient(_sftpConfig);
                        break;
                    case ProtocolType.Ftp:
                        client = new FtpTransferClient();
                        break;
                    case ProtocolType.Local:
                    default:
                        client = new LocalTransferClient();
                        break;
                }
                
                if (client.TestConnection(_settings, out string error))
                {
                    // Get server info for SFTP connections
                    if (_settings.Protocol == ProtocolType.Sftp && SftpUtilities.GetServerInfo(_settings, out var serverInfo))
                    {
                        var result = $"✓ Connection Successful\n\n" +
                                   $"Server: {serverInfo.ServerVersion}\n" +
                                   $"Protocol: {serverInfo.ProtocolVersion}\n" +
                                   $"Working Directory: {serverInfo.WorkingDirectory}\n" +
                                   $"Supports POSIX Rename: {serverInfo.SupportsPosixRename}\n" +
                                   $"Supports Hard Links: {serverInfo.SupportsHardLink}";
                        
                        MessageBox.Show(result, "Connection Test Results", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"✓ Connection Successful\nProtocol: {_settings.Protocol}", "Test Result", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show($"✗ Connection Failed\n\nProtocol: {_settings.Protocol}\nError: {error}", "Test Failed", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"✗ Connection Test Error\n\n{ex.Message}", "Test Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestConnection.Enabled = true;
                btnTestConnection.Text = "Test Connection";
                progressBarTest.Visible = false;
            }
        }

        private void btnTestAuthMethods_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;
                
            // Auth methods test is only relevant for SFTP
            if (_settings.Protocol != ProtocolType.Sftp)
            {
                MessageBox.Show("Authentication methods test is only available for SFTP connections.", 
                    "Not Supported", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveSettingsFromUI();
            
            btnTestAuthMethods.Enabled = false;
            progressBarTest.Visible = true;
            
            try
            {
                if (SftpUtilities.TestConnectionMethods(_settings, out var testResults))
                {
                    var result = "Authentication Method Test Results:\n\n";
                    foreach (var kvp in testResults)
                    {
                        result += $"{kvp.Key}: {kvp.Value}\n";
                    }
                    
                    MessageBox.Show(result, "Authentication Test Results", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("All authentication methods failed.", "Test Failed", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Authentication test error: {ex.Message}", "Test Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestAuthMethods.Enabled = true;
                progressBarTest.Visible = false;
            }
        }

        private void btnSaveProfile_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            string profileName = txtProfileName.Text.Trim();
            if (string.IsNullOrEmpty(profileName))
            {
                MessageBox.Show("Please enter a profile name.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProfileName.Focus();
                return;
            }

            SaveSettingsFromUI();

            var profile = new SftpProfile
            {
                Name = profileName,
                Description = txtProfileDescription.Text.Trim(),
                ConnectionSettings = _settings,
                TransferConfiguration = _sftpConfig
            };

            try
            {
                _configManager.SaveProfile(profile);
                MessageBox.Show("Profile saved successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                LoadProfiles();
                cmbProfiles.Text = profileName;
                _currentProfileName = profileName;
                _isEditingProfile = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving profile: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProfiles.SelectedIndex == 0) // (New Profile)
            {
                _isEditingProfile = false;
                _currentProfileName = null;
                txtProfileName.Text = "";
                txtProfileDescription.Text = "";
                LoadDefaultSettings();
            }
            else
            {
                string profileName = cmbProfiles.SelectedItem.ToString();
                LoadProfile(profileName);
                txtProfileName.Text = profileName;
            }
        }

        private void btnDeleteProfile_Click(object sender, EventArgs e)
        {
            if (!_isEditingProfile || string.IsNullOrEmpty(_currentProfileName))
            {
                MessageBox.Show("No profile selected for deletion.", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete the profile '{_currentProfileName}'?", 
                "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    _configManager.RemoveProfile(_currentProfileName);
                    MessageBox.Show("Profile deleted successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    LoadProfiles();
                    cmbProfiles.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting profile: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool ValidateInputs()
        {
            if (cmbProtocol.SelectedIndex == 0) // LOCAL
                return true;

            if (IsNullOrWhiteSpace(txtHost.Text))
            {
                MessageBox.Show("Please enter a host address.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHost.Focus();
                return false;
            }

            if (!int.TryParse(txtPort.Text, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Please enter a valid port number (1-65535).", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPort.Focus();
                return false;
            }

            if (IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }

            // Protocol-specific validations
            if (cmbProtocol.SelectedIndex == 2) // SFTP
            {
                if (IsNullOrWhiteSpace(txtPassword.Text) && IsNullOrWhiteSpace(txtSshKeyPath.Text))
                {
                    MessageBox.Show("Please enter either a password or select an SSH key file.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (!IsNullOrWhiteSpace(txtSshKeyPath.Text) && !File.Exists(txtSshKeyPath.Text))
                {
                    MessageBox.Show("The specified SSH key file does not exist.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSshKeyPath.Focus();
                    return false;
                }
            }
            else if (cmbProtocol.SelectedIndex == 1) // FTP
            {
                if (IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Please enter a password for FTP authentication.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return false;
                }
            }

            return true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                SaveSettingsFromUI();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
