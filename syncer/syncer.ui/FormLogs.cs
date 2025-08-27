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
            dtpTo.Value = DateTime.Today; // Default to current date
            
            // Set default values for time pickers
            dtpFromTime.Value = DateTime.Today; // Default to 00:00:00
            dtpToTime.Value = DateTime.Today.AddDays(1).AddSeconds(-1); // Default to 23:59:59
            
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

                // Time filter - use the single Timestamp column
                if (chkEnableTimeFilter.Checked && dt.Columns.Contains("Timestamp"))
                {
                    DateTime fromDateTime = dtpFrom.Value.Date.Add(dtpFromTime.Value.TimeOfDay);
                    DateTime toDateTime = dtpTo.Value.Date.Add(dtpToTime.Value.TimeOfDay);

                    string timeFilter = string.Format("Timestamp >= #{0}# AND Timestamp <= #{1}#", 
                        fromDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        toDateTime.ToString("yyyy-MM-dd HH:mm:ss"));

                    filterExpression = string.IsNullOrEmpty(filterExpression) ?
                        timeFilter : string.Format("({0}) AND {1}", filterExpression, timeFilter);
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
            dtpFromTime.Enabled = chkEnableTimeFilter.Checked;
            dtpToTime.Enabled = chkEnableTimeFilter.Checked;
            ApplyFilters();
        }

        /// <summary>
        /// Event handler for From date/time selection change
        /// </summary>
        private void dtpFrom_ValueChanged(object sender, EventArgs e)
        {
            if (chkEnableTimeFilter.Checked)
            {
                ApplyFilters();
            }
        }

        /// <summary>
        /// Event handler for To date/time selection change
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

                // Load logs using the consistent LoadLogs method
                LoadLogs();
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

                // Configure timestamp column - should now always be "Timestamp"
                try
                {
                    if (dgvLogs.Columns.Contains("Timestamp"))
                    {
                        var timestampColumn = dgvLogs.Columns["Timestamp"];
                        timestampColumn.HeaderText = "Timestamp";
                        timestampColumn.Width = 150;
                        timestampColumn.DisplayIndex = 0; // First column
                        timestampColumn.Visible = true;
                        timestampColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        timestampColumn.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
                        timestampColumn.DefaultCellStyle.NullValue = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error configuring timestamp column: " + ex.Message, "UI");
                }

                // Configure Level column
                try
                {
                    if (dgvLogs.Columns.Contains("Level"))
                    {
                        var levelColumn = dgvLogs.Columns["Level"];
                        levelColumn.HeaderText = "Level";
                        levelColumn.Width = 80;
                        levelColumn.DisplayIndex = 1;
                        levelColumn.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error configuring Level column: " + ex.Message, "UI");
                }

                // Configure JobName column
                try
                {
                    if (dgvLogs.Columns.Contains("JobName"))
                    {
                        var jobNameColumn = dgvLogs.Columns["JobName"];
                        jobNameColumn.HeaderText = "Job";
                        jobNameColumn.Width = 100;
                        jobNameColumn.DisplayIndex = 2;
                        jobNameColumn.Visible = true;
                    }
                    else if (dgvLogs.Columns.Contains("JobId"))
                    {
                        var jobIdColumn = dgvLogs.Columns["JobId"];
                        jobIdColumn.HeaderText = "Job";
                        jobIdColumn.Width = 100;
                        jobIdColumn.DisplayIndex = 2;
                        jobIdColumn.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error configuring Job column: " + ex.Message, "UI");
                }

                // Configure Source column
                try
                {
                    if (dgvLogs.Columns.Contains("Source"))
                    {
                        var sourceColumn = dgvLogs.Columns["Source"];
                        sourceColumn.HeaderText = "Source";
                        sourceColumn.Width = 80;
                        sourceColumn.DisplayIndex = 3;
                        sourceColumn.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error configuring Source column: " + ex.Message, "UI");
                }

                // Configure Message column
                try
                {
                    if (dgvLogs.Columns.Contains("Message"))
                    {
                        var messageColumn = dgvLogs.Columns["Message"];
                        messageColumn.HeaderText = "Message";
                        messageColumn.Width = 400;
                        messageColumn.DisplayIndex = 4;
                        messageColumn.Visible = true;
                        if (dgvLogs.IsHandleCreated)
                        {
                            messageColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error configuring Message column: " + ex.Message, "UI");
                }

                // Hide unwanted columns
                try
                {
                    string[] hiddenColumns = { "Exception", "FileName", "FileSize", "Duration", "RemotePath", "LocalPath" };
                    foreach (string columnName in hiddenColumns)
                    {
                        if (dgvLogs.Columns.Contains(columnName))
                        {
                            dgvLogs.Columns[columnName].Visible = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_logService != null)
                        _logService.LogError("Error hiding unwanted columns: " + ex.Message, "UI");
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

                // Load logs from service with broader date range to ensure we get all logs
                DateTime fromDate = DateTime.Today.AddDays(-30);
                DateTime toDate = DateTime.Now.AddDays(1);
                
                _logsDataTable = _logService.GetLogs(fromDate, toDate, null);
                
                // Remove duplicate entries based on timestamp and message to fix issue #3
                if (_logsDataTable != null && _logsDataTable.Rows.Count > 0)
                {
                    _logsDataTable = RemoveDuplicateLogEntries(_logsDataTable);
                }
                
                // Ensure consistent DateTime column structure
                if (_logsDataTable != null)
                {
                    _logsDataTable = EnsureConsistentLogStructure(_logsDataTable);
                }

                if (dgvLogs != null && _logsDataTable != null)
                {
                    // Clear any existing data binding to prevent structure issues
                    dgvLogs.DataSource = null;
                    dgvLogs.Refresh();
                    
                    // Create a DataView with Timestamp sorting enabled
                    DataView dv = new DataView(_logsDataTable);
                    // Always sort by Timestamp column (we ensure this exists in EnsureConsistentLogStructure)
                    dv.Sort = "Timestamp DESC"; // Sort by timestamp descending (newest first)
                    
                    // Set new data source
                    dgvLogs.DataSource = dv;

                    // Use a timer to configure columns after data binding is complete
                    var timer = new Timer();
                    timer.Interval = 200; // 200ms delay to ensure proper rendering
                    timer.Tick += (s, timerArgs) =>
                    {
                        timer.Stop();

                        // Configure columns consistently
                        ConfigureLogColumns();
                        
                        // After loading, apply any filters
                        ApplyFilters();

                        // Update the last updated timestamp
                        UpdateLastUpdatedLabel();
                        timer.Dispose();
                    };
                    timer.Start();
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

        /// <summary>
        /// Remove duplicate log entries based on timestamp and message content
        /// </summary>
        private DataTable RemoveDuplicateLogEntries(DataTable originalTable)
        {
            if (originalTable == null || originalTable.Rows.Count == 0)
                return originalTable;

            try
            {
                DataTable deduplicatedTable = originalTable.Clone();
                var seenEntries = new System.Collections.Generic.HashSet<string>();

                // Always use Timestamp column (ensured by EnsureConsistentLogStructure)
                string timestampColumn = "Timestamp";
                string messageColumn = "Message";

                foreach (DataRow row in originalTable.Rows)
                {
                    // Create a unique key based on timestamp and message
                    string timestamp = row[timestampColumn].ToString();
                    string message = row[messageColumn].ToString();
                    string uniqueKey = timestamp + "|" + message;

                    if (!seenEntries.Contains(uniqueKey))
                    {
                        seenEntries.Add(uniqueKey);
                        deduplicatedTable.ImportRow(row);
                    }
                }

                return deduplicatedTable;
            }
            catch (Exception ex)
            {
                if (_logService != null)
                {
                    _logService.LogError("Error removing duplicate log entries: " + ex.Message, "UI");
                }
                return originalTable; // Return original if deduplication fails
            }
        }

        /// <summary>
        /// Ensure the log DataTable has a consistent structure with a single proper DateTime column
        /// </summary>
        private DataTable EnsureConsistentLogStructure(DataTable originalTable)
        {
            if (originalTable == null)
                return null;

            try
            {
                // Check what timestamp columns we have
                bool hasTimestamp = originalTable.Columns.Contains("Timestamp");
                bool hasDateTime = originalTable.Columns.Contains("DateTime");
                
                // If we have both, remove DateTime and keep only Timestamp
                if (hasTimestamp && hasDateTime)
                {
                    originalTable.Columns.Remove("DateTime");
                    hasDateTime = false;
                }
                
                // If we have Timestamp column, ensure it's DateTime type
                if (hasTimestamp)
                {
                    if (originalTable.Columns["Timestamp"].DataType != typeof(DateTime))
                    {
                        DataColumn newTimestampCol = new DataColumn("TempTimestamp", typeof(DateTime));
                        originalTable.Columns.Add(newTimestampCol);
                        
                        foreach (DataRow row in originalTable.Rows)
                        {
                            try
                            {
                                if (row["Timestamp"] != DBNull.Value)
                                {
                                    DateTime dt;
                                    if (DateTime.TryParse(row["Timestamp"].ToString(), out dt))
                                    {
                                        row["TempTimestamp"] = dt;
                                    }
                                    else
                                    {
                                        row["TempTimestamp"] = DateTime.Now;
                                    }
                                }
                                else
                                {
                                    row["TempTimestamp"] = DateTime.Now;
                                }
                            }
                            catch
                            {
                                row["TempTimestamp"] = DateTime.Now;
                            }
                        }
                        
                        originalTable.Columns.Remove("Timestamp");
                        originalTable.Columns["TempTimestamp"].ColumnName = "Timestamp";
                    }
                }
                // If we only have DateTime column, rename it to Timestamp for consistency
                else if (hasDateTime)
                {
                    if (originalTable.Columns["DateTime"].DataType != typeof(DateTime))
                    {
                        // Fix DateTime column type first
                        DataColumn newDateTimeCol = new DataColumn("TempTimestamp", typeof(DateTime));
                        originalTable.Columns.Add(newDateTimeCol);
                        
                        foreach (DataRow row in originalTable.Rows)
                        {
                            try
                            {
                                if (row["DateTime"] != DBNull.Value)
                                {
                                    DateTime dt;
                                    if (DateTime.TryParse(row["DateTime"].ToString(), out dt))
                                    {
                                        row["TempTimestamp"] = dt;
                                    }
                                    else
                                    {
                                        row["TempTimestamp"] = DateTime.Now;
                                    }
                                }
                                else
                                {
                                    row["TempTimestamp"] = DateTime.Now;
                                }
                            }
                            catch
                            {
                                row["TempTimestamp"] = DateTime.Now;
                            }
                        }
                        
                        originalTable.Columns.Remove("DateTime");
                        originalTable.Columns["TempTimestamp"].ColumnName = "Timestamp";
                    }
                    else
                    {
                        // Just rename the column
                        originalTable.Columns["DateTime"].ColumnName = "Timestamp";
                    }
                }
                else
                {
                    // Create a Timestamp column if neither exists
                    DataColumn timestampCol = new DataColumn("Timestamp", typeof(DateTime));
                    originalTable.Columns.Add(timestampCol);
                    
                    foreach (DataRow row in originalTable.Rows)
                    {
                        row["Timestamp"] = DateTime.Now;
                    }
                }
                
                return originalTable;
            }
            catch (Exception ex)
            {
                if (_logService != null)
                {
                    _logService.LogError("Error ensuring consistent log structure: " + ex.Message, "UI");
                }
                return originalTable;
            }
        }

        private void UpdateLogCount()
        {
            if (lblLogCount != null)
            {
                try
                {
                    int totalCount = 0;
                    int filteredCount = 0;

                    if (dgvLogs != null && dgvLogs.DataSource != null)
                    {
                        if (dgvLogs.DataSource is DataView)
                        {
                            DataView dv = (DataView)dgvLogs.DataSource;
                            totalCount = dv.Table.Rows.Count;
                            filteredCount = dv.Count;
                        }
                        else if (dgvLogs.DataSource is DataTable)
                        {
                            DataTable dt = (DataTable)dgvLogs.DataSource;
                            totalCount = dt.Rows.Count;
                            filteredCount = dt.DefaultView.Count;
                        }
                    }
                    else if (_logsDataTable != null)
                    {
                        totalCount = _logsDataTable.Rows.Count;
                        filteredCount = totalCount;
                    }

                    if (filteredCount == totalCount)
                    {
                        lblLogCount.Text = "Total Logs: " + totalCount;
                    }
                    else
                    {
                        lblLogCount.Text = string.Format("Showing {0} of {1} logs", filteredCount, totalCount);
                    }
                }
                catch
                {
                    lblLogCount.Text = "Total Logs: 0";
                }
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
                    if (dt == null && dgvLogs.DataSource is DataView)
                    {
                        dt = ((DataView)dgvLogs.DataSource).Table;
                    }
                    
                    if (dt != null)
                    {
                        dt.DefaultView.RowFilter = string.Empty;
                    }
                }
                else
                {
                    // Apply filter - search across JobName, Source, and Message columns
                    string safe = searchText.Replace("'", "''");
                    string filter = string.Empty;
                    
                    DataTable dt = dgvLogs != null ? dgvLogs.DataSource as DataTable : null;
                    if (dt == null && dgvLogs.DataSource is DataView)
                    {
                        dt = ((DataView)dgvLogs.DataSource).Table;
                    }
                    
                    if (dt != null)
                    {
                        // Build dynamic filter based on available columns
                        var filterParts = new System.Collections.Generic.List<string>();
                        
                        if (dt.Columns.Contains("JobName"))
                            filterParts.Add("JobName LIKE '%" + safe + "%'");
                        if (dt.Columns.Contains("JobId"))
                            filterParts.Add("JobId LIKE '%" + safe + "%'");
                        if (dt.Columns.Contains("Source"))
                            filterParts.Add("Source LIKE '%" + safe + "%'");
                        if (dt.Columns.Contains("Message"))
                            filterParts.Add("Message LIKE '%" + safe + "%'");
                        
                        if (filterParts.Count > 0)
                        {
                            filter = string.Join(" OR ", filterParts.ToArray());
                            dt.DefaultView.RowFilter = filter;
                        }
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
            if (dt == null && dgvLogs != null && dgvLogs.DataSource is DataView)
            {
                dt = ((DataView)dgvLogs.DataSource).Table;
            }
            
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
                // Special handling for timestamp column
                string columnName = dgvLogs.Columns[e.ColumnIndex].Name;
                if (columnName == "Timestamp")
                {
                    try
                    {
                        // Handle null values
                        if (e.Value == null || e.Value == DBNull.Value)
                        {
                            e.Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            e.FormattingApplied = true;
                            return;
                        }
                        
                        // Handle DateTime values
                        if (e.Value is DateTime)
                        {
                            // Format datetime value with time precision
                            DateTime dateValue = (DateTime)e.Value;
                            e.Value = dateValue.ToString("dd/MM/yyyy HH:mm:ss");
                            e.FormattingApplied = true;
                        }
                        // Handle string values
                        else if (e.Value is string)
                        {
                            // Try to parse string to DateTime
                            DateTime dateValue;
                            if (DateTime.TryParse(e.Value.ToString(), out dateValue))
                            {
                                e.Value = dateValue.ToString("dd/MM/yyyy HH:mm:ss");
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
                                e.Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                                e.FormattingApplied = true;
                            }
                        }
                        // Handle any other type
                        else
                        {
                            e.Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            e.FormattingApplied = true;
                        }
                    }
                    catch
                    {
                        // If there's an error formatting, use current date
                        e.Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
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
                else if (logLevel == "Warning")
                {
                    // Format Warning rows with yellow background
                    e.CellStyle.BackColor = Color.LightYellow;
                    e.CellStyle.ForeColor = Color.Black;
                    e.CellStyle.SelectionBackColor = Color.Orange;
                    e.CellStyle.SelectionForeColor = Color.Black;
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
   
