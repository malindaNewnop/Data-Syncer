namespace FTPSyncer.ui.Forms
{
    partial class FormTraySettings
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
            this.checkBoxNotificationsEnabled = new System.Windows.Forms.CheckBox();
            this.checkBoxStartupNotification = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoStart = new System.Windows.Forms.CheckBox();
            this.labelNotificationDelay = new System.Windows.Forms.Label();
            this.numericUpDownDelay = new System.Windows.Forms.NumericUpDown();
            this.labelSeconds = new System.Windows.Forms.Label();
            this.groupBoxTrayBehavior = new System.Windows.Forms.GroupBox();
            this.checkBoxMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.checkBoxStartMinimized = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxNotifications.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).BeginInit();
            this.groupBoxTrayBehavior.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxNotifications
            // 
            this.groupBoxNotifications.Controls.Add(this.labelSeconds);
            this.groupBoxNotifications.Controls.Add(this.numericUpDownDelay);
            this.groupBoxNotifications.Controls.Add(this.labelNotificationDelay);
            this.groupBoxNotifications.Controls.Add(this.checkBoxStartupNotification);
            this.groupBoxNotifications.Controls.Add(this.checkBoxNotificationsEnabled);
            this.groupBoxNotifications.Location = new System.Drawing.Point(12, 12);
            this.groupBoxNotifications.Name = "groupBoxNotifications";
            this.groupBoxNotifications.Size = new System.Drawing.Size(360, 120);
            this.groupBoxNotifications.TabIndex = 0;
            this.groupBoxNotifications.TabStop = false;
            this.groupBoxNotifications.Text = "Notifications";
            // 
            // checkBoxNotificationsEnabled
            // 
            this.checkBoxNotificationsEnabled.AutoSize = true;
            this.checkBoxNotificationsEnabled.Location = new System.Drawing.Point(20, 25);
            this.checkBoxNotificationsEnabled.Name = "checkBoxNotificationsEnabled";
            this.checkBoxNotificationsEnabled.Size = new System.Drawing.Size(128, 17);
            this.checkBoxNotificationsEnabled.TabIndex = 0;
            this.checkBoxNotificationsEnabled.Text = "Enable notifications";
            this.checkBoxNotificationsEnabled.UseVisualStyleBackColor = true;
            this.checkBoxNotificationsEnabled.CheckedChanged += new System.EventHandler(this.checkBoxNotificationsEnabled_CheckedChanged);
            // 
            // checkBoxStartupNotification
            // 
            this.checkBoxStartupNotification.AutoSize = true;
            this.checkBoxStartupNotification.Location = new System.Drawing.Point(20, 48);
            this.checkBoxStartupNotification.Name = "checkBoxStartupNotification";
            this.checkBoxStartupNotification.Size = new System.Drawing.Size(155, 17);
            this.checkBoxStartupNotification.TabIndex = 1;
            this.checkBoxStartupNotification.Text = "Show startup notification";
            this.checkBoxStartupNotification.UseVisualStyleBackColor = true;
            // 
            // labelNotificationDelay
            // 
            this.labelNotificationDelay.AutoSize = true;
            this.labelNotificationDelay.Location = new System.Drawing.Point(20, 78);
            this.labelNotificationDelay.Name = "labelNotificationDelay";
            this.labelNotificationDelay.Size = new System.Drawing.Size(101, 13);
            this.labelNotificationDelay.TabIndex = 2;
            this.labelNotificationDelay.Text = "Notification duration:";
            // 
            // numericUpDownDelay
            // 
            this.numericUpDownDelay.Location = new System.Drawing.Point(127, 76);
            this.numericUpDownDelay.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownDelay.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownDelay.Name = "numericUpDownDelay";
            this.numericUpDownDelay.Size = new System.Drawing.Size(60, 20);
            this.numericUpDownDelay.TabIndex = 3;
            this.numericUpDownDelay.Value = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            // 
            // labelSeconds
            // 
            this.labelSeconds.AutoSize = true;
            this.labelSeconds.Location = new System.Drawing.Point(193, 78);
            this.labelSeconds.Name = "labelSeconds";
            this.labelSeconds.Size = new System.Drawing.Size(20, 13);
            this.labelSeconds.TabIndex = 4;
            this.labelSeconds.Text = "ms";
            // 
            // groupBoxTrayBehavior
            // 
            this.groupBoxTrayBehavior.Controls.Add(this.checkBoxAutoStart);
            this.groupBoxTrayBehavior.Controls.Add(this.checkBoxStartMinimized);
            this.groupBoxTrayBehavior.Controls.Add(this.checkBoxMinimizeToTray);
            this.groupBoxTrayBehavior.Location = new System.Drawing.Point(12, 150);
            this.groupBoxTrayBehavior.Name = "groupBoxTrayBehavior";
            this.groupBoxTrayBehavior.Size = new System.Drawing.Size(360, 105);
            this.groupBoxTrayBehavior.TabIndex = 1;
            this.groupBoxTrayBehavior.TabStop = false;
            this.groupBoxTrayBehavior.Text = "Tray Behavior";
            // 
            // checkBoxMinimizeToTray
            // 
            this.checkBoxMinimizeToTray.AutoSize = true;
            this.checkBoxMinimizeToTray.Location = new System.Drawing.Point(20, 25);
            this.checkBoxMinimizeToTray.Name = "checkBoxMinimizeToTray";
            this.checkBoxMinimizeToTray.Size = new System.Drawing.Size(183, 17);
            this.checkBoxMinimizeToTray.TabIndex = 0;
            this.checkBoxMinimizeToTray.Text = "Minimize to tray when window closed";
            this.checkBoxMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // checkBoxStartMinimized
            // 
            this.checkBoxStartMinimized.AutoSize = true;
            this.checkBoxStartMinimized.Location = new System.Drawing.Point(20, 48);
            this.checkBoxStartMinimized.Name = "checkBoxStartMinimized";
            this.checkBoxStartMinimized.Size = new System.Drawing.Size(137, 17);
            this.checkBoxStartMinimized.TabIndex = 1;
            this.checkBoxStartMinimized.Text = "Start minimized to tray";
            this.checkBoxStartMinimized.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoStart
            // 
            this.checkBoxAutoStart.AutoSize = true;
            this.checkBoxAutoStart.Location = new System.Drawing.Point(20, 71);
            this.checkBoxAutoStart.Name = "checkBoxAutoStart";
            this.checkBoxAutoStart.Size = new System.Drawing.Size(158, 17);
            this.checkBoxAutoStart.TabIndex = 2;
            this.checkBoxAutoStart.Text = "Auto-start with Windows";
            this.checkBoxAutoStart.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(216, 275);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(297, 275);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // FormTraySettings
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(384, 310);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxTrayBehavior);
            this.Controls.Add(this.groupBoxNotifications);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormTraySettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tray Settings";
            this.groupBoxNotifications.ResumeLayout(false);
            this.groupBoxNotifications.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).EndInit();
            this.groupBoxTrayBehavior.ResumeLayout(false);
            this.groupBoxTrayBehavior.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxNotifications;
        private System.Windows.Forms.CheckBox checkBoxNotificationsEnabled;
        private System.Windows.Forms.CheckBox checkBoxStartupNotification;
        private System.Windows.Forms.Label labelNotificationDelay;
        private System.Windows.Forms.NumericUpDown numericUpDownDelay;
        private System.Windows.Forms.Label labelSeconds;
        private System.Windows.Forms.GroupBox groupBoxTrayBehavior;
        private System.Windows.Forms.CheckBox checkBoxMinimizeToTray;
        private System.Windows.Forms.CheckBox checkBoxStartMinimized;
        private System.Windows.Forms.CheckBox checkBoxAutoStart;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}





