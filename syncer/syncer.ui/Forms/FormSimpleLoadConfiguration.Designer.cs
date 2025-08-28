namespace syncer.ui.Forms
{
    partial class FormSimpleLoadConfiguration
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.listBoxConfigurations = new System.Windows.Forms.ListBox();
            this.groupBoxDetails = new System.Windows.Forms.GroupBox();
            this.lblConfigName = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblCategory = new System.Windows.Forms.Label();
            this.lblCreated = new System.Windows.Forms.Label();
            this.lblLastUsed = new System.Windows.Forms.Label();
            this.lblTimesUsed = new System.Windows.Forms.Label();
            this.lblSourcePath = new System.Windows.Forms.Label();
            this.lblDestinationPath = new System.Windows.Forms.Label();
            this.lblSourceConnection = new System.Windows.Forms.Label();
            this.lblDestinationConnection = new System.Windows.Forms.Label();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnLoadAndStart = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.groupBoxDetails.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(170, 17);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Select Configuration:";
            
            // 
            // listBoxConfigurations
            // 
            this.listBoxConfigurations.DisplayMember = "Name";
            this.listBoxConfigurations.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.listBoxConfigurations.FormattingEnabled = true;
            this.listBoxConfigurations.ItemHeight = 15;
            this.listBoxConfigurations.Location = new System.Drawing.Point(15, 40);
            this.listBoxConfigurations.Name = "listBoxConfigurations";
            this.listBoxConfigurations.Size = new System.Drawing.Size(270, 350);
            this.listBoxConfigurations.TabIndex = 1;
            this.listBoxConfigurations.SelectedIndexChanged += new System.EventHandler(this.listBoxConfigurations_SelectedIndexChanged);
            this.listBoxConfigurations.DoubleClick += new System.EventHandler(this.listBoxConfigurations_DoubleClick);
            
            // 
            // groupBoxDetails
            // 
            this.groupBoxDetails.Controls.Add(this.lblConfigName);
            this.groupBoxDetails.Controls.Add(this.lblDescription);
            this.groupBoxDetails.Controls.Add(this.lblCategory);
            this.groupBoxDetails.Controls.Add(this.lblCreated);
            this.groupBoxDetails.Controls.Add(this.lblLastUsed);
            this.groupBoxDetails.Controls.Add(this.lblTimesUsed);
            this.groupBoxDetails.Controls.Add(this.lblSourcePath);
            this.groupBoxDetails.Controls.Add(this.lblDestinationPath);
            this.groupBoxDetails.Controls.Add(this.lblSourceConnection);
            this.groupBoxDetails.Controls.Add(this.lblDestinationConnection);
            this.groupBoxDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxDetails.Location = new System.Drawing.Point(300, 40);
            this.groupBoxDetails.Name = "groupBoxDetails";
            this.groupBoxDetails.Size = new System.Drawing.Size(320, 350);
            this.groupBoxDetails.TabIndex = 2;
            this.groupBoxDetails.TabStop = false;
            this.groupBoxDetails.Text = "Configuration Details";
            
            // 
            // lblConfigName
            // 
            this.lblConfigName.AutoSize = true;
            this.lblConfigName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblConfigName.Location = new System.Drawing.Point(10, 25);
            this.lblConfigName.Name = "lblConfigName";
            this.lblConfigName.Size = new System.Drawing.Size(41, 13);
            this.lblConfigName.TabIndex = 0;
            this.lblConfigName.Text = "Name: ";
            
            // 
            // lblDescription
            // 
            this.lblDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblDescription.Location = new System.Drawing.Point(10, 45);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(250, 40);
            this.lblDescription.TabIndex = 1;
            this.lblDescription.Text = "Description: ";
            
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblCategory.Location = new System.Drawing.Point(10, 95);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(55, 13);
            this.lblCategory.TabIndex = 2;
            this.lblCategory.Text = "Category: ";
            
            // 
            // lblCreated
            // 
            this.lblCreated.AutoSize = true;
            this.lblCreated.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblCreated.Location = new System.Drawing.Point(10, 115);
            this.lblCreated.Name = "lblCreated";
            this.lblCreated.Size = new System.Drawing.Size(50, 13);
            this.lblCreated.TabIndex = 3;
            this.lblCreated.Text = "Created: ";
            
            // 
            // lblLastUsed
            // 
            this.lblLastUsed.AutoSize = true;
            this.lblLastUsed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblLastUsed.Location = new System.Drawing.Point(10, 135);
            this.lblLastUsed.Name = "lblLastUsed";
            this.lblLastUsed.Size = new System.Drawing.Size(62, 13);
            this.lblLastUsed.TabIndex = 4;
            this.lblLastUsed.Text = "Last Used: ";
            
            // 
            // lblTimesUsed
            // 
            this.lblTimesUsed.AutoSize = true;
            this.lblTimesUsed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblTimesUsed.Location = new System.Drawing.Point(10, 155);
            this.lblTimesUsed.Name = "lblTimesUsed";
            this.lblTimesUsed.Size = new System.Drawing.Size(70, 13);
            this.lblTimesUsed.TabIndex = 5;
            this.lblTimesUsed.Text = "Times Used: ";
            
            // 
            // lblSourcePath
            // 
            this.lblSourcePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblSourcePath.Location = new System.Drawing.Point(10, 180);
            this.lblSourcePath.Name = "lblSourcePath";
            this.lblSourcePath.Size = new System.Drawing.Size(300, 30);
            this.lblSourcePath.TabIndex = 6;
            this.lblSourcePath.Text = "Source: ";
            
            // 
            // lblDestinationPath
            // 
            this.lblDestinationPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblDestinationPath.Location = new System.Drawing.Point(10, 220);
            this.lblDestinationPath.Name = "lblDestinationPath";
            this.lblDestinationPath.Size = new System.Drawing.Size(300, 30);
            this.lblDestinationPath.TabIndex = 7;
            this.lblDestinationPath.Text = "Destination: ";
            
            // 
            // lblSourceConnection
            // 
            this.lblSourceConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblSourceConnection.Location = new System.Drawing.Point(10, 260);
            this.lblSourceConnection.Name = "lblSourceConnection";
            this.lblSourceConnection.Size = new System.Drawing.Size(300, 30);
            this.lblSourceConnection.TabIndex = 8;
            this.lblSourceConnection.Text = "Source Connection: ";
            
            // 
            // lblDestinationConnection
            // 
            this.lblDestinationConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblDestinationConnection.Location = new System.Drawing.Point(10, 300);
            this.lblDestinationConnection.Name = "lblDestinationConnection";
            this.lblDestinationConnection.Size = new System.Drawing.Size(300, 30);
            this.lblDestinationConnection.TabIndex = 9;
            this.lblDestinationConnection.Text = "Destination Connection: ";
            
            // 
            // btnLoad
            // 
            this.btnLoad.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnLoad.Location = new System.Drawing.Point(15, 400);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(90, 35);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "Load Only";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            
            // 
            // btnLoadAndStart
            // 
            this.btnLoadAndStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnLoadAndStart.Location = new System.Drawing.Point(115, 400);
            this.btnLoadAndStart.Name = "btnLoadAndStart";
            this.btnLoadAndStart.Size = new System.Drawing.Size(100, 35);
            this.btnLoadAndStart.TabIndex = 4;
            this.btnLoadAndStart.Text = "Load && Start";
            this.btnLoadAndStart.UseVisualStyleBackColor = true;
            this.btnLoadAndStart.Click += new System.EventHandler(this.btnLoadAndStart_Click);
            
            // 
            // btnEdit
            // 
            this.btnEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnEdit.Location = new System.Drawing.Point(225, 400);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(70, 35);
            this.btnEdit.TabIndex = 5;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            
            // 
            // btnDelete
            // 
            this.btnDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnDelete.Location = new System.Drawing.Point(305, 400);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(70, 35);
            this.btnDelete.TabIndex = 6;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            
            // 
            // btnExport
            // 
            this.btnExport.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnExport.Location = new System.Drawing.Point(385, 400);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(70, 35);
            this.btnExport.TabIndex = 7;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            
            // 
            // btnImport
            // 
            this.btnImport.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnImport.Location = new System.Drawing.Point(465, 400);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(70, 35);
            this.btnImport.TabIndex = 8;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            
            // 
            // FormSimpleLoadConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 480);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnLoadAndStart);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.groupBoxDetails);
            this.Controls.Add(this.listBoxConfigurations);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSimpleLoadConfiguration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Load Configuration";
            this.groupBoxDetails.ResumeLayout(false);
            this.groupBoxDetails.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ListBox listBoxConfigurations;
        private System.Windows.Forms.GroupBox groupBoxDetails;
        private System.Windows.Forms.Label lblConfigName;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.Label lblCreated;
        private System.Windows.Forms.Label lblLastUsed;
        private System.Windows.Forms.Label lblTimesUsed;
        private System.Windows.Forms.Label lblSourcePath;
        private System.Windows.Forms.Label lblDestinationPath;
        private System.Windows.Forms.Label lblSourceConnection;
        private System.Windows.Forms.Label lblDestinationConnection;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnLoadAndStart;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnImport;
    }
}
