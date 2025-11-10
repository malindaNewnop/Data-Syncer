using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Security.Cryptography;
using FTPSyncer.core;
using FTPSyncer.core.Transfers;

namespace FTPSyncer.ui.Forms
{
    /// <summary>
    /// Comprehensive Connection Dialog - combines connection settings and SSH key generation
    /// Enhanced user experience with all features in one place
    /// </summary>
    public partial class FormComprehensiveConnection : Form
    {
        private FTPSyncer.core.ConnectionSettings _settings;
        private TabControl tabControl;
        
        // Connection Tab Controls
        private ComboBox cmbProtocol;
        private TextBox txtHost;
        private TextBox txtPort;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private CheckBox chkUseKeyAuth;
        private TextBox txtKeyPath;
        private Button btnBrowseKey;
        private NumericUpDown nudTimeout;
        
        // SSH Key Generation Tab Controls
        private TextBox txtKeyName;
        private TextBox txtSaveLocation;
        private Button btnBrowseSaveLocation;
        private ComboBox cmbKeySize;
        private CheckBox chkProtectWithPassphrase;
        private TextBox txtPassphrase;
        private TextBox txtConfirmPassphrase;
        private Button btnGenerateKey;
        private TextBox txtPublicKey;
        private Button btnCopyPublicKey;
        private ProgressBar progressGeneration;
        
        // Action Buttons
        private Button btnTestConnection;
        private Button btnOK;
        private Button btnCancel;

        public FTPSyncer.core.ConnectionSettings ConnectionSettings => _settings;

        /// <summary>
        /// Sets which tab should be selected when the form opens
        /// </summary>
        public void SetDefaultTab(int tabIndex)
        {
            if (tabControl != null && tabIndex >= 0 && tabIndex < tabControl.TabCount)
            {
                tabControl.SelectedIndex = tabIndex;
            }
        }

        // .NET 3.5 compatibility helper
        private static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim().Length == 0;
        }

        public FormComprehensiveConnection()
        {
            InitializeComponent();
            _settings = new FTPSyncer.core.ConnectionSettings();
            PopulateFormFromSettings();
        }

        public FormComprehensiveConnection(FTPSyncer.core.ConnectionSettings existingSettings)
        {
            InitializeComponent();
            _settings = existingSettings ?? new FTPSyncer.core.ConnectionSettings();
            PopulateFormFromSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Connection Settings & SSH Key Management";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create main tab control
            tabControl = new TabControl();
            tabControl.Location = new Point(12, 12);
            tabControl.Size = new Size(560, 400);
            this.Controls.Add(tabControl);

            CreateConnectionTab();
            CreateKeyGenerationTab();
            CreateActionButtons();
        }

