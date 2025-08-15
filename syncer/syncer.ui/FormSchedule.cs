using System;
using System.Drawing;
using System.Windows.Forms;
using syncer.core;
using syncer.core.Configuration;
using syncer.ui.Forms;

namespace syncer.ui
{
    public partial class FormSchedule : Form
    {
        private ISyncJobService _jobService;
        private IConnectionService _connectionService;
        private SyncJob _currentJob;
        private bool _isEditMode;

        public FormSchedule() : this(null) { }

        public FormSchedule(SyncJob jobToEdit)
        {
            InitializeComponent();
            InitializeServices();
            _currentJob = jobToEdit;
            _isEditMode = jobToEdit != null;
            InitializeCustomComponents();
        }

        private void InitializeServices()
        {
            _jobService = ServiceLocator.SyncJobService;
            _connectionService = ServiceLocator.ConnectionService;
        }

        private void InitializeCustomComponents()
        {
            this.Text = _isEditMode ? "Edit Schedule Settings" : "Add Schedule Settings";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            SetDefaultValues();
            if (_isEditMode) LoadJobSettings();
        }

        private void SetDefaultValues()
        {
            if (chkEnabled != null) chkEnabled.Checked = true;
            if (dtpStartTime != null) dtpStartTime.Value = DateTime.Now.Date.AddHours(9);
            if (numInterval != null) numInterval.Value = 60;
            if (cmbIntervalType != null) cmbIntervalType.SelectedIndex = 1;
        }

