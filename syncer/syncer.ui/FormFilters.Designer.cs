namespace syncer.ui
{
    partial class FormFilters
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.CheckBox chkEnableFilters;
        private System.Windows.Forms.GroupBox gbFileTypes;
        private System.Windows.Forms.CheckedListBox clbFileTypes;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnSelectNone;
        private System.Windows.Forms.Button btnAddCustom;
        private System.Windows.Forms.Button btnRemoveSelected;
        private System.Windows.Forms.GroupBox gbSizeFilters;
        private System.Windows.Forms.Label lblMinSize;
        private System.Windows.Forms.NumericUpDown numMinSize;
        private System.Windows.Forms.Label lblMaxSize;
        private System.Windows.Forms.NumericUpDown numMaxSize;
        private System.Windows.Forms.GroupBox gbAdvancedFilters;
        private System.Windows.Forms.CheckBox chkIncludeHidden;
        private System.Windows.Forms.CheckBox chkIncludeSystem;
        private System.Windows.Forms.CheckBox chkIncludeReadOnly;
        private System.Windows.Forms.CheckBox chkIncludeSubfolders;
        private System.Windows.Forms.Label lblExcludePatterns;
        private System.Windows.Forms.TextBox txtExcludePatterns;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnPreview;

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
            this.chkEnableFilters = new System.Windows.Forms.CheckBox();
            this.gbFileTypes = new System.Windows.Forms.GroupBox();
            this.clbFileTypes = new System.Windows.Forms.CheckedListBox();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnSelectNone = new System.Windows.Forms.Button();
            this.btnAddCustom = new System.Windows.Forms.Button();
            this.btnRemoveSelected = new System.Windows.Forms.Button();
            this.gbSizeFilters = new System.Windows.Forms.GroupBox();
            this.lblMinSize = new System.Windows.Forms.Label();
            this.numMinSize = new System.Windows.Forms.NumericUpDown();
            this.lblMaxSize = new System.Windows.Forms.Label();
            this.numMaxSize = new System.Windows.Forms.NumericUpDown();
            this.gbAdvancedFilters = new System.Windows.Forms.GroupBox();
            this.chkIncludeHidden = new System.Windows.Forms.CheckBox();
            this.chkIncludeSystem = new System.Windows.Forms.CheckBox();
            this.chkIncludeReadOnly = new System.Windows.Forms.CheckBox();
            this.chkIncludeSubfolders = new System.Windows.Forms.CheckBox();
            this.lblExcludePatterns = new System.Windows.Forms.Label();
            this.txtExcludePatterns = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.gbFileTypes.SuspendLayout();
            this.gbSizeFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMinSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxSize)).BeginInit();
            this.gbAdvancedFilters.SuspendLayout();
            this.SuspendLayout();
            // chkEnableFilters
            this.chkEnableFilters.AutoSize = true;
            this.chkEnableFilters.Location = new System.Drawing.Point(12, 12);
            this.chkEnableFilters.Name = "chkEnableFilters";
            this.chkEnableFilters.Size = new System.Drawing.Size(97, 17);
            this.chkEnableFilters.TabIndex = 0;
            this.chkEnableFilters.Text = "Enable Filters";
            this.chkEnableFilters.UseVisualStyleBackColor = true;
            this.chkEnableFilters.CheckedChanged += new System.EventHandler(this.chkEnableFilters_CheckedChanged);
            // gbFileTypes
            this.gbFileTypes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbFileTypes.Controls.Add(this.clbFileTypes);
            this.gbFileTypes.Controls.Add(this.btnSelectAll);
            this.gbFileTypes.Controls.Add(this.btnSelectNone);
            this.gbFileTypes.Controls.Add(this.btnAddCustom);
            this.gbFileTypes.Controls.Add(this.btnRemoveSelected);
            this.gbFileTypes.Location = new System.Drawing.Point(12, 35);
            this.gbFileTypes.Name = "gbFileTypes";
            this.gbFileTypes.Size = new System.Drawing.Size(560, 220);
            this.gbFileTypes.TabIndex = 1;
            this.gbFileTypes.TabStop = false;
            this.gbFileTypes.Text = "File Types";
            // clbFileTypes
            this.clbFileTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clbFileTypes.CheckOnClick = true;
            this.clbFileTypes.FormattingEnabled = true;
            this.clbFileTypes.Location = new System.Drawing.Point(10, 20);
            this.clbFileTypes.Name = "clbFileTypes";
            this.clbFileTypes.Size = new System.Drawing.Size(420, 169);
            this.clbFileTypes.TabIndex = 0;
            // btnSelectAll
            this.btnSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectAll.Location = new System.Drawing.Point(440, 20);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(110, 25);
            this.btnSelectAll.TabIndex = 1;
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // btnSelectNone
            this.btnSelectNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectNone.Location = new System.Drawing.Point(440, 50);
            this.btnSelectNone.Name = "btnSelectNone";
            this.btnSelectNone.Size = new System.Drawing.Size(110, 25);
            this.btnSelectNone.TabIndex = 2;
            this.btnSelectNone.Text = "Select None";
            this.btnSelectNone.UseVisualStyleBackColor = true;
            this.btnSelectNone.Click += new System.EventHandler(this.btnSelectNone_Click);
            // btnAddCustom
            this.btnAddCustom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddCustom.Location = new System.Drawing.Point(440, 80);
            this.btnAddCustom.Name = "btnAddCustom";
            this.btnAddCustom.Size = new System.Drawing.Size(110, 25);
            this.btnAddCustom.TabIndex = 3;
            this.btnAddCustom.Text = "Add Custom";
            this.btnAddCustom.UseVisualStyleBackColor = true;
            this.btnAddCustom.Click += new System.EventHandler(this.btnAddCustom_Click);
            // btnRemoveSelected
            this.btnRemoveSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveSelected.Location = new System.Drawing.Point(440, 110);
            this.btnRemoveSelected.Name = "btnRemoveSelected";
            this.btnRemoveSelected.Size = new System.Drawing.Size(110, 25);
            this.btnRemoveSelected.TabIndex = 4;
            this.btnRemoveSelected.Text = "Remove Selected";
            this.btnRemoveSelected.UseVisualStyleBackColor = true;
            this.btnRemoveSelected.Click += new System.EventHandler(this.btnRemoveSelected_Click);
            // gbSizeFilters
            this.gbSizeFilters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSizeFilters.Controls.Add(this.lblMinSize);
            this.gbSizeFilters.Controls.Add(this.numMinSize);
            this.gbSizeFilters.Controls.Add(this.lblMaxSize);
            this.gbSizeFilters.Controls.Add(this.numMaxSize);
            this.gbSizeFilters.Location = new System.Drawing.Point(12, 261);
            this.gbSizeFilters.Name = "gbSizeFilters";
            this.gbSizeFilters.Size = new System.Drawing.Size(560, 60);
            this.gbSizeFilters.TabIndex = 2;
            this.gbSizeFilters.TabStop = false;
            this.gbSizeFilters.Text = "Size Filters (MB)";
            // lblMinSize
            this.lblMinSize.AutoSize = true;
            this.lblMinSize.Location = new System.Drawing.Point(10, 27);
            this.lblMinSize.Name = "lblMinSize";
            this.lblMinSize.Size = new System.Drawing.Size(57, 13);
            this.lblMinSize.TabIndex = 0;
            this.lblMinSize.Text = "Min Size:";
            // numMinSize
            this.numMinSize.Location = new System.Drawing.Point(70, 24);
            this.numMinSize.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numMinSize.Name = "numMinSize";
            this.numMinSize.Size = new System.Drawing.Size(100, 20);
            this.numMinSize.TabIndex = 1;
            // lblMaxSize
            this.lblMaxSize.AutoSize = true;
            this.lblMaxSize.Location = new System.Drawing.Point(200, 27);
            this.lblMaxSize.Name = "lblMaxSize";
            this.lblMaxSize.Size = new System.Drawing.Size(60, 13);
            this.lblMaxSize.TabIndex = 2;
            this.lblMaxSize.Text = "Max Size:";
            // numMaxSize
            this.numMaxSize.Location = new System.Drawing.Point(265, 24);
            this.numMaxSize.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numMaxSize.Name = "numMaxSize";
            this.numMaxSize.Size = new System.Drawing.Size(100, 20);
            this.numMaxSize.TabIndex = 3;
            // gbAdvancedFilters
            this.gbAdvancedFilters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbAdvancedFilters.Controls.Add(this.chkIncludeHidden);
            this.gbAdvancedFilters.Controls.Add(this.chkIncludeSystem);
            this.gbAdvancedFilters.Controls.Add(this.chkIncludeReadOnly);
            this.gbAdvancedFilters.Controls.Add(this.chkIncludeSubfolders);
            this.gbAdvancedFilters.Controls.Add(this.lblExcludePatterns);
            this.gbAdvancedFilters.Controls.Add(this.txtExcludePatterns);
            this.gbAdvancedFilters.Location = new System.Drawing.Point(12, 327);
            this.gbAdvancedFilters.Name = "gbAdvancedFilters";
            this.gbAdvancedFilters.Size = new System.Drawing.Size(560, 120);
            this.gbAdvancedFilters.TabIndex = 3;
            this.gbAdvancedFilters.TabStop = false;
            this.gbAdvancedFilters.Text = "Advanced";
            // chkIncludeHidden
            this.chkIncludeHidden.AutoSize = true;
            this.chkIncludeHidden.Location = new System.Drawing.Point(13, 22);
            this.chkIncludeHidden.Name = "chkIncludeHidden";
            this.chkIncludeHidden.Size = new System.Drawing.Size(105, 17);
            this.chkIncludeHidden.TabIndex = 0;
            this.chkIncludeHidden.Text = "Include Hidden";
            this.chkIncludeHidden.UseVisualStyleBackColor = true;
            // chkIncludeSystem
            this.chkIncludeSystem.AutoSize = true;
            this.chkIncludeSystem.Location = new System.Drawing.Point(134, 22);
            this.chkIncludeSystem.Name = "chkIncludeSystem";
            this.chkIncludeSystem.Size = new System.Drawing.Size(105, 17);
            this.chkIncludeSystem.TabIndex = 1;
            this.chkIncludeSystem.Text = "Include System";
            this.chkIncludeSystem.UseVisualStyleBackColor = true;
            // chkIncludeReadOnly
            this.chkIncludeReadOnly.AutoSize = true;
            this.chkIncludeReadOnly.Location = new System.Drawing.Point(256, 22);
            this.chkIncludeReadOnly.Name = "chkIncludeReadOnly";
            this.chkIncludeReadOnly.Size = new System.Drawing.Size(120, 17);
            this.chkIncludeReadOnly.TabIndex = 2;
            this.chkIncludeReadOnly.Text = "Include Read-only";
            this.chkIncludeReadOnly.UseVisualStyleBackColor = true;
            // chkIncludeSubfolders
            this.chkIncludeSubfolders.AutoSize = true;
            this.chkIncludeSubfolders.Location = new System.Drawing.Point(400, 22);
            this.chkIncludeSubfolders.Name = "chkIncludeSubfolders";
            this.chkIncludeSubfolders.Size = new System.Drawing.Size(115, 17);
            this.chkIncludeSubfolders.TabIndex = 3;
            this.chkIncludeSubfolders.Text = "Include Subfolders";
            this.chkIncludeSubfolders.UseVisualStyleBackColor = true;
            // lblExcludePatterns
            this.lblExcludePatterns.AutoSize = true;
            this.lblExcludePatterns.Location = new System.Drawing.Point(10, 55);
            this.lblExcludePatterns.Name = "lblExcludePatterns";
            this.lblExcludePatterns.Size = new System.Drawing.Size(93, 13);
            this.lblExcludePatterns.TabIndex = 3;
            this.lblExcludePatterns.Text = "Exclude Patterns:";
            // txtExcludePatterns
            this.txtExcludePatterns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExcludePatterns.Location = new System.Drawing.Point(13, 73);
            this.txtExcludePatterns.Name = "txtExcludePatterns";
            this.txtExcludePatterns.Size = new System.Drawing.Size(534, 20);
            this.txtExcludePatterns.TabIndex = 4;
            // btnSave
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(416, 456);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 25);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // btnCancel
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(497, 456);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // btnPreview
            this.btnPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPreview.Location = new System.Drawing.Point(12, 456);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(85, 25);
            this.btnPreview.TabIndex = 6;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // FormFilters
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 491);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.gbAdvancedFilters);
            this.Controls.Add(this.gbSizeFilters);
            this.Controls.Add(this.gbFileTypes);
            this.Controls.Add(this.chkEnableFilters);
            this.Name = "FormFilters";
            this.Text = "Filter Settings";
            this.gbFileTypes.ResumeLayout(false);
            this.gbSizeFilters.ResumeLayout(false);
            this.gbSizeFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMinSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxSize)).EndInit();
            this.gbAdvancedFilters.ResumeLayout(false);
            this.gbAdvancedFilters.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