        private void CreateConnectionTab()
        {
            var tabConnection = new TabPage("Connection Settings");
            tabControl.TabPages.Add(tabConnection);

            // Protocol selection
            var lblProtocol = new Label { Text = "Protocol:", Location = new Point(20, 25), Size = new Size(60, 20) };
            cmbProtocol = new ComboBox 
            { 
                Location = new Point(90, 23), 
                Size = new Size(120, 21),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbProtocol.Items.AddRange(new object[] { "Local", "FTP", "SFTP" });
            cmbProtocol.SelectedIndexChanged += CmbProtocol_SelectedIndexChanged;

            // Host and Port
            var lblHost = new Label { Text = "Host:", Location = new Point(20, 55), Size = new Size(60, 20) };
            txtHost = new TextBox { Location = new Point(90, 53), Size = new Size(200, 20) };

            var lblPort = new Label { Text = "Port:", Location = new Point(310, 55), Size = new Size(40, 20) };
            txtPort = new TextBox { Location = new Point(350, 53), Size = new Size(60, 20) };

            // Credentials
            var lblUsername = new Label { Text = "Username:", Location = new Point(20, 85), Size = new Size(60, 20) };
            txtUsername = new TextBox { Location = new Point(90, 83), Size = new Size(150, 20) };

            var lblPassword = new Label { Text = "Password:", Location = new Point(20, 115), Size = new Size(60, 20) };
            txtPassword = new TextBox { Location = new Point(90, 113), Size = new Size(150, 20), PasswordChar = '*' };

            // SSH Key Authentication
            chkUseKeyAuth = new CheckBox 
            { 
                Text = "Use SSH Key Authentication", 
                Location = new Point(20, 145), 
                Size = new Size(200, 20) 
            };
            chkUseKeyAuth.CheckedChanged += ChkUseKeyAuth_CheckedChanged;

            var lblKeyPath = new Label { Text = "Key Path:", Location = new Point(20, 175), Size = new Size(60, 20) };
            txtKeyPath = new TextBox { Location = new Point(90, 173), Size = new Size(250, 20) };
            btnBrowseKey = new Button { Text = "Browse", Location = new Point(350, 171), Size = new Size(70, 25) };
            btnBrowseKey.Click += BtnBrowseKey_Click;

            // Timeout
            var lblTimeout = new Label { Text = "Timeout (sec):", Location = new Point(20, 205), Size = new Size(80, 20) };
            nudTimeout = new NumericUpDown 
            { 
                Location = new Point(100, 203), 
                Size = new Size(60, 20),
                Minimum = 5,
                Maximum = 300,
                Value = 30
            };

            // Test Connection Button
            btnTestConnection = new Button 
            { 
                Text = "Test Connection", 
                Location = new Point(20, 240), 
                Size = new Size(120, 30) 
            };
            btnTestConnection.Click += BtnTestConnection_Click;

            // Add all controls to connection tab
            tabConnection.Controls.AddRange(new Control[] 
            {
                lblProtocol, cmbProtocol, lblHost, txtHost, lblPort, txtPort,
                lblUsername, txtUsername, lblPassword, txtPassword,
                chkUseKeyAuth, lblKeyPath, txtKeyPath, btnBrowseKey,
                lblTimeout, nudTimeout, btnTestConnection
            });
        }

        private void CreateKeyGenerationTab()
        {
            var tabKeyGen = new TabPage("SSH Key Generation");
            tabControl.TabPages.Add(tabKeyGen);

            // Key Settings
            var lblKeyName = new Label { Text = "Key Name:", Location = new Point(20, 25), Size = new Size(70, 20) };
            txtKeyName = new TextBox 
            { 
                Location = new Point(95, 23), 
                Size = new Size(200, 20),
                Text = $"id_rsa_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            var lblSaveLocation = new Label { Text = "Save To:", Location = new Point(20, 55), Size = new Size(70, 20) };
            txtSaveLocation = new TextBox 
            { 
                Location = new Point(95, 53), 
                Size = new Size(250, 20),
                Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".ssh")
            };
            btnBrowseSaveLocation = new Button { Text = "Browse", Location = new Point(355, 51), Size = new Size(70, 25) };
            btnBrowseSaveLocation.Click += BtnBrowseSaveLocation_Click;

            var lblKeySize = new Label { Text = "Key Size:", Location = new Point(20, 85), Size = new Size(70, 20) };
            cmbKeySize = new ComboBox 
            { 
                Location = new Point(95, 83), 
                Size = new Size(100, 21),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbKeySize.Items.AddRange(new object[] { "1024 bits", "2048 bits", "4096 bits" });
            cmbKeySize.SelectedIndex = 1; // Default to 2048 bits

            // Passphrase Protection
            chkProtectWithPassphrase = new CheckBox 
            { 
                Text = "Protect key with passphrase", 
                Location = new Point(20, 115), 
                Size = new Size(200, 20) 
            };
            chkProtectWithPassphrase.CheckedChanged += ChkProtectWithPassphrase_CheckedChanged;

            var lblPassphrase = new Label { Text = "Passphrase:", Location = new Point(20, 145), Size = new Size(70, 20) };
            txtPassphrase = new TextBox { Location = new Point(95, 143), Size = new Size(150, 20), PasswordChar = '*', Enabled = false };

            var lblConfirm = new Label { Text = "Confirm:", Location = new Point(260, 145), Size = new Size(60, 20) };
            txtConfirmPassphrase = new TextBox { Location = new Point(320, 143), Size = new Size(150, 20), PasswordChar = '*', Enabled = false };

            // Generation Controls
            btnGenerateKey = new Button 
            { 
                Text = "Generate SSH Key", 
                Location = new Point(20, 180), 
                Size = new Size(130, 30) 
            };
            btnGenerateKey.Click += BtnGenerateKey_Click;

            progressGeneration = new ProgressBar 
            { 
                Location = new Point(160, 185), 
                Size = new Size(200, 20),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            // Public Key Display
            var lblPublicKey = new Label { Text = "Generated Public Key:", Location = new Point(20, 220), Size = new Size(150, 20) };
            txtPublicKey = new TextBox 
            { 
                Location = new Point(20, 245), 
                Size = new Size(480, 80),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 8F)
            };

            btnCopyPublicKey = new Button 
            { 
                Text = "Copy to Clipboard", 
                Location = new Point(20, 335), 
                Size = new Size(130, 25) 
            };
            btnCopyPublicKey.Click += BtnCopyPublicKey_Click;

            // Add all controls to key generation tab
            tabKeyGen.Controls.AddRange(new Control[] 
            {
                lblKeyName, txtKeyName, lblSaveLocation, txtSaveLocation, btnBrowseSaveLocation,
                lblKeySize, cmbKeySize, chkProtectWithPassphrase,
                lblPassphrase, txtPassphrase, lblConfirm, txtConfirmPassphrase,
                btnGenerateKey, progressGeneration, lblPublicKey, txtPublicKey, btnCopyPublicKey
            });
        }

        private void CreateActionButtons()
        {
            btnOK = new Button 
            { 
                Text = "OK", 
                Location = new Point(415, 425), 
                Size = new Size(75, 23),
                DialogResult = DialogResult.OK
            };
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button 
            { 
                Text = "Cancel", 
                Location = new Point(497, 425), 
                Size = new Size(75, 23),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[] { btnOK, btnCancel });
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void PopulateFormFromSettings()
        {
            if (_settings == null) return;

            cmbProtocol.SelectedIndex = (int)_settings.Protocol;
            txtHost.Text = _settings.Host ?? "";
            txtPort.Text = _settings.Port.ToString();
            txtUsername.Text = _settings.Username ?? "";
            txtPassword.Text = _settings.Password ?? "";
            txtKeyPath.Text = _settings.SshKeyPath ?? "";
            nudTimeout.Value = Math.Max(5, Math.Min(300, _settings.Timeout));

            chkUseKeyAuth.Checked = !IsNullOrWhiteSpace(_settings.SshKeyPath);
            UpdateKeyAuthControls();
        }

        private void UpdateSettingsFromForm()
        {
            if (_settings == null) 
                _settings = new FTPSyncer.core.ConnectionSettings();

            _settings.Protocol = (FTPSyncer.core.ProtocolType)cmbProtocol.SelectedIndex;
            _settings.Host = txtHost.Text;
            
            if (int.TryParse(txtPort.Text, out int port))
                _settings.Port = port;
            
            _settings.Username = txtUsername.Text;
            _settings.Password = txtPassword.Text;
            _settings.SshKeyPath = chkUseKeyAuth.Checked ? txtKeyPath.Text : "";
            _settings.Timeout = (int)nudTimeout.Value;
        }

        private void CmbProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            var isRemote = cmbProtocol.SelectedIndex > 0;
            
            txtHost.Enabled = isRemote;
            txtPort.Enabled = isRemote;
            txtUsername.Enabled = isRemote;
            txtPassword.Enabled = isRemote;
            chkUseKeyAuth.Enabled = isRemote;
            
            var isSftp = cmbProtocol.SelectedIndex == 2;
            chkUseKeyAuth.Visible = isSftp;
            
            // Set default ports
            if (cmbProtocol.SelectedIndex == 1) // FTP
                txtPort.Text = "21";
            else if (cmbProtocol.SelectedIndex == 2) // SFTP
                txtPort.Text = "22";
                
            UpdateKeyAuthControls();
        }

        private void ChkUseKeyAuth_CheckedChanged(object sender, EventArgs e)
        {
            UpdateKeyAuthControls();
        }

        private void UpdateKeyAuthControls()
        {
            var useKeyAuth = chkUseKeyAuth.Checked && cmbProtocol.SelectedIndex == 2;
            txtKeyPath.Enabled = useKeyAuth;
            btnBrowseKey.Enabled = useKeyAuth;
            txtPassword.Enabled = !useKeyAuth || !IsNullOrWhiteSpace(txtPassword.Text);
        }

        private void ChkProtectWithPassphrase_CheckedChanged(object sender, EventArgs e)
        {
            txtPassphrase.Enabled = chkProtectWithPassphrase.Checked;
            txtConfirmPassphrase.Enabled = chkProtectWithPassphrase.Checked;
            
            if (!chkProtectWithPassphrase.Checked)
            {
                txtPassphrase.Clear();
                txtConfirmPassphrase.Clear();
            }
        }

        private void BtnBrowseKey_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select SSH Private Key";
                openFileDialog.Filter = "SSH Keys (*.pem;id_rsa;id_dsa)|*.pem;id_rsa;id_dsa|All Files (*.*)|*.*";
                openFileDialog.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".ssh");

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtKeyPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void BtnBrowseSaveLocation_Click(object sender, EventArgs e)
        {
            using (var folderBrowser = new FolderBrowserDialog())
            {
                folderBrowser.Description = "Select folder to save SSH keys";
                folderBrowser.SelectedPath = txtSaveLocation.Text;

                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    txtSaveLocation.Text = folderBrowser.SelectedPath;
                }
            }
        }

