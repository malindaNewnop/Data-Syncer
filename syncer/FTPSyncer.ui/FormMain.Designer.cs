namespace FTPSyncer.ui
{
    partial class FormMain
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectionSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem notificationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem viewLogsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fullScreenToolStripMenuItem;
        private System.Windows.Forms.Button btnAddJob;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Button btnLoadConfiguration;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblServiceStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblConnectionStatus;
        private System.Windows.Forms.GroupBox gbControls;
        private System.Windows.Forms.GroupBox gbTimerJobs;
        private System.Windows.Forms.DataGridView dgvTimerJobs;
        private System.Windows.Forms.Button btnStopTimerJob;
        private System.Windows.Forms.Button btnEditTimerJob;
        private System.Windows.Forms.Button btnDeleteTimerJob;
        private System.Windows.Forms.Button btnResumeTimerJob;
        private System.Windows.Forms.Label lblRunningTimerJobs;
        private System.Windows.Forms.Label lblSeparator;
        
        // Bandwidth Control components
        private System.Windows.Forms.GroupBox gbBandwidthControl;
        private System.Windows.Forms.CheckBox chkEnableBandwidthControl;
        private System.Windows.Forms.Label lblUploadLimit;
        private System.Windows.Forms.NumericUpDown numUploadLimit;
        private System.Windows.Forms.Label lblUploadUnit;
        private System.Windows.Forms.Label lblDownloadLimit;
        private System.Windows.Forms.NumericUpDown numDownloadLimit;
        private System.Windows.Forms.Label lblDownloadUnit;
        private System.Windows.Forms.Button btnApplyBandwidthSettings;
        private System.Windows.Forms.Button btnResetBandwidthSettings;
        private System.Windows.Forms.Label lblCurrentUploadSpeed;
        private System.Windows.Forms.Label lblCurrentDownloadSpeed;
        
        // Quick Launch menu item
        private System.Windows.Forms.ToolStripMenuItem showQuickLaunchToolStripMenuItem;
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of bandwidth control resources
                if (_speedUpdateTimer != null)
                {
                    _speedUpdateTimer.Stop();
                    _speedUpdateTimer.Dispose();
                    _speedUpdateTimer = null;
                }
                
                // Unsubscribe from events
                if (_bandwidthService != null)
                {
                    _bandwidthService.BandwidthSettingsChanged -= BandwidthService_SettingsChanged;
                }
                
                // Dispose of other resources
                if (_trayManager != null)
                {
                    _trayManager.Dispose();
                }
                
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notificationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddJob = new System.Windows.Forms.Button();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.btnLoadConfiguration = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblServiceStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblConnectionStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.gbControls = new System.Windows.Forms.GroupBox();
            this.gbTimerJobs = new System.Windows.Forms.GroupBox();
            this.dgvTimerJobs = new System.Windows.Forms.DataGridView();
            this.btnStopTimerJob = new System.Windows.Forms.Button();
            this.btnEditTimerJob = new System.Windows.Forms.Button();
            this.btnDeleteTimerJob = new System.Windows.Forms.Button();
            this.btnResumeTimerJob = new System.Windows.Forms.Button();
            this.lblRunningTimerJobs = new System.Windows.Forms.Label();
            this.lblSeparator = new System.Windows.Forms.Label();
            this.showQuickLaunchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            
            // Initialize bandwidth control components
            this.gbBandwidthControl = new System.Windows.Forms.GroupBox();
            this.chkEnableBandwidthControl = new System.Windows.Forms.CheckBox();
            this.lblUploadLimit = new System.Windows.Forms.Label();
            this.numUploadLimit = new System.Windows.Forms.NumericUpDown();
            this.lblUploadUnit = new System.Windows.Forms.Label();
            this.lblDownloadLimit = new System.Windows.Forms.Label();
            this.numDownloadLimit = new System.Windows.Forms.NumericUpDown();
            this.lblDownloadUnit = new System.Windows.Forms.Label();
            this.btnApplyBandwidthSettings = new System.Windows.Forms.Button();
            this.btnResetBandwidthSettings = new System.Windows.Forms.Button();
            this.lblCurrentUploadSpeed = new System.Windows.Forms.Label();
            this.lblCurrentDownloadSpeed = new System.Windows.Forms.Label();
            
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTimerJobs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUploadLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDownloadLimit)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.gbControls.SuspendLayout();
            this.gbTimerJobs.SuspendLayout();
            this.gbBandwidthControl.SuspendLayout();
            this.SuspendLayout();

            // menuStrip1
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1200, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            
            // Initialize menu items first
            this.newConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newConfigurationToolStripMenuItem.Name = "newConfigurationToolStripMenuItem";
            this.newConfigurationToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.newConfigurationToolStripMenuItem.Text = "New";
            this.newConfigurationToolStripMenuItem.Click += new System.EventHandler(this.newConfigurationToolStripMenuItem_Click);

            this.saveConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveConfigurationToolStripMenuItem.Name = "saveConfigurationToolStripMenuItem";
            this.saveConfigurationToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.saveConfigurationToolStripMenuItem.Text = "Save";
            this.saveConfigurationToolStripMenuItem.Click += new System.EventHandler(this.saveConfigurationToolStripMenuItem_Click);

            this.saveAsConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsConfigurationToolStripMenuItem.Name = "saveAsConfigurationToolStripMenuItem";
            this.saveAsConfigurationToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.saveAsConfigurationToolStripMenuItem.Text = "Save As...";
            this.saveAsConfigurationToolStripMenuItem.Click += new System.EventHandler(this.saveAsConfigurationToolStripMenuItem_Click);

            this.loadConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadConfigurationToolStripMenuItem.Name = "loadConfigurationToolStripMenuItem";
            this.loadConfigurationToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.loadConfigurationToolStripMenuItem.Text = "Load Configuration";
            this.loadConfigurationToolStripMenuItem.Click += new System.EventHandler(this.loadConfigurationToolStripMenuItem_Click);
            
            // fileToolStripMenuItem
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newConfigurationToolStripMenuItem,
            this.saveConfigurationToolStripMenuItem,
            this.saveAsConfigurationToolStripMenuItem,
            this.loadConfigurationToolStripMenuItem,
            this.showQuickLaunchToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            
            // showQuickLaunchToolStripMenuItem
            this.showQuickLaunchToolStripMenuItem.Name = "showQuickLaunchToolStripMenuItem";
            this.showQuickLaunchToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.showQuickLaunchToolStripMenuItem.Text = "Quick Launch Panel";
            this.showQuickLaunchToolStripMenuItem.Enabled = true;
            this.showQuickLaunchToolStripMenuItem.Click += new System.EventHandler(this.showQuickLaunchToolStripMenuItem_Click);
            // exitToolStripMenuItem
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            
            // settingsToolStripMenuItem
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectionSettingsToolStripMenuItem,
            this.notificationsToolStripMenuItem,
            this.viewLogsToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            
            // connectionSettingsToolStripMenuItem
            this.connectionSettingsToolStripMenuItem.Name = "connectionSettingsToolStripMenuItem";
            this.connectionSettingsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.connectionSettingsToolStripMenuItem.Text = "Connection Settings";
            this.connectionSettingsToolStripMenuItem.Click += new System.EventHandler(this.connectionSettingsToolStripMenuItem_Click);
            
            // notificationsToolStripMenuItem
            this.notificationsToolStripMenuItem.Name = "notificationsToolStripMenuItem";
            this.notificationsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.notificationsToolStripMenuItem.Text = "Notifications";
            this.notificationsToolStripMenuItem.Click += new System.EventHandler(this.notificationsToolStripMenuItem_Click);
            
            // viewLogsToolStripMenuItem
            this.viewLogsToolStripMenuItem.Name = "viewLogsToolStripMenuItem";
            this.viewLogsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.viewLogsToolStripMenuItem.Text = "View Logs";
            this.viewLogsToolStripMenuItem.Click += new System.EventHandler(this.viewLogsToolStripMenuItem_Click);
            
            // viewToolStripMenuItem
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fullScreenToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            
            // fullScreenToolStripMenuItem
            this.fullScreenToolStripMenuItem.Name = "fullScreenToolStripMenuItem";
            this.fullScreenToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F11;
            this.fullScreenToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.fullScreenToolStripMenuItem.Text = "Full Screen";
            this.fullScreenToolStripMenuItem.Click += new System.EventHandler(this.fullScreenToolStripMenuItem_Click);
            
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
            // btnAddJob
            this.btnAddJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnAddJob.Location = new System.Drawing.Point(15, 19);
            this.btnAddJob.Name = "btnAddJob";
            this.btnAddJob.Size = new System.Drawing.Size(120, 35);
            this.btnAddJob.TabIndex = 2;
            this.btnAddJob.Text = "Add Timer Job";
            this.btnAddJob.UseVisualStyleBackColor = true;
            this.btnAddJob.Click += new System.EventHandler(this.btnAddJob_Click);
            // btnStartStop - Hidden (service auto-starts with jobs)
            this.btnStartStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnStartStop.Location = new System.Drawing.Point(150, 19);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(120, 35);
            this.btnStartStop.TabIndex = 3;
            this.btnStartStop.Text = "Start Service";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Visible = false; // Hide the button
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            
            // btnLoadConfiguration - Adjusted position since btnSaveCurrentConfig removed
            this.btnLoadConfiguration.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnLoadConfiguration.Location = new System.Drawing.Point(150, 19);
            this.btnLoadConfiguration.Name = "btnLoadConfiguration";
            this.btnLoadConfiguration.Size = new System.Drawing.Size(150, 35);
            this.btnLoadConfiguration.TabIndex = 4;
            this.btnLoadConfiguration.Text = "Load Configuration";
            this.btnLoadConfiguration.UseVisualStyleBackColor = true;
            this.btnLoadConfiguration.Click += new System.EventHandler(this.btnLoadConfiguration_Click);
            // statusStrip1
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblServiceStatus,
            this.lblConnectionStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 738);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1200, 22);
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
            // gbControls
            this.gbControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbControls.Controls.Add(this.btnAddJob);
            this.gbControls.Controls.Add(this.btnLoadConfiguration);
            this.gbControls.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbControls.Location = new System.Drawing.Point(12, 35);
            this.gbControls.Name = "gbControls";
            this.gbControls.Size = new System.Drawing.Size(976, 75);
            this.gbControls.TabIndex = 7;
            this.gbControls.TabStop = false;
            this.gbControls.Text = "Controls";
            
            // gbTimerJobs
            this.gbTimerJobs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbTimerJobs.Controls.Add(this.dgvTimerJobs);
            this.gbTimerJobs.Controls.Add(this.btnStopTimerJob);
            this.gbTimerJobs.Controls.Add(this.btnEditTimerJob);
            this.gbTimerJobs.Controls.Add(this.btnDeleteTimerJob);
            this.gbTimerJobs.Controls.Add(this.btnResumeTimerJob);
            this.gbTimerJobs.Controls.Add(this.lblRunningTimerJobs);
            this.gbTimerJobs.Controls.Add(this.lblSeparator);
            this.gbTimerJobs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbTimerJobs.Location = new System.Drawing.Point(12, 120);
            this.gbTimerJobs.Name = "gbTimerJobs";
            this.gbTimerJobs.Size = new System.Drawing.Size(976, 340);
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
            this.dgvTimerJobs.Location = new System.Drawing.Point(6, 105);
            this.dgvTimerJobs.Name = "dgvTimerJobs";
            this.dgvTimerJobs.RowHeadersWidth = 51;
            this.dgvTimerJobs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTimerJobs.Size = new System.Drawing.Size(964, 233);
            this.dgvTimerJobs.TabIndex = 6;

            // Job Action Group - Better spaced layout
            // btnEditTimerJob
            this.btnEditTimerJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnEditTimerJob.Location = new System.Drawing.Point(15, 55);
            this.btnEditTimerJob.Name = "btnEditTimerJob";
            this.btnEditTimerJob.Size = new System.Drawing.Size(130, 35);
            this.btnEditTimerJob.TabIndex = 1;
            this.btnEditTimerJob.Text = "Edit Job";
            this.btnEditTimerJob.UseVisualStyleBackColor = true;
            this.btnEditTimerJob.Click += new System.EventHandler(this.btnEditTimerJob_Click);

            // btnResumeTimerJob
            this.btnResumeTimerJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnResumeTimerJob.Location = new System.Drawing.Point(160, 55);
            this.btnResumeTimerJob.Name = "btnResumeTimerJob";
            this.btnResumeTimerJob.Size = new System.Drawing.Size(130, 35);
            this.btnResumeTimerJob.TabIndex = 2;
            this.btnResumeTimerJob.Text = "Start Job";
            this.btnResumeTimerJob.UseVisualStyleBackColor = true;
            this.btnResumeTimerJob.Click += new System.EventHandler(this.btnResumeTimerJob_Click);

            // btnStopTimerJob
            this.btnStopTimerJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnStopTimerJob.Location = new System.Drawing.Point(305, 55);
            this.btnStopTimerJob.Name = "btnStopTimerJob";
            this.btnStopTimerJob.Size = new System.Drawing.Size(130, 35);
            this.btnStopTimerJob.TabIndex = 3;
            this.btnStopTimerJob.Text = "Stop Job";
            this.btnStopTimerJob.UseVisualStyleBackColor = true;
            this.btnStopTimerJob.Click += new System.EventHandler(this.btnStopTimerJob_Click);

            // btnDeleteTimerJob - Separated with extra space for safety
            this.btnDeleteTimerJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnDeleteTimerJob.Location = new System.Drawing.Point(460, 55);
            this.btnDeleteTimerJob.Name = "btnDeleteTimerJob";
            this.btnDeleteTimerJob.Size = new System.Drawing.Size(130, 35);
            this.btnDeleteTimerJob.TabIndex = 4;
            this.btnDeleteTimerJob.Text = "Delete Job";
            this.btnDeleteTimerJob.UseVisualStyleBackColor = true;
            this.btnDeleteTimerJob.Click += new System.EventHandler(this.btnDeleteTimerJob_Click);

            // lblSeparator - Visual separator line
            this.lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSeparator.Location = new System.Drawing.Point(15, 100);
            this.lblSeparator.Name = "lblSeparator";
            this.lblSeparator.Size = new System.Drawing.Size(950, 2);
            this.lblSeparator.TabIndex = 7;

            // lblRunningTimerJobs - Job count display with better spacing
            this.lblRunningTimerJobs.AutoSize = true;
            this.lblRunningTimerJobs.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.lblRunningTimerJobs.Location = new System.Drawing.Point(15, 25);
            this.lblRunningTimerJobs.Name = "lblRunningTimerJobs";
            this.lblRunningTimerJobs.Size = new System.Drawing.Size(191, 18);
            this.lblRunningTimerJobs.TabIndex = 5;
            this.lblRunningTimerJobs.Text = "Active Timer Jobs: 0";
            
            // gbBandwidthControl
            this.gbBandwidthControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbBandwidthControl.Controls.Add(this.chkEnableBandwidthControl);
            this.gbBandwidthControl.Controls.Add(this.lblUploadLimit);
            this.gbBandwidthControl.Controls.Add(this.numUploadLimit);
            this.gbBandwidthControl.Controls.Add(this.lblUploadUnit);
            this.gbBandwidthControl.Controls.Add(this.lblDownloadLimit);
            this.gbBandwidthControl.Controls.Add(this.numDownloadLimit);
            this.gbBandwidthControl.Controls.Add(this.lblDownloadUnit);
            this.gbBandwidthControl.Controls.Add(this.btnApplyBandwidthSettings);
            this.gbBandwidthControl.Controls.Add(this.btnResetBandwidthSettings);
            this.gbBandwidthControl.Controls.Add(this.lblCurrentUploadSpeed);
            this.gbBandwidthControl.Controls.Add(this.lblCurrentDownloadSpeed);
            this.gbBandwidthControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbBandwidthControl.Location = new System.Drawing.Point(12, 470);
            this.gbBandwidthControl.Name = "gbBandwidthControl";
            this.gbBandwidthControl.Size = new System.Drawing.Size(976, 100);
            this.gbBandwidthControl.TabIndex = 9;
            this.gbBandwidthControl.TabStop = false;
            this.gbBandwidthControl.Text = "Bandwidth Control";
            
            // chkEnableBandwidthControl
            this.chkEnableBandwidthControl.AutoSize = true;
            this.chkEnableBandwidthControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkEnableBandwidthControl.Location = new System.Drawing.Point(15, 25);
            this.chkEnableBandwidthControl.Name = "chkEnableBandwidthControl";
            this.chkEnableBandwidthControl.Size = new System.Drawing.Size(157, 19);
            this.chkEnableBandwidthControl.TabIndex = 0;
            this.chkEnableBandwidthControl.Text = "Enable Bandwidth Control";
            this.chkEnableBandwidthControl.UseVisualStyleBackColor = true;
            this.chkEnableBandwidthControl.CheckedChanged += new System.EventHandler(this.chkEnableBandwidthControl_CheckedChanged);
            
            // lblUploadLimit
            this.lblUploadLimit.AutoSize = true;
            this.lblUploadLimit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblUploadLimit.Location = new System.Drawing.Point(200, 26);
            this.lblUploadLimit.Name = "lblUploadLimit";
            this.lblUploadLimit.Size = new System.Drawing.Size(77, 15);
            this.lblUploadLimit.TabIndex = 1;
            this.lblUploadLimit.Text = "Upload Limit:";
            
            // numUploadLimit
            this.numUploadLimit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.numUploadLimit.Location = new System.Drawing.Point(283, 24);
            this.numUploadLimit.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            this.numUploadLimit.Name = "numUploadLimit";
            this.numUploadLimit.Size = new System.Drawing.Size(80, 21);
            this.numUploadLimit.TabIndex = 2;
            
            // lblUploadUnit
            this.lblUploadUnit.AutoSize = true;
            this.lblUploadUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblUploadUnit.Location = new System.Drawing.Point(369, 26);
            this.lblUploadUnit.Name = "lblUploadUnit";
            this.lblUploadUnit.Size = new System.Drawing.Size(88, 15);
            this.lblUploadUnit.TabIndex = 3;
            this.lblUploadUnit.Text = "KB/s (0=unlim.)";
            
            // lblDownloadLimit
            this.lblDownloadLimit.AutoSize = true;
            this.lblDownloadLimit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblDownloadLimit.Location = new System.Drawing.Point(470, 26);
            this.lblDownloadLimit.Name = "lblDownloadLimit";
            this.lblDownloadLimit.Size = new System.Drawing.Size(93, 15);
            this.lblDownloadLimit.TabIndex = 4;
            this.lblDownloadLimit.Text = "Download Limit:";
            
            // numDownloadLimit
            this.numDownloadLimit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.numDownloadLimit.Location = new System.Drawing.Point(569, 24);
            this.numDownloadLimit.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            this.numDownloadLimit.Name = "numDownloadLimit";
            this.numDownloadLimit.Size = new System.Drawing.Size(80, 21);
            this.numDownloadLimit.TabIndex = 5;
            
            // lblDownloadUnit
            this.lblDownloadUnit.AutoSize = true;
            this.lblDownloadUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblDownloadUnit.Location = new System.Drawing.Point(655, 26);
            this.lblDownloadUnit.Name = "lblDownloadUnit";
            this.lblDownloadUnit.Size = new System.Drawing.Size(88, 15);
            this.lblDownloadUnit.TabIndex = 6;
            this.lblDownloadUnit.Text = "KB/s (0=unlim.)";
            
            // btnApplyBandwidthSettings
            this.btnApplyBandwidthSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnApplyBandwidthSettings.Location = new System.Drawing.Point(770, 22);
            this.btnApplyBandwidthSettings.Name = "btnApplyBandwidthSettings";
            this.btnApplyBandwidthSettings.Size = new System.Drawing.Size(75, 25);
            this.btnApplyBandwidthSettings.TabIndex = 7;
            this.btnApplyBandwidthSettings.Text = "Apply";
            this.btnApplyBandwidthSettings.UseVisualStyleBackColor = true;
            this.btnApplyBandwidthSettings.Click += new System.EventHandler(this.btnApplyBandwidthSettings_Click);
            
            // btnResetBandwidthSettings
            this.btnResetBandwidthSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnResetBandwidthSettings.Location = new System.Drawing.Point(851, 22);
            this.btnResetBandwidthSettings.Name = "btnResetBandwidthSettings";
            this.btnResetBandwidthSettings.Size = new System.Drawing.Size(75, 25);
            this.btnResetBandwidthSettings.TabIndex = 8;
            this.btnResetBandwidthSettings.Text = "Reset";
            this.btnResetBandwidthSettings.UseVisualStyleBackColor = true;
            this.btnResetBandwidthSettings.Click += new System.EventHandler(this.btnResetBandwidthSettings_Click);
            
            // lblCurrentUploadSpeed
            this.lblCurrentUploadSpeed.AutoSize = true;
            this.lblCurrentUploadSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblCurrentUploadSpeed.Location = new System.Drawing.Point(15, 55);
            this.lblCurrentUploadSpeed.Name = "lblCurrentUploadSpeed";
            this.lblCurrentUploadSpeed.Size = new System.Drawing.Size(122, 15);
            this.lblCurrentUploadSpeed.TabIndex = 9;
            this.lblCurrentUploadSpeed.Text = "Current Upload: 0 B/s";
            
            // lblCurrentDownloadSpeed
            this.lblCurrentDownloadSpeed.AutoSize = true;
            this.lblCurrentDownloadSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblCurrentDownloadSpeed.Location = new System.Drawing.Point(15, 75);
            this.lblCurrentDownloadSpeed.Name = "lblCurrentDownloadSpeed";
            this.lblCurrentDownloadSpeed.Size = new System.Drawing.Size(138, 15);
            this.lblCurrentDownloadSpeed.TabIndex = 10;
            this.lblCurrentDownloadSpeed.Text = "Current Download: 0 B/s";

            // FormMain
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.gbBandwidthControl);
            this.Controls.Add(this.gbTimerJobs);
            this.Controls.Add(this.gbControls);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FTPSyncer - Main Dashboard";
            this.WindowState = System.Windows.Forms.FormWindowState.Normal;
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTimerJobs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUploadLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDownloadLimit)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.gbControls.ResumeLayout(false);
            this.gbTimerJobs.ResumeLayout(false);
            this.gbTimerJobs.PerformLayout();
            this.gbBandwidthControl.ResumeLayout(false);
            this.gbBandwidthControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}




