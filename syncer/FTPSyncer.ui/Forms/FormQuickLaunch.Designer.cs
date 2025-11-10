namespace FTPSyncer.ui.Forms
{
    partial class FormQuickLaunch
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
            this.lbQuickLaunch = new System.Windows.Forms.ListBox();
            this.btnLoadAndStartJob = new System.Windows.Forms.Button();
            this.btnEditSelectedJob = new System.Windows.Forms.Button();
            this.btnManageConfigurations = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbQuickLaunch
            // 
            this.lbQuickLaunch.DisplayMember = "DisplayName";
            this.lbQuickLaunch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lbQuickLaunch.FormattingEnabled = true;
            this.lbQuickLaunch.ItemHeight = 15;
            this.lbQuickLaunch.Location = new System.Drawing.Point(12, 12);
            this.lbQuickLaunch.Name = "lbQuickLaunch";
            this.lbQuickLaunch.Size = new System.Drawing.Size(545, 225);
            this.lbQuickLaunch.TabIndex = 0;
            this.lbQuickLaunch.ValueMember = "ConfigurationId";
            // 
            // btnLoadAndStartJob
            // 
            this.btnLoadAndStartJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnLoadAndStartJob.Location = new System.Drawing.Point(12, 250);
            this.btnLoadAndStartJob.Name = "btnLoadAndStartJob";
            this.btnLoadAndStartJob.Size = new System.Drawing.Size(120, 30);
            this.btnLoadAndStartJob.TabIndex = 1;
            this.btnLoadAndStartJob.Text = "Load & Start Job";
            this.btnLoadAndStartJob.UseVisualStyleBackColor = true;
            this.btnLoadAndStartJob.Click += new System.EventHandler(this.btnLoadAndStartJob_Click);
            // 
            // btnEditSelectedJob
            // 
            this.btnEditSelectedJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnEditSelectedJob.Location = new System.Drawing.Point(142, 250);
            this.btnEditSelectedJob.Name = "btnEditSelectedJob";
            this.btnEditSelectedJob.Size = new System.Drawing.Size(120, 30);
            this.btnEditSelectedJob.TabIndex = 2;
            this.btnEditSelectedJob.Text = "Edit Selected Job";
            this.btnEditSelectedJob.UseVisualStyleBackColor = true;
            this.btnEditSelectedJob.Click += new System.EventHandler(this.btnEditSelectedJob_Click);
            // 
            // btnManageConfigurations
            // 
            this.btnManageConfigurations.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnManageConfigurations.Location = new System.Drawing.Point(400, 250);
            this.btnManageConfigurations.Name = "btnManageConfigurations";
            this.btnManageConfigurations.Size = new System.Drawing.Size(150, 30);
            this.btnManageConfigurations.TabIndex = 3;
            this.btnManageConfigurations.Text = "Manage Configurations";
            this.btnManageConfigurations.UseVisualStyleBackColor = true;
            this.btnManageConfigurations.Click += new System.EventHandler(this.btnManageConfigurations_Click);
            // 
            // FormQuickLaunch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(569, 295);
            this.Controls.Add(this.btnManageConfigurations);
            this.Controls.Add(this.btnEditSelectedJob);
            this.Controls.Add(this.btnLoadAndStartJob);
            this.Controls.Add(this.lbQuickLaunch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormQuickLaunch";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quick Launch Configurations";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbQuickLaunch;
        private System.Windows.Forms.Button btnLoadAndStartJob;
        private System.Windows.Forms.Button btnEditSelectedJob;
        private System.Windows.Forms.Button btnManageConfigurations;
    }
}





