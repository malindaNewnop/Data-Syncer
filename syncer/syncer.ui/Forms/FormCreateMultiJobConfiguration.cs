using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using syncer.ui.Interfaces;
using syncer.ui.Services;

namespace syncer.ui.Forms
{
    /// <summary>
    /// Form for creating multi-job configurations with multiple sync jobs
    /// </summary>
    public partial class FormCreateMultiJobConfiguration : Form
    {
        private List<SyncJob> _jobs = new List<SyncJob>();
        private ISavedJobConfigurationService _configService;
        private IConnectionService _connectionService;
        private int _currentJobIndex = 0;

        public SavedJobConfiguration CreatedConfiguration { get; private set; }

        public FormCreateMultiJobConfiguration(ISavedJobConfigurationService configService, IConnectionService connectionService)
        {
            InitializeComponent();
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
            
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Create Multi-Job Configuration";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Add a default job to start with
            AddNewJob();
            RefreshJobsList();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Configuration details
            var lblConfigName = new Label() { Text = "Configuration Name:", Location = new Point(10, 10), Size = new Size(120, 23) };
            var txtConfigName = new TextBox() { Location = new Point(140, 10), Size = new Size(200, 23), Name = "txtConfigName" };
            
            var lblConfigDesc = new Label() { Text = "Description:", Location = new Point(10, 40), Size = new Size(120, 23) };
            var txtConfigDesc = new TextBox() { Location = new Point(140, 40), Size = new Size(400, 23), Name = "txtConfigDesc" };
            
            // Jobs list
            var lblJobs = new Label() { Text = "Jobs:", Location = new Point(10, 80), Size = new Size(100, 23) };
            var lstJobs = new ListBox() { Location = new Point(10, 110), Size = new Size(200, 200), Name = "lstJobs" };
            lstJobs.SelectedIndexChanged += LstJobs_SelectedIndexChanged;
            
            // Job management buttons
            var btnAddJob = new Button() { Text = "Add Job", Location = new Point(220, 110), Size = new Size(80, 30), Name = "btnAddJob" };
            btnAddJob.Click += BtnAddJob_Click;
            
            var btnRemoveJob = new Button() { Text = "Remove Job", Location = new Point(220, 145), Size = new Size(80, 30), Name = "btnRemoveJob" };
            btnRemoveJob.Click += BtnRemoveJob_Click;
            
            var btnMoveUp = new Button() { Text = "Move Up", Location = new Point(220, 180), Size = new Size(80, 30), Name = "btnMoveUp" };
            btnMoveUp.Click += BtnMoveUp_Click;
            
            var btnMoveDown = new Button() { Text = "Move Down", Location = new Point(220, 215), Size = new Size(80, 30), Name = "btnMoveDown" };
            btnMoveDown.Click += BtnMoveDown_Click;
            
            // Job details panel
            var pnlJobDetails = new Panel() { Location = new Point(320, 80), Size = new Size(450, 400), Name = "pnlJobDetails", BorderStyle = BorderStyle.FixedSingle };
            
            // Job details controls
            var lblJobName = new Label() { Text = "Job Name:", Location = new Point(10, 10), Size = new Size(100, 23), Parent = pnlJobDetails };
            var txtJobName = new TextBox() { Location = new Point(120, 10), Size = new Size(200, 23), Name = "txtJobName", Parent = pnlJobDetails };
            
            var lblSourcePath = new Label() { Text = "Source Path:", Location = new Point(10, 40), Size = new Size(100, 23), Parent = pnlJobDetails };
            var txtSourcePath = new TextBox() { Location = new Point(120, 40), Size = new Size(250, 23), Name = "txtSourcePath", Parent = pnlJobDetails };
            var btnBrowseSource = new Button() { Text = "...", Location = new Point(375, 40), Size = new Size(30, 23), Name = "btnBrowseSource", Parent = pnlJobDetails };
            btnBrowseSource.Click += BtnBrowseSource_Click;
            
            var lblDestPath = new Label() { Text = "Destination Path:", Location = new Point(10, 70), Size = new Size(100, 23), Parent = pnlJobDetails };
            var txtDestPath = new TextBox() { Location = new Point(120, 70), Size = new Size(250, 23), Name = "txtDestPath", Parent = pnlJobDetails };
            var btnBrowseDest = new Button() { Text = "...", Location = new Point(375, 70), Size = new Size(30, 23), Name = "btnBrowseDest", Parent = pnlJobDetails };
            btnBrowseDest.Click += BtnBrowseDest_Click;
            
            var lblInterval = new Label() { Text = "Interval:", Location = new Point(10, 100), Size = new Size(100, 23), Parent = pnlJobDetails };
            var numInterval = new NumericUpDown() { Location = new Point(120, 100), Size = new Size(80, 23), Minimum = 1, Maximum = 9999, Value = 5, Name = "numInterval", Parent = pnlJobDetails };
            
            var cmbIntervalType = new ComboBox() { Location = new Point(210, 100), Size = new Size(100, 23), Name = "cmbIntervalType", Parent = pnlJobDetails, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbIntervalType.Items.AddRange(new[] { "Seconds", "Minutes", "Hours" });
            cmbIntervalType.SelectedIndex = 1; // Default to Minutes
            
            var chkEnabled = new CheckBox() { Text = "Enabled", Location = new Point(10, 130), Size = new Size(100, 23), Checked = true, Name = "chkEnabled", Parent = pnlJobDetails };
            var chkIncludeSubfolders = new CheckBox() { Text = "Include Subfolders", Location = new Point(120, 130), Size = new Size(150, 23), Checked = true, Name = "chkIncludeSubfolders", Parent = pnlJobDetails };
            var chkDeleteSource = new CheckBox() { Text = "Delete Source After Transfer", Location = new Point(10, 160), Size = new Size(200, 23), Name = "chkDeleteSource", Parent = pnlJobDetails };
            
            // Action buttons
            var btnCreate = new Button() { Text = "Create Configuration", Location = new Point(600, 500), Size = new Size(150, 30), Name = "btnCreate" };
            btnCreate.Click += BtnCreate_Click;
            
            var btnCancel = new Button() { Text = "Cancel", Location = new Point(510, 500), Size = new Size(80, 30), Name = "btnCancel" };
            btnCancel.Click += BtnCancel_Click;
            
            // Status label
            var lblStatus = new Label() { Location = new Point(10, 505), Size = new Size(400, 23), Name = "lblStatus", Text = "Ready to create multi-job configuration..." };
            
            // Add all controls
            this.Controls.AddRange(new Control[] {
                lblConfigName, txtConfigName, lblConfigDesc, txtConfigDesc,
                lblJobs, lstJobs, btnAddJob, btnRemoveJob, btnMoveUp, btnMoveDown,
                pnlJobDetails, btnCreate, btnCancel, lblStatus
            });
            
            // Wire up text change events to update current job
            txtJobName.TextChanged += JobDetail_Changed;
            txtSourcePath.TextChanged += JobDetail_Changed;
            txtDestPath.TextChanged += JobDetail_Changed;
            numInterval.ValueChanged += JobDetail_Changed;
            cmbIntervalType.SelectedIndexChanged += JobDetail_Changed;
            chkEnabled.CheckedChanged += JobDetail_Changed;
            chkIncludeSubfolders.CheckedChanged += JobDetail_Changed;
            chkDeleteSource.CheckedChanged += JobDetail_Changed;
            
            this.ResumeLayout(false);
        }

        private void AddNewJob()
        {
            var newJob = new SyncJob()
            {
                Name = $"Job {_jobs.Count + 1}",
                IntervalValue = 5,
                IntervalType = "Minutes",
                IsEnabled = true,
                IncludeSubFolders = true
            };
            
            _jobs.Add(newJob);
        }

        private void RefreshJobsList()
        {
            var lstJobs = this.Controls.Find("lstJobs", false)[0] as ListBox;
            var lblStatus = this.Controls.Find("lblStatus", false)[0] as Label;
            
            lstJobs.Items.Clear();
            
            for (int i = 0; i < _jobs.Count; i++)
            {
                var job = _jobs[i];
                var displayText = $"{i + 1}. {job.Name ?? "Unnamed Job"}";
                if (!job.IsEnabled) displayText += " (Disabled)";
                lstJobs.Items.Add(displayText);
            }
            
            if (_currentJobIndex < lstJobs.Items.Count && _currentJobIndex >= 0)
            {
                lstJobs.SelectedIndex = _currentJobIndex;
            }
            else if (lstJobs.Items.Count > 0)
            {
                _currentJobIndex = 0;
                lstJobs.SelectedIndex = 0;
            }
            
            lblStatus.Text = $"Multi-job configuration with {_jobs.Count} job(s). All jobs will run in parallel.";
            
            LoadJobDetails();
        }

        private void LoadJobDetails()
        {
            if (_currentJobIndex < 0 || _currentJobIndex >= _jobs.Count) return;
            
            var job = _jobs[_currentJobIndex];
            var pnlJobDetails = this.Controls.Find("pnlJobDetails", false)[0] as Panel;
            
            (pnlJobDetails.Controls.Find("txtJobName", false)[0] as TextBox).Text = job.Name ?? "";
            (pnlJobDetails.Controls.Find("txtSourcePath", false)[0] as TextBox).Text = job.SourcePath ?? "";
            (pnlJobDetails.Controls.Find("txtDestPath", false)[0] as TextBox).Text = job.DestinationPath ?? "";
            (pnlJobDetails.Controls.Find("numInterval", false)[0] as NumericUpDown).Value = job.IntervalValue;
            
            var cmbIntervalType = pnlJobDetails.Controls.Find("cmbIntervalType", false)[0] as ComboBox;
            var index = cmbIntervalType.FindString(job.IntervalType ?? "Minutes");
            cmbIntervalType.SelectedIndex = index >= 0 ? index : 1;
            
            (pnlJobDetails.Controls.Find("chkEnabled", false)[0] as CheckBox).Checked = job.IsEnabled;
            (pnlJobDetails.Controls.Find("chkIncludeSubfolders", false)[0] as CheckBox).Checked = job.IncludeSubFolders;
            (pnlJobDetails.Controls.Find("chkDeleteSource", false)[0] as CheckBox).Checked = job.DeleteSourceAfterTransfer;
        }

        private void SaveCurrentJobDetails()
        {
            if (_currentJobIndex < 0 || _currentJobIndex >= _jobs.Count) return;
            
            var job = _jobs[_currentJobIndex];
            var pnlJobDetails = this.Controls.Find("pnlJobDetails", false)[0] as Panel;
            
            job.Name = (pnlJobDetails.Controls.Find("txtJobName", false)[0] as TextBox).Text;
            job.SourcePath = (pnlJobDetails.Controls.Find("txtSourcePath", false)[0] as TextBox).Text;
            job.DestinationPath = (pnlJobDetails.Controls.Find("txtDestPath", false)[0] as TextBox).Text;
            job.IntervalValue = (int)(pnlJobDetails.Controls.Find("numInterval", false)[0] as NumericUpDown).Value;
            job.IntervalType = (pnlJobDetails.Controls.Find("cmbIntervalType", false)[0] as ComboBox).Text;
            job.IsEnabled = (pnlJobDetails.Controls.Find("chkEnabled", false)[0] as CheckBox).Checked;
            job.IncludeSubFolders = (pnlJobDetails.Controls.Find("chkIncludeSubfolders", false)[0] as CheckBox).Checked;
            job.DeleteSourceAfterTransfer = (pnlJobDetails.Controls.Find("chkDeleteSource", false)[0] as CheckBox).Checked;
        }

        private void LstJobs_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lstJobs = sender as ListBox;
            if (lstJobs.SelectedIndex >= 0 && lstJobs.SelectedIndex != _currentJobIndex)
            {
                SaveCurrentJobDetails();
                _currentJobIndex = lstJobs.SelectedIndex;
                LoadJobDetails();
            }
        }

