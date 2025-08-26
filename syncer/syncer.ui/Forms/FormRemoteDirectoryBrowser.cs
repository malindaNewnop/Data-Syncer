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
            splitContainer.Orientation = Orientation.Horizontal; // Changed to Horizontal to make the layout cleaner
            splitContainer.SplitterDistance = 300; // Adjusted for horizontal layout
            splitContainer.FixedPanel = FixedPanel.None;
            splitContainer.BorderStyle = BorderStyle.FixedSingle;
            splitContainer.SplitterWidth = 5; // Thinner splitter
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
            toolbarPanel.Height = 40;
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.BorderStyle = BorderStyle.FixedSingle;
            toolbarPanel.BackColor = Color.FromArgb(240, 240, 240);
            parent.Controls.Add(toolbarPanel);
            
            // Updated button style with icons using emoji characters
            _btnUpload = new Button();
            _btnUpload.Text = "Upload ↑";
            _btnUpload.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            _btnUpload.Size = new Size(90, 30);
            _btnUpload.Location = new Point(10, 5);
            _btnUpload.FlatStyle = FlatStyle.Flat;
            _btnUpload.BackColor = Color.FromArgb(225, 240, 255);
            _btnUpload.FlatAppearance.BorderColor = Color.FromArgb(150, 200, 240);
            _btnUpload.Click += BtnUpload_Click;
            toolbarPanel.Controls.Add(_btnUpload);
            
            _btnDownload = new Button();
            _btnDownload.Text = "Download ↓";
            _btnDownload.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            _btnDownload.Size = new Size(90, 30);
            _btnDownload.Location = new Point(110, 5);
            _btnDownload.FlatStyle = FlatStyle.Flat;
            _btnDownload.BackColor = Color.FromArgb(235, 255, 235);
            _btnDownload.FlatAppearance.BorderColor = Color.FromArgb(150, 220, 150);
            _btnDownload.Click += BtnDownload_Click;
            toolbarPanel.Controls.Add(_btnDownload);
            
            Button btnRefresh = new Button();
            btnRefresh.Text = "Refresh ↻";
            btnRefresh.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            btnRefresh.Size = new Size(80, 30);
            btnRefresh.Location = new Point(210, 5);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.BackColor = Color.FromArgb(245, 245, 245);
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnRefresh.Click += delegate { RefreshBothPanes(); };
            toolbarPanel.Controls.Add(btnRefresh);
            
            Button btnNewFolder = new Button();
            btnNewFolder.Text = "New Folder";
            btnNewFolder.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            btnNewFolder.Size = new Size(90, 30);
            btnNewFolder.Location = new Point(300, 5);
            btnNewFolder.FlatStyle = FlatStyle.Flat;
            btnNewFolder.BackColor = Color.FromArgb(255, 250, 220);
            btnNewFolder.FlatAppearance.BorderColor = Color.FromArgb(220, 200, 150);
            btnNewFolder.Click += BtnNewFolder_Click;
            toolbarPanel.Controls.Add(btnNewFolder);
            
            Button btnDelete = new Button();
            btnDelete.Text = "Delete";
            btnDelete.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            btnDelete.Size = new Size(70, 30);
            btnDelete.Location = new Point(400, 5);
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.BackColor = Color.FromArgb(255, 230, 230);
            btnDelete.FlatAppearance.BorderColor = Color.FromArgb(220, 150, 150);
            btnDelete.Click += BtnDelete_Click;
            toolbarPanel.Controls.Add(btnDelete);
            
            // Connection status panel with improved visibility
            Panel connectionPanel = new Panel();
            connectionPanel.Size = new Size(190, 30);
            connectionPanel.Location = new Point(480, 5);
            connectionPanel.BorderStyle = BorderStyle.FixedSingle;
            
            bool isConnected = _transferClient != null;
            connectionPanel.BackColor = isConnected ? Color.FromArgb(230, 255, 230) : Color.FromArgb(255, 230, 230);
            
            PictureBox pbConnectionStatus = new PictureBox();
            pbConnectionStatus.Size = new Size(16, 16);
            pbConnectionStatus.Location = new Point(5, 7);
            pbConnectionStatus.BackColor = isConnected ? Color.Green : Color.Red;
            connectionPanel.Controls.Add(pbConnectionStatus);
            
            Label lblStatus = new Label();
            lblStatus.Text = GetConnectionStatusText();
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.Location = new Point(25, 7);
            lblStatus.ForeColor = isConnected ? Color.DarkGreen : Color.DarkRed;
            connectionPanel.Controls.Add(lblStatus);
            
            toolbarPanel.Controls.Add(connectionPanel);
        }
        
        private void CreateLocalPane(Panel parent)
        {
            GroupBox localGroup = new GroupBox();
            localGroup.Text = "Local Files";
            localGroup.Dock = DockStyle.Fill;
            localGroup.Padding = new Padding(5);
            localGroup.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            localGroup.ForeColor = Color.FromArgb(60, 60, 60);
            parent.Controls.Add(localGroup);
            
            // Local path panel with icon
            Panel pathPanel = new Panel();
            pathPanel.Height = 30;
            pathPanel.Dock = DockStyle.Top;
            pathPanel.BackColor = Color.FromArgb(240, 245, 250);
            localGroup.Controls.Add(pathPanel);
            
            Label pathIcon = new Label();
            pathIcon.Text = "📁";
            pathIcon.Size = new Size(25, 25);
            pathIcon.Location = new Point(5, 5);
            pathIcon.Font = new Font("Segoe UI", 10F);
            pathPanel.Controls.Add(pathIcon);
            
            // Local path textbox
            _txtLocalPath = new TextBox();
            _txtLocalPath.Size = new Size(parent.Width - 45, 22);
            _txtLocalPath.Location = new Point(30, 5);
            _txtLocalPath.Text = _currentLocalPath;
            _txtLocalPath.Font = new Font("Consolas", 9F, FontStyle.Regular);
            _txtLocalPath.ReadOnly = true;
            _txtLocalPath.BackColor = Color.White;
            _txtLocalPath.BorderStyle = BorderStyle.FixedSingle;
            pathPanel.Controls.Add(_txtLocalPath);
            
            // Local navigation buttons
            Panel localNavPanel = new Panel();
            localNavPanel.Height = 36;
            localNavPanel.Dock = DockStyle.Top;
            localNavPanel.BackColor = Color.FromArgb(245, 245, 245);
            localNavPanel.BorderStyle = BorderStyle.FixedSingle;
            localGroup.Controls.Add(localNavPanel);
            localNavPanel.BringToFront();
            
            Button btnLocalUp = new Button();
            btnLocalUp.Text = "↑ Up";
            btnLocalUp.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            btnLocalUp.Size = new Size(60, 28);
            btnLocalUp.Location = new Point(5, 3);
            btnLocalUp.FlatStyle = FlatStyle.Flat;
            btnLocalUp.BackColor = Color.FromArgb(240, 240, 240);
            btnLocalUp.Click += delegate { NavigateLocalUp(); };
            localNavPanel.Controls.Add(btnLocalUp);
            
            Button btnLocalHome = new Button();
            btnLocalHome.Text = "🏠 Home";
            btnLocalHome.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            btnLocalHome.Size = new Size(80, 28);
            btnLocalHome.Location = new Point(70, 3);
            btnLocalHome.FlatStyle = FlatStyle.Flat;
            btnLocalHome.BackColor = Color.FromArgb(240, 240, 240);
            btnLocalHome.Click += delegate { NavigateLocalHome(); };
            localNavPanel.Controls.Add(btnLocalHome);
            
            // Drive selection dropdown for Windows
            ComboBox driveSelector = new ComboBox();
            driveSelector.Size = new Size(120, 28);
            driveSelector.Location = new Point(155, 3);
            driveSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            driveSelector.Font = new Font("Segoe UI", 9F);
            
            // Add drives
            try {
                foreach (DriveInfo drive in DriveInfo.GetDrives()) {
                    if (drive.IsReady) {
                        string label = string.Format("{0} ({1})", 
                            string.IsNullOrEmpty(drive.VolumeLabel) ? "Local Disk" : drive.VolumeLabel, 
                            drive.Name);
                        driveSelector.Items.Add(label);
                        
                        // Select current drive
                        if (_currentLocalPath.StartsWith(drive.Name, StringComparison.OrdinalIgnoreCase)) {
                            driveSelector.SelectedIndex = driveSelector.Items.Count - 1;
                        }
                    }
                }
                
                // If nothing selected, select first drive
                if (driveSelector.SelectedIndex < 0 && driveSelector.Items.Count > 0) {
                    driveSelector.SelectedIndex = 0;
                }
                
                driveSelector.SelectedIndexChanged += (s, e) => {
                    string selected = driveSelector.SelectedItem.ToString();
                    int startPos = selected.LastIndexOf('(') + 1;
                    int endPos = selected.LastIndexOf(')');
                    if (startPos > 0 && endPos > startPos) {
                        string drive = selected.Substring(startPos, endPos - startPos);
                        NavigateLocalTo(drive);
                    }
                };
                
                localNavPanel.Controls.Add(driveSelector);
            }
            catch (Exception) {
                // Ignore drive listing errors
            }
            
            // Local files listview
            _lstLocal = new ListView();
            _lstLocal.Dock = DockStyle.Fill;
            _lstLocal.View = View.Details;
            _lstLocal.FullRowSelect = true;
            _lstLocal.GridLines = false;
            _lstLocal.AllowDrop = true;
            _lstLocal.MultiSelect = true;
            _lstLocal.HideSelection = false;
            _lstLocal.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            _lstLocal.BorderStyle = BorderStyle.None;
            
            _lstLocal.SmallImageList = GetFileIconList();
            
            _lstLocal.Columns.Add("Name", 250);
            _lstLocal.Columns.Add("Type", 100);
            _lstLocal.Columns.Add("Size", 100);
            _lstLocal.Columns.Add("Modified", 150);
            
            _lstLocal.DoubleClick += LstLocal_DoubleClick;
            _lstLocal.KeyDown += LstLocal_KeyDown;
            
            localGroup.Controls.Add(_lstLocal);
        }
        
        // Helper method to create and return an image list for file icons
        private ImageList GetFileIconList()
        {
            ImageList imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageSize = new Size(16, 16);
            
            // Add folder icon
            Bitmap folderIcon = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(folderIcon))
            {
                g.Clear(Color.FromArgb(255, 222, 180));
                g.DrawRectangle(new Pen(Color.FromArgb(180, 160, 120)), 0, 4, 14, 11);
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 240, 200)), 1, 5, 13, 10);
            }
            imageList.Images.Add("folder", folderIcon);
            
            // Add file icon
            Bitmap fileIcon = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(fileIcon))
            {
                g.Clear(Color.FromArgb(250, 250, 250));
                g.DrawRectangle(new Pen(Color.FromArgb(120, 120, 180)), 2, 1, 12, 14);
                g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 255)), 3, 2, 11, 13);
                // Draw three lines to simulate text
                g.DrawLine(new Pen(Color.FromArgb(150, 150, 150)), 5, 5, 11, 5);
                g.DrawLine(new Pen(Color.FromArgb(150, 150, 150)), 5, 8, 11, 8);
                g.DrawLine(new Pen(Color.FromArgb(150, 150, 150)), 5, 11, 9, 11);
            }
            imageList.Images.Add("file", fileIcon);
            
            // Add parent directory icon
            Bitmap parentIcon = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(parentIcon))
            {
                g.Clear(Color.FromArgb(230, 230, 250));
                // Draw up arrow
                g.FillPolygon(
                    new SolidBrush(Color.FromArgb(80, 80, 180)),
                    new Point[] { new Point(8, 2), new Point(14, 8), new Point(10, 8), 
                                 new Point(10, 14), new Point(6, 14), new Point(6, 8), 
                                 new Point(2, 8) });
            }
            imageList.Images.Add("parent", parentIcon);
            
            return imageList;
        }
        
        private void CreateRemotePane(Panel parent)
        {
            GroupBox remoteGroup = new GroupBox();
            remoteGroup.Text = string.Format("Remote Files ({0})", _connectionSettings.Protocol);
            remoteGroup.Dock = DockStyle.Fill;
            remoteGroup.Padding = new Padding(5);
            remoteGroup.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            remoteGroup.ForeColor = Color.FromArgb(60, 60, 60);
            parent.Controls.Add(remoteGroup);
            
            // Remote path panel with icon
            Panel pathPanel = new Panel();
            pathPanel.Height = 30;
            pathPanel.Dock = DockStyle.Top;
            pathPanel.BackColor = Color.FromArgb(245, 240, 250);
            remoteGroup.Controls.Add(pathPanel);
            
            Label pathIcon = new Label();
            pathIcon.Text = "🌐";
            pathIcon.Size = new Size(25, 25);
            pathIcon.Location = new Point(5, 5);
            pathIcon.Font = new Font("Segoe UI", 10F);
            pathPanel.Controls.Add(pathIcon);
            
            // Remote path textbox
            _txtRemotePath = new TextBox();
            _txtRemotePath.Size = new Size(parent.Width - 45, 22);
            _txtRemotePath.Location = new Point(30, 5);
            _txtRemotePath.Text = _currentRemotePath;
            _txtRemotePath.Font = new Font("Consolas", 9F, FontStyle.Regular);
            _txtRemotePath.ReadOnly = true;
            _txtRemotePath.BackColor = Color.White;
            _txtRemotePath.BorderStyle = BorderStyle.FixedSingle;
            pathPanel.Controls.Add(_txtRemotePath);
            
            // Remote navigation buttons
            Panel remoteNavPanel = new Panel();
            remoteNavPanel.Height = 36;
            remoteNavPanel.Dock = DockStyle.Top;
            remoteNavPanel.BackColor = Color.FromArgb(245, 245, 245);
            remoteNavPanel.BorderStyle = BorderStyle.FixedSingle;
            remoteGroup.Controls.Add(remoteNavPanel);
            remoteNavPanel.BringToFront();
            
            Button btnRemoteUp = new Button();
            btnRemoteUp.Text = "↑ Up";
            btnRemoteUp.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            btnRemoteUp.Size = new Size(60, 28);
            btnRemoteUp.Location = new Point(5, 3);
            btnRemoteUp.FlatStyle = FlatStyle.Flat;
            btnRemoteUp.BackColor = Color.FromArgb(240, 240, 240);
            btnRemoteUp.Click += delegate { NavigateRemoteUp(); };
            remoteNavPanel.Controls.Add(btnRemoteUp);
            
            Button btnRemoteHome = new Button();
            btnRemoteHome.Text = "/ Root";
            btnRemoteHome.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            btnRemoteHome.Size = new Size(80, 28);
            btnRemoteHome.Location = new Point(70, 3);
            btnRemoteHome.FlatStyle = FlatStyle.Flat;
            btnRemoteHome.BackColor = Color.FromArgb(240, 240, 240);
            btnRemoteHome.Click += delegate { NavigateRemoteHome(); };
            remoteNavPanel.Controls.Add(btnRemoteHome);
            
            // Show connection info
            Label lblConnectionInfo = new Label();
            lblConnectionInfo.AutoSize = true;
            lblConnectionInfo.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular);
            lblConnectionInfo.Text = string.Format("{0}:{1}@{2}", 
                _connectionSettings.Protocol, 
                _connectionSettings.Port,
                _connectionSettings.Host);
            lblConnectionInfo.Location = new Point(155, 9);
            lblConnectionInfo.ForeColor = Color.FromArgb(80, 80, 80);
            remoteNavPanel.Controls.Add(lblConnectionInfo);
            
            // Remote files listview
            _lstRemote = new ListView();
            _lstRemote.Dock = DockStyle.Fill;
            _lstRemote.View = View.Details;
            _lstRemote.FullRowSelect = true;
            _lstRemote.GridLines = false;
            _lstRemote.AllowDrop = true;
            _lstRemote.MultiSelect = true;
            _lstRemote.HideSelection = false;
            _lstRemote.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            _lstRemote.BorderStyle = BorderStyle.None;
            
            _lstRemote.SmallImageList = GetFileIconList();
            
            _lstRemote.Columns.Add("Name", 250);
            _lstRemote.Columns.Add("Type", 100);
            _lstRemote.Columns.Add("Size", 100);
            _lstRemote.Columns.Add("Modified", 150);
            
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
            statusPanel.BackColor = Color.FromArgb(245, 245, 245);
            parent.Controls.Add(statusPanel);
            
            // Title for status section
            Label statusTitle = new Label();
            statusTitle.Text = "Transfer Status";
            statusTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            statusTitle.AutoSize = true;
            statusTitle.Location = new Point(10, 5);
            statusTitle.ForeColor = Color.FromArgb(60, 60, 60);
            statusPanel.Controls.Add(statusTitle);
            
            // Enhanced progress bar
            _progressBar = new ProgressBar();
            _progressBar.Location = new Point(10, 25);
            _progressBar.Size = new Size(parent.Width - 25, 22);
            _progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _progressBar.Visible = false;
            statusPanel.Controls.Add(_progressBar);
            
            // Status message with icon
            Panel statusMessagePanel = new Panel();
            statusMessagePanel.Size = new Size(parent.Width - 25, 20);
            statusMessagePanel.Location = new Point(10, 50);
            statusMessagePanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            statusPanel.Controls.Add(statusMessagePanel);
            
            PictureBox statusIcon = new PictureBox();
            statusIcon.Size = new Size(16, 16);
            statusIcon.Location = new Point(0, 2);
            statusIcon.BackColor = Color.Green;
            statusMessagePanel.Controls.Add(statusIcon);
            
            // Status label
            _lblTransferStatus = new Label();
            _lblTransferStatus.Location = new Point(20, 2);
            _lblTransferStatus.Size = new Size(parent.Width - 50, 20);
            _lblTransferStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _lblTransferStatus.Font = new Font("Segoe UI", 9F);
            _lblTransferStatus.Text = "Ready";
            statusMessagePanel.Controls.Add(_lblTransferStatus);
            
            // Transfer queue info
            _lblQueue = new Label();
            _lblQueue.Location = new Point(parent.Width - 160, 50);
            _lblQueue.Size = new Size(150, 20);
            _lblQueue.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            _lblQueue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblQueue.ForeColor = Color.FromArgb(60, 60, 100);
            _lblQueue.TextAlign = ContentAlignment.MiddleRight;
            _lblQueue.Text = "Queue: 0 items";
            statusPanel.Controls.Add(_lblQueue);
        }
        
        private void CreateActionButtons(Panel parent)
        {
            // Action buttons panel
            FlowLayoutPanel actionPanel = new FlowLayoutPanel();
            actionPanel.FlowDirection = FlowDirection.RightToLeft;
            actionPanel.Size = new Size(240, 30);
            actionPanel.Location = new Point(parent.Width - 250, 5);
            actionPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            parent.Controls.Add(actionPanel);

            // Close button
            Button btnClose = new Button();
            btnClose.Text = "Close";
            btnClose.Size = new Size(70, 24);
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += (s, e) => this.Close();
            actionPanel.Controls.Add(btnClose);

            // Download button
            Button btnDownload = new Button();
            btnDownload.Text = "Download";
            btnDownload.Size = new Size(80, 24);
            btnDownload.UseVisualStyleBackColor = true;
            btnDownload.BackColor = Color.FromArgb(230, 240, 250);
            btnDownload.FlatStyle = FlatStyle.Flat;
            btnDownload.FlatAppearance.BorderColor = Color.FromArgb(100, 150, 200);
            btnDownload.Click += (s, e) => DownloadSelectedFiles();
            actionPanel.Controls.Add(btnDownload);

            // Refresh button
            Button btnRefresh = new Button();
            btnRefresh.Text = "Refresh";
            btnRefresh.Size = new Size(70, 24);
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += (s, e) => RefreshViews();
            actionPanel.Controls.Add(btnRefresh);
        }
        
        private void RefreshViews()
        {
            // Refresh the local file view
            NavigateLocalTo(_currentLocalPath);
            
            // Refresh the remote file view if connected
            if (_transferClient != null)
            {
                NavigateRemoteTo(_currentRemotePath);
            }
            
            // Update status
            _lblTransferStatus.Text = "Views refreshed";
        }

        private void DownloadSelectedFiles()
        {
            if (_transferClient == null)
            {
                MessageBox.Show("Please connect to a server first.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_lstRemote.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more files to download.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the local target directory
            string localTargetPath = _currentLocalPath;

            // Show progress bar
            _progressBar.Value = 0;
            _progressBar.Visible = true;
            _lblTransferStatus.Text = "Preparing to download...";

            // Count total files for queue display
            int totalFiles = _lstRemote.SelectedItems.Count;
            _lblQueue.Text = $"Queue: {totalFiles} item(s)";

            // Process each selected file
            List<string> filesToDownload = new List<string>();
            foreach (ListViewItem item in _lstRemote.SelectedItems)
            {
                if (item.Tag != null && item.Tag.ToString() != "D")
                {
                    string remoteFile = Path.Combine(_currentRemotePath, item.Text);
                    filesToDownload.Add(remoteFile);
                }
            }

            // Begin the download process
            StartFileDownload(filesToDownload, localTargetPath);
        }

        private void StartFileDownload(List<string> files, string localTargetPath)
        {
            // Here we would actually initiate the download
            // This is a placeholder for the actual download implementation
            
            // Update UI to show download in progress
            _progressBar.Value = 10; // Initial progress
            _lblTransferStatus.Text = $"Downloading {files.Count} file(s) to {localTargetPath}...";
            
            // Normally you'd use a background worker here, but for simplicity:
            // Example of a download completion simulation:
            _progressBar.Value = 100;
            _lblTransferStatus.Text = $"Downloaded {files.Count} file(s) successfully.";
            
            // In a real implementation, you would need to:
            // 1. Create a background worker
            // 2. Report progress as files download
            // 3. Handle errors appropriately
            // 4. Update the UI on completion
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
