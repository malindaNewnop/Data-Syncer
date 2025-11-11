namespace FTPSyncer.ui.Forms
{
    partial class FormNotificationSettings
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
            this.groupBoxNotifications = new System.Windows.Forms.GroupBox();
            this.checkBoxEnableNotifications = new System.Windows.Forms.CheckBox();
            this.checkBoxShowConnectionNotifications = new System.Windows.Forms.CheckBox();
            this.checkBoxShowJobStartNotifications = new System.Windows.Forms.CheckBox();
            this.checkBoxShowJobCompleteNotifications = new System.Windows.Forms.CheckBox();
            this.checkBoxShowErrorNotifications = new System.Windows.Forms.CheckBox();
            this.checkBoxShowWarningNotifications = new System.Windows.Forms.CheckBox();
            this.checkBoxPlaySound = new System.Windows.Forms.CheckBox();
            
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonRestoreDefaults = new System.Windows.Forms.Button();
            
            this.groupBoxNotifications.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // groupBoxNotifications
            // 
            this.groupBoxNotifications.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNotifications.Controls.Add(this.checkBoxEnableNotifications);
            this.groupBoxNotifications.Controls.Add(this.checkBoxShowConnectionNotifications);
            this.groupBoxNotifications.Controls.Add(this.checkBoxShowJobStartNotifications);
            this.groupBoxNotifications.Controls.Add(this.checkBoxShowJobCompleteNotifications);
            this.groupBoxNotifications.Controls.Add(this.checkBoxShowErrorNotifications);
            this.groupBoxNotifications.Controls.Add(this.checkBoxShowWarningNotifications);
            this.groupBoxNotifications.Controls.Add(this.checkBoxPlaySound);
            this.groupBoxNotifications.Location = new System.Drawing.Point(12, 12);
            this.groupBoxNotifications.Name = "groupBoxNotifications";
            this.groupBoxNotifications.Size = new System.Drawing.Size(460, 240);
            this.groupBoxNotifications.TabIndex = 0;
            this.groupBoxNotifications.TabStop = false;
            this.groupBoxNotifications.Text = "Notification Settings";
            
            // 
            // checkBoxEnableNotifications
            // 
            this.checkBoxEnableNotifications.AutoSize = true;
            this.checkBoxEnableNotifications.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxEnableNotifications.Location = new System.Drawing.Point(20, 30);
            this.checkBoxEnableNotifications.Name = "checkBoxEnableNotifications";
            this.checkBoxEnableNotifications.Size = new System.Drawing.Size(125, 17);
            this.checkBoxEnableNotifications.TabIndex = 0;
            this.checkBoxEnableNotifications.Text = "Enable Notifications";
            this.checkBoxEnableNotifications.UseVisualStyleBackColor = true;
            this.checkBoxEnableNotifications.CheckedChanged += new System.EventHandler(this.checkBoxEnableNotifications_CheckedChanged);
            
            // 
            // checkBoxShowConnectionNotifications
            // 
            this.checkBoxShowConnectionNotifications.AutoSize = true;
            this.checkBoxShowConnectionNotifications.Location = new System.Drawing.Point(40, 60);
            this.checkBoxShowConnectionNotifications.Name = "checkBoxShowConnectionNotifications";
            this.checkBoxShowConnectionNotifications.Size = new System.Drawing.Size(170, 17);
            this.checkBoxShowConnectionNotifications.TabIndex = 1;
            this.checkBoxShowConnectionNotifications.Text = "Show Connection Notifications";
            this.checkBoxShowConnectionNotifications.UseVisualStyleBackColor = true;
            
            // 
            // checkBoxShowJobStartNotifications
            // 
            this.checkBoxShowJobStartNotifications.AutoSize = true;
            this.checkBoxShowJobStartNotifications.Location = new System.Drawing.Point(40, 90);
            this.checkBoxShowJobStartNotifications.Name = "checkBoxShowJobStartNotifications";
            this.checkBoxShowJobStartNotifications.Size = new System.Drawing.Size(152, 17);
            this.checkBoxShowJobStartNotifications.TabIndex = 2;
            this.checkBoxShowJobStartNotifications.Text = "Show Job Start Notifications";
            this.checkBoxShowJobStartNotifications.UseVisualStyleBackColor = true;
            
            // 
            // checkBoxShowJobCompleteNotifications
            // 
            this.checkBoxShowJobCompleteNotifications.AutoSize = true;
            this.checkBoxShowJobCompleteNotifications.Location = new System.Drawing.Point(40, 120);
            this.checkBoxShowJobCompleteNotifications.Name = "checkBoxShowJobCompleteNotifications";
            this.checkBoxShowJobCompleteNotifications.Size = new System.Drawing.Size(178, 17);
            this.checkBoxShowJobCompleteNotifications.TabIndex = 3;
            this.checkBoxShowJobCompleteNotifications.Text = "Show Job Complete Notifications";
            this.checkBoxShowJobCompleteNotifications.UseVisualStyleBackColor = true;
            
            // 
            // checkBoxShowErrorNotifications
            // 
            this.checkBoxShowErrorNotifications.AutoSize = true;
            this.checkBoxShowErrorNotifications.Location = new System.Drawing.Point(40, 150);
            this.checkBoxShowErrorNotifications.Name = "checkBoxShowErrorNotifications";
            this.checkBoxShowErrorNotifications.Size = new System.Drawing.Size(139, 17);
            this.checkBoxShowErrorNotifications.TabIndex = 4;
            this.checkBoxShowErrorNotifications.Text = "Show Error Notifications";
            this.checkBoxShowErrorNotifications.UseVisualStyleBackColor = true;
            
            // 
            // checkBoxShowWarningNotifications
            // 
            this.checkBoxShowWarningNotifications.AutoSize = true;
            this.checkBoxShowWarningNotifications.Location = new System.Drawing.Point(40, 180);
            this.checkBoxShowWarningNotifications.Name = "checkBoxShowWarningNotifications";
            this.checkBoxShowWarningNotifications.Size = new System.Drawing.Size(155, 17);
            this.checkBoxShowWarningNotifications.TabIndex = 5;
            this.checkBoxShowWarningNotifications.Text = "Show Warning Notifications";
            this.checkBoxShowWarningNotifications.UseVisualStyleBackColor = true;
            
            // 
            // checkBoxPlaySound
            // 
            this.checkBoxPlaySound.AutoSize = true;
            this.checkBoxPlaySound.Location = new System.Drawing.Point(40, 210);
            this.checkBoxPlaySound.Name = "checkBoxPlaySound";
            this.checkBoxPlaySound.Size = new System.Drawing.Size(156, 17);
            this.checkBoxPlaySound.TabIndex = 6;
            this.checkBoxPlaySound.Text = "Play Sound with Notifications";
            this.checkBoxPlaySound.UseVisualStyleBackColor = true;
            
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(236, 268);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 7;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(397, 268);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApply.Location = new System.Drawing.Point(317, 268);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 23);
            this.buttonApply.TabIndex = 8;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            
            // 
            // buttonRestoreDefaults
            // 
            this.buttonRestoreDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRestoreDefaults.Location = new System.Drawing.Point(12, 268);
            this.buttonRestoreDefaults.Name = "buttonRestoreDefaults";
            this.buttonRestoreDefaults.Size = new System.Drawing.Size(110, 23);
            this.buttonRestoreDefaults.TabIndex = 10;
            this.buttonRestoreDefaults.Text = "Restore Defaults";
            this.buttonRestoreDefaults.UseVisualStyleBackColor = true;
            this.buttonRestoreDefaults.Click += new System.EventHandler(this.buttonRestoreDefaults_Click);
            
            // 
            // FormNotificationSettings
            // 
            this.AcceptButton = this.buttonSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(484, 303);
            this.Controls.Add(this.buttonRestoreDefaults);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.groupBoxNotifications);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormNotificationSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Notification Settings";
            this.groupBoxNotifications.ResumeLayout(false);
            this.groupBoxNotifications.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxNotifications;
        private System.Windows.Forms.CheckBox checkBoxEnableNotifications;
        private System.Windows.Forms.CheckBox checkBoxShowConnectionNotifications;
        private System.Windows.Forms.CheckBox checkBoxShowJobStartNotifications;
        private System.Windows.Forms.CheckBox checkBoxShowJobCompleteNotifications;
        private System.Windows.Forms.CheckBox checkBoxShowErrorNotifications;
        private System.Windows.Forms.CheckBox checkBoxShowWarningNotifications;
        private System.Windows.Forms.CheckBox checkBoxPlaySound;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonRestoreDefaults;
    }
}
