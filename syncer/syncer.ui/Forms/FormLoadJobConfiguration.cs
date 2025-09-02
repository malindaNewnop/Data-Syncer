using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using syncer.ui.Interfaces;

namespace syncer.ui.Forms
{
    /// <summary>
    /// Form for loading and managing saved job configurations
    /// </summary>
    public partial class FormLoadJobConfiguration : Form
    {
        private ISavedJobConfigurationService _configService;
        private IConnectionService _connectionService;
        private ITimerJobManager _timerJobManager;
        private List<SavedJobConfiguration> _allConfigurations;
        private List<SavedJobConfiguration> _filteredConfigurations;
        private SavedJobConfiguration _selectedConfiguration;
        
        public SavedJobConfiguration LoadedConfiguration { get; private set; }
        public bool StartJobAfterLoad { get; private set; }
        public SavedJobConfiguration SelectedConfiguration => _selectedConfiguration;

        public FormLoadJobConfiguration(
            ISavedJobConfigurationService configService,
            IConnectionService connectionService,
            ITimerJobManager timerJobManager)
        {
            InitializeComponent();
            
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
            _timerJobManager = timerJobManager ?? throw new ArgumentNullException(nameof(timerJobManager));

            InitializeForm();
        }

        private void InitializeForm()
        {
            // Setup event handlers
            this.Load += FormLoadJobConfiguration_Load;
            this.textBoxSearch.TextChanged += FilterConfigurations;
            this.comboBoxCategoryFilter.SelectedIndexChanged += FilterConfigurations;
            this.checkBoxFavoritesOnly.CheckedChanged += FilterConfigurations;
            this.listViewConfigurations.SelectedIndexChanged += ListViewConfigurations_SelectedIndexChanged;
            this.listViewConfigurations.DoubleClick += ListViewConfigurations_DoubleClick;
            
            // Setup button event handlers
            this.buttonLoad.Click += ButtonLoad_Click;
            this.buttonLoadAndStart.Click += ButtonLoadAndStart_Click;
            this.buttonRefresh.Click += ButtonRefresh_Click;
            this.buttonLoadDefault.Click += ButtonLoadDefault_Click;
            this.buttonLoadRecent.Click += ButtonLoadRecent_Click;
            this.buttonLoadMostUsed.Click += ButtonLoadMostUsed_Click;
            this.buttonImport.Click += ButtonImport_Click;
            this.buttonExportAll.Click += ButtonExportAll_Click;
            
            // Setup context menu event handlers
            this.menuItemLoad.Click += MenuItemLoad_Click;
            this.menuItemLoadAndStart.Click += MenuItemLoadAndStart_Click;
            this.menuItemEdit.Click += MenuItemEdit_Click;
            this.menuItemDelete.Click += MenuItemDelete_Click;
            this.menuItemSetAsDefault.Click += MenuItemSetAsDefault_Click;
            this.menuItemAddToQuickLaunch.Click += MenuItemAddToQuickLaunch_Click;
            this.menuItemSetAsFavorite.Click += MenuItemSetAsFavorite_Click;
            this.menuItemExport.Click += MenuItemExport_Click;
            this.menuItemDuplicate.Click += MenuItemDuplicate_Click;
            
            // Setup list view columns
            SetupListView();
            
            // Initialize collections
            _allConfigurations = new List<SavedJobConfiguration>();
            _filteredConfigurations = new List<SavedJobConfiguration>();
        }

        private void FormLoadJobConfiguration_Load(object sender, EventArgs e)
        {
            LoadConfigurations();
            LoadCategories();
            UpdateButtonStates();
        }

        private void SetupListView()
        {
            listViewConfigurations.Columns.Clear();
            listViewConfigurations.Columns.Add("Name", 150);
            listViewConfigurations.Columns.Add("Category", 100);
            listViewConfigurations.Columns.Add("Description", 200);
            listViewConfigurations.Columns.Add("Created", 100);
            listViewConfigurations.Columns.Add("Last Used", 100);
            listViewConfigurations.Columns.Add("Times Used", 80);
            listViewConfigurations.Columns.Add("Default", 60);
        }

