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
            this.chkEnableJob = new System.Windows.Forms.CheckBox();
            this.lblJobName = new System.Windows.Forms.Label();
            this.txtJobName = new System.Windows.Forms.TextBox();
            this.gbTransfer = new System.Windows.Forms.GroupBox();
            this.rbDownload = new System.Windows.Forms.RadioButton();
            this.rbUpload = new System.Windows.Forms.RadioButton();
            this.gbTimerSettings = new System.Windows.Forms.GroupBox();
            this.lblLastUpload = new System.Windows.Forms.Label();
            this.lblTimerStatus = new System.Windows.Forms.Label();
            this.btnStopTimer = new System.Windows.Forms.Button();
            this.btnStartTimer = new System.Windows.Forms.Button();
            this.cmbTimerUnit = new System.Windows.Forms.ComboBox();
            this.numTimerInterval = new System.Windows.Forms.NumericUpDown();
            this.lblUploadEvery = new System.Windows.Forms.Label();
            this.chkEnableTimer = new System.Windows.Forms.CheckBox();
            this.chkIncludeSubfolders = new System.Windows.Forms.CheckBox();
            this.chkIncludeSubfolders = new System.Windows.Forms.CheckBox();
            this.gbFileManager = new System.Windows.Forms.GroupBox();
            this.btnDownloadFile = new System.Windows.Forms.Button();
            this.btnUploadFiles = new System.Windows.Forms.Button();
            this.lblManualOperations = new System.Windows.Forms.Label();
            this.lblNoFilesSelected = new System.Windows.Forms.Label();
            this.btnBrowseFilesForTimer = new System.Windows.Forms.Button();
            this.lblFileSelection = new System.Windows.Forms.Label();
            this.btnSaveTimerJob = new System.Windows.Forms.Button();
            this.btnLoadConfiguration = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbJobDetails.SuspendLayout();
            this.gbTransfer.SuspendLayout();
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
            this.gbJobDetails.Location = new System.Drawing.Point(15, 15);
            this.gbJobDetails.Name = "gbJobDetails";
            this.gbJobDetails.Padding = new System.Windows.Forms.Padding(15, 10, 15, 15);
            this.gbJobDetails.Size = new System.Drawing.Size(760, 80);
            this.gbJobDetails.TabIndex = 0;
            this.gbJobDetails.TabStop = false;
            this.gbJobDetails.Text = "Job Details";
            // 
            // chkEnableJob
            // 
            this.chkEnableJob.AutoSize = true;
            this.chkEnableJob.Checked = true;
            this.chkEnableJob.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableJob.Location = new System.Drawing.Point(670, 35);
            this.chkEnableJob.Name = "chkEnableJob";
            this.chkEnableJob.Size = new System.Drawing.Size(79, 17);
            this.chkEnableJob.TabIndex = 2;
            this.chkEnableJob.Text = "Enable";
            this.chkEnableJob.UseVisualStyleBackColor = true;
            // 
            // lblJobName
            // 
            this.lblJobName.AutoSize = true;
            this.lblJobName.Location = new System.Drawing.Point(20, 35);
            this.lblJobName.Name = "lblJobName";
            this.lblJobName.Size = new System.Drawing.Size(62, 13);
            this.lblJobName.TabIndex = 1;
            this.lblJobName.Text = "Job Name *";
            // 
            // txtJobName
            // 
            this.txtJobName.Location = new System.Drawing.Point(100, 32);
            this.txtJobName.Name = "txtJobName";
            this.txtJobName.Size = new System.Drawing.Size(560, 20);
            this.txtJobName.TabIndex = 0;
            // 
            // gbTransfer
            // 
            this.gbTransfer.Controls.Add(this.rbDownload);
            this.gbTransfer.Controls.Add(this.rbUpload);
            this.gbTransfer.Location = new System.Drawing.Point(15, 105);
            this.gbTransfer.Name = "gbTransfer";
            this.gbTransfer.Padding = new System.Windows.Forms.Padding(15, 10, 15, 15);
            this.gbTransfer.Size = new System.Drawing.Size(760, 60);
            this.gbTransfer.TabIndex = 1;
            this.gbTransfer.TabStop = false;
            this.gbTransfer.Text = "Transfer";
            // 
            // rbDownload
            // 
            this.rbDownload.AutoSize = true;
            this.rbDownload.Location = new System.Drawing.Point(200, 25);
            this.rbDownload.Name = "rbDownload";
            this.rbDownload.Size = new System.Drawing.Size(133, 17);
            this.rbDownload.TabIndex = 1;
            this.rbDownload.Text = "Download (Remote →)";
            this.rbDownload.UseVisualStyleBackColor = true;
            // 
            // rbUpload
            // 
            this.rbUpload.AutoSize = true;
            this.rbUpload.Checked = true;
            this.rbUpload.Location = new System.Drawing.Point(20, 25);
            this.rbUpload.Name = "rbUpload";
            this.rbUpload.Size = new System.Drawing.Size(108, 17);
            this.rbUpload.TabIndex = 0;
            this.rbUpload.TabStop = true;
            this.rbUpload.Text = "Upload (Local →)";
            this.rbUpload.UseVisualStyleBackColor = true;
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
            this.gbTimerSettings.Controls.Add(this.chkIncludeSubfolders);
            this.gbTimerSettings.Location = new System.Drawing.Point(15, 175);
            this.gbTimerSettings.Name = "gbTimerSettings";
            this.gbTimerSettings.Padding = new System.Windows.Forms.Padding(15, 10, 15, 15);
            this.gbTimerSettings.Size = new System.Drawing.Size(360, 200);
            this.gbTimerSettings.TabIndex = 2;
            this.gbTimerSettings.TabStop = false;
            this.gbTimerSettings.Text = "Timer Settings";
            // 
            // lblLastUpload
            // 
            this.lblLastUpload.AutoSize = true;
            this.lblLastUpload.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLastUpload.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblLastUpload.Location = new System.Drawing.Point(210, 145);
            this.lblLastUpload.Name = "lblLastUpload";
            this.lblLastUpload.Size = new System.Drawing.Size(36, 13);
            this.lblLastUpload.TabIndex = 7;
            this.lblLastUpload.Text = "Never";
            // 
            // lblTimerStatus
            // 
            this.lblTimerStatus.AutoSize = true;
            this.lblTimerStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTimerStatus.ForeColor = System.Drawing.Color.DarkRed;
            this.lblTimerStatus.Location = new System.Drawing.Point(210, 125);
            this.lblTimerStatus.Name = "lblTimerStatus";
            this.lblTimerStatus.Size = new System.Drawing.Size(74, 13);
            this.lblTimerStatus.TabIndex = 6;
            this.lblTimerStatus.Text = "Timer stopped";
            // 
            // btnStopTimer
            // 
            this.btnStopTimer.Enabled = false;
            this.btnStopTimer.Location = new System.Drawing.Point(110, 150);
            this.btnStopTimer.Name = "btnStopTimer";
            this.btnStopTimer.Size = new System.Drawing.Size(80, 30);
            this.btnStopTimer.TabIndex = 5;
            this.btnStopTimer.Text = "Stop";
            this.btnStopTimer.UseVisualStyleBackColor = true;
            this.btnStopTimer.Click += new System.EventHandler(this.btnStopTimer_Click);
            // 
            // btnStartTimer
            // 
            this.btnStartTimer.Enabled = false;
            this.btnStartTimer.Location = new System.Drawing.Point(20, 150);
            this.btnStartTimer.Name = "btnStartTimer";
            this.btnStartTimer.Size = new System.Drawing.Size(80, 30);
            this.btnStartTimer.TabIndex = 4;
            this.btnStartTimer.Text = "Start";
            this.btnStartTimer.UseVisualStyleBackColor = true;
            this.btnStartTimer.Click += new System.EventHandler(this.btnStartTimer_Click);
            // 
            // cmbTimerUnit
            // 
            this.cmbTimerUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTimerUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbTimerUnit.FormattingEnabled = true;
            this.cmbTimerUnit.Items.AddRange(new object[] {
            "Seconds",
            "Minutes",
            "Hours"});
            this.cmbTimerUnit.Location = new System.Drawing.Point(165, 65);
            this.cmbTimerUnit.Name = "cmbTimerUnit";
            this.cmbTimerUnit.Size = new System.Drawing.Size(80, 21);
            this.cmbTimerUnit.TabIndex = 3;
            // 
            // numTimerInterval
            // 
            this.numTimerInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numTimerInterval.Location = new System.Drawing.Point(95, 65);
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
            // lblUploadEvery
            // 
            this.lblUploadEvery.AutoSize = true;
            this.lblUploadEvery.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUploadEvery.Location = new System.Drawing.Point(55, 67);
            this.lblUploadEvery.Name = "lblUploadEvery";
            this.lblUploadEvery.Size = new System.Drawing.Size(34, 13);
            this.lblUploadEvery.TabIndex = 1;
            this.lblUploadEvery.Text = "Every";
            // 
            // chkEnableTimer
            // 
            this.chkEnableTimer.AutoSize = true;
            this.chkEnableTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkEnableTimer.Location = new System.Drawing.Point(20, 30);
            this.chkEnableTimer.Name = "chkEnableTimer";
            this.chkEnableTimer.Size = new System.Drawing.Size(88, 17);
            this.chkEnableTimer.TabIndex = 0;
            this.chkEnableTimer.Text = "Enable Timer";
            this.chkEnableTimer.UseVisualStyleBackColor = true;
            this.chkEnableTimer.CheckedChanged += new System.EventHandler(this.chkEnableTimer_CheckedChanged);
            // 
            // chkIncludeSubfolders
            // 
            this.chkIncludeSubfolders.AutoSize = true;
            this.chkIncludeSubfolders.Checked = true;
            this.chkIncludeSubfolders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIncludeSubfolders.Location = new System.Drawing.Point(20, 105);
            this.chkIncludeSubfolders.Name = "chkIncludeSubfolders";
            this.chkIncludeSubfolders.Size = new System.Drawing.Size(117, 17);
            this.chkIncludeSubfolders.TabIndex = 1;
            this.chkIncludeSubfolders.Text = "Include Subfolders";
            this.chkIncludeSubfolders.UseVisualStyleBackColor = true;
            // 
            // gbFileManager
            // 
            this.gbFileManager.Controls.Add(this.btnDownloadFile);
            this.gbFileManager.Controls.Add(this.btnUploadFiles);
            this.gbFileManager.Controls.Add(this.lblManualOperations);
            this.gbFileManager.Controls.Add(this.lblNoFilesSelected);
            this.gbFileManager.Controls.Add(this.btnBrowseFilesForTimer);
            this.gbFileManager.Controls.Add(this.lblFileSelection);
            this.gbFileManager.Location = new System.Drawing.Point(385, 175);
            this.gbFileManager.Name = "gbFileManager";
            this.gbFileManager.Padding = new System.Windows.Forms.Padding(15, 10, 15, 15);
            this.gbFileManager.Size = new System.Drawing.Size(390, 200);
            this.gbFileManager.TabIndex = 3;
            this.gbFileManager.TabStop = false;
            this.gbFileManager.Text = "File Manager";
            // 
            // btnDownloadFile
            // 
            this.btnDownloadFile.Location = new System.Drawing.Point(140, 145);
            this.btnDownloadFile.Name = "btnDownloadFile";
            this.btnDownloadFile.Size = new System.Drawing.Size(100, 30);
            this.btnDownloadFile.TabIndex = 5;
            this.btnDownloadFile.Text = "Download";
            this.btnDownloadFile.UseVisualStyleBackColor = true;
            this.btnDownloadFile.Click += new System.EventHandler(this.btnDirectDownload_Click);
            // 
            // btnUploadFiles
            // 
            this.btnUploadFiles.Location = new System.Drawing.Point(20, 145);
            this.btnUploadFiles.Name = "btnUploadFiles";
            this.btnUploadFiles.Size = new System.Drawing.Size(100, 30);
            this.btnUploadFiles.TabIndex = 4;
            this.btnUploadFiles.Text = "Upload";
            this.btnUploadFiles.UseVisualStyleBackColor = true;
            this.btnUploadFiles.Click += new System.EventHandler(this.btnDirectUpload_Click);
            // 
            // lblManualOperations
            // 
            this.lblManualOperations.AutoSize = true;
            this.lblManualOperations.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblManualOperations.Location = new System.Drawing.Point(20, 125);
            this.lblManualOperations.Name = "lblManualOperations";
            this.lblManualOperations.Size = new System.Drawing.Size(148, 13);
            this.lblManualOperations.TabIndex = 3;
            this.lblManualOperations.Text = "Manual file transfer operations";
            // 
            // lblNoFilesSelected
            // 
            this.lblNoFilesSelected.AutoSize = true;
            this.lblNoFilesSelected.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoFilesSelected.ForeColor = System.Drawing.Color.Blue;
            this.lblNoFilesSelected.Location = new System.Drawing.Point(140, 85);
            this.lblNoFilesSelected.Name = "lblNoFilesSelected";
            this.lblNoFilesSelected.Size = new System.Drawing.Size(85, 13);
            this.lblNoFilesSelected.TabIndex = 2;
            this.lblNoFilesSelected.Text = "No files selected";
            // 
            // btnBrowseFilesForTimer
            // 
            this.btnBrowseFilesForTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBrowseFilesForTimer.Location = new System.Drawing.Point(20, 80);
            this.btnBrowseFilesForTimer.Name = "btnBrowseFilesForTimer";
            this.btnBrowseFilesForTimer.Size = new System.Drawing.Size(110, 25);
            this.btnBrowseFilesForTimer.TabIndex = 1;
            this.btnBrowseFilesForTimer.Text = "Browse Folder";
            this.btnBrowseFilesForTimer.UseVisualStyleBackColor = true;
            this.btnBrowseFilesForTimer.Click += new System.EventHandler(this.btnBrowseFilesForTimer_Click);
            // 
            // lblFileSelection
            // 
            this.lblFileSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileSelection.Location = new System.Drawing.Point(20, 25);
            this.lblFileSelection.Name = "lblFileSelection";
            this.lblFileSelection.Size = new System.Drawing.Size(350, 40);
            this.lblFileSelection.TabIndex = 0;
            this.lblFileSelection.Text = "File Selection: All files in the selected folder will be uploaded automatically.";
            // 
            // btnSaveTimerJob
            // 
            this.btnSaveTimerJob.Location = new System.Drawing.Point(15, 395);
            this.btnSaveTimerJob.Name = "btnSaveTimerJob";
            this.btnSaveTimerJob.Size = new System.Drawing.Size(160, 40);
            this.btnSaveTimerJob.TabIndex = 4;
            this.btnSaveTimerJob.Text = "Save Timer Job";
            this.btnSaveTimerJob.UseVisualStyleBackColor = true;
            this.btnSaveTimerJob.Click += new System.EventHandler(this.btnSaveTimerJob_Click);
            // 
            // btnLoadConfiguration
            // 
            this.btnLoadConfiguration.Location = new System.Drawing.Point(185, 395);
            this.btnLoadConfiguration.Name = "btnLoadConfiguration";
            this.btnLoadConfiguration.Size = new System.Drawing.Size(140, 40);
            this.btnLoadConfiguration.TabIndex = 5;
            this.btnLoadConfiguration.Text = "Load Config";
            this.btnLoadConfiguration.UseVisualStyleBackColor = true;
            this.btnLoadConfiguration.Click += new System.EventHandler(this.btnLoadConfiguration_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(595, 395);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 40);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(695, 395);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 40);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FormSchedule
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 470);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnLoadConfiguration);
            this.Controls.Add(this.btnSaveTimerJob);
            this.Controls.Add(this.gbFileManager);
            this.Controls.Add(this.gbTimerSettings);
            this.Controls.Add(this.gbTransfer);
            this.Controls.Add(this.gbJobDetails);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSchedule";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Timer Settings";
            this.gbJobDetails.ResumeLayout(false);
            this.gbJobDetails.PerformLayout();
            this.gbTransfer.ResumeLayout(false);
            this.gbTransfer.PerformLayout();
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
        private System.Windows.Forms.GroupBox gbTransfer;
        private System.Windows.Forms.RadioButton rbUpload;
        private System.Windows.Forms.RadioButton rbDownload;
        private System.Windows.Forms.GroupBox gbTimerSettings;
        private System.Windows.Forms.CheckBox chkEnableTimer;
        private System.Windows.Forms.Label lblUploadEvery;
        private System.Windows.Forms.NumericUpDown numTimerInterval;
        private System.Windows.Forms.ComboBox cmbTimerUnit;
        private System.Windows.Forms.Button btnStartTimer;
        private System.Windows.Forms.Button btnStopTimer;
        private System.Windows.Forms.Label lblTimerStatus;
        private System.Windows.Forms.Label lblLastUpload;
        private System.Windows.Forms.CheckBox chkIncludeSubfolders;
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