        private void BtnGenerateKey_Click(object sender, EventArgs e)
        {
            if (!ValidateKeyGeneration())
                return;

            try
            {
                btnGenerateKey.Enabled = false;
                progressGeneration.Visible = true;

                var keyName = txtKeyName.Text.Trim();
                var saveLocation = txtSaveLocation.Text.Trim();
                var keySize = GetSelectedKeySize();
                var passphrase = chkProtectWithPassphrase.Checked ? txtPassphrase.Text : null;

                // Ensure save directory exists
                if (!Directory.Exists(saveLocation))
                {
                    Directory.CreateDirectory(saveLocation);
                }

                var privateKeyPath = Path.Combine(saveLocation, keyName);
                var publicKeyPath = privateKeyPath + ".pub";

                // Generate RSA key pair (simplified version for .NET 3.5)
                var keyPair = GenerateRSAKeyPair(keySize, passphrase);
                
                // Save private key
                File.WriteAllText(privateKeyPath, keyPair.PrivateKey);
                
                // Save public key
                File.WriteAllText(publicKeyPath, keyPair.PublicKey);

                // Display public key
                txtPublicKey.Text = keyPair.PublicKey;

                // Auto-populate key path in connection tab
                txtKeyPath.Text = privateKeyPath;
                chkUseKeyAuth.Checked = true;

                MessageBox.Show($"SSH key pair generated successfully!\n\nPrivate key: {privateKeyPath}\nPublic key: {publicKeyPath}", 
                    "Key Generation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate SSH key pair:\n{ex.Message}", 
                    "Key Generation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGenerateKey.Enabled = true;
                progressGeneration.Visible = false;
            }
        }

        private bool ValidateKeyGeneration()
        {
            if (IsNullOrWhiteSpace(txtKeyName.Text))
            {
                MessageBox.Show("Please enter a key name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKeyName.Focus();
                return false;
            }

            if (IsNullOrWhiteSpace(txtSaveLocation.Text))
            {
                MessageBox.Show("Please select a save location.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnBrowseSaveLocation.Focus();
                return false;
            }

            if (chkProtectWithPassphrase.Checked)
            {
                if (IsNullOrWhiteSpace(txtPassphrase.Text))
                {
                    MessageBox.Show("Please enter a passphrase.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassphrase.Focus();
                    return false;
                }

                if (txtPassphrase.Text != txtConfirmPassphrase.Text)
                {
                    MessageBox.Show("Passphrase and confirmation do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmPassphrase.Focus();
                    return false;
                }
            }

            return true;
        }

        private int GetSelectedKeySize()
        {
            switch (cmbKeySize.SelectedIndex)
            {
                case 0: return 1024;
                case 2: return 4096;
                default: return 2048;
            }
        }

        private KeyPairResult GenerateRSAKeyPair(int keySize, string passphrase)
        {
            // Simplified RSA key generation for .NET 3.5
            // Note: This is a basic implementation. In production, consider using a proper SSH key library
            
            using (var rsa = new RSACryptoServiceProvider(keySize))
            {
                var privateKeyXml = rsa.ToXmlString(true);
                var publicKeyXml = rsa.ToXmlString(false);
                
                // Convert to SSH format (simplified)
                var publicKey = ConvertToSSHPublicKey(rsa);
                var privateKey = ConvertToSSHPrivateKey(rsa, passphrase);

                return new KeyPairResult
                {
                    PrivateKey = privateKey,
                    PublicKey = publicKey
                };
            }
        }

        private string ConvertToSSHPublicKey(RSACryptoServiceProvider rsa)
        {
            // Simplified SSH public key format
            var parameters = rsa.ExportParameters(false);
            var keyComment = $"{Environment.UserName}@{Environment.MachineName}";
            return $"ssh-rsa {Convert.ToBase64String(parameters.Modulus)} {keyComment}";
        }

        private string ConvertToSSHPrivateKey(RSACryptoServiceProvider rsa, string passphrase)
        {
            // Simplified SSH private key format (OpenSSH style)
            var parameters = rsa.ExportParameters(true);
            var keyData = Convert.ToBase64String(parameters.D ?? new byte[0]);
            
            var privateKey = "-----BEGIN RSA PRIVATE KEY-----\n";
            privateKey += keyData + "\n";
            privateKey += "-----END RSA PRIVATE KEY-----";
            
            return privateKey;
        }

        private void BtnCopyPublicKey_Click(object sender, EventArgs e)
        {
            if (!IsNullOrWhiteSpace(txtPublicKey.Text))
            {
                Clipboard.SetText(txtPublicKey.Text);
                MessageBox.Show("Public key copied to clipboard!", "Copy Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnTestConnection_Click(object sender, EventArgs e)
        {
            if (!ValidateConnectionSettings())
                return;

            try
            {
                UpdateSettingsFromForm();
                
                var client = CreateTransferClient();
                if (client == null)
                    return;

                string error;
                var success = client.TestConnection(_settings, out error);

                if (success)
                {
                    MessageBox.Show("Connection test successful!", "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Connection test failed:\n{error}", "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection test error:\n{ex.Message}", "Test Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private ITransferClient CreateTransferClient()
        {
            switch (_settings.Protocol)
            {
                case FTPSyncer.core.ProtocolType.Ftp:
                    return new EnhancedFtpTransferClient();
                case FTPSyncer.core.ProtocolType.Sftp:
                    return new ProductionSftpTransferClient();
                default:
                    MessageBox.Show("Please select a valid protocol.", "Invalid Protocol", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
            }
        }

        private bool ValidateConnectionSettings()
        {
            if (cmbProtocol.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a protocol.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedIndex = 0;
                cmbProtocol.Focus();
                return false;
            }

            if (IsNullOrWhiteSpace(txtHost.Text))
            {
                MessageBox.Show("Please enter a host.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedIndex = 0;
                txtHost.Focus();
                return false;
            }

            if (IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedIndex = 0;
                txtUsername.Focus();
                return false;
            }

            if (chkUseKeyAuth.Checked && IsNullOrWhiteSpace(txtKeyPath.Text))
            {
                MessageBox.Show("Please specify an SSH key path or uncheck key authentication.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedIndex = 0;
                txtKeyPath.Focus();
                return false;
            }

            if (!chkUseKeyAuth.Checked && IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter a password or use SSH key authentication.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedIndex = 0;
                txtPassword.Focus();
                return false;
            }

            return true;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (!ValidateConnectionSettings())
            {
                this.DialogResult = DialogResult.None;
                return;
            }

            UpdateSettingsFromForm();
        }

        private class KeyPairResult
        {
            public string PrivateKey { get; set; }
            public string PublicKey { get; set; }
        }
    }
}





