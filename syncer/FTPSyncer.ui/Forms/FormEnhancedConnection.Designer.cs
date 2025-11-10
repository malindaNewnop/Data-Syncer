using System;
using System.Windows.Forms;

namespace FTPSyncer.ui.Forms
{
    partial class FormEnhancedConnection
    {
        private System.ComponentModel.IContainer components = null;
        
        // Profile Management
        private System.Windows.Forms.GroupBox gbProfiles;
        private System.Windows.Forms.Label lblProfile;
        private System.Windows.Forms.ComboBox cmbProfiles;
        private System.Windows.Forms.Label lblProfileName;
        private System.Windows.Forms.TextBox txtProfileName;
        private System.Windows.Forms.Label lblProfileDescription;
        private System.Windows.Forms.TextBox txtProfileDescription;
        private System.Windows.Forms.Button btnSaveProfile;
        private System.Windows.Forms.Button btnDeleteProfile;
        
        // Basic Connection
        private System.Windows.Forms.GroupBox gbBasicConnection;
        private System.Windows.Forms.Label lblProtocol;
        private System.Windows.Forms.ComboBox cmbProtocol;
        private System.Windows.Forms.Label lblHost;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblTimeout;
        private System.Windows.Forms.NumericUpDown numTimeout;
        
        // Authentication
        private System.Windows.Forms.GroupBox gbAuthentication;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.CheckBox chkShowPassword;
        private System.Windows.Forms.Label lblSshKey;
        private System.Windows.Forms.TextBox txtSshKeyPath;
        private System.Windows.Forms.Button btnBrowseKey;
        private System.Windows.Forms.Button btnGenerateKey;
        
        // Advanced SFTP Settings
        private System.Windows.Forms.GroupBox gbSftpAdvanced;
        private System.Windows.Forms.Label lblBandwidthLimit;
        private System.Windows.Forms.NumericUpDown numBandwidthLimit;
        private System.Windows.Forms.Label lblBandwidthUnit;
        private System.Windows.Forms.CheckBox chkEnableResumption;
        private System.Windows.Forms.CheckBox chkVerifyIntegrity;
        private System.Windows.Forms.Label lblRetryAttempts;
        private System.Windows.Forms.NumericUpDown numRetryAttempts;
        private System.Windows.Forms.Label lblRetryDelay;
        private System.Windows.Forms.NumericUpDown numRetryDelay;
        private System.Windows.Forms.CheckBox chkExponentialBackoff;
        private System.Windows.Forms.Label lblConnectionTimeout;
        private System.Windows.Forms.NumericUpDown numConnectionTimeout;
        private System.Windows.Forms.Label lblOperationTimeout;
        private System.Windows.Forms.NumericUpDown numOperationTimeout;
        private System.Windows.Forms.CheckBox chkPreserveTimestamps;
        private System.Windows.Forms.CheckBox chkEnableCompression;
        
        // Testing and Actions
        private System.Windows.Forms.GroupBox gbTesting;
        private System.Windows.Forms.Button btnTestConnection;
        private System.Windows.Forms.Button btnTestAuthMethods;
        private System.Windows.Forms.ProgressBar progressBarTest;
        private System.Windows.Forms.TextBox txtTestResults;
        
        // Dialog Buttons
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;

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
            this.gbProfiles = new System.Windows.Forms.GroupBox();
            this.lblProfile = new System.Windows.Forms.Label();
            this.cmbProfiles = new System.Windows.Forms.ComboBox();
            this.lblProfileName = new System.Windows.Forms.Label();
            this.txtProfileName = new System.Windows.Forms.TextBox();
            this.lblProfileDescription = new System.Windows.Forms.Label();
            this.txtProfileDescription = new System.Windows.Forms.TextBox();
            this.btnSaveProfile = new System.Windows.Forms.Button();
            this.btnDeleteProfile = new System.Windows.Forms.Button();
            
            this.gbBasicConnection = new System.Windows.Forms.GroupBox();
            this.lblProtocol = new System.Windows.Forms.Label();
            this.cmbProtocol = new System.Windows.Forms.ComboBox();
            this.lblHost = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblTimeout = new System.Windows.Forms.Label();
            this.numTimeout = new System.Windows.Forms.NumericUpDown();
            
