using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FTPSyncer.ui
{
    /// <summary>
    /// Form for managing saved connection profiles
    /// </summary>
    public partial class FormConnectionManager : Form
    {
        private IConnectionService _connectionService;
        private ListBox lstConnections;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnSetDefault;
        private Button btnTest;
        private Button btnClose;
        private Label lblConnectionInfo;
        private TextBox txtConnectionInfo;

        public string SelectedConnectionName { get; private set; }
        public bool ConnectionSelected { get; private set; }

        public FormConnectionManager()
        {
            InitializeComponent();
            InitializeServices();
            LoadConnections();
        }

        private void InitializeServices()
        {
            _connectionService = ServiceLocator.ConnectionService;
        }

        private void InitializeComponent()
        {
            this.lstConnections = new System.Windows.Forms.ListBox();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSetDefault = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblConnectionInfo = new System.Windows.Forms.Label();
            this.txtConnectionInfo = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lstConnections
            // 
            this.lstConnections.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstConnections.ItemHeight = 16;
            this.lstConnections.Location = new System.Drawing.Point(12, 12);
            this.lstConnections.Name = "lstConnections";
            this.lstConnections.Size = new System.Drawing.Size(300, 276);
            this.lstConnections.TabIndex = 0;
            // 
            // btnEdit
            // 
            this.btnEdit.Enabled = false;
            this.btnEdit.Location = new System.Drawing.Point(330, 12);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 23);
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Text = "&Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(330, 41);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "&Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnSetDefault
            // 
            this.btnSetDefault.Enabled = false;
            this.btnSetDefault.Location = new System.Drawing.Point(330, 70);
            this.btnSetDefault.Name = "btnSetDefault";
            this.btnSetDefault.Size = new System.Drawing.Size(75, 23);
            this.btnSetDefault.TabIndex = 3;
            this.btnSetDefault.Text = "&Set Default";
            this.btnSetDefault.UseVisualStyleBackColor = true;
            // 
            // btnTest
            // 
            this.btnTest.Enabled = false;
            this.btnTest.Location = new System.Drawing.Point(330, 99);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 4;
            this.btnTest.Text = "&Test";
            this.btnTest.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(507, 336);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click_1);
            // 
            // lblConnectionInfo
            // 
            this.lblConnectionInfo.Location = new System.Drawing.Point(330, 135);
            this.lblConnectionInfo.Name = "lblConnectionInfo";
            this.lblConnectionInfo.Size = new System.Drawing.Size(100, 13);
            this.lblConnectionInfo.TabIndex = 6;
            this.lblConnectionInfo.Text = "Connection Details:";
            // 
            // txtConnectionInfo
            // 
            this.txtConnectionInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConnectionInfo.Location = new System.Drawing.Point(330, 151);
            this.txtConnectionInfo.Multiline = true;
            this.txtConnectionInfo.Name = "txtConnectionInfo";
            this.txtConnectionInfo.ReadOnly = true;
            this.txtConnectionInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtConnectionInfo.Size = new System.Drawing.Size(287, 144);
            this.txtConnectionInfo.TabIndex = 7;
            // 
            // FormConnectionManager
            // 
            this.ClientSize = new System.Drawing.Size(615, 386);
            this.Controls.Add(this.lstConnections);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnSetDefault);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblConnectionInfo);
            this.Controls.Add(this.txtConnectionInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormConnectionManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Connection Manager";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void LoadConnections()
        {
            try
            {
                lstConnections.Items.Clear();
                
                var connections = _connectionService.GetAllConnections();
                foreach (var connection in connections)
                {
                    lstConnections.Items.Add(connection);
                }

                if (lstConnections.Items.Count == 0)
                {
                    txtConnectionInfo.Text = "No saved connections found.\r\n\r\nUse the Connection Settings dialog to create and save new connections.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading connections: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lstConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasSelection = lstConnections.SelectedItem != null;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
            btnSetDefault.Enabled = hasSelection;
            btnTest.Enabled = hasSelection;

            if (hasSelection)
            {
                var selectedConnection = (SavedConnection)lstConnections.SelectedItem;
                DisplayConnectionInfo(selectedConnection);
            }
            else
            {
                txtConnectionInfo.Clear();
            }
        }

        private void DisplayConnectionInfo(SavedConnection connection)
        {
            if (connection?.Settings == null)
            {
                txtConnectionInfo.Text = "Invalid connection data";
                return;
            }

            var info = new System.Text.StringBuilder();
            info.AppendLine($"Name: {connection.Name}");
            info.AppendLine($"Type: {connection.Settings.ConnectionTypeDisplay}");
            info.AppendLine($"Status: {(connection.IsDefault ? "Default Connection" : "Saved Connection")}");
            info.AppendLine();

            if (!connection.Settings.IsLocalConnection)
            {
                info.AppendLine($"Protocol: {connection.Settings.Protocol}");
                info.AppendLine($"Host: {connection.Settings.Host}");
                info.AppendLine($"Port: {connection.Settings.Port}");
                info.AppendLine($"Username: {connection.Settings.Username}");
                info.AppendLine($"Authentication: {(string.IsNullOrEmpty(connection.Settings.SshKeyPath) ? "Password" : "SSH Key")}");
            }

            info.AppendLine();
            info.AppendLine($"Created: {connection.CreatedDate:yyyy-MM-dd HH:mm}");
            info.AppendLine($"Last Used: {connection.LastUsed?.ToString("yyyy-MM-dd HH:mm") ?? "Never"}");

            if (!string.IsNullOrEmpty(connection.Description))
            {
                info.AppendLine();
                info.AppendLine($"Description: {connection.Description}");
            }

            txtConnectionInfo.Text = info.ToString();
        }

        private void lstConnections_DoubleClick(object sender, EventArgs e)
        {
            if (lstConnections.SelectedItem != null)
            {
                SelectConnection();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lstConnections.SelectedItem == null) return;

            var selectedConnection = (SavedConnection)lstConnections.SelectedItem;
            
            try
            {
                using (var connectionForm = new FormConnection())
                {
                    // Load the selected connection settings into the form
                    var connectionService = ServiceLocator.ConnectionService;
                    connectionService.SaveConnectionSettings(selectedConnection.Settings);

                    if (connectionForm.ShowDialog(this) == DialogResult.OK)
                    {
                        // Refresh the connections list
                        LoadConnections();
                        
                        // Try to maintain selection if possible
                        for (int i = 0; i < lstConnections.Items.Count; i++)
                        {
                            var conn = (SavedConnection)lstConnections.Items[i];
                            if (conn.Name == selectedConnection.Name)
                            {
                                lstConnections.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing connection: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstConnections.SelectedItem == null) return;

            var selectedConnection = (SavedConnection)lstConnections.SelectedItem;
            
            var result = MessageBox.Show(
                $"Are you sure you want to delete the connection '{selectedConnection.Name}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (_connectionService.DeleteConnection(selectedConnection.Name))
                    {
                        LoadConnections();
                        MessageBox.Show($"Connection '{selectedConnection.Name}' deleted successfully.", 
                            "Connection Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete connection.", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting connection: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSetDefault_Click(object sender, EventArgs e)
        {
            if (lstConnections.SelectedItem == null) return;

            var selectedConnection = (SavedConnection)lstConnections.SelectedItem;
            
            try
            {
                if (_connectionService.SetDefaultConnection(selectedConnection.Name))
                {
                    LoadConnections(); // Refresh to show updated default status
                    MessageBox.Show($"'{selectedConnection.Name}' is now the default connection.", 
                        "Default Set", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to set default connection.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting default connection: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (lstConnections.SelectedItem == null) return;

            var selectedConnection = (SavedConnection)lstConnections.SelectedItem;
            
            btnTest.Enabled = false;
            btnTest.Text = "Testing...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                bool success = _connectionService.TestConnection(selectedConnection.Settings);
                
                if (success)
                {
                    MessageBox.Show($"✓ Connection test successful!\n\nConnection '{selectedConnection.Name}' is working properly.", 
                        "Test Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"✗ Connection test failed!\n\nConnection '{selectedConnection.Name}' could not be established.\n\nPlease check the connection settings.", 
                        "Test Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"✗ Connection test error!\n\nError: {ex.Message}", 
                    "Test Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTest.Enabled = true;
                btnTest.Text = "&Test";
                this.Cursor = Cursors.Default;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SelectConnection()
        {
            if (lstConnections.SelectedItem != null)
            {
                var selectedConnection = (SavedConnection)lstConnections.SelectedItem;
                SelectedConnectionName = selectedConnection.Name;
                ConnectionSelected = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// Show dialog for selecting a connection
        /// </summary>
        public static string ShowConnectionSelector(IWin32Window owner)
        {
            using (var form = new FormConnectionManager())
            {
                form.Text = "Select Connection";
                form.btnEdit.Visible = false;
                form.btnDelete.Visible = false;
                form.btnSetDefault.Visible = false;
                
                // Add Select button
                var btnSelect = new Button
                {
                    Text = "&Select",
                    Location = new Point(form.btnClose.Location.X - 85, form.btnClose.Location.Y),
                    Size = new Size(75, 23),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };
                btnSelect.Click += (s, e) => form.SelectConnection();
                form.Controls.Add(btnSelect);

                if (form.ShowDialog(owner) == DialogResult.OK && form.ConnectionSelected)
                {
                    return form.SelectedConnectionName;
                }
            }

            return null;
        }

        private void btnClose_Click_1(object sender, EventArgs e)
        {

        }
    }
}





