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
                    cmbProtocol.Text = _currentSettings.Protocol;
                if (txtHost != null) txtHost.Text = _currentSettings.Host ?? string.Empty;
                if (txtPort != null) txtPort.Text = _currentSettings.Port.ToString();
                if (txtUsername != null) txtUsername.Text = _currentSettings.Username ?? string.Empty;
                if (txtPassword != null) txtPassword.Text = _currentSettings.Password ?? string.Empty;
            }
            else
            {
                if (cmbProtocol != null) cmbProtocol.SelectedIndex = 0; // FTP
                if (txtPort != null) txtPort.Text = "21";
            }
        }

        private void SaveSettings()
        {
            if (_currentSettings == null) _currentSettings = new ConnectionSettings();
            _currentSettings.Protocol = cmbProtocol != null && cmbProtocol.SelectedItem != null ? cmbProtocol.SelectedItem.ToString() : "FTP";
            _currentSettings.Host = txtHost != null ? txtHost.Text.Trim() : string.Empty;
            int port;
            if (!int.TryParse(txtPort != null ? txtPort.Text : "21", out port)) port = 21;
            _currentSettings.Port = port;
            _currentSettings.Username = txtUsername != null ? txtUsername.Text.Trim() : string.Empty;
            _currentSettings.Password = txtPassword != null ? txtPassword.Text : string.Empty;
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
                    ConnectionSettings testSettings = new ConnectionSettings();
                    testSettings.Protocol = cmbProtocol != null && cmbProtocol.SelectedItem != null ? cmbProtocol.SelectedItem.ToString() : "FTP";
                    testSettings.Host = txtHost != null ? txtHost.Text.Trim() : string.Empty;
                    int p; if (!int.TryParse(txtPort != null ? txtPort.Text : "21", out p)) p = 21; testSettings.Port = p;
                    testSettings.Username = txtUsername != null ? txtUsername.Text.Trim() : string.Empty;
                    testSettings.Password = txtPassword != null ? txtPassword.Text : string.Empty;

                    bool success = _connectionService.TestConnection(testSettings);
                    if (success)
                    {
                        MessageBox.Show("Connection test successful!", "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ServiceLocator.LogService.LogInfo("Connection test successful to " + testSettings.Host + ":" + testSettings.Port);
                    }
                    else
                    {
                        MessageBox.Show("Connection test failed. Please check your settings.", "Test Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            string protocol = cmbProtocol != null && cmbProtocol.SelectedItem != null ? cmbProtocol.SelectedItem.ToString() : string.Empty;
            if (protocol == "LOCAL")
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
            if (StringExtensions.IsNullOrWhiteSpace(txtHost != null ? txtHost.Text : null))
            {
                MessageBox.Show("Please enter a host address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (txtHost != null) txtHost.Focus();
                return false;
            }
            if (StringExtensions.IsNullOrWhiteSpace(txtPort != null ? txtPort.Text : null))
            {
                MessageBox.Show("Please enter a port number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (txtPort != null) txtPort.Focus();
                return false;
            }
            int port; if (!int.TryParse(txtPort.Text, out port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Please enter a valid port number (1-65535).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (txtPort != null) txtPort.Focus();
                return false;
            }
            if (StringExtensions.IsNullOrWhiteSpace(txtUsername != null ? txtUsername.Text : null))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (txtUsername != null) txtUsername.Focus();
                return false;
            }
            return true;
        }

        private void cmbProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProtocol != null && cmbProtocol.SelectedItem != null)
            {
                string protocol = cmbProtocol.SelectedItem.ToString();
                if (protocol == "LOCAL")
                {
                    EnableRemoteFields(false);
                    if (txtHost != null) txtHost.Text = "localhost";
                    if (txtPort != null) txtPort.Text = "0";
                    if (txtUsername != null) txtUsername.Text = "local";
                    if (txtPassword != null) txtPassword.Text = string.Empty;
                }
                else
                {
                    EnableRemoteFields(true);
                    if (txtPort != null)
                        txtPort.Text = protocol == "FTP" ? "21" : "22";
                    if (txtHost != null && txtHost.Text == "localhost") txtHost.Text = string.Empty;
                    if (txtUsername != null && txtUsername.Text == "local") txtUsername.Text = string.Empty;
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
