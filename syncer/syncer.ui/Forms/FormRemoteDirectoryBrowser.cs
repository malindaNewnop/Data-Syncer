using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using syncer.core;
using syncer.core.Configuration;
using syncer.core.Transfers;
using syncer.ui.Services;

namespace syncer.ui.Forms
{
    public partial class FormRemoteDirectoryBrowser : Form
    {
        private IConnectionService _connectionService;
        private ITransferClient _transferClient;
        private syncer.core.ConnectionSettings _connectionSettings;
        
        // UI Controls
        private ListView _lstLocal;
        private ListView _lstRemote;
        private TextBox _txtLocalPath;
        private TextBox _txtRemotePath;
        private ProgressBar _progressBar;
        private Label _lblTransferStatus;
        private Label _lblQueue;
        private Button _btnUpload;
        private Button _btnDownload;
        
        // Current directory tracking
        private string _currentLocalPath = @"C:\";
        private string _currentRemotePath = "/";
        
        // Selected paths for return
        public string SelectedLocalPath { get; private set; }
        public string SelectedRemotePath { get; private set; }
        public bool IsUploadMode { get; set; } = true; // true for upload, false for download
        
        // Progress tracking
        private BackgroundWorker _transferWorker;
        private List<TransferItem> _transferQueue = new List<TransferItem>();
        
        public FormRemoteDirectoryBrowser(syncer.core.ConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
            InitializeComponent();
            InitializeServices();
            InitializeFileManager();
        }
        
