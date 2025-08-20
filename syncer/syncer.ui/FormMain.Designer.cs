namespace syncer.ui
{
    partial class FormMain
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectionSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scheduleSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewLogsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enhancedSftpSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sshKeyGenerationToolStripMenuItem;
        private System.Windows.Forms.DataGridView dgvJobs;
        private System.Windows.Forms.Button btnAddJob;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnPauseJob;
        private System.Windows.Forms.Button btnDeleteJob;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblServiceStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblConnectionStatus;
        private System.Windows.Forms.GroupBox gbJobs;
        private System.Windows.Forms.GroupBox gbControls;
        private System.Windows.Forms.GroupBox gbTimerJobs;
        private System.Windows.Forms.DataGridView dgvTimerJobs;
        private System.Windows.Forms.Button btnRefreshTimerJobs;
        private System.Windows.Forms.Button btnStopTimerJob;
        private System.Windows.Forms.Label lblRunningTimerJobs;

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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scheduleSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dgvJobs = new System.Windows.Forms.DataGridView();
            this.btnAddJob = new System.Windows.Forms.Button();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnPauseJob = new System.Windows.Forms.Button();
            this.btnDeleteJob = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblServiceStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblConnectionStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.gbJobs = new System.Windows.Forms.GroupBox();
            this.gbControls = new System.Windows.Forms.GroupBox();
            this.gbTimerJobs = new System.Windows.Forms.GroupBox();
            this.dgvTimerJobs = new System.Windows.Forms.DataGridView();
            this.btnRefreshTimerJobs = new System.Windows.Forms.Button();
            this.btnStopTimerJob = new System.Windows.Forms.Button();
            this.lblRunningTimerJobs = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJobs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTimerJobs)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.gbJobs.SuspendLayout();
            this.gbControls.SuspendLayout();
            this.gbTimerJobs.SuspendLayout();
            this.SuspendLayout();

            // Initialize enhancedSftpSettingsToolStripMenuItem
            this.enhancedSftpSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enhancedSftpSettingsToolStripMenuItem.Name = "enhancedSftpSettingsToolStripMenuItem";
            this.enhancedSftpSettingsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.enhancedSftpSettingsToolStripMenuItem.Text = "Enhanced SFTP Settings";
            this.enhancedSftpSettingsToolStripMenuItem.Click += new System.EventHandler(this.enhancedSftpSettingsToolStripMenuItem_Click);

            // Initialize sshKeyGenerationToolStripMenuItem
            this.sshKeyGenerationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sshKeyGenerationToolStripMenuItem.Name = "sshKeyGenerationToolStripMenuItem";
            this.sshKeyGenerationToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.sshKeyGenerationToolStripMenuItem.Text = "SSH Key Generation";
            this.sshKeyGenerationToolStripMenuItem.Click += new System.EventHandler(this.sshKeyGenerationToolStripMenuItem_Click);

            // menuStrip1
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(884, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // fileToolStripMenuItem
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // exitToolStripMenuItem
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // settingsToolStripMenuItem
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectionSettingsToolStripMenuItem,
            this.enhancedSftpSettingsToolStripMenuItem,
            this.sshKeyGenerationToolStripMenuItem,
            this.scheduleSettingsToolStripMenuItem,
            this.filterSettingsToolStripMenuItem,
            this.viewLogsToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // connectionSettingsToolStripMenuItem
            this.connectionSettingsToolStripMenuItem.Name = "connectionSettingsToolStripMenuItem";
            this.connectionSettingsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.connectionSettingsToolStripMenuItem.Text = "Connection Settings";
            this.connectionSettingsToolStripMenuItem.Click += new System.EventHandler(this.connectionSettingsToolStripMenuItem_Click);
            // scheduleSettingsToolStripMenuItem
            this.scheduleSettingsToolStripMenuItem.Name = "scheduleSettingsToolStripMenuItem";
            this.scheduleSettingsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.scheduleSettingsToolStripMenuItem.Text = "Timer Jobs";
            this.scheduleSettingsToolStripMenuItem.Click += new System.EventHandler(this.scheduleSettingsToolStripMenuItem_Click);
            // filterSettingsToolStripMenuItem
            this.filterSettingsToolStripMenuItem.Name = "filterSettingsToolStripMenuItem";
            this.filterSettingsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.filterSettingsToolStripMenuItem.Text = "Filter Settings";
            this.filterSettingsToolStripMenuItem.Click += new System.EventHandler(this.filterSettingsToolStripMenuItem_Click);
            // viewLogsToolStripMenuItem
            this.viewLogsToolStripMenuItem.Name = "viewLogsToolStripMenuItem";
            this.viewLogsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.viewLogsToolStripMenuItem.Text = "View Logs";
            this.viewLogsToolStripMenuItem.Click += new System.EventHandler(this.viewLogsToolStripMenuItem_Click);
            // helpToolStripMenuItem
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // aboutToolStripMenuItem
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // dgvJobs
            this.dgvJobs.AllowUserToAddRows = false;
            this.dgvJobs.AllowUserToDeleteRows = false;
            this.dgvJobs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvJobs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvJobs.Location = new System.Drawing.Point(6, 19);
            this.dgvJobs.MultiSelect = false;
            this.dgvJobs.Name = "dgvJobs";
            this.dgvJobs.ReadOnly = true;
            this.dgvJobs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvJobs.Size = new System.Drawing.Size(856, 400);
            this.dgvJobs.TabIndex = 1;
            this.dgvJobs.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvJobs_CellDoubleClick);
            // btnAddJob
            this.btnAddJob.BackColor = System.Drawing.Color.LightBlue;
            this.btnAddJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnAddJob.Location = new System.Drawing.Point(15, 19);
            this.btnAddJob.Name = "btnAddJob";
            this.btnAddJob.Size = new System.Drawing.Size(120, 35);
            this.btnAddJob.TabIndex = 2;
            this.btnAddJob.Text = "Add New Job";
            this.btnAddJob.UseVisualStyleBackColor = false;
            this.btnAddJob.Click += new System.EventHandler(this.btnAddJob_Click);
            // btnStartStop
            this.btnStartStop.BackColor = System.Drawing.Color.LightGreen;
            this.btnStartStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnStartStop.Location = new System.Drawing.Point(150, 19);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(120, 35);
            this.btnStartStop.TabIndex = 3;
            this.btnStartStop.Text = "Start Service";
            this.btnStartStop.UseVisualStyleBackColor = false;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // btnRefresh
            this.btnRefresh.BackColor = System.Drawing.Color.LightYellow;
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnRefresh.Location = new System.Drawing.Point(285, 19);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(120, 35);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // btnPauseJob
            this.btnPauseJob.BackColor = System.Drawing.Color.Orange;
            this.btnPauseJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnPauseJob.Location = new System.Drawing.Point(420, 19);
            this.btnPauseJob.Name = "btnPauseJob";
            this.btnPauseJob.Size = new System.Drawing.Size(120, 35);
            this.btnPauseJob.TabIndex = 5;
            this.btnPauseJob.Text = "Pause Job";
            this.btnPauseJob.UseVisualStyleBackColor = false;
            this.btnPauseJob.Click += new System.EventHandler(this.btnPauseJob_Click);
            // btnDeleteJob
            this.btnDeleteJob.BackColor = System.Drawing.Color.LightCoral;
            this.btnDeleteJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnDeleteJob.Location = new System.Drawing.Point(555, 19);
            this.btnDeleteJob.Name = "btnDeleteJob";
            this.btnDeleteJob.Size = new System.Drawing.Size(120, 35);
            this.btnDeleteJob.TabIndex = 6;
            this.btnDeleteJob.Text = "Delete Job";
            this.btnDeleteJob.UseVisualStyleBackColor = false;
            this.btnDeleteJob.Click += new System.EventHandler(this.btnDeleteJob_Click);
            // statusStrip1
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblServiceStatus,
            this.lblConnectionStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 639);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(884, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // lblServiceStatus
            this.lblServiceStatus.Name = "lblServiceStatus";
            this.lblServiceStatus.Size = new System.Drawing.Size(89, 17);
            this.lblServiceStatus.Text = "Service: Stopped";
            // lblConnectionStatus
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(128, 17);
            this.lblConnectionStatus.Text = "Connection: Disconnected";
            // gbJobs
            this.gbJobs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbJobs.Controls.Add(this.dgvJobs);
            this.gbJobs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbJobs.Location = new System.Drawing.Point(12, 100);
            this.gbJobs.Name = "gbJobs";
            this.gbJobs.Size = new System.Drawing.Size(868, 330);
            this.gbJobs.TabIndex = 6;
            this.gbJobs.TabStop = false;
            this.gbJobs.Text = "Sync Jobs";
            // gbControls
            this.gbControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbControls.Controls.Add(this.btnAddJob);
            this.gbControls.Controls.Add(this.btnStartStop);
            this.gbControls.Controls.Add(this.btnRefresh);
            this.gbControls.Controls.Add(this.btnPauseJob);
            this.gbControls.Controls.Add(this.btnDeleteJob);
            this.gbControls.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbControls.Location = new System.Drawing.Point(12, 35);
            this.gbControls.Name = "gbControls";
            this.gbControls.Size = new System.Drawing.Size(868, 65);
            this.gbControls.TabIndex = 7;
            this.gbControls.TabStop = false;
            this.gbControls.Text = "Controls";
            // gbTimerJobs
            this.gbTimerJobs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbTimerJobs.Controls.Add(this.dgvTimerJobs);
            this.gbTimerJobs.Controls.Add(this.btnRefreshTimerJobs);
            this.gbTimerJobs.Controls.Add(this.btnStopTimerJob);
            this.gbTimerJobs.Controls.Add(this.lblRunningTimerJobs);
            this.gbTimerJobs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbTimerJobs.Location = new System.Drawing.Point(12, 440);
            this.gbTimerJobs.Name = "gbTimerJobs";
            this.gbTimerJobs.Size = new System.Drawing.Size(868, 185);
            this.gbTimerJobs.TabIndex = 8;
            this.gbTimerJobs.TabStop = false;
            this.gbTimerJobs.Text = "Running Timer Jobs";

            // dgvTimerJobs
            this.dgvTimerJobs.AllowUserToAddRows = false;
            this.dgvTimerJobs.AllowUserToDeleteRows = false;
            this.dgvTimerJobs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTimerJobs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTimerJobs.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvTimerJobs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTimerJobs.Location = new System.Drawing.Point(6, 58);
            this.dgvTimerJobs.Name = "dgvTimerJobs";
            this.dgvTimerJobs.ReadOnly = true;
            this.dgvTimerJobs.RowHeadersWidth = 51;
            this.dgvTimerJobs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTimerJobs.Size = new System.Drawing.Size(856, 121);
            this.dgvTimerJobs.TabIndex = 0;

            // btnRefreshTimerJobs
            this.btnRefreshTimerJobs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnRefreshTimerJobs.Location = new System.Drawing.Point(220, 22);
            this.btnRefreshTimerJobs.Name = "btnRefreshTimerJobs";
            this.btnRefreshTimerJobs.Size = new System.Drawing.Size(120, 28);
            this.btnRefreshTimerJobs.TabIndex = 1;
            this.btnRefreshTimerJobs.Text = "Refresh List";
            this.btnRefreshTimerJobs.UseVisualStyleBackColor = true;
            this.btnRefreshTimerJobs.Click += new System.EventHandler(this.btnRefreshTimerJobs_Click);

            // btnStopTimerJob
            this.btnStopTimerJob.BackColor = System.Drawing.Color.LightCoral;
            this.btnStopTimerJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnStopTimerJob.Location = new System.Drawing.Point(346, 22);
            this.btnStopTimerJob.Name = "btnStopTimerJob";
            this.btnStopTimerJob.Size = new System.Drawing.Size(120, 28);
            this.btnStopTimerJob.TabIndex = 2;
            this.btnStopTimerJob.Text = "Stop Selected Job";
            this.btnStopTimerJob.UseVisualStyleBackColor = false;
            this.btnStopTimerJob.Click += new System.EventHandler(this.btnStopTimerJob_Click);

            // lblRunningTimerJobs
            this.lblRunningTimerJobs.AutoSize = true;
            this.lblRunningTimerJobs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblRunningTimerJobs.Location = new System.Drawing.Point(6, 28);
            this.lblRunningTimerJobs.Name = "lblRunningTimerJobs";
            this.lblRunningTimerJobs.Size = new System.Drawing.Size(191, 15);
            this.lblRunningTimerJobs.TabIndex = 3;
            this.lblRunningTimerJobs.Text = "Running Timer Jobs: 0";

            // FormMain
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 661);
            this.Controls.Add(this.gbTimerJobs);
            this.Controls.Add(this.gbControls);
            this.Controls.Add(this.gbJobs);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.Text = "DataSyncer - Main Dashboard";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJobs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTimerJobs)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.gbJobs.ResumeLayout(false);
            this.gbControls.ResumeLayout(false);
            this.gbTimerJobs.ResumeLayout(false);
            this.gbTimerJobs.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}