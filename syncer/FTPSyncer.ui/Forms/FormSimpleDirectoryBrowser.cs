using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using FTPSyncer.core;
using FTPSyncer.core.Configuration;
using FTPSyncer.core.Transfers;
using FTPSyncer.ui.Services;

namespace FTPSyncer.ui.Forms
{
    public partial class FormSimpleDirectoryBrowser : Form
    {
        private IConnectionService _connectionService;
        private ITransferClient _transferClient;
        private FTPSyncer.core.ConnectionSettings _connectionSettings;
        
        // UI Controls
        private ListView _lstFiles;
        private TextBox _txtCurrentPath;
        private Label _lblTitle;
        private TreeView _treeDirectories;
        private ImageList _imageList;
        
        // Current directory tracking
        private string _currentPath;
        private bool _isRemoteMode;
        private bool _allowFileSelection = false; // New property for file selection
        private bool _isDownloadMode = false; // New property for download mode
        
        // Selected paths for return
        public string SelectedPath { get; private set; }
        public string SelectedFileName { get; private set; } // New property for selected file
        public bool IsFileSelected { get; private set; } // New property to check if file is selected
        
        public string SelectedRemotePath 
        { 
            get { return _isRemoteMode ? SelectedPath : null; }
        }
        public string SelectedLocalPath 
        { 
            get { return !_isRemoteMode ? SelectedPath : null; }
        }
        
        // New properties for file selection support
        public bool AllowFileSelection
        {
            get { return _allowFileSelection; }
            set 
            { 
                _allowFileSelection = value;
                UpdateUIForMode();
            }
        }
        
        public bool IsDownloadMode
        {
            get { return _isDownloadMode; }
            set 
            { 
                _isDownloadMode = value;
                _allowFileSelection = value; // Enable file selection for download mode
                UpdateUIForMode();
            }
        }
        public bool IsUploadMode 
        { 
            get { return !_isRemoteMode; }
            set { IsRemoteMode = !value; }
        }
        public bool IsRemoteMode 
        { 
            get { return _isRemoteMode; }
            set 
            { 
                _isRemoteMode = value;
                _currentPath = _isRemoteMode ? "/" : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                UpdateUIForMode();
            }
        }
        
        public FormSimpleDirectoryBrowser(FTPSyncer.core.ConnectionSettings connectionSettings = null)
        {
            _connectionSettings = connectionSettings;
            InitializeComponent();
            InitializeServices();
            InitializeUI();
            IsRemoteMode = false; // Default to local mode
        }
        
        #region Initialization
        
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
                if (_connectionSettings == null || !_isRemoteMode) return;
                
                switch (_connectionSettings.Protocol)
                {
                    case FTPSyncer.core.ProtocolType.Local:
                        _transferClient = new LocalTransferClient();
                        break;
                    case FTPSyncer.core.ProtocolType.Ftp:
                        _transferClient = new EnhancedFtpTransferClient();
                        break;
                    case FTPSyncer.core.ProtocolType.Sftp:
                        _transferClient = new ProductionSftpTransferClient();
                        break;
                }
                
                if (_transferClient != null && _connectionSettings.Protocol != FTPSyncer.core.ProtocolType.Local)
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
        
        private void InitializeUI()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "Directory Browser";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(600, 400);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            
            // Create image list for icons
            CreateImageList();
            
            // Create main layout
            CreateMainLayout();
            
            this.ResumeLayout();
        }
        
        private void CreateImageList()
        {
            _imageList = new ImageList();
            _imageList.ImageSize = new Size(16, 16);
            _imageList.ColorDepth = ColorDepth.Depth32Bit;
            
            try
            {
                // Add folder icon
                _imageList.Images.Add("folder", SystemIcons.WinLogo.ToBitmap());
                // Add file icon  
                _imageList.Images.Add("file", SystemIcons.WinLogo.ToBitmap());
            }
            catch
            {
                // Fallback if system icons fail
                Bitmap folderBmp = new Bitmap(16, 16);
                Bitmap fileBmp = new Bitmap(16, 16);
                _imageList.Images.Add("folder", folderBmp);
                _imageList.Images.Add("file", fileBmp);
            }
        }
        
