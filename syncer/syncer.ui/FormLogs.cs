using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace syncer.ui
{
    public partial class FormLogs : Form
    {
        private ILogService _logService;
        private DataTable _logsDataTable;

        public FormLogs()
        {
            InitializeComponent();
            InitializeServices();
            InitializeCustomComponents();
        }

        private void InitializeServices()
        {
            try
            {
                _logService = ServiceLocator.LogService;
                if (_logService == null)
                {
                    throw new InvalidOperationException("LogService is not available from ServiceLocator");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing services: " + ex.Message, "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // Re-throw to prevent further execution with null service
            }
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Log Viewer";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Set default value for log level combo box
            if (cmbLogLevel != null && cmbLogLevel.Items.Count > 0)
            {
                cmbLogLevel.SelectedIndex = 0; // Select "All" by default
            }

            InitializeLogData();
        }

        private void InitializeLogData()
        {
            try
            {
                // Ensure LogService is available
                if (_logService == null)
                {
                    MessageBox.Show("Log service is not available.", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Get logs from service (UI interface doesn't need parameters)
                _logsDataTable = _logService.GetLogs();

                // Bind to DataGridView
                if (dgvLogs != null && _logsDataTable != null)
                {
                    dgvLogs.DataSource = _logsDataTable;

                    // Use a timer to configure columns after a short delay
                    var timer = new Timer();
                    timer.Interval = 100; // 100ms delay
                    timer.Tick += (s, timerArgs) => {
                        timer.Stop();
                        timer.Dispose();
                        ConfigureLogColumns();
                    };
                    timer.Start();

                    // Force the DataGridView to process the data binding
                    dgvLogs.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing log data: " + ex.Message, "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Use UI interface method
                if (_logService != null)
                {
                    _logService.LogError("Error initializing log data in FormLogs: " + ex.Message, "UI");
                }
            }
        }

        private void ConfigureLogColumns()
        {
            try
            {
                if (dgvLogs == null || dgvLogs.Columns == null || dgvLogs.Columns.Count == 0)
                {
                    return;
                }

                // Add a small delay to ensure DataGridView has fully processed the binding
                Application.DoEvents();

                // Use actual column names from UI LogService
                // Columns: DateTime, Level, Job, File, Status, Message

                // Configure DateTime column
                try
                {
                    if (dgvLogs.Columns.Contains("DateTime"))
                    {
                        var dateTimeColumn = dgvLogs.Columns["DateTime"];
                        if (dateTimeColumn != null)
                        {
                            dateTimeColumn.HeaderText = "Date/Time";
                            dateTimeColumn.Width = 150;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error configuring DateTime column: " + ex.Message, "UI");
                }

                // Configure Level column
                try
                {
                    if (dgvLogs.Columns.Contains("Level"))
                    {
                        var levelColumn = dgvLogs.Columns["Level"];
                        if (levelColumn != null)
                        {
                            levelColumn.Width = 70;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error configuring Level column: " + ex.Message, "UI");
                }

                // Configure Job column
                try
                {
                    if (dgvLogs.Columns.Contains("Job"))
                    {
                        var jobColumn = dgvLogs.Columns["Job"];
                        if (jobColumn != null)
                        {
                            jobColumn.HeaderText = "Job";
                            jobColumn.Width = 100;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error configuring Job column: " + ex.Message, "UI");
                }

                // Configure File column
                try
                {
                    if (dgvLogs.Columns.Contains("File"))
                    {
                        var fileColumn = dgvLogs.Columns["File"];
                        if (fileColumn != null)
                        {
                            fileColumn.HeaderText = "File";
                            fileColumn.Width = 200;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error configuring File column: " + ex.Message, "UI");
                }

                // Configure Status column
                try
                {
                    if (dgvLogs.Columns.Contains("Status"))
                    {
                        var statusColumn = dgvLogs.Columns["Status"];
                        if (statusColumn != null)
                        {
                            statusColumn.Width = 80;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error configuring Status column: " + ex.Message, "UI");
                }

                // Configure Message column with extra safety
                try
                {
                    if (dgvLogs.Columns.Contains("Message"))
                    {
                        var messageColumn = dgvLogs.Columns["Message"];
                        if (messageColumn != null)
                        {
                            messageColumn.Width = 250;
                            // Extra safety check before setting AutoSizeMode
                            if (messageColumn != null && dgvLogs.IsHandleCreated)
                            {
                                messageColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error configuring Message column: " + ex.Message, "UI");
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't show a message box to avoid spamming the user
                if (_logService != null)
                {
                    _logService.LogError("Error configuring log columns: " + ex.Message, "UI");
                }
            }
        }

        private void LoadLogs()
        {
            try
            {
                if (_logService == null)
                {
                    MessageBox.Show("Log service is not initialized.", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Load logs from service (UI interface)
                _logsDataTable = _logService.GetLogs();

                if (dgvLogs != null && _logsDataTable != null)
                {
                    dgvLogs.DataSource = _logsDataTable;

                    // Use a timer to configure columns after a short delay
                    var timer = new Timer();
                    timer.Interval = 100; // 100ms delay
                    timer.Tick += (s, timerArgs) => {
                        timer.Stop();
                        timer.Dispose();
                        ConfigureLogColumns();
                    };
                    timer.Start();

                    // Force the DataGridView to process the data binding
                    dgvLogs.Refresh();
                }

                UpdateLogCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading logs: " + ex.Message, "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Use UI interface method
                if (_logService != null)
                {
                    _logService.LogError("Error loading logs in FormLogs: " + ex.Message, "UI");
                }
            }
        }

        private void UpdateLogCount()
        {
            if (lblLogCount != null && _logsDataTable != null)
            {
                lblLogCount.Text = "Total Logs: " + _logsDataTable.Rows.Count;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string searchText = txtSearch != null ? txtSearch.Text.Trim() : "";

                if (UIStringExtensions.IsNullOrWhiteSpace(searchText))
                {
                    // Clear filter
                    DataTable dt = dgvLogs != null ? dgvLogs.DataSource as DataTable : null;
                    if (dt != null)
                    {
                        dt.DefaultView.RowFilter = string.Empty;
                    }
                }
                else
                {
                    // Apply filter - use correct column names from UI LogService
                    // Columns: DateTime, Level, Job, File, Status, Message
                    string safe = searchText.Replace("'", "''");
                    string filter = "Job LIKE '%" + safe + "%' OR File LIKE '%" + safe + "%' OR Message LIKE '%" + safe + "%'";
                    DataTable dt = dgvLogs != null ? dgvLogs.DataSource as DataTable : null;
                    if (dt != null)
                    {
                        dt.DefaultView.RowFilter = filter;
                    }
                }

                UpdateLogCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching logs: " + ex.Message, "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            if (txtSearch != null)
            {
                txtSearch.Text = string.Empty;
            }

            DataTable dt = dgvLogs != null ? dgvLogs.DataSource as DataTable : null;
            if (dt != null)
            {
                dt.DefaultView.RowFilter = string.Empty;
            }

            UpdateLogCount();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadLogs();
        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to clear all logs? This action cannot be undone.",
                "Confirm Clear Logs", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _logService.ClearLogs(); // Use the UI interface method
                    LoadLogs(); // Refresh the display
                    MessageBox.Show("Logs cleared successfully.", "Success",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _logService.LogInfo("Log history cleared by user", "UI");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error clearing logs: " + ex.Message, "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt";
                dialog.DefaultExt = "csv";
                dialog.FileName = "DataSyncer_Logs_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Use UI interface export method
                        if (_logService.ExportLogs(dialog.FileName))
                        {
                            MessageBox.Show("Logs exported successfully to:\n" + dialog.FileName, "Export Complete",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            _logService.LogInfo("Logs exported to: " + dialog.FileName, "UI");
                        }
                        else
                        {
                            MessageBox.Show("Failed to export logs.", "Export Error",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error exporting logs: " + ex.Message, "Export Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _logService.LogError("Error exporting logs: " + ex.Message, "UI");
                    }
                }
            }
        }

        private void cmbLogLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_logService == null)
                {
                    return;
                }

                string selectedLevel = (cmbLogLevel != null && cmbLogLevel.SelectedItem != null) ? cmbLogLevel.SelectedItem.ToString() : null;

                if (UIStringExtensions.IsNullOrWhiteSpace(selectedLevel) || selectedLevel == "All")
                {
                    // Load all logs using UI interface
                    _logsDataTable = _logService.GetLogs();
                }
                else
                {
                    // Filter by log level using UI interface
                    DateTime? from = DateTime.Now.AddDays(-30);
                    DateTime? to = DateTime.Now;
                    _logsDataTable = _logService.GetLogs(from, to, selectedLevel);
                }

                if (dgvLogs != null && _logsDataTable != null)
                {
                    dgvLogs.DataSource = _logsDataTable;

                    // Use a timer to configure columns after a short delay
                    var timer = new Timer();
                    timer.Interval = 100; // 100ms delay
                    timer.Tick += (s, timerArgs) => {
                        timer.Stop();
                        timer.Dispose();
                        ConfigureLogColumns();
                    };
                    timer.Start();

                    // Force the DataGridView to process the data binding
                    dgvLogs.Refresh();
                }

                UpdateLogCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering logs: " + ex.Message, "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvLogs_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvLogs != null && dgvLogs.Columns != null && e.ColumnIndex >= 0 && e.ColumnIndex < dgvLogs.Columns.Count)
            {
                if (dgvLogs.Columns[e.ColumnIndex].Name == "Level")
                {
                    string level = e.Value != null ? e.Value.ToString() : null;
                    if (level == "ERROR")
                    {
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                    }
                    else if (level == "WARNING")
                    {
                        e.CellStyle.ForeColor = Color.Orange;
                    }
                    else if (level == "INFO")
                    {
                        e.CellStyle.ForeColor = Color.Blue;
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.Black;
                    }
                }
                else if (dgvLogs.Columns[e.ColumnIndex].Name == "Status")
                {
                    string status = e.Value != null ? e.Value.ToString() : null;
                    if (status == "Success")
                    {
                        e.CellStyle.ForeColor = Color.Green;
                    }
                    else if (status == "Failed")
                    {
                        e.CellStyle.ForeColor = Color.Red;
                    }
                    else if (status == "Skipped")
                    {
                        e.CellStyle.ForeColor = Color.Orange;
                    }
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnSearch_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
