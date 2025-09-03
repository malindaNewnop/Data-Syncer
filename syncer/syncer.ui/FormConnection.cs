using System;
using System.Drawing;
using System.Windows.Forms;
using syncer.core;
using syncer.core.Transfers;
using syncer.ui.Forms;

namespace syncer.ui
{
    public partial class FormConnection : Form
    {
        private IConnectionService _connectionService;
        private ConnectionSettings _currentSettings;

        // Override to prevent unwanted resizing
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // Maintain the designed size
            base.SetBoundsCore(x, y, 900, 580, specified);
        }

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
            // Remove manual size setting - let the designer handle the size
            // this.Size = new Size(490, 510); // This was causing layout issues
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
                
                // SSH Key Authentication controls
                if (txtSSHKeyPath != null) txtSSHKeyPath.Text = _currentSettings.SshKeyPath ?? string.Empty;
                if (chkUseSSHKey != null) chkUseSSHKey.Checked = !string.IsNullOrEmpty(_currentSettings.SshKeyPath);
                
                // Connection name field
                if (txtConnectionName != null) 
                {
                    if (!string.IsNullOrEmpty(_currentSettings.Host))
                    {
                        txtConnectionName.Text = GenerateConnectionName(_currentSettings);
                    }
                    else
                    {
                        txtConnectionName.Text = "";
                    }
                }
                
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
            
            // SSH Key Authentication - use the path from either the main tab or SSH key tab
            string sshKeyPath = string.Empty;
            if (chkUseSSHKey != null && chkUseSSHKey.Checked && txtSSHKeyPath != null)
            {
                sshKeyPath = txtSSHKeyPath.Text.Trim();
            }
            else if (txtKeyPath != null)
            {
                sshKeyPath = txtKeyPath.Text.Trim();
            }
            _currentSettings.SshKeyPath = sshKeyPath;
            
            // SSH Key Generation tab
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
                this.Cursor = Cursors.WaitCursor;
                
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
                    testSettings.SshKeyPath = txtKeyPath != null ? txtKeyPath.Text.Trim() : string.Empty;
                    testSettings.Timeout = numTimeout != null ? (int)numTimeout.Value : 30;

                    // Use the actual transfer engines for REAL credential validation like FileZilla
                    ServiceLocator.LogService.LogInfo(string.Format("Starting connection test for {0}://{1}:{2}", testSettings.Protocol, testSettings.Host, testSettings.Port));
                    
                    bool success = TestConnectionWithTransferEngines(testSettings);
                    
                    if (success)
                    {
                        string connectionInfo;
                        if (testSettings.ProtocolType == 0)
                        {
                            connectionInfo = "Local File System";
                        }
                        else
                        {
                            connectionInfo = string.Format("{0}://{1}@{2}:{3}", testSettings.Protocol, testSettings.Username, testSettings.Host, testSettings.Port);
                        }
                        
                        string successMessage = "✓ Connection successful!\n\n" +
                                              string.Format("Connection: {0}\n", connectionInfo) +
                                              string.Format("Protocol: {0}\n", testSettings.Protocol);
                        
                        if (testSettings.ProtocolType != 0)
                        {
                            successMessage += "Authentication: Verified\n" +
                                            "File listing: Successful";
                        }
                        else
                        {
                            successMessage += "File system access: Verified";
                        }
                        
                        MessageBox.Show(successMessage, "Connection Test Successful", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ServiceLocator.LogService.LogInfo("Connection test completed successfully");
                        
                        // Enable Save Connection button after successful test
                        EnableSaveConnection();
                    }
                    else
                    {
                        string failureMessage = "✗ Connection test failed!\n\n";
                        
                        if (testSettings.ProtocolType == 0)
                        {
                            failureMessage += "Cannot access local file system.\n" +
                                            "Please check file system permissions.";
                        }
                        else
                        {
                            failureMessage += "Please verify the following:\n" +
                                            "• Host address and port number\n" +
                                            "• Username and password\n" +
                                            "• Network connectivity\n" +
                                            "• Server is running and accessible\n" +
                                            "• Firewall settings\n\n" +
                                            string.Format("Connection: {0}://{1}:{2}", testSettings.Protocol, testSettings.Host, testSettings.Port);
                        }
                        
                        MessageBox.Show(failureMessage, "Connection Test Failed", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ServiceLocator.LogService.LogWarning(string.Format("Connection test failed for {0}://{1}:{2}", testSettings.Protocol, testSettings.Host, testSettings.Port));
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = "✗ Connection test error!\n\n" +
                                        string.Format("Error: {0}\n\n", ex.Message) +
                                        "Please check your settings and try again.";
                    
                    MessageBox.Show(errorMessage, "Connection Test Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ServiceLocator.LogService.LogError(string.Format("Connection test exception: {0}", ex.Message));
                }
                finally
                {
                    btnTestConnection.Enabled = true;
                    btnTestConnection.Text = "Test Connection";
                    this.Cursor = Cursors.Default;
                }
            }
        }
        
        /// <summary>
        /// Test connection using actual transfer engines to ensure proper credential validation
        /// This method works like FileZilla's connection mechanism - it actually connects to the server
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
                        return TestLocalConnection(); // Special handling for local
                    case 1: // FTP
                        coreSettings.Protocol = syncer.core.ProtocolType.Ftp;
                        break;
                    case 2: // SFTP
                        coreSettings.Protocol = syncer.core.ProtocolType.Sftp;
                        break;
                    default:
                        return false;
                }
                