        private void CreateMainLayout()
        {
            // Main container
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(10);
            this.Controls.Add(mainPanel);
            
            // Title label
            _lblTitle = new Label();
            _lblTitle.Text = "Select Directory";
            _lblTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            _lblTitle.AutoSize = true;
            _lblTitle.Location = new Point(0, 0);
            _lblTitle.Dock = DockStyle.Top;
            _lblTitle.Height = 30;
            _lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            mainPanel.Controls.Add(_lblTitle);
            
            // Current path display
            Panel pathPanel = new Panel();
            pathPanel.Height = 35;
            pathPanel.Dock = DockStyle.Top;
            mainPanel.Controls.Add(pathPanel);
            
            Label lblPath = new Label();
            lblPath.Text = "Path:";
            lblPath.AutoSize = true;
            lblPath.Location = new Point(0, 8);
            pathPanel.Controls.Add(lblPath);
            
            _txtCurrentPath = new TextBox();
            _txtCurrentPath.Location = new Point(45, 5);
            _txtCurrentPath.Width = pathPanel.Width - 130;
            _txtCurrentPath.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _txtCurrentPath.ReadOnly = true;
            _txtCurrentPath.BackColor = SystemColors.Control;
            pathPanel.Controls.Add(_txtCurrentPath);
            
            Button btnUp = new Button();
            btnUp.Text = "Up";
            btnUp.Location = new Point(pathPanel.Width - 80, 3);
            btnUp.Size = new Size(35, 25);
            btnUp.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnUp.Click += BtnUp_Click;
            pathPanel.Controls.Add(btnUp);
            
            Button btnHome = new Button();
            btnHome.Text = "Home";
            btnHome.Location = new Point(pathPanel.Width - 40, 3);
            btnHome.Size = new Size(45, 25);
            btnHome.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnHome.Click += BtnHome_Click;
            pathPanel.Controls.Add(btnHome);
            
            // Split container for tree and list
            SplitContainer splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Orientation = Orientation.Horizontal;
            splitContainer.SplitterDistance = 200;
            splitContainer.FixedPanel = FixedPanel.Panel1;
            mainPanel.Controls.Add(splitContainer);
            
            // Directory tree (for navigation)
            _treeDirectories = new TreeView();
            _treeDirectories.Dock = DockStyle.Fill;
            _treeDirectories.ImageList = _imageList;
            _treeDirectories.HideSelection = false;
            _treeDirectories.AfterSelect += TreeDirectories_AfterSelect;
            splitContainer.Panel1.Controls.Add(_treeDirectories);
            
            // Files list view
            _lstFiles = new ListView();
            _lstFiles.Dock = DockStyle.Fill;
            _lstFiles.View = View.Details;
            _lstFiles.FullRowSelect = true;
            _lstFiles.GridLines = true;
            _lstFiles.MultiSelect = false;
            _lstFiles.HideSelection = false;
            _lstFiles.SmallImageList = _imageList;
            
            _lstFiles.Columns.Add("Name", 300);
            _lstFiles.Columns.Add("Type", 100);
            _lstFiles.Columns.Add("Size", 100);
            _lstFiles.Columns.Add("Modified", 150);
            
            _lstFiles.DoubleClick += LstFiles_DoubleClick;
            _lstFiles.SelectedIndexChanged += LstFiles_SelectedIndexChanged;
            splitContainer.Panel2.Controls.Add(_lstFiles);
            
            // Buttons panel
            Panel buttonPanel = new Panel();
            buttonPanel.Height = 50;
            buttonPanel.Dock = DockStyle.Bottom;
            mainPanel.Controls.Add(buttonPanel);
            
            Button btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Size = new Size(80, 30);
            btnOK.Location = new Point(buttonPanel.Width - 170, 10);
            btnOK.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Click += BtnOK_Click;
            buttonPanel.Controls.Add(btnOK);
            
            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Size = new Size(80, 30);
            btnCancel.Location = new Point(buttonPanel.Width - 85, 10);
            btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnCancel.DialogResult = DialogResult.Cancel;
            buttonPanel.Controls.Add(btnCancel);
            
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateUIForMode()
        {
            if (_isRemoteMode)
            {
                if (_isDownloadMode && _allowFileSelection)
                {
                    this.Text = $"Remote File Browser - {_connectionSettings?.Protocol}";
                    _lblTitle.Text = "Select Remote Files to Download";
                }
                else
                {
                    this.Text = $"Remote Directory Browser - {_connectionSettings?.Protocol}";
                    _lblTitle.Text = "Select Remote Directory";
                }
                InitializeTransferClient();
            }
            else
            {
                if (_allowFileSelection)
                {
                    this.Text = "Local File Browser";
                    _lblTitle.Text = "Select Local Files";
                }
                else
                {
                    this.Text = "Local Directory Browser";
                    _lblTitle.Text = "Select Local Directory";
                }
            }
            
            // Update ListView selection mode based on file selection capability
            if (_allowFileSelection)
            {
                _lstFiles.MultiSelect = true; // Allow multiple file selection
                _lstFiles.FullRowSelect = true;
            }
            else
            {
                _lstFiles.MultiSelect = false;
                _lstFiles.FullRowSelect = true;
            }
            
            RefreshView();
        }
        
        private void RefreshView()
        {
            _txtCurrentPath.Text = _currentPath;
            PopulateDirectoryTree();
            PopulateFilesList();
        }
        
        #endregion
        
        #region Directory Tree Population
        
        private void PopulateDirectoryTree()
        {
            _treeDirectories.Nodes.Clear();
            
            try
            {
                if (_isRemoteMode)
                {
                    PopulateRemoteDirectoryTree();
                }
                else
                {
                    PopulateLocalDirectoryTree();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading directory tree: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void PopulateLocalDirectoryTree()
        {
            // Add drives as root nodes
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    TreeNode driveNode = new TreeNode(drive.Name);
                    driveNode.Tag = drive.RootDirectory.FullName;
                    driveNode.ImageIndex = 0;
                    driveNode.SelectedImageIndex = 0;
                    _treeDirectories.Nodes.Add(driveNode);
                    
                    // Add dummy node for expansion
                    driveNode.Nodes.Add("Loading...");
                }
            }
            
            _treeDirectories.BeforeExpand += TreeDirectories_BeforeExpand;
        }
        
        private void PopulateRemoteDirectoryTree()
        {
            if (_transferClient == null) return;
            
            // Start with root
            TreeNode rootNode = new TreeNode("/");
            rootNode.Tag = "/";
            rootNode.ImageIndex = 0;
            rootNode.SelectedImageIndex = 0;
            _treeDirectories.Nodes.Add(rootNode);
            
            // Add dummy node for expansion
            rootNode.Nodes.Add("Loading...");
            rootNode.Expand();
            
            _treeDirectories.BeforeExpand += TreeDirectories_BeforeExpand;
        }
        
        private void TreeDirectories_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "Loading...")
            {
                e.Node.Nodes.Clear();
                
                if (_isRemoteMode)
                {
                    LoadRemoteSubdirectories(e.Node);
                }
                else
                {
                    LoadLocalSubdirectories(e.Node);
                }
            }
        }
        
