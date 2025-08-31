namespace syncer.ui.Forms
{
    partial class FormKeyGeneration
    {
        private System.ComponentModel.IContainer components = null;
        
        private System.Windows.Forms.GroupBox gbKeySettings;
        private System.Windows.Forms.Label lblKeyName;
        private System.Windows.Forms.TextBox txtKeyName;
        private System.Windows.Forms.Label lblKeyPath;
        private System.Windows.Forms.TextBox txtKeyPath;
        private System.Windows.Forms.Button btnBrowsePath;
        private System.Windows.Forms.Label lblKeySize;
        private System.Windows.Forms.ComboBox cmbKeySize;
        
        private System.Windows.Forms.GroupBox gbPassphrase;
        private System.Windows.Forms.CheckBox chkUsePassphrase;
        private System.Windows.Forms.Label lblPassphrase;
        private System.Windows.Forms.TextBox txtPassphrase;
        private System.Windows.Forms.Label lblConfirmPassphrase;
        private System.Windows.Forms.TextBox txtConfirmPassphrase;
        
        private System.Windows.Forms.GroupBox gbGeneration;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Button btnCopyPublicKey;
        
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
            this.gbKeySettings = new System.Windows.Forms.GroupBox();
            this.lblKeyName = new System.Windows.Forms.Label();
            this.txtKeyName = new System.Windows.Forms.TextBox();
            this.lblKeyPath = new System.Windows.Forms.Label();
            this.txtKeyPath = new System.Windows.Forms.TextBox();
            this.btnBrowsePath = new System.Windows.Forms.Button();
            this.lblKeySize = new System.Windows.Forms.Label();
            this.cmbKeySize = new System.Windows.Forms.ComboBox();
            
            this.gbPassphrase = new System.Windows.Forms.GroupBox();
            this.chkUsePassphrase = new System.Windows.Forms.CheckBox();
            this.lblPassphrase = new System.Windows.Forms.Label();
            this.txtPassphrase = new System.Windows.Forms.TextBox();
            this.lblConfirmPassphrase = new System.Windows.Forms.Label();
            this.txtConfirmPassphrase = new System.Windows.Forms.TextBox();
            
            this.gbGeneration = new System.Windows.Forms.GroupBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.btnCopyPublicKey = new System.Windows.Forms.Button();
            
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            
            this.gbKeySettings.SuspendLayout();
            this.gbPassphrase.SuspendLayout();
            this.gbGeneration.SuspendLayout();
            this.SuspendLayout();
            
            // gbKeySettings
            this.gbKeySettings.Controls.Add(this.lblKeyName);
            this.gbKeySettings.Controls.Add(this.txtKeyName);
            this.gbKeySettings.Controls.Add(this.lblKeyPath);
            this.gbKeySettings.Controls.Add(this.txtKeyPath);
            this.gbKeySettings.Controls.Add(this.btnBrowsePath);
            this.gbKeySettings.Controls.Add(this.lblKeySize);
            this.gbKeySettings.Controls.Add(this.cmbKeySize);
            this.gbKeySettings.Location = new System.Drawing.Point(12, 12);
            this.gbKeySettings.Name = "gbKeySettings";
            this.gbKeySettings.Size = new System.Drawing.Size(460, 110);
            this.gbKeySettings.TabIndex = 0;
            this.gbKeySettings.TabStop = false;
            this.gbKeySettings.Text = "Key Settings";
            
            // lblKeyName
            this.lblKeyName.AutoSize = true;
            this.lblKeyName.Location = new System.Drawing.Point(15, 25);
            this.lblKeyName.Name = "lblKeyName";
            this.lblKeyName.Size = new System.Drawing.Size(59, 13);
            this.lblKeyName.Text = "Key Name:";
            
            // txtKeyName
            this.txtKeyName.Location = new System.Drawing.Point(100, 22);
            this.txtKeyName.Name = "txtKeyName";
            this.txtKeyName.Size = new System.Drawing.Size(200, 20);
            this.txtKeyName.TabIndex = 1;
            
            // lblKeyPath
            this.lblKeyPath.AutoSize = true;
            this.lblKeyPath.Location = new System.Drawing.Point(15, 55);
            this.lblKeyPath.Name = "lblKeyPath";
            this.lblKeyPath.Size = new System.Drawing.Size(77, 13);
            this.lblKeyPath.Text = "Save Location:";
            
            // txtKeyPath
            this.txtKeyPath.Location = new System.Drawing.Point(100, 52);
            this.txtKeyPath.Name = "txtKeyPath";
            this.txtKeyPath.Size = new System.Drawing.Size(280, 20);
            this.txtKeyPath.TabIndex = 2;
            
            // btnBrowsePath
            this.btnBrowsePath.Location = new System.Drawing.Point(390, 50);
            this.btnBrowsePath.Name = "btnBrowsePath";
            this.btnBrowsePath.Size = new System.Drawing.Size(60, 23);
            this.btnBrowsePath.TabIndex = 3;
            this.btnBrowsePath.Text = "Browse";
            this.btnBrowsePath.UseVisualStyleBackColor = true;
            this.btnBrowsePath.Click += new System.EventHandler(this.btnBrowsePath_Click);
            
            // lblKeySize
            this.lblKeySize.AutoSize = true;
            this.lblKeySize.Location = new System.Drawing.Point(15, 85);
            this.lblKeySize.Name = "lblKeySize";
            this.lblKeySize.Size = new System.Drawing.Size(53, 13);
            this.lblKeySize.Text = "Key Size:";
            
            // cmbKeySize
            this.cmbKeySize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKeySize.Items.AddRange(new object[] { "1024 bits", "2048 bits", "3072 bits", "4096 bits" });
            this.cmbKeySize.Location = new System.Drawing.Point(100, 82);
            this.cmbKeySize.Name = "cmbKeySize";
            this.cmbKeySize.Size = new System.Drawing.Size(120, 21);
            this.cmbKeySize.TabIndex = 4;
            
            // gbPassphrase
            this.gbPassphrase.Controls.Add(this.chkUsePassphrase);
            this.gbPassphrase.Controls.Add(this.lblPassphrase);
            this.gbPassphrase.Controls.Add(this.txtPassphrase);
            this.gbPassphrase.Controls.Add(this.lblConfirmPassphrase);
            this.gbPassphrase.Controls.Add(this.txtConfirmPassphrase);
            this.gbPassphrase.Location = new System.Drawing.Point(12, 130);
            this.gbPassphrase.Name = "gbPassphrase";
            this.gbPassphrase.Size = new System.Drawing.Size(460, 90);
            this.gbPassphrase.TabIndex = 1;
            this.gbPassphrase.TabStop = false;
            this.gbPassphrase.Text = "Passphrase Protection";
            
            // chkUsePassphrase
            this.chkUsePassphrase.AutoSize = true;
            this.chkUsePassphrase.Location = new System.Drawing.Point(15, 25);
            this.chkUsePassphrase.Name = "chkUsePassphrase";
            this.chkUsePassphrase.Size = new System.Drawing.Size(162, 17);
            this.chkUsePassphrase.Text = "Protect key with passphrase";
            this.chkUsePassphrase.UseVisualStyleBackColor = true;
            this.chkUsePassphrase.CheckedChanged += new System.EventHandler(this.chkUsePassphrase_CheckedChanged);
            
            // lblPassphrase
            this.lblPassphrase.AutoSize = true;
            this.lblPassphrase.Location = new System.Drawing.Point(15, 55);
            this.lblPassphrase.Name = "lblPassphrase";
            this.lblPassphrase.Size = new System.Drawing.Size(65, 13);
            this.lblPassphrase.Text = "Passphrase:";
            
            // txtPassphrase
            this.txtPassphrase.Location = new System.Drawing.Point(100, 52);
            this.txtPassphrase.Name = "txtPassphrase";
            this.txtPassphrase.Size = new System.Drawing.Size(150, 20);
            this.txtPassphrase.TabIndex = 2;
            this.txtPassphrase.UseSystemPasswordChar = true;
            
            // lblConfirmPassphrase
            this.lblConfirmPassphrase.AutoSize = true;
            this.lblConfirmPassphrase.Location = new System.Drawing.Point(270, 55);
            this.lblConfirmPassphrase.Name = "lblConfirmPassphrase";
            this.lblConfirmPassphrase.Size = new System.Drawing.Size(45, 13);
            this.lblConfirmPassphrase.Text = "Confirm:";
            
            // txtConfirmPassphrase
            this.txtConfirmPassphrase.Location = new System.Drawing.Point(320, 52);
            this.txtConfirmPassphrase.Name = "txtConfirmPassphrase";
            this.txtConfirmPassphrase.Size = new System.Drawing.Size(130, 20);
            this.txtConfirmPassphrase.TabIndex = 3;
            this.txtConfirmPassphrase.UseSystemPasswordChar = true;
            
            // gbGeneration
            this.gbGeneration.Controls.Add(this.btnGenerate);
            this.gbGeneration.Controls.Add(this.progressBar);
            this.gbGeneration.Controls.Add(this.txtOutput);
            this.gbGeneration.Controls.Add(this.btnCopyPublicKey);
            this.gbGeneration.Location = new System.Drawing.Point(12, 230);
            this.gbGeneration.Name = "gbGeneration";
            this.gbGeneration.Size = new System.Drawing.Size(460, 180);
            this.gbGeneration.TabIndex = 2;
            this.gbGeneration.TabStop = false;
            this.gbGeneration.Text = "Key Generation";
            
            // btnGenerate            this.btnGenerate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnGenerate.Location = new System.Drawing.Point(15, 25);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(120, 30);
            this.btnGenerate.TabIndex = 1;
            this.btnGenerate.Text = "Generate Key Pair";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            
            // progressBar
            this.progressBar.Location = new System.Drawing.Point(145, 30);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(200, 20);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 2;
            this.progressBar.Visible = false;
            
            // btnCopyPublicKey            this.btnCopyPublicKey.Location = new System.Drawing.Point(355, 25);
            this.btnCopyPublicKey.Name = "btnCopyPublicKey";
            this.btnCopyPublicKey.Size = new System.Drawing.Size(95, 30);
            this.btnCopyPublicKey.TabIndex = 3;
            this.btnCopyPublicKey.Text = "Copy Public Key";
            this.btnCopyPublicKey.UseVisualStyleBackColor = true;
            this.btnCopyPublicKey.Click += new System.EventHandler(this.btnCopyPublicKey_Click);
            
            // txtOutput
            this.txtOutput.Location = new System.Drawing.Point(15, 65);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(435, 105);
            this.txtOutput.TabIndex = 4;
            
            // btnOK            this.btnOK.Enabled = false;
            this.btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnOK.Location = new System.Drawing.Point(300, 420);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 30);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            
            // btnCancel            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Location = new System.Drawing.Point(390, 420);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            
            // FormKeyGeneration
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 462);
            this.Controls.Add(this.gbKeySettings);
            this.Controls.Add(this.gbPassphrase);
            this.Controls.Add(this.gbGeneration);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Name = "FormKeyGeneration";
            this.Text = "SSH Key Generation";
            
            this.gbKeySettings.ResumeLayout(false);
            this.gbKeySettings.PerformLayout();
            this.gbPassphrase.ResumeLayout(false);
            this.gbPassphrase.PerformLayout();
            this.gbGeneration.ResumeLayout(false);
            this.gbGeneration.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
