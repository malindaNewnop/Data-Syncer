using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace syncer.ui
{
    public partial class FormFilters : Form
    {
        public FormFilters()
        {
            InitializeComponent();
            InitializeCustomComponents();
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
            
            // Set default values
            numMinSize.Value = 0;
            numMaxSize.Value = 100;
            chkEnableFilters.Checked = true;
            
            LoadSettings();
        }

        private void InitializeFileTypeFilters()
        {
            // Common file types
            string[] fileTypes = {
                ".txt - Text files",
                ".doc, .docx - Word documents", 
                ".xls, .xlsx - Excel files",
                ".pdf - PDF documents",
                ".jpg, .jpeg - JPEG images",
                ".png - PNG images",
                ".gif - GIF images",
                ".mp4 - Video files",
                ".mp3 - Audio files",
                ".zip, .rar - Archive files",
                ".exe - Executable files",
                ".dll - Library files",
                ".log - Log files",
                ".csv - CSV files",
                ".xml - XML files",
                ".json - JSON files"
            };

            clbFileTypes.Items.AddRange(fileTypes);
            
            // Check some common types by default
            for (int i = 0; i < 6; i++)
            {
                clbFileTypes.SetItemChecked(i, true);
            }
        }

        private void LoadSettings()
        {
            // TODO: Load settings from configuration
        }

        private void SaveSettings()
        {
            // TODO: Save settings to configuration
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbFileTypes.Items.Count; i++)
            {
                clbFileTypes.SetItemChecked(i, true);
            }
        }

        private void btnSelectNone_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbFileTypes.Items.Count; i++)
            {
                clbFileTypes.SetItemChecked(i, false);
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
                        clbFileTypes.Items.Add(customItem);
                        clbFileTypes.SetItemChecked(clbFileTypes.Items.Count - 1, true);
                    }
                }
            }
        }

        private void btnRemoveSelected_Click(object sender, EventArgs e)
        {
            if (clbFileTypes.SelectedIndex >= 0)
            {
                string item = clbFileTypes.SelectedItem.ToString();
                if (item.Contains("Custom extension"))
                {
                    clbFileTypes.Items.RemoveAt(clbFileTypes.SelectedIndex);
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
                SaveSettings();
                MessageBox.Show("Filter settings saved successfully!", "Success", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateInputs()
        {
            if (chkEnableFilters.Checked)
            {
                if (numMinSize.Value > numMaxSize.Value)
                {
                    MessageBox.Show("Minimum size cannot be greater than maximum size.", 
                                  "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    numMinSize.Focus();
                    return false;
                }

                if (clbFileTypes.CheckedItems.Count == 0)
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
            gbFileTypes.Enabled = chkEnableFilters.Checked;
            gbSizeFilters.Enabled = chkEnableFilters.Checked;
            gbAdvancedFilters.Enabled = chkEnableFilters.Checked;
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
            
            if (!chkEnableFilters.Checked)
            {
                preview += "Filters are disabled - all files will be processed.";
                return preview;
            }

            preview += "Filters are enabled\n\n";
            
            // File types
            preview += "Included file types:\n";
            if (clbFileTypes.CheckedItems.Count > 0)
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
            preview += "  • Minimum size: " + numMinSize.Value + " MB\n";
            preview += "  • Maximum size: " + numMaxSize.Value + " MB\n";
            
            // Advanced filters
            preview += "\nAdvanced options:\n";
            preview += "  • Include hidden files: " + (chkIncludeHidden.Checked ? "Yes" : "No") + "\n";
            preview += "  • Include system files: " + (chkIncludeSystem.Checked ? "Yes" : "No") + "\n";
            preview += "  • Include read-only files: " + (chkIncludeReadOnly.Checked ? "Yes" : "No") + "\n";
            
            if (!StringExtensions.IsNullOrWhiteSpace(txtExcludePatterns.Text))
            {
                preview += "  • Exclude patterns: " + txtExcludePatterns.Text + "\n";
            }

            return preview;
        }
    }
}
