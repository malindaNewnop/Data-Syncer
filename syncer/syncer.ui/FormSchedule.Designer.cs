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
            this.gbFileFilters = new System.Windows.Forms.GroupBox();
            this.chkEnableFileFilter = new System.Windows.Forms.CheckBox();
            this.lblIncludeExtensions = new System.Windows.Forms.Label();
            this.txtIncludeExtensions = new System.Windows.Forms.TextBox();
            this.lblExcludeFiles = new System.Windows.Forms.Label();
            this.txtExcludeFiles = new System.Windows.Forms.TextBox();
            this.lblFilterHelp = new System.Windows.Forms.Label();
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
            this.gbFileFilters.SuspendLayout();
            this.gbFileManager.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbJobDetails
            // 
            this.gbJobDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbJobDetails.Controls.Add(this.chkEnableJob);
            this.gbJobDetails.Controls.Add(this.lblJobName);
            this.gbJobDetails.Controls.Add(this.txtJobName);
            this.gbJobDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbJobDetails.Location = new System.Drawing.Point(15, 15);
            this.gbJobDetails.Name = "gbJobDetails";
            this.gbJobDetails.Padding = new System.Windows.Forms.Padding(15, 10, 15, 15);
            this.gbJobDetails.Size = new System.Drawing.Size(870, 70);
            this.gbJobDetails.TabIndex = 0;
            this.gbJobDetails.TabStop = false;
            this.gbJobDetails.Text = "Job Details";
            // 
            // chkEnableJob
            // 
            this.chkEnableJob.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkEnableJob.AutoSize = true;
            this.chkEnableJob.Checked = true;
            this.chkEnableJob.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkEnableJob.Location = new System.Drawing.Point(770, 35);
            this.chkEnableJob.Name = "chkEnableJob";
            this.chkEnableJob.Size = new System.Drawing.Size(79, 19);
            this.chkEnableJob.TabIndex = 2;
            this.chkEnableJob.Text = "Enable";
            this.chkEnableJob.UseVisualStyleBackColor = true;
            // 
            // lblJobName
            // 
            this.lblJobName.AutoSize = true;
            this.lblJobName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblJobName.Location = new System.Drawing.Point(20, 35);
            this.lblJobName.Name = "lblJobName";
            this.lblJobName.Size = new System.Drawing.Size(70, 15);
            this.lblJobName.TabIndex = 1;
            this.lblJobName.Text = "Job Name *";
            // 
            // txtJobName
            // 
            this.txtJobName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtJobName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtJobName.Location = new System.Drawing.Point(100, 32);
            this.txtJobName.Name = "txtJobName";
            this.txtJobName.Size = new System.Drawing.Size(650, 21);
            this.txtJobName.TabIndex = 0;
            // 
            // gbTransfer
            // 
            this.gbTransfer.Controls.Add(this.rbDownload);
            this.gbTransfer.Controls.Add(this.rbUpload);
            this.gbTransfer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbTransfer.Location = new System.Drawing.Point(15, 95);
            this.gbTransfer.Name = "gbTransfer";
            this.gbTransfer.Padding = new System.Windows.Forms.Padding(15, 10, 15, 15);
            this.gbTransfer.Size = new System.Drawing.Size(400, 65);
            this.gbTransfer.TabIndex = 1;
            this.gbTransfer.TabStop = false;
            this.gbTransfer.Text = "Transfer Mode";
            // 
            // rbDownload
            // 
            this.rbDownload.AutoSize = true;
            this.rbDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.rbDownload.Location = new System.Drawing.Point(220, 30);
            this.rbDownload.Name = "rbDownload";
            this.rbDownload.Size = new System.Drawing.Size(144, 19);
            this.rbDownload.TabIndex = 1;
            this.rbDownload.Text = "Download (Remote →)";
            this.rbDownload.UseVisualStyleBackColor = true;
            this.rbDownload.CheckedChanged += new System.EventHandler(this.RbDownload_CheckedChanged);
            // 
            // rbUpload
            // 
            this.rbUpload.AutoSize = true;
            this.rbUpload.Checked = true;
            this.rbUpload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.rbUpload.Location = new System.Drawing.Point(20, 30);
            this.rbUpload.Name = "rbUpload";
            this.rbUpload.Size = new System.Drawing.Size(118, 19);
            this.rbUpload.TabIndex = 0;
            this.rbUpload.TabStop = true;
            this.rbUpload.Text = "Upload (Local →)";
            this.rbUpload.UseVisualStyleBackColor = true;
            this.rbUpload.CheckedChanged += new System.EventHandler(this.RbUpload_CheckedChanged);
            // 
            // gbTimerSettings
            // 
            this.gbTimerSettings.Controls.Add(this.chkIncludeSubfolders);
            this.gbTimerSettings.Controls.Add(this.lblLastUpload);
            this.gbTimerSettings.Controls.Add(this.lblTimerStatus);
            this.gbTimerSettings.Controls.Add(this.btnStopTimer);
            this.gbTimerSettings.Controls.Add(this.btnStartTimer);
            this.gbTimerSettings.Controls.Add(this.cmbTimerUnit);
            this.gbTimerSettings.Controls.Add(this.numTimerInterval);
            this.gbTimerSettings.Controls.Add(this.lblUploadEvery);
            this.gbTimerSettings.Controls.Add(this.chkEnableTimer);
            this.gbTimerSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbTimerSettings.Location = new System.Drawing.Point(15, 170);
            this.gbTimerSettings.Name = "gbTimerSettings";
            this.gbTimerSettings.Padding = new System.Windows.Forms.Padding(15, 10, 15, 15);
            this.gbTimerSettings.Size = new System.Drawing.Size(400, 150);
            this.gbTimerSettings.TabIndex = 2;
            this.gbTimerSettings.TabStop = false;
            this.gbTimerSettings.Text = "Timer Settings";
            // 
            // chkIncludeSubfolders
            // 
            this.chkIncludeSubfolders.AutoSize = true;
            this.chkIncludeSubfolders.Checked = true;
            this.chkIncludeSubfolders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIncludeSubfolders.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkIncludeSubfolders.Location = new System.Drawing.Point(20, 115);
            this.chkIncludeSubfolders.Name = "chkIncludeSubfolders";
            this.chkIncludeSubfolders.Size = new System.Drawing.Size(134, 19);
            this.chkIncludeSubfolders.TabIndex = 8;
            this.chkIncludeSubfolders.Text = "Include Subfolders";
            this.chkIncludeSubfolders.UseVisualStyleBackColor = true;
            // 
            // lblLastUpload
            // 
            this.lblLastUpload.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Italic);
            this.lblLastUpload.ForeColor = System.Drawing.Color.Gray;
            this.lblLastUpload.Location = new System.Drawing.Point(200, 90);
            this.lblLastUpload.Name = "lblLastUpload";
            this.lblLastUpload.Size = new System.Drawing.Size(180, 20);
            this.lblLastUpload.TabIndex = 7;
            this.lblLastUpload.Text = "Last upload: Never";
            // 
            // lblTimerStatus
            // 
            this.lblTimerStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.lblTimerStatus.ForeColor = System.Drawing.Color.Red;
            this.lblTimerStatus.Location = new System.Drawing.Point(20, 90);
            this.lblTimerStatus.Name = "lblTimerStatus";
            this.lblTimerStatus.Size = new System.Drawing.Size(170, 20);
            this.lblTimerStatus.TabIndex = 6;
            this.lblTimerStatus.Text = "Timer: Stopped";
            // 
            // btnStopTimer
            // 
            this.btnStopTimer.Enabled = false;
            this.btnStopTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnStopTimer.Location = new System.Drawing.Point(320, 55);
            this.btnStopTimer.Name = "btnStopTimer";
            this.btnStopTimer.Size = new System.Drawing.Size(60, 28);
            this.btnStopTimer.TabIndex = 5;
            this.btnStopTimer.Text = "Stop";
            this.btnStopTimer.UseVisualStyleBackColor = true;
            this.btnStopTimer.Click += new System.EventHandler(this.btnStopTimer_Click);
            // 
            // btnStartTimer
            // 
            this.btnStartTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnStartTimer.Location = new System.Drawing.Point(250, 55);
            this.btnStartTimer.Name = "btnStartTimer";
            this.btnStartTimer.Size = new System.Drawing.Size(60, 28);
            this.btnStartTimer.TabIndex = 4;
            this.btnStartTimer.Text = "Start";
            this.btnStartTimer.UseVisualStyleBackColor = true;
            this.btnStartTimer.Click += new System.EventHandler(this.btnStartTimer_Click);
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
            this.cmbTimerUnit.Location = new System.Drawing.Point(160, 57);
            this.cmbTimerUnit.Name = "cmbTimerUnit";
            this.cmbTimerUnit.Size = new System.Drawing.Size(80, 23);
            this.cmbTimerUnit.TabIndex = 3;
            // 
            // numTimerInterval
            // 
            this.numTimerInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.numTimerInterval.Location = new System.Drawing.Point(90, 57);
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
            this.numTimerInterval.Size = new System.Drawing.Size(65, 21);
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
            this.lblUploadEvery.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblUploadEvery.Location = new System.Drawing.Point(40, 60);
            this.lblUploadEvery.Name = "lblUploadEvery";
            this.lblUploadEvery.Size = new System.Drawing.Size(40, 15);
            this.lblUploadEvery.TabIndex = 1;
            this.lblUploadEvery.Text = "Every";
            // 
            // chkEnableTimer
            // 
            this.chkEnableTimer.AutoSize = true;
            this.chkEnableTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkEnableTimer.Location = new System.Drawing.Point(20, 30);
            this.chkEnableTimer.Name = "chkEnableTimer";
            this.chkEnableTimer.Size = new System.Drawing.Size(103, 19);
            this.chkEnableTimer.TabIndex = 0;
            this.chkEnableTimer.Text = "Enable Timer";
            this.chkEnableTimer.UseVisualStyleBackColor = true;
            this.chkEnableTimer.CheckedChanged += new System.EventHandler(this.chkEnableTimer_CheckedChanged);
            // 
            // gbFileFilters
            // 
            this.gbFileFilters.Controls.Add(this.lblFilterHelp);
            this.gbFileFilters.Controls.Add(this.txtExcludeFiles);
            this.gbFileFilters.Controls.Add(this.lblExcludeFiles);
            this.gbFileFilters.Controls.Add(this.txtIncludeExtensions);
            this.gbFileFilters.Controls.Add(this.lblIncludeExtensions);
            this.gbFileFilters.Controls.Add(this.chkEnableFileFilter);
            this.gbFileFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbFileFilters.Location = new System.Drawing.Point(15, 330);
            this.gbFileFilters.Name = "gbFileFilters";
            this.gbFileFilters.Padding = new System.Windows.Forms.Padding(15, 10, 15, 15);
            this.gbFileFilters.Size = new System.Drawing.Size(400, 120);
            this.gbFileFilters.TabIndex = 3;
            this.gbFileFilters.TabStop = false;
            this.gbFileFilters.Text = "File Filters";
            // 
            // chkEnableFileFilter
            // 
            this.chkEnableFileFilter.AutoSize = true;
            this.chkEnableFileFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkEnableFileFilter.Location = new System.Drawing.Point(20, 30);
            this.chkEnableFileFilter.Name = "chkEnableFileFilter";
            this.chkEnableFileFilter.Size = new System.Drawing.Size(104, 19);
            this.chkEnableFileFilter.TabIndex = 0;
            this.chkEnableFileFilter.Text = "Enable Filters";
            this.chkEnableFileFilter.UseVisualStyleBackColor = true;
            this.chkEnableFileFilter.CheckedChanged += new System.EventHandler(this.chkEnableFileFilter_CheckedChanged);
            // 
            // lblIncludeExtensions
            // 
            this.lblIncludeExtensions.AutoSize = true;
            this.lblIncludeExtensions.Enabled = false;
            this.lblIncludeExtensions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblIncludeExtensions.Location = new System.Drawing.Point(20, 60);
            this.lblIncludeExtensions.Name = "lblIncludeExtensions";
            this.lblIncludeExtensions.Size = new System.Drawing.Size(73, 13);
            this.lblIncludeExtensions.TabIndex = 1;
            this.lblIncludeExtensions.Text = "Include Types:";
            // 
            // txtIncludeExtensions
            // 
            this.txtIncludeExtensions.Enabled = false;
            this.txtIncludeExtensions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.txtIncludeExtensions.Location = new System.Drawing.Point(100, 57);
            this.txtIncludeExtensions.Name = "txtIncludeExtensions";
            this.txtIncludeExtensions.Size = new System.Drawing.Size(150, 20);
            this.txtIncludeExtensions.TabIndex = 2;
            this.txtIncludeExtensions.Text = "*.txt,*.doc,*.pdf";
            // 
            // lblExcludeFiles
            // 
            this.lblExcludeFiles.AutoSize = true;
            this.lblExcludeFiles.Enabled = false;
            this.lblExcludeFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblExcludeFiles.Location = new System.Drawing.Point(20, 87);
            this.lblExcludeFiles.Name = "lblExcludeFiles";
            this.lblExcludeFiles.Size = new System.Drawing.Size(70, 13);
            this.lblExcludeFiles.TabIndex = 3;
            this.lblExcludeFiles.Text = "Exclude Files:";
            // 
            // txtExcludeFiles
            // 
            this.txtExcludeFiles.Enabled = false;
            this.txtExcludeFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.txtExcludeFiles.Location = new System.Drawing.Point(100, 84);
            this.txtExcludeFiles.Name = "txtExcludeFiles";
            this.txtExcludeFiles.Size = new System.Drawing.Size(150, 20);
            this.txtExcludeFiles.TabIndex = 4;
            this.txtExcludeFiles.Text = "*.tmp,*.log";
            // 
            // lblFilterHelp
            // 
            this.lblFilterHelp.Enabled = false;
            this.lblFilterHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Italic);
            this.lblFilterHelp.ForeColor = System.Drawing.Color.Gray;
            this.lblFilterHelp.Location = new System.Drawing.Point(260, 55);
            this.lblFilterHelp.Name = "lblFilterHelp";
            this.lblFilterHelp.Size = new System.Drawing.Size(125, 50);
            this.lblFilterHelp.TabIndex = 5;
            this.lblFilterHelp.Text = "Use commas to separate multiple types/patterns";
            // 
            // gbFileManager
            // 
            this.gbFileManager.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbFileManager.Controls.Add(this.btnDownloadFile);
            this.gbFileManager.Controls.Add(this.btnUploadFiles);
            this.gbFileManager.Controls.Add(this.lblManualOperations);
            this.gbFileManager.Controls.Add(this.lblNoFilesSelected);
            this.gbFileManager.Controls.Add(this.btnBrowseFilesForTimer);
            this.gbFileManager.Controls.Add(this.lblFileSelection);
            this.gbFileManager.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.gbFileManager.Location = new System.Drawing.Point(445, 95);
            this.gbFileManager.Name = "gbFileManager";
            this.gbFileManager.Padding = new System.Windows.Forms.Padding(15, 10, 15, 15);
            this.gbFileManager.Size = new System.Drawing.Size(440, 355);
            this.gbFileManager.TabIndex = 4;
            this.gbFileManager.TabStop = false;
            this.gbFileManager.Text = "File Manager (Local → Remote)";
            // 
            // btnDownloadFile
            // 
            this.btnDownloadFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDownloadFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnDownloadFile.Location = new System.Drawing.Point(150, 300);
            this.btnDownloadFile.Name = "btnDownloadFile";
            this.btnDownloadFile.Size = new System.Drawing.Size(120, 40);
            this.btnDownloadFile.TabIndex = 5;
            this.btnDownloadFile.Text = "Download";
            this.btnDownloadFile.UseVisualStyleBackColor = true;
            this.btnDownloadFile.Click += new System.EventHandler(this.btnDirectDownload_Click);
            // 
            // btnUploadFiles
            // 
            this.btnUploadFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUploadFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnUploadFiles.Location = new System.Drawing.Point(20, 300);
            this.btnUploadFiles.Name = "btnUploadFiles";
            this.btnUploadFiles.Size = new System.Drawing.Size(120, 40);
            this.btnUploadFiles.TabIndex = 4;
            this.btnUploadFiles.Text = "Upload";
            this.btnUploadFiles.UseVisualStyleBackColor = true;
            this.btnUploadFiles.Click += new System.EventHandler(this.btnDirectUpload_Click);
            // 
            // lblManualOperations
            // 
            this.lblManualOperations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblManualOperations.AutoSize = true;
            this.lblManualOperations.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblManualOperations.Location = new System.Drawing.Point(20, 275);
            this.lblManualOperations.Name = "lblManualOperations";
            this.lblManualOperations.Size = new System.Drawing.Size(148, 13);
            this.lblManualOperations.TabIndex = 3;
            this.lblManualOperations.Text = "Manual file transfer operations";
            // 
            // lblNoFilesSelected
            // 
            this.lblNoFilesSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNoFilesSelected.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Italic);
            this.lblNoFilesSelected.ForeColor = System.Drawing.Color.Blue;
            this.lblNoFilesSelected.Location = new System.Drawing.Point(20, 140);
            this.lblNoFilesSelected.Name = "lblNoFilesSelected";
            this.lblNoFilesSelected.Size = new System.Drawing.Size(400, 120);
            this.lblNoFilesSelected.TabIndex = 2;
            this.lblNoFilesSelected.Text = "No files selected for timer upload.\r\n\r\nClick \"Browse Folder\" to select a folder" +
    " for automatic uploads.";
            // 
            // btnBrowseFilesForTimer
            // 
            this.btnBrowseFilesForTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnBrowseFilesForTimer.Location = new System.Drawing.Point(20, 90);
            this.btnBrowseFilesForTimer.Name = "btnBrowseFilesForTimer";
            this.btnBrowseFilesForTimer.Size = new System.Drawing.Size(130, 35);
            this.btnBrowseFilesForTimer.TabIndex = 1;
            this.btnBrowseFilesForTimer.Text = "Browse Folder";
            this.btnBrowseFilesForTimer.UseVisualStyleBackColor = true;
            this.btnBrowseFilesForTimer.Click += new System.EventHandler(this.btnBrowseFilesForTimer_Click);
            // 
            // lblFileSelection
            // 
            this.lblFileSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblFileSelection.Location = new System.Drawing.Point(20, 30);
            this.lblFileSelection.Name = "lblFileSelection";
            this.lblFileSelection.Size = new System.Drawing.Size(370, 50);
            this.lblFileSelection.TabIndex = 0;
            this.lblFileSelection.Text = "File Selection: Select a folder and configure filters to automatically upload on" +
    "ly specific file types at regular intervals.";
            // 
            // btnSaveTimerJob
            // 
            this.btnSaveTimerJob.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSaveTimerJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnSaveTimerJob.Location = new System.Drawing.Point(15, 500);
            this.btnSaveTimerJob.Name = "btnSaveTimerJob";
            this.btnSaveTimerJob.Size = new System.Drawing.Size(160, 40);
            this.btnSaveTimerJob.TabIndex = 5;
            this.btnSaveTimerJob.Text = "Save Timer Job";
            this.btnSaveTimerJob.UseVisualStyleBackColor = true;
            this.btnSaveTimerJob.Click += new System.EventHandler(this.btnSaveTimerJob_Click);
            // 
            // btnLoadConfiguration
            // 
            this.btnLoadConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLoadConfiguration.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnLoadConfiguration.Location = new System.Drawing.Point(185, 500);
            this.btnLoadConfiguration.Name = "btnLoadConfiguration";
            this.btnLoadConfiguration.Size = new System.Drawing.Size(140, 40);
            this.btnLoadConfiguration.TabIndex = 6;
            this.btnLoadConfiguration.Text = "Load Config";
            this.btnLoadConfiguration.UseVisualStyleBackColor = true;
            this.btnLoadConfiguration.Click += new System.EventHandler(this.btnLoadConfiguration_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnSave.Location = new System.Drawing.Point(685, 500);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 40);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnCancel.Location = new System.Drawing.Point(785, 500);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 40);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FormSchedule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 580);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnLoadConfiguration);
            this.Controls.Add(this.btnSaveTimerJob);
            this.Controls.Add(this.gbFileManager);
            this.Controls.Add(this.gbFileFilters);
            this.Controls.Add(this.gbTimerSettings);
            this.Controls.Add(this.gbTransfer);
            this.Controls.Add(this.gbJobDetails);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.MinimumSize = new System.Drawing.Size(900, 580);
            this.Name = "FormSchedule";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Timer Settings - File Sync Scheduler";
            this.WindowState = System.Windows.Forms.FormWindowState.Normal;
            this.gbJobDetails.ResumeLayout(false);
            this.gbJobDetails.PerformLayout();
            this.gbTransfer.ResumeLayout(false);
            this.gbTransfer.PerformLayout();
            this.gbTimerSettings.ResumeLayout(false);
            this.gbTimerSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimerInterval)).EndInit();
            this.gbFileFilters.ResumeLayout(false);
            this.gbFileFilters.PerformLayout();
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
        private System.Windows.Forms.GroupBox gbFileFilters;
        private System.Windows.Forms.CheckBox chkEnableFileFilter;
        private System.Windows.Forms.Label lblIncludeExtensions;
        private System.Windows.Forms.TextBox txtIncludeExtensions;
        private System.Windows.Forms.Label lblExcludeFiles;
        private System.Windows.Forms.TextBox txtExcludeFiles;
        private System.Windows.Forms.Label lblFilterHelp;
    }
}