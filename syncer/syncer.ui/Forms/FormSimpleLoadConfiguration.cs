using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using syncer.ui.Interfaces;

namespace syncer.ui.Forms
{
    /// <summary>
    /// Enhanced simplified form for loading and managing saved job configurations
    /// Features: Load, Load & Start, Edit, Delete, Export, Import, and detailed configuration info
    /// </summary>
    public partial class FormSimpleLoadConfiguration : Form
    {
        private ISavedJobConfigurationService _configService;
        private List<SavedJobConfiguration> _configurations;
        
        public SavedJobConfiguration SelectedConfiguration { get; private set; }
        public bool LoadAndStart { get; private set; }

        public FormSimpleLoadConfiguration(ISavedJobConfigurationService configService)
        {
            InitializeComponent();
            
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            
            InitializeForm();
            LoadConfigurations();
        }

        private void InitializeForm()
        {
            this.Text = "Configuration Manager";
            // Don't override the designer size - let the designer settings take precedence
            // this.Size = new Size(650, 430);  // Commented out to use designer size
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Use Font mode for better compatibility with .NET Framework 3.5
            this.AutoScaleMode = AutoScaleMode.Font;
            // Set consistent font to match other forms
            this.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular);
            
            // Setup event handlers
            this.Load += FormSimpleLoadConfiguration_Load;
        }

