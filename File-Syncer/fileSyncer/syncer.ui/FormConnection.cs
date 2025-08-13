using System;
using System.Drawing;
using System.Windows.Forms;

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
            this.Size = new Size(500, 450);
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
                if (cmbProtocol != null)
                {
                    cmbProtocol.Text = _currentSettings.Protocol;
                }
                else
                {
                    // Fallback to default if control not available
                    if (cmbProtocol != null) cmbProtocol.SelectedIndex = 0; // FTP
                }

                if (txtHost != null) txtHost.Text = _currentSettings.Host ?? "";
                if (txtPort != null) txtPort.Text = _currentSettings.Port.ToString();
                if (txtUsername != null) txtUsername.Text = _currentSettings.Username ?? "";
                if (txtPassword != null) txtPassword.Text = _currentSettings.Password ?? "";
            }
            else
            {
                // Set defaults if no settings exist
                if (cmbProtocol != null) cmbProtocol.SelectedIndex = 0; // FTP
                if (txtPort != null) txtPort.Text = "21";
            }
        }

        private void SaveSettings()
        {
            try
            {
                if (_currentSettings == null)
                    _currentSettings = new ConnectionSettings();

                _currentSettings.Protocol = cmbProtocol?.SelectedItem?.ToString() ?? "FTP";
                _currentSettings.Host = txtHost?.Text?.Trim() ?? "";
                
                if (int.TryParse(txtPort?.Text ?? "21", out int port))
                    _currentSettings.Port = port;
                else
                    _currentSettings.Port = 21;

                _currentSettings.Username = txtUsername?.Text?.Trim() ?? "";
                _currentSettings.Password = txtPassword?.Text ?? "";

                _connectionService.SaveConnectionSettings(_currentSettings);
                ServiceLocator.LogService.LogInfo("Connection settings saved");
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Error saving connection settings: " + ex.Message);
                throw;
            }
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                // Disable the button during test
                btnTestConnection.Enabled = false;
                btnTestConnection.Text = "Testing...";
                
                try
                {
                    // Create temporary settings for testing
                    var testSettings = new ConnectionSettings
                    {
                        Protocol = cmbProtocol?.SelectedItem?.ToString() ?? "FTP",
                        Host = txtHost?.Text?.Trim() ?? "",
                        Port = int.TryParse(txtPort?.Text ?? "21", out int port) ? port : 21,
                        Username = txtUsername?.Text?.Trim() ?? "",
                        Password = txtPassword?.Text ?? ""
                    };

                    bool success = _connectionService.TestConnection(testSettings);
                    
                    if (success)
                    {
                        MessageBox.Show("Connection test successful!", "Test Result", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ServiceLocator.LogService.LogInfo($"Connection test successful to {testSettings.Host}:{testSettings.Port}");
                    }
                    else
                    {
                        MessageBox.Show("Connection test failed. Please check your settings.", "Test Failed", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ServiceLocator.LogService.LogWarning($"Connection test failed to {testSettings.Host}:{testSettings.Port}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection test failed: " + ex.Message, "Test Failed", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ServiceLocator.LogService.LogError("Connection test error: " + ex.Message);
                }
                finally
                {
                    btnTestConnection.Enabled = true;
                    btnTestConnection.Text = "Test Connection";
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                try
                {
                    SaveSettings();
                    MessageBox.Show("Settings saved successfully!", "Success", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving settings: " + ex.Message, "Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            string protocol = cmbProtocol?.SelectedItem?.ToString() ?? "";
            
            // For LOCAL protocol, minimal validation needed
            if (protocol == "LOCAL")
            {
                // Just check if we can access local file system
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
                    MessageBox.Show("Local file system access error: " + ex.Message, "Local Access Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return true;
            }

            // For FTP/SFTP protocols, validate remote connection fields
            if (StringExtensions.IsNullOrWhiteSpace(txtHost?.Text))
            {
                MessageBox.Show("Please enter a host address.", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHost?.Focus();
                return false;
            }

            if (StringExtensions.IsNullOrWhiteSpace(txtPort?.Text))
            {
                MessageBox.Show("Please enter a port number.", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPort?.Focus();
                return false;
            }

            if (!int.TryParse(txtPort?.Text, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Please enter a valid port number (1-65535).", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPort?.Focus();
                return false;
            }

            if (StringExtensions.IsNullOrWhiteSpace(txtUsername?.Text))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername?.Focus();
                return false;
            }

            return true;
        }

        private void cmbProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update default port and enable/disable fields based on protocol
            if (cmbProtocol?.SelectedItem != null)
            {
                string protocol = cmbProtocol.SelectedItem.ToString();
                
                if (protocol == "LOCAL")
                {
                    // For LOCAL, disable remote connection fields
                    EnableRemoteFields(false);
                    if (txtHost != null) txtHost.Text = "localhost";
                    if (txtPort != null) txtPort.Text = "0";
                    if (txtUsername != null) txtUsername.Text = "local";
                    if (txtPassword != null) txtPassword.Text = "";
                }
                else
                {
                    // For FTP/SFTP, enable remote connection fields
                    EnableRemoteFields(true);
                    if (txtPort != null)
                    {
                        if (protocol == "FTP")
                        {
                            txtPort.Text = "21";
                        }
                        else if (protocol == "SFTP")
                        {
                            txtPort.Text = "22";
                        }
                    }
                    if (txtHost != null && txtHost.Text == "localhost") txtHost.Text = "";
                    if (txtUsername != null && txtUsername.Text == "local") txtUsername.Text = "";
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
    }
}
