using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using syncer.core;

namespace syncer.ui.Forms
{
    public partial class FormRemoteDirectoryBrowser : Form
    {
        private ConnectionSettings _connectionSettings;
        private string _currentPath = "/";
        private List<string> _directories = new List<string>();
        private List<string> _files = new List<string>();
        
        public string SelectedPath { get; private set; }

        public FormRemoteDirectoryBrowser(ConnectionSettings connectionSettings)
        {
            InitializeComponent();
            _connectionSettings = connectionSettings;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = $"Browse Remote Directory ({_connectionSettings.Protocol})";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(500, 400);
            this.MaximizeBox = false;
            
            // Create components
            var lblCurrentPath = new Label
            {
                Text = "Current Path:",
                Location = new Point(12, 15),
                AutoSize = true
            };
            
            var txtCurrentPath = new TextBox
            {
                Location = new Point(110, 12),
                Size = new Size(370, 23),
                ReadOnly = true,
                Text = _currentPath
            };
            
            var btnParent = new Button
            {
                Text = "Parent Directory",
                Location = new Point(486, 11),
                Size = new Size(90, 25)
            };
            btnParent.Click += (sender, e) => NavigateToParent();
            
            var lstDirectories = new ListView
            {
                Location = new Point(12, 45),
                Size = new Size(564, 350),
                View = View.Details,
                FullRowSelect = true
            };
            lstDirectories.Columns.Add("Name", 300);
            lstDirectories.Columns.Add("Type", 100);
            
            lstDirectories.DoubleClick += (sender, e) => 
            {
                if (lstDirectories.SelectedItems.Count > 0)
                {
                    var selectedItem = lstDirectories.SelectedItems[0];
                    string itemType = selectedItem.SubItems[1].Text;
                    
                    if (itemType == "Directory")
                    {
                        string dirName = selectedItem.Text;
                        if (dirName == "..")
                        {
                            NavigateToParent();
                        }
                        else
                        {
                            string newPath = _currentPath.EndsWith("/") 
                                ? _currentPath + dirName 
                                : _currentPath + "/" + dirName;
                            
                            NavigateTo(newPath);
                        }
                    }
                }
            };
            
            var btnSelect = new Button
            {
                Text = "Select This Directory",
                Location = new Point(333, 405),
                Size = new Size(150, 30),
                DialogResult = DialogResult.OK
            };
            btnSelect.Click += (sender, e) => 
            {
                SelectedPath = _currentPath;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            
            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(489, 405),
                Size = new Size(87, 30),
                DialogResult = DialogResult.Cancel
            };
            
            // Add controls to form
            this.Controls.Add(lblCurrentPath);
            this.Controls.Add(txtCurrentPath);
            this.Controls.Add(btnParent);
            this.Controls.Add(lstDirectories);
            this.Controls.Add(btnSelect);
            this.Controls.Add(btnCancel);
            
            // Store references to controls we need to update
            _txtCurrentPath = txtCurrentPath;
            _lstDirectories = lstDirectories;
            
            // Load directories on form load
            this.Load += (sender, e) => 
            {
                RefreshDirectoryListing();
            };
        }
        
        private TextBox _txtCurrentPath;
        private ListView _lstDirectories;
        
        private void NavigateTo(string path)
        {
            _currentPath = path;
            _txtCurrentPath.Text = _currentPath;
            RefreshDirectoryListing();
        }
        
        private void NavigateToParent()
        {
            if (_currentPath == "/" || _currentPath == "")
                return;
                
            string parent = _currentPath.Substring(0, _currentPath.LastIndexOf('/'));
            if (string.IsNullOrEmpty(parent))
                parent = "/";
                
            NavigateTo(parent);
        }
        
        private void RefreshDirectoryListing()
        {
            _lstDirectories.Items.Clear();
            
            try
            {
                this.Cursor = Cursors.WaitCursor;
                
                // Add parent directory entry if not at root
                if (_currentPath != "/" && _currentPath != "")
                {
                    var parentItem = new ListViewItem("..");
                    parentItem.SubItems.Add("Directory");
                    _lstDirectories.Items.Add(parentItem);
                }
                
                // Get directory listing based on protocol
                ITransferClient client;
                switch (_connectionSettings.Protocol)
                {
                    case ProtocolType.Sftp:
                        client = new EnhancedSftpTransferClient(new SftpConfiguration());
                        break;
                    case ProtocolType.Ftp:
                        client = new FtpTransferClient();
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported protocol for remote directory browsing");
                }
                
                if (client.ListFiles(_connectionSettings, _currentPath, out List<string> fileList, out string error))
                {
                    // Process the file list
                    _directories.Clear();
                    _files.Clear();
                    
                    foreach (string path in fileList)
                    {
                        string fileName = GetFileNameFromPath(path);
                        
                        // For FTP, we need to determine if it's a directory
                        bool isDirectory = false;
                        if (_connectionSettings.Protocol == ProtocolType.Ftp)
                        {
                            // For FTP we might need to make an additional call to check if it's a directory
                            // This is simplified for now
                            isDirectory = fileName.EndsWith("/");
                            if (isDirectory)
                            {
                                fileName = fileName.TrimEnd('/');
                            }
                        }
                        
                        if (isDirectory)
                        {
                            _directories.Add(fileName);
                            
                            var item = new ListViewItem(fileName);
                            item.SubItems.Add("Directory");
                            _lstDirectories.Items.Add(item);
                        }
                        else
                        {
                            _files.Add(fileName);
                            
                            var item = new ListViewItem(fileName);
                            item.SubItems.Add("File");
                            _lstDirectories.Items.Add(item);
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Error listing directory: {error}", "Directory Listing Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error browsing remote directory: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        
        private string GetFileNameFromPath(string path)
        {
            int lastSlash = path.LastIndexOf('/');
            if (lastSlash >= 0 && lastSlash < path.Length - 1)
            {
                return path.Substring(lastSlash + 1);
            }
            return path;
        }
    }
}