        private void LoadConfigurations()
        {
            try
            {
                _allConfigurations = _configService.GetAllConfigurations();
                FilterConfigurations(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configurations: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _allConfigurations.Clear();
                _filteredConfigurations.Clear();
                PopulateListView();
            }
        }

        private void LoadCategories()
        {
            try
            {
                var categories = _configService.GetAllCategories();
                comboBoxCategoryFilter.Items.Clear();
                comboBoxCategoryFilter.Items.Add("(All Categories)");
                
                foreach (var category in categories)
                {
                    comboBoxCategoryFilter.Items.Add(category);
                }
                
                comboBoxCategoryFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FilterConfigurations(object sender, EventArgs e)
        {
            try
            {
                _filteredConfigurations = _allConfigurations.ToList();
                
                // Apply search filter
                var searchTerm = textBoxSearch.Text.Trim();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    _filteredConfigurations = _configService.SearchConfigurations(searchTerm);
                }
                
                // Apply category filter
                var selectedCategory = comboBoxCategoryFilter.Text;
                if (!string.IsNullOrEmpty(selectedCategory) && selectedCategory != "(All Categories)")
                {
                    _filteredConfigurations = _filteredConfigurations
                        .Where(c => string.Equals(c.Category, selectedCategory, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
                
                // Apply favorites filter
                if (checkBoxFavoritesOnly.Checked)
                {
                    var favoriteItems = _configService.GetQuickLaunchItems()
                        .Where(q => q.IsFavorite)
                        .Select(q => q.ConfigurationId)
                        .ToList();
                        
                    _filteredConfigurations = _filteredConfigurations
                        .Where(c => favoriteItems.Contains(c.Id))
                        .ToList();
                }
                
                PopulateListView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering configurations: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateListView()
        {
            listViewConfigurations.Items.Clear();
            
            foreach (var config in _filteredConfigurations)
            {
                var item = new ListViewItem(config.DisplayName);
                item.SubItems.Add(config.Category ?? "");
                item.SubItems.Add(config.FormattedDescription);
                item.SubItems.Add(config.CreatedDate.ToString("yyyy-MM-dd"));
                item.SubItems.Add(config.LastUsed?.ToString("yyyy-MM-dd") ?? "Never");
                item.SubItems.Add(config.TimesUsed.ToString());
                item.SubItems.Add(config.IsDefault ? "Yes" : "");
                item.Tag = config;
                
                // Highlight default configuration
                if (config.IsDefault)
                {
                    item.Font = new System.Drawing.Font(item.Font, System.Drawing.FontStyle.Bold);
                }
                
                listViewConfigurations.Items.Add(item);
            }
            
            UpdateButtonStates();
        }

        private void ListViewConfigurations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewConfigurations.SelectedItems.Count > 0)
            {
                _selectedConfiguration = listViewConfigurations.SelectedItems[0].Tag as SavedJobConfiguration;
                ShowConfigurationDetails(_selectedConfiguration);
            }
            else
            {
                _selectedConfiguration = null;
                ClearConfigurationDetails();
            }
            
            UpdateButtonStates();
        }

        private void ListViewConfigurations_DoubleClick(object sender, EventArgs e)
        {
            if (_selectedConfiguration != null)
            {
                LoadConfiguration(false);
            }
        }

        private void ShowConfigurationDetails(SavedJobConfiguration config)
        {
            if (config == null)
            {
                ClearConfigurationDetails();
                return;
            }
            
            labelConfigName.Text = $"Name: {config.DisplayName}";
            labelConfigDescription.Text = $"Description: {config.FormattedDescription}";
            labelConfigCategory.Text = $"Category: {config.Category ?? "None"}";
            labelConfigTags.Text = $"Tags: {(config.Tags != null && config.Tags.Count > 0 ? string.Join(", ", config.Tags.ToArray()) : "None")}";
            labelConfigCreated.Text = $"Created: {config.CreatedDate:yyyy-MM-dd HH:mm}";
            labelConfigLastUsed.Text = $"Last Used: {(config.LastUsed?.ToString("yyyy-MM-dd HH:mm") ?? "Never")}";
            labelConfigTimesUsed.Text = $"Times Used: {config.TimesUsed}";
            
            // Show detailed configuration information
            var details = GetConfigurationDetails(config);
            textBoxConfigDetails.Text = details;
        }

        private void ClearConfigurationDetails()
        {
            labelConfigName.Text = "Name:";
            labelConfigDescription.Text = "Description:";
            labelConfigCategory.Text = "Category:";
            labelConfigTags.Text = "Tags:";
            labelConfigCreated.Text = "Created:";
            labelConfigLastUsed.Text = "Last Used:";
            labelConfigTimesUsed.Text = "Times Used:";
            textBoxConfigDetails.Text = "";
        }

        private string GetConfigurationDetails(SavedJobConfiguration config)
        {
            var details = "=== JOB SETTINGS ===\n";
            if (config.JobSettings != null)
            {
                details += $"Job Name: {config.JobSettings.Name}\n";
                details += $"Source Path: {config.JobSettings.SourcePath}\n";
                details += $"Destination Path: {config.JobSettings.DestinationPath}\n";
                details += $"Interval: {config.JobSettings.IntervalValue} {config.JobSettings.IntervalType}\n";
                details += $"Transfer Mode: {config.JobSettings.TransferMode}\n";
                details += $"Include Subfolders: {config.JobSettings.IncludeSubFolders}\n";
                details += $"Overwrite Existing: {config.JobSettings.OverwriteExisting}\n";
            }
            
            details += "\n=== SOURCE CONNECTION ===\n";
            if (config.SourceConnection?.Settings != null)
            {
                details += GetConnectionDetails(config.SourceConnection.Settings);
            }
            
            details += "\n=== DESTINATION CONNECTION ===\n";
            if (config.DestinationConnection?.Settings != null)
            {
                details += GetConnectionDetails(config.DestinationConnection.Settings);
            }
            
            details += "\n=== OPTIONS ===\n";
            details += $"Auto Start on Load: {config.AutoStartOnLoad}\n";
            details += $"Show Notification: {config.ShowNotificationOnStart}\n";
            details += $"Quick Launch Enabled: {config.EnableQuickLaunch}\n";
            
            return details;
        }

        private string GetConnectionDetails(ConnectionSettings connection)
        {
            var details = "";
            details += $"Type: {connection.ConnectionTypeDisplay}\n";
            
            if (!connection.IsLocalConnection)
            {
                details += $"Host: {connection.Host}\n";
                details += $"Port: {connection.Port}\n";
                details += $"Username: {connection.Username}\n";
                details += $"Protocol: {connection.Protocol}\n";
            }
            
            return details;
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = _selectedConfiguration != null;
            bool hasConfigurations = _allConfigurations.Count > 0;
            
            buttonLoad.Enabled = hasSelection;
            buttonLoadAndStart.Enabled = hasSelection;
            buttonLoadDefault.Enabled = _configService.GetDefaultConfiguration() != null;
            buttonLoadRecent.Enabled = hasConfigurations;
            buttonLoadMostUsed.Enabled = hasConfigurations;
            buttonExportAll.Enabled = hasConfigurations;
            
            // Context menu items
            menuItemLoad.Enabled = hasSelection;
            menuItemLoadAndStart.Enabled = hasSelection;
            menuItemEdit.Enabled = hasSelection;
            menuItemDelete.Enabled = hasSelection;
            menuItemSetAsDefault.Enabled = hasSelection;
            menuItemAddToQuickLaunch.Enabled = hasSelection;
            menuItemSetAsFavorite.Enabled = hasSelection;
            menuItemExport.Enabled = hasSelection;
            menuItemDuplicate.Enabled = hasSelection;
        }

        #region Button Event Handlers

        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            LoadConfiguration(false);
        }

        private void ButtonLoadAndStart_Click(object sender, EventArgs e)
        {
            LoadConfiguration(true);
        }

        private void ButtonRefresh_Click(object sender, EventArgs e)
        {
            LoadConfigurations();
            LoadCategories();
        }

        private void ButtonLoadDefault_Click(object sender, EventArgs e)
        {
            var defaultConfig = _configService.GetDefaultConfiguration();
            if (defaultConfig != null)
            {
                LoadSpecificConfiguration(defaultConfig, false);
            }
            else
            {
                MessageBox.Show("No default configuration is set.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ButtonLoadRecent_Click(object sender, EventArgs e)
        {
            var recentConfigs = _configService.GetRecentlyUsedConfigurations(1);
            if (recentConfigs.Count > 0)
            {
                LoadSpecificConfiguration(recentConfigs[0], false);
            }
            else
            {
                MessageBox.Show("No recently used configurations found.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ButtonLoadMostUsed_Click(object sender, EventArgs e)
        {
            var mostUsedConfigs = _configService.GetMostUsedConfigurations(1);
            if (mostUsedConfigs.Count > 0)
            {
                LoadSpecificConfiguration(mostUsedConfigs[0], false);
            }
            else
            {
                MessageBox.Show("No frequently used configurations found.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ButtonImport_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                openFileDialog.Title = "Import Configuration";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var importedConfigs = _configService.ImportMultipleConfigurations(openFileDialog.FileName);
                        if (importedConfigs.Count > 0)
                        {
                            MessageBox.Show($"Successfully imported {importedConfigs.Count} configuration(s).", 
                                "Import Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadConfigurations();
                        }
                        else
                        {
                            MessageBox.Show("No configurations were imported. Please check the file format.", 
                                "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error importing configurations: {ex.Message}", "Import Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ButtonExportAll_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JSON Files (*.json)|*.json";
                saveFileDialog.Title = "Export All Configurations";
                saveFileDialog.FileName = $"JobConfigurations_{DateTime.Now:yyyyMMdd}.json";
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (_configService.ExportAllConfigurations(saveFileDialog.FileName))
                        {
                            MessageBox.Show("All configurations exported successfully.", "Export Successful", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to export configurations.", "Export Failed", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting configurations: {ex.Message}", "Export Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        #endregion

        #region Context Menu Event Handlers

        private void MenuItemLoad_Click(object sender, EventArgs e)
        {
            LoadConfiguration(false);
        }

        private void MenuItemLoadAndStart_Click(object sender, EventArgs e)
        {
            LoadConfiguration(true);
        }

        private void MenuItemEdit_Click(object sender, EventArgs e)
        {
            if (_selectedConfiguration != null)
            {
                // This would open an edit form - for now just show a message
                MessageBox.Show("Edit functionality would be implemented here.", "Edit Configuration", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void MenuItemDelete_Click(object sender, EventArgs e)
        {
            if (_selectedConfiguration != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the configuration '{_selectedConfiguration.DisplayName}'?", 
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        if (_configService.DeleteConfiguration(_selectedConfiguration.Id))
                        {
                            MessageBox.Show("Configuration deleted successfully.", "Delete Successful", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadConfigurations();
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete configuration.", "Delete Failed", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting configuration: {ex.Message}", "Delete Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void MenuItemSetAsDefault_Click(object sender, EventArgs e)
        {
            if (_selectedConfiguration != null)
            {
                try
                {
                    if (_configService.SetDefaultConfiguration(_selectedConfiguration.Id))
                    {
                        MessageBox.Show("Configuration set as default successfully.", "Default Set", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadConfigurations();
                    }
                    else
                    {
                        MessageBox.Show("Failed to set configuration as default.", "Set Default Failed", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error setting default configuration: {ex.Message}", "Set Default Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MenuItemAddToQuickLaunch_Click(object sender, EventArgs e)
        {
            if (_selectedConfiguration != null)
            {
                try
                {
                    var quickLaunchItem = new QuickLaunchItem
                    {
                        ConfigurationId = _selectedConfiguration.Id,
                        DisplayName = _selectedConfiguration.DisplayName,
                        Description = _selectedConfiguration.FormattedDescription,
                        IsFavorite = false,
                        SortOrder = 0
                    };
                    
                    if (_configService.AddToQuickLaunch(_selectedConfiguration.Id, quickLaunchItem))
                    {
                        MessageBox.Show("Configuration added to quick launch successfully.", "Added to Quick Launch", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to add configuration to quick launch.", "Add to Quick Launch Failed", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding to quick launch: {ex.Message}", "Quick Launch Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MenuItemSetAsFavorite_Click(object sender, EventArgs e)
        {
            if (_selectedConfiguration != null)
            {
                try
                {
                    if (_configService.SetQuickLaunchFavorite(_selectedConfiguration.Id, true))
                    {
                        MessageBox.Show("Configuration marked as favorite successfully.", "Favorite Set", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to mark configuration as favorite.", "Set Favorite Failed", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error setting favorite: {ex.Message}", "Favorite Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MenuItemExport_Click(object sender, EventArgs e)
        {
            if (_selectedConfiguration != null)
            {
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "JSON Files (*.json)|*.json";
                    saveFileDialog.Title = "Export Configuration";
                    saveFileDialog.FileName = $"{_selectedConfiguration.Name}_{DateTime.Now:yyyyMMdd}.json";
                    
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            if (_configService.ExportConfiguration(_selectedConfiguration.Id, saveFileDialog.FileName))
                            {
                                MessageBox.Show("Configuration exported successfully.", "Export Successful", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Failed to export configuration.", "Export Failed", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error exporting configuration: {ex.Message}", "Export Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void MenuItemDuplicate_Click(object sender, EventArgs e)
        {
            if (_selectedConfiguration != null)
            {
                try
                {
                    var duplicateConfig = CloneConfiguration(_selectedConfiguration);
                    duplicateConfig.Name = $"{_selectedConfiguration.Name} (Copy)";
                    duplicateConfig.Id = Guid.NewGuid().ToString();
                    duplicateConfig.CreatedDate = DateTime.Now;
                    duplicateConfig.LastUsed = null;
                    duplicateConfig.TimesUsed = 0;
                    duplicateConfig.IsDefault = false;
                    
                    if (_configService.SaveConfiguration(duplicateConfig))
                    {
                        MessageBox.Show("Configuration duplicated successfully.", "Duplicate Successful", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadConfigurations();
                    }
                    else
                    {
                        MessageBox.Show("Failed to duplicate configuration.", "Duplicate Failed", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error duplicating configuration: {ex.Message}", "Duplicate Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Helper Methods

        private void LoadConfiguration(bool startAfterLoad)
        {
            if (_selectedConfiguration != null)
            {
                LoadSpecificConfiguration(_selectedConfiguration, startAfterLoad);
            }
        }

        private void LoadSpecificConfiguration(SavedJobConfiguration config, bool startAfterLoad)
        {
            try
            {
                LoadedConfiguration = config;
                StartJobAfterLoad = startAfterLoad;
                
                // Update usage statistics
                _configService.UpdateUsageStatistics(config.Id);
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Load Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private SavedJobConfiguration CloneConfiguration(SavedJobConfiguration original)
        {
            // Create a deep copy of the configuration
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(original);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<SavedJobConfiguration>(json);
        }

        #endregion
    }
}
