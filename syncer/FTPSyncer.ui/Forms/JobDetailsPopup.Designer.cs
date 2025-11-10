namespace FTPSyncer.ui.Forms
{
    partial class JobDetailsPopup
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lblJobId;
        private System.Windows.Forms.Label lblJobName;
        private System.Windows.Forms.Label lblJobType;
        private System.Windows.Forms.Label lblSourcePath;
        private System.Windows.Forms.Label lblDestinationPath;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblLastTransfer;
        private System.Windows.Forms.Label lblNextRun;
        private System.Windows.Forms.Label lblIncludeSubfolders;
        private System.Windows.Forms.Label lblDeleteSource;
        private System.Windows.Forms.Label lblFiltersEnabled;
        private System.Windows.Forms.Label lblIncludeExtensions;
        private System.Windows.Forms.Label lblExcludeExtensions;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.GroupBox gbBasicInfo;
        private System.Windows.Forms.GroupBox gbConfiguration;
        private System.Windows.Forms.GroupBox gbStatus;
        private System.Windows.Forms.GroupBox gbFilters;

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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lblJobId = new System.Windows.Forms.Label();
            this.lblJobName = new System.Windows.Forms.Label();
            this.lblJobType = new System.Windows.Forms.Label();
            this.lblSourcePath = new System.Windows.Forms.Label();
            this.lblDestinationPath = new System.Windows.Forms.Label();
            this.lblInterval = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblLastTransfer = new System.Windows.Forms.Label();
            this.lblNextRun = new System.Windows.Forms.Label();
            this.lblIncludeSubfolders = new System.Windows.Forms.Label();
            this.lblDeleteSource = new System.Windows.Forms.Label();
            this.lblFiltersEnabled = new System.Windows.Forms.Label();
            this.lblIncludeExtensions = new System.Windows.Forms.Label();
            this.lblExcludeExtensions = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.gbBasicInfo = new System.Windows.Forms.GroupBox();
            this.gbConfiguration = new System.Windows.Forms.GroupBox();
            this.gbStatus = new System.Windows.Forms.GroupBox();
            this.gbFilters = new System.Windows.Forms.GroupBox();
            this.gbBasicInfo.SuspendLayout();
            this.gbConfiguration.SuspendLayout();
            this.gbStatus.SuspendLayout();
            this.gbFilters.SuspendLayout();
            this.SuspendLayout();
            
            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 560);
            this.Controls.Add(this.gbBasicInfo);
            this.Controls.Add(this.gbConfiguration);
            this.Controls.Add(this.gbStatus);
            this.Controls.Add(this.gbFilters);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRefresh);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JobDetailsPopup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Job Details";
            
            // gbBasicInfo
            this.gbBasicInfo.Controls.Add(this.label1);
            this.gbBasicInfo.Controls.Add(this.lblJobId);
            this.gbBasicInfo.Controls.Add(this.label2);
            this.gbBasicInfo.Controls.Add(this.lblJobName);
            this.gbBasicInfo.Controls.Add(this.label3);
            this.gbBasicInfo.Controls.Add(this.lblJobType);
            this.gbBasicInfo.Controls.Add(this.label4);
            this.gbBasicInfo.Controls.Add(this.lblSourcePath);
            this.gbBasicInfo.Controls.Add(this.label5);
            this.gbBasicInfo.Controls.Add(this.lblDestinationPath);
            this.gbBasicInfo.Controls.Add(this.label6);
            this.gbBasicInfo.Controls.Add(this.lblInterval);
            this.gbBasicInfo.Location = new System.Drawing.Point(12, 12);
            this.gbBasicInfo.Name = "gbBasicInfo";
            this.gbBasicInfo.Size = new System.Drawing.Size(556, 160);
            this.gbBasicInfo.TabIndex = 0;
            this.gbBasicInfo.TabStop = false;
            this.gbBasicInfo.Text = "Basic Information";
            
            // label1 (Job ID)
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Job ID:";
            
            // lblJobId
            this.lblJobId.AutoSize = true;
            this.lblJobId.Location = new System.Drawing.Point(120, 22);
            this.lblJobId.Name = "lblJobId";
            this.lblJobId.Size = new System.Drawing.Size(27, 13);
            this.lblJobId.TabIndex = 1;
            this.lblJobId.Text = "N/A";
            
            // label2 (Job Name)
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(6, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Job Name:";
            
            // lblJobName
            this.lblJobName.AutoSize = true;
            this.lblJobName.Location = new System.Drawing.Point(120, 42);
            this.lblJobName.Name = "lblJobName";
            this.lblJobName.Size = new System.Drawing.Size(27, 13);
            this.lblJobName.TabIndex = 3;
            this.lblJobName.Text = "N/A";
            
            // label3 (Job Type)
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(6, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Job Type:";
            
            // lblJobType
            this.lblJobType.AutoSize = true;
            this.lblJobType.Location = new System.Drawing.Point(120, 62);
            this.lblJobType.Name = "lblJobType";
            this.lblJobType.Size = new System.Drawing.Size(27, 13);
            this.lblJobType.TabIndex = 5;
            this.lblJobType.Text = "N/A";
            
            // label4 (Source Path)
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(6, 82);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Source Path:";
            
            // lblSourcePath
            this.lblSourcePath.Location = new System.Drawing.Point(120, 82);
            this.lblSourcePath.Name = "lblSourcePath";
            this.lblSourcePath.Size = new System.Drawing.Size(430, 13);
            this.lblSourcePath.TabIndex = 7;
            this.lblSourcePath.Text = "N/A";
            
            // label5 (Destination Path)
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label5.Location = new System.Drawing.Point(6, 102);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(103, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Destination Path:";
            
            // lblDestinationPath
            this.lblDestinationPath.Location = new System.Drawing.Point(120, 102);
            this.lblDestinationPath.Name = "lblDestinationPath";
            this.lblDestinationPath.Size = new System.Drawing.Size(430, 13);
            this.lblDestinationPath.TabIndex = 9;
            this.lblDestinationPath.Text = "N/A";
            
            // label6 (Interval)
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(6, 122);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Interval:";
            
            // lblInterval
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(120, 122);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(27, 13);
            this.lblInterval.TabIndex = 11;
            this.lblInterval.Text = "N/A";
            
            // gbStatus
            this.gbStatus.Controls.Add(this.label7);
            this.gbStatus.Controls.Add(this.lblStatus);
            this.gbStatus.Controls.Add(this.label8);
            this.gbStatus.Controls.Add(this.lblLastTransfer);
            this.gbStatus.Controls.Add(this.label9);
            this.gbStatus.Controls.Add(this.lblNextRun);
            this.gbStatus.Location = new System.Drawing.Point(12, 178);
            this.gbStatus.Name = "gbStatus";
            this.gbStatus.Size = new System.Drawing.Size(556, 85);
            this.gbStatus.TabIndex = 1;
            this.gbStatus.TabStop = false;
            this.gbStatus.Text = "Status Information";
            
            // label7 (Status)
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label7.Location = new System.Drawing.Point(6, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Status:";
            
            // lblStatus
            this.lblStatus.Location = new System.Drawing.Point(120, 22);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(430, 13);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "N/A";
            
            // label8 (Last Transfer)
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label8.Location = new System.Drawing.Point(6, 42);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(86, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "Last Transfer:";
            
            // lblLastTransfer
            this.lblLastTransfer.AutoSize = true;
            this.lblLastTransfer.Location = new System.Drawing.Point(120, 42);
            this.lblLastTransfer.Name = "lblLastTransfer";
            this.lblLastTransfer.Size = new System.Drawing.Size(27, 13);
            this.lblLastTransfer.TabIndex = 3;
            this.lblLastTransfer.Text = "N/A";
            
            // label9 (Next Run)
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label9.Location = new System.Drawing.Point(6, 62);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(61, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "Next Run:";
            
            // lblNextRun
            this.lblNextRun.AutoSize = true;
            this.lblNextRun.Location = new System.Drawing.Point(120, 62);
            this.lblNextRun.Name = "lblNextRun";
            this.lblNextRun.Size = new System.Drawing.Size(27, 13);
            this.lblNextRun.TabIndex = 5;
            this.lblNextRun.Text = "N/A";
            
            // gbConfiguration
            this.gbConfiguration.Controls.Add(this.label10);
            this.gbConfiguration.Controls.Add(this.lblIncludeSubfolders);
            this.gbConfiguration.Controls.Add(this.label11);
            this.gbConfiguration.Controls.Add(this.lblDeleteSource);
            this.gbConfiguration.Location = new System.Drawing.Point(12, 269);
            this.gbConfiguration.Name = "gbConfiguration";
            this.gbConfiguration.Size = new System.Drawing.Size(556, 65);
            this.gbConfiguration.TabIndex = 2;
            this.gbConfiguration.TabStop = false;
            this.gbConfiguration.Text = "Configuration";
            
            // label10 (Include Subfolders)
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label10.Location = new System.Drawing.Point(6, 22);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(108, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "Include Subfolders:";
            
            // lblIncludeSubfolders
            this.lblIncludeSubfolders.AutoSize = true;
            this.lblIncludeSubfolders.Location = new System.Drawing.Point(120, 22);
            this.lblIncludeSubfolders.Name = "lblIncludeSubfolders";
            this.lblIncludeSubfolders.Size = new System.Drawing.Size(27, 13);
            this.lblIncludeSubfolders.TabIndex = 1;
            this.lblIncludeSubfolders.Text = "N/A";
            
            // label11 (Delete Source)
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label11.Location = new System.Drawing.Point(6, 42);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(155, 13);
            this.label11.TabIndex = 2;
            this.label11.Text = "Delete Source After Transfer:";
            
            // lblDeleteSource
            this.lblDeleteSource.AutoSize = true;
            this.lblDeleteSource.Location = new System.Drawing.Point(167, 42);
            this.lblDeleteSource.Name = "lblDeleteSource";
            this.lblDeleteSource.Size = new System.Drawing.Size(27, 13);
            this.lblDeleteSource.TabIndex = 3;
            this.lblDeleteSource.Text = "N/A";
            
            // gbFilters
            this.gbFilters.Controls.Add(this.label12);
            this.gbFilters.Controls.Add(this.lblFiltersEnabled);
            this.gbFilters.Controls.Add(this.label13);
            this.gbFilters.Controls.Add(this.lblIncludeExtensions);
            this.gbFilters.Controls.Add(this.label14);
            this.gbFilters.Controls.Add(this.lblExcludeExtensions);
            this.gbFilters.Location = new System.Drawing.Point(12, 340);
            this.gbFilters.Name = "gbFilters";
            this.gbFilters.Size = new System.Drawing.Size(556, 85);
            this.gbFilters.TabIndex = 3;
            this.gbFilters.TabStop = false;
            this.gbFilters.Text = "File Filters";
            
            // label12 (Filters Enabled)
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label12.Location = new System.Drawing.Point(6, 22);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(89, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Filters Enabled:";
            
            // lblFiltersEnabled
            this.lblFiltersEnabled.AutoSize = true;
            this.lblFiltersEnabled.Location = new System.Drawing.Point(101, 22);
            this.lblFiltersEnabled.Name = "lblFiltersEnabled";
            this.lblFiltersEnabled.Size = new System.Drawing.Size(27, 13);
            this.lblFiltersEnabled.TabIndex = 1;
            this.lblFiltersEnabled.Text = "N/A";
            
            // label13 (Include Extensions)
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label13.Location = new System.Drawing.Point(6, 42);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(106, 13);
            this.label13.TabIndex = 2;
            this.label13.Text = "Include Extensions:";
            
            // lblIncludeExtensions
            this.lblIncludeExtensions.Location = new System.Drawing.Point(120, 42);
            this.lblIncludeExtensions.Name = "lblIncludeExtensions";
            this.lblIncludeExtensions.Size = new System.Drawing.Size(430, 13);
            this.lblIncludeExtensions.TabIndex = 3;
            this.lblIncludeExtensions.Text = "N/A";
            
            // label14 (Exclude Extensions)
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label14.Location = new System.Drawing.Point(6, 62);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(107, 13);
            this.label14.TabIndex = 4;
            this.label14.Text = "Exclude Extensions:";
            
            // lblExcludeExtensions
            this.lblExcludeExtensions.Location = new System.Drawing.Point(120, 62);
            this.lblExcludeExtensions.Name = "lblExcludeExtensions";
            this.lblExcludeExtensions.Size = new System.Drawing.Size(430, 13);
            this.lblExcludeExtensions.TabIndex = 5;
            this.lblExcludeExtensions.Text = "N/A";
            
            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(412, 431);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            
            // btnClose
            this.btnClose.Location = new System.Drawing.Point(493, 431);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            
            this.gbBasicInfo.ResumeLayout(false);
            this.gbBasicInfo.PerformLayout();
            this.gbConfiguration.ResumeLayout(false);
            this.gbConfiguration.PerformLayout();
            this.gbStatus.ResumeLayout(false);
            this.gbStatus.PerformLayout();
            this.gbFilters.ResumeLayout(false);
            this.gbFilters.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}





