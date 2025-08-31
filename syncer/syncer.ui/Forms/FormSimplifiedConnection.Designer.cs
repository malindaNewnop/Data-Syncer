namespace syncer.ui.Forms
{
    partial class FormSimplifiedConnection
    {
        private System.ComponentModel.IContainer components = null;
        
        private System.Windows.Forms.GroupBox gbConnection;
        private System.Windows.Forms.Label lblProtocol;
        private System.Windows.Forms.ComboBox cmbProtocol;
        private System.Windows.Forms.Label lblHost;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        
        private System.Windows.Forms.GroupBox gbSftpSettings;
        private System.Windows.Forms.CheckBox chkUseKeyAuth;
        private System.Windows.Forms.Label lblSshKeyPath;
        private System.Windows.Forms.TextBox txtSshKeyPath;
        private System.Windows.Forms.Button btnBrowseKey;
        private System.Windows.Forms.Button btnGenerateKey;
        private System.Windows.Forms.Label lblTimeout;
        private System.Windows.Forms.NumericUpDown numTimeout;
        
        private System.Windows.Forms.Button btnTestConnection;
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
            this.gbConnection = new System.Windows.Forms.GroupBox();
            this.lblProtocol = new System.Windows.Forms.Label();
            this.cmbProtocol = new System.Windows.Forms.ComboBox();
            this.lblHost = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblUsername = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            
            this.gbSftpSettings = new System.Windows.Forms.GroupBox();
            this.chkUseKeyAuth = new System.Windows.Forms.CheckBox();
            this.lblSshKeyPath = new System.Windows.Forms.Label();
            this.txtSshKeyPath = new System.Windows.Forms.TextBox();
            this.btnBrowseKey = new System.Windows.Forms.Button();
            this.btnGenerateKey = new System.Windows.Forms.Button();
            this.lblTimeout = new System.Windows.Forms.Label();
            this.numTimeout = new System.Windows.Forms.NumericUpDown();
            
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            
            this.gbConnection.SuspendLayout();
            this.gbSftpSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).BeginInit();
            this.SuspendLayout();
            
            // gbConnection
            this.gbConnection.Controls.Add(this.lblProtocol);
            this.gbConnection.Controls.Add(this.cmbProtocol);
            this.gbConnection.Controls.Add(this.lblHost);
            this.gbConnection.Controls.Add(this.txtHost);
            this.gbConnection.Controls.Add(this.lblPort);
            this.gbConnection.Controls.Add(this.txtPort);
            this.gbConnection.Controls.Add(this.lblUsername);
            this.gbConnection.Controls.Add(this.txtUsername);
            this.gbConnection.Controls.Add(this.lblPassword);
            this.gbConnection.Controls.Add(this.txtPassword);
            this.gbConnection.Location = new System.Drawing.Point(12, 12);
            this.gbConnection.Name = "gbConnection";
            this.gbConnection.Size = new System.Drawing.Size(460, 160);
            this.gbConnection.TabIndex = 0;
            this.gbConnection.TabStop = false;
            this.gbConnection.Text = "Connection Settings";
            
            // lblProtocol
            this.lblProtocol.AutoSize = true;
            this.lblProtocol.Location = new System.Drawing.Point(15, 25);
            this.lblProtocol.Name = "lblProtocol";
            this.lblProtocol.Size = new System.Drawing.Size(49, 13);
            this.lblProtocol.Text = "Protocol:";
            
            // cmbProtocol
            this.cmbProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProtocol.Location = new System.Drawing.Point(100, 22);
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
            this.txtHost.Location = new System.Drawing.Point(100, 52);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(200, 20);
            this.txtHost.TabIndex = 2;
            
            // lblPort
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(320, 55);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(29, 13);
            this.lblPort.Text = "Port:";
            
            // txtPort
            this.txtPort.Location = new System.Drawing.Point(355, 52);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(60, 20);
            this.txtPort.TabIndex = 3;
            
            // lblUsername
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(15, 85);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(58, 13);
            this.lblUsername.Text = "Username:";
            
            // txtUsername
            this.txtUsername.Location = new System.Drawing.Point(100, 82);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(150, 20);
            this.txtUsername.TabIndex = 4;
            
            // lblPassword
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(15, 115);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.Text = "Password:";
            
            // txtPassword
            this.txtPassword.Location = new System.Drawing.Point(100, 112);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(150, 20);
            this.txtPassword.TabIndex = 5;
            this.txtPassword.UseSystemPasswordChar = true;
            
            // gbSftpSettings
            this.gbSftpSettings.Controls.Add(this.chkUseKeyAuth);
            this.gbSftpSettings.Controls.Add(this.lblSshKeyPath);
            this.gbSftpSettings.Controls.Add(this.txtSshKeyPath);
            this.gbSftpSettings.Controls.Add(this.btnBrowseKey);
            this.gbSftpSettings.Controls.Add(this.btnGenerateKey);
            this.gbSftpSettings.Controls.Add(this.lblTimeout);
            this.gbSftpSettings.Controls.Add(this.numTimeout);
            this.gbSftpSettings.Location = new System.Drawing.Point(12, 180);
            this.gbSftpSettings.Name = "gbSftpSettings";
            this.gbSftpSettings.Size = new System.Drawing.Size(460, 120);
            this.gbSftpSettings.TabIndex = 1;
            this.gbSftpSettings.TabStop = false;
            this.gbSftpSettings.Text = "SFTP Settings";
            
            // chkUseKeyAuth
            this.chkUseKeyAuth.AutoSize = true;
            this.chkUseKeyAuth.Location = new System.Drawing.Point(15, 25);
            this.chkUseKeyAuth.Name = "chkUseKeyAuth";
            this.chkUseKeyAuth.Size = new System.Drawing.Size(143, 17);
            this.chkUseKeyAuth.Text = "Use SSH Key Authentication";
            this.chkUseKeyAuth.UseVisualStyleBackColor = true;
            this.chkUseKeyAuth.CheckedChanged += new System.EventHandler(this.chkUseKeyAuth_CheckedChanged);
            
            // lblSshKeyPath
            this.lblSshKeyPath.AutoSize = true;
            this.lblSshKeyPath.Location = new System.Drawing.Point(15, 55);
            this.lblSshKeyPath.Name = "lblSshKeyPath";
            this.lblSshKeyPath.Size = new System.Drawing.Size(75, 13);
            this.lblSshKeyPath.Text = "SSH Key Path:";
            
            // txtSshKeyPath
            this.txtSshKeyPath.Location = new System.Drawing.Point(100, 52);
            this.txtSshKeyPath.Name = "txtSshKeyPath";
            this.txtSshKeyPath.Size = new System.Drawing.Size(250, 20);
            this.txtSshKeyPath.TabIndex = 2;
            
            // btnBrowseKey
            this.btnBrowseKey.Location = new System.Drawing.Point(360, 50);
            this.btnBrowseKey.Name = "btnBrowseKey";
            this.btnBrowseKey.Size = new System.Drawing.Size(60, 23);
            this.btnBrowseKey.TabIndex = 3;
            this.btnBrowseKey.Text = "Browse";
            this.btnBrowseKey.UseVisualStyleBackColor = true;
            this.btnBrowseKey.Click += new System.EventHandler(this.btnBrowseKey_Click);
            
            // btnGenerateKey
            this.btnGenerateKey.Location = new System.Drawing.Point(100, 80);
            this.btnGenerateKey.Name = "btnGenerateKey";
            this.btnGenerateKey.Size = new System.Drawing.Size(100, 25);
            this.btnGenerateKey.TabIndex = 4;
            this.btnGenerateKey.Text = "Generate Key";
            this.btnGenerateKey.UseVisualStyleBackColor = true;
            this.btnGenerateKey.Click += new System.EventHandler(this.btnGenerateKey_Click);
            
            // lblTimeout
            this.lblTimeout.AutoSize = true;
            this.lblTimeout.Location = new System.Drawing.Point(220, 85);
            this.lblTimeout.Name = "lblTimeout";
            this.lblTimeout.Size = new System.Drawing.Size(78, 13);
            this.lblTimeout.Text = "Timeout (sec):";
            
            // numTimeout
            this.numTimeout.Location = new System.Drawing.Point(305, 83);
            this.numTimeout.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            this.numTimeout.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            this.numTimeout.Name = "numTimeout";
            this.numTimeout.Size = new System.Drawing.Size(60, 20);
            this.numTimeout.TabIndex = 5;
            this.numTimeout.Value = new decimal(new int[] { 30, 0, 0, 0 });
            
            // btnTestConnection
            this.btnTestConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnTestConnection.Location = new System.Drawing.Point(12, 310);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(120, 30);
            this.btnTestConnection.TabIndex = 2;
            this.btnTestConnection.Text = "Test Connection";
            this.btnTestConnection.UseVisualStyleBackColor = true;
            this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);
            
            // btnOK
            this.btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnOK.Location = new System.Drawing.Point(300, 310);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 30);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            
            // btnCancel
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Location = new System.Drawing.Point(390, 310);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            
            // FormSimplifiedConnection
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 352);
            this.Controls.Add(this.gbConnection);
            this.Controls.Add(this.gbSftpSettings);
            this.Controls.Add(this.btnTestConnection);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Name = "FormSimplifiedConnection";
            this.Text = "Enhanced SFTP Connection Settings";
            
            this.gbConnection.ResumeLayout(false);
            this.gbConnection.PerformLayout();
            this.gbSftpSettings.ResumeLayout(false);
            this.gbSftpSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
