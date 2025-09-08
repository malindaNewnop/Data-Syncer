using System;
using System.Windows.Forms;

namespace syncer.ui.Forms
{
    /// <summary>
    /// Form for editing individual sync job within a multi-job configuration
    /// </summary>
    public partial class FormEditSyncJob : Form
    {
        private SyncJob _syncJob;
        
        public SyncJob SyncJob 
        { 
            get { return _syncJob; } 
        }
        
        public bool IsNewJob { get; private set; }

        public FormEditSyncJob(SyncJob syncJob = null)
        {
            InitializeComponent();
            
            IsNewJob = syncJob == null;
            _syncJob = syncJob ?? new SyncJob();
            
            InitializeForm();
            LoadJobData();
        }

        private void InitializeForm()
        {
            this.Text = IsNewJob ? "Add New Job" : "Edit Job";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new System.Drawing.Size(500, 400);

            // Set up tooltips for better usability
            var toolTip = new ToolTip();
            toolTip.SetToolTip(txtJobName, "Enter a unique name for this sync job");
            toolTip.SetToolTip(txtSourcePath, "Enter the source path for file synchronization");
            toolTip.SetToolTip(txtDestinationPath, "Enter the destination path for file synchronization");
        }

        private void LoadJobData()
        {
            if (_syncJob == null) return;

            txtJobName.Text = _syncJob.Name ?? "";
            txtDescription.Text = _syncJob.Description ?? "";
            txtSourcePath.Text = _syncJob.SourcePath ?? "";
            txtDestinationPath.Text = _syncJob.DestinationPath ?? "";
            numInterval.Value = Math.Max(1, _syncJob.IntervalValue);
            
            if (!string.IsNullOrEmpty(_syncJob.IntervalType))
            {
                int index = cmbIntervalType.FindString(_syncJob.IntervalType);
                if (index >= 0) 
                    cmbIntervalType.SelectedIndex = index;
            }
            
            chkEnabled.Checked = _syncJob.IsEnabled;
            chkIncludeSubfolders.Checked = _syncJob.IncludeSubFolders;
            chkDeleteSourceAfterTransfer.Checked = _syncJob.DeleteSourceAfterTransfer;
            
            // File filtering
            chkEnableFilters.Checked = _syncJob.EnableFilters;
            txtIncludeFileTypes.Text = _syncJob.IncludeFileTypes ?? "";
            txtExcludeFileTypes.Text = _syncJob.ExcludeFileTypes ?? "";
            
            UpdateFilterControlStates();
        }

        private void SaveJobData()
        {
            if (_syncJob == null) return;

            _syncJob.Name = txtJobName.Text.Trim();
            _syncJob.Description = txtDescription.Text.Trim();
            _syncJob.SourcePath = txtSourcePath.Text.Trim();
            _syncJob.DestinationPath = txtDestinationPath.Text.Trim();
            _syncJob.IntervalValue = (int)numInterval.Value;
            _syncJob.IntervalType = cmbIntervalType.Text;
            _syncJob.IsEnabled = chkEnabled.Checked;
            _syncJob.IncludeSubFolders = chkIncludeSubfolders.Checked;
            _syncJob.DeleteSourceAfterTransfer = chkDeleteSourceAfterTransfer.Checked;
            
            // File filtering
            _syncJob.EnableFilters = chkEnableFilters.Checked;
            _syncJob.IncludeFileTypes = txtIncludeFileTypes.Text.Trim();
            _syncJob.ExcludeFileTypes = txtExcludeFileTypes.Text.Trim();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(txtJobName.Text.Trim()))
            {
                MessageBox.Show("Job name is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtJobName.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtSourcePath.Text.Trim()))
            {
                MessageBox.Show("Source path is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSourcePath.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtDestinationPath.Text.Trim()))
            {
                MessageBox.Show("Destination path is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDestinationPath.Focus();
                return false;
            }

            return true;
        }

        private void UpdateFilterControlStates()
        {
            bool enableFilters = chkEnableFilters.Checked;
            txtIncludeFileTypes.Enabled = enableFilters;
            txtExcludeFileTypes.Enabled = enableFilters;
            lblIncludeFileTypes.Enabled = enableFilters;
            lblExcludeFileTypes.Enabled = enableFilters;
        }

        #region Event Handlers

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                SaveJobData();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnBrowseSource_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select Source Folder";
                folderDialog.SelectedPath = txtSourcePath.Text;
                
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtSourcePath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnBrowseDestination_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select Destination Folder";
                folderDialog.SelectedPath = txtDestinationPath.Text;
                
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtDestinationPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void chkEnableFilters_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFilterControlStates();
        }

        #endregion
    }
}