        private void InitializeServices()
        {
            try
            {
                _connectionService = ServiceLocator.ConnectionService;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing services: " + ex.Message, "Initialization Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void InitializeTransferClient()
        {
            try
            {
                if (_connectionSettings == null) return;
                
                switch (_connectionSettings.Protocol)
                {
                    case syncer.core.ProtocolType.Local:
                        _transferClient = new LocalTransferClient();
                        break;
                    case syncer.core.ProtocolType.Ftp:
                        _transferClient = new EnhancedFtpTransferClient();
                        break;
                    case syncer.core.ProtocolType.Sftp:
                        _transferClient = new ProductionSftpTransferClient();
                        break;
                }
                
                if (_transferClient != null && _connectionSettings.Protocol != syncer.core.ProtocolType.Local)
                {
                    string error;
                    if (!_transferClient.TestConnection(_connectionSettings, out error))
                    {
                        MessageBox.Show("Connection test failed: " + error, "Connection Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to server: " + ex.Message, "Connection Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void InitializeFileManager()
        {
            this.Text = "File Manager - DataSyncer";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            
            CreateFileManagerLayout();
            InitializeBackgroundWorker();
            InitializeTransferClient();
            RefreshBothPanes();
        }
        
        private void CreateFileManagerLayout()
        {
            this.SuspendLayout();
            
            // Clear existing controls
            this.Controls.Clear();
            
            // Main container panel
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(5);
            this.Controls.Add(mainPanel);
            
            // Toolbar
            CreateToolbar(mainPanel);
            
            // Splitter container for dual panes
            SplitContainer splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Orientation = Orientation.Vertical;
            splitContainer.SplitterDistance = 480;
            splitContainer.FixedPanel = FixedPanel.None;
            splitContainer.BorderStyle = BorderStyle.FixedSingle;
            mainPanel.Controls.Add(splitContainer);
            splitContainer.BringToFront();
            
            // Left pane - Local files
            CreateLocalPane(splitContainer.Panel1);
            
            // Right pane - Remote files  
            CreateRemotePane(splitContainer.Panel2);
            
            // Bottom status panel
            CreateStatusPanel(mainPanel);
            
            // OK/Cancel buttons
            CreateActionButtons(mainPanel);
            
            this.ResumeLayout();
        }
        
        private void CreateToolbar(Panel parent)
        {
            Panel toolbarPanel = new Panel();
            toolbarPanel.Height = 35;
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.BorderStyle = BorderStyle.FixedSingle;
            toolbarPanel.BackColor = SystemColors.Control;
            parent.Controls.Add(toolbarPanel);
            
            _btnUpload = new Button();
            _btnUpload.Text = "Upload →";
            _btnUpload.Size = new Size(80, 25);
            _btnUpload.Location = new Point(5, 5);
            _btnUpload.UseVisualStyleBackColor = true;
            _btnUpload.Click += BtnUpload_Click;
            toolbarPanel.Controls.Add(_btnUpload);
            
            _btnDownload = new Button();
            _btnDownload.Text = "← Download";
            _btnDownload.Size = new Size(80, 25);
            _btnDownload.Location = new Point(90, 5);
            _btnDownload.UseVisualStyleBackColor = true;
            _btnDownload.Click += BtnDownload_Click;
            toolbarPanel.Controls.Add(_btnDownload);
            
            Button btnRefresh = new Button();
            btnRefresh.Text = "Refresh";
            btnRefresh.Size = new Size(60, 25);
            btnRefresh.Location = new Point(175, 5);
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += delegate { RefreshBothPanes(); };
            toolbarPanel.Controls.Add(btnRefresh);
            
            Button btnNewFolder = new Button();
            btnNewFolder.Text = "New Folder";
            btnNewFolder.Size = new Size(75, 25);
            btnNewFolder.Location = new Point(240, 5);
            btnNewFolder.UseVisualStyleBackColor = true;
            btnNewFolder.Click += BtnNewFolder_Click;
            toolbarPanel.Controls.Add(btnNewFolder);
            
            Button btnDelete = new Button();
            btnDelete.Text = "Delete";
            btnDelete.Size = new Size(60, 25);
            btnDelete.Location = new Point(320, 5);
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += BtnDelete_Click;
            toolbarPanel.Controls.Add(btnDelete);
            
            // Connection status
            Label lblStatus = new Label();
            lblStatus.Text = GetConnectionStatusText();
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(400, 10);
            lblStatus.ForeColor = _transferClient != null ? Color.Green : Color.Red;
            toolbarPanel.Controls.Add(lblStatus);
        }
        
        private void CreateLocalPane(Panel parent)
        {
            GroupBox localGroup = new GroupBox();
            localGroup.Text = "Local Files";
            localGroup.Dock = DockStyle.Fill;
            localGroup.Padding = new Padding(5);
            parent.Controls.Add(localGroup);
            
            // Local path textbox
            _txtLocalPath = new TextBox();
            _txtLocalPath.Dock = DockStyle.Top;
            _txtLocalPath.Text = _currentLocalPath;
            _txtLocalPath.ReadOnly = true;
            _txtLocalPath.BackColor = SystemColors.Control;
            localGroup.Controls.Add(_txtLocalPath);
            
            // Local navigation buttons
            Panel localNavPanel = new Panel();
            localNavPanel.Height = 30;
            localNavPanel.Dock = DockStyle.Top;
            localGroup.Controls.Add(localNavPanel);
            localNavPanel.BringToFront();
            
            Button btnLocalUp = new Button();
            btnLocalUp.Text = "↑ Up";
            btnLocalUp.Size = new Size(50, 25);
            btnLocalUp.Location = new Point(5, 2);
            btnLocalUp.UseVisualStyleBackColor = true;
            btnLocalUp.Click += delegate { NavigateLocalUp(); };
            localNavPanel.Controls.Add(btnLocalUp);
            
            Button btnLocalHome = new Button();
            btnLocalHome.Text = "Home";
            btnLocalHome.Size = new Size(50, 25);
            btnLocalHome.Location = new Point(60, 2);
            btnLocalHome.UseVisualStyleBackColor = true;
            btnLocalHome.Click += delegate { NavigateLocalHome(); };
            localNavPanel.Controls.Add(btnLocalHome);
            
            // Local files listview
            _lstLocal = new ListView();
            _lstLocal.Dock = DockStyle.Fill;
            _lstLocal.View = View.Details;
            _lstLocal.FullRowSelect = true;
            _lstLocal.GridLines = true;
            _lstLocal.AllowDrop = true;
            _lstLocal.MultiSelect = true;
            _lstLocal.HideSelection = false;
            
            _lstLocal.Columns.Add("Name", 200);
            _lstLocal.Columns.Add("Type", 80);
            _lstLocal.Columns.Add("Size", 100);
            _lstLocal.Columns.Add("Modified", 120);
            
            _lstLocal.DoubleClick += LstLocal_DoubleClick;
            _lstLocal.KeyDown += LstLocal_KeyDown;
            
            localGroup.Controls.Add(_lstLocal);
        }
        
        private void CreateRemotePane(Panel parent)
        {
            GroupBox remoteGroup = new GroupBox();
            remoteGroup.Text = string.Format("Remote Files ({0})", _connectionSettings.Protocol);
            remoteGroup.Dock = DockStyle.Fill;
            remoteGroup.Padding = new Padding(5);
            parent.Controls.Add(remoteGroup);
            
            // Remote path textbox
            _txtRemotePath = new TextBox();
            _txtRemotePath.Dock = DockStyle.Top;
            _txtRemotePath.Text = _currentRemotePath;
            _txtRemotePath.ReadOnly = true;
            _txtRemotePath.BackColor = SystemColors.Control;
            remoteGroup.Controls.Add(_txtRemotePath);
            
            // Remote navigation buttons
            Panel remoteNavPanel = new Panel();
            remoteNavPanel.Height = 30;
            remoteNavPanel.Dock = DockStyle.Top;
            remoteGroup.Controls.Add(remoteNavPanel);
            remoteNavPanel.BringToFront();
            
            Button btnRemoteUp = new Button();
            btnRemoteUp.Text = "↑ Up";
            btnRemoteUp.Size = new Size(50, 25);
            btnRemoteUp.Location = new Point(5, 2);
            btnRemoteUp.UseVisualStyleBackColor = true;
            btnRemoteUp.Click += delegate { NavigateRemoteUp(); };
            remoteNavPanel.Controls.Add(btnRemoteUp);
            
            Button btnRemoteHome = new Button();
            btnRemoteHome.Text = "Home";
            btnRemoteHome.Size = new Size(50, 25);
            btnRemoteHome.Location = new Point(60, 2);
            btnRemoteHome.UseVisualStyleBackColor = true;
            btnRemoteHome.Click += delegate { NavigateRemoteHome(); };
            remoteNavPanel.Controls.Add(btnRemoteHome);
            
            // Remote files listview
            _lstRemote = new ListView();
            _lstRemote.Dock = DockStyle.Fill;
            _lstRemote.View = View.Details;
            _lstRemote.FullRowSelect = true;
            _lstRemote.GridLines = true;
            _lstRemote.AllowDrop = true;
            _lstRemote.MultiSelect = true;
            _lstRemote.HideSelection = false;
            
            _lstRemote.Columns.Add("Name", 200);
            _lstRemote.Columns.Add("Type", 80);
            _lstRemote.Columns.Add("Size", 100);
            _lstRemote.Columns.Add("Modified", 120);
            
            _lstRemote.DoubleClick += LstRemote_DoubleClick;
            _lstRemote.KeyDown += LstRemote_KeyDown;
            
            remoteGroup.Controls.Add(_lstRemote);
        }
        
        private void CreateStatusPanel(Panel parent)
        {
            Panel statusPanel = new Panel();
            statusPanel.Height = 80;
            statusPanel.Dock = DockStyle.Bottom;
            statusPanel.BorderStyle = BorderStyle.FixedSingle;
            statusPanel.BackColor = SystemColors.Control;
            parent.Controls.Add(statusPanel);
            
            // Progress bar
            _progressBar = new ProgressBar();
            _progressBar.Location = new Point(5, 5);
            _progressBar.Size = new Size(parent.Width - 15, 20);
            _progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _progressBar.Visible = false;
            statusPanel.Controls.Add(_progressBar);
            
            // Status label
            _lblTransferStatus = new Label();
            _lblTransferStatus.Location = new Point(5, 30);
            _lblTransferStatus.Size = new Size(parent.Width - 10, 20);
            _lblTransferStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _lblTransferStatus.Text = "Ready";
            statusPanel.Controls.Add(_lblTransferStatus);
            
            // Transfer queue info
            _lblQueue = new Label();
            _lblQueue.Location = new Point(5, 50);
            _lblQueue.Size = new Size(parent.Width - 10, 20);
            _lblQueue.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _lblQueue.Text = "Queue: 0 items";
            statusPanel.Controls.Add(_lblQueue);
        }
        
        private void CreateActionButtons(Panel parent)
        {
            Panel buttonPanel = new Panel();
            buttonPanel.Height = 40;
            buttonPanel.Dock = DockStyle.Bottom;
            parent.Controls.Add(buttonPanel);
            
            Button btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Size = new Size(75, 25);
            btnOK.Location = new Point(parent.Width - 160, 8);
            btnOK.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnOK.DialogResult = DialogResult.OK;
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            buttonPanel.Controls.Add(btnOK);
            
            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Size = new Size(75, 25);
            btnCancel.Location = new Point(parent.Width - 80, 8);
            btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.UseVisualStyleBackColor = true;
            buttonPanel.Controls.Add(btnCancel);
            
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }
        
        private void InitializeBackgroundWorker()
        {
            _transferWorker = new BackgroundWorker();
            _transferWorker.WorkerReportsProgress = true;
            _transferWorker.WorkerSupportsCancellation = true;
            _transferWorker.DoWork += TransferWorker_DoWork;
            _transferWorker.ProgressChanged += TransferWorker_ProgressChanged;
            _transferWorker.RunWorkerCompleted += TransferWorker_RunWorkerCompleted;
        }
        
        private string GetConnectionStatusText()
        {
            if (_connectionSettings == null)
                return "No connection configured";
                
            if (_connectionSettings.Protocol == syncer.core.ProtocolType.Local)
                return "Local mode";
                
            string status = _transferClient != null ? "Connected" : "Disconnected";
            return string.Format("{0} to {1}:{2} ({3})", 
                status, _connectionSettings.Host, _connectionSettings.Port, _connectionSettings.Protocol);
        }
        
        // Event handlers for UI interactions
        private void BtnUpload_Click(object sender, EventArgs e)
        {
            PerformTransfer(true); // Upload from local to remote
        }
        
        private void BtnDownload_Click(object sender, EventArgs e)
        {
            PerformTransfer(false); // Download from remote to local
        }
        
        private void BtnNewFolder_Click(object sender, EventArgs e)
        {
            CreateNewFolder();
        }
        
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedItems();
        }
        
        private void BtnOK_Click(object sender, EventArgs e)
        {
            // For download mode, we need to select a specific file, not just the directory
            if (!IsUploadMode)
            {
                if (_lstRemote.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = _lstRemote.SelectedItems[0];
                    if (selectedItem.Text != ".." && selectedItem.SubItems[1].Text != "Folder")
                    {
                        // Selected item is a file
                        string selectedFileName = selectedItem.Text;
                        SelectedRemotePath = _currentRemotePath.TrimEnd('/') + "/" + selectedFileName;
                        SelectedLocalPath = _currentLocalPath;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Please select a file to download, not a folder.", "Invalid Selection",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Please select a file to download.", "No Selection",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                // Upload mode - just return the current directory
                SelectedLocalPath = _currentLocalPath;
                SelectedRemotePath = _currentRemotePath;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        
        private void LstLocal_DoubleClick(object sender, EventArgs e)
        {
            if (_lstLocal.SelectedItems.Count > 0)
            {
                ListViewItem item = _lstLocal.SelectedItems[0];
                if (item.Text == "..")
                {
                    NavigateLocalUp();
                }
                else if (item.SubItems[1].Text == "Folder")
                {
                    string newPath = Path.Combine(_currentLocalPath, item.Text);
                    NavigateLocalTo(newPath);
                }
            }
        }
        
        private void LstRemote_DoubleClick(object sender, EventArgs e)
        {
            if (_lstRemote.SelectedItems.Count > 0)
            {
                ListViewItem item = _lstRemote.SelectedItems[0];
                if (item.Text == "..")
                {
                    NavigateRemoteUp();
                }
                else if (item.SubItems[1].Text == "Folder")
                {
                    string newPath = CombineRemotePath(_currentRemotePath, item.Text);
                    NavigateRemoteTo(newPath);
                }
            }
        }
        
        private void LstLocal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedLocalItems();
            }
            else if (e.KeyCode == Keys.F5)
            {
                RefreshLocalPane();
            }
        }
        
        private void LstRemote_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedRemoteItems();
            }
            else if (e.KeyCode == Keys.F5)
            {
                RefreshRemotePane();
            }
        }
        
        // Navigation methods
        private void NavigateLocalUp()
        {
            if (_currentLocalPath.Length > 3) // Not at root like C:\
            {
                string parentPath = Directory.GetParent(_currentLocalPath).FullName;
                NavigateLocalTo(parentPath);
            }
        }
        
        private void NavigateLocalHome()
        {
            // Use MyDocuments instead of UserProfile for .NET 3.5 compatibility
            NavigateLocalTo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }
        
        private void NavigateLocalTo(string path)
        {
            if (Directory.Exists(path))
            {
                _currentLocalPath = path;
                _txtLocalPath.Text = path;
                RefreshLocalPane();
            }
        }
        
        private void NavigateRemoteUp()
        {
            if (_currentRemotePath != "/")
            {
                int lastSlash = _currentRemotePath.TrimEnd('/').LastIndexOf('/');
                if (lastSlash >= 0)
                {
                    string parentPath = lastSlash == 0 ? "/" : _currentRemotePath.Substring(0, lastSlash);
                    NavigateRemoteTo(parentPath);
                }
            }
        }
        
        private void NavigateRemoteHome()
        {
            NavigateRemoteTo("/");
        }
        
        private void NavigateRemoteTo(string path)
        {
            _currentRemotePath = path;
            _txtRemotePath.Text = path;
            RefreshRemotePane();
        }
        
        private string CombineRemotePath(string basePath, string relativePath)
        {
            if (basePath.EndsWith("/"))
                return basePath + relativePath;
            else
                return basePath + "/" + relativePath;
        }
        
        // Refresh methods
        private void RefreshBothPanes()
        {
            RefreshLocalPane();
            RefreshRemotePane();
        }
        
        private void RefreshLocalPane()
        {
            try
            {
                _lstLocal.Items.Clear();
                
                // Add parent directory entry
                if (_currentLocalPath.Length > 3)
                {
                    ListViewItem parentItem = new ListViewItem("..");
                    parentItem.SubItems.Add("Folder");
                    parentItem.SubItems.Add("");
                    parentItem.SubItems.Add("");
                    parentItem.ImageIndex = 0;
                    _lstLocal.Items.Add(parentItem);
                }
                
                // Add directories
                string[] directories = Directory.GetDirectories(_currentLocalPath);
                foreach (string dir in directories)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);
                    ListViewItem item = new ListViewItem(dirInfo.Name);
                    item.SubItems.Add("Folder");
                    item.SubItems.Add("");
                    item.SubItems.Add(dirInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
                    item.ImageIndex = 0;
                    item.Tag = dir;
                    _lstLocal.Items.Add(item);
                }
                
                // Add files
                string[] files = Directory.GetFiles(_currentLocalPath);
                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    ListViewItem item = new ListViewItem(fileInfo.Name);
                    item.SubItems.Add("File");
                    item.SubItems.Add(FormatFileSize(fileInfo.Length));
                    item.SubItems.Add(fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
                    item.ImageIndex = 1;
                    item.Tag = file;
                    _lstLocal.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing local directory: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void RefreshRemotePane()
        {
            try
            {
                if (_transferClient == null) return;
                
                _lstRemote.Items.Clear();
                
                // Add parent directory entry
                if (_currentRemotePath != "/")
                {
                    ListViewItem parentItem = new ListViewItem("..");
                    parentItem.SubItems.Add("Folder");
                    parentItem.SubItems.Add("");
                    parentItem.SubItems.Add("");
                    parentItem.ImageIndex = 0;
                    _lstRemote.Items.Add(parentItem);
                }
                
                // Get remote directory listing using actual ITransferClient interface
                List<string> files;
                string error;
                if (_transferClient.ListFiles(_connectionSettings, _currentRemotePath, out files, out error))
                {
                    foreach (string file in files)
                    {
                        ListViewItem item = new ListViewItem(Path.GetFileName(file));
                        item.SubItems.Add("File");
                        item.SubItems.Add(""); // Size not available with current interface
                        item.SubItems.Add(""); // Modified date not available with current interface
                        item.ImageIndex = 1;
                        item.Tag = file;
                        _lstRemote.Items.Add(item);
                    }
                }
                else if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show("Error listing remote files: " + error, "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing remote directory: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // File operations
        private void PerformTransfer(bool isUpload)
        {
            try
            {
                _transferQueue.Clear();
                
                if (isUpload)
                {
                    // Upload selected local items
                    foreach (ListViewItem item in _lstLocal.SelectedItems)
                    {
                        if (item.Text == "..") continue;
                        
                        string localPath = (string)item.Tag;
                        string fileName = Path.GetFileName(localPath);
                        string remotePath = CombineRemotePath(_currentRemotePath, fileName);
                        
                        TransferItem transferItem = new TransferItem();
                        transferItem.SourcePath = localPath;
                        transferItem.DestinationPath = remotePath;
                        transferItem.IsUpload = true;
                        transferItem.IsDirectory = item.SubItems[1].Text == "Folder";
                        transferItem.Name = fileName;
                        
                        if (!transferItem.IsDirectory)
                        {
                            FileInfo fileInfo = new FileInfo(localPath);
                            transferItem.Size = fileInfo.Length;
                        }
                        
                        _transferQueue.Add(transferItem);
                    }
                }
                else
                {
                    // Download selected remote items
                    foreach (ListViewItem item in _lstRemote.SelectedItems)
                    {
                        if (item.Text == "..") continue;
                        
                        string remoteFile = (string)item.Tag;
                        string localPath = Path.Combine(_currentLocalPath, Path.GetFileName(remoteFile));
                        
                        TransferItem transferItem = new TransferItem();
                        transferItem.SourcePath = remoteFile;
                        transferItem.DestinationPath = localPath;
                        transferItem.IsUpload = false;
                        transferItem.IsDirectory = false; // Current interface doesn't distinguish directories
                        transferItem.Size = 0;
                        transferItem.Name = Path.GetFileName(remoteFile);
                        
                        _transferQueue.Add(transferItem);
                    }
                }
                
                if (_transferQueue.Count > 0)
                {
                    StartTransfer();
                }
                else
                {
                    MessageBox.Show("Please select files to transfer.", "No Selection", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error preparing transfer: " + ex.Message, "Transfer Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void StartTransfer()
        {
            if (_transferWorker.IsBusy) return;
            
            _progressBar.Visible = true;
            _progressBar.Value = 0;
            _progressBar.Maximum = _transferQueue.Count;
            _lblTransferStatus.Text = "Starting transfer...";
            _lblQueue.Text = string.Format("Queue: {0} items", _transferQueue.Count);
            
            _btnUpload.Enabled = false;
            _btnDownload.Enabled = false;
            
            _transferWorker.RunWorkerAsync();
        }
        
        private void CreateNewFolder()
        {
            string folderName = ShowInputDialog("Enter folder name:", "Create New Folder", "New Folder");
                
            if (!string.IsNullOrEmpty(folderName))
            {
                try
                {
                    // Determine where to create the folder based on focus
                    if (_lstLocal.Focused)
                    {
                        string newPath = Path.Combine(_currentLocalPath, folderName);
                        Directory.CreateDirectory(newPath);
                        RefreshLocalPane();
                    }
                    else if (_lstRemote.Focused && _transferClient != null)
                    {
                        string newPath = CombineRemotePath(_currentRemotePath, folderName);
                        string error;
                        if (_transferClient.EnsureDirectory(_connectionSettings, newPath, out error))
                        {
                            RefreshRemotePane();
                        }
                        else
                        {
                            MessageBox.Show("Error creating remote folder: " + error, "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error creating folder: " + ex.Message, "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private string ShowInputDialog(string text, string caption, string defaultValue)
        {
            Form prompt = new Form();
            prompt.Width = 400;
            prompt.Height = 150;
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.Text = caption;
            prompt.StartPosition = FormStartPosition.CenterScreen;
            prompt.MaximizeBox = false;
            prompt.MinimizeBox = false;
            
            Label textLabel = new Label();
            textLabel.Left = 20;
            textLabel.Top = 20;
            textLabel.Width = 350;
            textLabel.Text = text;
            
            TextBox textBox = new TextBox();
            textBox.Left = 20;
            textBox.Top = 50;
            textBox.Width = 350;
            textBox.Text = defaultValue;
            
            Button confirmation = new Button();
            confirmation.Text = "Ok";
            confirmation.Left = 200;
            confirmation.Width = 80;
            confirmation.Top = 80;
            confirmation.DialogResult = DialogResult.OK;
            
            Button cancel = new Button();
            cancel.Text = "Cancel";
            cancel.Left = 290;
            cancel.Width = 80;
            cancel.Top = 80;
            cancel.DialogResult = DialogResult.Cancel;
            
            confirmation.Click += delegate { prompt.Close(); };
            cancel.Click += delegate { prompt.Close(); };
            
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;
            
            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
        
        private void DeleteSelectedItems()
        {
            if (_lstLocal.Focused)
                DeleteSelectedLocalItems();
            else if (_lstRemote.Focused)
                DeleteSelectedRemoteItems();
        }
        
        private void DeleteSelectedLocalItems()
        {
            if (_lstLocal.SelectedItems.Count == 0) return;
            
            string message = string.Format("Are you sure you want to delete {0} selected item(s)?", 
                _lstLocal.SelectedItems.Count);
            
            if (MessageBox.Show(message, "Confirm Delete", MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    foreach (ListViewItem item in _lstLocal.SelectedItems)
                    {
                        if (item.Text == "..") continue;
                        
                        string path = (string)item.Tag;
                        if (item.SubItems[1].Text == "Folder")
                        {
                            Directory.Delete(path, true);
                        }
                        else
                        {
                            File.Delete(path);
                        }
                    }
                    RefreshLocalPane();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting items: " + ex.Message, "Delete Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private void DeleteSelectedRemoteItems()
        {
            if (_lstRemote.SelectedItems.Count == 0 || _transferClient == null) return;
            
            string message = string.Format("Are you sure you want to delete {0} selected item(s)?", 
                _lstRemote.SelectedItems.Count);
            
            if (MessageBox.Show(message, "Confirm Delete", MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    foreach (ListViewItem item in _lstRemote.SelectedItems)
                    {
                        if (item.Text == "..") continue;
                        
                        string remoteFile = (string)item.Tag;
                        string error;
                        
                        if (!_transferClient.DeleteFile(_connectionSettings, remoteFile, out error))
                        {
                            MessageBox.Show("Error deleting " + remoteFile + ": " + error, "Delete Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    RefreshRemotePane();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting remote items: " + ex.Message, "Delete Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        // Background worker events
        private void TransferWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            
            for (int i = 0; i < _transferQueue.Count; i++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                
                TransferItem item = _transferQueue[i];
                
                try
                {
                    worker.ReportProgress(i, string.Format("Transferring: {0}", item.Name));
                    
                    string error;
                    bool success;
                    
                    if (item.IsUpload)
                    {
                        // Upload file
                        success = _transferClient.UploadFile(_connectionSettings, item.SourcePath, 
                            item.DestinationPath, true, out error);
                    }
                    else
                    {
                        // Download file
                        success = _transferClient.DownloadFile(_connectionSettings, item.SourcePath, 
                            item.DestinationPath, true, out error);
                    }
                    
                    if (!success && !string.IsNullOrEmpty(error))
                    {
                        worker.ReportProgress(i, string.Format("Error transferring {0}: {1}", item.Name, error));
                    }
                }
                catch (Exception ex)
                {
                    worker.ReportProgress(i, string.Format("Error transferring {0}: {1}", item.Name, ex.Message));
                }
            }
        }
        
        private void TransferWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _progressBar.Value = e.ProgressPercentage;
            _lblTransferStatus.Text = (string)e.UserState;
            _lblQueue.Text = string.Format("Queue: {0}/{1} completed", 
                e.ProgressPercentage + 1, _transferQueue.Count);
        }
        
        private void TransferWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _progressBar.Visible = false;
            _btnUpload.Enabled = true;
            _btnDownload.Enabled = true;
            
            if (e.Cancelled)
            {
                _lblTransferStatus.Text = "Transfer cancelled";
            }
            else if (e.Error != null)
            {
                _lblTransferStatus.Text = "Transfer completed with errors";
                MessageBox.Show("Transfer error: " + e.Error.Message, "Transfer Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                _lblTransferStatus.Text = "Transfer completed successfully";
                RefreshBothPanes();
            }
            
            _lblQueue.Text = "Queue: 0 items";
        }
        
        // Utility methods
        private string FormatFileSize(long bytes)
        {
            if (bytes >= 1073741824)
                return string.Format("{0:F1} GB", (double)bytes / 1073741824);
            else if (bytes >= 1048576)
                return string.Format("{0:F1} MB", (double)bytes / 1048576);
            else if (bytes >= 1024)
                return string.Format("{0:F1} KB", (double)bytes / 1024);
            else
                return string.Format("{0} bytes", bytes);
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_transferWorker != null && _transferWorker.IsBusy)
            {
                _transferWorker.CancelAsync();
            }
            
            base.OnFormClosing(e);
        }
    }
    
    // Helper class for transfer operations
    public class TransferItem
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public bool IsUpload { get; set; }
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public string Name { get; set; }
    }
}
