using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace syncer.ui.Forms
{
    /// <summary>
    /// Form for saving job configurations with connection settings
    /// </summary>
    public partial class FormSaveJobConfiguration : Form
    {
        private SyncJob _currentJob;
        private ConnectionSettings _sourceConnection;
        private ConnectionSettings _destinationConnection;
        private ISavedJobConfigurationService _configService;
        private IConnectionService _connectionService;
        
        public SavedJobConfiguration SavedConfiguration { get; private set; }

        // .NET 3.5 compatibility helper
        private static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim().Length == 0;
        }

        public FormSaveJobConfiguration(
            SyncJob currentJob, 
            ConnectionSettings sourceConnection, 
            ConnectionSettings destinationConnection,
            ISavedJobConfigurationService configService,
            IConnectionService connectionService)
        {
            InitializeComponent();
            
            _currentJob = currentJob ?? throw new ArgumentNullException(nameof(currentJob));
            _sourceConnection = sourceConnection ?? throw new ArgumentNullException(nameof(sourceConnection));
            _destinationConnection = destinationConnection ?? throw new ArgumentNullException(nameof(destinationConnection));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));

            InitializeForm();
        }

        private void InitializeForm()
        {
            // Setup event handlers
            this.Load += FormSaveJobConfiguration_Load;
            this.textBoxTags.Enter += TextBoxTags_Enter;
            this.textBoxTags.Leave += TextBoxTags_Leave;
            this.textBoxName.TextChanged += UpdatePreview;
            this.textBoxDescription.TextChanged += UpdatePreview;
            this.comboBoxCategory.SelectedIndexChanged += UpdatePreview;
            this.textBoxTags.TextChanged += UpdatePreview;
            
            // Setup list view columns
            SetupPreviewListView();
            
            // Set default values
            SetDefaultValues();
        }

        private void FormSaveJobConfiguration_Load(object sender, EventArgs e)
        {
            // Load existing categories
            LoadExistingCategories();
            
            // Update the preview
            UpdatePreview(null, null);
        }

        private void SetDefaultValues()
        {
            // Set default name
            if (!string.IsNullOrEmpty(_currentJob.Name))
                textBoxName.Text = _currentJob.Name;
            else
                textBoxName.Text = $"Job_{DateTime.Now:yyyyMMdd_HHmmss}";

            // Set default description
            if (!string.IsNullOrEmpty(_currentJob.Description))
                textBoxDescription.Text = _currentJob.Description;
            else
            {
                var sourceDesc = _sourceConnection.IsLocalConnection ? "Local" : _sourceConnection.Host;
                var destDesc = _destinationConnection.IsLocalConnection ? "Local" : _destinationConnection.Host;
                textBoxDescription.Text = $"Transfer from {sourceDesc} to {destDesc}";
            }

            // Set default category
            comboBoxCategory.Text = "General";
            
            // Set placeholder text for tags
            if (textBoxTags.Text == "Separate tags with commas")
                textBoxTags.ForeColor = System.Drawing.SystemColors.GrayText;
        }

        private void LoadExistingCategories()
        {
            try
            {
                var existingCategories = _configService.GetAllCategories();
                
                // Add existing categories that aren't already in the combo box
                foreach (var category in existingCategories)
                {
                    if (!comboBoxCategory.Items.Contains(category))
                        comboBoxCategory.Items.Add(category);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetupPreviewListView()
        {
            listViewPreview.Columns.Clear();
            listViewPreview.Columns.Add("Property", 150);
            listViewPreview.Columns.Add("Value", 350);
        }

        private void UpdatePreview(object sender, EventArgs e)
        {
            try
            {
                listViewPreview.Items.Clear();
                
                // Basic information
                AddPreviewItem("Configuration Name", textBoxName.Text);
                AddPreviewItem("Description", textBoxDescription.Text);
                AddPreviewItem("Category", comboBoxCategory.Text);
                AddPreviewItem("Tags", GetTagsText());
                
                // Job information
                AddPreviewItem("Job Name", _currentJob.Name ?? "");
                AddPreviewItem("Source Path", _currentJob.SourcePath ?? "");
                AddPreviewItem("Destination Path", _currentJob.DestinationPath ?? "");
                AddPreviewItem("Interval", $"{_currentJob.IntervalValue} {_currentJob.IntervalType}");
                AddPreviewItem("Transfer Mode", _currentJob.TransferMode ?? "");
                
                // Connection information
                AddPreviewItem("Source Connection", GetConnectionDescription(_sourceConnection));
                AddPreviewItem("Destination Connection", GetConnectionDescription(_destinationConnection));
                
                // Options
                AddPreviewItem("Set as Default", checkBoxSetAsDefault.Checked ? "Yes" : "No");
                AddPreviewItem("Add to Quick Launch", checkBoxAddToQuickLaunch.Checked ? "Yes" : "No");
                AddPreviewItem("Auto Start on Load", checkBoxAutoStartOnLoad.Checked ? "Yes" : "No");
                AddPreviewItem("Show Notification", checkBoxShowNotificationOnStart.Checked ? "Yes" : "No");
            }
            catch (Exception ex)
            {
                // Handle preview update errors silently
                System.Diagnostics.Debug.WriteLine($"Preview update error: {ex.Message}");
            }
        }

        private void AddPreviewItem(string property, string value)
        {
            var item = new ListViewItem(property);
            item.SubItems.Add(value ?? "");
            listViewPreview.Items.Add(item);
        }

        private string GetConnectionDescription(ConnectionSettings connection)
        {
            if (connection == null) return "Not configured";
            
            if (connection.IsLocalConnection)
                return "Local File System";
                
            return $"{connection.Protocol}://{connection.Username}@{connection.Host}:{connection.Port}";
        }

        private string GetTagsText()
        {
            if (textBoxTags.ForeColor == System.Drawing.SystemColors.GrayText)
                return "";
            return textBoxTags.Text;
        }

        private List<string> GetTagsList()
        {
            var tagsText = GetTagsText();
            if (string.IsNullOrEmpty(tagsText))
                return new List<string>();
                
            return tagsText.Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();
        }

        private void TextBoxTags_Enter(object sender, EventArgs e)
        {
            if (textBoxTags.ForeColor == System.Drawing.SystemColors.GrayText)
            {
                textBoxTags.Text = "";
                textBoxTags.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        private void TextBoxTags_Leave(object sender, EventArgs e)
        {
            if (IsNullOrWhiteSpace(textBoxTags.Text))
            {
                textBoxTags.Text = "Separate tags with commas";
                textBoxTags.ForeColor = System.Drawing.SystemColors.GrayText;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (!ValidateInput())
                    return;

                // Create the saved configuration
                var config = CreateSavedConfiguration();
                
                // Validate the configuration
                var validationErrors = _configService.GetValidationErrors(config);
                if (validationErrors.Count > 0)
                {
                    MessageBox.Show($"Configuration validation failed:\n{string.Join("\n", validationErrors.ToArray())}", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Save the configuration
                if (_configService.SaveConfiguration(config))
                {
                    SavedConfiguration = config;
                    
                    // Set as default if requested
                    if (checkBoxSetAsDefault.Checked)
                        _configService.SetDefaultConfiguration(config.Id);
                    
                    // Add to quick launch if requested
                    if (checkBoxAddToQuickLaunch.Checked)
                    {
                        var quickLaunchItem = new QuickLaunchItem
                        {
                            ConfigurationId = config.Id,
                            DisplayName = config.DisplayName,
                            Description = config.FormattedDescription,
                            IsFavorite = false,
                            SortOrder = 0
                        };
                        _configService.AddToQuickLaunch(config.Id, quickLaunchItem);
                    }
                    
                    MessageBox.Show("Configuration saved successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save configuration. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (IsNullOrWhiteSpace(textBoxName.Text))
            {
                MessageBox.Show("Please enter a configuration name.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxName.Focus();
                return false;
            }

            // Check if name already exists
            if (_configService.ConfigurationNameExists(textBoxName.Text.Trim()))
            {
                var result = MessageBox.Show(
                    "A configuration with this name already exists. Do you want to overwrite it?", 
                    "Configuration Exists", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                    
                if (result != DialogResult.Yes)
                {
                    textBoxName.Focus();
                    return false;
                }
            }

            if (IsNullOrWhiteSpace(comboBoxCategory.Text))
            {
                MessageBox.Show("Please select or enter a category.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBoxCategory.Focus();
                return false;
            }

            return true;
        }

        private SavedJobConfiguration CreateSavedConfiguration()
        {
            var config = new SavedJobConfiguration
            {
                Name = textBoxName.Text.Trim(),
                Description = textBoxDescription.Text.Trim(),
                Category = comboBoxCategory.Text.Trim(),
                Tags = GetTagsList(),
                IsDefault = checkBoxSetAsDefault.Checked,
                EnableQuickLaunch = checkBoxAddToQuickLaunch.Checked,
                AutoStartOnLoad = checkBoxAutoStartOnLoad.Checked,
                ShowNotificationOnStart = checkBoxShowNotificationOnStart.Checked,
                
                // Copy job settings
                JobSettings = CloneJob(_currentJob)
            };
            
            // Create saved connections after config is initialized
            config.SourceConnection = new SavedConnection
            {
                Name = string.Format("{0}_Source", config.Name),
                Description = string.Format("Source connection for {0}", config.Name),
                Settings = CloneConnectionSettings(_sourceConnection),
                CreatedDate = DateTime.Now
            };
            
            config.DestinationConnection = new SavedConnection
            {
                Name = string.Format("{0}_Destination", config.Name), 
                Description = string.Format("Destination connection for {0}", config.Name),
                Settings = CloneConnectionSettings(_destinationConnection),
                CreatedDate = DateTime.Now
            };
            
            return config;
        }

        private SyncJob CloneJob(SyncJob original)
        {
            return new SyncJob
            {
                Name = original.Name,
                Description = original.Description,
                IsEnabled = original.IsEnabled,
                SourcePath = original.SourcePath,
                DestinationPath = original.DestinationPath,
                IncludeSubFolders = original.IncludeSubFolders,
                OverwriteExisting = original.OverwriteExisting,
                StartTime = original.StartTime,
                IntervalValue = original.IntervalValue,
                IntervalType = original.IntervalType,
                TransferMode = original.TransferMode,
                RetryCount = original.RetryCount,
                MaxRetries = original.MaxRetries,
                RetryDelaySeconds = original.RetryDelaySeconds,
                EnableBackup = original.EnableBackup,
                BackupPath = original.BackupPath,
                ValidateTransfer = original.ValidateTransfer,
                ShowTransferProgress = original.ShowTransferProgress,
                FilterSettings = CloneFilterSettings(original.FilterSettings)
            };
        }

        private FilterSettings CloneFilterSettings(FilterSettings original)
        {
            if (original == null) return new FilterSettings();
            
            return new FilterSettings
            {
                FiltersEnabled = original.FiltersEnabled,
                AllowedFileTypes = original.AllowedFileTypes,
                MaxFileSize = original.MaxFileSize,
                MinFileSize = original.MinFileSize,
                IncludeHiddenFiles = original.IncludeHiddenFiles,
                IncludeSystemFiles = original.IncludeSystemFiles,
                IncludeReadOnlyFiles = original.IncludeReadOnlyFiles,
                ExcludePatterns = original.ExcludePatterns
            };
        }

        private ConnectionSettings CloneConnectionSettings(ConnectionSettings original)
        {
            return new ConnectionSettings
            {
                Protocol = original.Protocol,
                ProtocolType = original.ProtocolType,
                Host = original.Host,
                Port = original.Port,
                Username = original.Username,
                Password = original.Password,
                UsePassiveMode = original.UsePassiveMode,
                Timeout = original.Timeout,
                SshKeyPath = original.SshKeyPath,
                SshKeyPassphrase = original.SshKeyPassphrase,
                EnableSsl = original.EnableSsl,
                IgnoreCertificateErrors = original.IgnoreCertificateErrors,
                CertificatePath = original.CertificatePath,
                MaxConnections = original.MaxConnections,
                KeepAlive = original.KeepAlive,
                OperationTimeout = original.OperationTimeout
            };
        }

        private void buttonTestConfig_Click(object sender, EventArgs e)
        {
            try
            {
                var config = CreateSavedConfiguration();
                var validationErrors = _configService.GetValidationErrors(config);
                
                if (validationErrors.Count > 0)
                {
                    MessageBox.Show($"Configuration has validation errors:\n{string.Join("\n", validationErrors.ToArray())}", 
                        "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Test connections
                var sourceTestResult = config.SourceConnection.Settings.TestConnection();
                var destTestResult = config.DestinationConnection.Settings.TestConnection();
                
                var message = "Configuration Test Results:\n\n";
                message += $"Source Connection: {(sourceTestResult.Success ? "✓ Success" : "✗ Failed")}\n";
                if (!sourceTestResult.Success)
                    message += $"  Error: {sourceTestResult.ErrorMessage}\n";
                    
                message += $"Destination Connection: {(destTestResult.Success ? "✓ Success" : "✗ Failed")}\n";
                if (!destTestResult.Success)
                    message += $"  Error: {destTestResult.ErrorMessage}\n";
                
                message += $"\nJob Configuration: ✓ Valid";
                
                var icon = (sourceTestResult.Success && destTestResult.Success) ? 
                    MessageBoxIcon.Information : MessageBoxIcon.Warning;
                    
                MessageBox.Show(message, "Configuration Test", MessageBoxButtons.OK, icon);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing configuration: {ex.Message}", "Test Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
