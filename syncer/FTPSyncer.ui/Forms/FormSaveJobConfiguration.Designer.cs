namespace FTPSyncer.ui.Forms
{
    partial class FormSaveJobConfiguration
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
            this.groupBoxBasicInfo = new System.Windows.Forms.GroupBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.labelName = new System.Windows.Forms.Label();
            this.textBoxDescription = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.comboBoxCategory = new System.Windows.Forms.ComboBox();
            this.labelCategory = new System.Windows.Forms.Label();
            this.textBoxTags = new System.Windows.Forms.TextBox();
            this.labelTags = new System.Windows.Forms.Label();
            
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.checkBoxSetAsDefault = new System.Windows.Forms.CheckBox();
            this.checkBoxAddToQuickLaunch = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoStartOnLoad = new System.Windows.Forms.CheckBox();
            this.checkBoxShowNotificationOnStart = new System.Windows.Forms.CheckBox();
            
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.listViewPreview = new System.Windows.Forms.ListView();
            
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonTestConfig = new System.Windows.Forms.Button();
            
            this.groupBoxBasicInfo.SuspendLayout();
            this.groupBoxOptions.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // groupBoxBasicInfo
            // 
            this.groupBoxBasicInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBasicInfo.Controls.Add(this.labelName);
            this.groupBoxBasicInfo.Controls.Add(this.textBoxName);
            this.groupBoxBasicInfo.Controls.Add(this.labelDescription);
            this.groupBoxBasicInfo.Controls.Add(this.textBoxDescription);
            this.groupBoxBasicInfo.Controls.Add(this.labelCategory);
            this.groupBoxBasicInfo.Controls.Add(this.comboBoxCategory);
            this.groupBoxBasicInfo.Controls.Add(this.labelTags);
            this.groupBoxBasicInfo.Controls.Add(this.textBoxTags);
            this.groupBoxBasicInfo.Location = new System.Drawing.Point(12, 12);
            this.groupBoxBasicInfo.Name = "groupBoxBasicInfo";
            this.groupBoxBasicInfo.Size = new System.Drawing.Size(560, 160);
            this.groupBoxBasicInfo.TabIndex = 0;
            this.groupBoxBasicInfo.TabStop = false;
            this.groupBoxBasicInfo.Text = "Basic Information";
            
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(15, 25);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(35, 13);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Name:";
            
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(100, 22);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(440, 20);
            this.textBoxName.TabIndex = 1;
            
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(15, 55);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(63, 13);
            this.labelDescription.TabIndex = 2;
            this.labelDescription.Text = "Description:";
            
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDescription.Location = new System.Drawing.Point(100, 52);
            this.textBoxDescription.Multiline = true;
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(440, 40);
            this.textBoxDescription.TabIndex = 3;
            
            // 
            // labelCategory
            // 
            this.labelCategory.AutoSize = true;
            this.labelCategory.Location = new System.Drawing.Point(15, 105);
            this.labelCategory.Name = "labelCategory";
            this.labelCategory.Size = new System.Drawing.Size(52, 13);
            this.labelCategory.TabIndex = 4;
            this.labelCategory.Text = "Category:";
            
            // 
            // comboBoxCategory
            // 
            this.comboBoxCategory.FormattingEnabled = true;
            this.comboBoxCategory.Items.AddRange(new object[] {
            "General",
            "Backup",
            "Sync",
            "Upload",
            "Download",
            "Archive",
            "Migration",
            "Development",
            "Production",
            "Testing"});
            this.comboBoxCategory.Location = new System.Drawing.Point(100, 102);
            this.comboBoxCategory.Name = "comboBoxCategory";
            this.comboBoxCategory.Size = new System.Drawing.Size(200, 21);
            this.comboBoxCategory.TabIndex = 5;
            
            // 
            // labelTags
            // 
            this.labelTags.AutoSize = true;
            this.labelTags.Location = new System.Drawing.Point(320, 105);
            this.labelTags.Name = "labelTags";
            this.labelTags.Size = new System.Drawing.Size(34, 13);
            this.labelTags.TabIndex = 6;
            this.labelTags.Text = "Tags:";
            
            // 
            // textBoxTags
            // 
            this.textBoxTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTags.Location = new System.Drawing.Point(360, 102);
            this.textBoxTags.Name = "textBoxTags";
            this.textBoxTags.Size = new System.Drawing.Size(180, 20);
            this.textBoxTags.TabIndex = 7;
            this.textBoxTags.Text = "Separate tags with commas";
            this.textBoxTags.ForeColor = System.Drawing.SystemColors.GrayText;
            
            // 
            // groupBoxOptions
            // 
            this.groupBoxOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOptions.Controls.Add(this.checkBoxSetAsDefault);
            this.groupBoxOptions.Controls.Add(this.checkBoxAddToQuickLaunch);
            this.groupBoxOptions.Controls.Add(this.checkBoxAutoStartOnLoad);
            this.groupBoxOptions.Controls.Add(this.checkBoxShowNotificationOnStart);
            this.groupBoxOptions.Location = new System.Drawing.Point(12, 180);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(560, 100);
            this.groupBoxOptions.TabIndex = 1;
            this.groupBoxOptions.TabStop = false;
            this.groupBoxOptions.Text = "Options";
            this.groupBoxOptions.Visible = false;
            
            // 
            // checkBoxSetAsDefault
            // 
            this.checkBoxSetAsDefault.AutoSize = true;
            this.checkBoxSetAsDefault.Location = new System.Drawing.Point(20, 25);
            this.checkBoxSetAsDefault.Name = "checkBoxSetAsDefault";
            this.checkBoxSetAsDefault.Size = new System.Drawing.Size(92, 17);
            this.checkBoxSetAsDefault.TabIndex = 0;
            this.checkBoxSetAsDefault.Text = "Set as Default";
            this.checkBoxSetAsDefault.UseVisualStyleBackColor = true;
            
            // 
            // checkBoxAddToQuickLaunch
            // 
            this.checkBoxAddToQuickLaunch.AutoSize = true;
            this.checkBoxAddToQuickLaunch.Checked = true;
            this.checkBoxAddToQuickLaunch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAddToQuickLaunch.Location = new System.Drawing.Point(280, 25);
            this.checkBoxAddToQuickLaunch.Name = "checkBoxAddToQuickLaunch";
            this.checkBoxAddToQuickLaunch.Size = new System.Drawing.Size(120, 17);
            this.checkBoxAddToQuickLaunch.TabIndex = 1;
            this.checkBoxAddToQuickLaunch.Text = "Add to Quick Launch";
            this.checkBoxAddToQuickLaunch.UseVisualStyleBackColor = true;
            
            // 
            // checkBoxAutoStartOnLoad
            // 
            this.checkBoxAutoStartOnLoad.AutoSize = true;
            this.checkBoxAutoStartOnLoad.Location = new System.Drawing.Point(20, 55);
            this.checkBoxAutoStartOnLoad.Name = "checkBoxAutoStartOnLoad";
            this.checkBoxAutoStartOnLoad.Size = new System.Drawing.Size(112, 17);
            this.checkBoxAutoStartOnLoad.TabIndex = 2;
            this.checkBoxAutoStartOnLoad.Text = "Auto Start on Load";
            this.checkBoxAutoStartOnLoad.UseVisualStyleBackColor = true;
            
            // 
            // checkBoxShowNotificationOnStart
            // 
            this.checkBoxShowNotificationOnStart.AutoSize = true;
            this.checkBoxShowNotificationOnStart.Checked = true;
            this.checkBoxShowNotificationOnStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowNotificationOnStart.Location = new System.Drawing.Point(280, 55);
            this.checkBoxShowNotificationOnStart.Name = "checkBoxShowNotificationOnStart";
            this.checkBoxShowNotificationOnStart.Size = new System.Drawing.Size(150, 17);
            this.checkBoxShowNotificationOnStart.TabIndex = 3;
            this.checkBoxShowNotificationOnStart.Text = "Show Notification on Start";
            this.checkBoxShowNotificationOnStart.UseVisualStyleBackColor = true;
            
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.listViewPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(12, 180);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(560, 290);
            this.groupBoxPreview.TabIndex = 2;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Configuration Preview";
            
            // 
            // listViewPreview
            // 
            this.listViewPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewPreview.FullRowSelect = true;
            this.listViewPreview.GridLines = true;
            this.listViewPreview.Location = new System.Drawing.Point(15, 25);
            this.listViewPreview.Name = "listViewPreview";
            this.listViewPreview.Size = new System.Drawing.Size(525, 250);
            this.listViewPreview.TabIndex = 0;
            this.listViewPreview.UseCompatibleStateImageBehavior = false;
            this.listViewPreview.View = System.Windows.Forms.View.Details;
            
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(416, 485);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 3;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(497, 485);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            
            // 
            // buttonTestConfig
            // 
            this.buttonTestConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonTestConfig.Location = new System.Drawing.Point(12, 485);
            this.buttonTestConfig.Name = "buttonTestConfig";
            this.buttonTestConfig.Size = new System.Drawing.Size(100, 23);
            this.buttonTestConfig.TabIndex = 5;
            this.buttonTestConfig.Text = "Test Configuration";
            this.buttonTestConfig.UseVisualStyleBackColor = true;
            this.buttonTestConfig.Click += new System.EventHandler(this.buttonTestConfig_Click);
            
            // 
            // FormSaveJobConfiguration
            // 
            this.AcceptButton = this.buttonSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(584, 520);
            this.Controls.Add(this.buttonTestConfig);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.groupBoxOptions);
            this.Controls.Add(this.groupBoxBasicInfo);
            this.MinimumSize = new System.Drawing.Size(600, 550);
            this.Name = "FormSaveJobConfiguration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Save Job Configuration";
            this.groupBoxBasicInfo.ResumeLayout(false);
            this.groupBoxBasicInfo.PerformLayout();
            this.groupBoxOptions.ResumeLayout(false);
            this.groupBoxOptions.PerformLayout();
            this.groupBoxPreview.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxBasicInfo;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Label labelCategory;
        private System.Windows.Forms.ComboBox comboBoxCategory;
        private System.Windows.Forms.Label labelTags;
        private System.Windows.Forms.TextBox textBoxTags;
        
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private System.Windows.Forms.CheckBox checkBoxSetAsDefault;
        private System.Windows.Forms.CheckBox checkBoxAddToQuickLaunch;
        private System.Windows.Forms.CheckBox checkBoxAutoStartOnLoad;
        private System.Windows.Forms.CheckBox checkBoxShowNotificationOnStart;
        
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.ListView listViewPreview;
        
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonTestConfig;
    }
}





