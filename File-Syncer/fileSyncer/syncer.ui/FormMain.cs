using System;
using System.Drawing;
using System.Windows.Forms;

namespace syncer.ui
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            InitializeCustomComponents();
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

            // Update status
            UpdateServiceStatus("Stopped");
            UpdateConnectionStatus("Disconnected");
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

            // Add some sample data
            AddSampleJobs();
        }

        private void AddSampleJobs()
        {
            // Add sample job data for demonstration
            dgvJobs.Rows.Add("Daily Backup", "Enabled", @"C:\Documents", "/backup/documents", "Every 24 Hours", "2024-08-11 09:00", "2024-08-12 09:00");
            dgvJobs.Rows.Add("Hourly Sync", "Enabled", @"C:\Projects", "/sync/projects", "Every 1 Hours", "2024-08-12 11:00", "2024-08-12 12:00");
            dgvJobs.Rows.Add("Weekly Reports", "Disabled", @"C:\Reports", "/archive/reports", "Every 7 Days", "Never", "Not Scheduled");
        }

        private void UpdateServiceStatus(string status)
        {
            if (lblServiceStatus != null)
            {
                lblServiceStatus.Text = "Service: " + status;
                lblServiceStatus.ForeColor = status == "Running" ? Color.Green : Color.Red;
            }
        }

        private void UpdateConnectionStatus(string status)
        {
            if (lblConnectionStatus != null)
            {
                lblConnectionStatus.Text = "Connection: " + status;
                lblConnectionStatus.ForeColor = status == "Connected" ? Color.Green : Color.Red;
            }
        }

        // Menu event handlers
        private void connectionSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormConnection connForm = new FormConnection();
            connForm.ShowDialog();
        }

        private void scheduleSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSchedule scheduleForm = new FormSchedule();
            scheduleForm.ShowDialog();
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
                // Refresh the jobs grid
                RefreshJobsGrid();
            }
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            // Toggle service state
            if (btnStartStop.Text == "Start Service")
            {
                btnStartStop.Text = "Stop Service";
                btnStartStop.BackColor = Color.LightCoral;
                UpdateServiceStatus("Running");
            }
            else
            {
                btnStartStop.Text = "Start Service";
                btnStartStop.BackColor = Color.LightGreen;
                UpdateServiceStatus("Stopped");
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshJobsGrid();
        }

        private void RefreshJobsGrid()
        {
            // TODO: Implement job refresh logic
            // This will later load from saved configurations
        }

        private void dgvJobs_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Open edit dialog for selected job
                FormSchedule scheduleForm = new FormSchedule();
                scheduleForm.ShowDialog();
            }
        }
    }
}
