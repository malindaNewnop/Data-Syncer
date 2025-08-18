using System;
using System.Windows.Forms;
using syncer.core;

namespace syncer.ui
{
    /// <summary>
    /// Simple FTP connection test utility for .NET 3.5 compatibility
    /// Use this to verify FTP functionality before setting up FileZilla tests
    /// </summary>
    public partial class FtpTestForm : Form
    {
        private GroupBox gbConnection;
        private TextBox txtHost, txtUsername, txtPassword, txtPort;
        private ComboBox cmbProtocol;
        private Button btnTest, btnClose;
        private CheckBox chkPassiveMode;
        private TextBox txtResults;
        
        public FtpTestForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "FTP Connection Test";
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
            cmbProtocol.Items.AddRange(new object[] { "FTP", "SFTP" });
            cmbProtocol.SelectedIndex = 0;
            cmbProtocol.Location = new System.Drawing.Point(100, 22);
            cmbProtocol.Size = new System.Drawing.Size(100, 21);
            
            // Host
            var lblHost = new Label();
            lblHost.Text = "Host:";
            lblHost.Location = new System.Drawing.Point(10, 55);
            lblHost.Size = new System.Drawing.Size(80, 20);
            
            txtHost = new TextBox();
            txtHost.Location = new System.Drawing.Point(100, 52);
            txtHost.Size = new System.Drawing.Size(200, 20);
            txtHost.Text = "127.0.0.1"; // Default to localhost for FileZilla server testing
            
            // Port
            var lblPort = new Label();
            lblPort.Text = "Port:";
            lblPort.Location = new System.Drawing.Point(320, 55);
            lblPort.Size = new System.Drawing.Size(40, 20);
            
            txtPort = new TextBox();
            txtPort.Location = new System.Drawing.Point(370, 52);
            txtPort.Size = new System.Drawing.Size(60, 20);
            txtPort.Text = "21"; // Default FTP port
            
            // Username
            var lblUsername = new Label();
            lblUsername.Text = "Username:";
            lblUsername.Location = new System.Drawing.Point(10, 85);
            lblUsername.Size = new System.Drawing.Size(80, 20);
            
            txtUsername = new TextBox();
            txtUsername.Location = new System.Drawing.Point(100, 82);
            txtUsername.Size = new System.Drawing.Size(150, 20);
            txtUsername.Text = "test"; // Default test user
            
            // Password
            var lblPassword = new Label();
            lblPassword.Text = "Password:";
            lblPassword.Location = new System.Drawing.Point(10, 115);
            lblPassword.Size = new System.Drawing.Size(80, 20);
            
            txtPassword = new TextBox();
            txtPassword.Location = new System.Drawing.Point(100, 112);
            txtPassword.Size = new System.Drawing.Size(150, 20);
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Text = "test"; // Default test password
            
            // Passive mode
            chkPassiveMode = new CheckBox();
            chkPassiveMode.Text = "Use Passive Mode (recommended)";
            chkPassiveMode.Location = new System.Drawing.Point(10, 145);
            chkPassiveMode.Size = new System.Drawing.Size(200, 20);
            chkPassiveMode.Checked = true;
            
            // Test button
            btnTest = new Button();
            btnTest.Text = "Test Connection";
            btnTest.Location = new System.Drawing.Point(320, 140);
            btnTest.Size = new System.Drawing.Size(120, 30);
            btnTest.Click += BtnTest_Click;
            
            // Results area
            var lblResults = new Label();
            lblResults.Text = "Test Results:";
            lblResults.Location = new System.Drawing.Point(12, 225);
            lblResults.Size = new System.Drawing.Size(100, 20);
            
            txtResults = new TextBox();
            txtResults.Location = new System.Drawing.Point(12, 250);
            txtResults.Size = new System.Drawing.Size(460, 250);
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
                lblUsername, txtUsername, lblPassword, txtPassword, chkPassiveMode, btnTest
            });
            
            // Add all to form
            this.Controls.AddRange(new Control[] {
                gbConnection, lblResults, txtResults, btnClose
            });
        }
        
        private void BtnTest_Click(object sender, EventArgs e)
        {
            txtResults.Clear();
            LogMessage("Starting FTP connection test...");
            
            try
            {
                var settings = new syncer.core.ConnectionSettings
                {
                    Protocol = cmbProtocol.SelectedItem.ToString() == "SFTP" ? syncer.core.ProtocolType.Sftp : syncer.core.ProtocolType.Ftp,
                    Host = txtHost.Text.Trim(),
                    Port = int.Parse(txtPort.Text),
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Text,
                    UsePassiveMode = chkPassiveMode.Checked
                };
                
                LogMessage($"Protocol: {settings.Protocol}");
                LogMessage($"Host: {settings.Host}");
                LogMessage($"Port: {settings.Port}");
                LogMessage($"Username: {settings.Username}");
                LogMessage($"Passive Mode: {settings.UsePassiveMode}");
                LogMessage("");
                
                ITransferClient client;
                if (settings.Protocol == syncer.core.ProtocolType.Ftp)
                {
                    client = new FtpTransferClient();
                }
                else
                {
                    client = new SftpTransferClient();
                }
                
                LogMessage("Testing connection...");
                string error;
                bool success = client.TestConnection(settings, out error);
                
                if (success)
                {
                    LogMessage("✓ Connection test SUCCESSFUL!");
                    LogMessage("");
                    
                    // Test directory listing
                    LogMessage("Testing directory listing...");
                    System.Collections.Generic.List<string> files;
                    if (client.ListFiles(settings, "/", out files, out error))
                    {
                        LogMessage($"✓ Directory listing successful. Found {files.Count} items:");
                        foreach (string file in files)
                        {
                            LogMessage($"  - {file}");
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
                    LogMessage("Common issues:");
                    LogMessage("- Check if FileZilla Server is running");
                    LogMessage("- Verify host/port settings");
                    LogMessage("- Check username/password");
                    LogMessage("- Try toggling passive mode");
                    LogMessage("- Check firewall settings");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"✗ Test failed with exception: {ex.Message}");
                LogMessage($"Stack trace: {ex.StackTrace}");
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
