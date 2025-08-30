namespace syncer.ui
{
    partial class FormConnection
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabConnectionSettings;
        private System.Windows.Forms.TabPage tabSSHKeyGeneration;
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
        private System.Windows.Forms.CheckBox chkShowPassword;
        private System.Windows.Forms.Button btnTestConnection;
        private System.Windows.Forms.Button btnSaveConnection;
        private System.Windows.Forms.Button btnLoadConnection;
        private System.Windows.Forms.Button btnManageConnections;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        
        // Connection Name for saving
        private System.Windows.Forms.Label lblConnectionName;
        private System.Windows.Forms.TextBox txtConnectionName;
        
        // SSH Key Authentication controls (for SFTP)
        private System.Windows.Forms.CheckBox chkUseSSHKey;
        private System.Windows.Forms.Label lblSSHKeyPath;
        private System.Windows.Forms.TextBox txtSSHKeyPath;
        private System.Windows.Forms.Button btnBrowseSSHKey;
        
        // SSH Key Generation tab controls
        private System.Windows.Forms.Label lblKeyPath;
        private System.Windows.Forms.TextBox txtKeyPath;
        private System.Windows.Forms.Button btnBrowseKey;
        private System.Windows.Forms.Label lblTimeout;
        private System.Windows.Forms.NumericUpDown numTimeout;

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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabConnectionSettings = new System.Windows.Forms.TabPage();
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
            this.chkShowPassword = new System.Windows.Forms.CheckBox();
            this.lblConnectionName = new System.Windows.Forms.Label();
            this.txtConnectionName = new System.Windows.Forms.TextBox();
            this.chkUseSSHKey = new System.Windows.Forms.CheckBox();
            this.lblSSHKeyPath = new System.Windows.Forms.Label();
            this.txtSSHKeyPath = new System.Windows.Forms.TextBox();
            this.btnBrowseSSHKey = new System.Windows.Forms.Button();
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.btnSaveConnection = new System.Windows.Forms.Button();
            this.btnLoadConnection = new System.Windows.Forms.Button();
            this.btnManageConnections = new System.Windows.Forms.Button();
            this.tabSSHKeyGeneration = new System.Windows.Forms.TabPage();
            this.lblKeyPath = new System.Windows.Forms.Label();
            this.txtKeyPath = new System.Windows.Forms.TextBox();
            this.btnBrowseKey = new System.Windows.Forms.Button();
            this.lblTimeout = new System.Windows.Forms.Label();
            this.numTimeout = new System.Windows.Forms.NumericUpDown();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabConnectionSettings.SuspendLayout();
            this.tabSSHKeyGeneration.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabConnectionSettings);
            this.tabControl.Controls.Add(this.tabSSHKeyGeneration);
            this.tabControl.Location = new System.Drawing.Point(20, 20);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(860, 480);
            this.tabControl.TabIndex = 0;
            // 
            // tabConnectionSettings
            // 
            this.tabConnectionSettings.Controls.Add(this.lblProtocol);
            this.tabConnectionSettings.Controls.Add(this.btnCancel);
            this.tabConnectionSettings.Controls.Add(this.btnSave);
            this.tabConnectionSettings.Controls.Add(this.cmbProtocol);
            this.tabConnectionSettings.Controls.Add(this.lblHost);
            this.tabConnectionSettings.Controls.Add(this.txtHost);
            this.tabConnectionSettings.Controls.Add(this.lblPort);
            this.tabConnectionSettings.Controls.Add(this.txtPort);
            this.tabConnectionSettings.Controls.Add(this.lblUsername);
            this.tabConnectionSettings.Controls.Add(this.txtUsername);
            this.tabConnectionSettings.Controls.Add(this.lblPassword);
            this.tabConnectionSettings.Controls.Add(this.txtPassword);
            this.tabConnectionSettings.Controls.Add(this.chkShowPassword);
            this.tabConnectionSettings.Controls.Add(this.lblConnectionName);
            this.tabConnectionSettings.Controls.Add(this.txtConnectionName);
            this.tabConnectionSettings.Controls.Add(this.chkUseSSHKey);
            this.tabConnectionSettings.Controls.Add(this.lblSSHKeyPath);
            this.tabConnectionSettings.Controls.Add(this.txtSSHKeyPath);
            this.tabConnectionSettings.Controls.Add(this.btnBrowseSSHKey);
            this.tabConnectionSettings.Controls.Add(this.btnTestConnection);
            this.tabConnectionSettings.Controls.Add(this.btnSaveConnection);
            this.tabConnectionSettings.Controls.Add(this.btnLoadConnection);
            this.tabConnectionSettings.Controls.Add(this.btnManageConnections);
            this.tabConnectionSettings.Location = new System.Drawing.Point(4, 27);
            this.tabConnectionSettings.Name = "tabConnectionSettings";
            this.tabConnectionSettings.Padding = new System.Windows.Forms.Padding(20);
            this.tabConnectionSettings.Size = new System.Drawing.Size(852, 449);
            this.tabConnectionSettings.TabIndex = 0;
            this.tabConnectionSettings.Text = "Connection Settings";
            this.tabConnectionSettings.UseVisualStyleBackColor = true;
            // 
            // lblProtocol
            // 
            this.lblProtocol.AutoSize = true;
            this.lblProtocol.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProtocol.Location = new System.Drawing.Point(30, 40);
            this.lblProtocol.Name = "lblProtocol";
            this.lblProtocol.Size = new System.Drawing.Size(69, 18);
            this.lblProtocol.TabIndex = 0;
            this.lblProtocol.Text = "Protocol:";
            // 
            // cmbProtocol
            // 
            this.cmbProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProtocol.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbProtocol.FormattingEnabled = true;
            this.cmbProtocol.Items.AddRange(new object[] {
            "LOCAL",
            "FTP",
            "SFTP"});
            this.cmbProtocol.Location = new System.Drawing.Point(150, 37);
            this.cmbProtocol.Name = "cmbProtocol";
            this.cmbProtocol.Size = new System.Drawing.Size(150, 26);
            this.cmbProtocol.TabIndex = 1;
            this.cmbProtocol.SelectedIndexChanged += new System.EventHandler(this.cmbProtocol_SelectedIndexChanged);
            // 
            // lblHost
            // 
            this.lblHost.AutoSize = true;
            this.lblHost.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHost.Location = new System.Drawing.Point(30, 85);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new System.Drawing.Size(44, 18);
            this.lblHost.TabIndex = 2;
            this.lblHost.Text = "Host:";
            // 
            // txtHost
            // 
            this.txtHost.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHost.Location = new System.Drawing.Point(150, 82);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(350, 24);
            this.txtHost.TabIndex = 3;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPort.Location = new System.Drawing.Point(520, 85);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(40, 18);
            this.lblPort.TabIndex = 4;
            this.lblPort.Text = "Port:";
            // 
            // txtPort
            // 
            this.txtPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPort.Location = new System.Drawing.Point(560, 82);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(80, 24);
            this.txtPort.TabIndex = 5;
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsername.Location = new System.Drawing.Point(30, 130);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(81, 18);
            this.lblUsername.TabIndex = 6;
            this.lblUsername.Text = "Username:";
            // 
            // txtUsername
            // 
            this.txtUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUsername.Location = new System.Drawing.Point(150, 127);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(250, 24);
            this.txtUsername.TabIndex = 7;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPassword.Location = new System.Drawing.Point(30, 175);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(79, 18);
            this.lblPassword.TabIndex = 8;
            this.lblPassword.Text = "Password:";
            // 
            // txtPassword
            // 
            this.txtPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPassword.Location = new System.Drawing.Point(150, 172);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(250, 24);
            this.txtPassword.TabIndex = 9;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // chkShowPassword
            // 
            this.chkShowPassword.AutoSize = true;
            this.chkShowPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkShowPassword.Location = new System.Drawing.Point(420, 175);
            this.chkShowPassword.Name = "chkShowPassword";
            this.chkShowPassword.Size = new System.Drawing.Size(139, 22);
            this.chkShowPassword.TabIndex = 10;
            this.chkShowPassword.Text = "Show Password";
            this.chkShowPassword.UseVisualStyleBackColor = true;
            this.chkShowPassword.CheckedChanged += new System.EventHandler(this.chkShowPassword_CheckedChanged);
            // 
            // lblConnectionName
            // 
            this.lblConnectionName.AutoSize = true;
            this.lblConnectionName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConnectionName.Location = new System.Drawing.Point(30, 220);
            this.lblConnectionName.Name = "lblConnectionName";
            this.lblConnectionName.Size = new System.Drawing.Size(132, 18);
            this.lblConnectionName.TabIndex = 11;
            this.lblConnectionName.Text = "Connection Name:";
            // 
            // txtConnectionName
            // 
            this.txtConnectionName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConnectionName.Location = new System.Drawing.Point(150, 217);
            this.txtConnectionName.Name = "txtConnectionName";
            this.txtConnectionName.Size = new System.Drawing.Size(250, 24);
            this.txtConnectionName.TabIndex = 12;
            // 
            // chkUseSSHKey
            // 
            this.chkUseSSHKey.AutoSize = true;
            this.chkUseSSHKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkUseSSHKey.Location = new System.Drawing.Point(30, 265);
            this.chkUseSSHKey.Name = "chkUseSSHKey";
            this.chkUseSSHKey.Size = new System.Drawing.Size(217, 22);
            this.chkUseSSHKey.TabIndex = 13;
            this.chkUseSSHKey.Text = "Use SSH Key Authentication";
            this.chkUseSSHKey.UseVisualStyleBackColor = true;
            this.chkUseSSHKey.CheckedChanged += new System.EventHandler(this.chkUseSSHKey_CheckedChanged);
            // 
            // lblSSHKeyPath
            // 
            this.lblSSHKeyPath.AutoSize = true;
            this.lblSSHKeyPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSSHKeyPath.Location = new System.Drawing.Point(30, 300);
            this.lblSSHKeyPath.Name = "lblSSHKeyPath";
            this.lblSSHKeyPath.Size = new System.Drawing.Size(99, 18);
            this.lblSSHKeyPath.TabIndex = 14;
            this.lblSSHKeyPath.Text = "SSH Key File:";
            // 
            // txtSSHKeyPath
            // 
            this.txtSSHKeyPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSSHKeyPath.Location = new System.Drawing.Point(150, 297);
            this.txtSSHKeyPath.Name = "txtSSHKeyPath";
            this.txtSSHKeyPath.ReadOnly = true;
            this.txtSSHKeyPath.Size = new System.Drawing.Size(350, 24);
            this.txtSSHKeyPath.TabIndex = 15;
            // 
            // btnBrowseSSHKey
            // 
            this.btnBrowseSSHKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBrowseSSHKey.Location = new System.Drawing.Point(520, 296);
            this.btnBrowseSSHKey.Name = "btnBrowseSSHKey";
            this.btnBrowseSSHKey.Size = new System.Drawing.Size(90, 25);
            this.btnBrowseSSHKey.TabIndex = 16;
            this.btnBrowseSSHKey.Text = "Browse...";
            this.btnBrowseSSHKey.UseVisualStyleBackColor = true;
            this.btnBrowseSSHKey.Click += new System.EventHandler(this.btnBrowseSSHKey_Click);
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.BackColor = System.Drawing.Color.LightBlue;
            this.btnTestConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestConnection.Location = new System.Drawing.Point(30, 330);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(100, 35);
            this.btnTestConnection.TabIndex = 17;
            this.btnTestConnection.Text = "Test";
            this.btnTestConnection.UseVisualStyleBackColor = false;
            this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);
            // 
            // btnSaveConnection
            // 
            this.btnSaveConnection.BackColor = System.Drawing.Color.LightGreen;
            this.btnSaveConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveConnection.Location = new System.Drawing.Point(150, 330);
            this.btnSaveConnection.Name = "btnSaveConnection";
            this.btnSaveConnection.Size = new System.Drawing.Size(100, 35);
            this.btnSaveConnection.TabIndex = 18;
            this.btnSaveConnection.Text = "Save";
            this.btnSaveConnection.UseVisualStyleBackColor = false;
            this.btnSaveConnection.Click += new System.EventHandler(this.btnSaveConnection_Click);
            // 
            // btnLoadConnection
            // 
            this.btnLoadConnection.BackColor = System.Drawing.Color.LightYellow;
            this.btnLoadConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLoadConnection.Location = new System.Drawing.Point(270, 330);
            this.btnLoadConnection.Name = "btnLoadConnection";
            this.btnLoadConnection.Size = new System.Drawing.Size(100, 35);
            this.btnLoadConnection.TabIndex = 20;
            this.btnLoadConnection.Text = "Load Config";
            this.btnLoadConnection.UseVisualStyleBackColor = false;
            this.btnLoadConnection.Click += new System.EventHandler(this.btnLoadConnection_Click);
            // 
            // btnManageConnections
            // 
            this.btnManageConnections.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManageConnections.Location = new System.Drawing.Point(390, 330);
            this.btnManageConnections.Name = "btnManageConnections";
            this.btnManageConnections.Size = new System.Drawing.Size(100, 35);
            this.btnManageConnections.TabIndex = 19;
            this.btnManageConnections.Text = "Manage";
            this.btnManageConnections.UseVisualStyleBackColor = true;
            this.btnManageConnections.Click += new System.EventHandler(this.btnManageConnections_Click);
            // 
            // tabSSHKeyGeneration
            // 
            this.tabSSHKeyGeneration.Controls.Add(this.lblKeyPath);
            this.tabSSHKeyGeneration.Controls.Add(this.txtKeyPath);
            this.tabSSHKeyGeneration.Controls.Add(this.btnBrowseKey);
            this.tabSSHKeyGeneration.Controls.Add(this.lblTimeout);
            this.tabSSHKeyGeneration.Controls.Add(this.numTimeout);
            this.tabSSHKeyGeneration.Location = new System.Drawing.Point(4, 27);
            this.tabSSHKeyGeneration.Name = "tabSSHKeyGeneration";
            this.tabSSHKeyGeneration.Padding = new System.Windows.Forms.Padding(3);
            this.tabSSHKeyGeneration.Size = new System.Drawing.Size(852, 449);
            this.tabSSHKeyGeneration.TabIndex = 1;
            this.tabSSHKeyGeneration.Text = "SSH Key Generation";
            this.tabSSHKeyGeneration.UseVisualStyleBackColor = true;
            // 
            // lblKeyPath
            // 
            this.lblKeyPath.AutoSize = true;
            this.lblKeyPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblKeyPath.Location = new System.Drawing.Point(15, 30);
            this.lblKeyPath.Name = "lblKeyPath";
            this.lblKeyPath.Size = new System.Drawing.Size(71, 18);
            this.lblKeyPath.TabIndex = 0;
            this.lblKeyPath.Text = "Key Path:";
            // 
            // txtKeyPath
            // 
            this.txtKeyPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtKeyPath.Location = new System.Drawing.Point(100, 27);
            this.txtKeyPath.Name = "txtKeyPath";
            this.txtKeyPath.Size = new System.Drawing.Size(250, 24);
            this.txtKeyPath.TabIndex = 1;
            // 
            // btnBrowseKey
            // 
            this.btnBrowseKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnBrowseKey.Location = new System.Drawing.Point(360, 25);
            this.btnBrowseKey.Name = "btnBrowseKey";
            this.btnBrowseKey.Size = new System.Drawing.Size(60, 25);
            this.btnBrowseKey.TabIndex = 2;
            this.btnBrowseKey.Text = "Browse";
            this.btnBrowseKey.UseVisualStyleBackColor = true;
            this.btnBrowseKey.Click += new System.EventHandler(this.btnBrowseKey_Click);
            // 
            // lblTimeout
            // 
            this.lblTimeout.AutoSize = true;
            this.lblTimeout.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblTimeout.Location = new System.Drawing.Point(15, 70);
            this.lblTimeout.Name = "lblTimeout";
            this.lblTimeout.Size = new System.Drawing.Size(104, 18);
            this.lblTimeout.TabIndex = 3;
            this.lblTimeout.Text = "Timeout (sec):";
            // 
            // numTimeout
            // 
            this.numTimeout.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.numTimeout.Location = new System.Drawing.Point(100, 68);
            this.numTimeout.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numTimeout.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numTimeout.Name = "numTimeout";
            this.numTimeout.Size = new System.Drawing.Size(60, 24);
            this.numTimeout.TabIndex = 4;
            this.numTimeout.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.LightGreen;
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(616, 391);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 35);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.LightCoral;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Location = new System.Drawing.Point(739, 391);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FormConnection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 580);
            this.Controls.Add(this.tabControl);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormConnection";
            this.Padding = new System.Windows.Forms.Padding(15);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Connection Settings - Local";
            this.tabControl.ResumeLayout(false);
            this.tabConnectionSettings.ResumeLayout(false);
            this.tabConnectionSettings.PerformLayout();
            this.tabSSHKeyGeneration.ResumeLayout(false);
            this.tabSSHKeyGeneration.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
