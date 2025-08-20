namespace syncer.ui
{
    partial class FormSchedule
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.GroupBox gbJobDetails;
        private System.Windows.Forms.Label lblJobName;
        private System.Windows.Forms.TextBox txtJobName;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.GroupBox gbPaths;
        private System.Windows.Forms.Label lblSourcePath;
        private System.Windows.Forms.TextBox txtSourcePath;
        private System.Windows.Forms.Button btnBrowseSource;
        private System.Windows.Forms.Label lblDestinationPath;
        private System.Windows.Forms.TextBox txtDestinationPath;
        private System.Windows.Forms.Button btnBrowseDestination;
        private System.Windows.Forms.GroupBox gbScheduleSettings;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.DateTimePicker dtpStartTime;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.NumericUpDown numInterval;
        private System.Windows.Forms.ComboBox cmbIntervalType;
        private System.Windows.Forms.Label lblTransferMode;
        private System.Windows.Forms.ComboBox cmbTransferMode;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox gbFileManager;
        private System.Windows.Forms.Button btnUploadFile;
        private System.Windows.Forms.Button btnDownloadFile;
        private System.Windows.Forms.Button btnOpenFileManager;
        private System.Windows.Forms.Label lblFileOperations;

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
            this.gbJobDetails = new System.Windows.Forms.GroupBox();
            this.lblJobName = new System.Windows.Forms.Label();
            this.txtJobName = new System.Windows.Forms.TextBox();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.gbPaths = new System.Windows.Forms.GroupBox();
            this.lblSourcePath = new System.Windows.Forms.Label();
            this.txtSourcePath = new System.Windows.Forms.TextBox();
            this.btnBrowseSource = new System.Windows.Forms.Button();
            this.lblDestinationPath = new System.Windows.Forms.Label();
            this.txtDestinationPath = new System.Windows.Forms.TextBox();
            this.btnBrowseDestination = new System.Windows.Forms.Button();
            this.gbScheduleSettings = new System.Windows.Forms.GroupBox();
            this.lblStartTime = new System.Windows.Forms.Label();
            this.dtpStartTime = new System.Windows.Forms.DateTimePicker();
            this.lblInterval = new System.Windows.Forms.Label();
            this.numInterval = new System.Windows.Forms.NumericUpDown();
            this.cmbIntervalType = new System.Windows.Forms.ComboBox();
            this.lblTransferMode = new System.Windows.Forms.Label();
            this.cmbTransferMode = new System.Windows.Forms.ComboBox();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbFileManager = new System.Windows.Forms.GroupBox();
            this.lblFileOperations = new System.Windows.Forms.Label();
            this.btnUploadFile = new System.Windows.Forms.Button();
            this.btnDownloadFile = new System.Windows.Forms.Button();
            this.btnOpenFileManager = new System.Windows.Forms.Button();
            this.gbJobDetails.SuspendLayout();
            this.gbPaths.SuspendLayout();
            this.gbScheduleSettings.SuspendLayout();
            this.gbFileManager.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).BeginInit();
            this.SuspendLayout();
            // gbJobDetails
            this.gbJobDetails.Controls.Add(this.lblJobName);
            this.gbJobDetails.Controls.Add(this.txtJobName);
            this.gbJobDetails.Controls.Add(this.chkEnabled);
            this.gbJobDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbJobDetails.Location = new System.Drawing.Point(12, 12);
            this.gbJobDetails.Name = "gbJobDetails";
            this.gbJobDetails.Size = new System.Drawing.Size(560, 80);
            this.gbJobDetails.TabIndex = 0;
            this.gbJobDetails.TabStop = false;
            this.gbJobDetails.Text = "Job Details";
            // lblJobName
            this.lblJobName.AutoSize = true;
            this.lblJobName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblJobName.Location = new System.Drawing.Point(15, 25);
            this.lblJobName.Name = "lblJobName";
            this.lblJobName.Size = new System.Drawing.Size(69, 15);
            this.lblJobName.TabIndex = 0;
            this.lblJobName.Text = "Job Name:";
            // txtJobName
            this.txtJobName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtJobName.Location = new System.Drawing.Point(100, 22);
            this.txtJobName.Name = "txtJobName";
            this.txtJobName.Size = new System.Drawing.Size(300, 21);
            this.txtJobName.TabIndex = 1;
            // chkEnabled
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkEnabled.Location = new System.Drawing.Point(100, 50);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(98, 19);
            this.chkEnabled.TabIndex = 2;
            this.chkEnabled.Text = "Enable Job";
            this.chkEnabled.UseVisualStyleBackColor = true;
            this.chkEnabled.CheckedChanged += new System.EventHandler(this.chkEnabled_CheckedChanged);
            // gbPaths
            this.gbPaths.Controls.Add(this.lblSourcePath);
            this.gbPaths.Controls.Add(this.txtSourcePath);
            this.gbPaths.Controls.Add(this.btnBrowseSource);
            this.gbPaths.Controls.Add(this.lblDestinationPath);
            this.gbPaths.Controls.Add(this.txtDestinationPath);
            this.gbPaths.Controls.Add(this.btnBrowseDestination);
            this.gbPaths.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbPaths.Location = new System.Drawing.Point(12, 100);
            this.gbPaths.Name = "gbPaths";
            this.gbPaths.Size = new System.Drawing.Size(560, 120);
            this.gbPaths.TabIndex = 1;
            this.gbPaths.TabStop = false;
            this.gbPaths.Text = "Source and Destination";
            // lblSourcePath
            this.lblSourcePath.AutoSize = true;
            this.lblSourcePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblSourcePath.Location = new System.Drawing.Point(15, 30);
            this.lblSourcePath.Name = "lblSourcePath";
            this.lblSourcePath.Size = new System.Drawing.Size(82, 15);
            this.lblSourcePath.TabIndex = 0;
            this.lblSourcePath.Text = "Source Folder:";
            // txtSourcePath
            this.txtSourcePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtSourcePath.Location = new System.Drawing.Point(120, 27);
            this.txtSourcePath.Name = "txtSourcePath";
            this.txtSourcePath.ReadOnly = true;
            this.txtSourcePath.Size = new System.Drawing.Size(350, 21);
            this.txtSourcePath.TabIndex = 1;
            // btnBrowseSource
            this.btnBrowseSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnBrowseSource.Location = new System.Drawing.Point(480, 26);
            this.btnBrowseSource.Name = "btnBrowseSource";
            this.btnBrowseSource.Size = new System.Drawing.Size(65, 23);
            this.btnBrowseSource.TabIndex = 2;
            this.btnBrowseSource.Text = "Browse...";
            this.btnBrowseSource.UseVisualStyleBackColor = true;
            this.btnBrowseSource.Click += new System.EventHandler(this.btnBrowseSource_Click);
            // lblDestinationPath
            this.lblDestinationPath.AutoSize = true;
            this.lblDestinationPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblDestinationPath.Location = new System.Drawing.Point(15, 70);
            this.lblDestinationPath.Name = "lblDestinationPath";
            this.lblDestinationPath.Size = new System.Drawing.Size(99, 15);
            this.lblDestinationPath.TabIndex = 3;
            this.lblDestinationPath.Text = "Destination Path:";
            // txtDestinationPath
            this.txtDestinationPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtDestinationPath.Location = new System.Drawing.Point(120, 67);
            this.txtDestinationPath.Name = "txtDestinationPath";
            this.txtDestinationPath.Size = new System.Drawing.Size(350, 21);
            this.txtDestinationPath.TabIndex = 4;
            // btnBrowseDestination
            this.btnBrowseDestination.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnBrowseDestination.Location = new System.Drawing.Point(480, 66);
            this.btnBrowseDestination.Name = "btnBrowseDestination";
            this.btnBrowseDestination.Size = new System.Drawing.Size(65, 23);
            this.btnBrowseDestination.TabIndex = 5;
            this.btnBrowseDestination.Text = "Browse...";
            this.btnBrowseDestination.UseVisualStyleBackColor = true;
            this.btnBrowseDestination.Click += new System.EventHandler(this.btnBrowseDestination_Click);
            // gbScheduleSettings
            this.gbScheduleSettings.Controls.Add(this.lblStartTime);
            this.gbScheduleSettings.Controls.Add(this.dtpStartTime);
            this.gbScheduleSettings.Controls.Add(this.lblInterval);
            this.gbScheduleSettings.Controls.Add(this.numInterval);
            this.gbScheduleSettings.Controls.Add(this.cmbIntervalType);
            this.gbScheduleSettings.Controls.Add(this.lblTransferMode);
            this.gbScheduleSettings.Controls.Add(this.cmbTransferMode);
            this.gbScheduleSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbScheduleSettings.Location = new System.Drawing.Point(12, 230);
            this.gbScheduleSettings.Name = "gbScheduleSettings";
            this.gbScheduleSettings.Size = new System.Drawing.Size(560, 150);
            this.gbScheduleSettings.TabIndex = 2;
            this.gbScheduleSettings.TabStop = false;
            this.gbScheduleSettings.Text = "Schedule Settings";
            // lblStartTime
            this.lblStartTime.AutoSize = true;
            this.lblStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblStartTime.Location = new System.Drawing.Point(15, 30);
            this.lblStartTime.Name = "lblStartTime";
            this.lblStartTime.Size = new System.Drawing.Size(67, 15);
            this.lblStartTime.TabIndex = 0;
            this.lblStartTime.Text = "Start Time:";
            // dtpStartTime
            this.dtpStartTime.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dtpStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartTime.Location = new System.Drawing.Point(120, 27);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.Size = new System.Drawing.Size(180, 21);
            this.dtpStartTime.TabIndex = 1;
            // lblInterval
            this.lblInterval.AutoSize = true;
            this.lblInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblInterval.Location = new System.Drawing.Point(15, 65);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(88, 15);
            this.lblInterval.TabIndex = 2;
            this.lblInterval.Text = "Repeat Every:";
            // numInterval
            this.numInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.numInterval.Location = new System.Drawing.Point(120, 62);
            this.numInterval.Maximum = new decimal(new int[] {
            1440,
            0,
            0,
            0});
            this.numInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numInterval.Name = "numInterval";
            this.numInterval.Size = new System.Drawing.Size(80, 21);
            this.numInterval.TabIndex = 3;
            this.numInterval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // cmbIntervalType
            this.cmbIntervalType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIntervalType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.cmbIntervalType.FormattingEnabled = true;
            this.cmbIntervalType.Items.AddRange(new object[] {
            "Minutes",
            "Hours",
            "Days"});
            this.cmbIntervalType.Location = new System.Drawing.Point(210, 62);
            this.cmbIntervalType.Name = "cmbIntervalType";
            this.cmbIntervalType.Size = new System.Drawing.Size(100, 23);
            this.cmbIntervalType.TabIndex = 4;
            this.cmbIntervalType.SelectedIndexChanged += new System.EventHandler(this.cmbIntervalType_SelectedIndexChanged);
            // lblTransferMode
            this.lblTransferMode.AutoSize = true;
            this.lblTransferMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblTransferMode.Location = new System.Drawing.Point(15, 100);
            this.lblTransferMode.Name = "lblTransferMode";
            this.lblTransferMode.Size = new System.Drawing.Size(87, 15);
            this.lblTransferMode.TabIndex = 5;
            this.lblTransferMode.Text = "Transfer Mode:";
            // cmbTransferMode
            this.cmbTransferMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTransferMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.cmbTransferMode.FormattingEnabled = true;
            this.cmbTransferMode.Items.AddRange(new object[] {
            "Copy (Keep both files)",
            "Sync (Mirror destination)",
            "Move (Delete from source)"});
            this.cmbTransferMode.Location = new System.Drawing.Point(120, 97);
            this.cmbTransferMode.Name = "cmbTransferMode";
            this.cmbTransferMode.Size = new System.Drawing.Size(200, 23);
            this.cmbTransferMode.TabIndex = 6;
            // gbFileManager
            this.gbFileManager.Controls.Add(this.lblFileOperations);
            this.gbFileManager.Controls.Add(this.btnUploadFile);
            this.gbFileManager.Controls.Add(this.btnDownloadFile);
            this.gbFileManager.Controls.Add(this.btnOpenFileManager);
            this.gbFileManager.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbFileManager.Location = new System.Drawing.Point(12, 390);
            this.gbFileManager.Name = "gbFileManager";
            this.gbFileManager.Size = new System.Drawing.Size(560, 100);
            this.gbFileManager.TabIndex = 3;
            this.gbFileManager.TabStop = false;
            this.gbFileManager.Text = "File Manager (FTP/SFTP Operations)";
            // lblFileOperations
            this.lblFileOperations.AutoSize = true;
            this.lblFileOperations.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblFileOperations.Location = new System.Drawing.Point(15, 25);
            this.lblFileOperations.Name = "lblFileOperations";
            this.lblFileOperations.Size = new System.Drawing.Size(350, 15);
            this.lblFileOperations.TabIndex = 0;
            this.lblFileOperations.Text = "Upload/Download files to/from remote server (requires connection):";
            // btnUploadFile
            this.btnUploadFile.BackColor = System.Drawing.Color.LightBlue;
            this.btnUploadFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnUploadFile.Location = new System.Drawing.Point(15, 50);
            this.btnUploadFile.Name = "btnUploadFile";
            this.btnUploadFile.Size = new System.Drawing.Size(120, 30);
            this.btnUploadFile.TabIndex = 1;
            this.btnUploadFile.Text = "Upload File(s)";
            this.btnUploadFile.UseVisualStyleBackColor = false;
            this.btnUploadFile.Click += new System.EventHandler(this.btnUploadFile_Click);
            // btnDownloadFile
            this.btnDownloadFile.BackColor = System.Drawing.Color.LightGreen;
            this.btnDownloadFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnDownloadFile.Location = new System.Drawing.Point(150, 50);
            this.btnDownloadFile.Name = "btnDownloadFile";
            this.btnDownloadFile.Size = new System.Drawing.Size(120, 30);
            this.btnDownloadFile.TabIndex = 2;
            this.btnDownloadFile.Text = "Download File(s)";
            this.btnDownloadFile.UseVisualStyleBackColor = false;
            this.btnDownloadFile.Click += new System.EventHandler(this.btnDownloadFile_Click);
           
            // btnPreview
            this.btnPreview.BackColor = System.Drawing.Color.LightYellow;
            this.btnPreview.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnPreview.Location = new System.Drawing.Point(12, 500);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(100, 30);
            this.btnPreview.TabIndex = 4;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = false;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // btnSave
            this.btnSave.BackColor = System.Drawing.Color.LightGreen;
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(450, 500);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 30);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // btnCancel
            this.btnCancel.BackColor = System.Drawing.Color.LightCoral;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Location = new System.Drawing.Point(540, 500);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // FormSchedule
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 540);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.gbFileManager);
            this.Controls.Add(this.gbScheduleSettings);
            this.Controls.Add(this.gbPaths);
            this.Controls.Add(this.gbJobDetails);
            this.Name = "FormSchedule";
            this.Text = "Schedule Settings";
            this.gbJobDetails.ResumeLayout(false);
            this.gbJobDetails.PerformLayout();
            this.gbPaths.ResumeLayout(false);
            this.gbPaths.PerformLayout();
            this.gbScheduleSettings.ResumeLayout(false);
            this.gbScheduleSettings.PerformLayout();
            this.gbFileManager.ResumeLayout(false);
            this.gbFileManager.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
