using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using syncer.core;

namespace syncer.ui.Forms
{
    /// <summary>
    /// Form for generating SSH key pairs
    /// </summary>
    public partial class FormKeyGeneration : Form
    {
        public string GeneratedKeyPath { get; private set; }

        // .NET 3.5 compatibility helper
        private static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim().Length == 0;
        }

        public FormKeyGeneration()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "SSH Key Generation";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Set default values
            txtKeyName.Text = "id_rsa_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            txtKeyPath.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".ssh");
            cmbKeySize.SelectedIndex = 1; // 2048 bits
            chkUsePassphrase.Checked = true;
        }

        private void btnBrowsePath_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select directory to save SSH key";
                folderDialog.SelectedPath = txtKeyPath.Text;
                
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtKeyPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void chkUsePassphrase_CheckedChanged(object sender, EventArgs e)
        {
            txtPassphrase.Enabled = chkUsePassphrase.Checked;
            txtConfirmPassphrase.Enabled = chkUsePassphrase.Checked;
            
            if (!chkUsePassphrase.Checked)
            {
                txtPassphrase.Text = "";
                txtConfirmPassphrase.Text = "";
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            btnGenerate.Enabled = false;
            progressBar.Visible = true;
            txtOutput.Text = "Generating SSH key pair...\r\n";

            try
            {
                string keyName = txtKeyName.Text.Trim();
                string keyPath = Path.Combine(txtKeyPath.Text.Trim(), keyName);
                string passphrase = chkUsePassphrase.Checked ? txtPassphrase.Text : null;
                int keySize = int.Parse(cmbKeySize.SelectedItem.ToString().Split(' ')[0]);

                // Ensure directory exists
                Directory.CreateDirectory(txtKeyPath.Text.Trim());

                txtOutput.Text += $"Key path: {keyPath}\r\n";
                txtOutput.Text += $"Key size: {keySize} bits\r\n";
                txtOutput.Text += $"Passphrase protected: {chkUsePassphrase.Checked}\r\n\r\n";

                // Generate the key pair
                string publicKey = SftpUtilities.GenerateKeyPair(keyPath, passphrase, keySize);

                txtOutput.Text += "✓ Key pair generated successfully!\r\n\r\n";
                txtOutput.Text += $"Private key: {keyPath}\r\n";
                txtOutput.Text += $"Public key: {keyPath}.pub\r\n\r\n";
                txtOutput.Text += "Public key content:\r\n";
                txtOutput.Text += publicKey + "\r\n\r\n";

                // Validate the generated key
                if (SftpUtilities.ValidatePrivateKey(keyPath, passphrase))
                {
                    txtOutput.Text += "✓ Key validation successful!\r\n";
                    GeneratedKeyPath = keyPath;
                    btnOK.Enabled = true;
                }
                else
                {
                    txtOutput.Text += "✗ Key validation failed!\r\n";
                }

                txtOutput.Text += "\r\nIMPORTANT: Keep your private key secure and never share it.\r\n";
                txtOutput.Text += "You can share the public key content above with servers.\r\n";
            }
            catch (Exception ex)
            {
                txtOutput.Text += $"✗ Error generating key: {ex.Message}\r\n";
                MessageBox.Show($"Error generating SSH key: {ex.Message}", "Generation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGenerate.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private bool ValidateInputs()
        {
            if (IsNullOrWhiteSpace(txtKeyName.Text))
            {
                MessageBox.Show("Please enter a key name.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKeyName.Focus();
                return false;
            }

            if (IsNullOrWhiteSpace(txtKeyPath.Text))
            {
                MessageBox.Show("Please select a directory path.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKeyPath.Focus();
                return false;
            }

            if (chkUsePassphrase.Checked)
            {
                if (IsNullOrWhiteSpace(txtPassphrase.Text))
                {
                    MessageBox.Show("Please enter a passphrase.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassphrase.Focus();
                    return false;
                }

                if (txtPassphrase.Text != txtConfirmPassphrase.Text)
                {
                    MessageBox.Show("Passphrases do not match.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmPassphrase.Focus();
                    return false;
                }

                if (txtPassphrase.Text.Length < 8)
                {
                    MessageBox.Show("Passphrase must be at least 8 characters long.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassphrase.Focus();
                    return false;
                }
            }

            string fullPath = Path.Combine(txtKeyPath.Text.Trim(), txtKeyName.Text.Trim());
            if (File.Exists(fullPath))
            {
                var result = MessageBox.Show($"A key file already exists at:\n{fullPath}\n\nDo you want to overwrite it?", 
                    "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result != DialogResult.Yes)
                    return false;
            }

            return true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnCopyPublicKey_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(GeneratedKeyPath))
            {
                try
                {
                    string publicKeyPath = GeneratedKeyPath + ".pub";
                    if (File.Exists(publicKeyPath))
                    {
                        string publicKey = File.ReadAllText(publicKeyPath);
                        Clipboard.SetText(publicKey);
                        MessageBox.Show("Public key copied to clipboard!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error copying public key: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
