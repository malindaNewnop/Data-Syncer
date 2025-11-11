using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FTPSyncer.core;

namespace FTPSyncer.ui.Forms
{
    /// <summary>
    /// Enhanced connection form with advanced SFTP settings
    /// </summary>
    public partial class FormEnhancedConnection : Form
    {
        private ConnectionSettings _settings;
        private bool _testingConnection = false;

        public ConnectionSettings ConnectionSettings => _settings;

        // .NET 3.5 compatibility helper
        private static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim().Length == 0;
        }

        public FormEnhancedConnection()
        {
            InitializeComponent();
            InitializeForm();
        }

        public FormEnhancedConnection(ConnectionSettings settings) : this()
        {
            _settings = settings ?? new ConnectionSettings();
            PopulateControls();
        }

        private void InitializeForm()
        {
            _settings = new ConnectionSettings();
            
            this.Text = "Enhanced SFTP Connection Settings";
            this.Size = new Size(650, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;

            InitializeControls();
            PopulateControls();
        }

        private void InitializeControls()
        {
            try
            {
                // Initialize combo boxes
                if (cmbProtocol != null)
                {
                    cmbProtocol.Items.Clear();
                    cmbProtocol.Items.Add("FTP");
                    cmbProtocol.Items.Add("SFTP");
                    cmbProtocol.Items.Add("Local");
                    cmbProtocol.SelectedIndex = 1; // Default to SFTP
                }

                if (cmbAuthMethod != null)
                {
                    cmbAuthMethod.Items.Clear();
                    cmbAuthMethod.Items.Add("Password");
                    cmbAuthMethod.Items.Add("Public Key");
                    cmbAuthMethod.Items.Add("Password + Public Key");
                    cmbAuthMethod.SelectedIndex = 0; // Default to Password
                }

                // Set default values for numeric controls
                if (numPort != null) numPort.Value = 22;
                if (numConnectionTimeout != null) numConnectionTimeout.Value = 30000;
                if (numOperationTimeout != null) numOperationTimeout.Value = 60000;
                if (numRetryAttempts != null) numRetryAttempts.Value = 3;
                if (numRetryDelay != null) numRetryDelay.Value = 1000;
                if (numBandwidthLimit != null) numBandwidthLimit.Value = 0; // No limit
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing controls: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateControls()
        {
            try
            {
                if (_settings == null)
                    return;

                // Basic connection settings
                if (txtName != null) txtName.Text = _settings.Name ?? "";
                if (txtHost != null) txtHost.Text = _settings.Host ?? "";
                if (numPort != null) numPort.Value = _settings.Port > 0 ? _settings.Port : 22;
                if (txtUsername != null) txtUsername.Text = _settings.Username ?? "";
                if (txtPassword != null) txtPassword.Text = _settings.Password ?? "";
                if (txtPrivateKeyPath != null) txtPrivateKeyPath.Text = _settings.PrivateKeyPath ?? "";
                if (txtPassphrase != null) txtPassphrase.Text = _settings.Passphrase ?? "";

                // Protocol selection
                if (cmbProtocol != null)
                {
                    switch (_settings.Protocol?.ToUpper())
                    {
                        case "FTP":
                            cmbProtocol.SelectedIndex = 0;
                            break;
                        case "SFTP":
                            cmbProtocol.SelectedIndex = 1;
                            break;
                        case "LOCAL":
                            cmbProtocol.SelectedIndex = 2;
                            break;
                        default:
                            cmbProtocol.SelectedIndex = 1; // Default to SFTP
                            break;
                    }
                }

                // Authentication method
                if (cmbAuthMethod != null)
                {
                    if (!IsNullOrWhiteSpace(_settings.Password) && !IsNullOrWhiteSpace(_settings.PrivateKeyPath))
                        cmbAuthMethod.SelectedIndex = 2; // Password + Public Key
                    else if (!IsNullOrWhiteSpace(_settings.PrivateKeyPath))
                        cmbAuthMethod.SelectedIndex = 1; // Public Key
                    else
                        cmbAuthMethod.SelectedIndex = 0; // Password
                }

                // Set default values for advanced settings
                if (numBandwidthLimit != null) numBandwidthLimit.Value = 0; // No limit
                if (chkEnableResumption != null) chkEnableResumption.Checked = true;
                if (chkVerifyIntegrity != null) chkVerifyIntegrity.Checked = true;
                if (numRetryAttempts != null) numRetryAttempts.Value = 3;
                if (numRetryDelay != null) numRetryDelay.Value = 1000;
                if (chkExponentialBackoff != null) chkExponentialBackoff.Checked = true;
                if (numConnectionTimeout != null) numConnectionTimeout.Value = 30000;
                if (numOperationTimeout != null) numOperationTimeout.Value = 60000;
                if (chkPreserveTimestamps != null) chkPreserveTimestamps.Checked = true;
                if (chkEnableCompression != null) chkEnableCompression.Checked = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error populating controls: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSettings()
        {
            try
            {
                if (_settings == null)
                    _settings = new ConnectionSettings();

                // Basic settings
                _settings.Name = txtName?.Text ?? "";
                _settings.Host = txtHost?.Text ?? "";
                _settings.Port = (int)(numPort?.Value ?? 22);
                _settings.Username = txtUsername?.Text ?? "";
                _settings.Password = txtPassword?.Text ?? "";
                _settings.PrivateKeyPath = txtPrivateKeyPath?.Text ?? "";
                _settings.Passphrase = txtPassphrase?.Text ?? "";

                // Protocol
                if (cmbProtocol != null)
                {
                    switch (cmbProtocol.SelectedIndex)
                    {
                        case 0:
                            _settings.Protocol = "FTP";
                            break;
                        case 1:
                            _settings.Protocol = "SFTP";
                            break;
                        case 2:
                            _settings.Protocol = "Local";
                            break;
                        default:
                            _settings.Protocol = "SFTP";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating settings: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            if (_testingConnection)
                return;

            _testingConnection = true;
            btnTestConnection.Enabled = false;
            btnTestConnection.Text = "Testing...";

            try
            {
                UpdateSettings();

                if (IsNullOrWhiteSpace(_settings.Host))
                {
                    MessageBox.Show("Please enter a host name.", "Validation Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (IsNullOrWhiteSpace(_settings.Username))
                {
                    MessageBox.Show("Please enter a username.", "Validation Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_settings.Protocol == "SFTP")
                {
                    var client = new ProductionSftpTransferClient();
                    bool success = client.TestConnection(_settings);
                    
                    if (success)
                    {
                        MessageBox.Show("Connection successful!", "Test Connection", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Connection failed. Please check your settings.", "Test Connection", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Connection testing is only supported for SFTP connections.", "Test Connection", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection test failed: {ex.Message}", "Test Connection", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _testingConnection = false;
                btnTestConnection.Enabled = true;
                btnTestConnection.Text = "Test Connection";
            }
        }

        private void btnBrowsePrivateKey_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Private Key File";
                dialog.Filter = "Private Key Files (*.pem;*.ppk;*.key)|*.pem;*.ppk;*.key|All Files (*.*)|*.*";
                dialog.CheckFileExists = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (txtPrivateKeyPath != null)
                        txtPrivateKeyPath.Text = dialog.FileName;
                }
            }
        }

        private void btnGenerateKeyPair_Click(object sender, EventArgs e)
        {
            try
            {
                using (var keyGenForm = new FormComprehensiveConnection())
                {
                    keyGenForm.Text = "SSH Key Generation & Connection Settings";
                    keyGenForm.SetDefaultTab(1); // Tab index 1 is SSH Key Generation
                    
                    if (keyGenForm.ShowDialog() == DialogResult.OK)
                    {
                        var settings = keyGenForm.ConnectionSettings;
                        if (!IsNullOrWhiteSpace(settings?.SshKeyPath) && txtPrivateKeyPath != null)
                        {
                            txtPrivateKeyPath.Text = settings.SshKeyPath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening key generation form: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateSettings();

                // Basic validation
                if (IsNullOrWhiteSpace(_settings.Name))
                {
                    MessageBox.Show("Please enter a connection name.", "Validation Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (IsNullOrWhiteSpace(_settings.Host))
                {
                    MessageBox.Show("Please enter a host name.", "Validation Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (IsNullOrWhiteSpace(_settings.Username))
                {
                    MessageBox.Show("Please enter a username.", "Validation Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void cmbAuthMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbAuthMethod == null) return;

                bool showPassword = (cmbAuthMethod.SelectedIndex == 0 || cmbAuthMethod.SelectedIndex == 2);
                bool showPrivateKey = (cmbAuthMethod.SelectedIndex == 1 || cmbAuthMethod.SelectedIndex == 2);

                // Enable/disable controls based on authentication method
                if (txtPassword != null) txtPassword.Enabled = showPassword;
                if (lblPassword != null) lblPassword.Enabled = showPassword;
                
                if (txtPrivateKeyPath != null) txtPrivateKeyPath.Enabled = showPrivateKey;
                if (lblPrivateKey != null) lblPrivateKey.Enabled = showPrivateKey;
                if (btnBrowsePrivateKey != null) btnBrowsePrivateKey.Enabled = showPrivateKey;
                if (txtPassphrase != null) txtPassphrase.Enabled = showPrivateKey;
                if (lblPassphrase != null) lblPassphrase.Enabled = showPrivateKey;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating authentication controls: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbProtocol == null) return;

                bool isSftp = (cmbProtocol.SelectedIndex == 1); // SFTP

                // Enable/disable SFTP-specific controls
                if (grpAdvancedSettings != null) grpAdvancedSettings.Enabled = isSftp;
                if (grpSecuritySettings != null) grpSecuritySettings.Enabled = isSftp;
                if (grpPerformanceSettings != null) grpPerformanceSettings.Enabled = isSftp;

                // Update default port
                if (numPort != null)
                {
                    switch (cmbProtocol.SelectedIndex)
                    {
                        case 0: // FTP
                            numPort.Value = 21;
                            break;
                        case 1: // SFTP
                            numPort.Value = 22;
                            break;
                        case 2: // Local
                            numPort.Value = 0;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating protocol controls: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}





