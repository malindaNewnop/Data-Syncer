namespace syncer.ui
{
    partial class FormFilters
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
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
        private System.Windows.Forms.Label lblSizeUnit1;
        private System.Windows.Forms.Label lblSizeUnit2;
        private System.Windows.Forms.GroupBox gbAdvancedFilters;
        private System.Windows.Forms.CheckBox chkIncludeHidden;
        private System.Windows.Forms.CheckBox chkIncludeSystem;
        private System.Windows.Forms.CheckBox chkIncludeReadOnly;
        private System.Windows.Forms.Label lblExcludePatterns;
        private System.Windows.Forms.TextBox txtExcludePatterns;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

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
            this.lblSizeUnit1 = new System.Windows.Forms.Label();
            this.lblMaxSize = new System.Windows.Forms.Label();
            this.numMaxSize = new System.Windows.Forms.NumericUpDown();
            this.lblSizeUnit2 = new System.Windows.Forms.Label();
            this.gbAdvancedFilters = new System.Windows.Forms.GroupBox();
            this.chkIncludeHidden = new System.Windows.Forms.CheckBox();
            this.chkIncludeSystem = new System.Windows.Forms.CheckBox();
            this.chkIncludeReadOnly = new System.Windows.Forms.CheckBox();
            this.lblExcludePatterns = new System.Windows.Forms.Label();
            this.txtExcludePatterns = new System.Windows.Forms.TextBox();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbFileTypes.SuspendLayout();
            this.gbSizeFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMinSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxSize)).BeginInit();
            this.gbAdvancedFilters.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkEnableFilters
            // 
            this.chkEnableFilters.AutoSize = true;
            this.chkEnableFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkEnableFilters.Location = new System.Drawing.Point(12, 12);
            this.chkEnableFilters.Name = "chkEnableFilters";
            this.chkEnableFilters.Size = new System.Drawing.Size(113, 19);
            this.chkEnableFilters.TabIndex = 0;
            this.chkEnableFilters.Text = "Enable Filters";
            this.chkEnableFilters.UseVisualStyleBackColor = true;
            this.chkEnableFilters.CheckedChanged += new System.EventHandler(this.chkEnableFilters_CheckedChanged);
            // 
            // gbFileTypes
            // 
            this.gbFileTypes.Controls.Add(this.clbFileTypes);
            this.gbFileTypes.Controls.Add(this.btnSelectAll);
            this.gbFileTypes.Controls.Add(this.btnSelectNone);
            this.gbFileTypes.Controls.Add(this.btnAddCustom);
            this.gbFileTypes.Controls.Add(this.btnRemoveSelected);
            this.gbFileTypes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbFileTypes.Location = new System.Drawing.Point(12, 40);
            this.gbFileTypes.Name = "gbFileTypes";
            this.gbFileTypes.Size = new System.Drawing.Size(560, 200);
            this.gbFileTypes.TabIndex = 1;
            this.gbFileTypes.TabStop = false;
            this.gbFileTypes.Text = "File Type Filters";
            // 
            // clbFileTypes
            // 
            this.clbFileTypes.CheckOnClick = true;
            this.clbFileTypes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clbFileTypes.FormattingEnabled = true;
            this.clbFileTypes.Location = new System.Drawing.Point(15, 25);
            this.clbFileTypes.Name = "clbFileTypes";
            this.clbFileTypes.Size = new System.Drawing.Size(400, 154);
            this.clbFileTypes.TabIndex = 0;
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectAll.Location = new System.Drawing.Point(430, 25);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(100, 25);
            this.btnSelectAll.TabIndex = 1;
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnSelectNone
            // 
            this.btnSelectNone.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectNone.Location = new System.Drawing.Point(430, 55);
            this.btnSelectNone.Name = "btnSelectNone";
            this.btnSelectNone.Size = new System.Drawing.Size(100, 25);
            this.btnSelectNone.TabIndex = 2;
            this.btnSelectNone.Text = "Select None";
            this.btnSelectNone.UseVisualStyleBackColor = true;
            this.btnSelectNone.Click += new System.EventHandler(this.btnSelectNone_Click);
            // 
            // btnAddCustom
            // 
            this.btnAddCustom.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddCustom.Location = new System.Drawing.Point(430, 100);
            this.btnAddCustom.Name = "btnAddCustom";
            this.btnAddCustom.Size = new System.Drawing.Size(100, 25);
            this.btnAddCustom.TabIndex = 3;
            this.btnAddCustom.Text = "Add Custom";
            this.btnAddCustom.UseVisualStyleBackColor = true;
            this.btnAddCustom.Click += new System.EventHandler(this.btnAddCustom_Click);
            // 
            // btnRemoveSelected
            // 
            this.btnRemoveSelected.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemoveSelected.Location = new System.Drawing.Point(430, 130);
            this.btnRemoveSelected.Name = "btnRemoveSelected";
            this.btnRemoveSelected.Size = new System.Drawing.Size(100, 25);
            this.btnRemoveSelected.TabIndex = 4;
            this.btnRemoveSelected.Text = "Remove Selected";
            this.btnRemoveSelected.UseVisualStyleBackColor = true;
            this.btnRemoveSelected.Click += new System.EventHandler(this.btnRemoveSelected_Click);
            // 
            // gbSizeFilters
            // 
            this.gbSizeFilters.Controls.Add(this.lblMinSize);
            this.gbSizeFilters.Controls.Add(this.numMinSize);
            this.gbSizeFilters.Controls.Add(this.lblSizeUnit1);
            this.gbSizeFilters.Controls.Add(this.lblMaxSize);
            this.gbSizeFilters.Controls.Add(this.numMaxSize);
            this.gbSizeFilters.Controls.Add(this.lblSizeUnit2);
            this.gbSizeFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbSizeFilters.Location = new System.Drawing.Point(12, 250);
            this.gbSizeFilters.Name = "gbSizeFilters";
            this.gbSizeFilters.Size = new System.Drawing.Size(270, 100);
            this.gbSizeFilters.TabIndex = 2;
            this.gbSizeFilters.TabStop = false;
            this.gbSizeFilters.Text = "File Size Filters";
            // 
            // lblMinSize
            // 
            this.lblMinSize.AutoSize = true;
            this.lblMinSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMinSize.Location = new System.Drawing.Point(15, 30);
            this.lblMinSize.Name = "lblMinSize";
            this.lblMinSize.Size = new System.Drawing.Size(85, 15);
            this.lblMinSize.TabIndex = 0;
            this.lblMinSize.Text = "Minimum Size:";
            // 
            // numMinSize
            // 
            this.numMinSize.DecimalPlaces = 1;
            this.numMinSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numMinSize.Location = new System.Drawing.Point(110, 28);
            this.numMinSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numMinSize.Name = "numMinSize";
            this.numMinSize.Size = new System.Drawing.Size(80, 21);
            this.numMinSize.TabIndex = 1;
            // 
            // lblSizeUnit1
            // 
            this.lblSizeUnit1.AutoSize = true;
            this.lblSizeUnit1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSizeUnit1.Location = new System.Drawing.Point(200, 30);
            this.lblSizeUnit1.Name = "lblSizeUnit1";
            this.lblSizeUnit1.Size = new System.Drawing.Size(25, 15);
            this.lblSizeUnit1.TabIndex = 2;
            this.lblSizeUnit1.Text = "MB";
            // 
            // lblMaxSize
            // 
            this.lblMaxSize.AutoSize = true;
            this.lblMaxSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMaxSize.Location = new System.Drawing.Point(15, 65);
            this.lblMaxSize.Name = "lblMaxSize";
            this.lblMaxSize.Size = new System.Drawing.Size(88, 15);
            this.lblMaxSize.TabIndex = 3;
            this.lblMaxSize.Text = "Maximum Size:";
            // 
            // numMaxSize
            // 
            this.numMaxSize.DecimalPlaces = 1;
            this.numMaxSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numMaxSize.Location = new System.Drawing.Point(110, 63);
            this.numMaxSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numMaxSize.Name = "numMaxSize";
            this.numMaxSize.Size = new System.Drawing.Size(80, 21);
            this.numMaxSize.TabIndex = 4;
            this.numMaxSize.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // lblSizeUnit2
            // 
            this.lblSizeUnit2.AutoSize = true;
            this.lblSizeUnit2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSizeUnit2.Location = new System.Drawing.Point(200, 65);
            this.lblSizeUnit2.Name = "lblSizeUnit2";
            this.lblSizeUnit2.Size = new System.Drawing.Size(25, 15);
            this.lblSizeUnit2.TabIndex = 5;
            this.lblSizeUnit2.Text = "MB";
            // 
            // gbAdvancedFilters
            // 
            this.gbAdvancedFilters.Controls.Add(this.chkIncludeHidden);
            this.gbAdvancedFilters.Controls.Add(this.chkIncludeSystem);
            this.gbAdvancedFilters.Controls.Add(this.chkIncludeReadOnly);
            this.gbAdvancedFilters.Controls.Add(this.lblExcludePatterns);
            this.gbAdvancedFilters.Controls.Add(this.txtExcludePatterns);
            this.gbAdvancedFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbAdvancedFilters.Location = new System.Drawing.Point(300, 250);
            this.gbAdvancedFilters.Name = "gbAdvancedFilters";
            this.gbAdvancedFilters.Size = new System.Drawing.Size(272, 150);
            this.gbAdvancedFilters.TabIndex = 3;
            this.gbAdvancedFilters.TabStop = false;
            this.gbAdvancedFilters.Text = "Advanced Filters";
            // 
            // chkIncludeHidden
            // 
            this.chkIncludeHidden.AutoSize = true;
            this.chkIncludeHidden.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkIncludeHidden.Location = new System.Drawing.Point(15, 25);
            this.chkIncludeHidden.Name = "chkIncludeHidden";
            this.chkIncludeHidden.Size = new System.Drawing.Size(129, 19);
            this.chkIncludeHidden.TabIndex = 0;
            this.chkIncludeHidden.Text = "Include hidden files";
            this.chkIncludeHidden.UseVisualStyleBackColor = true;
            // 
            // chkIncludeSystem
            // 
            this.chkIncludeSystem.AutoSize = true;
            this.chkIncludeSystem.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkIncludeSystem.Location = new System.Drawing.Point(15, 50);
            this.chkIncludeSystem.Name = "chkIncludeSystem";
            this.chkIncludeSystem.Size = new System.Drawing.Size(130, 19);
            this.chkIncludeSystem.TabIndex = 1;
            this.chkIncludeSystem.Text = "Include system files";
            this.chkIncludeSystem.UseVisualStyleBackColor = true;
            // 
            // chkIncludeReadOnly
            // 
            this.chkIncludeReadOnly.AutoSize = true;
            this.chkIncludeReadOnly.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkIncludeReadOnly.Location = new System.Drawing.Point(15, 75);
            this.chkIncludeReadOnly.Name = "chkIncludeReadOnly";
            this.chkIncludeReadOnly.Size = new System.Drawing.Size(146, 19);
            this.chkIncludeReadOnly.TabIndex = 2;
            this.chkIncludeReadOnly.Text = "Include read-only files";
            this.chkIncludeReadOnly.UseVisualStyleBackColor = true;
            // 
            // lblExcludePatterns
            // 
            this.lblExcludePatterns.AutoSize = true;
            this.lblExcludePatterns.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExcludePatterns.Location = new System.Drawing.Point(15, 105);
            this.lblExcludePatterns.Name = "lblExcludePatterns";
            this.lblExcludePatterns.Size = new System.Drawing.Size(106, 15);
            this.lblExcludePatterns.TabIndex = 3;
            this.lblExcludePatterns.Text = "Exclude Patterns:";
            // 
            // txtExcludePatterns
            // 
            this.txtExcludePatterns.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtExcludePatterns.Location = new System.Drawing.Point(15, 123);
            this.txtExcludePatterns.Name = "txtExcludePatterns";
            this.txtExcludePatterns.Size = new System.Drawing.Size(240, 20);
            this.txtExcludePatterns.TabIndex = 4;
            // 
            // btnPreview
            // 
            this.btnPreview.BackColor = System.Drawing.Color.LightYellow;
            this.btnPreview.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPreview.Location = new System.Drawing.Point(12, 420);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(100, 30);
            this.btnPreview.TabIndex = 4;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = false;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.LightGreen;
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(450, 420);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 30);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.LightCoral;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(540, 420);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FormFilters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 461);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnPreview);
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

        #endregion
    }
}
