using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace syncer.ui
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
            this.lstConnections = new ListBox();
            this.btnEdit = new Button();
            this.btnDelete = new Button();
            this.btnSetDefault = new Button();
            this.btnTest = new Button();
            this.btnClose = new Button();
            this.lblConnectionInfo = new Label();
            this.txtConnectionInfo = new TextBox();
            this.SuspendLayout();

            // Form settings
            this.Text = "Connection Manager";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // lstConnections
            this.lstConnections.Location = new Point(12, 12);
            this.lstConnections.Size = new Size(300, 250);
            this.lstConnections.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            this.lstConnections.SelectedIndexChanged += this.lstConnections_SelectedIndexChanged;
            this.lstConnections.DoubleClick += this.lstConnections_DoubleClick;

            // btnEdit
            this.btnEdit.Location = new Point(330, 12);
            this.btnEdit.Size = new Size(75, 23);
            this.btnEdit.Text = "&Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += this.btnEdit_Click;
            this.btnEdit.Enabled = false;

            // btnDelete
            this.btnDelete.Location = new Point(330, 41);
            this.btnDelete.Size = new Size(75, 23);
            this.btnDelete.Text = "&Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += this.btnDelete_Click;
            this.btnDelete.Enabled = false;

            // btnSetDefault
            this.btnSetDefault.Location = new Point(330, 70);
            this.btnSetDefault.Size = new Size(75, 23);
            this.btnSetDefault.Text = "&Set Default";
            this.btnSetDefault.UseVisualStyleBackColor = true;
            this.btnSetDefault.Click += this.btnSetDefault_Click;
            this.btnSetDefault.Enabled = false;

            // btnTest
            this.btnTest.Location = new Point(330, 99);
            this.btnTest.Size = new Size(75, 23);
            this.btnTest.Text = "&Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += this.btnTest_Click;
            this.btnTest.Enabled = false;

            // btnClose
            this.btnClose.Location = new Point(509, 339);
            this.btnClose.Size = new Size(75, 23);
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnClose.Click += this.btnClose_Click;

            // lblConnectionInfo
            this.lblConnectionInfo.Location = new Point(330, 135);
            this.lblConnectionInfo.Size = new Size(100, 13);
            this.lblConnectionInfo.Text = "Connection Details:";

            // txtConnectionInfo
            this.txtConnectionInfo.Location = new Point(330, 151);
            this.txtConnectionInfo.Size = new Size(254, 111);
            this.txtConnectionInfo.Multiline = true;
            this.txtConnectionInfo.ReadOnly = true;
            this.txtConnectionInfo.ScrollBars = ScrollBars.Vertical;
            this.txtConnectionInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            // Add controls to form
            this.Controls.Add(this.lstConnections);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnSetDefault);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblConnectionInfo);
            this.Controls.Add(this.txtConnectionInfo);

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
    }
}