            this.gbAuthentication = new System.Windows.Forms.GroupBox();
            this.lblUsername = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.chkShowPassword = new System.Windows.Forms.CheckBox();
            this.lblSshKey = new System.Windows.Forms.Label();
            this.txtSshKeyPath = new System.Windows.Forms.TextBox();
            this.btnBrowseKey = new System.Windows.Forms.Button();
            this.btnGenerateKey = new System.Windows.Forms.Button();
            
            this.gbSftpAdvanced = new System.Windows.Forms.GroupBox();
            this.lblBandwidthLimit = new System.Windows.Forms.Label();
            this.numBandwidthLimit = new System.Windows.Forms.NumericUpDown();
            this.lblBandwidthUnit = new System.Windows.Forms.Label();
            this.chkEnableResumption = new System.Windows.Forms.CheckBox();
            this.chkVerifyIntegrity = new System.Windows.Forms.CheckBox();
            this.lblRetryAttempts = new System.Windows.Forms.Label();
            this.numRetryAttempts = new System.Windows.Forms.NumericUpDown();
            this.lblRetryDelay = new System.Windows.Forms.Label();
            this.numRetryDelay = new System.Windows.Forms.NumericUpDown();
            this.chkExponentialBackoff = new System.Windows.Forms.CheckBox();
            this.lblConnectionTimeout = new System.Windows.Forms.Label();
            this.numConnectionTimeout = new System.Windows.Forms.NumericUpDown();
            this.lblOperationTimeout = new System.Windows.Forms.Label();
            this.numOperationTimeout = new System.Windows.Forms.NumericUpDown();
            this.chkPreserveTimestamps = new System.Windows.Forms.CheckBox();
            this.chkEnableCompression = new System.Windows.Forms.CheckBox();
            