        private void FormSimpleLoadConfiguration_Load(object sender, EventArgs e)
        {
            try
            {
                // Always enable import button
                btnImport.Enabled = true;
                
                RefreshConfigurationsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading configurations: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadConfigurations()
        {
            try
            {
                _configurations = _configService.GetAllConfigurations();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading configurations: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _configurations = new List<SavedJobConfiguration>();
            }
        }

        private void RefreshConfigurationsList()
        {
            try
            {
                LoadConfigurations();
                
                listBoxConfigurations.Items.Clear();
                
                if (_configurations == null || _configurations.Count == 0)
                {
                    listBoxConfigurations.Items.Add("No saved configurations found");
                    btnLoadAndStart.Enabled = false;
                    btnEdit.Enabled = false;
                    btnDelete.Enabled = false;
                    btnExport.Enabled = false;
                    return;
                }

                // Add configurations to listbox
                foreach (var config in _configurations.OrderBy(c => c.Name))
                {
                    listBoxConfigurations.Items.Add(config);
                }
                
                // Enable import button (always available)
                btnImport.Enabled = true;
                
                // Other buttons will be enabled when a selection is made
                btnLoadAndStart.Enabled = false;
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
                btnExport.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing configurations: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateConfigurationDetails()
        {
            if (listBoxConfigurations.SelectedItem == null || 
                !(listBoxConfigurations.SelectedItem is SavedJobConfiguration))
            {
                lblConfigName.Text = "Name: ";
                lblDescription.Text = "Description: ";
                lblCategory.Text = "Category: ";
                lblCreated.Text = "Created: ";
                lblLastUsed.Text = "Last Used: ";
                lblTimesUsed.Text = "Times Used: ";
                lblSourcePath.Text = "Source: ";
                lblDestinationPath.Text = "Destination: ";
                lblSourceConnection.Text = "Source Connection: ";
                lblDestinationConnection.Text = "Destination Connection: ";
                lblSourceFilePath.Text = "File Location: ";
                
                // Disable action buttons
                btnLoadAndStart.Enabled = false;
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
                btnExport.Enabled = false;
                return;
            }

            var config = (SavedJobConfiguration)listBoxConfigurations.SelectedItem;
            
            lblConfigName.Text = "Name: " + (config.Name ?? "N/A");
            lblDescription.Text = "Description: " + (config.Description ?? "N/A");
            lblCategory.Text = "Category: " + (config.Category ?? "General");
            lblCreated.Text = "Created: " + config.CreatedDate.ToString("yyyy-MM-dd HH:mm");
            lblLastUsed.Text = "Last Used: " + (config.LastUsed != null ? config.LastUsed.Value.ToString("yyyy-MM-dd HH:mm") : "Never");
            lblTimesUsed.Text = "Times Used: " + config.TimesUsed.ToString();
            
            // Show job settings if available
            if (config.JobSettings != null)
            {
                lblSourcePath.Text = "Source: " + (config.JobSettings.SourcePath ?? "Not set");
                lblDestinationPath.Text = "Destination: " + (config.JobSettings.DestinationPath ?? "Not set");
            }
            else
            {
                lblSourcePath.Text = "Source: Not configured";
                lblDestinationPath.Text = "Destination: Not configured";
            }
            
            // Show connection settings if available
            if (config.SourceConnection != null && config.SourceConnection.Settings != null)
            {
                string sourceInfo = "Type: " + config.SourceConnection.Settings.ConnectionTypeDisplay;
                if (!string.IsNullOrEmpty(config.SourceConnection.Settings.Host))
                {
                    sourceInfo += ", Host: " + config.SourceConnection.Settings.Host;
                    if (config.SourceConnection.Settings.Port > 0)
                    {
                        sourceInfo += ":" + config.SourceConnection.Settings.Port.ToString();
                    }
                }
                lblSourceConnection.Text = "Source Connection: " + sourceInfo;
            }
            else
            {
                lblSourceConnection.Text = "Source Connection: Local/Not configured";
            }
            
            if (config.DestinationConnection != null && config.DestinationConnection.Settings != null)
            {
                string destInfo = "Type: " + config.DestinationConnection.Settings.ConnectionTypeDisplay;
                if (!string.IsNullOrEmpty(config.DestinationConnection.Settings.Host))
                {
                    destInfo += ", Host: " + config.DestinationConnection.Settings.Host;
                    if (config.DestinationConnection.Settings.Port > 0)
                    {
                        destInfo += ":" + config.DestinationConnection.Settings.Port.ToString();
                    }
                }
                lblDestinationConnection.Text = "Destination Connection: " + destInfo;
            }
            else
            {
                lblDestinationConnection.Text = "Destination Connection: Local/Not configured";
            }
            
            // Show file path information
            if (!string.IsNullOrEmpty(config.SourceFilePath))
            {
                lblSourceFilePath.Text = "File Location: " + config.SourceFilePath;
            }
            else
            {
                // Show the local storage path for configurations created within the application
                string storagePath = _configService.GetConfigurationStoragePath();
                lblSourceFilePath.Text = "File Location: " + Path.Combine(storagePath, "configurations.json") + " (Local Storage)";
            }
            
            // Enable action buttons
            btnLoadAndStart.Enabled = true;
            btnEdit.Enabled = true;
            btnDelete.Enabled = true;
            btnExport.Enabled = true;
        }

        #region Event Handlers

        private void listBoxConfigurations_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateConfigurationDetails();
        }

        private void listBoxConfigurations_DoubleClick(object sender, EventArgs e)
        {
            // Double click to load and start
            LoadConfiguration(true);
        }

        private void btnLoadAndStart_Click(object sender, EventArgs e)
        {
            LoadConfiguration(true);
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadConfiguration(false);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBoxConfigurations.SelectedItem == null || 
                    !(listBoxConfigurations.SelectedItem is SavedJobConfiguration))
                {
                    MessageBox.Show("Please select a configuration to edit.", "No Selection", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedConfig = (SavedJobConfiguration)listBoxConfigurations.SelectedItem;
                
                using (var editForm = new FormEditConfiguration(selectedConfig))
                {
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        RefreshConfigurationsList();
                        MessageBox.Show("Configuration updated successfully!", "Edit Configuration", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error editing configuration: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBoxConfigurations.SelectedItem == null || 
                    !(listBoxConfigurations.SelectedItem is SavedJobConfiguration))
                {
                    MessageBox.Show("Please select a configuration to delete.", "No Selection", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedConfig = (SavedJobConfiguration)listBoxConfigurations.SelectedItem;
                
                var result = MessageBox.Show(
                    "Are you sure you want to delete the configuration '" + selectedConfig.Name + "'?\n\nThis action cannot be undone.", 
                    "Confirm Delete", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    if (_configService.DeleteConfiguration(selectedConfig.Id))
                    {
                        RefreshConfigurationsList();
                        MessageBox.Show("Configuration deleted successfully!", "Delete Configuration", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete configuration.", "Delete Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting configuration: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBoxConfigurations.SelectedItem == null || 
                    !(listBoxConfigurations.SelectedItem is SavedJobConfiguration))
                {
                    MessageBox.Show("Please select a configuration to export.", "No Selection", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedConfig = (SavedJobConfiguration)listBoxConfigurations.SelectedItem;
                
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    saveDialog.Title = "Export Configuration";
                    saveDialog.FileName = selectedConfig.Name + ".json";
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (_configService.ExportConfiguration(selectedConfig.Id, saveDialog.FileName))
                        {
                            MessageBox.Show("Configuration exported successfully!", "Export Configuration", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to export configuration.", "Export Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting configuration: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    openDialog.Title = "Import Configuration";
                    
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        var importedConfig = _configService.ImportConfiguration(openDialog.FileName);
                        
                        if (importedConfig != null)
                        {
                            RefreshConfigurationsList();
                            MessageBox.Show("Configuration imported successfully!\n\nName: " + importedConfig.Name, 
                                "Import Configuration", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to import configuration. Please check the file format.", 
                                "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importing configuration: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void LoadConfiguration(bool loadAndStart)
        {
            try
            {
                if (listBoxConfigurations.SelectedItem == null || 
                    !(listBoxConfigurations.SelectedItem is SavedJobConfiguration))
                {
                    MessageBox.Show("Please select a configuration to load.", "No Selection", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SelectedConfiguration = (SavedJobConfiguration)listBoxConfigurations.SelectedItem;
                LoadAndStart = loadAndStart;

                // Update usage statistics
                _configService.UpdateUsageStatistics(SelectedConfiguration.Id);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading configuration: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }
    }
}
