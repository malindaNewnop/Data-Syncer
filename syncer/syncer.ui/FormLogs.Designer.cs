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
        
        // Date & Time Filter (moved to Search Filter area)
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private System.Windows.Forms.Label lblFromTime;
        private System.Windows.Forms.Label lblToTime;
        private System.Windows.Forms.DateTimePicker dtpFromTime;
        private System.Windows.Forms.DateTimePicker dtpToTime;
        private System.Windows.Forms.CheckBox chkEnableTimeFilter;
        
        // Time Filter section (hidden - controls moved to Search Filter)
        private System.Windows.Forms.GroupBox gbTimeFilter;
        
        // Log Level Filter (hidden but functional)
        private System.Windows.Forms.Label lblLogLevel;
        private System.Windows.Forms.ComboBox cmbLogLevel;
        
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
        
        // Real-time Logging section
        private System.Windows.Forms.GroupBox gbRealTimeLogging;
        private System.Windows.Forms.CheckBox chkEnableRealTimeLogging;
        private System.Windows.Forms.Label lblRealTimeLogPath;
        private System.Windows.Forms.TextBox txtRealTimeLogPath;
        private System.Windows.Forms.Button btnBrowseRealTimeLogPath;
        private System.Windows.Forms.Label lblRealTimeStatus;
        
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
            
            // Date & Time Filter controls (moved from gbTimeFilter to gbSearchFilter)
            this.lblFrom = new System.Windows.Forms.Label();
            this.lblTo = new System.Windows.Forms.Label();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.dtpTo = new System.Windows.Forms.DateTimePicker();
            this.lblFromTime = new System.Windows.Forms.Label();
            this.lblToTime = new System.Windows.Forms.Label();
            this.dtpFromTime = new System.Windows.Forms.DateTimePicker();
            this.dtpToTime = new System.Windows.Forms.DateTimePicker();
            this.chkEnableTimeFilter = new System.Windows.Forms.CheckBox();
            
            // Hidden Time Filter GroupBox
            this.gbTimeFilter = new System.Windows.Forms.GroupBox();
            
            // Removed Log Level controls
            this.lblLogLevel = new System.Windows.Forms.Label();
            this.cmbLogLevel = new System.Windows.Forms.ComboBox();
            
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
            
            // Real-time Logging
            this.gbRealTimeLogging = new System.Windows.Forms.GroupBox();
            this.chkEnableRealTimeLogging = new System.Windows.Forms.CheckBox();
            this.lblRealTimeLogPath = new System.Windows.Forms.Label();
            this.txtRealTimeLogPath = new System.Windows.Forms.TextBox();
            this.btnBrowseRealTimeLogPath = new System.Windows.Forms.Button();
            this.lblRealTimeStatus = new System.Windows.Forms.Label();
            
            // Status
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblLogCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblLastUpdated = new System.Windows.Forms.ToolStripStatusLabel();
            
            ((System.ComponentModel.ISupportInitialize)(this.dgvLogs)).BeginInit();
            this.gbSearchFilter.SuspendLayout();
            this.gbTimeFilter.SuspendLayout();
            this.gbJobFilter.SuspendLayout();
            this.gbActions.SuspendLayout();
            this.gbRealTimeLogging.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // dgvLogs - Make fully responsive
            // 
            this.dgvLogs.AllowUserToAddRows = false;
            this.dgvLogs.AllowUserToDeleteRows = false;
            this.dgvLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvLogs.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLogs.AutoGenerateColumns = true;
            this.dgvLogs.Location = new System.Drawing.Point(12, 270);
            this.dgvLogs.MultiSelect = false;
            this.dgvLogs.Name = "dgvLogs";
            this.dgvLogs.ReadOnly = true;
            this.dgvLogs.RowHeadersWidth = 25;
            this.dgvLogs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLogs.Size = new System.Drawing.Size(860, 230);
            this.dgvLogs.TabIndex = 5;
            this.dgvLogs.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvLogs_CellFormatting);
            
            // 
            // gbSearchFilter - Now contains Date & Time filter instead of Log Level
            // 
            this.gbSearchFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSearchFilter.Controls.Add(this.lblSearch);
            this.gbSearchFilter.Controls.Add(this.txtSearch);
            this.gbSearchFilter.Controls.Add(this.btnSearch);
            this.gbSearchFilter.Controls.Add(this.btnClearSearch);
            this.gbSearchFilter.Controls.Add(this.chkEnableTimeFilter);
            this.gbSearchFilter.Controls.Add(this.lblFrom);
            this.gbSearchFilter.Controls.Add(this.dtpFrom);
            this.gbSearchFilter.Controls.Add(this.lblFromTime);
            this.gbSearchFilter.Controls.Add(this.dtpFromTime);
            this.gbSearchFilter.Controls.Add(this.lblTo);
            this.gbSearchFilter.Controls.Add(this.dtpTo);
            this.gbSearchFilter.Controls.Add(this.lblToTime);
            this.gbSearchFilter.Controls.Add(this.dtpToTime);
            this.gbSearchFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbSearchFilter.Location = new System.Drawing.Point(12, 12);
            this.gbSearchFilter.Name = "gbSearchFilter";
            this.gbSearchFilter.Size = new System.Drawing.Size(668, 135);
            this.gbSearchFilter.TabIndex = 1;
            this.gbSearchFilter.TabStop = false;
            this.gbSearchFilter.Text = "Search && Date/Time Filter";
            
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblSearch.Location = new System.Drawing.Point(10, 25);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(48, 15);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search:";
            
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtSearch.Location = new System.Drawing.Point(70, 22);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(420, 21);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearch_KeyPress);
            
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnSearch.Location = new System.Drawing.Point(500, 21);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(70, 23);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            
            // 
            // btnClearSearch
            // 
            this.btnClearSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnClearSearch.Location = new System.Drawing.Point(580, 21);
            this.btnClearSearch.Name = "btnClearSearch";
            this.btnClearSearch.Size = new System.Drawing.Size(70, 23);
            this.btnClearSearch.TabIndex = 3;
            this.btnClearSearch.Text = "Clear";
            this.btnClearSearch.UseVisualStyleBackColor = true;
            this.btnClearSearch.Click += new System.EventHandler(this.btnClearSearch_Click);
            
            // 
            // chkEnableTimeFilter
            // 
            this.chkEnableTimeFilter.AutoSize = true;
            this.chkEnableTimeFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkEnableTimeFilter.Location = new System.Drawing.Point(13, 55);
            this.chkEnableTimeFilter.Name = "chkEnableTimeFilter";
            this.chkEnableTimeFilter.Size = new System.Drawing.Size(130, 19);
            this.chkEnableTimeFilter.TabIndex = 4;
            this.chkEnableTimeFilter.Text = "Enable Date/Time Filter";
            this.chkEnableTimeFilter.UseVisualStyleBackColor = true;
            this.chkEnableTimeFilter.CheckedChanged += new System.EventHandler(this.chkEnableTimeFilter_CheckedChanged);
            
            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblFrom.Location = new System.Drawing.Point(30, 83);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(38, 15);
            this.lblFrom.TabIndex = 5;
            this.lblFrom.Text = "From:";
            
            // 
            // dtpFrom
            // 
            this.dtpFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFrom.Location = new System.Drawing.Point(75, 80);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(105, 21);
            this.dtpFrom.TabIndex = 6;
            this.dtpFrom.ValueChanged += new System.EventHandler(this.dtpFrom_ValueChanged);
            
            // 
            // lblFromTime
            // 
            this.lblFromTime.AutoSize = true;
            this.lblFromTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblFromTime.Location = new System.Drawing.Point(185, 83);
            this.lblFromTime.Name = "lblFromTime";
            this.lblFromTime.Size = new System.Drawing.Size(37, 15);
            this.lblFromTime.TabIndex = 7;
            this.lblFromTime.Text = "Time:";
            
            // 
            // dtpFromTime
            // 
            this.dtpFromTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.dtpFromTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpFromTime.Location = new System.Drawing.Point(228, 80);
            this.dtpFromTime.Name = "dtpFromTime";
            this.dtpFromTime.ShowUpDown = true;
            this.dtpFromTime.Size = new System.Drawing.Size(95, 21);
            this.dtpFromTime.TabIndex = 8;
            this.dtpFromTime.ValueChanged += new System.EventHandler(this.dtpFrom_ValueChanged);
            
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblTo.Location = new System.Drawing.Point(30, 110);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(24, 15);
            this.lblTo.TabIndex = 9;
            this.lblTo.Text = "To:";
            
            // 
            // dtpTo
            // 
            this.dtpTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpTo.Location = new System.Drawing.Point(75, 107);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(105, 21);
            this.dtpTo.TabIndex = 10;
            this.dtpTo.ValueChanged += new System.EventHandler(this.dtpTo_ValueChanged);
            
            // 
            // lblToTime
            // 
            this.lblToTime.AutoSize = true;
            this.lblToTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblToTime.Location = new System.Drawing.Point(185, 110);
            this.lblToTime.Name = "lblToTime";
            this.lblToTime.Size = new System.Drawing.Size(37, 15);
            this.lblToTime.TabIndex = 11;
            this.lblToTime.Text = "Time:";
            
            // 
            // dtpToTime
            // 
            this.dtpToTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.dtpToTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpToTime.Location = new System.Drawing.Point(228, 107);
            this.dtpToTime.Name = "dtpToTime";
            this.dtpToTime.ShowUpDown = true;
            this.dtpToTime.Size = new System.Drawing.Size(95, 21);
            this.dtpToTime.TabIndex = 12;
            this.dtpToTime.ValueChanged += new System.EventHandler(this.dtpTo_ValueChanged);
            
            // 
            // gbTimeFilter - Hidden (controls moved to gbSearchFilter)
            // 
            this.gbTimeFilter.Visible = false;
            this.gbTimeFilter.Location = new System.Drawing.Point(0, 0);
            this.gbTimeFilter.Name = "gbTimeFilter";
            this.gbTimeFilter.Size = new System.Drawing.Size(0, 0);
            this.gbTimeFilter.TabIndex = 99;
            this.gbTimeFilter.TabStop = false;
            
            // 
            // lblLogLevel - Hidden (removed from UI)
            // 
            this.lblLogLevel.Visible = false;
            this.lblLogLevel.Location = new System.Drawing.Point(0, 0);
            this.lblLogLevel.Name = "lblLogLevel";
            this.lblLogLevel.Size = new System.Drawing.Size(0, 0);
            this.lblLogLevel.TabIndex = 99;
            
            // 
            // cmbLogLevel - Hidden (removed from UI)
            // 
            this.cmbLogLevel.Visible = false;
            this.cmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLogLevel.Location = new System.Drawing.Point(0, 0);
            this.cmbLogLevel.Name = "cmbLogLevel";
            this.cmbLogLevel.Size = new System.Drawing.Size(0, 0);
            this.cmbLogLevel.TabIndex = 99;
            this.cmbLogLevel.Items.AddRange(new object[] { "All", "INFO", "WARNING", "ERROR" });
            this.cmbLogLevel.SelectedIndex = 0;
            this.cmbLogLevel.SelectedIndexChanged += new System.EventHandler(this.cmbLogLevel_SelectedIndexChanged);
            
            // 
            // gbJobFilter - Make responsive
            // 
            this.gbJobFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbJobFilter.Controls.Add(this.lblJob);
            this.gbJobFilter.Controls.Add(this.cmbJobs);
            this.gbJobFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbJobFilter.Location = new System.Drawing.Point(12, 155);
            this.gbJobFilter.Name = "gbJobFilter";
            this.gbJobFilter.Size = new System.Drawing.Size(668, 60);
            this.gbJobFilter.TabIndex = 3;
            this.gbJobFilter.TabStop = false;
            this.gbJobFilter.Text = "Job Filter";
            
            // 
            // lblJob
            // 
            this.lblJob.AutoSize = true;
            this.lblJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblJob.Location = new System.Drawing.Point(10, 27);
            this.lblJob.Name = "lblJob";
            this.lblJob.Size = new System.Drawing.Size(30, 15);
            this.lblJob.TabIndex = 0;
            this.lblJob.Text = "Job:";
            
            // 
            // cmbJobs - Make responsive
            // 
            this.cmbJobs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbJobs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbJobs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.cmbJobs.FormattingEnabled = true;
            this.cmbJobs.Items.AddRange(new object[] {
            "All Jobs",
            "UI",
            "Core"});
            this.cmbJobs.Location = new System.Drawing.Point(70, 24);
            this.cmbJobs.Name = "cmbJobs";
            this.cmbJobs.Size = new System.Drawing.Size(580, 23);
            this.cmbJobs.TabIndex = 1;
            this.cmbJobs.SelectedIndexChanged += new System.EventHandler(this.cmbJobs_SelectedIndexChanged);
            
            // 
            // gbActions - Keep on right side, make responsive
            // 
            this.gbActions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gbActions.Controls.Add(this.btnRefresh);
            this.gbActions.Controls.Add(this.btnClearLogs);
            this.gbActions.Controls.Add(this.btnExport);
            this.gbActions.Controls.Add(this.btnClose);
            this.gbActions.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbActions.Location = new System.Drawing.Point(690, 12);
            this.gbActions.Name = "gbActions";
            this.gbActions.Size = new System.Drawing.Size(182, 203);
            this.gbActions.TabIndex = 4;
            this.gbActions.TabStop = false;
            this.gbActions.Text = "Actions";
            
            // 
            // btnRefresh
            // 
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRefresh.Location = new System.Drawing.Point(15, 25);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(70, 25);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            
            // 
            // btnClearLogs
            // 
            this.btnClearLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnClearLogs.Location = new System.Drawing.Point(95, 25);
            this.btnClearLogs.Name = "btnClearLogs";
            this.btnClearLogs.Size = new System.Drawing.Size(70, 25);
            this.btnClearLogs.TabIndex = 1;
            this.btnClearLogs.Text = "Clear";
            this.btnClearLogs.UseVisualStyleBackColor = true;
            this.btnClearLogs.Click += new System.EventHandler(this.btnClearLogs_Click);
            
            // 
            // btnExport
            // 
            this.btnExport.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnExport.Location = new System.Drawing.Point(15, 55);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(70, 25);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnClose.Location = new System.Drawing.Point(95, 55);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(70, 25);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            
            // 
            // gbRealTimeLogging - Make fully responsive
            // 
            this.gbRealTimeLogging.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbRealTimeLogging.Controls.Add(this.chkEnableRealTimeLogging);
            this.gbRealTimeLogging.Controls.Add(this.lblRealTimeLogPath);
            this.gbRealTimeLogging.Controls.Add(this.txtRealTimeLogPath);
            this.gbRealTimeLogging.Controls.Add(this.btnBrowseRealTimeLogPath);
            this.gbRealTimeLogging.Controls.Add(this.lblRealTimeStatus);
            this.gbRealTimeLogging.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbRealTimeLogging.Location = new System.Drawing.Point(12, 223);
            this.gbRealTimeLogging.Name = "gbRealTimeLogging";
            this.gbRealTimeLogging.Size = new System.Drawing.Size(860, 40);
            this.gbRealTimeLogging.TabIndex = 5;
            this.gbRealTimeLogging.TabStop = false;
            this.gbRealTimeLogging.Text = "Real-time CSV Logging";
            
            // 
            // chkEnableRealTimeLogging
            // 
            this.chkEnableRealTimeLogging.AutoSize = true;
            this.chkEnableRealTimeLogging.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkEnableRealTimeLogging.Location = new System.Drawing.Point(13, 18);
            this.chkEnableRealTimeLogging.Name = "chkEnableRealTimeLogging";
            this.chkEnableRealTimeLogging.Size = new System.Drawing.Size(65, 19);
            this.chkEnableRealTimeLogging.TabIndex = 0;
            this.chkEnableRealTimeLogging.Text = "Enable";
            this.chkEnableRealTimeLogging.UseVisualStyleBackColor = true;
            this.chkEnableRealTimeLogging.CheckedChanged += new System.EventHandler(this.chkEnableRealTimeLogging_CheckedChanged);
            
            // 
            // lblRealTimeLogPath
            // 
            this.lblRealTimeLogPath.AutoSize = true;
            this.lblRealTimeLogPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblRealTimeLogPath.Location = new System.Drawing.Point(90, 19);
            this.lblRealTimeLogPath.Name = "lblRealTimeLogPath";
            this.lblRealTimeLogPath.Size = new System.Drawing.Size(32, 15);
            this.lblRealTimeLogPath.TabIndex = 1;
            this.lblRealTimeLogPath.Text = "File:";
            
            // 
            // txtRealTimeLogPath - Make responsive
            // 
            this.txtRealTimeLogPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRealTimeLogPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtRealTimeLogPath.Location = new System.Drawing.Point(130, 16);
            this.txtRealTimeLogPath.Name = "txtRealTimeLogPath";
            this.txtRealTimeLogPath.Size = new System.Drawing.Size(555, 21);
            this.txtRealTimeLogPath.TabIndex = 2;
            
            // 
            // btnBrowseRealTimeLogPath - Keep on right side
            // 
            this.btnBrowseRealTimeLogPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseRealTimeLogPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnBrowseRealTimeLogPath.Location = new System.Drawing.Point(695, 15);
            this.btnBrowseRealTimeLogPath.Name = "btnBrowseRealTimeLogPath";
            this.btnBrowseRealTimeLogPath.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseRealTimeLogPath.TabIndex = 3;
            this.btnBrowseRealTimeLogPath.Text = "Browse...";
            this.btnBrowseRealTimeLogPath.UseVisualStyleBackColor = true;
            this.btnBrowseRealTimeLogPath.Click += new System.EventHandler(this.btnBrowseRealTimeLogPath_Click);
            
            // 
            // lblRealTimeStatus
            // 
            this.lblRealTimeStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRealTimeStatus.AutoSize = true;
            this.lblRealTimeStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblRealTimeStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblRealTimeStatus.Location = new System.Drawing.Point(780, 19);
            this.lblRealTimeStatus.Name = "lblRealTimeStatus";
            this.lblRealTimeStatus.Size = new System.Drawing.Size(50, 13);
            this.lblRealTimeStatus.TabIndex = 4;
            this.lblRealTimeStatus.Text = "Disabled";
            
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblLogCount,
            this.lblLastUpdated});
            this.statusStrip1.Location = new System.Drawing.Point(0, 539);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(884, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            
            // 
            // lblLogCount
            // 
            this.lblLogCount.Name = "lblLogCount";
            this.lblLogCount.Size = new System.Drawing.Size(68, 17);
            this.lblLogCount.Text = "Total Logs: 0";
            
            // 
            // lblLastUpdated - Right aligned
            // 
            this.lblLastUpdated.Name = "lblLastUpdated";
            this.lblLastUpdated.Size = new System.Drawing.Size(151, 17);
            this.lblLastUpdated.Spring = true;
            this.lblLastUpdated.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblLastUpdated.Text = "Last Updated: Not updated yet";
            
            // 
            // FormLogs - Fully responsive
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.gbRealTimeLogging);
            this.Controls.Add(this.gbActions);
            this.Controls.Add(this.gbJobFilter);
            this.Controls.Add(this.gbSearchFilter);
            this.Controls.Add(this.dgvLogs);
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "FormLogs";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FTPSyncer - Log Viewer";
            this.Load += new System.EventHandler(this.FormLogs_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLogs)).EndInit();
            this.gbSearchFilter.ResumeLayout(false);
            this.gbSearchFilter.PerformLayout();
            this.gbTimeFilter.ResumeLayout(false);
            this.gbJobFilter.ResumeLayout(false);
            this.gbJobFilter.PerformLayout();
            this.gbActions.ResumeLayout(false);
            this.gbRealTimeLogging.ResumeLayout(false);
            this.gbRealTimeLogging.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