        private void LoadLocalSubdirectories(TreeNode parentNode)
        {
            try
            {
                string path = (string)parentNode.Tag;
                string[] directories = Directory.GetDirectories(path);
                
                foreach (string dir in directories)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);
                    if ((dirInfo.Attributes & FileAttributes.Hidden) == 0 && 
                        (dirInfo.Attributes & FileAttributes.System) == 0)
                    {
                        TreeNode childNode = new TreeNode(dirInfo.Name);
                        childNode.Tag = dirInfo.FullName;
                        childNode.ImageIndex = 0;
                        childNode.SelectedImageIndex = 0;
                        parentNode.Nodes.Add(childNode);
                        
                        // Check if this directory has subdirectories
                        try
                        {
                            if (Directory.GetDirectories(dirInfo.FullName).Length > 0)
                            {
                                childNode.Nodes.Add("Loading...");
                            }
                        }
                        catch { } // Ignore access denied errors
                    }
                }
            }
            catch (Exception ex)
            {
                TreeNode errorNode = new TreeNode("Error: " + ex.Message);
                parentNode.Nodes.Add(errorNode);
            }
        }
        
        private void LoadRemoteSubdirectories(TreeNode parentNode)
        {
            try
            {
                if (_transferClient == null) return;
                
                string path = (string)parentNode.Tag;
                List<string> allItems;
                string error;
                
                // For remote directories, we'll have to use a different approach since 
                // the current interface doesn't distinguish between files and directories
                // We'll show a simplified structure for now
                if (_transferClient.ListFiles(_connectionSettings, path, out allItems, out error))
                {
                    // For simplicity, we won't populate subdirectories in the tree view
                    // as the current interface doesn't provide directory information
                    // The user can navigate using the main file list
                }
                else if (!string.IsNullOrEmpty(error))
                {
                    TreeNode errorNode = new TreeNode("Error: " + error);
                    parentNode.Nodes.Add(errorNode);
                }
            }
            catch (Exception ex)
            {
                TreeNode errorNode = new TreeNode("Error: " + ex.Message);
                parentNode.Nodes.Add(errorNode);
            }
        }
        
        #endregion
        
        #region Files List Population
        
        private void PopulateFilesList()
        {
            _lstFiles.Items.Clear();
            
            try
            {
                if (_isRemoteMode)
                {
                    PopulateRemoteFilesList();
                }
                else
                {
                    PopulateLocalFilesList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading files: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void PopulateLocalFilesList()
        {
            if (!Directory.Exists(_currentPath)) return;
            
            // Add parent directory entry
            if (_currentPath != Path.GetPathRoot(_currentPath))
            {
                ListViewItem parentItem = new ListViewItem("..");
                parentItem.SubItems.Add("Folder");
                parentItem.SubItems.Add("");
                parentItem.SubItems.Add("");
                parentItem.ImageIndex = 0;
                parentItem.Tag = Directory.GetParent(_currentPath)?.FullName;
                _lstFiles.Items.Add(parentItem);
            }
            
            // Add directories
            string[] directories = Directory.GetDirectories(_currentPath);
            foreach (string dir in directories)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                if ((dirInfo.Attributes & FileAttributes.Hidden) == 0 && 
                    (dirInfo.Attributes & FileAttributes.System) == 0)
                {
                    ListViewItem item = new ListViewItem(dirInfo.Name);
                    item.SubItems.Add("Folder");
                    item.SubItems.Add("");
                    item.SubItems.Add(dirInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
                    item.ImageIndex = 0;
                    item.Tag = dirInfo.FullName;
                    _lstFiles.Items.Add(item);
                }
            }
            
            // Add files
            string[] files = Directory.GetFiles(_currentPath);
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                if ((fileInfo.Attributes & FileAttributes.Hidden) == 0 && 
                    (fileInfo.Attributes & FileAttributes.System) == 0)
                {
                    ListViewItem item = new ListViewItem(fileInfo.Name);
                    item.SubItems.Add("File");
                    item.SubItems.Add(FormatFileSize(fileInfo.Length));
                    item.SubItems.Add(fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
                    item.ImageIndex = 1;
                    item.Tag = fileInfo.FullName;
                    _lstFiles.Items.Add(item);
                }
            }
        }
        
        private void PopulateRemoteFilesList()
        {
            if (_transferClient == null) return;
            
            // Add parent directory entry
            if (_currentPath != "/")
            {
                ListViewItem parentItem = new ListViewItem("..");
                parentItem.SubItems.Add("Folder");
                parentItem.SubItems.Add("");
                parentItem.SubItems.Add("");
                parentItem.ImageIndex = 0;
                parentItem.Tag = GetParentPath(_currentPath);
                _lstFiles.Items.Add(parentItem);
            }
            
            // Get remote files and directories
            List<string> allItems;
            string error;
            if (_transferClient.ListFiles(_connectionSettings, _currentPath, out allItems, out error))
            {
                foreach (string item in allItems)
                {
                    string itemName = Path.GetFileName(item);
                    if (!string.IsNullOrEmpty(itemName) && itemName != "." && itemName != "..")
                    {
                        ListViewItem listItem = new ListViewItem(itemName);
                        
                        // Try to determine if it's a file or directory
                        bool isDirectory = false;
                        try
                        {
                            // Check if this is a directory by trying to list its contents
                            List<string> testList;
                            string testError;
                            string testPath = _currentPath.EndsWith("/") ? _currentPath + itemName : _currentPath + "/" + itemName;
                            isDirectory = _transferClient.ListFiles(_connectionSettings, testPath, out testList, out testError);
                        }
                        catch
                        {
                            // If we can't determine, assume it's a file
                            isDirectory = false;
                        }
                        
                        if (isDirectory)
                        {
                            listItem.SubItems.Add("Folder");
                            listItem.SubItems.Add("");
                            listItem.SubItems.Add("");
                            listItem.ImageIndex = 0;
                        }
                        else
                        {
                            listItem.SubItems.Add("File");
                            listItem.SubItems.Add(""); // Size not available with current interface
                            listItem.SubItems.Add(""); // Modified date not available with current interface
                            listItem.ImageIndex = 1;
                        }
                        
                        listItem.Tag = item;
                        _lstFiles.Items.Add(listItem);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show("Error listing remote files: " + error, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void TreeDirectories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                _currentPath = (string)e.Node.Tag;
                PopulateFilesList();
                _txtCurrentPath.Text = _currentPath;
            }
        }
        
        private void LstFiles_DoubleClick(object sender, EventArgs e)
        {
            if (_lstFiles.SelectedItems.Count > 0)
            {
                ListViewItem item = _lstFiles.SelectedItems[0];
                
                if (item.Text == "..")
                {
                    NavigateUp();
                }
                else if (item.SubItems[1].Text == "Folder")
                {
                    NavigateTo((string)item.Tag);
                }
                else if (item.SubItems[1].Text == "File" && _allowFileSelection)
                {
                    // If file selection is allowed and it's a file, select it and close
                    SelectedPath = _currentPath;
                    SelectedFileName = item.Text;
                    IsFileSelected = true;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }
        
        private void LstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Could be used to update status or preview
        }
        
        private void BtnUp_Click(object sender, EventArgs e)
        {
            NavigateUp();
        }
        
        private void BtnHome_Click(object sender, EventArgs e)
        {
            NavigateHome();
        }
        
        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (_allowFileSelection && _lstFiles.SelectedItems.Count > 0)
            {
                // Check if a file is selected
                ListViewItem selectedItem = _lstFiles.SelectedItems[0];
                if (selectedItem.SubItems[1].Text == "File")
                {
                    SelectedPath = _currentPath;
                    SelectedFileName = selectedItem.Text;
                    IsFileSelected = true;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                }
                else if (selectedItem.SubItems[1].Text == "Folder" && selectedItem.Text != "..")
                {
                    // If folder selected in file selection mode, navigate into it
                    NavigateTo((string)selectedItem.Tag);
                    return;
                }
            }
            
            // Default behavior: select current directory
            SelectedPath = _currentPath;
            SelectedFileName = null;
            IsFileSelected = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        
        #endregion
        
        #region Navigation
        
        private void NavigateTo(string path)
        {
            _currentPath = path;
            RefreshView();
        }
        
        private void NavigateUp()
        {
            if (_isRemoteMode)
            {
                if (_currentPath != "/")
                {
                    _currentPath = GetParentPath(_currentPath);
                    RefreshView();
                }
            }
            else
            {
                if (_currentPath != Path.GetPathRoot(_currentPath))
                {
                    DirectoryInfo parent = Directory.GetParent(_currentPath);
                    if (parent != null)
                    {
                        _currentPath = parent.FullName;
                        RefreshView();
                    }
                }
            }
        }
        
        private void NavigateHome()
        {
            if (_isRemoteMode)
            {
                _currentPath = "/";
            }
            else
            {
                _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            RefreshView();
        }
        
        private string GetParentPath(string path)
        {
            if (path == "/") return "/";
            
            int lastSlash = path.TrimEnd('/').LastIndexOf('/');
            if (lastSlash <= 0) return "/";
            
            return path.Substring(0, lastSlash);
        }
        
        #endregion
        
        #region Utilities
        
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }
        
        #endregion
    }
}





