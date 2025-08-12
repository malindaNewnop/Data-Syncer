using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace syncer.ui
{
    public partial class FormFilters : Form
    {
        private IFilterService _filterService;
        private FilterSettings _currentSettings;

        public FormFilters()
        {
            InitializeComponent();
            InitializeServices();
            InitializeCustomComponents();
        }

        private void InitializeServices()
        {
            _filterService = ServiceLocator.FilterService;
            _currentSettings = _filterService.GetFilterSettings();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Filter Settings";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Initialize file type filters
            InitializeFileTypeFilters();
            
            LoadSettings();
        }

        private void InitializeFileTypeFilters()
        {
            if (clbFileTypes != null)
            {
                clbFileTypes.Items.Clear();
                
                // Load default file types from service
                string[] fileTypes = _filterService.GetDefaultFileTypes();
                clbFileTypes.Items.AddRange(fileTypes);
            }
        }

        private void LoadSettings()
        {
            if (_currentSettings != null)
            {
                // Load filter settings
                if (chkEnableFilters != null) 
                    chkEnableFilters.Checked = _currentSettings.FiltersEnabled;
                
                if (numMinSize != null) 
                    numMinSize.Value = _currentSettings.MinFileSize;
                
                if (numMaxSize != null) 
                    numMaxSize.Value = _currentSettings.MaxFileSize;
                
                if (chkIncludeHidden != null) 
                    chkIncludeHidden.Checked = _currentSettings.IncludeHiddenFiles;
                
                if (chkIncludeSystem != null) 
                    chkIncludeSystem.Checked = _currentSettings.IncludeSystemFiles;
                
                if (chkIncludeReadOnly != null) 
                    chkIncludeReadOnly.Checked = _currentSettings.IncludeReadOnlyFiles;
                
                if (txtExcludePatterns != null) 
                    txtExcludePatterns.Text = _currentSettings.ExcludePatterns ?? "";

                // Load selected file types
                if (_currentSettings.AllowedFileTypes != null && clbFileTypes != null)
                {
                    for (int i = 0; i < clbFileTypes.Items.Count; i++)
                    {
                        string item = clbFileTypes.Items[i].ToString();
                        bool isSelected = _currentSettings.AllowedFileTypes.Any(fileType => 
                            item.StartsWith(fileType.Split(' ')[0]));
                        clbFileTypes.SetItemChecked(i, isSelected);
                    }
                }
                else
                {
                    // Set some defaults if no settings exist
                    SetDefaultFileTypeSelection();
                }
            }
            else
            {
                // Set defaults if no settings exist
                SetDefaultValues();
            }

            // Update UI state based on filters enabled checkbox
            UpdateFilterControlsState();
        }

        private void SetDefaultValues()
        {
            if (chkEnableFilters != null) chkEnableFilters.Checked = true;
            if (numMinSize != null) numMinSize.Value = 0;
            if (numMaxSize != null) numMaxSize.Value = 100;
            if (chkIncludeReadOnly != null) chkIncludeReadOnly.Checked = true;
            
            SetDefaultFileTypeSelection();
        }

        private void SetDefaultFileTypeSelection()
        {
            // Check some common types by default
            if (clbFileTypes != null)
            {
                int itemsToCheck = Math.Min(6, clbFileTypes.Items.Count);
                for (int i = 0; i < itemsToCheck; i++)
                {
                    clbFileTypes.SetItemChecked(i, true);
                }
            }
        }

        private void SaveSettings()
        {
            try
            {
                if (_currentSettings == null)
                    _currentSettings = new FilterSettings();

                _currentSettings.FiltersEnabled = chkEnableFilters?.Checked ?? true;
                _currentSettings.MinFileSize = numMinSize?.Value ?? 0;
                _currentSettings.MaxFileSize = numMaxSize?.Value ?? 100;
                _currentSettings.IncludeHiddenFiles = chkIncludeHidden?.Checked ?? false;
                _currentSettings.IncludeSystemFiles = chkIncludeSystem?.Checked ?? false;
                _currentSettings.IncludeReadOnlyFiles = chkIncludeReadOnly?.Checked ?? true;
                _currentSettings.ExcludePatterns = txtExcludePatterns?.Text?.Trim() ?? "";

                // Save selected file types
                if (clbFileTypes != null)
                {
                    var selectedTypes = new System.Collections.Generic.List<string>();
                    foreach (var item in clbFileTypes.CheckedItems)
                    {
                        selectedTypes.Add(item.ToString());
                    }
                    _currentSettings.AllowedFileTypes = selectedTypes.ToArray();
                }

                _filterService.SaveFilterSettings(_currentSettings);
                ServiceLocator.LogService.LogInfo("Filter settings saved");
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Error saving filter settings: " + ex.Message);
                throw;
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            if (clbFileTypes != null)
            {
                for (int i = 0; i < clbFileTypes.Items.Count; i++)
                {
                    clbFileTypes.SetItemChecked(i, true);
                }
            }
        }

        private void btnSelectNone_Click(object sender, EventArgs e)
        {
            if (clbFileTypes != null)
            {
                for (int i = 0; i < clbFileTypes.Items.Count; i++)
                {
                    clbFileTypes.SetItemChecked(i, false);
                }
            }
        }

        private void btnAddCustom_Click(object sender, EventArgs e)
        {
            using (var inputForm = new Form())
            {
                inputForm.Text = "Add Custom Extension";
                inputForm.Size = new Size(300, 150);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;

                var label = new Label() { Left = 10, Top = 15, Text = "Enter file extension (e.g., .abc):", AutoSize = true };
                var textBox = new TextBox() { Left = 10, Top = 40, Width = 260 };
                var okButton = new Button() { Text = "OK", Left = 115, Width = 75, Top = 70, DialogResult = DialogResult.OK };
                var cancelButton = new Button() { Text = "Cancel", Left = 195, Width = 75, Top = 70, DialogResult = DialogResult.Cancel };

                inputForm.Controls.Add(label);
                inputForm.Controls.Add(textBox);
                inputForm.Controls.Add(okButton);
                inputForm.Controls.Add(cancelButton);
                inputForm.AcceptButton = okButton;
                inputForm.CancelButton = cancelButton;

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    string customExtension = textBox.Text.Trim();
                    if (!StringExtensions.IsNullOrWhiteSpace(customExtension))
                    {
                        if (!customExtension.StartsWith("."))
                            customExtension = "." + customExtension;
                        
                        string customItem = customExtension + " - Custom extension";
                        
                        if (clbFileTypes != null)
                        {
                            clbFileTypes.Items.Add(customItem);
                            clbFileTypes.SetItemChecked(clbFileTypes.Items.Count - 1, true);
                        }
                        
                        ServiceLocator.LogService.LogInfo($"Custom file extension added: {customExtension}");
                    }
                }
            }
        }

        private void btnRemoveSelected_Click(object sender, EventArgs e)
        {
            if (clbFileTypes != null && clbFileTypes.SelectedIndex >= 0)
            {
                string item = clbFileTypes.SelectedItem.ToString();
                if (item.Contains("Custom extension"))
                {
                    clbFileTypes.Items.RemoveAt(clbFileTypes.SelectedIndex);
                    ServiceLocator.LogService.LogInfo($"Custom file extension removed: {item}");
                }
                else
                {
                    MessageBox.Show("Cannot remove built-in file types. You can uncheck them instead.", 
                                  "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select an item to remove.", "Information", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                try
                {
                    SaveSettings();
                    MessageBox.Show("Filter settings saved successfully!", "Success", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving settings: " + ex.Message, "Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateInputs()
        {
            if (chkEnableFilters?.Checked ?? false)
            {
                if ((numMinSize?.Value ?? 0) > (numMaxSize?.Value ?? 100))
                {
                    MessageBox.Show("Minimum size cannot be greater than maximum size.", 
                                  "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    numMinSize?.Focus();
                    return false;
                }

                if (clbFileTypes?.CheckedItems.Count == 0)
                {
                    var result = MessageBox.Show("No file types are selected. This will exclude all files. Continue?", 
                                                "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.No)
                        return false;
                }
            }

            return true;
        }

        private void chkEnableFilters_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFilterControlsState();
        }

        private void UpdateFilterControlsState()
        {
            bool enabled = chkEnableFilters?.Checked ?? false;
            
            if (gbFileTypes != null) gbFileTypes.Enabled = enabled;
            if (gbSizeFilters != null) gbSizeFilters.Enabled = enabled;
            if (gbAdvancedFilters != null) gbAdvancedFilters.Enabled = enabled;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                string filterInfo = GenerateFilterPreview();
                MessageBox.Show(filterInfo, "Filter Preview", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string GenerateFilterPreview()
        {
            string preview = "Filter Settings Preview:\n\n";
            
            if (!(chkEnableFilters?.Checked ?? false))
            {
                preview += "Filters are disabled - all files will be processed.";
                return preview;
            }

            preview += "Filters are enabled\n\n";
            
            // File types
            preview += "Included file types:\n";
            if (clbFileTypes?.CheckedItems.Count > 0)
            {
                foreach (string item in clbFileTypes.CheckedItems)
                {
                    preview += "  • " + item + "\n";
                }
            }
            else
            {
                preview += "  • None (all files will be excluded)\n";
            }
            
            // Size filters
            preview += "\nSize filters:\n";
            preview += "  • Minimum size: " + (numMinSize?.Value ?? 0) + " MB\n";
            preview += "  • Maximum size: " + (numMaxSize?.Value ?? 100) + " MB\n";
            
            // Advanced filters
            preview += "\nAdvanced options:\n";
            preview += "  • Include hidden files: " + ((chkIncludeHidden?.Checked ?? false) ? "Yes" : "No") + "\n";
            preview += "  • Include system files: " + ((chkIncludeSystem?.Checked ?? false) ? "Yes" : "No") + "\n";
            preview += "  • Include read-only files: " + ((chkIncludeReadOnly?.Checked ?? true) ? "Yes" : "No") + "\n";
            
            if (!StringExtensions.IsNullOrWhiteSpace(txtExcludePatterns?.Text))
            {
                preview += "  • Exclude patterns: " + txtExcludePatterns.Text + "\n";
            }

            return preview;
        }
    }
}
