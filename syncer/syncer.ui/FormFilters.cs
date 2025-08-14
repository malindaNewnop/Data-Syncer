using System;
using System.Drawing;
using System.Windows.Forms;

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
            InitializeFileTypeFilters();
            LoadSettings();
        }

        private void InitializeFileTypeFilters()
        {
            if (clbFileTypes != null)
            {
                clbFileTypes.Items.Clear();
                string[] fileTypes = _filterService.GetDefaultFileTypes();
                clbFileTypes.Items.AddRange(fileTypes);
            }
        }

        private void LoadSettings()
        {
            if (_currentSettings != null)
            {
                if (chkEnableFilters != null) chkEnableFilters.Checked = _currentSettings.FiltersEnabled;
                if (numMinSize != null) numMinSize.Value = _currentSettings.MinFileSize;
                if (numMaxSize != null) numMaxSize.Value = _currentSettings.MaxFileSize;
                if (chkIncludeHidden != null) chkIncludeHidden.Checked = _currentSettings.IncludeHiddenFiles;
                if (chkIncludeSystem != null) chkIncludeSystem.Checked = _currentSettings.IncludeSystemFiles;
                if (chkIncludeReadOnly != null) chkIncludeReadOnly.Checked = _currentSettings.IncludeReadOnlyFiles;
                if (txtExcludePatterns != null) txtExcludePatterns.Text = _currentSettings.ExcludePatterns ?? string.Empty;
                if (_currentSettings.AllowedFileTypes != null && clbFileTypes != null)
                {
                    for (int i = 0; i < clbFileTypes.Items.Count; i++)
                    {
                        string item = clbFileTypes.Items[i].ToString();
                        bool isSelected = false;
                        for (int j = 0; j < _currentSettings.AllowedFileTypes.Length; j++)
                        {
                            string fileType = _currentSettings.AllowedFileTypes[j];
                            string prefix = fileType.Split(' ')[0];
                            if (item.IndexOf(prefix) == 0) { isSelected = true; break; }
                        }
                        clbFileTypes.SetItemChecked(i, isSelected);
                    }
                }
                else
                {
                    SetDefaultFileTypeSelection();
                }
            }
            else
            {
                SetDefaultValues();
            }
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
            if (clbFileTypes != null)
            {
                int itemsToCheck = clbFileTypes.Items.Count < 6 ? clbFileTypes.Items.Count : 6;
                for (int i = 0; i < itemsToCheck; i++) clbFileTypes.SetItemChecked(i, true);
            }
        }

        private void SaveSettings()
        {
            if (_currentSettings == null) _currentSettings = new FilterSettings();
            _currentSettings.FiltersEnabled = chkEnableFilters != null && chkEnableFilters.Checked;
            _currentSettings.MinFileSize = numMinSize != null ? numMinSize.Value : 0;
            _currentSettings.MaxFileSize = numMaxSize != null ? numMaxSize.Value : 100;
            _currentSettings.IncludeHiddenFiles = chkIncludeHidden != null && chkIncludeHidden.Checked;
            _currentSettings.IncludeSystemFiles = chkIncludeSystem != null && chkIncludeSystem.Checked;
            _currentSettings.IncludeReadOnlyFiles = chkIncludeReadOnly == null || chkIncludeReadOnly.Checked;
            _currentSettings.ExcludePatterns = txtExcludePatterns != null ? txtExcludePatterns.Text.Trim() : string.Empty;
            if (clbFileTypes != null)
            {
                System.Collections.Generic.List<string> selectedTypes = new System.Collections.Generic.List<string>();
                foreach (object item in clbFileTypes.CheckedItems)
                {
                    selectedTypes.Add(item.ToString());
                }
                _currentSettings.AllowedFileTypes = selectedTypes.ToArray();
            }
            _filterService.SaveFilterSettings(_currentSettings);
            ServiceLocator.LogService.LogInfo("Filter settings saved");
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            if (clbFileTypes != null)
            {
                for (int i = 0; i < clbFileTypes.Items.Count; i++) clbFileTypes.SetItemChecked(i, true);
            }
        }

        private void btnSelectNone_Click(object sender, EventArgs e)
        {
            if (clbFileTypes != null)
            {
                for (int i = 0; i < clbFileTypes.Items.Count; i++) clbFileTypes.SetItemChecked(i, false);
            }
        }

        private void btnAddCustom_Click(object sender, EventArgs e)
        {
            using (Form inputForm = new Form())
            {
                inputForm.Text = "Add Custom Extension";
                inputForm.Size = new Size(300, 150);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;
                Label label = new Label(); label.Left = 10; label.Top = 15; label.Text = "Enter file extension (e.g., .abc):"; label.AutoSize = true;
                TextBox textBox = new TextBox(); textBox.Left = 10; textBox.Top = 40; textBox.Width = 260;
                Button okButton = new Button(); okButton.Text = "OK"; okButton.Left = 115; okButton.Width = 75; okButton.Top = 70; okButton.DialogResult = DialogResult.OK;
                Button cancelButton = new Button(); cancelButton.Text = "Cancel"; cancelButton.Left = 195; cancelButton.Width = 75; cancelButton.Top = 70; cancelButton.DialogResult = DialogResult.Cancel;
                inputForm.Controls.Add(label);
                inputForm.Controls.Add(textBox);
                inputForm.Controls.Add(okButton);
                inputForm.Controls.Add(cancelButton);
                inputForm.AcceptButton = okButton;
                inputForm.CancelButton = cancelButton;
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    string customExtension = textBox.Text.Trim();
                    if (!UIStringExtensions.IsNullOrWhiteSpace(customExtension))
                    {
                        if (!customExtension.StartsWith(".")) customExtension = "." + customExtension;
                        string customItem = customExtension + " - Custom extension";
                        if (clbFileTypes != null)
                        {
                            clbFileTypes.Items.Add(customItem);
                            clbFileTypes.SetItemChecked(clbFileTypes.Items.Count - 1, true);
                        }
                        ServiceLocator.LogService.LogInfo("Custom file extension added: " + customExtension);
                    }
                }
            }
        }

        private void btnRemoveSelected_Click(object sender, EventArgs e)
        {
            if (clbFileTypes != null && clbFileTypes.SelectedIndex >= 0)
            {
                string item = clbFileTypes.SelectedItem.ToString();
                if (item.IndexOf("Custom extension") >= 0)
                {
                    clbFileTypes.Items.RemoveAt(clbFileTypes.SelectedIndex);
                    ServiceLocator.LogService.LogInfo("Custom file extension removed: " + item);
                }
                else
                {
                    MessageBox.Show("Cannot remove built-in file types. You can uncheck them instead.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select an item to remove.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                try
                {
                    SaveSettings();
                    MessageBox.Show("Filter settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (chkEnableFilters != null && chkEnableFilters.Checked)
            {
                if ((numMinSize != null ? numMinSize.Value : 0) > (numMaxSize != null ? numMaxSize.Value : 100))
                {
                    MessageBox.Show("Minimum size cannot be greater than maximum size.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (numMinSize != null) numMinSize.Focus();
                    return false;
                }
                if (clbFileTypes != null && clbFileTypes.CheckedItems.Count == 0)
                {
                    DialogResult result = MessageBox.Show("No file types are selected. This will exclude all files. Continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.No) return false;
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
            bool enabled = chkEnableFilters != null && chkEnableFilters.Checked;
            if (gbFileTypes != null) gbFileTypes.Enabled = enabled;
            if (gbSizeFilters != null) gbSizeFilters.Enabled = enabled;
            if (gbAdvancedFilters != null) gbAdvancedFilters.Enabled = enabled;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                string filterInfo = GenerateFilterPreview();
                MessageBox.Show(filterInfo, "Filter Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string GenerateFilterPreview()
        {
            string preview = "Filter Settings Preview:\n\n";
            if (!(chkEnableFilters != null && chkEnableFilters.Checked))
            {
                preview += "Filters are disabled - all files will be processed.";
                return preview;
            }
            preview += "Filters are enabled\n\n";
            preview += "Included file types:\n";
            if (clbFileTypes != null && clbFileTypes.CheckedItems.Count > 0)
            {
                foreach (object o in clbFileTypes.CheckedItems)
                {
                    preview += "  • " + o.ToString() + "\n";
                }
            }
            else
            {
                preview += "  • None (all files will be excluded)\n";
            }
            preview += "\nSize filters:\n";
            preview += "  • Minimum size: " + (numMinSize != null ? numMinSize.Value.ToString() : "0") + " MB\n";
            preview += "  • Maximum size: " + (numMaxSize != null ? numMaxSize.Value.ToString() : "100") + " MB\n";
            preview += "\nAdvanced options:\n";
            preview += "  • Include hidden files: " + ((chkIncludeHidden != null && chkIncludeHidden.Checked) ? "Yes" : "No") + "\n";
            preview += "  • Include system files: " + ((chkIncludeSystem != null && chkIncludeSystem.Checked) ? "Yes" : "No") + "\n";
            preview += "  • Include read-only files: " + ((chkIncludeReadOnly == null || chkIncludeReadOnly.Checked) ? "Yes" : "No") + "\n";
            if (txtExcludePatterns != null && !UIStringExtensions.IsNullOrWhiteSpace(txtExcludePatterns.Text))
            {
                preview += "  • Exclude patterns: " + txtExcludePatterns.Text + "\n";
            }
            return preview;
        }
    }
}