        private void BtnAddJob_Click(object sender, EventArgs e)
        {
            SaveCurrentJobDetails();
            AddNewJob();
            RefreshJobsList();
            _currentJobIndex = _jobs.Count - 1;
            RefreshJobsList(); // Refresh again to select the new job
        }

        private void BtnRemoveJob_Click(object sender, EventArgs e)
        {
            if (_jobs.Count <= 1)
            {
                MessageBox.Show("You must have at least one job in a multi-job configuration.", "Cannot Remove", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_currentJobIndex >= 0 && _currentJobIndex < _jobs.Count)
            {
                _jobs.RemoveAt(_currentJobIndex);
                
                if (_currentJobIndex >= _jobs.Count)
                    _currentJobIndex = _jobs.Count - 1;
                
                RefreshJobsList();
            }
        }

        private void BtnMoveUp_Click(object sender, EventArgs e)
        {
            if (_currentJobIndex > 0)
            {
                SaveCurrentJobDetails();
                var job = _jobs[_currentJobIndex];
                _jobs.RemoveAt(_currentJobIndex);
                _jobs.Insert(_currentJobIndex - 1, job);
                _currentJobIndex--;
                RefreshJobsList();
            }
        }

        private void BtnMoveDown_Click(object sender, EventArgs e)
        {
            if (_currentJobIndex < _jobs.Count - 1)
            {
                SaveCurrentJobDetails();
                var job = _jobs[_currentJobIndex];
                _jobs.RemoveAt(_currentJobIndex);
                _jobs.Insert(_currentJobIndex + 1, job);
                _currentJobIndex++;
                RefreshJobsList();
            }
        }

        private void BtnBrowseSource_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select source folder";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var pnlJobDetails = this.Controls.Find("pnlJobDetails", false)[0] as Panel;
                    (pnlJobDetails.Controls.Find("txtSourcePath", false)[0] as TextBox).Text = dialog.SelectedPath;
                }
            }
        }

        private void BtnBrowseDest_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select destination folder";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var pnlJobDetails = this.Controls.Find("pnlJobDetails", false)[0] as Panel;
                    (pnlJobDetails.Controls.Find("txtDestPath", false)[0] as TextBox).Text = dialog.SelectedPath;
                }
            }
        }

        private void JobDetail_Changed(object sender, EventArgs e)
        {
            SaveCurrentJobDetails();
            RefreshJobsList(); // Update the display name in the list
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                SaveCurrentJobDetails();
                
                var txtConfigName = this.Controls.Find("txtConfigName", false)[0] as TextBox;
                var txtConfigDesc = this.Controls.Find("txtConfigDesc", false)[0] as TextBox;
                
                if (string.IsNullOrWhiteSpace(txtConfigName.Text))
                {
                    MessageBox.Show("Please enter a configuration name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfigName.Focus();
                    return;
                }
                
                // Validate that all jobs have source and destination paths
                var incompleteJobs = _jobs.Where(j => string.IsNullOrWhiteSpace(j.SourcePath) || string.IsNullOrWhiteSpace(j.DestinationPath)).ToList();
                if (incompleteJobs.Count > 0)
                {
                    MessageBox.Show($"Please fill in source and destination paths for all jobs. {incompleteJobs.Count} job(s) are incomplete.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Create the configuration
                var config = new SavedJobConfiguration()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = txtConfigName.Text.Trim(),
                    Description = txtConfigDesc.Text.Trim(),
                    CreatedDate = DateTime.Now,
                    Category = "Multi-Job",
                    Jobs = new List<SyncJob>(_jobs), // Create a copy
                    
                    // Get current connection settings
                    SourceConnection = new SavedConnection()
                    {
                        Name = $"{txtConfigName.Text.Trim()}_Source",
                        Description = $"Source connection for {txtConfigName.Text.Trim()}",
                        Settings = _connectionService.GetConnectionSettings(),
                        CreatedDate = DateTime.Now
                    },
                    DestinationConnection = new SavedConnection()
                    {
                        Name = $"{txtConfigName.Text.Trim()}_Destination", 
                        Description = $"Destination connection for {txtConfigName.Text.Trim()}",
                        Settings = _connectionService.GetConnectionSettings(),
                        CreatedDate = DateTime.Now
                    }
                };
                
                // Ensure Jobs compatibility
                config.EnsureJobsCompatibility();
                
                // Save the configuration
                if (_configService.SaveConfiguration(config))
                {
                    CreatedConfiguration = config;
                    
                    MessageBox.Show($"Multi-job configuration '{config.Name}' created successfully with {config.Jobs.Count} jobs!\n\nAll jobs will run in parallel when started.", 
                        "Configuration Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save the configuration. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