            this.gbTesting = new System.Windows.Forms.GroupBox();
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.btnTestAuthMethods = new System.Windows.Forms.Button();
            this.progressBarTest = new System.Windows.Forms.ProgressBar();
            this.txtTestResults = new System.Windows.Forms.TextBox();
            
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            
            // Suspend layout
            this.gbProfiles.SuspendLayout();
            this.gbBasicConnection.SuspendLayout();
            this.gbAuthentication.SuspendLayout();
            this.gbSftpAdvanced.SuspendLayout();
            this.gbTesting.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBandwidthLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRetryAttempts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRetryDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numConnectionTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOperationTimeout)).BeginInit();
            this.SuspendLayout();
            
            // gbProfiles
            this.gbProfiles.Controls.Add(this.lblProfile);
            this.gbProfiles.Controls.Add(this.cmbProfiles);
            this.gbProfiles.Controls.Add(this.lblProfileName);
            this.gbProfiles.Controls.Add(this.txtProfileName);
            this.gbProfiles.Controls.Add(this.lblProfileDescription);
            this.gbProfiles.Controls.Add(this.txtProfileDescription);
            this.gbProfiles.Controls.Add(this.btnSaveProfile);
            this.gbProfiles.Controls.Add(this.btnDeleteProfile);
            this.gbProfiles.Location = new System.Drawing.Point(12, 12);
            this.gbProfiles.Name = "gbProfiles";
            this.gbProfiles.Size = new System.Drawing.Size(610, 120);
            this.gbProfiles.TabIndex = 0;
            this.gbProfiles.TabStop = false;
            this.gbProfiles.Text = "Connection Profiles";
            
            // lblProfile
            this.lblProfile.AutoSize = true;
            this.lblProfile.Location = new System.Drawing.Point(15, 25);
            this.lblProfile.Name = "lblProfile";
            this.lblProfile.Size = new System.Drawing.Size(39, 13);
            this.lblProfile.Text = "Profile:";
            
            // cmbProfiles
            this.cmbProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProfiles.Location = new System.Drawing.Point(80, 22);
            this.cmbProfiles.Name = "cmbProfiles";
            this.cmbProfiles.Size = new System.Drawing.Size(200, 21);
            this.cmbProfiles.TabIndex = 1;
            this.cmbProfiles.SelectedIndexChanged += new System.EventHandler(this.cmbProfiles_SelectedIndexChanged);
            
            // lblProfileName
            this.lblProfileName.AutoSize = true;
            this.lblProfileName.Location = new System.Drawing.Point(15, 55);
            this.lblProfileName.Name = "lblProfileName";
            this.lblProfileName.Size = new System.Drawing.Size(38, 13);
            this.lblProfileName.Text = "Name:";
            
            // txtProfileName
            this.txtProfileName.Location = new System.Drawing.Point(80, 52);
            this.txtProfileName.Name = "txtProfileName";
            this.txtProfileName.Size = new System.Drawing.Size(200, 20);
            this.txtProfileName.TabIndex = 2;
            
            // lblProfileDescription
            this.lblProfileDescription.AutoSize = true;
            this.lblProfileDescription.Location = new System.Drawing.Point(300, 25);
            this.lblProfileDescription.Name = "lblProfileDescription";
            this.lblProfileDescription.Size = new System.Drawing.Size(63, 13);
            this.lblProfileDescription.Text = "Description:";
            
            // txtProfileDescription
            this.txtProfileDescription.Location = new System.Drawing.Point(300, 42);
            this.txtProfileDescription.Multiline = true;
            this.txtProfileDescription.Name = "txtProfileDescription";
            this.txtProfileDescription.Size = new System.Drawing.Size(200, 40);
            this.txtProfileDescription.TabIndex = 3;
            
            // btnSaveProfile            this.btnSaveProfile.Location = new System.Drawing.Point(80, 85);
            this.btnSaveProfile.Name = "btnSaveProfile";
            this.btnSaveProfile.Size = new System.Drawing.Size(90, 25);
            this.btnSaveProfile.TabIndex = 4;
            this.btnSaveProfile.Text = "Save Profile";
            this.btnSaveProfile.UseVisualStyleBackColor = true;
            this.btnSaveProfile.Click += new System.EventHandler(this.btnSaveProfile_Click);
            
            // btnDeleteProfile            this.btnDeleteProfile.Location = new System.Drawing.Point(180, 85);
            this.btnDeleteProfile.Name = "btnDeleteProfile";
            this.btnDeleteProfile.Size = new System.Drawing.Size(90, 25);
            this.btnDeleteProfile.TabIndex = 5;
            this.btnDeleteProfile.Text = "Delete Profile";
            this.btnDeleteProfile.UseVisualStyleBackColor = true;
            this.btnDeleteProfile.Click += new System.EventHandler(this.btnDeleteProfile_Click);
            
            // gbBasicConnection
            this.gbBasicConnection.Controls.Add(this.lblProtocol);
            this.gbBasicConnection.Controls.Add(this.cmbProtocol);
            this.gbBasicConnection.Controls.Add(this.lblHost);
            this.gbBasicConnection.Controls.Add(this.txtHost);
            this.gbBasicConnection.Controls.Add(this.lblPort);
            this.gbBasicConnection.Controls.Add(this.txtPort);
            this.gbBasicConnection.Controls.Add(this.lblTimeout);
            this.gbBasicConnection.Controls.Add(this.numTimeout);
            this.gbBasicConnection.Location = new System.Drawing.Point(12, 140);
            this.gbBasicConnection.Name = "gbBasicConnection";
            this.gbBasicConnection.Size = new System.Drawing.Size(300, 130);
            this.gbBasicConnection.TabIndex = 1;
            this.gbBasicConnection.TabStop = false;
            this.gbBasicConnection.Text = "Basic Connection";
            
            // lblProtocol
            this.lblProtocol.AutoSize = true;
            this.lblProtocol.Location = new System.Drawing.Point(15, 25);
            this.lblProtocol.Name = "lblProtocol";
            this.lblProtocol.Size = new System.Drawing.Size(49, 13);
            this.lblProtocol.Text = "Protocol:";
            
            // cmbProtocol
            this.cmbProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProtocol.Items.AddRange(new object[] { "LOCAL", "FTP", "SFTP" });
            this.cmbProtocol.Location = new System.Drawing.Point(80, 22);
            this.cmbProtocol.Name = "cmbProtocol";
            this.cmbProtocol.Size = new System.Drawing.Size(120, 21);
            this.cmbProtocol.TabIndex = 1;
            this.cmbProtocol.SelectedIndexChanged += new System.EventHandler(this.cmbProtocol_SelectedIndexChanged);
            
            // lblHost
            this.lblHost.AutoSize = true;
            this.lblHost.Location = new System.Drawing.Point(15, 55);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new System.Drawing.Size(32, 13);
            this.lblHost.Text = "Host:";
            
            // txtHost
            this.txtHost.Location = new System.Drawing.Point(80, 52);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(200, 20);
            this.txtHost.TabIndex = 2;
            
            // lblPort
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(15, 85);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(29, 13);
            this.lblPort.Text = "Port:";
            
            // txtPort
            this.txtPort.Location = new System.Drawing.Point(80, 82);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(60, 20);
            this.txtPort.TabIndex = 3;
            
            // lblTimeout
            this.lblTimeout.AutoSize = true;
            this.lblTimeout.Location = new System.Drawing.Point(150, 85);
            this.lblTimeout.Name = "lblTimeout";
            this.lblTimeout.Size = new System.Drawing.Size(48, 13);
            this.lblTimeout.Text = "Timeout:";
            
            // numTimeout
            this.numTimeout.Location = new System.Drawing.Point(200, 82);
            this.numTimeout.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            this.numTimeout.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            this.numTimeout.Name = "numTimeout";
            this.numTimeout.Size = new System.Drawing.Size(60, 20);
            this.numTimeout.TabIndex = 4;
            this.numTimeout.Value = new decimal(new int[] { 30, 0, 0, 0 });
            
            // gbAuthentication
            this.gbAuthentication.Controls.Add(this.lblUsername);
            this.gbAuthentication.Controls.Add(this.txtUsername);
            this.gbAuthentication.Controls.Add(this.lblPassword);
            this.gbAuthentication.Controls.Add(this.txtPassword);
            this.gbAuthentication.Controls.Add(this.chkShowPassword);
            this.gbAuthentication.Controls.Add(this.lblSshKey);
            this.gbAuthentication.Controls.Add(this.txtSshKeyPath);
            this.gbAuthentication.Controls.Add(this.btnBrowseKey);
            this.gbAuthentication.Controls.Add(this.btnGenerateKey);
            this.gbAuthentication.Location = new System.Drawing.Point(322, 140);
            this.gbAuthentication.Name = "gbAuthentication";
            this.gbAuthentication.Size = new System.Drawing.Size(300, 130);
            this.gbAuthentication.TabIndex = 2;
            this.gbAuthentication.TabStop = false;
            this.gbAuthentication.Text = "Authentication";
            
            // lblUsername
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(15, 25);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(58, 13);
            this.lblUsername.Text = "Username:";
            
            // txtUsername
            this.txtUsername.Location = new System.Drawing.Point(80, 22);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(200, 20);
            this.txtUsername.TabIndex = 1;
            
            // lblPassword
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(15, 55);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.Text = "Password:";
            
            // txtPassword
            this.txtPassword.Location = new System.Drawing.Point(80, 52);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(150, 20);
            this.txtPassword.TabIndex = 2;
            this.txtPassword.UseSystemPasswordChar = true;
            
            // chkShowPassword
            this.chkShowPassword.AutoSize = true;
            this.chkShowPassword.Location = new System.Drawing.Point(240, 54);
            this.chkShowPassword.Name = "chkShowPassword";
            this.chkShowPassword.Size = new System.Drawing.Size(53, 17);
            this.chkShowPassword.Text = "Show";
            this.chkShowPassword.UseVisualStyleBackColor = true;
            this.chkShowPassword.CheckedChanged += new System.EventHandler(this.chkShowPassword_CheckedChanged);
            
            // lblSshKey
            this.lblSshKey.AutoSize = true;
            this.lblSshKey.Location = new System.Drawing.Point(15, 85);
            this.lblSshKey.Name = "lblSshKey";
            this.lblSshKey.Size = new System.Drawing.Size(55, 13);
            this.lblSshKey.Text = "SSH Key:";
            
            // txtSshKeyPath
            this.txtSshKeyPath.Location = new System.Drawing.Point(80, 82);
            this.txtSshKeyPath.Name = "txtSshKeyPath";
            this.txtSshKeyPath.Size = new System.Drawing.Size(150, 20);
            this.txtSshKeyPath.TabIndex = 3;
            
            // btnBrowseKey
            this.btnBrowseKey.Location = new System.Drawing.Point(240, 80);
            this.btnBrowseKey.Name = "btnBrowseKey";
            this.btnBrowseKey.Size = new System.Drawing.Size(50, 23);
            this.btnBrowseKey.TabIndex = 4;
            this.btnBrowseKey.Text = "Browse";
            this.btnBrowseKey.UseVisualStyleBackColor = true;
            this.btnBrowseKey.Click += new System.EventHandler(this.btnBrowseKey_Click);
            
            // btnGenerateKey
            this.btnGenerateKey.Location = new System.Drawing.Point(80, 105);
            this.btnGenerateKey.Name = "btnGenerateKey";
            this.btnGenerateKey.Size = new System.Drawing.Size(100, 23);
            this.btnGenerateKey.TabIndex = 5;
            this.btnGenerateKey.Text = "Generate New Key";
            this.btnGenerateKey.UseVisualStyleBackColor = true;
            this.btnGenerateKey.Click += new System.EventHandler(this.btnGenerateKey_Click);
            
            // gbSftpAdvanced
            this.gbSftpAdvanced.Controls.Add(this.lblBandwidthLimit);
            this.gbSftpAdvanced.Controls.Add(this.numBandwidthLimit);
            this.gbSftpAdvanced.Controls.Add(this.lblBandwidthUnit);
            this.gbSftpAdvanced.Controls.Add(this.chkEnableResumption);
            this.gbSftpAdvanced.Controls.Add(this.chkVerifyIntegrity);
            this.gbSftpAdvanced.Controls.Add(this.lblRetryAttempts);
            this.gbSftpAdvanced.Controls.Add(this.numRetryAttempts);
            this.gbSftpAdvanced.Controls.Add(this.lblRetryDelay);
            this.gbSftpAdvanced.Controls.Add(this.numRetryDelay);
            this.gbSftpAdvanced.Controls.Add(this.chkExponentialBackoff);
            this.gbSftpAdvanced.Controls.Add(this.lblConnectionTimeout);
            this.gbSftpAdvanced.Controls.Add(this.numConnectionTimeout);
            this.gbSftpAdvanced.Controls.Add(this.lblOperationTimeout);
            this.gbSftpAdvanced.Controls.Add(this.numOperationTimeout);
            this.gbSftpAdvanced.Controls.Add(this.chkPreserveTimestamps);
            this.gbSftpAdvanced.Controls.Add(this.chkEnableCompression);
            this.gbSftpAdvanced.Location = new System.Drawing.Point(12, 280);
            this.gbSftpAdvanced.Name = "gbSftpAdvanced";
            this.gbSftpAdvanced.Size = new System.Drawing.Size(610, 200);
            this.gbSftpAdvanced.TabIndex = 3;
            this.gbSftpAdvanced.TabStop = false;
            this.gbSftpAdvanced.Text = "Advanced SFTP Settings";
            
            // lblBandwidthLimit
            this.lblBandwidthLimit.AutoSize = true;
            this.lblBandwidthLimit.Location = new System.Drawing.Point(15, 25);
            this.lblBandwidthLimit.Name = "lblBandwidthLimit";
            this.lblBandwidthLimit.Size = new System.Drawing.Size(85, 13);
            this.lblBandwidthLimit.Text = "Bandwidth Limit:";
            
            // numBandwidthLimit
            this.numBandwidthLimit.Location = new System.Drawing.Point(110, 22);
            this.numBandwidthLimit.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            this.numBandwidthLimit.Name = "numBandwidthLimit";
            this.numBandwidthLimit.Size = new System.Drawing.Size(80, 20);
            this.numBandwidthLimit.TabIndex = 1;
            
            // lblBandwidthUnit
            this.lblBandwidthUnit.AutoSize = true;
            this.lblBandwidthUnit.Location = new System.Drawing.Point(195, 25);
            this.lblBandwidthUnit.Name = "lblBandwidthUnit";
            this.lblBandwidthUnit.Size = new System.Drawing.Size(70, 13);
            this.lblBandwidthUnit.Text = "KB/s (0=unlimited)";
            
            // chkEnableResumption
            this.chkEnableResumption.AutoSize = true;
            this.chkEnableResumption.Location = new System.Drawing.Point(15, 55);
            this.chkEnableResumption.Name = "chkEnableResumption";
            this.chkEnableResumption.Size = new System.Drawing.Size(133, 17);
            this.chkEnableResumption.Text = "Enable Transfer Resume";
            this.chkEnableResumption.UseVisualStyleBackColor = true;
            
            // chkVerifyIntegrity
            this.chkVerifyIntegrity.AutoSize = true;
            this.chkVerifyIntegrity.Location = new System.Drawing.Point(200, 55);
            this.chkVerifyIntegrity.Name = "chkVerifyIntegrity";
            this.chkVerifyIntegrity.Size = new System.Drawing.Size(115, 17);
            this.chkVerifyIntegrity.Text = "Verify File Integrity";
            this.chkVerifyIntegrity.UseVisualStyleBackColor = true;
            
            // lblRetryAttempts
            this.lblRetryAttempts.AutoSize = true;
            this.lblRetryAttempts.Location = new System.Drawing.Point(15, 85);
            this.lblRetryAttempts.Name = "lblRetryAttempts";
            this.lblRetryAttempts.Size = new System.Drawing.Size(79, 13);
            this.lblRetryAttempts.Text = "Retry Attempts:";
            
            // numRetryAttempts
            this.numRetryAttempts.Location = new System.Drawing.Point(110, 82);
            this.numRetryAttempts.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numRetryAttempts.Name = "numRetryAttempts";
            this.numRetryAttempts.Size = new System.Drawing.Size(50, 20);
            this.numRetryAttempts.TabIndex = 2;
            
            // lblRetryDelay
            this.lblRetryDelay.AutoSize = true;
            this.lblRetryDelay.Location = new System.Drawing.Point(200, 85);
            this.lblRetryDelay.Name = "lblRetryDelay";
            this.lblRetryDelay.Size = new System.Drawing.Size(69, 13);
            this.lblRetryDelay.Text = "Retry Delay (ms):";
            
            // numRetryDelay
            this.numRetryDelay.Location = new System.Drawing.Point(280, 82);
            this.numRetryDelay.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            this.numRetryDelay.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numRetryDelay.Name = "numRetryDelay";
            this.numRetryDelay.Size = new System.Drawing.Size(70, 20);
            this.numRetryDelay.TabIndex = 3;
            this.numRetryDelay.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            
            // chkExponentialBackoff
            this.chkExponentialBackoff.AutoSize = true;
            this.chkExponentialBackoff.Location = new System.Drawing.Point(15, 115);
            this.chkExponentialBackoff.Name = "chkExponentialBackoff";
            this.chkExponentialBackoff.Size = new System.Drawing.Size(125, 17);
            this.chkExponentialBackoff.Text = "Exponential Backoff";
            this.chkExponentialBackoff.UseVisualStyleBackColor = true;
            
            // lblConnectionTimeout
            this.lblConnectionTimeout.AutoSize = true;
            this.lblConnectionTimeout.Location = new System.Drawing.Point(15, 145);
            this.lblConnectionTimeout.Name = "lblConnectionTimeout";
            this.lblConnectionTimeout.Size = new System.Drawing.Size(110, 13);
            this.lblConnectionTimeout.Text = "Connection Timeout (ms):";
            
            // numConnectionTimeout
            this.numConnectionTimeout.Location = new System.Drawing.Point(130, 142);
            this.numConnectionTimeout.Maximum = new decimal(new int[] { 300000, 0, 0, 0 });
            this.numConnectionTimeout.Minimum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numConnectionTimeout.Name = "numConnectionTimeout";
            this.numConnectionTimeout.Size = new System.Drawing.Size(80, 20);
            this.numConnectionTimeout.TabIndex = 4;
            this.numConnectionTimeout.Value = new decimal(new int[] { 30000, 0, 0, 0 });
            
            // lblOperationTimeout
            this.lblOperationTimeout.AutoSize = true;
            this.lblOperationTimeout.Location = new System.Drawing.Point(250, 145);
            this.lblOperationTimeout.Name = "lblOperationTimeout";
            this.lblOperationTimeout.Size = new System.Drawing.Size(102, 13);
            this.lblOperationTimeout.Text = "Operation Timeout (ms):";
            
            // numOperationTimeout
            this.numOperationTimeout.Location = new System.Drawing.Point(360, 142);
            this.numOperationTimeout.Maximum = new decimal(new int[] { 600000, 0, 0, 0 });
            this.numOperationTimeout.Minimum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numOperationTimeout.Name = "numOperationTimeout";
            this.numOperationTimeout.Size = new System.Drawing.Size(80, 20);
            this.numOperationTimeout.TabIndex = 5;
            this.numOperationTimeout.Value = new decimal(new int[] { 60000, 0, 0, 0 });
            
            // chkPreserveTimestamps
            this.chkPreserveTimestamps.AutoSize = true;
            this.chkPreserveTimestamps.Location = new System.Drawing.Point(15, 175);
            this.chkPreserveTimestamps.Name = "chkPreserveTimestamps";
            this.chkPreserveTimestamps.Size = new System.Drawing.Size(127, 17);
            this.chkPreserveTimestamps.Text = "Preserve Timestamps";
            this.chkPreserveTimestamps.UseVisualStyleBackColor = true;
            
            // chkEnableCompression
            this.chkEnableCompression.AutoSize = true;
            this.chkEnableCompression.Location = new System.Drawing.Point(200, 175);
            this.chkEnableCompression.Name = "chkEnableCompression";
            this.chkEnableCompression.Size = new System.Drawing.Size(120, 17);
            this.chkEnableCompression.Text = "Enable Compression";
            this.chkEnableCompression.UseVisualStyleBackColor = true;
            
            // gbTesting
            this.gbTesting.Controls.Add(this.btnTestConnection);
            this.gbTesting.Controls.Add(this.btnTestAuthMethods);
            this.gbTesting.Controls.Add(this.progressBarTest);
            this.gbTesting.Controls.Add(this.txtTestResults);
            this.gbTesting.Location = new System.Drawing.Point(12, 490);
            this.gbTesting.Name = "gbTesting";
            this.gbTesting.Size = new System.Drawing.Size(610, 120);
            this.gbTesting.TabIndex = 4;
            this.gbTesting.TabStop = false;
            this.gbTesting.Text = "Connection Testing";
            
            // btnTestConnection            this.btnTestConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnTestConnection.Location = new System.Drawing.Point(15, 25);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(120, 30);
            this.btnTestConnection.TabIndex = 1;
            this.btnTestConnection.Text = "Test Connection";
            this.btnTestConnection.UseVisualStyleBackColor = true;
            this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);
            
            // btnTestAuthMethods            this.btnTestAuthMethods.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnTestAuthMethods.Location = new System.Drawing.Point(145, 25);
            this.btnTestAuthMethods.Name = "btnTestAuthMethods";
            this.btnTestAuthMethods.Size = new System.Drawing.Size(120, 30);
            this.btnTestAuthMethods.TabIndex = 2;
            this.btnTestAuthMethods.Text = "Test Auth Methods";
            this.btnTestAuthMethods.UseVisualStyleBackColor = true;
            this.btnTestAuthMethods.Click += new System.EventHandler(this.btnTestAuthMethods_Click);
            
            // progressBarTest
            this.progressBarTest.Location = new System.Drawing.Point(275, 30);
            this.progressBarTest.Name = "progressBarTest";
            this.progressBarTest.Size = new System.Drawing.Size(200, 20);
            this.progressBarTest.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBarTest.TabIndex = 3;
            this.progressBarTest.Visible = false;
            
            // txtTestResults
            this.txtTestResults.Location = new System.Drawing.Point(15, 65);
            this.txtTestResults.Multiline = true;
            this.txtTestResults.Name = "txtTestResults";
            this.txtTestResults.ReadOnly = true;
            this.txtTestResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTestResults.Size = new System.Drawing.Size(580, 45);
            this.txtTestResults.TabIndex = 4;
            
            // btnOK            this.btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnOK.Location = new System.Drawing.Point(450, 620);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 35);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            
            // btnCancel            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Location = new System.Drawing.Point(540, 620);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 35);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            
            // FormEnhancedConnection
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 670);
            this.Controls.Add(this.gbProfiles);
            this.Controls.Add(this.gbBasicConnection);
            this.Controls.Add(this.gbAuthentication);
            this.Controls.Add(this.gbSftpAdvanced);
            this.Controls.Add(this.gbTesting);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Name = "FormEnhancedConnection";
            this.Text = "Enhanced SFTP Connection Settings";
            
            // Resume layout
            this.gbProfiles.ResumeLayout(false);
            this.gbProfiles.PerformLayout();
            this.gbBasicConnection.ResumeLayout(false);
            this.gbBasicConnection.PerformLayout();
            this.gbAuthentication.ResumeLayout(false);
            this.gbAuthentication.PerformLayout();
            this.gbSftpAdvanced.ResumeLayout(false);
            this.gbSftpAdvanced.PerformLayout();
            this.gbTesting.ResumeLayout(false);
            this.gbTesting.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBandwidthLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRetryAttempts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRetryDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numConnectionTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOperationTimeout)).EndInit();
            this.ResumeLayout(false);
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }
    }
}





