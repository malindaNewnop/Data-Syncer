using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace syncer.ui
{
    public partial class FormMain : Form
    {
        private ISyncJobService _jobService;
        private IServiceManager _serviceManager;
        private IConnectionService _connectionService;

        public FormMain()
        {
            InitializeComponent();
            InitializeServices();
            InitializeCustomComponents();
        }

        private void InitializeServices()
        {
            _jobService = ServiceLocator.SyncJobService;
            _serviceManager = ServiceLocator.ServiceManager;
            _connectionService = ServiceLocator.ConnectionService;
        }

        private void InitializeCustomComponents()
        {
            // Set form properties
            this.Text = "DataSyncer - Main Dashboard";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);

            // Initialize DataGridView columns
            InitializeJobsGrid();

            // Update status from services
            UpdateServiceStatus();
            UpdateConnectionStatus();
            
            // Load actual jobs from service
            RefreshJobsGrid();
        }

        private void InitializeJobsGrid()
        {
            // Clear any existing columns
            dgvJobs.Columns.Clear();

            // Add columns
            dgvJobs.Columns.Add("JobName", "Job Name");
            dgvJobs.Columns.Add("Status", "Status");
            dgvJobs.Columns.Add("SourcePath", "Source Path");
            dgvJobs.Columns.Add("DestinationPath", "Destination Path");
            dgvJobs.Columns.Add("Schedule", "Schedule");
            dgvJobs.Columns.Add("LastRun", "Last Run");
            dgvJobs.Columns.Add("NextRun", "Next Run");

            // Set column widths
            dgvJobs.Columns["JobName"].Width = 120;
            dgvJobs.Columns["Status"].Width = 80;
            dgvJobs.Columns["SourcePath"].Width = 200;
            dgvJobs.Columns["DestinationPath"].Width = 200;
            dgvJobs.Columns["Schedule"].Width = 100;
            dgvJobs.Columns["LastRun"].Width = 130;
            dgvJobs.Columns["NextRun"].Width = 130;

            // Auto-resize the last column
            dgvJobs.Columns["NextRun"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void UpdateServiceStatus()
        {
            string status = _serviceManager.GetServiceStatus();
            if (lblServiceStatus != null)
            {
                lblServiceStatus.Text = "Service: " + status;
                lblServiceStatus.ForeColor = status == "Running" ? Color.Green : Color.Red;
                
                // Update button state
                if (btnStartStop != null)
                {
                    if (status == "Running")
                    {
                        btnStartStop.Text = "Stop Service";
                        btnStartStop.BackColor = Color.LightCoral;
                    }
                    else
                    {
                        btnStartStop.Text = "Start Service";
                        btnStartStop.BackColor = Color.LightGreen;
                    }
                }
            }
        }

        private void UpdateConnectionStatus()
        {
            bool isConnected = _connectionService.IsConnected();
            string status = isConnected ? "Connected" : "Disconnected";
            
            if (lblConnectionStatus != null)
            {
                lblConnectionStatus.Text = "Connection: " + status;
                lblConnectionStatus.ForeColor = isConnected ? Color.Green : Color.Red;
            }
        }

        // Menu event handlers
        private void connectionSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormConnection connForm = new FormConnection();
            if (connForm.ShowDialog() == DialogResult.OK)
            {
                UpdateConnectionStatus();
            }
        }

        private void scheduleSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSchedule scheduleForm = new FormSchedule();
            if (scheduleForm.ShowDialog() == DialogResult.OK)
            {
                RefreshJobsGrid();
            }
        }

        private void filterSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormFilters filtersForm = new FormFilters();
            filtersForm.ShowDialog();
        }

        private void viewLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormLogs logsForm = new FormLogs();
            logsForm.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("DataSyncer v1.0\nFile Synchronization Tool\n\nDeveloped for automated file transfers.", 
                           "About DataSyncer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Button event handlers
        private void btnAddJob_Click(object sender, EventArgs e)
        {
            FormSchedule scheduleForm = new FormSchedule();
            if (scheduleForm.ShowDialog() == DialogResult.OK)
            {
                RefreshJobsGrid();
            }
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (_serviceManager.IsServiceRunning())
                {
                    if (_serviceManager.StopService())
                    {
                        ServiceLocator.LogService.LogInfo("Service stopped by user");
                    }
                    else
                    {
                        MessageBox.Show("Failed to stop service", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    if (_serviceManager.StartService())
                    {
                        ServiceLocator.LogService.LogInfo("Service started by user");
                    }
                    else
                    {
                        MessageBox.Show("Failed to start service", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                
                UpdateServiceStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error managing service: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error managing service: " + ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshJobsGrid();
            UpdateServiceStatus();
            UpdateConnectionStatus();
        }

        private void RefreshJobsGrid()
        {
            try
            {
                dgvJobs.Rows.Clear();
                
                var jobs = _jobService.GetAllJobs();
                foreach (var job in jobs)
                {
                    string schedule = $"Every {job.IntervalValue} {job.IntervalType}";
                    string lastRun = job.LastRun?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
                    string nextRun = job.GetNextRunTime();
                    string status = job.IsEnabled ? "Enabled" : "Disabled";

                    dgvJobs.Rows.Add(
                        job.JobName,
                        status,
                        job.SourcePath,
                        job.DestinationPath,
                        schedule,
                        lastRun,
                        nextRun
                    );
                    
                    // Store job ID in the Tag property for later reference
                    dgvJobs.Rows[dgvJobs.Rows.Count - 1].Tag = job.Id;
                }
                
                ServiceLocator.LogService.LogInfo($"Jobs grid refreshed. Found {jobs.Count} jobs.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading jobs: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error loading jobs: " + ex.Message);
            }
        }

        private void dgvJobs_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    int jobId = (int)dgvJobs.Rows[e.RowIndex].Tag;
                    var job = _jobService.GetJobById(jobId);
                    
                    if (job != null)
                    {
                        FormSchedule scheduleForm = new FormSchedule(job);
                        if (scheduleForm.ShowDialog() == DialogResult.OK)
                        {
                            RefreshJobsGrid();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening job for editing: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ServiceLocator.LogService.LogError("Error opening job for editing: " + ex.Message);
                }
            }
        }
    }
}
