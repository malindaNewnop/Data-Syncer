using System;
using System.Windows.Forms;
using FTPSyncer.core;

namespace FTPSyncer.ui
{
    /// <summary>
    /// Transfer connection test utility for .NET 3.5 compatibility
    /// Tests FTP, SFTP, and LOCAL transfer engines
    /// Works with the actual transfer clients used by the application
    /// </summary>
    public partial class FtpTestForm : Form
    {
        private GroupBox gbConnection;
        private TextBox txtHost, txtUsername, txtPassword, txtPort;
        private ComboBox cmbProtocol;
        private Button btnTest, btnClose, btnLoadSettings;
        private CheckBox chkPassiveMode;
        private TextBox txtResults;
        private Label lblStatus;
        
        public FtpTestForm()
        {
            InitializeComponent();
            UpdateConnectionStatus();
            // Trigger initial protocol setup
            CmbProtocol_SelectedIndexChanged(cmbProtocol, EventArgs.Empty);
        }

        private void InitializeComponent()
        {
            this.Text = "Transfer Connection Test";
            this.Size = new System.Drawing.Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            // Connection settings group
            gbConnection = new GroupBox();
            gbConnection.Text = "Connection Settings";
            gbConnection.Location = new System.Drawing.Point(12, 12);
            gbConnection.Size = new System.Drawing.Size(460, 200);
            
            // Protocol
            var lblProtocol = new Label();
            lblProtocol.Text = "Protocol:";
            lblProtocol.Location = new System.Drawing.Point(10, 25);
            lblProtocol.Size = new System.Drawing.Size(80, 20);
            
            cmbProtocol = new ComboBox();
            cmbProtocol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProtocol.Items.AddRange(new object[] { "LOCAL", "FTP", "SFTP" });
            cmbProtocol.SelectedIndex = 1; // Default to FTP
            cmbProtocol.Location = new System.Drawing.Point(100, 22);
            cmbProtocol.Size = new System.Drawing.Size(100, 21);
            cmbProtocol.SelectedIndexChanged += CmbProtocol_SelectedIndexChanged;
            
            // Host
            var lblHost = new Label();
            lblHost.Text = "Host:";
            lblHost.Location = new System.Drawing.Point(10, 55);
            lblHost.Size = new System.Drawing.Size(80, 20);
            
            txtHost = new TextBox();
            txtHost.Location = new System.Drawing.Point(100, 52);
            txtHost.Size = new System.Drawing.Size(200, 20);
            txtHost.Text = ""; // Empty by default
            txtHost.TextChanged += (s, ev) => UpdateConnectionStatus();
            
            // Port
            var lblPort = new Label();
            lblPort.Text = "Port:";
            lblPort.Location = new System.Drawing.Point(320, 55);
            lblPort.Size = new System.Drawing.Size(40, 20);
            
            txtPort = new TextBox();
            txtPort.Location = new System.Drawing.Point(370, 52);
            txtPort.Size = new System.Drawing.Size(60, 20);
            txtPort.Text = "21"; // Default FTP port
            txtPort.TextChanged += (s, ev) => UpdateConnectionStatus();
            
            // Username
            var lblUsername = new Label();
            lblUsername.Text = "Username:";
            lblUsername.Location = new System.Drawing.Point(10, 85);
            lblUsername.Size = new System.Drawing.Size(80, 20);
            
            txtUsername = new TextBox();
            txtUsername.Location = new System.Drawing.Point(100, 82);
            txtUsername.Size = new System.Drawing.Size(150, 20);
            txtUsername.Text = ""; // Empty by default
            
            // Password
            var lblPassword = new Label();
            lblPassword.Text = "Password:";
            lblPassword.Location = new System.Drawing.Point(10, 115);
            lblPassword.Size = new System.Drawing.Size(80, 20);
            
            txtPassword = new TextBox();
            txtPassword.Location = new System.Drawing.Point(100, 112);
            txtPassword.Size = new System.Drawing.Size(150, 20);
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Text = ""; // Empty by default
            
            // Passive mode
            chkPassiveMode = new CheckBox();
            chkPassiveMode.Text = "Use Passive Mode (recommended)";
            chkPassiveMode.Location = new System.Drawing.Point(10, 145);
            chkPassiveMode.Size = new System.Drawing.Size(200, 20);
            chkPassiveMode.Checked = true;
            
            // Test button
            btnTest = new Button();
            btnTest.Text = "Test Connection";
            btnTest.Location = new System.Drawing.Point(220, 140);
            btnTest.Size = new System.Drawing.Size(100, 30);
            btnTest.Click += BtnTest_Click;
            
            // Load Settings button
            btnLoadSettings = new Button();
            btnLoadSettings.Text = "Load App Settings";
            btnLoadSettings.Location = new System.Drawing.Point(330, 140);
            btnLoadSettings.Size = new System.Drawing.Size(110, 30);
            btnLoadSettings.Click += BtnLoadSettings_Click;
            
            // Connection status label
            lblStatus = new Label();
            lblStatus.Text = "Status: Not Connected";
            lblStatus.Location = new System.Drawing.Point(12, 225);
            lblStatus.Size = new System.Drawing.Size(460, 20);
            lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            lblStatus.ForeColor = System.Drawing.Color.DarkRed;
            
            // Results area
            var lblResults = new Label();
            lblResults.Text = "Test Results:";
            lblResults.Location = new System.Drawing.Point(12, 250);
            lblResults.Size = new System.Drawing.Size(100, 20);
            
            txtResults = new TextBox();
            txtResults.Location = new System.Drawing.Point(12, 275);
            txtResults.Size = new System.Drawing.Size(460, 225);
            txtResults.Multiline = true;
            txtResults.ScrollBars = ScrollBars.Both;
            txtResults.ReadOnly = true;
            txtResults.Font = new System.Drawing.Font("Consolas", 9);
            
            // Close button
            btnClose = new Button();
            btnClose.Text = "Close";
            btnClose.Location = new System.Drawing.Point(397, 515);
            btnClose.Size = new System.Drawing.Size(75, 30);
            btnClose.Click += (s, e) => this.Close();
            
            // Add controls to group
            gbConnection.Controls.AddRange(new Control[] {
                lblProtocol, cmbProtocol, lblHost, txtHost, lblPort, txtPort,
                lblUsername, txtUsername, lblPassword, txtPassword, chkPassiveMode, btnTest, btnLoadSettings
            });
            
            // Add all to form
            this.Controls.AddRange(new Control[] {
                gbConnection, lblStatus, lblResults, txtResults, btnClose
            });
        }
        
