using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
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
        private ITransferClient _currentTransferClient;
        private syncer.core.ConnectionSettings _coreConnectionSettings;

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
            this.Size = new Size(600, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            SetDefaultValues();
            if (_isEditMode) LoadJobSettings();
            InitializeTransferClient();
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
                    
                    // Use the new FileZilla-like file manager
                    try
                    {
                        using (FormRemoteDirectoryBrowser fileManager = new FormRemoteDirectoryBrowser(coreSettings))
                        {
                            fileManager.IsUploadMode = false; // We want to select destination path
                            if (fileManager.ShowDialog() == DialogResult.OK)
                            {
                                if (!string.IsNullOrEmpty(fileManager.SelectedRemotePath))
                                {
                                    if (txtDestinationPath != null) 
                                        txtDestinationPath.Text = fileManager.SelectedRemotePath;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error opening file manager: " + ex.Message, "Error",
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
            
            // Set connection settings for source and destination
            var currentConnection = _connectionService.GetConnectionSettings();
            if (currentConnection != null && currentConnection.IsRemoteConnection)
            {
                // If we have a remote connection configured, use it as source or destination
                // For FTP sender scenario: local source, remote destination
                // For FTP receiver scenario: remote source, local destination
                
                if (_currentJob.DestinationPath.StartsWith("ftp://") || _currentJob.DestinationPath.StartsWith("sftp://") || 
                    (currentConnection.Protocol == "FTP" || currentConnection.Protocol == "SFTP"))
                {
                    // Upload scenario: local to remote
                    _currentJob.SourceConnection = new ConnectionSettings(); // Local
                    _currentJob.DestinationConnection = currentConnection; // Remote
                }
                else
                {
                    // Download scenario: remote to local
                    _currentJob.SourceConnection = currentConnection; // Remote
                    _currentJob.DestinationConnection = new ConnectionSettings(); // Local
                }
            }
            else
            {
                // Local to local transfer
                _currentJob.SourceConnection = new ConnectionSettings(); // Local
                _currentJob.DestinationConnection = new ConnectionSettings(); // Local
            }
            
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

        #region File Manager and Transfer Operations

        private void InitializeTransferClient()
        {
            try
            {
                // Get connection settings and initialize transfer client
                ConnectionSettings uiSettings = _connectionService.GetConnectionSettings();
                if (uiSettings != null)
                {
                    _coreConnectionSettings = ConvertToCore(uiSettings);
                    UpdateTransferClient();
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to initialize transfer client: " + ex.Message);
            }
        }

        private void UpdateTransferClient()
        {
            if (_currentTransferClient != null)
            {
                // ITransferClient doesn't implement IDisposable, no need to dispose
                _currentTransferClient = null;
            }

            if (_coreConnectionSettings == null) return;

            try
            {
                var factory = syncer.core.ServiceFactory.CreateTransferClientFactory();
                _currentTransferClient = factory.Create(_coreConnectionSettings.Protocol);
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to create transfer client: " + ex.Message);
                MessageBox.Show("Failed to create transfer client: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private syncer.core.ConnectionSettings ConvertToCore(ConnectionSettings uiSettings)
        {
            return new syncer.core.ConnectionSettings
            {
                Protocol = uiSettings.Protocol == "SFTP" ? syncer.core.ProtocolType.Sftp :
                          uiSettings.Protocol == "FTP" ? syncer.core.ProtocolType.Ftp :
                          syncer.core.ProtocolType.Local,
                Host = uiSettings.Host,
                Port = uiSettings.Port,
                Username = uiSettings.Username,
                Password = uiSettings.Password,
                SshKeyPath = uiSettings.SshKeyPath,
                Timeout = uiSettings.Timeout
            };
        }

        private void btnUploadFile_Click(object sender, EventArgs e)
        {
            if (!ValidateConnection()) return;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select File(s) to Upload";
                dialog.Filter = "All Files (*.*)|*.*";
                dialog.Multiselect = true;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    UploadFiles(dialog.FileNames);
                }
            }
        }

        private void btnDownloadFile_Click(object sender, EventArgs e)
        {
            if (!ValidateConnection()) return;

            // Open file manager in download mode
            try
            {
                using (FormRemoteDirectoryBrowser fileManager = new FormRemoteDirectoryBrowser(_coreConnectionSettings))
                {
                    fileManager.IsUploadMode = false;
                    fileManager.Text = "Download Files from Remote Server";
                    
                    if (fileManager.ShowDialog() == DialogResult.OK)
                    {
                        if (!string.IsNullOrEmpty(fileManager.SelectedRemotePath))
                        {
                            // Ask user for local download location
                            using (FolderBrowserDialog localDialog = new FolderBrowserDialog())
                            {
                                localDialog.Description = "Select local folder to download files to";
                                localDialog.ShowNewFolderButton = true;
                                
                                if (localDialog.ShowDialog() == DialogResult.OK)
                                {
                                    DownloadFile(fileManager.SelectedRemotePath, localDialog.SelectedPath);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening download dialog: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenFileManager_Click(object sender, EventArgs e)
        {
            if (!ValidateConnection()) return;

            try
            {
                using (FormRemoteDirectoryBrowser fileManager = new FormRemoteDirectoryBrowser(_coreConnectionSettings))
                {
                    fileManager.Text = "FileZilla-like File Manager";
                    fileManager.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening file manager: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateConnection()
        {
            if (_coreConnectionSettings == null)
            {
                MessageBox.Show("No connection settings available. Please configure connection settings first.", 
                    "Connection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_coreConnectionSettings.Protocol == syncer.core.ProtocolType.Local)
            {
                return true; // Local operations don't need validation
            }

            if (!_connectionService.IsConnected())
            {
                MessageBox.Show("Not connected to remote server. Please connect first using Connection Settings.", 
                    "Connection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_currentTransferClient == null)
            {
                UpdateTransferClient();
                if (_currentTransferClient == null)
                {
                    MessageBox.Show("Failed to initialize transfer client.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            return true;
        }

        private void UploadFiles(string[] localFilePaths)
        {
            if (localFilePaths == null || localFilePaths.Length == 0) return;

            // Ask for remote destination folder
            using (Form inputForm = new Form())
            {
                inputForm.Text = "Upload Destination";
                inputForm.Size = new Size(450, 180);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;
                
                Label label = new Label(); 
                label.Left = 10; 
                label.Top = 20; 
                label.Text = "Enter remote destination path:"; 
                label.AutoSize = true;
                
                TextBox textBox = new TextBox(); 
                textBox.Left = 10; 
                textBox.Top = 50; 
                textBox.Width = 400;
                textBox.Text = "/";
                
                Button buttonOk = new Button(); 
                buttonOk.Text = "Upload"; 
                buttonOk.Left = 250; 
                buttonOk.Top = 90; 
                buttonOk.DialogResult = DialogResult.OK;
                
                Button buttonCancel = new Button(); 
                buttonCancel.Text = "Cancel"; 
                buttonCancel.Left = 330; 
                buttonCancel.Top = 90; 
                buttonCancel.DialogResult = DialogResult.Cancel;
                
                inputForm.Controls.Add(label);
                inputForm.Controls.Add(textBox);
                inputForm.Controls.Add(buttonOk);
                inputForm.Controls.Add(buttonCancel);
                inputForm.AcceptButton = buttonOk;
                inputForm.CancelButton = buttonCancel;
                
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    string remotePath = textBox.Text.Trim();
                    if (!string.IsNullOrEmpty(remotePath))
                    {
                        PerformUpload(localFilePaths, remotePath);
                    }
                }
            }
        }

        private void PerformUpload(string[] localFilePaths, string remotePath)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (sender, e) =>
            {
                try
                {
                    for (int i = 0; i < localFilePaths.Length; i++)
                    {
                        string localFile = localFilePaths[i];
                        string fileName = Path.GetFileName(localFile);
                        string remoteFile = remotePath.TrimEnd('/') + "/" + fileName;
                        
                        worker.ReportProgress((i * 100) / localFilePaths.Length, 
                            string.Format("Uploading {0}...", fileName));
                        
                        string error;
                        bool success = _currentTransferClient.UploadFile(_coreConnectionSettings, localFile, remoteFile, true, out error);
                        
                        if (!success)
                        {
                            throw new Exception(string.Format("Failed to upload {0}: {1}", fileName, error));
                        }
                    }
                    
                    worker.ReportProgress(100, "Upload completed successfully!");
                }
                catch (Exception ex)
                {
                    e.Result = ex;
                }
            };
            
            worker.ProgressChanged += (sender, e) =>
            {
                // Could show progress dialog here
                if (e.UserState != null)
                {
                    ServiceLocator.LogService.LogInfo(e.UserState.ToString());
                }
            };
            
            worker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Result is Exception)
                {
                    Exception ex = (Exception)e.Result;
                    MessageBox.Show("Upload failed: " + ex.Message, "Upload Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ServiceLocator.LogService.LogError("Upload failed: " + ex.Message);
                }
                else
                {
                    MessageBox.Show("Files uploaded successfully!", "Upload Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ServiceLocator.LogService.LogInfo("Upload completed successfully");
                }
            };
            
            worker.RunWorkerAsync();
        }

        private void DownloadFile(string remotePath, string localFolderPath)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (sender, e) =>
            {
                try
                {
                    string fileName = Path.GetFileName(remotePath);
                    string localFilePath = Path.Combine(localFolderPath, fileName);
                    
                    worker.ReportProgress(50, string.Format("Downloading {0}...", fileName));
                    
                    string error;
                    bool success = _currentTransferClient.DownloadFile(_coreConnectionSettings, remotePath, localFilePath, true, out error);
                    
                    if (!success)
                    {
                        throw new Exception(string.Format("Failed to download {0}: {1}", fileName, error));
                    }
                    
                    worker.ReportProgress(100, "Download completed successfully!");
                }
                catch (Exception ex)
                {
                    e.Result = ex;
                }
            };
            
            worker.ProgressChanged += (sender, e) =>
            {
                if (e.UserState != null)
                {
                    ServiceLocator.LogService.LogInfo(e.UserState.ToString());
                }
            };
            
            worker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Result is Exception)
                {
                    Exception ex = (Exception)e.Result;
                    MessageBox.Show("Download failed: " + ex.Message, "Download Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ServiceLocator.LogService.LogError("Download failed: " + ex.Message);
                }
                else
                {
                    MessageBox.Show("File downloaded successfully!", "Download Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ServiceLocator.LogService.LogInfo("Download completed successfully");
                }
            };
            
            worker.RunWorkerAsync();
        }

        #endregion
    }
}
