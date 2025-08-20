using System;

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
        private System.Windows.Forms.GroupBox gbTimerSettings;
        private System.Windows.Forms.CheckBox chkEnableTimer;
        private System.Windows.Forms.Label lblTimerInterval;
        private System.Windows.Forms.NumericUpDown numTimerInterval;
        private System.Windows.Forms.ComboBox cmbTimerUnit;
        private System.Windows.Forms.Button btnStartTimer;
        private System.Windows.Forms.Button btnStopTimer;
        private System.Windows.Forms.Label lblTimerStatus;
        private System.Windows.Forms.Label lblLastUpload;
        private System.Windows.Forms.Button btnBrowseFilesForTimer;
        private System.Windows.Forms.Label lblSelectedFiles;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox gbFileManager;
        private System.Windows.Forms.Button btnUploadFile;
        private System.Windows.Forms.Button btnDownloadFile;
        private System.Windows.Forms.Label lblFileOperations;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose standard components
                if (components != null)
                {
                    components.Dispose();
                }
                
                // Clean up the timer if it exists
                if (_uploadTimer != null)
                {
                    try
                    {
                        _uploadTimer.Stop();
                        _uploadTimer.Elapsed -= OnTimerElapsed;
                        _uploadTimer.Dispose();
                        _uploadTimer = null;
                        _isTimerRunning = false;
                    }
                    catch (Exception ex)
                    {
                        if (ServiceLocator.LogService != null)
                            ServiceLocator.LogService.LogError("Error disposing timer: " + ex.Message);
                    }
                }
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
            this.gbTimerSettings = new System.Windows.Forms.GroupBox();
            this.lblTimerInterval = new System.Windows.Forms.Label();
            this.numTimerInterval = new System.Windows.Forms.NumericUpDown();
            this.cmbTimerUnit = new System.Windows.Forms.ComboBox();
            this.btnStartTimer = new System.Windows.Forms.Button();
            this.btnStopTimer = new System.Windows.Forms.Button();
            this.lblTimerStatus = new System.Windows.Forms.Label();
            this.lblLastUpload = new System.Windows.Forms.Label();
            this.chkEnableTimer = new System.Windows.Forms.CheckBox();
            this.btnBrowseFilesForTimer = new System.Windows.Forms.Button();
            this.lblSelectedFiles = new System.Windows.Forms.Label();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbFileManager = new System.Windows.Forms.GroupBox();
            this.btnUploadFile = new System.Windows.Forms.Button();
            this.btnDownloadFile = new System.Windows.Forms.Button();
            this.lblFileOperations = new System.Windows.Forms.Label();
            this.gbJobDetails.SuspendLayout();
            this.gbPaths.SuspendLayout();
            this.gbTimerSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimerInterval)).BeginInit();
            this.gbFileManager.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbJobDetails
            // 
            this.gbJobDetails.Controls.Add(this.lblJobName);
            this.gbJobDetails.Controls.Add(this.txtJobName);
            this.gbJobDetails.Controls.Add(this.chkEnabled);
            this.gbJobDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbJobDetails.Location = new System.Drawing.Point(16, 15);
            this.gbJobDetails.Margin = new System.Windows.Forms.Padding(4);
            this.gbJobDetails.Name = "gbJobDetails";
            this.gbJobDetails.Padding = new System.Windows.Forms.Padding(4);
            this.gbJobDetails.Size = new System.Drawing.Size(747, 74);
            this.gbJobDetails.TabIndex = 0;
            this.gbJobDetails.TabStop = false;
            this.gbJobDetails.Text = "Job Details";
            // 
            // lblJobName
            // 
            this.lblJobName.AutoSize = true;
            this.lblJobName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblJobName.Location = new System.Drawing.Point(20, 33);
            this.lblJobName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblJobName.Name = "lblJobName";
            this.lblJobName.Size = new System.Drawing.Size(79, 18);
            this.lblJobName.TabIndex = 0;
            this.lblJobName.Text = "Job Name:";
            // 
            // txtJobName
            // 
            this.txtJobName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtJobName.Location = new System.Drawing.Point(111, 30);
            this.txtJobName.Margin = new System.Windows.Forms.Padding(4);
            this.txtJobName.Name = "txtJobName";
            this.txtJobName.Size = new System.Drawing.Size(359, 24);
            this.txtJobName.TabIndex = 1;
            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Checked = true;
            this.chkEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkEnabled.Location = new System.Drawing.Point(595, 33);
            this.chkEnabled.Margin = new System.Windows.Forms.Padding(4);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(102, 22);
            this.chkEnabled.TabIndex = 2;
            this.chkEnabled.Text = "Enable Job";
            this.chkEnabled.UseVisualStyleBackColor = true;
            this.chkEnabled.CheckedChanged += new System.EventHandler(this.chkEnabled_CheckedChanged);
            // 
            // gbPaths
            // 
            this.gbPaths.Controls.Add(this.lblSourcePath);
            this.gbPaths.Controls.Add(this.txtSourcePath);
            this.gbPaths.Controls.Add(this.btnBrowseSource);
            this.gbPaths.Controls.Add(this.lblDestinationPath);
            this.gbPaths.Controls.Add(this.txtDestinationPath);
            this.gbPaths.Controls.Add(this.btnBrowseDestination);
            this.gbPaths.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbPaths.Location = new System.Drawing.Point(16, 97);
            this.gbPaths.Margin = new System.Windows.Forms.Padding(4);
            this.gbPaths.Name = "gbPaths";
            this.gbPaths.Padding = new System.Windows.Forms.Padding(4);
            this.gbPaths.Size = new System.Drawing.Size(747, 120);
            this.gbPaths.TabIndex = 1;
            this.gbPaths.TabStop = false;
            this.gbPaths.Text = "Source and Destination";
            // 
            // lblSourcePath
            // 
            this.lblSourcePath.AutoSize = true;
            this.lblSourcePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblSourcePath.Location = new System.Drawing.Point(20, 39);
            this.lblSourcePath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSourcePath.Name = "lblSourcePath";
            this.lblSourcePath.Size = new System.Drawing.Size(102, 18);
            this.lblSourcePath.TabIndex = 0;
            this.lblSourcePath.Text = "Source Folder:";
            // 
            // txtSourcePath
            // 
            this.txtSourcePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtSourcePath.Location = new System.Drawing.Point(135, 36);
            this.txtSourcePath.Margin = new System.Windows.Forms.Padding(4);
            this.txtSourcePath.Name = "txtSourcePath";
            this.txtSourcePath.Size = new System.Drawing.Size(509, 24);
            this.txtSourcePath.TabIndex = 1;
            // 
            // btnBrowseSource
            // 
            this.btnBrowseSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnBrowseSource.Location = new System.Drawing.Point(653, 34);
            this.btnBrowseSource.Margin = new System.Windows.Forms.Padding(4);
            this.btnBrowseSource.Name = "btnBrowseSource";
            this.btnBrowseSource.Size = new System.Drawing.Size(87, 28);
            this.btnBrowseSource.TabIndex = 2;
            this.btnBrowseSource.Text = "Browse...";
            this.btnBrowseSource.UseVisualStyleBackColor = true;
            this.btnBrowseSource.Click += new System.EventHandler(this.btnBrowseSource_Click);
            // 
            // lblDestinationPath
            // 
            this.lblDestinationPath.AutoSize = true;
            this.lblDestinationPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblDestinationPath.Location = new System.Drawing.Point(20, 74);
            this.lblDestinationPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDestinationPath.Name = "lblDestinationPath";
            this.lblDestinationPath.Size = new System.Drawing.Size(117, 18);
            this.lblDestinationPath.TabIndex = 3;
            this.lblDestinationPath.Text = "Destination Path:";
            // 
            // txtDestinationPath
            // 
            this.txtDestinationPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtDestinationPath.Location = new System.Drawing.Point(135, 71);
            this.txtDestinationPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtDestinationPath.Name = "txtDestinationPath";
            this.txtDestinationPath.Size = new System.Drawing.Size(509, 24);
            this.txtDestinationPath.TabIndex = 4;
            // 
            // btnBrowseDestination
            // 
            this.btnBrowseDestination.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnBrowseDestination.Location = new System.Drawing.Point(653, 69);
            this.btnBrowseDestination.Margin = new System.Windows.Forms.Padding(4);
            this.btnBrowseDestination.Name = "btnBrowseDestination";
            this.btnBrowseDestination.Size = new System.Drawing.Size(87, 28);
            this.btnBrowseDestination.TabIndex = 5;
            this.btnBrowseDestination.Text = "Browse...";
            this.btnBrowseDestination.UseVisualStyleBackColor = true;
            this.btnBrowseDestination.Click += new System.EventHandler(this.btnBrowseDestination_Click);
            // 
            // gbTimerSettings
            // 
            this.gbTimerSettings.Controls.Add(this.lblSelectedFiles);
            this.gbTimerSettings.Controls.Add(this.btnBrowseFilesForTimer);
            this.gbTimerSettings.Controls.Add(this.lblLastUpload);
            this.gbTimerSettings.Controls.Add(this.lblTimerStatus);
            this.gbTimerSettings.Controls.Add(this.btnStopTimer);
            this.gbTimerSettings.Controls.Add(this.btnStartTimer);
            this.gbTimerSettings.Controls.Add(this.cmbTimerUnit);
            this.gbTimerSettings.Controls.Add(this.numTimerInterval);
            this.gbTimerSettings.Controls.Add(this.lblTimerInterval);
            this.gbTimerSettings.Controls.Add(this.chkEnableTimer);
            this.gbTimerSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbTimerSettings.Location = new System.Drawing.Point(16, 225);
            this.gbTimerSettings.Margin = new System.Windows.Forms.Padding(4);
            this.gbTimerSettings.Name = "gbTimerSettings";
            this.gbTimerSettings.Padding = new System.Windows.Forms.Padding(4);
            this.gbTimerSettings.Size = new System.Drawing.Size(747, 230);
            this.gbTimerSettings.TabIndex = 2;
            this.gbTimerSettings.TabStop = false;
            this.gbTimerSettings.Text = "Upload Timer Settings";
            // 
            // lblTimerInterval
            // 
            this.lblTimerInterval.AutoSize = true;
            this.lblTimerInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblTimerInterval.Location = new System.Drawing.Point(20, 68);
            this.lblTimerInterval.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTimerInterval.Name = "lblTimerInterval";
            this.lblTimerInterval.Size = new System.Drawing.Size(97, 18);
            this.lblTimerInterval.TabIndex = 1;
            this.lblTimerInterval.Text = "Upload Every:";
            // 
            // numTimerInterval
            // 
            this.numTimerInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.numTimerInterval.Location = new System.Drawing.Point(135, 66);
            this.numTimerInterval.Margin = new System.Windows.Forms.Padding(4);
            this.numTimerInterval.Maximum = new decimal(new int[] {
            1440,
            0,
            0,
            0});
            this.numTimerInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numTimerInterval.Name = "numTimerInterval";
            this.numTimerInterval.Size = new System.Drawing.Size(80, 24);
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
            this.cmbTimerUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.cmbTimerUnit.FormattingEnabled = true;
            this.cmbTimerUnit.Items.AddRange(new object[] {
            "Seconds",
            "Minutes",
            "Hours"});
            this.cmbTimerUnit.Location = new System.Drawing.Point(224, 66);
            this.cmbTimerUnit.Margin = new System.Windows.Forms.Padding(4);
            this.cmbTimerUnit.Name = "cmbTimerUnit";
            this.cmbTimerUnit.Size = new System.Drawing.Size(120, 26);
            this.cmbTimerUnit.TabIndex = 3;
            // 
            // btnStartTimer
            // 
            this.btnStartTimer.BackColor = System.Drawing.Color.LightGreen;
            this.btnStartTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnStartTimer.Location = new System.Drawing.Point(135, 107);
            this.btnStartTimer.Margin = new System.Windows.Forms.Padding(4);
            this.btnStartTimer.Name = "btnStartTimer";
            this.btnStartTimer.Size = new System.Drawing.Size(120, 32);
            this.btnStartTimer.TabIndex = 4;
            this.btnStartTimer.Text = "Start Timer";
            this.btnStartTimer.UseVisualStyleBackColor = false;
            this.btnStartTimer.Click += new System.EventHandler(this.btnStartTimer_Click);
            // 
            // btnStopTimer
            // 
            this.btnStopTimer.BackColor = System.Drawing.Color.LightCoral;
            this.btnStopTimer.Enabled = false;
            this.btnStopTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnStopTimer.Location = new System.Drawing.Point(263, 107);
            this.btnStopTimer.Margin = new System.Windows.Forms.Padding(4);
            this.btnStopTimer.Name = "btnStopTimer";
            this.btnStopTimer.Size = new System.Drawing.Size(120, 32);
            this.btnStopTimer.TabIndex = 5;
            this.btnStopTimer.Text = "Stop Timer";
            this.btnStopTimer.UseVisualStyleBackColor = false;
            this.btnStopTimer.Click += new System.EventHandler(this.btnStopTimer_Click);
            // 
            // lblTimerStatus
            // 
            this.lblTimerStatus.AutoSize = true;
            this.lblTimerStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblTimerStatus.Location = new System.Drawing.Point(402, 113);
            this.lblTimerStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTimerStatus.Name = "lblTimerStatus";
            this.lblTimerStatus.Size = new System.Drawing.Size(100, 18);
            this.lblTimerStatus.TabIndex = 6;
            this.lblTimerStatus.Text = "Timer stopped";
            // 
            // lblLastUpload
            // 
            this.lblLastUpload.AutoSize = true;
            this.lblLastUpload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblLastUpload.Location = new System.Drawing.Point(402, 145);
            this.lblLastUpload.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLastUpload.Name = "lblLastUpload";
            this.lblLastUpload.Size = new System.Drawing.Size(120, 18);
            this.lblLastUpload.TabIndex = 7;
            this.lblLastUpload.Text = "Last Upload: Never";
            // 
            // chkEnableTimer
            // 
            this.chkEnableTimer.AutoSize = true;
            this.chkEnableTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkEnableTimer.Location = new System.Drawing.Point(23, 33);
            this.chkEnableTimer.Margin = new System.Windows.Forms.Padding(4);
            this.chkEnableTimer.Name = "chkEnableTimer";
            this.chkEnableTimer.Size = new System.Drawing.Size(117, 22);
            this.chkEnableTimer.TabIndex = 0;
            this.chkEnableTimer.Text = "Enable Timer";
            this.chkEnableTimer.UseVisualStyleBackColor = true;
            this.chkEnableTimer.CheckedChanged += new System.EventHandler(this.chkEnableTimer_CheckedChanged);
            // 
            // btnBrowseFilesForTimer
            // 
            this.btnBrowseFilesForTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnBrowseFilesForTimer.Location = new System.Drawing.Point(23, 145);
            this.btnBrowseFilesForTimer.Margin = new System.Windows.Forms.Padding(4);
            this.btnBrowseFilesForTimer.Name = "btnBrowseFilesForTimer";
            this.btnBrowseFilesForTimer.Size = new System.Drawing.Size(120, 32);
            this.btnBrowseFilesForTimer.TabIndex = 8;
            this.btnBrowseFilesForTimer.Text = "Browse Folder...";
            this.btnBrowseFilesForTimer.UseVisualStyleBackColor = true;
            this.btnBrowseFilesForTimer.Click += new System.EventHandler(this.btnBrowseFilesForTimer_Click);
            // 
            // lblSelectedFiles
            // 
            this.lblSelectedFiles.AutoSize = true;
            this.lblSelectedFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblSelectedFiles.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblSelectedFiles.Location = new System.Drawing.Point(151, 152);
            this.lblSelectedFiles.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSelectedFiles.Name = "lblSelectedFiles";
            this.lblSelectedFiles.Size = new System.Drawing.Size(113, 18);
            this.lblSelectedFiles.TabIndex = 9;
            this.lblSelectedFiles.Text = "No files selected";
            // 
            // btnPreview
            // 
            this.btnPreview.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnPreview.Location = new System.Drawing.Point(16, 530);
            this.btnPreview.Margin = new System.Windows.Forms.Padding(4);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(100, 30);
            this.btnPreview.TabIndex = 4;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnSave.Location = new System.Drawing.Point(583, 530);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(87, 30);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnCancel.Location = new System.Drawing.Point(676, 530);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(87, 30);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // gbFileManager
            // 
            this.gbFileManager.Controls.Add(this.lblFileOperations);
            this.gbFileManager.Controls.Add(this.btnDownloadFile);
            this.gbFileManager.Controls.Add(this.btnUploadFile);
            this.gbFileManager.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbFileManager.Location = new System.Drawing.Point(16, 463);
            this.gbFileManager.Margin = new System.Windows.Forms.Padding(4);
            this.gbFileManager.Name = "gbFileManager";
            this.gbFileManager.Padding = new System.Windows.Forms.Padding(4);
            this.gbFileManager.Size = new System.Drawing.Size(747, 70);
            this.gbFileManager.TabIndex = 3;
            this.gbFileManager.TabStop = false;
            this.gbFileManager.Text = "File Manager";
            // 
            // btnUploadFile
            // 
            this.btnUploadFile.BackColor = System.Drawing.Color.LightBlue;
            this.btnUploadFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnUploadFile.Location = new System.Drawing.Point(215, 25);
            this.btnUploadFile.Margin = new System.Windows.Forms.Padding(4);
            this.btnUploadFile.Name = "btnUploadFile";
            this.btnUploadFile.Size = new System.Drawing.Size(127, 32);
            this.btnUploadFile.TabIndex = 1;
            this.btnUploadFile.Text = "Upload File(s)";
            this.btnUploadFile.UseVisualStyleBackColor = false;
            this.btnUploadFile.Click += new System.EventHandler(this.btnUploadFile_Click);
            // 
            // btnDownloadFile
            // 
            this.btnDownloadFile.BackColor = System.Drawing.Color.LightGreen;
            this.btnDownloadFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnDownloadFile.Location = new System.Drawing.Point(351, 25);
            this.btnDownloadFile.Margin = new System.Windows.Forms.Padding(4);
            this.btnDownloadFile.Name = "btnDownloadFile";
            this.btnDownloadFile.Size = new System.Drawing.Size(127, 32);
            this.btnDownloadFile.TabIndex = 2;
            this.btnDownloadFile.Text = "Download File(s)";
            this.btnDownloadFile.UseVisualStyleBackColor = false;
            this.btnDownloadFile.Click += new System.EventHandler(this.btnDownloadFile_Click);
            // 
            // lblFileOperations
            // 
            this.lblFileOperations.AutoSize = true;
            this.lblFileOperations.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblFileOperations.Location = new System.Drawing.Point(20, 32);
            this.lblFileOperations.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileOperations.Name = "lblFileOperations";
            this.lblFileOperations.Size = new System.Drawing.Size(191, 18);
            this.lblFileOperations.TabIndex = 0;
            this.lblFileOperations.Text = "Manual file transfer operations:";
            // 
            // FormSchedule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(779, 573);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.gbFileManager);
            this.Controls.Add(this.gbTimerSettings);
            this.Controls.Add(this.gbPaths);
            this.Controls.Add(this.gbJobDetails);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSchedule";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Upload Timer Settings";
            this.gbJobDetails.ResumeLayout(false);
            this.gbJobDetails.PerformLayout();
            this.gbPaths.ResumeLayout(false);
            this.gbPaths.PerformLayout();
            this.gbTimerSettings.ResumeLayout(false);
            this.gbTimerSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimerInterval)).EndInit();
            this.gbFileManager.ResumeLayout(false);
            this.gbFileManager.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}
