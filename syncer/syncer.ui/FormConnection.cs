using System;
using System.Drawing;
using System.Windows.Forms;
using syncer.core;
using syncer.core.Transfers;

namespace syncer.ui
{
    public partial class FormConnection : Form
    {
        private IConnectionService _connectionService;
        private ConnectionSettings _currentSettings;

        public FormConnection()
        {
            InitializeComponent();
            InitializeServices();
            InitializeCustomComponents();
        }

        private void InitializeServices()
        {
            _connectionService = ServiceLocator.ConnectionService;
            _currentSettings = _connectionService.GetConnectionSettings();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Connection Settings";
            this.Size = new Size(500, 470);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (_currentSettings != null)
            {
                // Map protocol to correct index based on our enum order: Local=0, Ftp=1, Sftp=2
                if (cmbProtocol != null)
                {
                    switch (_currentSettings.ProtocolType)
                    {
                        case 0: // Local
                            cmbProtocol.SelectedIndex = 0;
                            break;
                        case 1: // Ftp
                            cmbProtocol.SelectedIndex = 1;
                            break;
                        case 2: // Sftp
                            cmbProtocol.SelectedIndex = 2;
                            break;
                        default:
                            cmbProtocol.SelectedIndex = 0; // Default to LOCAL
                            break;
                    }
                }
                if (txtHost != null) txtHost.Text = _currentSettings.Host ?? string.Empty;
                if (txtPort != null) txtPort.Text = _currentSettings.Port.ToString();
                if (txtUsername != null) txtUsername.Text = _currentSettings.Username ?? string.Empty;
                if (txtPassword != null) txtPassword.Text = _currentSettings.Password ?? string.Empty;
                
                // SSH Key Generation tab
                if (txtKeyPath != null) txtKeyPath.Text = _currentSettings.SshKeyPath ?? string.Empty;
                if (numTimeout != null) numTimeout.Value = _currentSettings.Timeout > 0 ? _currentSettings.Timeout : 30;
            }
            else
            {
                if (cmbProtocol != null) cmbProtocol.SelectedIndex = 0; // Default to LOCAL
                if (txtPort != null) txtPort.Text = "21"; // Default FTP port
                if (numTimeout != null) numTimeout.Value = 30; // Default timeout
            }
        }

