namespace syncer.ui.Forms
{
    partial class FormEditConfiguration
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtConfigName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabJob = new System.Windows.Forms.TabPage();
            this.chkDeleteSourceAfterTransfer = new System.Windows.Forms.CheckBox();
            this.chkIncludeSubfolders = new System.Windows.Forms.CheckBox();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.cmbIntervalType = new System.Windows.Forms.ComboBox();
            this.numInterval = new System.Windows.Forms.NumericUpDown();
            this.btnBrowseDestination = new System.Windows.Forms.Button();
            this.btnBrowseSource = new System.Windows.Forms.Button();
            this.txtDestinationPath = new System.Windows.Forms.TextBox();
            this.txtSourcePath = new System.Windows.Forms.TextBox();
            this.txtJobName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tabFilters = new System.Windows.Forms.TabPage();
            this.lblFilterHint = new System.Windows.Forms.Label();
            this.txtExcludeFileTypes = new System.Windows.Forms.TextBox();
            this.lblExcludeTypes = new System.Windows.Forms.Label();
            this.txtIncludeFileTypes = new System.Windows.Forms.TextBox();
            this.lblIncludeTypes = new System.Windows.Forms.Label();
            this.chkEnableFilters = new System.Windows.Forms.CheckBox();
            this.tabConnection = new System.Windows.Forms.TabPage();
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.chkUseSSL = new System.Windows.Forms.CheckBox();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.cmbConnectionType = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabJob.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).BeginInit();
            this.tabConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabJob);
            this.tabControl1.Controls.Add(this.tabFilters);
            this.tabControl1.Controls.Add(this.tabConnection);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(560, 420);
            this.tabControl1.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.txtDescription);
            this.tabGeneral.Controls.Add(this.txtConfigName);
            this.tabGeneral.Controls.Add(this.label2);
            this.tabGeneral.Controls.Add(this.label1);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(552, 394);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(20, 100);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(500, 80);
            this.txtDescription.TabIndex = 3;
            // 
            // txtConfigName
            // 
            this.txtConfigName.Location = new System.Drawing.Point(20, 40);
            this.txtConfigName.Name = "txtConfigName";
            this.txtConfigName.Size = new System.Drawing.Size(400, 20);
            this.txtConfigName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Description:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Configuration Name:";
            // 
            // tabJob
            // 
            this.tabJob.Controls.Add(this.chkDeleteSourceAfterTransfer);
            this.tabJob.Controls.Add(this.chkIncludeSubfolders);
            this.tabJob.Controls.Add(this.chkEnabled);
            this.tabJob.Controls.Add(this.cmbIntervalType);
            this.tabJob.Controls.Add(this.numInterval);
            this.tabJob.Controls.Add(this.btnBrowseDestination);
            this.tabJob.Controls.Add(this.btnBrowseSource);
            this.tabJob.Controls.Add(this.txtDestinationPath);
            this.tabJob.Controls.Add(this.txtSourcePath);
            this.tabJob.Controls.Add(this.txtJobName);
            this.tabJob.Controls.Add(this.label8);
            this.tabJob.Controls.Add(this.label7);
            this.tabJob.Controls.Add(this.label6);
            this.tabJob.Controls.Add(this.label5);
            this.tabJob.Controls.Add(this.label4);
            this.tabJob.Controls.Add(this.label3);
            this.tabJob.Location = new System.Drawing.Point(4, 22);
            this.tabJob.Name = "tabJob";
            this.tabJob.Padding = new System.Windows.Forms.Padding(3);
            this.tabJob.Size = new System.Drawing.Size(552, 394);
            this.tabJob.TabIndex = 1;
            this.tabJob.Text = "Job Settings";
            this.tabJob.UseVisualStyleBackColor = true;
            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Checked = true;
            this.chkEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnabled.Location = new System.Drawing.Point(20, 280);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(87, 17);
            this.chkEnabled.TabIndex = 13;
            this.chkEnabled.Text = "Job Enabled";
            this.chkEnabled.UseVisualStyleBackColor = true;
            // 
            // cmbIntervalType
            // 
            this.cmbIntervalType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIntervalType.FormattingEnabled = true;
            this.cmbIntervalType.Items.AddRange(new object[] {
            "Seconds",
            "Minutes",
            "Hours"});
            this.cmbIntervalType.Location = new System.Drawing.Point(200, 240);
            this.cmbIntervalType.Name = "cmbIntervalType";
            this.cmbIntervalType.Size = new System.Drawing.Size(100, 21);
            this.cmbIntervalType.TabIndex = 12;
            // 
            // numInterval
            // 
            this.numInterval.Location = new System.Drawing.Point(20, 240);
            this.numInterval.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numInterval.Name = "numInterval";
            this.numInterval.Size = new System.Drawing.Size(120, 20);
            this.numInterval.TabIndex = 11;
            this.numInterval.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // btnBrowseDestination
            // 
            this.btnBrowseDestination.Location = new System.Drawing.Point(450, 160);
            this.btnBrowseDestination.Name = "btnBrowseDestination";
            this.btnBrowseDestination.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseDestination.TabIndex = 10;
            this.btnBrowseDestination.Text = "Browse...";
            this.btnBrowseDestination.UseVisualStyleBackColor = true;
            this.btnBrowseDestination.Click += new System.EventHandler(this.btnBrowseDestination_Click);
            // 
            // btnBrowseSource
            // 
            this.btnBrowseSource.Location = new System.Drawing.Point(450, 100);
            this.btnBrowseSource.Name = "btnBrowseSource";
            this.btnBrowseSource.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseSource.TabIndex = 9;
            this.btnBrowseSource.Text = "Browse...";
            this.btnBrowseSource.UseVisualStyleBackColor = true;
            this.btnBrowseSource.Click += new System.EventHandler(this.btnBrowseSource_Click);
            // 
            // txtDestinationPath
            // 
            this.txtDestinationPath.Location = new System.Drawing.Point(20, 160);
            this.txtDestinationPath.Name = "txtDestinationPath";
            this.txtDestinationPath.Size = new System.Drawing.Size(420, 20);
            this.txtDestinationPath.TabIndex = 8;
            // 
            // txtSourcePath
            // 
            this.txtSourcePath.Location = new System.Drawing.Point(20, 100);
            this.txtSourcePath.Name = "txtSourcePath";
            this.txtSourcePath.Size = new System.Drawing.Size(420, 20);
            this.txtSourcePath.TabIndex = 7;
            // 
            // txtJobName
            // 
            this.txtJobName.Location = new System.Drawing.Point(20, 40);
            this.txtJobName.Name = "txtJobName";
            this.txtJobName.Size = new System.Drawing.Size(400, 20);
            this.txtJobName.TabIndex = 6;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(20, 220);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(73, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "Run Interval:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(200, 220);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(72, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Interval Type:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 260);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "Status:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 140);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Destination Path:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Source Path:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Job Name:";
            // 
            // chkIncludeSubfolders
            // 
            this.chkIncludeSubfolders.AutoSize = true;
            this.chkIncludeSubfolders.Checked = true;
            this.chkIncludeSubfolders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIncludeSubfolders.Location = new System.Drawing.Point(20, 300);
            this.chkIncludeSubfolders.Name = "chkIncludeSubfolders";
            this.chkIncludeSubfolders.Size = new System.Drawing.Size(134, 17);
            this.chkIncludeSubfolders.TabIndex = 13;
            this.chkIncludeSubfolders.Text = "Include Subfolders";
            this.chkIncludeSubfolders.UseVisualStyleBackColor = true;
            // 
            // chkDeleteSourceAfterTransfer
            // 
            this.chkDeleteSourceAfterTransfer.AutoSize = true;
            this.chkDeleteSourceAfterTransfer.Location = new System.Drawing.Point(200, 300);
            this.chkDeleteSourceAfterTransfer.Name = "chkDeleteSourceAfterTransfer";
            this.chkDeleteSourceAfterTransfer.Size = new System.Drawing.Size(180, 17);
            this.chkDeleteSourceAfterTransfer.TabIndex = 14;
            this.chkDeleteSourceAfterTransfer.Text = "Delete source after transfer";
            this.chkDeleteSourceAfterTransfer.UseVisualStyleBackColor = true;
            this.chkDeleteSourceAfterTransfer.CheckedChanged += new System.EventHandler(this.chkDeleteSourceAfterTransfer_CheckedChanged);
            // 
            // tabFilters
            // 
            this.tabFilters.Controls.Add(this.lblFilterHint);
            this.tabFilters.Controls.Add(this.txtExcludeFileTypes);
            this.tabFilters.Controls.Add(this.lblExcludeTypes);
            this.tabFilters.Controls.Add(this.txtIncludeFileTypes);
            this.tabFilters.Controls.Add(this.lblIncludeTypes);
            this.tabFilters.Controls.Add(this.chkEnableFilters);
            this.tabFilters.Location = new System.Drawing.Point(4, 22);
            this.tabFilters.Name = "tabFilters";
            this.tabFilters.Size = new System.Drawing.Size(552, 394);
            this.tabFilters.TabIndex = 2;
            this.tabFilters.Text = "Filters";
            this.tabFilters.UseVisualStyleBackColor = true;
            // 
            // chkEnableFilters
            // 
            this.chkEnableFilters.AutoSize = true;
            this.chkEnableFilters.Location = new System.Drawing.Point(20, 20);
            this.chkEnableFilters.Name = "chkEnableFilters";
            this.chkEnableFilters.Size = new System.Drawing.Size(130, 17);
            this.chkEnableFilters.TabIndex = 0;
            this.chkEnableFilters.Text = "Enable file filtering";
            this.chkEnableFilters.UseVisualStyleBackColor = true;
            this.chkEnableFilters.CheckedChanged += new System.EventHandler(this.chkEnableFilters_CheckedChanged);
            // 
            // lblIncludeTypes
            // 
            this.lblIncludeTypes.AutoSize = true;
            this.lblIncludeTypes.Location = new System.Drawing.Point(20, 50);
            this.lblIncludeTypes.Name = "lblIncludeTypes";
            this.lblIncludeTypes.Size = new System.Drawing.Size(160, 13);
            this.lblIncludeTypes.TabIndex = 1;
            this.lblIncludeTypes.Text = "Include only these file types:";
            // 
            // txtIncludeFileTypes
            // 
            this.txtIncludeFileTypes.Location = new System.Drawing.Point(20, 70);
            this.txtIncludeFileTypes.Name = "txtIncludeFileTypes";
            this.txtIncludeFileTypes.Size = new System.Drawing.Size(500, 20);
            this.txtIncludeFileTypes.TabIndex = 2;
            // 
            // lblExcludeTypes
            // 
            this.lblExcludeTypes.AutoSize = true;
            this.lblExcludeTypes.Location = new System.Drawing.Point(20, 100);
            this.lblExcludeTypes.Name = "lblExcludeTypes";
            this.lblExcludeTypes.Size = new System.Drawing.Size(152, 13);
            this.lblExcludeTypes.TabIndex = 3;
            this.lblExcludeTypes.Text = "Exclude these file types:";
            // 
            // txtExcludeFileTypes
            // 
            this.txtExcludeFileTypes.Location = new System.Drawing.Point(20, 120);
            this.txtExcludeFileTypes.Name = "txtExcludeFileTypes";
            this.txtExcludeFileTypes.Size = new System.Drawing.Size(500, 20);
            this.txtExcludeFileTypes.TabIndex = 4;
            // 
            // lblFilterHint
            // 
            this.lblFilterHint.ForeColor = System.Drawing.Color.Gray;
            this.lblFilterHint.Location = new System.Drawing.Point(20, 150);
            this.lblFilterHint.Name = "lblFilterHint";
            this.lblFilterHint.Size = new System.Drawing.Size(500, 45);
            this.lblFilterHint.TabIndex = 5;
            this.lblFilterHint.Text = "Enter file extensions separated by commas (e.g., pdf,jpg,png for include or tmp,bak,log for exclude).\r\nInclude takes priority - if both are specified, files matching include types will be transferred unless excluded.";
            // 
            // tabConnection
            // 
            this.tabConnection.Controls.Add(this.btnTestConnection);
            this.tabConnection.Controls.Add(this.chkUseSSL);
            this.tabConnection.Controls.Add(this.numPort);
            this.tabConnection.Controls.Add(this.txtPassword);
            this.tabConnection.Controls.Add(this.txtUsername);
            this.tabConnection.Controls.Add(this.txtServer);
            this.tabConnection.Controls.Add(this.cmbConnectionType);
            this.tabConnection.Controls.Add(this.label14);
            this.tabConnection.Controls.Add(this.label13);
            this.tabConnection.Controls.Add(this.label12);
            this.tabConnection.Controls.Add(this.label11);
            this.tabConnection.Controls.Add(this.label10);
            this.tabConnection.Controls.Add(this.label9);
            this.tabConnection.Location = new System.Drawing.Point(4, 22);
            this.tabConnection.Name = "tabConnection";
            this.tabConnection.Size = new System.Drawing.Size(552, 394);
            this.tabConnection.TabIndex = 3;
            this.tabConnection.Text = "Connection";
            this.tabConnection.UseVisualStyleBackColor = true;
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.Location = new System.Drawing.Point(20, 280);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(120, 30);
            this.btnTestConnection.TabIndex = 12;
            this.btnTestConnection.Text = "Test Connection";
            this.btnTestConnection.UseVisualStyleBackColor = true;
            this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);
            // 
            // chkUseSSL
            // 
            this.chkUseSSL.AutoSize = true;
            this.chkUseSSL.Location = new System.Drawing.Point(20, 240);
            this.chkUseSSL.Name = "chkUseSSL";
            this.chkUseSSL.Size = new System.Drawing.Size(69, 17);
            this.chkUseSSL.TabIndex = 11;
            this.chkUseSSL.Text = "Use SSL";
            this.chkUseSSL.UseVisualStyleBackColor = true;
            // 
            // numPort
            // 
            this.numPort.Location = new System.Drawing.Point(20, 200);
            this.numPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(120, 20);
            this.numPort.TabIndex = 10;
            this.numPort.Value = new decimal(new int[] {
            21,
            0,
            0,
            0});
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(20, 160);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(300, 20);
            this.txtPassword.TabIndex = 9;
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(20, 120);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(300, 20);
            this.txtUsername.TabIndex = 8;
            // 
            // txtServer
            // 
            this.txtServer.Location = new System.Drawing.Point(20, 80);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(400, 20);
            this.txtServer.TabIndex = 7;
            // 
            // cmbConnectionType
            // 
            this.cmbConnectionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbConnectionType.FormattingEnabled = true;
            this.cmbConnectionType.Items.AddRange(new object[] {
            "Local",
            "FTP",
            "SFTP"});
            this.cmbConnectionType.Location = new System.Drawing.Point(20, 40);
            this.cmbConnectionType.Name = "cmbConnectionType";
            this.cmbConnectionType.Size = new System.Drawing.Size(150, 21);
            this.cmbConnectionType.TabIndex = 6;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(20, 220);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(51, 13);
            this.label14.TabIndex = 5;
            this.label14.Text = "Options:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(20, 180);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(29, 13);
            this.label13.TabIndex = 4;
            this.label13.Text = "Port:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(20, 140);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(56, 13);
            this.label12.TabIndex = 3;
            this.label12.Text = "Password:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(20, 100);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(58, 13);
            this.label11.TabIndex = 2;
            this.label11.Text = "Username:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(20, 60);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(41, 13);
            this.label10.TabIndex = 1;
            this.label10.Text = "Server:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(20, 20);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(94, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Connection Type:";
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(380, 450);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 35);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnCancel.Location = new System.Drawing.Point(480, 450);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FormEditConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 501);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormEditConfiguration";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Configuration";
            this.tabControl1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.tabJob.ResumeLayout(false);
            this.tabJob.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).EndInit();
            this.tabConnection.ResumeLayout(false);
            this.tabConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabJob;
        private System.Windows.Forms.TabPage tabConnection;
        private System.Windows.Forms.TextBox txtConfigName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.ComboBox cmbIntervalType;
        private System.Windows.Forms.NumericUpDown numInterval;
        private System.Windows.Forms.Button btnBrowseDestination;
        private System.Windows.Forms.Button btnBrowseSource;
        private System.Windows.Forms.TextBox txtDestinationPath;
        private System.Windows.Forms.TextBox txtSourcePath;
        private System.Windows.Forms.TextBox txtJobName;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnTestConnection;
        private System.Windows.Forms.CheckBox chkUseSSL;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.ComboBox cmbConnectionType;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkIncludeSubfolders;
        private System.Windows.Forms.CheckBox chkDeleteSourceAfterTransfer;
        private System.Windows.Forms.TabPage tabFilters;
        private System.Windows.Forms.CheckBox chkEnableFilters;
        private System.Windows.Forms.Label lblIncludeTypes;
        private System.Windows.Forms.TextBox txtIncludeFileTypes;
        private System.Windows.Forms.Label lblExcludeTypes;
        private System.Windows.Forms.TextBox txtExcludeFileTypes;
        private System.Windows.Forms.Label lblFilterHint;
    }
}
