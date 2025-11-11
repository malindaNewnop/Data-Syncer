namespace FTPSyncer.ui
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
        private System.Windows.Forms.Label lblKeyName;
        private System.Windows.Forms.TextBox txtKeyName;
        private System.Windows.Forms.Label lblSaveTo;
        private System.Windows.Forms.TextBox txtSaveTo;
        private System.Windows.Forms.Button btnBrowseSaveTo;
        private System.Windows.Forms.Label lblKeySize;
        private System.Windows.Forms.ComboBox cmbKeySize;
        private System.Windows.Forms.CheckBox chkProtectWithPassphrase;
        private System.Windows.Forms.Label lblPassphrase;
        private System.Windows.Forms.TextBox txtPassphrase;
        private System.Windows.Forms.Label lblConfirmPassphrase;
        private System.Windows.Forms.TextBox txtConfirmPassphrase;
        private System.Windows.Forms.Button btnGenerateSSHKey;
        private System.Windows.Forms.Label lblGeneratedPublicKey;
        private System.Windows.Forms.TextBox txtGeneratedPublicKey;
        private System.Windows.Forms.Button btnCopyToClipboard;

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
            this.tabSSHKeyGeneration = new System.Windows.Forms.TabPage();
            this.lblKeyName = new System.Windows.Forms.Label();
            this.txtKeyName = new System.Windows.Forms.TextBox();
            this.lblSaveTo = new System.Windows.Forms.Label();
            this.txtSaveTo = new System.Windows.Forms.TextBox();
            this.btnBrowseSaveTo = new System.Windows.Forms.Button();
            this.lblKeySize = new System.Windows.Forms.Label();
            this.cmbKeySize = new System.Windows.Forms.ComboBox();
            this.chkProtectWithPassphrase = new System.Windows.Forms.CheckBox();
            this.lblPassphrase = new System.Windows.Forms.Label();
            this.txtPassphrase = new System.Windows.Forms.TextBox();
            this.lblConfirmPassphrase = new System.Windows.Forms.Label();
            this.txtConfirmPassphrase = new System.Windows.Forms.TextBox();
            this.btnGenerateSSHKey = new System.Windows.Forms.Button();
            this.lblGeneratedPublicKey = new System.Windows.Forms.Label();
            this.txtGeneratedPublicKey = new System.Windows.Forms.TextBox();
            this.btnCopyToClipboard = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabConnectionSettings.SuspendLayout();
            this.tabSSHKeyGeneration.SuspendLayout();
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
            //            this.btnTestConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestConnection.Location = new System.Drawing.Point(30, 330);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(100, 35);
            this.btnTestConnection.TabIndex = 17;
            this.btnTestConnection.Text = "Test";
            this.btnTestConnection.UseVisualStyleBackColor = true;
            this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);
            // 
            // tabSSHKeyGeneration
            // 
            this.tabSSHKeyGeneration.Controls.Add(this.lblKeyName);
            this.tabSSHKeyGeneration.Controls.Add(this.txtKeyName);
            this.tabSSHKeyGeneration.Controls.Add(this.lblSaveTo);
            this.tabSSHKeyGeneration.Controls.Add(this.txtSaveTo);
            this.tabSSHKeyGeneration.Controls.Add(this.btnBrowseSaveTo);
            this.tabSSHKeyGeneration.Controls.Add(this.lblKeySize);
            this.tabSSHKeyGeneration.Controls.Add(this.cmbKeySize);
            this.tabSSHKeyGeneration.Controls.Add(this.chkProtectWithPassphrase);
            this.tabSSHKeyGeneration.Controls.Add(this.lblPassphrase);
            this.tabSSHKeyGeneration.Controls.Add(this.txtPassphrase);
            this.tabSSHKeyGeneration.Controls.Add(this.lblConfirmPassphrase);
            this.tabSSHKeyGeneration.Controls.Add(this.txtConfirmPassphrase);
            this.tabSSHKeyGeneration.Controls.Add(this.btnGenerateSSHKey);
            this.tabSSHKeyGeneration.Controls.Add(this.lblGeneratedPublicKey);
            this.tabSSHKeyGeneration.Controls.Add(this.txtGeneratedPublicKey);
            this.tabSSHKeyGeneration.Controls.Add(this.btnCopyToClipboard);
            this.tabSSHKeyGeneration.Location = new System.Drawing.Point(4, 27);
            this.tabSSHKeyGeneration.Name = "tabSSHKeyGeneration";
            this.tabSSHKeyGeneration.Padding = new System.Windows.Forms.Padding(3);
            this.tabSSHKeyGeneration.Size = new System.Drawing.Size(852, 449);
            this.tabSSHKeyGeneration.TabIndex = 1;
            this.tabSSHKeyGeneration.Text = "SSH Key Generation";
            this.tabSSHKeyGeneration.UseVisualStyleBackColor = true;
            // 
            // lblKeyName
            // 
            this.lblKeyName.AutoSize = true;
            this.lblKeyName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblKeyName.Location = new System.Drawing.Point(20, 30);
            this.lblKeyName.Name = "lblKeyName";
            this.lblKeyName.Size = new System.Drawing.Size(80, 18);
            this.lblKeyName.TabIndex = 0;
            this.lblKeyName.Text = "Key Name:";
            // 
            // txtKeyName
            // 
            this.txtKeyName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtKeyName.Location = new System.Drawing.Point(105, 27);
            this.txtKeyName.Name = "txtKeyName";
            this.txtKeyName.Size = new System.Drawing.Size(300, 24);
            this.txtKeyName.TabIndex = 1;
            this.txtKeyName.Text = "id_rsa_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            // 
            // lblSaveTo
            // 
            this.lblSaveTo.AutoSize = true;
            this.lblSaveTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblSaveTo.Location = new System.Drawing.Point(20, 70);
            this.lblSaveTo.Name = "lblSaveTo";
            this.lblSaveTo.Size = new System.Drawing.Size(68, 18);
            this.lblSaveTo.TabIndex = 2;
            this.lblSaveTo.Text = "Save To:";
            // 
            // txtSaveTo
            // 
            this.txtSaveTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtSaveTo.Location = new System.Drawing.Point(105, 67);
            this.txtSaveTo.Name = "txtSaveTo";
            this.txtSaveTo.Size = new System.Drawing.Size(450, 24);
            this.txtSaveTo.TabIndex = 3;
            // 
            // btnBrowseSaveTo
            // 
            this.btnBrowseSaveTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnBrowseSaveTo.Location = new System.Drawing.Point(565, 65);
            this.btnBrowseSaveTo.Name = "btnBrowseSaveTo";
            this.btnBrowseSaveTo.Size = new System.Drawing.Size(80, 28);
            this.btnBrowseSaveTo.TabIndex = 4;
            this.btnBrowseSaveTo.Text = "Browse";
            this.btnBrowseSaveTo.UseVisualStyleBackColor = true;
            this.btnBrowseSaveTo.Click += new System.EventHandler(this.btnBrowseSaveTo_Click);
            // 
            // lblKeySize
            // 
            this.lblKeySize.AutoSize = true;
            this.lblKeySize.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblKeySize.Location = new System.Drawing.Point(20, 110);
            this.lblKeySize.Name = "lblKeySize";
            this.lblKeySize.Size = new System.Drawing.Size(70, 18);
            this.lblKeySize.TabIndex = 5;
            this.lblKeySize.Text = "Key Size:";
            // 
            // cmbKeySize
            // 
            this.cmbKeySize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKeySize.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.cmbKeySize.FormattingEnabled = true;
            this.cmbKeySize.Items.AddRange(new object[] {
            "1024 bits",
            "2048 bits",
            "4096 bits"});
            this.cmbKeySize.Location = new System.Drawing.Point(105, 107);
            this.cmbKeySize.Name = "cmbKeySize";
            this.cmbKeySize.Size = new System.Drawing.Size(120, 26);
            this.cmbKeySize.TabIndex = 6;
            this.cmbKeySize.SelectedIndex = 1;
            // 
            // chkProtectWithPassphrase
            // 
            this.chkProtectWithPassphrase.AutoSize = true;
            this.chkProtectWithPassphrase.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkProtectWithPassphrase.Location = new System.Drawing.Point(23, 155);
            this.chkProtectWithPassphrase.Name = "chkProtectWithPassphrase";
            this.chkProtectWithPassphrase.Size = new System.Drawing.Size(200, 22);
            this.chkProtectWithPassphrase.TabIndex = 7;
            this.chkProtectWithPassphrase.Text = "Protect key with passphrase";
            this.chkProtectWithPassphrase.UseVisualStyleBackColor = true;
            this.chkProtectWithPassphrase.CheckedChanged += new System.EventHandler(this.chkProtectWithPassphrase_CheckedChanged);
            // 
            // lblPassphrase
            // 
            this.lblPassphrase.AutoSize = true;
            this.lblPassphrase.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblPassphrase.Location = new System.Drawing.Point(20, 195);
            this.lblPassphrase.Name = "lblPassphrase";
            this.lblPassphrase.Size = new System.Drawing.Size(88, 18);
            this.lblPassphrase.TabIndex = 8;
            this.lblPassphrase.Text = "Passphrase:";
            // 
            // txtPassphrase
            // 
            this.txtPassphrase.Enabled = false;
            this.txtPassphrase.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtPassphrase.Location = new System.Drawing.Point(115, 192);
            this.txtPassphrase.Name = "txtPassphrase";
            this.txtPassphrase.PasswordChar = '*';
            this.txtPassphrase.Size = new System.Drawing.Size(200, 24);
            this.txtPassphrase.TabIndex = 9;
            // 
            // lblConfirmPassphrase
            // 
            this.lblConfirmPassphrase.AutoSize = true;
            this.lblConfirmPassphrase.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblConfirmPassphrase.Location = new System.Drawing.Point(335, 195);
            this.lblConfirmPassphrase.Name = "lblConfirmPassphrase";
            this.lblConfirmPassphrase.Size = new System.Drawing.Size(65, 18);
            this.lblConfirmPassphrase.TabIndex = 10;
            this.lblConfirmPassphrase.Text = "Confirm:";
            // 
            // txtConfirmPassphrase
            // 
            this.txtConfirmPassphrase.Enabled = false;
            this.txtConfirmPassphrase.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtConfirmPassphrase.Location = new System.Drawing.Point(405, 192);
            this.txtConfirmPassphrase.Name = "txtConfirmPassphrase";
            this.txtConfirmPassphrase.PasswordChar = '*';
            this.txtConfirmPassphrase.Size = new System.Drawing.Size(200, 24);
            this.txtConfirmPassphrase.TabIndex = 11;
            // 
            // btnGenerateSSHKey
            // 
            this.btnGenerateSSHKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnGenerateSSHKey.Location = new System.Drawing.Point(23, 240);
            this.btnGenerateSSHKey.Name = "btnGenerateSSHKey";
            this.btnGenerateSSHKey.Size = new System.Drawing.Size(160, 35);
            this.btnGenerateSSHKey.TabIndex = 12;
            this.btnGenerateSSHKey.Text = "Generate SSH Key";
            this.btnGenerateSSHKey.UseVisualStyleBackColor = true;
            this.btnGenerateSSHKey.Click += new System.EventHandler(this.btnGenerateSSHKey_Click);
            // 
            // lblGeneratedPublicKey
            // 
            this.lblGeneratedPublicKey.AutoSize = true;
            this.lblGeneratedPublicKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblGeneratedPublicKey.Location = new System.Drawing.Point(20, 295);
            this.lblGeneratedPublicKey.Name = "lblGeneratedPublicKey";
            this.lblGeneratedPublicKey.Size = new System.Drawing.Size(176, 18);
            this.lblGeneratedPublicKey.TabIndex = 13;
            this.lblGeneratedPublicKey.Text = "Generated Public Key:";
            // 
            // txtGeneratedPublicKey
            // 
            this.txtGeneratedPublicKey.Font = new System.Drawing.Font("Consolas", 8F);
            this.txtGeneratedPublicKey.Location = new System.Drawing.Point(23, 320);
            this.txtGeneratedPublicKey.Multiline = true;
            this.txtGeneratedPublicKey.Name = "txtGeneratedPublicKey";
            this.txtGeneratedPublicKey.ReadOnly = true;
            this.txtGeneratedPublicKey.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtGeneratedPublicKey.Size = new System.Drawing.Size(770, 80);
            this.txtGeneratedPublicKey.TabIndex = 14;
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnCopyToClipboard.Location = new System.Drawing.Point(23, 410);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(150, 30);
            this.btnCopyToClipboard.TabIndex = 15;
            this.btnCopyToClipboard.Text = "Copy to Clipboard";
            this.btnCopyToClipboard.UseVisualStyleBackColor = true;
            this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
            // 
            // btnSave
            //            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(616, 391);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 35);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            //            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Location = new System.Drawing.Point(739, 391);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
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
            this.ResumeLayout(false);

        }
    }
}