        private void SaveSettings()
        {
            if (_currentSettings == null) _currentSettings = new ConnectionSettings();
            
            // Map protocol selection to proper enum values
            if (cmbProtocol != null)
            {
                switch (cmbProtocol.SelectedIndex)
                {
                    case 0: // LOCAL
                        _currentSettings.Protocol = "LOCAL";
                        _currentSettings.ProtocolType = 0;
                        break;
                    case 1: // FTP
                        _currentSettings.Protocol = "FTP";
                        _currentSettings.ProtocolType = 1;
                        break;
                    case 2: // SFTP
                        _currentSettings.Protocol = "SFTP";
                        _currentSettings.ProtocolType = 2;
                        break;
                    default:
                        _currentSettings.Protocol = "LOCAL";
                        _currentSettings.ProtocolType = 0;
                        break;
                }
            }
            
            _currentSettings.Host = txtHost != null ? txtHost.Text.Trim() : string.Empty;
            int port;
            if (!int.TryParse(txtPort != null ? txtPort.Text : "0", out port)) port = 0;
            _currentSettings.Port = port;
            _currentSettings.Username = txtUsername != null ? txtUsername.Text.Trim() : string.Empty;
            _currentSettings.Password = txtPassword != null ? txtPassword.Text : string.Empty;
            
            // SSH Key Generation tab
            if (txtKeyPath != null) _currentSettings.SshKeyPath = txtKeyPath.Text.Trim();
            if (numTimeout != null) _currentSettings.Timeout = (int)numTimeout.Value;
            
            _connectionService.SaveConnectionSettings(_currentSettings);
            ServiceLocator.LogService.LogInfo("Connection settings saved");
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                btnTestConnection.Enabled = false;
                btnTestConnection.Text = "Testing...";
                
                try
                {
                    // Create test settings with proper protocol mapping
                    ConnectionSettings testSettings = new ConnectionSettings();
                    
                    if (cmbProtocol != null)
                    {
                        switch (cmbProtocol.SelectedIndex)
                        {
                            case 0: // LOCAL
                                testSettings.Protocol = "LOCAL";
                                testSettings.ProtocolType = 0;
                                break;
                            case 1: // FTP
                                testSettings.Protocol = "FTP";
                                testSettings.ProtocolType = 1;
                                break;
                            case 2: // SFTP
                                testSettings.Protocol = "SFTP";
                                testSettings.ProtocolType = 2;
                                break;
                            default:
                                testSettings.Protocol = "LOCAL";
                                testSettings.ProtocolType = 0;
                                break;
                        }
                    }
                    
                    testSettings.Host = txtHost != null ? txtHost.Text.Trim() : string.Empty;
                    int p; 
                    if (!int.TryParse(txtPort != null ? txtPort.Text : "0", out p)) p = 0; 
                    testSettings.Port = p;
                    testSettings.Username = txtUsername != null ? txtUsername.Text.Trim() : string.Empty;
                    testSettings.Password = txtPassword != null ? txtPassword.Text : string.Empty;

                    // Use the actual transfer engines for REAL credential validation
                    bool success = TestConnectionWithTransferEngines(testSettings);
                    
                    if (success)
                    {
                        string connectionInfo = testSettings.ProtocolType == 0 ? "Local File System" : 
                                              $"{testSettings.Protocol}://{testSettings.Host}:{testSettings.Port}";
                        MessageBox.Show($"Connection test successful!\n\nConnection: {connectionInfo}", 
                                      "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ServiceLocator.LogService.LogInfo("Connection test successful to " + connectionInfo);
                    }
                    else
                    {
                        MessageBox.Show("Connection test failed. Please verify your credentials and settings.", 
                                      "Test Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ServiceLocator.LogService.LogWarning("Connection test failed to " + testSettings.Host + ":" + testSettings.Port);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection test failed: " + ex.Message, "Test Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ServiceLocator.LogService.LogError("Connection test error: " + ex.Message);
                }
                finally
                {
                    btnTestConnection.Enabled = true;
                    btnTestConnection.Text = "Test Connection";
                }
            }
        }
        
        /// <summary>
        /// Test connection using actual transfer engines to ensure proper credential validation
        /// </summary>
        private bool TestConnectionWithTransferEngines(ConnectionSettings uiSettings)
        {
            try
            {
                // Convert UI settings to core settings for proper validation
                var coreSettings = new syncer.core.ConnectionSettings();
                
                switch (uiSettings.ProtocolType)
                {
                    case 0: // LOCAL
                        coreSettings.Protocol = syncer.core.ProtocolType.Local;
                        break;
                    case 1: // FTP
                        coreSettings.Protocol = syncer.core.ProtocolType.Ftp;
                        break;
                    case 2: // SFTP
                        coreSettings.Protocol = syncer.core.ProtocolType.Sftp;
                        break;
                }
                
                coreSettings.Host = uiSettings.Host;
                coreSettings.Port = uiSettings.Port;
                coreSettings.Username = uiSettings.Username;
                coreSettings.Password = uiSettings.Password;
                coreSettings.UsePassiveMode = true; // Default for FTP
                
                // Create the appropriate transfer client
                syncer.core.ITransferClient client;
                switch (coreSettings.Protocol)
                {
                    case syncer.core.ProtocolType.Local:
                        client = new syncer.core.LocalTransferClient();
                        break;
                    case syncer.core.ProtocolType.Ftp:
                        client = new syncer.core.Transfers.EnhancedFtpTransferClient();
                        break;
                    case syncer.core.ProtocolType.Sftp:
                        client = new syncer.core.Transfers.ProductionSftpTransferClient();
                        break;
                    default:
                        client = new syncer.core.LocalTransferClient();
                        break;
                }
                
                // Test the connection with the actual transfer engine
                string error;
                bool connectionSuccess = client.TestConnection(coreSettings, out error);
                
                if (connectionSuccess)
                {
                    // Additional validation: try to list files to ensure credentials work
                    System.Collections.Generic.List<string> files;
                    string testPath = coreSettings.Protocol == syncer.core.ProtocolType.Local ? 
                                    System.IO.Path.GetTempPath() : "/";
                    
                    bool listSuccess = client.ListFiles(coreSettings, testPath, out files, out error);
                    return listSuccess; // Only return success if both connection AND listing work
                }
                
                return false;
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Transfer engine test failed: " + ex.Message);
                return false;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                try
                {
                    SaveSettings();
                    MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            // Check for LOCAL protocol (index 0)
            if (cmbProtocol != null && cmbProtocol.SelectedIndex == 0) // LOCAL
            {
                try
                {
                    string tempPath = System.IO.Path.GetTempPath();
                    if (!System.IO.Directory.Exists(tempPath))
                    {
                        MessageBox.Show("Cannot access local file system.", "Local Access Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Local file system access error: " + ex.Message, "Local Access Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return true;
            }
            
            // Validation for remote protocols (FTP/SFTP)
            if (UIStringExtensions.IsNullOrWhiteSpace(txtHost != null ? txtHost.Text : null))
            {
                MessageBox.Show("Please enter a host address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (txtHost != null) txtHost.Focus();
                return false;
            }
            if (UIStringExtensions.IsNullOrWhiteSpace(txtPort != null ? txtPort.Text : null))
            {
                MessageBox.Show("Please enter a port number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (txtPort != null) txtPort.Focus();
                return false;
            }
            int port; 
            if (!int.TryParse(txtPort.Text, out port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Please enter a valid port number (1-65535).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (txtPort != null) txtPort.Focus();
                return false;
            }
            if (UIStringExtensions.IsNullOrWhiteSpace(txtUsername != null ? txtUsername.Text : null))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (txtUsername != null) txtUsername.Focus();
                return false;
            }
            return true;
        }

        private void cmbProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProtocol != null)
            {
                switch (cmbProtocol.SelectedIndex)
                {
                    case 0: // LOCAL
                        EnableRemoteFields(false);
                        if (txtHost != null) txtHost.Text = "";
                        if (txtPort != null) txtPort.Text = "0";
                        if (txtUsername != null) txtUsername.Text = "";
                        if (txtPassword != null) txtPassword.Text = "";
                        break;
                        
                    case 1: // FTP
                        EnableRemoteFields(true);
                        if (txtPort != null) txtPort.Text = "21";
                        if (txtHost != null && (txtHost.Text == "localhost" || txtHost.Text == "")) txtHost.Text = "";
                        if (txtUsername != null && txtUsername.Text == "local") txtUsername.Text = "";
                        break;
                        
                    case 2: // SFTP
                        EnableRemoteFields(true);
                        if (txtPort != null) txtPort.Text = "22";
                        if (txtHost != null && (txtHost.Text == "localhost" || txtHost.Text == "")) txtHost.Text = "";
                        if (txtUsername != null && txtUsername.Text == "local") txtUsername.Text = "";
                        break;
                }
            }
        }

        private void EnableRemoteFields(bool enabled)
        {
            if (txtHost != null) txtHost.Enabled = enabled;
            if (txtPort != null) txtPort.Enabled = enabled;
            if (txtUsername != null) txtUsername.Enabled = enabled;
            if (txtPassword != null) txtPassword.Enabled = enabled;
            if (chkShowPassword != null) chkShowPassword.Enabled = enabled;
            if (btnTestConnection != null) btnTestConnection.Text = enabled ? "Test Connection" : "Test Local Access";
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (txtPassword != null && chkShowPassword != null)
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            }
        }

        private void btnBrowseKey_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select SSH Private Key File";
                openFileDialog.Filter = "SSH Key Files (*.pem;*.ppk;*.key)|*.pem;*.ppk;*.key|All Files (*.*)|*.*";
                openFileDialog.Multiselect = false;
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (txtKeyPath != null)
                    {
                        txtKeyPath.Text = openFileDialog.FileName;
                    }
                }
            }
        }
    }
}