        private void LoadJobSettings()
        {
            if (_currentJob != null)
            {
                if (txtJobName != null) txtJobName.Text = _currentJob.Name;
                if (chkEnabled != null) chkEnabled.Checked = _currentJob.IsEnabled;
                if (txtSourcePath != null) txtSourcePath.Text = _currentJob.SourcePath;
                if (txtDestinationPath != null) txtDestinationPath.Text = _currentJob.DestinationPath;
                if (dtpStartTime != null) dtpStartTime.Value = _currentJob.StartTime;
                if (numInterval != null) numInterval.Value = _currentJob.IntervalValue;
                if (cmbIntervalType != null)
                {
                    string[] items = new string[] { "Minutes", "Hours", "Days" };
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i] == _currentJob.IntervalType)
                        {
                            cmbIntervalType.SelectedIndex = i;
                            break;
                        }
                    }
                }
                if (cmbTransferMode != null && !UIStringExtensions.IsNullOrWhiteSpace(_currentJob.TransferMode))
                {
                    cmbTransferMode.Text = _currentJob.TransferMode;
                }
            }
        }

        private void btnBrowseSource_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select source folder to sync";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtSourcePath.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnBrowseDestination_Click(object sender, EventArgs e)
        {
            ConnectionSettings connectionSettings = _connectionService.GetConnectionSettings();
            if (connectionSettings != null && connectionSettings.Protocol == "LOCAL")
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Select destination folder for local sync";
                    dialog.ShowNewFolderButton = true;
                    if (txtDestinationPath != null && !UIStringExtensions.IsNullOrWhiteSpace(txtDestinationPath.Text))
                        dialog.SelectedPath = txtDestinationPath.Text;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (txtDestinationPath != null) txtDestinationPath.Text = dialog.SelectedPath;
                    }
                }
            }
            else if (connectionSettings != null && (connectionSettings.Protocol == "FTP" || connectionSettings.Protocol == "SFTP"))
            {
                if (_connectionService.IsConnected())
                {
                    // Convert UI ConnectionSettings to Core ConnectionSettings
                    var coreSettings = new syncer.core.ConnectionSettings
                    {
                        Protocol = connectionSettings.Protocol == "SFTP" ? 
                            syncer.core.ProtocolType.Sftp : 
                            connectionSettings.Protocol == "FTP" ? 
                                syncer.core.ProtocolType.Ftp : 
                                syncer.core.ProtocolType.Local,
                        Host = connectionSettings.Host,
                        Port = connectionSettings.Port,
                        Username = connectionSettings.Username,
                        Password = connectionSettings.Password,
                        SshKeyPath = connectionSettings.SshKeyPath,
                        Timeout = connectionSettings.Timeout
                    };
                    
                    // Use the remote directory browser
                    try
                    {
                        // For now, we'll use a simple input dialog since FormRemoteDirectoryBrowser would need more work
                        // to handle the connection settings type differences
                        ShowRemotePathInputDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error browsing remote directory: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ShowRemotePathInputDialog();
                    }
                }
                else
                {
                    MessageBox.Show("Not connected to a remote server. Please connect first.", 
                        "Connection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    
                    // Show remote path input dialog for now
                    ShowRemotePathInputDialog();
                }
            }
            else
            {
                ShowRemotePathInputDialog();
            }
        }

        private void ShowRemotePathInputDialog()
        {
            using (Form inputForm = new Form())
            {
                ConnectionSettings connectionSettings = _connectionService.GetConnectionSettings();
                string protocol = connectionSettings != null ? connectionSettings.Protocol : "Unknown";
                string host = connectionSettings != null ? connectionSettings.Host : "";
                
                inputForm.Text = $"Enter Remote Destination Path ({protocol})";
                inputForm.Size = new Size(450, 180);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;
                
                Label labelInfo = new Label(); 
                labelInfo.Left = 10; 
                labelInfo.Top = 10; 
                labelInfo.Width = 420;
                labelInfo.Text = $"Enter the remote path on {protocol} server: {host}"; 
                labelInfo.AutoSize = false;
                
                Label label = new Label(); 
                label.Left = 10; 
                label.Top = 40; 
                label.Text = "Remote path (e.g., /remote/folder):"; 
                label.AutoSize = true;
                
                TextBox textBox = new TextBox(); 
                textBox.Left = 10; 
                textBox.Top = 65; 
                textBox.Width = 410; 
                textBox.Text = txtDestinationPath != null ? txtDestinationPath.Text : string.Empty;
                
                Label tipLabel = new Label();
                tipLabel.Left = 10;
                tipLabel.Top = 95;
                tipLabel.Width = 420;
                tipLabel.Height = 30;
                tipLabel.Text = "Tip: For FTP/SFTP, paths typically start with / and use forward slashes.";
                tipLabel.ForeColor = Color.DarkBlue;
                tipLabel.AutoSize = false;
                
                Button okButton = new Button(); 
                okButton.Text = "OK"; 
                okButton.Left = 255; 
                okButton.Width = 75; 
                okButton.Top = 130; 
                okButton.DialogResult = DialogResult.OK;
                
                Button cancelButton = new Button(); 
                cancelButton.Text = "Cancel"; 
                cancelButton.Left = 345; 
                cancelButton.Width = 75; 
                cancelButton.Top = 130; 
                cancelButton.DialogResult = DialogResult.Cancel;
                
                inputForm.Controls.Add(labelInfo);
                inputForm.Controls.Add(label);
                inputForm.Controls.Add(textBox);
                inputForm.Controls.Add(tipLabel);
                inputForm.Controls.Add(okButton);
                inputForm.Controls.Add(cancelButton);
                inputForm.AcceptButton = okButton;
                inputForm.CancelButton = cancelButton;
                
                if (inputForm.ShowDialog() == DialogResult.OK && txtDestinationPath != null)
                {
                    txtDestinationPath.Text = textBox.Text.Trim();
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                try
                {
                    SaveJob();
                    MessageBox.Show(_isEditMode ? "Job updated successfully!" : "Job created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving job: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ServiceLocator.LogService.LogError("Error saving job: " + ex.Message);
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
            if (UIStringExtensions.IsNullOrWhiteSpace(txtJobName.Text))
            {
                MessageBox.Show("Please enter a job name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtJobName.Focus();
                return false;
            }
            if (UIStringExtensions.IsNullOrWhiteSpace(txtSourcePath.Text))
            {
                MessageBox.Show("Please select a source folder.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnBrowseSource.Focus();
                return false;
            }
            if (UIStringExtensions.IsNullOrWhiteSpace(txtDestinationPath.Text))
            {
                MessageBox.Show("Please enter a destination path.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDestinationPath.Focus();
                return false;
            }
            if (numInterval.Value <= 0)
            {
                MessageBox.Show("Please enter a valid interval.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numInterval.Focus();
                return false;
            }
            return true;
        }

        private void SaveJob()
        {
            if (_currentJob == null) _currentJob = new SyncJob();
            _currentJob.Name = txtJobName.Text.Trim();
            _currentJob.IsEnabled = chkEnabled.Checked;
            _currentJob.SourcePath = txtSourcePath.Text.Trim();
            _currentJob.DestinationPath = txtDestinationPath.Text.Trim();
            _currentJob.StartTime = dtpStartTime.Value;
            _currentJob.IntervalValue = (int)numInterval.Value;
            _currentJob.IntervalType = cmbIntervalType.SelectedItem != null ? cmbIntervalType.SelectedItem.ToString() : "Minutes";
            _currentJob.TransferMode = cmbTransferMode.SelectedItem != null ? cmbTransferMode.SelectedItem.ToString() : "Copy (Keep both files)";
            if (_isEditMode)
            {
                _jobService.UpdateJob(_currentJob);
                ServiceLocator.LogService.LogInfo("Job '" + _currentJob.Name + "' updated");
            }
            else
            {
                _currentJob.Id = _jobService.CreateJob(_currentJob);
                ServiceLocator.LogService.LogInfo("Job '" + _currentJob.Name + "' created");
            }
        }

        private void chkEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (gbScheduleSettings != null) gbScheduleSettings.Enabled = chkEnabled.Checked;
        }

        private void cmbIntervalType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbIntervalType.SelectedItem != null)
            {
                string val = cmbIntervalType.SelectedItem.ToString();
                if (val == "Minutes") { numInterval.Minimum = 1; numInterval.Maximum = 1440; }
                else if (val == "Hours") { numInterval.Minimum = 1; numInterval.Maximum = 24; }
                else if (val == "Days") { numInterval.Minimum = 1; numInterval.Maximum = 365; }
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                string scheduleInfo = GenerateSchedulePreview();
                MessageBox.Show(scheduleInfo, "Schedule Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string GenerateSchedulePreview()
        {
            string enabled = chkEnabled.Checked ? "Enabled" : "Disabled";
            string interval = "Every " + numInterval.Value + " " + (cmbIntervalType.SelectedItem != null ? cmbIntervalType.SelectedItem.ToString() : "Minutes");
            string startTime = dtpStartTime.Value.ToString("yyyy-MM-dd HH:mm");
            return "Job Name: " + txtJobName.Text + "\n" +
                   "Status: " + enabled + "\n" +
                   "Source: " + txtSourcePath.Text + "\n" +
                   "Destination: " + txtDestinationPath.Text + "\n" +
                   "Schedule: " + interval + "\n" +
                   "Start Time: " + startTime + "\n" +
                   "Transfer Mode: " + (cmbTransferMode.SelectedItem != null ? cmbTransferMode.SelectedItem.ToString() : "Copy (Keep both files)");
        }
    }
}
