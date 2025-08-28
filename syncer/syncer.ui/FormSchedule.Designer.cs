namespace syncer.ui
{
    partial class FormSchedule
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbJobDetails = new System.Windows.Forms.GroupBox();
            this.txtJobName = new System.Windows.Forms.TextBox();
            this.lblJobName = new System.Windows.Forms.Label();
            this.chkEnableJob = new System.Windows.Forms.CheckBox();
            this.gbTimerSettings = new System.Windows.Forms.GroupBox();
            this.chkEnableTimer = new System.Windows.Forms.CheckBox();
            this.lblUploadEvery = new System.Windows.Forms.Label();
            this.numTimerInterval = new System.Windows.Forms.NumericUpDown();
            this.cmbTimerUnit = new System.Windows.Forms.ComboBox();
            this.btnStartTimer = new System.Windows.Forms.Button();
            this.btnStopTimer = new System.Windows.Forms.Button();
            this.lblTimerStatus = new System.Windows.Forms.Label();
            this.lblLastUpload = new System.Windows.Forms.Label();
            this.gbFileManager = new System.Windows.Forms.GroupBox();
            this.lblFileSelection = new System.Windows.Forms.Label();
            this.btnBrowseFilesForTimer = new System.Windows.Forms.Button();
            this.lblNoFilesSelected = new System.Windows.Forms.Label();
            this.lblManualOperations = new System.Windows.Forms.Label();
            this.btnUploadFiles = new System.Windows.Forms.Button();
            this.btnDownloadFile = new System.Windows.Forms.Button();
            this.btnSaveTimerJob = new System.Windows.Forms.Button();
            this.btnLoadConfiguration = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbJobDetails.SuspendLayout();
            this.gbTimerSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimerInterval)).BeginInit();
            this.gbFileManager.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbJobDetails
            // 
            this.gbJobDetails.Controls.Add(this.chkEnableJob);
            this.gbJobDetails.Controls.Add(this.lblJobName);
            this.gbJobDetails.Controls.Add(this.txtJobName);
            this.gbJobDetails.Location = new System.Drawing.Point(12, 12);
            this.gbJobDetails.Name = "gbJobDetails";
            this.gbJobDetails.Size = new System.Drawing.Size(756, 60);
            this.gbJobDetails.TabIndex = 0;
            this.gbJobDetails.TabStop = false;
            this.gbJobDetails.Text = "Job Details";
            // 
            // txtJobName
            // 
            this.txtJobName.Location = new System.Drawing.Point(80, 25);
            this.txtJobName.Name = "txtJobName";
            this.txtJobName.Size = new System.Drawing.Size(445, 20);
            this.txtJobName.TabIndex = 0;
            // 
            // lblJobName
            // 
            this.lblJobName.AutoSize = true;
            this.lblJobName.Location = new System.Drawing.Point(15, 28);
            this.lblJobName.Name = "lblJobName";
            this.lblJobName.Size = new System.Drawing.Size(59, 13);
            this.lblJobName.TabIndex = 1;
            this.lblJobName.Text = "Job Name:";
            // 
            // chkEnableJob
            // 
            this.chkEnableJob.AutoSize = true;
            this.chkEnableJob.Checked = true;
            this.chkEnableJob.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableJob.Location = new System.Drawing.Point(540, 27);
            this.chkEnableJob.Name = "chkEnableJob";
            this.chkEnableJob.Size = new System.Drawing.Size(79, 17);
            this.chkEnableJob.TabIndex = 2;
            this.chkEnableJob.Text = "Enable Job";
            this.chkEnableJob.UseVisualStyleBackColor = true;
            // 
            // gbTimerSettings
            // 
            this.gbTimerSettings.Controls.Add(this.lblLastUpload);
            this.gbTimerSettings.Controls.Add(this.lblTimerStatus);
            this.gbTimerSettings.Controls.Add(this.btnStopTimer);
            this.gbTimerSettings.Controls.Add(this.btnStartTimer);
            this.gbTimerSettings.Controls.Add(this.cmbTimerUnit);
            this.gbTimerSettings.Controls.Add(this.numTimerInterval);
            this.gbTimerSettings.Controls.Add(this.lblUploadEvery);
            this.gbTimerSettings.Controls.Add(this.chkEnableTimer);
            this.gbTimerSettings.Location = new System.Drawing.Point(12, 85);
            this.gbTimerSettings.Name = "gbTimerSettings";
            this.gbTimerSettings.Size = new System.Drawing.Size(370, 120);
            this.gbTimerSettings.TabIndex = 1;
            this.gbTimerSettings.TabStop = false;
            this.gbTimerSettings.Text = "Upload Timer Settings";
            // 
            // chkEnableTimer
            // 
            this.chkEnableTimer.AutoSize = true;
            this.chkEnableTimer.Location = new System.Drawing.Point(15, 25);
            this.chkEnableTimer.Name = "chkEnableTimer";
            this.chkEnableTimer.Size = new System.Drawing.Size(86, 17);
            this.chkEnableTimer.TabIndex = 0;
            this.chkEnableTimer.Text = "Enable Timer";
            this.chkEnableTimer.UseVisualStyleBackColor = true;
            this.chkEnableTimer.CheckedChanged += new System.EventHandler(this.chkEnableTimer_CheckedChanged);
            // 
            // lblUploadEvery
            // 
            this.lblUploadEvery.AutoSize = true;
            this.lblUploadEvery.Location = new System.Drawing.Point(15, 55);
            this.lblUploadEvery.Name = "lblUploadEvery";
            this.lblUploadEvery.Size = new System.Drawing.Size(75, 13);
            this.lblUploadEvery.TabIndex = 1;
            this.lblUploadEvery.Text = "Upload Every:";
            // 
            // numTimerInterval
            // 
            this.numTimerInterval.Location = new System.Drawing.Point(96, 53);
            this.numTimerInterval.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numTimerInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numTimerInterval.Name = "numTimerInterval";
            this.numTimerInterval.Size = new System.Drawing.Size(60, 20);
            this.numTimerInterval.TabIndex = 2;
            this.numTimerInterval.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // cmbTimerUnit
            // 
            this.cmbTimerUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTimerUnit.FormattingEnabled = true;
            this.cmbTimerUnit.Items.AddRange(new object[] {
            "Seconds",
            "Minutes",
            "Hours"});
            this.cmbTimerUnit.Location = new System.Drawing.Point(162, 53);
            this.cmbTimerUnit.Name = "cmbTimerUnit";
            this.cmbTimerUnit.Size = new System.Drawing.Size(80, 21);
            this.cmbTimerUnit.TabIndex = 3;
            // 
            // btnStartTimer
            // 
            this.btnStartTimer.BackColor = System.Drawing.Color.LightGreen;
            this.btnStartTimer.Enabled = false;
            this.btnStartTimer.Location = new System.Drawing.Point(15, 85);
            this.btnStartTimer.Name = "btnStartTimer";
            this.btnStartTimer.Size = new System.Drawing.Size(80, 25);
            this.btnStartTimer.TabIndex = 4;
            this.btnStartTimer.Text = "Start Timer";
            this.btnStartTimer.UseVisualStyleBackColor = false;
            this.btnStartTimer.Click += new System.EventHandler(this.btnStartTimer_Click);
            // 
            // btnStopTimer
            // 
            this.btnStopTimer.BackColor = System.Drawing.Color.LightCoral;
            this.btnStopTimer.Enabled = false;
            this.btnStopTimer.Location = new System.Drawing.Point(105, 85);
            this.btnStopTimer.Name = "btnStopTimer";
            this.btnStopTimer.Size = new System.Drawing.Size(80, 25);
            this.btnStopTimer.TabIndex = 5;
            this.btnStopTimer.Text = "Stop Timer";
            this.btnStopTimer.UseVisualStyleBackColor = false;
            this.btnStopTimer.Click += new System.EventHandler(this.btnStopTimer_Click);
            // 
            // lblTimerStatus
            // 
            this.lblTimerStatus.AutoSize = true;
            this.lblTimerStatus.Location = new System.Drawing.Point(260, 28);
            this.lblTimerStatus.Name = "lblTimerStatus";
            this.lblTimerStatus.Size = new System.Drawing.Size(73, 13);
            this.lblTimerStatus.TabIndex = 6;
            this.lblTimerStatus.Text = "Timer stopped";
            // 
            // lblLastUpload
            // 
            this.lblLastUpload.AutoSize = true;
            this.lblLastUpload.Location = new System.Drawing.Point(260, 55);
            this.lblLastUpload.Name = "lblLastUpload";
            this.lblLastUpload.Size = new System.Drawing.Size(36, 13);
            this.lblLastUpload.TabIndex = 7;
            this.lblLastUpload.Text = "Never";
            // 
            // gbFileManager
            // 
            this.gbFileManager.Controls.Add(this.btnDownloadFile);
            this.gbFileManager.Controls.Add(this.btnUploadFiles);
            this.gbFileManager.Controls.Add(this.lblManualOperations);
            this.gbFileManager.Controls.Add(this.lblNoFilesSelected);
            this.gbFileManager.Controls.Add(this.btnBrowseFilesForTimer);
            this.gbFileManager.Controls.Add(this.lblFileSelection);
            this.gbFileManager.Location = new System.Drawing.Point(398, 85);
            this.gbFileManager.Name = "gbFileManager";
            this.gbFileManager.Size = new System.Drawing.Size(370, 120);
            this.gbFileManager.TabIndex = 2;
            this.gbFileManager.TabStop = false;
            this.gbFileManager.Text = "File Manager";
            // 
            // lblFileSelection
            // 
            this.lblFileSelection.Location = new System.Drawing.Point(15, 25);
            this.lblFileSelection.Name = "lblFileSelection";
            this.lblFileSelection.Size = new System.Drawing.Size(340, 13);
            this.lblFileSelection.TabIndex = 0;
            this.lblFileSelection.Text = "File Selection: All files in the selected folder will be uploaded automatically.";
            // 
            // btnBrowseFilesForTimer
            // 
            this.btnBrowseFilesForTimer.Location = new System.Drawing.Point(15, 45);
            this.btnBrowseFilesForTimer.Name = "btnBrowseFilesForTimer";
            this.btnBrowseFilesForTimer.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseFilesForTimer.TabIndex = 1;
            this.btnBrowseFilesForTimer.Text = "Browse";
            this.btnBrowseFilesForTimer.UseVisualStyleBackColor = true;
            this.btnBrowseFilesForTimer.Click += new System.EventHandler(this.btnBrowseFilesForTimer_Click);
            // 
            // lblNoFilesSelected
            // 
            this.lblNoFilesSelected.AutoSize = true;
            this.lblNoFilesSelected.ForeColor = System.Drawing.Color.Blue;
            this.lblNoFilesSelected.Location = new System.Drawing.Point(100, 50);
            this.lblNoFilesSelected.Name = "lblNoFilesSelected";
            this.lblNoFilesSelected.Size = new System.Drawing.Size(83, 13);
            this.lblNoFilesSelected.TabIndex = 2;
            this.lblNoFilesSelected.Text = "No files selected";
            // 
            // lblManualOperations
            // 
            this.lblManualOperations.AutoSize = true;
            this.lblManualOperations.Location = new System.Drawing.Point(15, 75);
            this.lblManualOperations.Name = "lblManualOperations";
            this.lblManualOperations.Size = new System.Drawing.Size(162, 13);
            this.lblManualOperations.TabIndex = 3;
            this.lblManualOperations.Text = "Manual file transfer operations:";
            // 
            // btnUploadFiles
            // 
            this.btnUploadFiles.BackColor = System.Drawing.Color.LightBlue;
            this.btnUploadFiles.Location = new System.Drawing.Point(189, 70);
            this.btnUploadFiles.Name = "btnUploadFiles";
            this.btnUploadFiles.Size = new System.Drawing.Size(80, 23);
            this.btnUploadFiles.TabIndex = 4;
            this.btnUploadFiles.Text = "Upload File(s)";
            this.btnUploadFiles.UseVisualStyleBackColor = false;
            this.btnUploadFiles.Click += new System.EventHandler(this.btnDirectUpload_Click);
            // 
            // btnDownloadFile
            // 
            this.btnDownloadFile.BackColor = System.Drawing.Color.LightGreen;
            this.btnDownloadFile.Location = new System.Drawing.Point(275, 70);
            this.btnDownloadFile.Name = "btnDownloadFile";
            this.btnDownloadFile.Size = new System.Drawing.Size(80, 23);
            this.btnDownloadFile.TabIndex = 5;
            this.btnDownloadFile.Text = "Download";
            this.btnDownloadFile.UseVisualStyleBackColor = false;
            this.btnDownloadFile.Click += new System.EventHandler(this.btnDirectDownload_Click);
            // 
            // btnSaveTimerJob
            // 
            this.btnSaveTimerJob.BackColor = System.Drawing.Color.LightBlue;
            this.btnSaveTimerJob.Location = new System.Drawing.Point(317, 220);
            this.btnSaveTimerJob.Name = "btnSaveTimerJob";
            this.btnSaveTimerJob.Size = new System.Drawing.Size(140, 30);
            this.btnSaveTimerJob.TabIndex = 3;
            this.btnSaveTimerJob.Text = "Save Timer Job Configuration";
            this.btnSaveTimerJob.UseVisualStyleBackColor = false;
            this.btnSaveTimerJob.Click += new System.EventHandler(this.btnSaveTimerJob_Click);
            // 
            // btnLoadConfiguration
            // 
            this.btnLoadConfiguration.BackColor = System.Drawing.Color.LightGreen;
            this.btnLoadConfiguration.Location = new System.Drawing.Point(470, 220);
            this.btnLoadConfiguration.Name = "btnLoadConfiguration";
            this.btnLoadConfiguration.Size = new System.Drawing.Size(100, 30);
            this.btnLoadConfiguration.TabIndex = 3;
            this.btnLoadConfiguration.Text = "Load Configuration";
            this.btnLoadConfiguration.UseVisualStyleBackColor = false;
            this.btnLoadConfiguration.Click += new System.EventHandler(this.btnLoadConfiguration_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(580, 220);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(670, 220);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FormSchedule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 320);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnSaveTimerJob);
            this.Controls.Add(this.gbFileManager);
            this.Controls.Add(this.gbTimerSettings);
            this.Controls.Add(this.gbJobDetails);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSchedule";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Upload Timer Settings";
            this.gbJobDetails.ResumeLayout(false);
            this.gbJobDetails.PerformLayout();
            this.gbTimerSettings.ResumeLayout(false);
            this.gbTimerSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimerInterval)).EndInit();
            this.gbFileManager.ResumeLayout(false);
            this.gbFileManager.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbJobDetails;
        private System.Windows.Forms.TextBox txtJobName;
        private System.Windows.Forms.Label lblJobName;
        private System.Windows.Forms.CheckBox chkEnableJob;
        private System.Windows.Forms.GroupBox gbTimerSettings;
        private System.Windows.Forms.CheckBox chkEnableTimer;
        private System.Windows.Forms.Label lblUploadEvery;
        private System.Windows.Forms.NumericUpDown numTimerInterval;
        private System.Windows.Forms.ComboBox cmbTimerUnit;
        private System.Windows.Forms.Button btnStartTimer;
        private System.Windows.Forms.Button btnStopTimer;
        private System.Windows.Forms.Label lblTimerStatus;
        private System.Windows.Forms.Label lblLastUpload;
        private System.Windows.Forms.GroupBox gbFileManager;
        private System.Windows.Forms.Label lblFileSelection;
        private System.Windows.Forms.Button btnBrowseFilesForTimer;
        private System.Windows.Forms.Label lblNoFilesSelected;
        private System.Windows.Forms.Label lblManualOperations;
        private System.Windows.Forms.Button btnUploadFiles;
        private System.Windows.Forms.Button btnDownloadFile;
        private System.Windows.Forms.Button btnSaveTimerJob;
        private System.Windows.Forms.Button btnLoadConfiguration;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
