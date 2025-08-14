using System;
using System.Drawing;
using System.Windows.Forms;

namespace syncer.ui
{
    public partial class FormMain : Form
    {
        private ISyncJobService _jobService;
        private IServiceManager _serviceManager;
        private IConnectionService _connectionService;

        public FormMain()
        {
            try
            {
                InitializeComponent();
                InitializeServices();
                InitializeCustomComponents();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing main form: " + ex.Message + "\n\nThe application may not function correctly.",
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeServices()
        {
            try
            {
                _jobService = ServiceLocator.SyncJobService;
                _serviceManager = ServiceLocator.ServiceManager;
                _connectionService = ServiceLocator.ConnectionService;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize services: " + ex.Message + "\n\nSome functionality may be limited.",
                    "Service Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InitializeCustomComponents()
        {
            this.Text = "DataSyncer - Main Dashboard";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            InitializeJobsGrid();
            UpdateServiceStatus();
            UpdateConnectionStatus();
            RefreshJobsGrid();
        }

        private void InitializeJobsGrid()
        {
            dgvJobs.Columns.Clear();
            dgvJobs.Columns.Add("JobName", "Job Name");
            dgvJobs.Columns.Add("Status", "Status");
            dgvJobs.Columns.Add("SourcePath", "Source Path");
            dgvJobs.Columns.Add("DestinationPath", "Destination Path");
            dgvJobs.Columns.Add("Schedule", "Schedule");
            dgvJobs.Columns.Add("LastRun", "Last Run");
            dgvJobs.Columns.Add("NextRun", "Next Run");
            dgvJobs.Columns["JobName"].Width = 120;
            dgvJobs.Columns["Status"].Width = 80;
            dgvJobs.Columns["SourcePath"].Width = 200;
            dgvJobs.Columns["DestinationPath"].Width = 200;
            dgvJobs.Columns["Schedule"].Width = 100;
            dgvJobs.Columns["LastRun"].Width = 130;
            dgvJobs.Columns["NextRun"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void UpdateServiceStatus()
        {
            try
            {
                string status = _serviceManager != null ? _serviceManager.GetServiceStatus() : "Unknown";
                lblServiceStatus.Text = "Service: " + status;
                lblServiceStatus.ForeColor = status == "Running" ? Color.Green : Color.Red;
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
            catch (Exception)
            {
                lblServiceStatus.Text = "Service: Unknown";
                lblServiceStatus.ForeColor = Color.Gray;
                btnStartStop.Text = "Start Service";
                btnStartStop.BackColor = Color.LightGray;
                btnStartStop.Enabled = false;
            }
        }

        private void UpdateConnectionStatus()
        {
            try
            {
                bool isConnected = _connectionService != null && _connectionService.IsConnected();
                ConnectionSettings connectionSettings = _connectionService != null ? _connectionService.GetConnectionSettings() : null;
                string status = isConnected ? "Connected (" + (connectionSettings != null ? connectionSettings.ConnectionTypeDisplay : "Unknown") + ")" : "Disconnected";
                if (!isConnected && connectionSettings != null)
                {
                    status += " (" + connectionSettings.ConnectionTypeDisplay + ")";
                }
                lblConnectionStatus.Text = "Connection: " + status;
                lblConnectionStatus.ForeColor = isConnected ? Color.Green : Color.Red;
            }
            catch (Exception)
            {
                lblConnectionStatus.Text = "Connection: Unknown";
                lblConnectionStatus.ForeColor = Color.Gray;
            }
        }

        private void connectionSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormConnection connForm = new FormConnection())
            {
                if (connForm.ShowDialog() == DialogResult.OK)
                {
                    UpdateConnectionStatus();
                }
            }
        }

        private void scheduleSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormSchedule scheduleForm = new FormSchedule())
            {
                if (scheduleForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshJobsGrid();
                }
            }
        }

        private void filterSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormFilters filtersForm = new FormFilters())
            {
                filtersForm.ShowDialog();
            }
        }

        private void viewLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormLogs logsForm = new FormLogs())
            {
                logsForm.ShowDialog();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("DataSyncer v1.0\nFile Synchronization Tool\n\nDeveloped for automated file transfers.", "About DataSyncer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void testBackendConnectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Test the backend connection using our connector class
            BackendConnector.TestConnection();
        }

        private void btnAddJob_Click(object sender, EventArgs e)
        {
            using (FormSchedule scheduleForm = new FormSchedule())
            {
                if (scheduleForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshJobsGrid();
                }
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
                System.Collections.Generic.List<SyncJob> jobs = null;
                
                try 
                {
                    jobs = _jobService.GetAllJobs();
                }
                catch (Exception ex)
                {
                    // Handle the specific case where jobs can't be loaded
                    MessageBox.Show("Cannot load jobs: " + ex.Message + "\n\nThe application will continue with an empty job list.", 
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    jobs = new System.Collections.Generic.List<SyncJob>();
                }
                
                for (int i = 0; i < jobs.Count; i++)
                {
                    SyncJob job = jobs[i];
                    string schedule = "Every " + job.IntervalValue + " " + job.IntervalType;
                    string lastRun = job.LastRun.HasValue ? job.LastRun.Value.ToString("yyyy-MM-dd HH:mm") : "Never";
                    string nextRun = job.GetNextRunTime();
                    string status = job.IsEnabled ? "Enabled" : "Disabled";
                    int rowIndex = dgvJobs.Rows.Add(new object[] { job.Name, status, job.SourcePath, job.DestinationPath, schedule, lastRun, nextRun });
                    dgvJobs.Rows[rowIndex].Tag = job.Id;
                }
                
                try
                {
                    ServiceLocator.LogService.LogInfo("Jobs grid refreshed. Found " + jobs.Count + " jobs.");
                }
                catch 
                {
                    // Ignore logging errors
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading jobs: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try 
                {
                    ServiceLocator.LogService.LogError("Error loading jobs: " + ex.Message);
                }
                catch 
                {
                    // Ignore logging errors
                }
            }
        }

        private void dgvJobs_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    int jobId = (int)dgvJobs.Rows[e.RowIndex].Tag;
                    SyncJob job = _jobService.GetJobById(jobId);
                    if (job != null)
                    {
                        using (FormSchedule scheduleForm = new FormSchedule(job))
                        {
                            if (scheduleForm.ShowDialog() == DialogResult.OK)
                            {
                                RefreshJobsGrid();
                            }
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
