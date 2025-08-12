using System;
using System.Drawing;
using System.Windows.Forms;

namespace syncer.ui
{
    public partial class FormSchedule : Form
    {
        public FormSchedule()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Schedule Settings";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Set default values
            chkEnabled.Checked = true;
            dtpStartTime.Value = DateTime.Now.Date.AddHours(9); // 9 AM today
            numInterval.Value = 60; // 60 minutes
            cmbIntervalType.SelectedIndex = 1; // Minutes
            
            LoadSettings();
        }

        private void LoadSettings()
        {
            // TODO: Load settings from configuration
        }

        private void SaveSettings()
        {
            // TODO: Save settings to configuration
        }

        private void btnBrowseSource_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select source folder to sync";
                dialog.ShowNewFolderButton = false;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtSourcePath.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnBrowseDestination_Click(object sender, EventArgs e)
        {
            txtDestinationPath.Text = "/remote/destination/"; // Default remote path
            
            // TODO: Implement remote folder browser when FTP connection is available
            MessageBox.Show("Remote folder browsing will be available when FTP connection is configured.", 
                          "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                SaveSettings();
                MessageBox.Show("Schedule saved successfully!", "Success", 
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
            if (StringExtensions.IsNullOrWhiteSpace(txtJobName.Text))
            {
                MessageBox.Show("Please enter a job name.", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtJobName.Focus();
                return false;
            }

            if (StringExtensions.IsNullOrWhiteSpace(txtSourcePath.Text))
            {
                MessageBox.Show("Please select a source folder.", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnBrowseSource.Focus();
                return false;
            }

            if (StringExtensions.IsNullOrWhiteSpace(txtDestinationPath.Text))
            {
                MessageBox.Show("Please enter a destination path.", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDestinationPath.Focus();
                return false;
            }

            if (numInterval.Value <= 0)
            {
                MessageBox.Show("Please enter a valid interval.", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numInterval.Focus();
                return false;
            }

            return true;
        }

        private void chkEnabled_CheckedChanged(object sender, EventArgs e)
        {
            // Enable/disable schedule controls based on checkbox
            gbScheduleSettings.Enabled = chkEnabled.Checked;
        }

        private void cmbIntervalType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Adjust interval limits based on type
            switch (cmbIntervalType.SelectedItem.ToString())
            {
                case "Minutes":
                    numInterval.Minimum = 1;
                    numInterval.Maximum = 1440; // 24 hours in minutes
                    break;
                case "Hours":
                    numInterval.Minimum = 1;
                    numInterval.Maximum = 24;
                    break;
                case "Days":
                    numInterval.Minimum = 1;
                    numInterval.Maximum = 365;
                    break;
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                string scheduleInfo = GenerateSchedulePreview();
                MessageBox.Show(scheduleInfo, "Schedule Preview", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string GenerateSchedulePreview()
        {
            string enabled = chkEnabled.Checked ? "Enabled" : "Disabled";
            string interval = "Every " + numInterval.Value + " " + cmbIntervalType.SelectedItem;
            string startTime = dtpStartTime.Value.ToString("yyyy-MM-dd HH:mm");
            
            return "Job Name: " + txtJobName.Text + "\n" +
                   "Status: " + enabled + "\n" +
                   "Source: " + txtSourcePath.Text + "\n" +
                   "Destination: " + txtDestinationPath.Text + "\n" +
                   "Schedule: " + interval + "\n" +
                   "Start Time: " + startTime + "\n" +
                   "Transfer Mode: " + cmbTransferMode.SelectedItem;
        }
    }
}