                // Set connection parameters
                coreSettings.Host = uiSettings.Host;
                coreSettings.Port = uiSettings.Port;
                coreSettings.Username = uiSettings.Username;
                coreSettings.Password = uiSettings.Password;
                coreSettings.SshKeyPath = uiSettings.SshKeyPath; // Include SSH key path
                coreSettings.UsePassiveMode = true; // Default for FTP
                coreSettings.Timeout = 30; // 30 second timeout
                
                // Set default ports if not specified
                if (coreSettings.Port <= 0)
                {
                    coreSettings.Port = (coreSettings.Protocol == syncer.core.ProtocolType.Ftp) ? 21 : 22;
                }
                
                // Validate required fields for remote connections
                if (string.IsNullOrEmpty(coreSettings.Host))
                {
                    ServiceLocator.LogService.LogError("Host is required for remote connections");
                    return false;
                }
                
                if (string.IsNullOrEmpty(coreSettings.Username))
                {
                    ServiceLocator.LogService.LogError("Username is required for remote connections");
                    return false;
                }
                
                // Create the appropriate transfer client
                syncer.core.ITransferClient client = null;
                try
                {
                    switch (coreSettings.Protocol)
                    {
                        case syncer.core.ProtocolType.Ftp:
                            client = new syncer.core.Transfers.EnhancedFtpTransferClient();
                            ServiceLocator.LogService.LogInfo(string.Format("Testing FTP connection to {0}:{1}", coreSettings.Host, coreSettings.Port));
                            break;
                        case syncer.core.ProtocolType.Sftp:
                            client = new syncer.core.Transfers.ProductionSftpTransferClient();
                            ServiceLocator.LogService.LogInfo(string.Format("Testing SFTP connection to {0}:{1}", coreSettings.Host, coreSettings.Port));
                            break;
                        default:
                            ServiceLocator.LogService.LogError("Unsupported protocol for remote connection");
                            return false;
                    }
                    
                    if (client == null)
                    {
                        ServiceLocator.LogService.LogError("Failed to create transfer client");
                        return false;
                    }
                    
                    // Step 1: Test basic connection
                    string connectionError;
                    bool connectionSuccess = client.TestConnection(coreSettings, out connectionError);
                    
                    if (!connectionSuccess)
                    {
                        ServiceLocator.LogService.LogError(string.Format("Connection test failed: {0}", connectionError));
                        return false;
                    }
                    
                    ServiceLocator.LogService.LogInfo("Basic connection test successful");
                    
                    // Step 2: Test credential validation by attempting to list files
                    // This is crucial - just like FileZilla, we verify credentials by performing an actual operation
                    System.Collections.Generic.List<string> files;
                    string listError;
                    string testPath = "/"; // Start with root directory
                    
                    bool listSuccess = client.ListFiles(coreSettings, testPath, out files, out listError);
                    
                    if (!listSuccess)
                    {
                        // Try home directory if root fails
                        testPath = "~";
                        listSuccess = client.ListFiles(coreSettings, testPath, out files, out listError);
                        
                        if (!listSuccess)
                        {
                            // Try empty path (working directory)
                            testPath = "";
                            listSuccess = client.ListFiles(coreSettings, testPath, out files, out listError);
                        }
                    }
                    
                    if (listSuccess)
                    {
                        ServiceLocator.LogService.LogInfo(string.Format("Credential validation successful - listed {0} items in '{1}'", files.Count, testPath));
                        return true;
                    }
                    else
                    {
                        ServiceLocator.LogService.LogError(string.Format("Credential validation failed: {0}", listError));
                        return false;
                    }
                }
                finally
                {
                    // Clean up - transfer clients don't implement IDisposable, so no disposal needed
                    client = null;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Authentication failed: {0}", ex.Message));
                return false;
            }
            catch (System.Net.WebException ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Network error: {0}", ex.Message));
                return false;
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Connection test error: {0}", ex.Message));
                return false;
            }
        }
        
        /// <summary>
        /// Test local file system access
        /// </summary>
        private bool TestLocalConnection()
        {
            try
            {
                // Test local file system access
                string tempPath = System.IO.Path.GetTempPath();
                if (!System.IO.Directory.Exists(tempPath))
                {
                    ServiceLocator.LogService.LogError("Cannot access local temp directory");
                    return false;
                }
                
                // Try to create a test file to verify write permissions
                string testFile = System.IO.Path.Combine(tempPath, "syncer_test_" + DateTime.Now.Ticks + ".tmp");
                try
                {
                    System.IO.File.WriteAllText(testFile, "test");
                    System.IO.File.Delete(testFile);
                    ServiceLocator.LogService.LogInfo("Local file system access test successful");
                    return true;
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError(string.Format("Local file system write test failed: {0}", ex.Message));
                    return false;
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Local connection test failed: {0}", ex.Message));
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
                        MessageBox.Show("Cannot access local file system.", "Local Access Error", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Local file system access error: {0}", ex.Message), "Local Access Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return true;
            }
            
            // Validation for remote protocols (FTP/SFTP) - like FileZilla's validation
            
            // Host validation
            if (UIStringExtensions.IsNullOrWhiteSpace(txtHost != null ? txtHost.Text : null))
            {
                MessageBox.Show("Please enter a host address.\n\nExample: ftp.example.com or 192.168.1.100", 
                              "Host Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (txtHost != null) txtHost.Focus();
                return false;
            }
            
            string host = txtHost.Text.Trim();
            if (host.Contains(" "))
            {
                MessageBox.Show("Host address cannot contain spaces.", "Invalid Host", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (txtHost != null) txtHost.Focus();
                return false;
            }
            
            // Port validation
            if (UIStringExtensions.IsNullOrWhiteSpace(txtPort != null ? txtPort.Text : null))
            {
                // Set default port based on protocol
                if (cmbProtocol != null)
                {
                    switch (cmbProtocol.SelectedIndex)
                    {
                        case 1: // FTP
                            txtPort.Text = "21";
                            break;
                        case 2: // SFTP
                            txtPort.Text = "22";
                            break;
                        default:
                            MessageBox.Show("Please enter a port number.\n\nCommon ports:\nFTP: 21\nSFTP: 22", 
                                          "Port Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            if (txtPort != null) txtPort.Focus();
                            return false;
                    }
                }
            }
            
            int port; 
            if (!int.TryParse(txtPort.Text, out port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Please enter a valid port number between 1 and 65535.\n\nCommon ports:\nFTP: 21\nSFTP: 22", 
                              "Invalid Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (txtPort != null) txtPort.Focus();
                return false;
            }
            
            // Username validation
            if (UIStringExtensions.IsNullOrWhiteSpace(txtUsername != null ? txtUsername.Text : null))
            {
                MessageBox.Show("Please enter a username.\n\nThis is required to authenticate with the server.", 
                              "Username Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (txtUsername != null) txtUsername.Focus();
                return false;
            }
            
            // Password validation (warn but don't require, especially for SSH key auth)
            bool usingSSHKey = chkUseSSHKey != null && chkUseSSHKey.Checked && 
                               txtSSHKeyPath != null && !string.IsNullOrEmpty(txtSSHKeyPath.Text.Trim());
            
            if (UIStringExtensions.IsNullOrWhiteSpace(txtPassword != null ? txtPassword.Text : null))
            {
                if (usingSSHKey)
                {
                    // SSH key authentication - password is optional
                }
                else
                {
                    DialogResult result = MessageBox.Show(
                        "No password entered. Do you want to continue?\n\n" +
                        "Note: Some servers allow key-based authentication or anonymous access.",
                        "No Password", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                    if (result == DialogResult.No)
                    {
                        if (txtPassword != null) txtPassword.Focus();
                        return false;
                    }
                }
            }
            
            // SSH Key validation for SFTP
            if (cmbProtocol != null && cmbProtocol.SelectedIndex == 2 && usingSSHKey) // SFTP with SSH key
            {
                if (!System.IO.File.Exists(txtSSHKeyPath.Text.Trim()))
                {
                    MessageBox.Show("SSH key file not found. Please select a valid SSH private key file.", 
                                  "SSH Key File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (txtSSHKeyPath != null) txtSSHKeyPath.Focus();
                    return false;
                }
            }
            
            // Protocol-specific validation
            if (cmbProtocol != null)
            {
                switch (cmbProtocol.SelectedIndex)
                {
                    case 1: // FTP
                        // FTP-specific validation
                        if (port != 21 && port < 1024)
                        {
                            DialogResult result = MessageBox.Show(
                                string.Format("Port {0} is unusual for FTP (standard is 21).\n\nDo you want to continue?", port),
                                "Unusual FTP Port", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (result == DialogResult.No) return false;
                        }
                        break;
                        
                    case 2: // SFTP
                        // SFTP-specific validation
                        if (port != 22 && port < 1024)
                        {
                            DialogResult result = MessageBox.Show(
                                string.Format("Port {0} is unusual for SFTP (standard is 22).\n\nDo you want to continue?", port),
                                "Unusual SFTP Port", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (result == DialogResult.No) return false;
                        }
                        break;
                }
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
                        if (txtHost != null && (txtHost.Text == "localhost" || txtHost.Text == "")) 
                        {
                            txtHost.Text = "";
                            txtHost.Focus(); // Focus for user convenience
                        }
                        if (txtUsername != null && txtUsername.Text == "local") txtUsername.Text = "";
                        break;
                        
                    case 2: // SFTP
                        EnableRemoteFields(true);
                        if (txtPort != null) txtPort.Text = "22";
                        if (txtHost != null && (txtHost.Text == "localhost" || txtHost.Text == "")) 
                        {
                            txtHost.Text = "";
                            txtHost.Focus(); // Focus for user convenience
                        }
                        if (txtUsername != null && txtUsername.Text == "local") txtUsername.Text = "";
                        break;
                }
                
                // Update the form title to show current protocol
                if (cmbProtocol.SelectedIndex >= 0)
                {
                    string[] protocols = { "Local", "FTP", "SFTP" };
                    this.Text = string.Format("Connection Settings - {0}", protocols[cmbProtocol.SelectedIndex]);
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
            
            // Connection name is always enabled for naming saved connections
            if (lblConnectionName != null) lblConnectionName.Enabled = true;
            if (txtConnectionName != null) txtConnectionName.Enabled = true;
            
            // Handle SSH key controls visibility based on protocol
            bool isSFTP = cmbProtocol != null && cmbProtocol.SelectedIndex == 2;
            if (chkUseSSHKey != null) 
            {
                chkUseSSHKey.Visible = enabled && isSFTP;
                chkUseSSHKey.Enabled = enabled && isSFTP;
            }
            if (lblSSHKeyPath != null) lblSSHKeyPath.Visible = enabled && isSFTP;
            if (txtSSHKeyPath != null) txtSSHKeyPath.Visible = enabled && isSFTP;
            if (btnBrowseSSHKey != null) btnBrowseSSHKey.Visible = enabled && isSFTP;
            
            // Disable Save Connection button when changing protocols - user needs to test first
            if (btnSaveConnection != null)
            {
                btnSaveConnection.Enabled = false;
                btnSaveConnection.Text = "Save Connection";
                btnSaveConnection.BackColor = System.Drawing.Color.LightGray;
            }
            
            // Update SSH key controls based on checkbox state
            if (enabled && isSFTP)
            {
                chkUseSSHKey_CheckedChanged(null, EventArgs.Empty);
            }
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

        private void chkUseSSHKey_CheckedChanged(object sender, EventArgs e)
        {
            bool useSSHKey = chkUseSSHKey != null && chkUseSSHKey.Checked;
            bool isSFTP = cmbProtocol != null && cmbProtocol.SelectedIndex == 2; // SFTP
            
            // Enable SSH key controls only for SFTP
            if (lblSSHKeyPath != null) lblSSHKeyPath.Enabled = useSSHKey && isSFTP;
            if (txtSSHKeyPath != null) txtSSHKeyPath.Enabled = useSSHKey && isSFTP;
            if (btnBrowseSSHKey != null) btnBrowseSSHKey.Enabled = useSSHKey && isSFTP;
            
            // When using SSH key, password becomes optional
            if (txtPassword != null && lblPassword != null)
            {
                if (useSSHKey && isSFTP)
                {
                    lblPassword.Text = "Password (optional):";
                    txtPassword.BackColor = System.Drawing.Color.LightYellow;
                }
                else
                {
                    lblPassword.Text = "Password:";
                    txtPassword.BackColor = System.Drawing.SystemColors.Window;
                }
            }
        }

        private void btnBrowseSSHKey_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select SSH Private Key File";
                openFileDialog.Filter = "SSH Key Files (*.pem;*.ppk;*.key;*.rsa)|*.pem;*.ppk;*.key;*.rsa|PEM Files (*.pem)|*.pem|PuTTY Key Files (*.ppk)|*.ppk|All Files (*.*)|*.*";
                openFileDialog.Multiselect = false;
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (txtSSHKeyPath != null)
                    {
                        txtSSHKeyPath.Text = openFileDialog.FileName;
                    }
                }
            }
        }

        #region Connection Management

        private void btnSaveConnection_Click(object sender, EventArgs e)
        {
            if (!ValidateConnectionForSave()) return;

            try
            {
                // Create connection settings from form
                ConnectionSettings connectionToSave = CreateConnectionSettingsFromForm();
                
                // Get connection name
                string connectionName = txtConnectionName != null ? txtConnectionName.Text.Trim() : "";
                if (string.IsNullOrEmpty(connectionName))
                {
                    connectionName = GenerateConnectionName(connectionToSave);
                    if (txtConnectionName != null) txtConnectionName.Text = connectionName;
                }

                // Save the connection
                bool saved = SaveConnectionToStorage(connectionName, connectionToSave);
                
                if (saved)
                {
                    string successMessage = string.Format("Connection '{0}' saved successfully!\n\n" +
                                                         "You can now use this connection for upload/download operations.",
                                                         connectionName);
                    MessageBox.Show(successMessage, "Connection Saved", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    ServiceLocator.LogService.LogInfo(string.Format("Connection '{0}' saved successfully", connectionName));
                    
                    // Disable save button after successful save
                    if (btnSaveConnection != null)
                    {
                        btnSaveConnection.Enabled = false;
                        btnSaveConnection.Text = "Saved";
                    }
                }
                else
                {
                    MessageBox.Show("Failed to save connection. Please try again.", "Save Failed", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error saving connection: {0}", ex.Message), "Save Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError(string.Format("Error saving connection: {0}", ex.Message));
            }
        }

        private bool ValidateConnectionForSave()
        {
            // First validate basic connection inputs
            if (!ValidateInputs()) return false;

            // Additional validation for saving
            if (cmbProtocol != null && cmbProtocol.SelectedIndex != 0) // Not LOCAL
            {
                // Check if connection name is reasonable
                string connectionName = txtConnectionName != null ? txtConnectionName.Text.Trim() : "";
                if (string.IsNullOrEmpty(connectionName))
                {
                    DialogResult result = MessageBox.Show(
                        "No connection name specified. Do you want to auto-generate one?", 
                        "Connection Name", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                    if (result == DialogResult.No) return false;
                }
                
                // Warn about password storage
                if (txtPassword != null && !string.IsNullOrEmpty(txtPassword.Text))
                {
                    if (chkUseSSHKey == null || !chkUseSSHKey.Checked)
                    {
                        DialogResult result = MessageBox.Show(
                            "This will save your password in the application.\n\n" +
                            "For better security, consider using SSH key authentication.\n\n" +
                            "Do you want to continue?", 
                            "Password Security Warning", 
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        
                        if (result == DialogResult.No) return false;
                    }
                }
            }

            return true;
        }

        private ConnectionSettings CreateConnectionSettingsFromForm()
        {
            ConnectionSettings settings = new ConnectionSettings();
            
            if (cmbProtocol != null)
            {
                switch (cmbProtocol.SelectedIndex)
                {
                    case 0: // LOCAL
                        settings.Protocol = "LOCAL";
                        settings.ProtocolType = 0;
                        break;
                    case 1: // FTP
                        settings.Protocol = "FTP";
                        settings.ProtocolType = 1;
                        break;
                    case 2: // SFTP
                        settings.Protocol = "SFTP";
                        settings.ProtocolType = 2;
                        break;
                }
            }
            
            settings.Host = txtHost != null ? txtHost.Text.Trim() : "";
            int port;
            if (!int.TryParse(txtPort != null ? txtPort.Text : "0", out port)) port = 0;
            settings.Port = port;
            settings.Username = txtUsername != null ? txtUsername.Text.Trim() : "";
            settings.Password = txtPassword != null ? txtPassword.Text : "";
            settings.SshKeyPath = txtSSHKeyPath != null ? txtSSHKeyPath.Text.Trim() : "";
            settings.Timeout = numTimeout != null ? (int)numTimeout.Value : 30;
            
            return settings;
        }

        private string GenerateConnectionName(ConnectionSettings settings)
        {
            if (settings.ProtocolType == 0)
            {
                return "Local File System";
            }
            else
            {
                return string.Format("{0} - {1}@{2}:{3}", 
                                   settings.Protocol, 
                                   settings.Username, 
                                   settings.Host, 
                                   settings.Port);
            }
        }

        private bool SaveConnectionToStorage(string connectionName, ConnectionSettings settings)
        {
            try
            {
                // Use the existing connection service to save
                // For now, we'll save as the current connection
                // In a full implementation, you'd want a connection manager that stores multiple connections
                
                _connectionService.SaveConnectionSettings(settings);
                
                // Also save to a dedicated connections file/registry for multiple connections
                SaveToConnectionsRegistry(connectionName, settings);
                
                return true;
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Failed to save connection: {0}", ex.Message));
                return false;
            }
        }

        private void SaveToConnectionsRegistry(string connectionName, ConnectionSettings settings)
        {
            try
            {
                // Save connection to Windows Registry or config file
                // This is a simplified implementation - in production you'd want encrypted storage
                
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\FTPSyncer\\Connections"))
                {
                    using (Microsoft.Win32.RegistryKey connectionKey = key.CreateSubKey(connectionName))
                    {
                        connectionKey.SetValue("Protocol", settings.Protocol);
                        connectionKey.SetValue("ProtocolType", settings.ProtocolType);
                        connectionKey.SetValue("Host", settings.Host ?? "");
                        connectionKey.SetValue("Port", settings.Port);
                        connectionKey.SetValue("Username", settings.Username ?? "");
                        
                        // For security, don't save password in plain text in production
                        // This is just for demo purposes
                        connectionKey.SetValue("Password", settings.Password ?? "");
                        connectionKey.SetValue("SshKeyPath", settings.SshKeyPath ?? "");
                        connectionKey.SetValue("Timeout", settings.Timeout);
                        connectionKey.SetValue("SavedDate", DateTime.Now.ToString());
                    }
                }
                
                ServiceLocator.LogService.LogInfo(string.Format("Connection '{0}' saved to registry", connectionName));
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Failed to save connection to registry: {0}", ex.Message));
                // Non-fatal error, don't throw
            }
        }

        /// <summary>
        /// Enable the Save Connection button when a successful test is completed
        /// </summary>
        private void EnableSaveConnection()
        {
            if (btnSaveConnection != null)
            {
                btnSaveConnection.Enabled = true;
                btnSaveConnection.Text = "Save Connection";
                btnSaveConnection.BackColor = System.Drawing.Color.LightGreen;
            }
        }

        #endregion

        #region Connection Management

        private void btnLoadConnection_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedConnection = FormConnectionManager.ShowConnectionSelector(this);
                
                if (!string.IsNullOrEmpty(selectedConnection))
                {
                    var connectionSettings = _connectionService.GetConnection(selectedConnection);
                    if (connectionSettings != null)
                    {
                        _currentSettings = connectionSettings;
                        LoadSettings();
                        
                        MessageBox.Show($"Connection '{selectedConnection}' loaded successfully!", 
                            "Connection Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                        ServiceLocator.LogService?.LogInfo($"Connection '{selectedConnection}' loaded by user");
                    }
                    else
                    {
                        MessageBox.Show($"Failed to load connection '{selectedConnection}'.", 
                            "Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading connection: {ex.Message}", 
                    "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService?.LogError($"Error loading connection: {ex.Message}");
            }
        }

        private void btnManageConnections_Click(object sender, EventArgs e)
        {
            try
            {
                using (var connectionManager = new FormConnectionManager())
                {
                    connectionManager.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening connection manager: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService?.LogError($"Error opening connection manager: {ex.Message}");
            }
        }

        #endregion
    }
}
