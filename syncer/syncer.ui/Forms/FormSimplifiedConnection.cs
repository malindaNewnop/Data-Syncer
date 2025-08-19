using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using syncer.core;
using syncer.core.Transfers;

namespace syncer.ui.Forms
{
    /// <summary>
    /// Simplified enhanced connection form with basic SFTP improvements
    /// </summary>
    public partial class FormSimplifiedConnection : Form
    {
        private ConnectionSettings _settings;

        public ConnectionSettings ConnectionSettings => _settings;

        // .NET 3.5 compatibility helper
        private static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim().Length == 0;
        }

        public FormSimplifiedConnection()
        {
            InitializeComponent();
            InitializeForm();
        }

        public FormSimplifiedConnection(ConnectionSettings existingSettings)
        {
            InitializeComponent();
            _settings = existingSettings ?? new ConnectionSettings();
            InitializeForm();
            PopulateFormFromSettings();
        }

        private void InitializeForm()
        {
            if (_settings == null)
                _settings = new ConnectionSettings();

            this.Text = "Enhanced SFTP Connection Settings";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Initialize protocol combo box
            cmbProtocol.Items.Clear();
            cmbProtocol.Items.Add("Local");
            cmbProtocol.Items.Add("FTP");
            cmbProtocol.Items.Add("SFTP");
            cmbProtocol.SelectedIndex = 0;

            // Set default port
            txtPort.Text = "22";
            
            // Set default timeout
            numTimeout.Value = 30;

            // Set default key path
            try
            {
                string defaultKeyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".ssh");
                txtSshKeyPath.Text = defaultKeyPath;
            }
            catch
            {
                txtSshKeyPath.Text = "";
            }
        }

        private void PopulateFormFromSettings()
        {
            if (_settings == null) return;

            txtHost.Text = _settings.Host ?? "";
            txtPort.Text = _settings.Port.ToString();
            txtUsername.Text = _settings.Username ?? "";
            txtPassword.Text = _settings.Password ?? "";
            
            // Set protocol
            if (_settings.Protocol == "SFTP" || _settings.UseSftp)
                cmbProtocol.SelectedIndex = 2; // SFTP
            else if (_settings.Protocol == "FTP")
                cmbProtocol.SelectedIndex = 1; // FTP
            else
                cmbProtocol.SelectedIndex = 0; // Local

            txtSshKeyPath.Text = _settings.SshKeyPath ?? "";
            numTimeout.Value = _settings.Timeout;
            chkUseKeyAuth.Checked = _settings.UseKeyAuthentication;
        }

        private void UpdateSettingsFromForm()
        {
            _settings.Host = txtHost.Text.Trim();
            
            if (int.TryParse(txtPort.Text, out int port))
                _settings.Port = port;
            
            _settings.Username = txtUsername.Text.Trim();
            _settings.Password = txtPassword.Text;

            // Set protocol based on combo box selection
            switch (cmbProtocol.SelectedIndex)
            {
                case 0: // Local
                    _settings.Protocol = "LOCAL";
                    _settings.ProtocolType = 0;
                    _settings.UseSftp = false;
                    break;
                case 1: // FTP
                    _settings.Protocol = "FTP";
                    _settings.ProtocolType = 1;
                    _settings.UseSftp = false;
                    break;
                case 2: // SFTP
                    _settings.Protocol = "SFTP";
                    _settings.ProtocolType = 2;
                    _settings.UseSftp = true;
                    break;
            }

            _settings.SshKeyPath = txtSshKeyPath.Text.Trim();
            _settings.Timeout = (int)numTimeout.Value;
            _settings.UseKeyAuthentication = chkUseKeyAuth.Checked;
        }

        private void btnBrowseKey_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select SSH Private Key";
                dialog.Filter = "SSH Private Keys|*;id_rsa;id_dsa;id_ecdsa;id_ed25519|All Files|*.*";
                dialog.CheckFileExists = true;

                if (!string.IsNullOrEmpty(txtSshKeyPath.Text))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(txtSshKeyPath.Text);
                        dialog.FileName = Path.GetFileName(txtSshKeyPath.Text);
                    }
                    catch { }
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtSshKeyPath.Text = dialog.FileName;
                }
            }
        }

        private void btnGenerateKey_Click(object sender, EventArgs e)
        {
            try
            {
                using (var keyGenForm = new FormKeyGeneration())
                {
                    if (keyGenForm.ShowDialog() == DialogResult.OK)
                    {
                        if (!string.IsNullOrEmpty(keyGenForm.GeneratedKeyPath))
                        {
                            txtSshKeyPath.Text = keyGenForm.GeneratedKeyPath;
                            chkUseKeyAuth.Checked = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening key generation dialog: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            UpdateSettingsFromForm();

            try
            {
                Cursor = Cursors.WaitCursor;
                btnTestConnection.Enabled = false;
                btnTestConnection.Text = "Testing...";

                // Simple connection test using basic SFTP client
                if (_settings.Protocol == "SFTP")
                {
                    var transferClient = new ProductionSftpTransferClient();
                    
                    // Create a basic connection settings for the core library
                    var coreSettings = new syncer.core.ConnectionSettings
                    {
                        Host = _settings.Host,
                        Port = _settings.Port,
                        Username = _settings.Username,
                        Password = _settings.Password,
                        Protocol = syncer.core.ProtocolType.Sftp
                    };

                    string error;
                    bool testResult = transferClient.TestConnection(coreSettings, out error);
                    
                    if (testResult)
                    {
                        MessageBox.Show("Connection test successful!", "Test Result", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Connection test failed. Please check your settings.", "Test Result", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Connection testing is currently only supported for SFTP connections.", 
                        "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error testing connection: " + ex.Message, "Test Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnTestConnection.Enabled = true;
                btnTestConnection.Text = "Test Connection";
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            UpdateSettingsFromForm();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void cmbProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isSftp = cmbProtocol.SelectedIndex == 2; // SFTP
            bool isFtp = cmbProtocol.SelectedIndex == 1;  // FTP
            bool isLocal = cmbProtocol.SelectedIndex == 0; // Local

            // Enable/disable controls based on protocol
            txtHost.Enabled = !isLocal;
            txtPort.Enabled = !isLocal;
            txtUsername.Enabled = !isLocal;
            txtPassword.Enabled = !isLocal;
            
            // SFTP-specific controls
            txtSshKeyPath.Enabled = isSftp;
            btnBrowseKey.Enabled = isSftp;
            btnGenerateKey.Enabled = isSftp;
            chkUseKeyAuth.Enabled = isSftp;
            numTimeout.Enabled = !isLocal;
            btnTestConnection.Enabled = !isLocal;

            // Set default ports
            if (isFtp)
                txtPort.Text = "21";
            else if (isSftp)
                txtPort.Text = "22";
        }

        private void chkUseKeyAuth_CheckedChanged(object sender, EventArgs e)
        {
            bool useKey = chkUseKeyAuth.Checked;
            txtSshKeyPath.Enabled = useKey;
            btnBrowseKey.Enabled = useKey;
            btnGenerateKey.Enabled = useKey;
            
            if (!useKey)
            {
                txtSshKeyPath.Text = "";
            }
        }

        private bool ValidateForm()
        {
            if (cmbProtocol.SelectedIndex == 0) // Local
                return true;

            if (IsNullOrWhiteSpace(txtHost.Text))
            {
                MessageBox.Show("Please enter a host name or IP address.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHost.Focus();
                return false;
            }

            if (!int.TryParse(txtPort.Text, out int port) || port <= 0 || port > 65535)
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

            if (cmbProtocol.SelectedIndex == 2) // SFTP
            {
                if (chkUseKeyAuth.Checked)
                {
                    if (IsNullOrWhiteSpace(txtSshKeyPath.Text))
                    {
                        MessageBox.Show("Please select an SSH key file or disable key authentication.", "Validation Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtSshKeyPath.Focus();
                        return false;
                    }

                    if (!File.Exists(txtSshKeyPath.Text))
                    {
                        MessageBox.Show("The specified SSH key file does not exist.", "Validation Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtSshKeyPath.Focus();
                        return false;
                    }
                }
                else
                {
                    if (IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        MessageBox.Show("Please enter a password or enable key authentication.", "Validation Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtPassword.Focus();
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
