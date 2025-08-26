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
            // Set default values for combo boxes
            if (cmbLogLevel != null && cmbLogLevel.Items.Count > 0)
            {
                cmbLogLevel.SelectedIndex = 0; // Select "All" by default
            }

            if (cmbJobs != null && cmbJobs.Items.Count > 0)
            {
                cmbJobs.SelectedIndex = 0; // Select "All Jobs" by default
            }

            // Set default values for date time pickers
            dtpFrom.Value = DateTime.Today.AddDays(-7); // Default to 7 days ago
            dtpTo.Value = DateTime.Now; // Default to current date/time
            chkEnableTimeFilter.Checked = true; // Default to enabled time filtering

            // Initialize log data
            InitializeLogData();
        }

        private void FormLogs_Load(object sender, EventArgs e)
        {
            // Additional initialization when form loads
            UpdateLastUpdatedLabel();
        }

        /// <summary>
        /// Apply all filters (search text, log level, job, and time range) to the logs
        /// </summary>
        private void ApplyFilters()
        {
            try
            {
                if (dgvLogs == null || dgvLogs.DataSource == null)
                    return;

                DataTable dt = dgvLogs.DataSource as DataTable;
                if (dt == null)
                    return;

                // Build filter string
                string filterExpression = string.Empty;

                // Text search filter
                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    string searchTerm = txtSearch.Text.Replace("'", "''"); // Escape single quotes

                    // Search in Message column
                    if (dt.Columns.Contains("Message"))
                    {
                        filterExpression += string.IsNullOrEmpty(filterExpression) ?
                            $"Message LIKE '%{searchTerm}%'" :
                            $" AND Message LIKE '%{searchTerm}%'";
                    }

                    // Also search in JobId column
                    if (dt.Columns.Contains("JobId"))
                    {
                        filterExpression += string.IsNullOrEmpty(filterExpression) ?
                            $"JobId LIKE '%{searchTerm}%'" :
                            $" OR JobId LIKE '%{searchTerm}%'";
                    }
                }

                // Log level filter
                if (cmbLogLevel.SelectedIndex > 0) // Not "All"
                {
                    string selectedLevel = cmbLogLevel.SelectedItem.ToString();

                    if (dt.Columns.Contains("Level"))
                    {
                        string levelFilter = $"Level = '{selectedLevel}'";

                        filterExpression = string.IsNullOrEmpty(filterExpression) ?
                            levelFilter : $"({filterExpression}) AND {levelFilter}";
                    }
                }

                // JobId filter
                if (cmbJobs.SelectedIndex > 0) // Not "All Jobs"
                {
                    string selectedJob = cmbJobs.SelectedItem.ToString();

                    if (dt.Columns.Contains("JobId"))
                    {
                        string jobFilter = $"JobId = '{selectedJob}'";

                        filterExpression = string.IsNullOrEmpty(filterExpression) ?
                            jobFilter : $"({filterExpression}) AND {jobFilter}";
                    }
                }

                // Time filter
                if (chkEnableTimeFilter.Checked && dt.Columns.Contains("DateTime"))
                {
                    DateTime fromDate = dtpFrom.Value.Date;
                    DateTime toDate = dtpTo.Value.Date.AddDays(1).AddSeconds(-1); // End of day

                    string timeFilter = $"DateTime >= #{fromDate.ToString("yyyy-MM-dd HH:mm:ss")}# AND DateTime <= #{toDate.ToString("yyyy-MM-dd HH:mm:ss")}#";

                    filterExpression = string.IsNullOrEmpty(filterExpression) ?
                        timeFilter : $"({filterExpression}) AND {timeFilter}";
                }

                // Apply the filter
                dt.DefaultView.RowFilter = filterExpression;

                // Update the log count
                UpdateLogCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying filters: " + ex.Message, "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Update the Last Updated label with the current date/time
        /// </summary>
        private void UpdateLastUpdatedLabel()
        {
            lblLastUpdated.Text = "Last Updated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Event handler for Job ComboBox selection change
        /// </summary>
        private void cmbJobs_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering by job: " + ex.Message, "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Event handler for Enable Time Filter checkbox
        /// </summary>
        private void chkEnableTimeFilter_CheckedChanged(object sender, EventArgs e)
        {
            dtpFrom.Enabled = chkEnableTimeFilter.Checked;
            dtpTo.Enabled = chkEnableTimeFilter.Checked;
            ApplyFilters();
        }

        /// <summary>
        /// Event handler for From date selection change
        /// </summary>
        private void dtpFrom_ValueChanged(object sender, EventArgs e)
        {
            if (chkEnableTimeFilter.Checked)
            {
                ApplyFilters();
            }
        }

        /// <summary>
        /// Event handler for To date selection change
        /// </summary>
        private void dtpTo_ValueChanged(object sender, EventArgs e)
        {
            if (chkEnableTimeFilter.Checked)
            {
                ApplyFilters();
            }
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
                
                // Ensure DateTime column has the right type and format
                if (_logsDataTable != null && _logsDataTable.Columns.Contains("DateTime"))
                {
                    // Make sure the column is of DateTime type
                    if (_logsDataTable.Columns["DateTime"].DataType != typeof(DateTime))
                    {
                        // Create a new DateTime column if needed
                        DataColumn newDateTimeCol = new DataColumn("TempDateTime", typeof(DateTime));
                        _logsDataTable.Columns.Add(newDateTimeCol);
                        
                        // Convert string values to DateTime
                        foreach (DataRow row in _logsDataTable.Rows)
                        {
                            try
                            {
                                if (row["DateTime"] != DBNull.Value)
                                {
                                    DateTime dt;
                                    if (DateTime.TryParse(row["DateTime"].ToString(), out dt))
                                    {
                                        row["TempDateTime"] = dt;
                                    }
                                }
                            }
                            catch
                            {
                                // Ignore conversion errors for individual rows
                            }
                        }
                        
                        // Replace original column
                        _logsDataTable.Columns.Remove("DateTime");
                        _logsDataTable.Columns["TempDateTime"].ColumnName = "DateTime";
                    }
                }

                // Bind to DataGridView
                if (dgvLogs != null && _logsDataTable != null)
                {
                    dgvLogs.DataSource = _logsDataTable;

                    // Use a timer to configure columns after a short delay
                    var timer = new Timer();
                    timer.Interval = 100; // 100ms delay
                    timer.Tick += (s, timerArgs) =>
                    {
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

                // First, hide all columns that we don't need
                foreach (DataGridViewColumn col in dgvLogs.Columns)
                {
                    if (col.Name != "DateTime" && col.Name != "Level" && 
                        col.Name != "JobId" && col.Name != "Message")
                    {
                        col.Visible = false;
                    }
                    else
                    {
                        col.Visible = true; // Ensure the desired columns are visible
                    }
                }

                // Use actual column names from UI LogService
                // We only want these columns: Timestamp, Level, JobId, Message

                // Configure DateTime column
                try
                {
                    if (dgvLogs.Columns.Contains("DateTime"))
                    {
                        var dateTimeColumn = dgvLogs.Columns["DateTime"];
                        if (dateTimeColumn != null)
                        {
                            // Force visible timestamp column 
                            dateTimeColumn.HeaderText = "Timestamp";
                            dateTimeColumn.Width = 140;
                            dateTimeColumn.DisplayIndex = 0; // First column
                            dateTimeColumn.Visible = true; // Make sure timestamp is visible
                            dateTimeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // Prevent auto-sizing
                            dateTimeColumn.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm"; // Format to match screenshot
                            dateTimeColumn.DefaultCellStyle.NullValue = DateTime.Now.ToString("dd/MM/yyyy HH:mm"); // Default value for nulls
                            
                            // Add custom cell formatting for DateTime column to ensure proper display
                            dgvLogs.CellFormatting += (sender, e) => {
                                if (e.ColumnIndex == dateTimeColumn.Index && e.Value != null)
                                {
                                    if (e.Value is DateTime)
                                    {
                                        e.Value = ((DateTime)e.Value).ToString("dd/MM/yyyy HH:mm");
                                        e.FormattingApplied = true;
                                    }
                                }
                            };
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
                            levelColumn.HeaderText = "Level";
                            levelColumn.Width = 70;
                            levelColumn.DisplayIndex = 1; // Second column
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
                            jobColumn.Width = 70;
                            jobColumn.DisplayIndex = 2; // Third column
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error configuring Job column: " + ex.Message, "UI");
                }

                // Configure JobId column
                try
                {
                    if (dgvLogs.Columns.Contains("JobId"))
                    {
                        var jobIdColumn = dgvLogs.Columns["JobId"];
                        if (jobIdColumn != null)
                        {
                            jobIdColumn.HeaderText = "JobId";
                            jobIdColumn.Width = 70;
                            jobIdColumn.DisplayIndex = 3; // Fourth column
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error configuring JobId column: " + ex.Message, "UI");
                }

                // Configure Status column
                try
                {
                    if (dgvLogs.Columns.Contains("Status"))
                    {
                        var statusColumn = dgvLogs.Columns["Status"];
                        if (statusColumn != null)
                        {
                            statusColumn.HeaderText = "Status";
                            statusColumn.Width = 80;
                            statusColumn.DisplayIndex = 4; // Fifth column
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
                            messageColumn.HeaderText = "Message";
                            messageColumn.Width = 400;
                            messageColumn.DisplayIndex = 5; // Last column
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
                
                // Create a DateTime column if it doesn't exist or ensure it has the correct format
                if (_logsDataTable != null)
                {
                    // Check if DateTime column exists
                    bool hasDateTimeColumn = _logsDataTable.Columns.Contains("DateTime");
                    
                    // If no DateTime column, add one
                    if (!hasDateTimeColumn)
                    {
                        DataColumn dateTimeCol = new DataColumn("DateTime", typeof(DateTime));
                        _logsDataTable.Columns.Add(dateTimeCol);
                        
                        // Add default values (current timestamp) for missing data
                        foreach (DataRow row in _logsDataTable.Rows)
                        {
                            row["DateTime"] = DateTime.Now;
                        }
                    }
                    // If DateTime column exists but has wrong type
                    else if (_logsDataTable.Columns["DateTime"].DataType != typeof(DateTime))
                    {
                        // Create a new properly typed DateTime column
                        DataColumn newDateTimeCol = new DataColumn("TempDateTime", typeof(DateTime));
                        _logsDataTable.Columns.Add(newDateTimeCol);
                        
                        // Convert string values to DateTime
                        foreach (DataRow row in _logsDataTable.Rows)
                        {
                            try
                            {
                                if (row["DateTime"] != DBNull.Value)
                                {
                                    DateTime dt;
                                    if (DateTime.TryParse(row["DateTime"].ToString(), out dt))
                                    {
                                        row["TempDateTime"] = dt;
                                    }
                                    else
                                    {
                                        // Use current time if parsing fails
                                        row["TempDateTime"] = DateTime.Now;
                                    }
                                }
                                else
                                {
                                    // Use current time if value is null
                                    row["TempDateTime"] = DateTime.Now;
                                }
                            }
                            catch
                            {
                                // Use current time if any error occurs
                                try { row["TempDateTime"] = DateTime.Now; } catch { }
                            }
                        }
                        
                        // Replace original column
                        _logsDataTable.Columns.Remove("DateTime");
                        _logsDataTable.Columns["TempDateTime"].ColumnName = "DateTime";
                    }
                    
                    // Ensure DateTime column is at the beginning
                    _logsDataTable.Columns["DateTime"].SetOrdinal(0);
                }

                if (dgvLogs != null && _logsDataTable != null)
                {
                    // Clear any existing data binding
                    dgvLogs.DataSource = null;
                    
                    // Create a DataView with DateTime sorting enabled
                    DataView dv = new DataView(_logsDataTable);
                    dv.Sort = "DateTime DESC"; // Sort by datetime descending (newest first)
                    
                    // Set new data source
                    dgvLogs.DataSource = dv;

                    // Use a timer to configure columns after a short delay
                    var timer = new Timer();
                    timer.Interval = 200; // 200ms delay to ensure proper rendering
                    timer.Tick += (s, timerArgs) =>
                    {
                        timer.Stop();

                        // Configure columns first
                        ConfigureLogColumns();
                        
                        // After loading, apply any filters
                        ApplyFilters();

                        // Update the last updated timestamp
                        UpdateLastUpdatedLabel();
                        timer.Dispose();
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
            UpdateLastUpdatedLabel();
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
                    timer.Tick += (s, timerArgs) =>
                    {
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
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            try
            {
                // Special handling for DateTime column
                if (dgvLogs.Columns[e.ColumnIndex].Name == "DateTime")
                {
                    try
                    {
                        // Handle null values
                        if (e.Value == null || e.Value == DBNull.Value)
                        {
                            e.Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                            e.FormattingApplied = true;
                            return;
                        }
                        
                        // Handle DateTime values
                        if (e.Value is DateTime)
                        {
                            // Format datetime value to match the screenshot (dd/MM/yyyy HH:mm)
                            DateTime dateValue = (DateTime)e.Value;
                            e.Value = dateValue.ToString("dd/MM/yyyy HH:mm");
                            e.FormattingApplied = true;
                        }
                        // Handle string values
                        else if (e.Value is string)
                        {
                            // Try to parse string to DateTime
                            DateTime dateValue;
                            if (DateTime.TryParse(e.Value.ToString(), out dateValue))
                            {
                                e.Value = dateValue.ToString("dd/MM/yyyy HH:mm");
                                e.FormattingApplied = true;
                            }
                            // If can't parse, still display something
                            else if (!string.IsNullOrEmpty(e.Value.ToString()))
                            {
                                // Keep original string
                                e.FormattingApplied = false;
                            }
                            else
                            {
                                // Use current date for empty strings
                                e.Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                                e.FormattingApplied = true;
                            }
                        }
                        // Handle any other type
                        else
                        {
                            e.Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                            e.FormattingApplied = true;
                        }
                    }
                    catch
                    {
                        // If there's an error formatting, use current date
                        e.Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                        e.FormattingApplied = true;
                    }
                }
                
                // Get log level for the current row
                DataGridViewRow row = dgvLogs.Rows[e.RowIndex];
                string logLevel = string.Empty;

                if (row.Cells["Level"].Value != null)
                {
                    logLevel = row.Cells["Level"].Value.ToString();
                }

                // Format entire row based on log level
                if (logLevel == "Error")
                {
                    // Format Error rows with red background and white text
                    e.CellStyle.BackColor = Color.IndianRed;
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.SelectionBackColor = Color.DarkRed;
                    e.CellStyle.SelectionForeColor = Color.White;
                }
                else if (logLevel == "Info")
                {
                    // Info rows use default style
                    e.CellStyle.BackColor = Color.White;
                    e.CellStyle.ForeColor = Color.Black;
                }

                // Highlight the first row if selected
                if (e.RowIndex == 0 && dgvLogs.Rows[e.RowIndex].Selected)
                {
                    e.CellStyle.SelectionBackColor = Color.RoyalBlue;
                    e.CellStyle.SelectionForeColor = Color.White;
                }
            }
            catch (Exception)
            {
                // Silently handle formatting errors
            }
        }

        /// <summary>
        /// Event handler for the Close button click
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Event handler for keypress events in the search box
        /// </summary>
        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            // If Enter key pressed
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                ApplyFilters();
            }
        }
    }
}
   
