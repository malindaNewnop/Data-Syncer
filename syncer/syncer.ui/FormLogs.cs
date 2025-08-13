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
            _logService = ServiceLocator.LogService;
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Log Viewer";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            InitializeLogData();
            LoadLogs();
        }

        private void InitializeLogData()
        {
            // Get logs from service instead of creating hardcoded data
            _logsDataTable = _logService.GetLogs();

            // Bind to DataGridView
            if (dgvLogs != null)
            {
                dgvLogs.DataSource = _logsDataTable;

                // Configure columns
                ConfigureLogColumns();
            }
        }

        private void ConfigureLogColumns()
        {
            if (dgvLogs != null && dgvLogs.Columns != null)
            {
                if (dgvLogs.Columns["DateTime"] != null)
                {
                    dgvLogs.Columns["DateTime"].HeaderText = "Date/Time";
                    dgvLogs.Columns["DateTime"].Width = 130;
                }

                if (dgvLogs.Columns["Level"] != null)
                    dgvLogs.Columns["Level"].Width = 70;

                if (dgvLogs.Columns["Job"] != null)
                    dgvLogs.Columns["Job"].Width = 100;

                if (dgvLogs.Columns["File"] != null)
                    dgvLogs.Columns["File"].Width = 200;

                if (dgvLogs.Columns["Status"] != null)
                    dgvLogs.Columns["Status"].Width = 80;

                if (dgvLogs.Columns["Message"] != null)
                {
                    dgvLogs.Columns["Message"].Width = 250;
                    dgvLogs.Columns["Message"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }
        }

        private void LoadLogs()
        {
            try
            {
                // Load logs from service
                _logsDataTable = _logService.GetLogs();

                if (dgvLogs != null)
                {
                    dgvLogs.DataSource = _logsDataTable;
                    ConfigureLogColumns();
                }

                UpdateLogCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading logs: " + ex.Message, "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceLocator.LogService.LogError("Error loading logs in FormLogs: " + ex.Message);
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

                if (StringExtensions.IsNullOrWhiteSpace(searchText))
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
                    // Apply filter
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
                    if (_logService.ClearLogs())
                    {
                        LoadLogs(); // Refresh the display
                        MessageBox.Show("Logs cleared successfully.", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ServiceLocator.LogService.LogInfo("Log history cleared by user");
                    }
                    else
                    {
                        MessageBox.Show("Failed to clear logs.", "Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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
                        if (_logService.ExportLogs(dialog.FileName))
                        {
                            MessageBox.Show("Logs exported successfully to:\n" + dialog.FileName, "Export Complete",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ServiceLocator.LogService.LogInfo("Logs exported to: " + dialog.FileName);
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
                        ServiceLocator.LogService.LogError("Error exporting logs: " + ex.Message);
                    }
                }
            }
        }

        private void cmbLogLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedLevel = (cmbLogLevel != null && cmbLogLevel.SelectedItem != null) ? cmbLogLevel.SelectedItem.ToString() : null;

                if (StringExtensions.IsNullOrWhiteSpace(selectedLevel) || selectedLevel == "All")
                {
                    // Load all logs
                    _logsDataTable = _logService.GetLogs();
                }
                else
                {
                    // Filter by log level
                    DataTable dt = dgvLogs != null ? dgvLogs.DataSource as DataTable : null;
                    if (dt != null)
                    {
                        dt.DefaultView.RowFilter = "Level = '" + selectedLevel.Replace("'", "''") + "'";
                    }
                }

                if (dgvLogs != null)
                {
                    dgvLogs.DataSource = _logsDataTable;
                    ConfigureLogColumns();
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
