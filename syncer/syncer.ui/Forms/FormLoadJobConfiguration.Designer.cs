namespace syncer.ui.Forms
{
    partial class FormLoadJobConfiguration
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
            this.components = new System.ComponentModel.Container();
            this.groupBoxSearch = new System.Windows.Forms.GroupBox();
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.labelSearch = new System.Windows.Forms.Label();
            this.comboBoxCategoryFilter = new System.Windows.Forms.ComboBox();
            this.labelCategoryFilter = new System.Windows.Forms.Label();
            this.checkBoxFavoritesOnly = new System.Windows.Forms.CheckBox();
            
            this.groupBoxConfigurations = new System.Windows.Forms.GroupBox();
            this.listViewConfigurations = new System.Windows.Forms.ListView();
            this.contextMenuConfigurations = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemLoad = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemLoadAndStart = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemSetAsDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAddToQuickLaunch = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemSetAsFavorite = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemExport = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemDuplicate = new System.Windows.Forms.ToolStripMenuItem();
            
            this.groupBoxDetails = new System.Windows.Forms.GroupBox();
            this.labelConfigName = new System.Windows.Forms.Label();
            this.labelConfigDescription = new System.Windows.Forms.Label();
            this.labelConfigCategory = new System.Windows.Forms.Label();
            this.labelConfigTags = new System.Windows.Forms.Label();
            this.labelConfigCreated = new System.Windows.Forms.Label();
            this.labelConfigLastUsed = new System.Windows.Forms.Label();
            this.labelConfigTimesUsed = new System.Windows.Forms.Label();
            this.textBoxConfigDetails = new System.Windows.Forms.TextBox();
            
            this.groupBoxQuickActions = new System.Windows.Forms.GroupBox();
            this.buttonLoadDefault = new System.Windows.Forms.Button();
            this.buttonLoadRecent = new System.Windows.Forms.Button();
            this.buttonLoadMostUsed = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.buttonExportAll = new System.Windows.Forms.Button();
            
            this.buttonLoad = new System.Windows.Forms.Button();
            this.buttonLoadAndStart = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            
            this.groupBoxSearch.SuspendLayout();
            this.groupBoxConfigurations.SuspendLayout();
            this.contextMenuConfigurations.SuspendLayout();
            this.groupBoxDetails.SuspendLayout();
            this.groupBoxQuickActions.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // groupBoxSearch
            // 
            this.groupBoxSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSearch.Controls.Add(this.labelSearch);
            this.groupBoxSearch.Controls.Add(this.textBoxSearch);
            this.groupBoxSearch.Controls.Add(this.labelCategoryFilter);
            this.groupBoxSearch.Controls.Add(this.comboBoxCategoryFilter);
            this.groupBoxSearch.Controls.Add(this.checkBoxFavoritesOnly);
            this.groupBoxSearch.Location = new System.Drawing.Point(12, 12);
            this.groupBoxSearch.Name = "groupBoxSearch";
            this.groupBoxSearch.Size = new System.Drawing.Size(760, 80);
            this.groupBoxSearch.TabIndex = 0;
            this.groupBoxSearch.TabStop = false;
            this.groupBoxSearch.Text = "Search & Filter";
            
            // 
            // labelSearch
            // 
            this.labelSearch.AutoSize = true;
            this.labelSearch.Location = new System.Drawing.Point(15, 25);
            this.labelSearch.Name = "labelSearch";
            this.labelSearch.Size = new System.Drawing.Size(44, 13);
            this.labelSearch.TabIndex = 0;
            this.labelSearch.Text = "Search:";
            
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Location = new System.Drawing.Point(70, 22);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(200, 20);
            this.textBoxSearch.TabIndex = 1;
            
            // 
            // labelCategoryFilter
            // 
            this.labelCategoryFilter.AutoSize = true;
            this.labelCategoryFilter.Location = new System.Drawing.Point(290, 25);
            this.labelCategoryFilter.Name = "labelCategoryFilter";
            this.labelCategoryFilter.Size = new System.Drawing.Size(52, 13);
            this.labelCategoryFilter.TabIndex = 2;
            this.labelCategoryFilter.Text = "Category:";
            
            // 
            // comboBoxCategoryFilter
            // 
            this.comboBoxCategoryFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCategoryFilter.FormattingEnabled = true;
            this.comboBoxCategoryFilter.Items.AddRange(new object[] {
            "(All Categories)"});
            this.comboBoxCategoryFilter.Location = new System.Drawing.Point(350, 22);
            this.comboBoxCategoryFilter.Name = "comboBoxCategoryFilter";
            this.comboBoxCategoryFilter.Size = new System.Drawing.Size(150, 21);
            this.comboBoxCategoryFilter.TabIndex = 3;
            
            // 
            // checkBoxFavoritesOnly
            // 
            this.checkBoxFavoritesOnly.AutoSize = true;
            this.checkBoxFavoritesOnly.Location = new System.Drawing.Point(520, 24);
            this.checkBoxFavoritesOnly.Name = "checkBoxFavoritesOnly";
            this.checkBoxFavoritesOnly.Size = new System.Drawing.Size(92, 17);
            this.checkBoxFavoritesOnly.TabIndex = 4;
            this.checkBoxFavoritesOnly.Text = "Favorites Only";
            this.checkBoxFavoritesOnly.UseVisualStyleBackColor = true;
            
            // 
            // groupBoxConfigurations
            // 
            this.groupBoxConfigurations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxConfigurations.Controls.Add(this.listViewConfigurations);
            this.groupBoxConfigurations.Location = new System.Drawing.Point(12, 100);
            this.groupBoxConfigurations.Name = "groupBoxConfigurations";
            this.groupBoxConfigurations.Size = new System.Drawing.Size(500, 350);
            this.groupBoxConfigurations.TabIndex = 1;
            this.groupBoxConfigurations.TabStop = false;
            this.groupBoxConfigurations.Text = "Saved Configurations";
            
            // 
            // listViewConfigurations
            // 
            this.listViewConfigurations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewConfigurations.ContextMenuStrip = this.contextMenuConfigurations;
            this.listViewConfigurations.FullRowSelect = true;
            this.listViewConfigurations.GridLines = true;
            this.listViewConfigurations.Location = new System.Drawing.Point(15, 25);
            this.listViewConfigurations.MultiSelect = false;
            this.listViewConfigurations.Name = "listViewConfigurations";
            this.listViewConfigurations.Size = new System.Drawing.Size(470, 310);
            this.listViewConfigurations.TabIndex = 0;
            this.listViewConfigurations.UseCompatibleStateImageBehavior = false;
            this.listViewConfigurations.View = System.Windows.Forms.View.Details;
            
            // 
            // contextMenuConfigurations
            // 
            this.contextMenuConfigurations.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemLoad,
            this.menuItemLoadAndStart,
            this.toolStripSeparator1,
            this.menuItemEdit,
            this.menuItemDelete,
            this.toolStripSeparator2,
            this.menuItemSetAsDefault,
            this.menuItemAddToQuickLaunch,
            this.menuItemSetAsFavorite,
            this.toolStripSeparator3,
            this.menuItemExport,
            this.menuItemDuplicate});
            this.contextMenuConfigurations.Name = "contextMenuConfigurations";
            this.contextMenuConfigurations.Size = new System.Drawing.Size(181, 236);
            
            // 
            // menuItemLoad
            // 
            this.menuItemLoad.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.menuItemLoad.Name = "menuItemLoad";
            this.menuItemLoad.Size = new System.Drawing.Size(180, 22);
            this.menuItemLoad.Text = "Load Configuration";
            
            // 
            // menuItemLoadAndStart
            // 
            this.menuItemLoadAndStart.Name = "menuItemLoadAndStart";
            this.menuItemLoadAndStart.Size = new System.Drawing.Size(180, 22);
            this.menuItemLoadAndStart.Text = "Load and Start Job";
            
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            
            // 
            // menuItemEdit
            // 
            this.menuItemEdit.Name = "menuItemEdit";
            this.menuItemEdit.Size = new System.Drawing.Size(180, 22);
            this.menuItemEdit.Text = "Edit Configuration";
            
            // 
            // menuItemDelete
            // 
            this.menuItemDelete.Name = "menuItemDelete";
            this.menuItemDelete.Size = new System.Drawing.Size(180, 22);
            this.menuItemDelete.Text = "Delete Configuration";
            
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            
            // 
            // menuItemSetAsDefault
            // 
            this.menuItemSetAsDefault.Name = "menuItemSetAsDefault";
            this.menuItemSetAsDefault.Size = new System.Drawing.Size(180, 22);
            this.menuItemSetAsDefault.Text = "Set as Default";
            
            // 
            // menuItemAddToQuickLaunch
            // 
            this.menuItemAddToQuickLaunch.Name = "menuItemAddToQuickLaunch";
            this.menuItemAddToQuickLaunch.Size = new System.Drawing.Size(180, 22);
            this.menuItemAddToQuickLaunch.Text = "Add to Quick Launch";
            
            // 
            // menuItemSetAsFavorite
            // 
            this.menuItemSetAsFavorite.Name = "menuItemSetAsFavorite";
            this.menuItemSetAsFavorite.Size = new System.Drawing.Size(180, 22);
            this.menuItemSetAsFavorite.Text = "Set as Favorite";
            
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            
            // 
            // menuItemExport
            // 
            this.menuItemExport.Name = "menuItemExport";
            this.menuItemExport.Size = new System.Drawing.Size(180, 22);
            this.menuItemExport.Text = "Export Configuration";
            
            // 
            // menuItemDuplicate
            // 
            this.menuItemDuplicate.Name = "menuItemDuplicate";
            this.menuItemDuplicate.Size = new System.Drawing.Size(180, 22);
            this.menuItemDuplicate.Text = "Duplicate Configuration";
            
            // 
            // groupBoxDetails
            // 
            this.groupBoxDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDetails.Controls.Add(this.labelConfigName);
            this.groupBoxDetails.Controls.Add(this.labelConfigDescription);
            this.groupBoxDetails.Controls.Add(this.labelConfigCategory);
            this.groupBoxDetails.Controls.Add(this.labelConfigTags);
            this.groupBoxDetails.Controls.Add(this.labelConfigCreated);
            this.groupBoxDetails.Controls.Add(this.labelConfigLastUsed);
            this.groupBoxDetails.Controls.Add(this.labelConfigTimesUsed);
            this.groupBoxDetails.Controls.Add(this.textBoxConfigDetails);
            this.groupBoxDetails.Location = new System.Drawing.Point(520, 100);
            this.groupBoxDetails.Name = "groupBoxDetails";
            this.groupBoxDetails.Size = new System.Drawing.Size(252, 350);
            this.groupBoxDetails.TabIndex = 2;
            this.groupBoxDetails.TabStop = false;
            this.groupBoxDetails.Text = "Configuration Details";
            
            // 
            // labelConfigName
            // 
            this.labelConfigName.AutoSize = true;
            this.labelConfigName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelConfigName.Location = new System.Drawing.Point(15, 25);
            this.labelConfigName.Name = "labelConfigName";
            this.labelConfigName.Size = new System.Drawing.Size(39, 13);
            this.labelConfigName.TabIndex = 0;
            this.labelConfigName.Text = "Name:";
            
            // 
            // labelConfigDescription
            // 
            this.labelConfigDescription.AutoSize = true;
            this.labelConfigDescription.Location = new System.Drawing.Point(15, 45);
            this.labelConfigDescription.Name = "labelConfigDescription";
            this.labelConfigDescription.Size = new System.Drawing.Size(63, 13);
            this.labelConfigDescription.TabIndex = 1;
            this.labelConfigDescription.Text = "Description:";
            
            // 
            // labelConfigCategory
            // 
            this.labelConfigCategory.AutoSize = true;
            this.labelConfigCategory.Location = new System.Drawing.Point(15, 65);
            this.labelConfigCategory.Name = "labelConfigCategory";
            this.labelConfigCategory.Size = new System.Drawing.Size(52, 13);
            this.labelConfigCategory.TabIndex = 2;
            this.labelConfigCategory.Text = "Category:";
            
            // 
            // labelConfigTags
            // 
            this.labelConfigTags.AutoSize = true;
            this.labelConfigTags.Location = new System.Drawing.Point(15, 85);
            this.labelConfigTags.Name = "labelConfigTags";
            this.labelConfigTags.Size = new System.Drawing.Size(34, 13);
            this.labelConfigTags.TabIndex = 3;
            this.labelConfigTags.Text = "Tags:";
            
            // 
            // labelConfigCreated
            // 
            this.labelConfigCreated.AutoSize = true;
            this.labelConfigCreated.Location = new System.Drawing.Point(15, 105);
            this.labelConfigCreated.Name = "labelConfigCreated";
            this.labelConfigCreated.Size = new System.Drawing.Size(47, 13);
            this.labelConfigCreated.TabIndex = 4;
            this.labelConfigCreated.Text = "Created:";
            
            // 
            // labelConfigLastUsed
            // 
            this.labelConfigLastUsed.AutoSize = true;
            this.labelConfigLastUsed.Location = new System.Drawing.Point(15, 125);
            this.labelConfigLastUsed.Name = "labelConfigLastUsed";
            this.labelConfigLastUsed.Size = new System.Drawing.Size(60, 13);
            this.labelConfigLastUsed.TabIndex = 5;
            this.labelConfigLastUsed.Text = "Last Used:";
            
            // 
            // labelConfigTimesUsed
            // 
            this.labelConfigTimesUsed.AutoSize = true;
            this.labelConfigTimesUsed.Location = new System.Drawing.Point(15, 145);
            this.labelConfigTimesUsed.Name = "labelConfigTimesUsed";
            this.labelConfigTimesUsed.Size = new System.Drawing.Size(67, 13);
            this.labelConfigTimesUsed.TabIndex = 6;
            this.labelConfigTimesUsed.Text = "Times Used:";
            
            // 
            // textBoxConfigDetails
            // 
            this.textBoxConfigDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxConfigDetails.Location = new System.Drawing.Point(15, 175);
            this.textBoxConfigDetails.Multiline = true;
            this.textBoxConfigDetails.Name = "textBoxConfigDetails";
            this.textBoxConfigDetails.ReadOnly = true;
            this.textBoxConfigDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxConfigDetails.Size = new System.Drawing.Size(220, 160);
            this.textBoxConfigDetails.TabIndex = 7;
            
            // 
            // groupBoxQuickActions
            // 
            this.groupBoxQuickActions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxQuickActions.Controls.Add(this.buttonLoadDefault);
            this.groupBoxQuickActions.Controls.Add(this.buttonLoadRecent);
            this.groupBoxQuickActions.Controls.Add(this.buttonLoadMostUsed);
            this.groupBoxQuickActions.Controls.Add(this.buttonImport);
            this.groupBoxQuickActions.Controls.Add(this.buttonExportAll);
            this.groupBoxQuickActions.Location = new System.Drawing.Point(12, 460);
            this.groupBoxQuickActions.Name = "groupBoxQuickActions";
            this.groupBoxQuickActions.Size = new System.Drawing.Size(500, 60);
            this.groupBoxQuickActions.TabIndex = 3;
            this.groupBoxQuickActions.TabStop = false;
            this.groupBoxQuickActions.Text = "Quick Actions";
            
            // 
            // buttonLoadDefault
            // 
            this.buttonLoadDefault.Location = new System.Drawing.Point(15, 25);
            this.buttonLoadDefault.Name = "buttonLoadDefault";
            this.buttonLoadDefault.Size = new System.Drawing.Size(85, 23);
            this.buttonLoadDefault.TabIndex = 0;
            this.buttonLoadDefault.Text = "Load Default";
            this.buttonLoadDefault.UseVisualStyleBackColor = true;
            
            // 
            // buttonLoadRecent
            // 
            this.buttonLoadRecent.Location = new System.Drawing.Point(110, 25);
            this.buttonLoadRecent.Name = "buttonLoadRecent";
            this.buttonLoadRecent.Size = new System.Drawing.Size(85, 23);
            this.buttonLoadRecent.TabIndex = 1;
            this.buttonLoadRecent.Text = "Load Recent";
            this.buttonLoadRecent.UseVisualStyleBackColor = true;
            
            // 
            // buttonLoadMostUsed
            // 
            this.buttonLoadMostUsed.Location = new System.Drawing.Point(205, 25);
            this.buttonLoadMostUsed.Name = "buttonLoadMostUsed";
            this.buttonLoadMostUsed.Size = new System.Drawing.Size(85, 23);
            this.buttonLoadMostUsed.TabIndex = 2;
            this.buttonLoadMostUsed.Text = "Most Used";
            this.buttonLoadMostUsed.UseVisualStyleBackColor = true;
            
            // 
            // buttonImport
            // 
            this.buttonImport.Location = new System.Drawing.Point(320, 25);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(75, 23);
            this.buttonImport.TabIndex = 3;
            this.buttonImport.Text = "Import";
            this.buttonImport.UseVisualStyleBackColor = true;
            
            // 
            // buttonExportAll
            // 
            this.buttonExportAll.Location = new System.Drawing.Point(405, 25);
            this.buttonExportAll.Name = "buttonExportAll";
            this.buttonExportAll.Size = new System.Drawing.Size(75, 23);
            this.buttonExportAll.TabIndex = 4;
            this.buttonExportAll.Text = "Export All";
            this.buttonExportAll.UseVisualStyleBackColor = true;
            
            // 
            // buttonLoad
            // 
            this.buttonLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLoad.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.buttonLoad.Location = new System.Drawing.Point(520, 460);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(100, 30);
            this.buttonLoad.TabIndex = 4;
            this.buttonLoad.Text = "Load";
            this.buttonLoad.UseVisualStyleBackColor = true;
            
            // 
            // buttonLoadAndStart
            // 
            this.buttonLoadAndStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLoadAndStart.Location = new System.Drawing.Point(630, 460);
            this.buttonLoadAndStart.Name = "buttonLoadAndStart";
            this.buttonLoadAndStart.Size = new System.Drawing.Size(100, 30);
            this.buttonLoadAndStart.TabIndex = 5;
            this.buttonLoadAndStart.Text = "Load && Start";
            this.buttonLoadAndStart.UseVisualStyleBackColor = true;
            
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(630, 500);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRefresh.Location = new System.Drawing.Point(540, 500);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(75, 23);
            this.buttonRefresh.TabIndex = 7;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            
            // 
            // FormLoadJobConfiguration
            // 
            this.AcceptButton = this.buttonLoad;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(784, 535);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonLoadAndStart);
            this.Controls.Add(this.buttonLoad);
            this.Controls.Add(this.groupBoxQuickActions);
            this.Controls.Add(this.groupBoxDetails);
            this.Controls.Add(this.groupBoxConfigurations);
            this.Controls.Add(this.groupBoxSearch);
            this.MinimumSize = new System.Drawing.Size(800, 570);
            this.Name = "FormLoadJobConfiguration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Load Job Configuration";
            this.groupBoxSearch.ResumeLayout(false);
            this.groupBoxSearch.PerformLayout();
            this.groupBoxConfigurations.ResumeLayout(false);
            this.contextMenuConfigurations.ResumeLayout(false);
            this.groupBoxDetails.ResumeLayout(false);
            this.groupBoxDetails.PerformLayout();
            this.groupBoxQuickActions.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxSearch;
        private System.Windows.Forms.Label labelSearch;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.Label labelCategoryFilter;
        private System.Windows.Forms.ComboBox comboBoxCategoryFilter;
        private System.Windows.Forms.CheckBox checkBoxFavoritesOnly;
        
        private System.Windows.Forms.GroupBox groupBoxConfigurations;
        private System.Windows.Forms.ListView listViewConfigurations;
        private System.Windows.Forms.ContextMenuStrip contextMenuConfigurations;
        private System.Windows.Forms.ToolStripMenuItem menuItemLoad;
        private System.Windows.Forms.ToolStripMenuItem menuItemLoadAndStart;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuItemEdit;
        private System.Windows.Forms.ToolStripMenuItem menuItemDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuItemSetAsDefault;
        private System.Windows.Forms.ToolStripMenuItem menuItemAddToQuickLaunch;
        private System.Windows.Forms.ToolStripMenuItem menuItemSetAsFavorite;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem menuItemExport;
        private System.Windows.Forms.ToolStripMenuItem menuItemDuplicate;
        
        private System.Windows.Forms.GroupBox groupBoxDetails;
        private System.Windows.Forms.Label labelConfigName;
        private System.Windows.Forms.Label labelConfigDescription;
        private System.Windows.Forms.Label labelConfigCategory;
        private System.Windows.Forms.Label labelConfigTags;
        private System.Windows.Forms.Label labelConfigCreated;
        private System.Windows.Forms.Label labelConfigLastUsed;
        private System.Windows.Forms.Label labelConfigTimesUsed;
        private System.Windows.Forms.TextBox textBoxConfigDetails;
        
        private System.Windows.Forms.GroupBox groupBoxQuickActions;
        private System.Windows.Forms.Button buttonLoadDefault;
        private System.Windows.Forms.Button buttonLoadRecent;
        private System.Windows.Forms.Button buttonLoadMostUsed;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.Button buttonExportAll;
        
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.Button buttonLoadAndStart;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonRefresh;
    }
}
