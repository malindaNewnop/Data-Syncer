namespace syncer.ui
{
    partial class FormLogs
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvLogs;
        
        // Search Filter section
        private System.Windows.Forms.GroupBox gbSearchFilter;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnClearSearch;
        private System.Windows.Forms.Label lblLogLevel;
        private System.Windows.Forms.ComboBox cmbLogLevel;
        
        // Time Filter section
        private System.Windows.Forms.GroupBox gbTimeFilter;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private System.Windows.Forms.Label lblFromTime;
        private System.Windows.Forms.Label lblToTime;
        private System.Windows.Forms.DateTimePicker dtpFromTime;
        private System.Windows.Forms.DateTimePicker dtpToTime;
        private System.Windows.Forms.CheckBox chkEnableTimeFilter;
        
        // Job Filter section
        private System.Windows.Forms.GroupBox gbJobFilter;
        private System.Windows.Forms.Label lblJob;
        private System.Windows.Forms.ComboBox cmbJobs;
        
        // Actions section
        private System.Windows.Forms.GroupBox gbActions;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClearLogs;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnClose;
        
        // Status section
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblLogCount;
        private System.Windows.Forms.ToolStripStatusLabel lblLastUpdated;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dgvLogs = new System.Windows.Forms.DataGridView();
            
            // Search Filter
            this.gbSearchFilter = new System.Windows.Forms.GroupBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnClearSearch = new System.Windows.Forms.Button();
            this.lblLogLevel = new System.Windows.Forms.Label();
            this.cmbLogLevel = new System.Windows.Forms.ComboBox();
            
            // Time Filter
            this.gbTimeFilter = new System.Windows.Forms.GroupBox();
            this.lblFrom = new System.Windows.Forms.Label();
            this.lblTo = new System.Windows.Forms.Label();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.dtpTo = new System.Windows.Forms.DateTimePicker();
            this.lblFromTime = new System.Windows.Forms.Label();
            this.lblToTime = new System.Windows.Forms.Label();
            this.dtpFromTime = new System.Windows.Forms.DateTimePicker();
            this.dtpToTime = new System.Windows.Forms.DateTimePicker();
            this.chkEnableTimeFilter = new System.Windows.Forms.CheckBox();
            
            // Job Filter
            this.gbJobFilter = new System.Windows.Forms.GroupBox();
            this.lblJob = new System.Windows.Forms.Label();
            this.cmbJobs = new System.Windows.Forms.ComboBox();
            
            // Actions
            this.gbActions = new System.Windows.Forms.GroupBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnClearLogs = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            
            // Status
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblLogCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblLastUpdated = new System.Windows.Forms.ToolStripStatusLabel();
            
            ((System.ComponentModel.ISupportInitialize)(this.dgvLogs)).BeginInit();
            this.gbSearchFilter.SuspendLayout();
            this.gbTimeFilter.SuspendLayout();
            this.gbJobFilter.SuspendLayout();
            this.gbActions.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // dgvLogs
            this.dgvLogs.AllowUserToAddRows = false;
            this.dgvLogs.AllowUserToDeleteRows = false;
            this.dgvLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvLogs.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLogs.AutoGenerateColumns = true;
            this.dgvLogs.Location = new System.Drawing.Point(12, 195);
            this.dgvLogs.MultiSelect = false;
            this.dgvLogs.Name = "dgvLogs";
            this.dgvLogs.ReadOnly = true;
            this.dgvLogs.RowHeadersWidth = 25;
            this.dgvLogs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLogs.Size = new System.Drawing.Size(860, 335);
            this.dgvLogs.TabIndex = 5;
            this.dgvLogs.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvLogs_CellFormatting);
            
            // Search Filter GroupBox
            this.gbSearchFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSearchFilter.Controls.Add(this.lblSearch);
            this.gbSearchFilter.Controls.Add(this.txtSearch);
            this.gbSearchFilter.Controls.Add(this.btnSearch);
            this.gbSearchFilter.Controls.Add(this.btnClearSearch);
            this.gbSearchFilter.Controls.Add(this.lblLogLevel);
            this.gbSearchFilter.Controls.Add(this.cmbLogLevel);
            this.gbSearchFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbSearchFilter.Location = new System.Drawing.Point(12, 12);
            this.gbSearchFilter.Name = "gbSearchFilter";
            this.gbSearchFilter.Size = new System.Drawing.Size(360, 80);
            this.gbSearchFilter.TabIndex = 1;
            this.gbSearchFilter.TabStop = false;
            this.gbSearchFilter.Text = "Search Filter";
            // lblSearch
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblSearch.Location = new System.Drawing.Point(15, 25);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(48, 15);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search:";
            // txtSearch
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtSearch.Location = new System.Drawing.Point(70, 22);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(180, 21);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearch_KeyPress);
            
            // Time Filter GroupBox
            this.gbTimeFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbTimeFilter.Controls.Add(this.lblFrom);
            this.gbTimeFilter.Controls.Add(this.dtpFrom);
            this.gbTimeFilter.Controls.Add(this.lblFromTime);
            this.gbTimeFilter.Controls.Add(this.dtpFromTime);
            this.gbTimeFilter.Controls.Add(this.lblTo);
            this.gbTimeFilter.Controls.Add(this.dtpTo);
            this.gbTimeFilter.Controls.Add(this.lblToTime);
            this.gbTimeFilter.Controls.Add(this.dtpToTime);
            this.gbTimeFilter.Controls.Add(this.chkEnableTimeFilter);
            this.gbTimeFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbTimeFilter.Location = new System.Drawing.Point(380, 12);
            this.gbTimeFilter.Name = "gbTimeFilter";
            this.gbTimeFilter.Size = new System.Drawing.Size(300, 110);
            this.gbTimeFilter.TabIndex = 2;
            this.gbTimeFilter.TabStop = false;
            this.gbTimeFilter.Text = "Date & Time Filter";
            
            // From label
            this.lblFrom.AutoSize = true;
            this.lblFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblFrom.Location = new System.Drawing.Point(15, 25);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(40, 15);
            this.lblFrom.TabIndex = 0;
            this.lblFrom.Text = "From:";
            
            // From DateTimePicker
            this.dtpFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFrom.Location = new System.Drawing.Point(60, 22);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(95, 21);
            this.dtpFrom.TabIndex = 1;
            this.dtpFrom.ValueChanged += new System.EventHandler(this.dtpFrom_ValueChanged);
            
            // From Time label
            this.lblFromTime.AutoSize = true;
            this.lblFromTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblFromTime.Location = new System.Drawing.Point(165, 25);
            this.lblFromTime.Name = "lblFromTime";
            this.lblFromTime.Size = new System.Drawing.Size(36, 15);
            this.lblFromTime.TabIndex = 2;
            this.lblFromTime.Text = "Time:";
            
            // From Time DateTimePicker
            this.dtpFromTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.dtpFromTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpFromTime.ShowUpDown = true;
            this.dtpFromTime.Location = new System.Drawing.Point(205, 22);
            this.dtpFromTime.Name = "dtpFromTime";
            this.dtpFromTime.Size = new System.Drawing.Size(85, 21);
            this.dtpFromTime.TabIndex = 3;
            this.dtpFromTime.ValueChanged += new System.EventHandler(this.dtpFrom_ValueChanged);
            
            // To label
            this.lblTo.AutoSize = true;
            this.lblTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblTo.Location = new System.Drawing.Point(15, 55);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(24, 15);
            this.lblTo.TabIndex = 4;
            this.lblTo.Text = "To:";
            
            // To DateTimePicker
            this.dtpTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpTo.Location = new System.Drawing.Point(60, 52);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(95, 21);
            this.dtpTo.TabIndex = 5;
            this.dtpTo.ValueChanged += new System.EventHandler(this.dtpTo_ValueChanged);
            
            // To Time label
            this.lblToTime.AutoSize = true;
            this.lblToTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblToTime.Location = new System.Drawing.Point(165, 55);
            this.lblToTime.Name = "lblToTime";
            this.lblToTime.Size = new System.Drawing.Size(36, 15);
            this.lblToTime.TabIndex = 6;
            this.lblToTime.Text = "Time:";
            
            // To Time DateTimePicker
            this.dtpToTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.dtpToTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpToTime.ShowUpDown = true;
            this.dtpToTime.Location = new System.Drawing.Point(205, 52);
            this.dtpToTime.Name = "dtpToTime";
            this.dtpToTime.Size = new System.Drawing.Size(85, 21);
            this.dtpToTime.TabIndex = 7;
            this.dtpToTime.ValueChanged += new System.EventHandler(this.dtpTo_ValueChanged);
            
            // Enable Time Filtering Checkbox
            this.chkEnableTimeFilter.AutoSize = true;
            this.chkEnableTimeFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkEnableTimeFilter.Location = new System.Drawing.Point(60, 80);
            this.chkEnableTimeFilter.Name = "chkEnableTimeFilter";
            this.chkEnableTimeFilter.Size = new System.Drawing.Size(133, 19);
            this.chkEnableTimeFilter.TabIndex = 8;
            this.chkEnableTimeFilter.Text = "Enable Time Filtering";
            this.chkEnableTimeFilter.UseVisualStyleBackColor = true;
            this.chkEnableTimeFilter.CheckedChanged += new System.EventHandler(this.chkEnableTimeFilter_CheckedChanged);
            // btnSearch
            this.btnSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnSearch.Location = new System.Drawing.Point(330, 21);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(60, 23);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // btnClearSearch
            this.btnClearSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnClearSearch.Location = new System.Drawing.Point(400, 21);
            this.btnClearSearch.Name = "btnClearSearch";
            this.btnClearSearch.Size = new System.Drawing.Size(60, 23);
            this.btnClearSearch.TabIndex = 3;
            this.btnClearSearch.Text = "Clear";
            this.btnClearSearch.UseVisualStyleBackColor = true;
            this.btnClearSearch.Click += new System.EventHandler(this.btnClearSearch_Click);
            // lblLogLevel
            this.lblLogLevel.AutoSize = true;
            this.lblLogLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblLogLevel.Location = new System.Drawing.Point(15, 55);
            this.lblLogLevel.Name = "lblLogLevel";
            this.lblLogLevel.Size = new System.Drawing.Size(64, 15);
            this.lblLogLevel.TabIndex = 4;
            this.lblLogLevel.Text = "Log Level:";
            // cmbLogLevel
            this.cmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLogLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.cmbLogLevel.FormattingEnabled = true;
            this.cmbLogLevel.Items.AddRange(new object[] {
            "All",
            "INFO",
            "WARNING",
            "ERROR"});
            this.cmbLogLevel.Location = new System.Drawing.Point(90, 52);
            this.cmbLogLevel.Name = "cmbLogLevel";
            this.cmbLogLevel.Size = new System.Drawing.Size(100, 23);
            this.cmbLogLevel.TabIndex = 5;
            this.cmbLogLevel.SelectedIndexChanged += new System.EventHandler(this.cmbLogLevel_SelectedIndexChanged);
            // Job Filter GroupBox
            this.gbJobFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbJobFilter.Controls.Add(this.lblJob);
            this.gbJobFilter.Controls.Add(this.cmbJobs);
            this.gbJobFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbJobFilter.Location = new System.Drawing.Point(12, 125);
            this.gbJobFilter.Name = "gbJobFilter";
            this.gbJobFilter.Size = new System.Drawing.Size(360, 60);
            this.gbJobFilter.TabIndex = 3;
            this.gbJobFilter.TabStop = false;
            this.gbJobFilter.Text = "Job Filter";
            
            // Job label
            this.lblJob.AutoSize = true;
            this.lblJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblJob.Location = new System.Drawing.Point(15, 25);
            this.lblJob.Name = "lblJob";
            this.lblJob.Size = new System.Drawing.Size(30, 15);
            this.lblJob.TabIndex = 0;
            this.lblJob.Text = "Job:";
            
            // Job ComboBox
            this.cmbJobs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbJobs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.cmbJobs.FormattingEnabled = true;
            this.cmbJobs.Items.AddRange(new object[] {
            "All Jobs",
            "UI",
            "Core"});
            this.cmbJobs.Location = new System.Drawing.Point(70, 22);
            this.cmbJobs.Name = "cmbJobs";
            this.cmbJobs.Size = new System.Drawing.Size(180, 23);
            this.cmbJobs.TabIndex = 1;
            this.cmbJobs.SelectedIndexChanged += new System.EventHandler(this.cmbJobs_SelectedIndexChanged);
            
            // Actions GroupBox
            this.gbActions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gbActions.Controls.Add(this.btnRefresh);
            this.gbActions.Controls.Add(this.btnClearLogs);
            this.gbActions.Controls.Add(this.btnExport);
            this.gbActions.Controls.Add(this.btnClose);
            this.gbActions.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbActions.Location = new System.Drawing.Point(690, 12);
            this.gbActions.Name = "gbActions";
            this.gbActions.Size = new System.Drawing.Size(182, 173);
            this.gbActions.TabIndex = 4;
            this.gbActions.TabStop = false;
            this.gbActions.Text = "Actions";
            // btnRefresh
            this.btnRefresh.BackColor = System.Drawing.Color.LightBlue;
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRefresh.Location = new System.Drawing.Point(15, 20);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(70, 25);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // btnClearLogs
            this.btnClearLogs.BackColor = System.Drawing.Color.LightCoral;
            this.btnClearLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnClearLogs.Location = new System.Drawing.Point(95, 20);
            this.btnClearLogs.Name = "btnClearLogs";
            this.btnClearLogs.Size = new System.Drawing.Size(70, 25);
            this.btnClearLogs.TabIndex = 1;
            this.btnClearLogs.Text = "Clear Logs";
            this.btnClearLogs.UseVisualStyleBackColor = false;
            this.btnClearLogs.Click += new System.EventHandler(this.btnClearLogs_Click);
            // btnExport
            this.btnExport.BackColor = System.Drawing.Color.LightGreen;
            this.btnExport.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnExport.Location = new System.Drawing.Point(15, 50);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(70, 25);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = false;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // btnClose
            this.btnClose.BackColor = System.Drawing.Color.LightGray;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnClose.Location = new System.Drawing.Point(95, 50);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(70, 25);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // statusStrip1
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblLogCount,
            this.lblLastUpdated});
            this.statusStrip1.Location = new System.Drawing.Point(0, 539);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(884, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            
            // lblLogCount
            this.lblLogCount.Name = "lblLogCount";
            this.lblLogCount.Size = new System.Drawing.Size(68, 17);
            this.lblLogCount.Text = "Total Logs: 0";
            
            // lblLastUpdated
            this.lblLastUpdated.Name = "lblLastUpdated";
            this.lblLastUpdated.Size = new System.Drawing.Size(151, 17);
            this.lblLastUpdated.Text = "Last Updated: Not updated yet";
            // FormLogs
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.gbActions);
            this.Controls.Add(this.gbSearchFilter);
            this.Controls.Add(this.gbTimeFilter);
            this.Controls.Add(this.gbJobFilter);
            this.Controls.Add(this.dgvLogs);
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "FormLogs";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Data Syncer - Log Viewer";
            this.Load += new System.EventHandler(this.FormLogs_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLogs)).EndInit();
            this.gbSearchFilter.ResumeLayout(false);
            this.gbSearchFilter.PerformLayout();
            this.gbTimeFilter.ResumeLayout(false);
            this.gbTimeFilter.PerformLayout();
            this.gbJobFilter.ResumeLayout(false);
            this.gbJobFilter.PerformLayout();
            this.gbActions.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