        private void CmbProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isLocal = cmbProtocol.SelectedIndex == 0; // LOCAL
            bool isFtp = cmbProtocol.SelectedIndex == 1;   // FTP
            bool isSftp = cmbProtocol.SelectedIndex == 2;  // SFTP

            // Enable/disable controls based on protocol
            txtHost.Enabled = !isLocal;
            txtPort.Enabled = !isLocal;
            txtUsername.Enabled = !isLocal;
            txtPassword.Enabled = !isLocal;
            chkPassiveMode.Enabled = !isLocal;

            // Set default values based on protocol
            if (isLocal)
            {
                txtHost.Text = "";
                txtPort.Text = "0";
                txtUsername.Text = "";
                txtPassword.Text = "";
                chkPassiveMode.Checked = false;
            }
            else if (isFtp)
            {
                txtPort.Text = "21";
                chkPassiveMode.Checked = true;
            }
            else if (isSftp)
            {
                txtPort.Text = "22";
                chkPassiveMode.Checked = false; // Not applicable for SFTP
                chkPassiveMode.Enabled = false;
            }
            
            UpdateConnectionStatus();
        }
        
        private void BtnLoadSettings_Click(object sender, EventArgs e)
        {
            try
            {
                txtResults.Clear();
                LogMessage("Attempting to load application connection settings...");
                
                // Try to get connection settings from the application's service locator
                try
                {
                    if (ServiceLocator.ConnectionService != null)
                    {
                        var appSettings = ServiceLocator.ConnectionService.GetConnectionSettings();
                        if (appSettings != null)
                        {
                            LoadSettingsToForm(appSettings);
                            LogMessage("✓ Application connection settings loaded successfully.");
                            LogMessage($"Loaded protocol: {appSettings.Protocol}, Host: {appSettings.Host}, Port: {appSettings.Port}");
                            return;
                        }
                    }
                    LogMessage("⚠ Connection service not available or no settings found.");
                }
                catch (Exception serviceEx)
                {
                    LogMessage($"⚠ Error accessing connection service: {serviceEx.Message}");
                }
                
                // Fallback: Try to create a dummy UI ConnectionSettings for compatibility
                try
                {
                    var uiSettings = new FTPSyncer.ui.ConnectionSettings();
                    LoadUISettingsToForm(uiSettings);
                    LogMessage("✓ Default connection settings loaded.");
                }
                catch (Exception uiEx)
                {
                    LogMessage($"✗ Error loading default settings: {uiEx.Message}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"✗ Unexpected error loading settings: {ex.Message}");
            }
        }
        
        private void LoadSettingsToForm(FTPSyncer.core.ConnectionSettings settings)
        {
            // Map from core.ConnectionSettings to form
            switch (settings.Protocol)
            {
                case FTPSyncer.core.ProtocolType.Local:
                    cmbProtocol.SelectedIndex = 0;
                    break;
                case FTPSyncer.core.ProtocolType.Ftp:
                    cmbProtocol.SelectedIndex = 1;
                    break;
                case FTPSyncer.core.ProtocolType.Sftp:
                    cmbProtocol.SelectedIndex = 2;
                    break;
            }
            
            txtHost.Text = settings.Host ?? "";
            txtPort.Text = settings.Port.ToString();
            txtUsername.Text = settings.Username ?? "";
            txtPassword.Text = settings.Password ?? "";
            chkPassiveMode.Checked = settings.UsePassiveMode;
            
            UpdateConnectionStatus();
        }
        
        private void LoadUISettingsToForm(FTPSyncer.ui.ConnectionSettings settings)
        {
            // Map from ui.ConnectionSettings to form
            cmbProtocol.SelectedIndex = settings.ProtocolType; // Should match our indices
            txtHost.Text = settings.Host ?? "";
            txtPort.Text = settings.Port.ToString();
            txtUsername.Text = settings.Username ?? "";
            txtPassword.Text = settings.Password ?? "";
            chkPassiveMode.Checked = settings.UsePassiveMode;
            
            UpdateConnectionStatus();
        }
        
        private void UpdateConnectionStatus()
        {
            try
            {
                string statusText = "Status: ";
                System.Drawing.Color statusColor = System.Drawing.Color.DarkRed;
                
                if (cmbProtocol.SelectedIndex == 0) // LOCAL
                {
                    statusText += "Local File System Connection";
                    statusColor = System.Drawing.Color.DarkGreen;
                }
                else
                {
                    string protocol = cmbProtocol.SelectedIndex == 1 ? "FTP" : "SFTP";
                    string host = txtHost.Text.Trim();
                    string port = txtPort.Text.Trim();
                    
                    if (string.IsNullOrEmpty(host))
                    {
                        statusText += $"{protocol} - No Host Specified";
                        statusColor = System.Drawing.Color.DarkRed;
                    }
                    else
                    {
                        statusText += $"{protocol} Connection to {host}:{port}";
                        statusColor = System.Drawing.Color.DarkBlue;
                    }
                }
                
                if (lblStatus != null)
                {
                    lblStatus.Text = statusText;
                    lblStatus.ForeColor = statusColor;
                }
            }
            catch (Exception ex)
            {
                if (lblStatus != null)
                {
                    lblStatus.Text = $"Status: Error - {ex.Message}";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                }
            }
        }
        
        private void BtnTest_Click(object sender, EventArgs e)
        {
            // Disable buttons during test
            btnTest.Enabled = false;
            btnLoadSettings.Enabled = false;
            btnTest.Text = "Testing...";
            
            // Update status to show testing
            if (lblStatus != null)
            {
                lblStatus.Text = "Status: Testing connection...";
                lblStatus.ForeColor = System.Drawing.Color.Orange;
            }
            
            txtResults.Clear();
            LogMessage("Starting transfer connection test...");
            
            try
            {
                var settings = new FTPSyncer.core.ConnectionSettings();
                
                // Map protocol correctly
                switch (cmbProtocol.SelectedIndex)
                {
                    case 0: // LOCAL
                        settings.Protocol = FTPSyncer.core.ProtocolType.Local;
                        break;
                    case 1: // FTP
                        settings.Protocol = FTPSyncer.core.ProtocolType.Ftp;
                        break;
                    case 2: // SFTP
                        settings.Protocol = FTPSyncer.core.ProtocolType.Sftp;
                        break;
                    default:
                        settings.Protocol = FTPSyncer.core.ProtocolType.Ftp;
                        break;
                }
                
                settings.Host = txtHost.Text.Trim();
                settings.Username = txtUsername.Text.Trim();
                settings.Password = txtPassword.Text;
                settings.UsePassiveMode = chkPassiveMode.Checked;
                
                // Parse port with validation
                if (int.TryParse(txtPort.Text, out int port))
                {
                    settings.Port = port;
                }
                else
                {
                    settings.Port = settings.Protocol == FTPSyncer.core.ProtocolType.Sftp ? 22 : 21;
                }
                
                LogMessage($"Protocol: {settings.Protocol}");
                if (settings.Protocol != FTPSyncer.core.ProtocolType.Local)
                {
                    LogMessage($"Host: {settings.Host}");
                    LogMessage($"Port: {settings.Port}");
                    LogMessage($"Username: {settings.Username}");
                    if (settings.Protocol == FTPSyncer.core.ProtocolType.Ftp)
                    {
                        LogMessage($"Passive Mode: {settings.UsePassiveMode}");
                    }
                }
                LogMessage("");
                
                // Validate required fields for remote connections
                if (settings.Protocol != FTPSyncer.core.ProtocolType.Local)
                {
                    if (string.IsNullOrEmpty(settings.Host))
                    {
                        LogMessage("✗ Host is required for remote connections");
                        return;
                    }
                    if (string.IsNullOrEmpty(settings.Username))
                    {
                        LogMessage("✗ Username is required for remote connections");
                        return;
                    }
                }
                
                // Create appropriate transfer client
                ITransferClient client;
                switch (settings.Protocol)
                {
                    case FTPSyncer.core.ProtocolType.Local:
                        client = new LocalTransferClient();
                        break;
                    case FTPSyncer.core.ProtocolType.Ftp:
                        client = new EnhancedFtpTransferClient();
                        break;
                    case FTPSyncer.core.ProtocolType.Sftp:
                        client = new ProductionSftpTransferClient();
                        break;
                    default:
                        client = new EnhancedFtpTransferClient();
                        break;
                }
                
                LogMessage("Testing connection...");
                string error;
                bool success = client.TestConnection(settings, out error);
                
                if (success)
                {
                    LogMessage("✓ Connection test SUCCESSFUL!");
                    LogMessage("");
                    
                    // Test directory listing based on protocol
                    LogMessage("Testing directory listing...");
                    System.Collections.Generic.List<string> files;
                    string testPath = "/";
                    
                    if (settings.Protocol == FTPSyncer.core.ProtocolType.Local)
                    {
                        testPath = System.IO.Path.GetTempPath(); // Use temp directory for local testing
                    }
                    
                    if (client.ListFiles(settings, testPath, out files, out error))
                    {
                        LogMessage($"✓ Directory listing successful. Found {files.Count} items:");
                        int itemsToShow = Math.Min(10, files.Count); // Show max 10 items
                        for (int i = 0; i < itemsToShow; i++)
                        {
                            LogMessage($"  - {files[i]}");
                        }
                        if (files.Count > 10)
                        {
                            LogMessage($"  ... and {files.Count - 10} more items");
                        }
                    }
                    else
                    {
                        LogMessage($"✗ Directory listing failed: {error}");
                    }
                }
                else
                {
                    LogMessage($"✗ Connection test FAILED: {error}");
                    LogMessage("");
                    LogMessage("Common troubleshooting tips:");
                    
                    if (settings.Protocol == FTPSyncer.core.ProtocolType.Local)
                    {
                        LogMessage("- Ensure the application has file system access");
                        LogMessage("- Check if the temp directory is accessible");
                    }
                    else
                    {
                        LogMessage("- Check if the server is running and accessible");
                        LogMessage("- Verify host/port settings are correct");
                        LogMessage("- Confirm username/password are valid");
                        
                        if (settings.Protocol == FTPSyncer.core.ProtocolType.Ftp)
                        {
                            LogMessage("- Try toggling passive mode");
                            LogMessage("- Check firewall settings for FTP data ports");
                        }
                        else if (settings.Protocol == FTPSyncer.core.ProtocolType.Sftp)
                        {
                            LogMessage("- Ensure SSH service is running");
                            LogMessage("- Verify SSH key authentication if using keys");
                        }
                        
                        LogMessage("- Check network connectivity");
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"✗ Test failed with exception: {ex.Message}");
                LogMessage($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                // Re-enable buttons and restore UI
                btnTest.Enabled = true;
                btnLoadSettings.Enabled = true;
                btnTest.Text = "Test Connection";
                UpdateConnectionStatus();
            }
        }
        
        private void LogMessage(string message)
        {
            txtResults.AppendText(DateTime.Now.ToString("HH:mm:ss") + " - " + message + Environment.NewLine);
            txtResults.SelectionStart = txtResults.Text.Length;
            txtResults.ScrollToCaret();
            Application.DoEvents(); // Allow UI to update
        }
    }
}





