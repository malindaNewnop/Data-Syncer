using System;
using System.Windows.Forms;

namespace FTPSyncer.ui.Forms
{
    /// <summary>
    /// Form for configuring notification settings
    /// </summary>
    public partial class FormNotificationSettings : Form
    {
        private NotificationSettings _settings;

        public FormNotificationSettings()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // Load current notification settings
                _settings = ServiceLocator.NotificationService?.GetSettings() ?? new NotificationSettings();

                // Set checkbox states
                checkBoxEnableNotifications.Checked = _settings.EnableNotifications;
                checkBoxShowConnectionNotifications.Checked = _settings.ShowConnectionNotifications;
                checkBoxShowJobStartNotifications.Checked = _settings.ShowJobStartNotifications;
                checkBoxShowJobCompleteNotifications.Checked = _settings.ShowJobCompleteNotifications;
                checkBoxShowErrorNotifications.Checked = _settings.ShowErrorNotifications;
                checkBoxShowWarningNotifications.Checked = _settings.ShowWarningNotifications;
                checkBoxPlaySound.Checked = _settings.PlaySound;

                // Update enabled state of child checkboxes
                UpdateChildCheckboxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading notification settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                // Update settings from checkboxes
                _settings.EnableNotifications = checkBoxEnableNotifications.Checked;
                _settings.ShowConnectionNotifications = checkBoxShowConnectionNotifications.Checked;
                _settings.ShowJobStartNotifications = checkBoxShowJobStartNotifications.Checked;
                _settings.ShowJobCompleteNotifications = checkBoxShowJobCompleteNotifications.Checked;
                _settings.ShowErrorNotifications = checkBoxShowErrorNotifications.Checked;
                _settings.ShowWarningNotifications = checkBoxShowWarningNotifications.Checked;
                _settings.PlaySound = checkBoxPlaySound.Checked;

                // Save settings
                ServiceLocator.NotificationService?.SaveSettings(_settings);
                
                // Also save to ConfigService for backward compatibility
                var configService = ServiceLocator.ConfigurationService;
                if (configService != null)
                {
                    configService.SaveSetting("NotificationsEnabled", _settings.EnableNotifications);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving notification settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateChildCheckboxes()
        {
            bool enabled = checkBoxEnableNotifications.Checked;
            
            checkBoxShowConnectionNotifications.Enabled = enabled;
            checkBoxShowJobStartNotifications.Enabled = enabled;
            checkBoxShowJobCompleteNotifications.Enabled = enabled;
            checkBoxShowErrorNotifications.Enabled = enabled;
            checkBoxShowWarningNotifications.Enabled = enabled;
            checkBoxPlaySound.Enabled = enabled;
        }

        private void checkBoxEnableNotifications_CheckedChanged(object sender, EventArgs e)
        {
            UpdateChildCheckboxes();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveSettings();
            
            // Update SystemTrayManager if notification service is available
            var notificationService = ServiceLocator.NotificationService;
            if (notificationService != null)
            {
                // This will update the tray manager's notification state
                notificationService.NotificationsEnabled = _settings.EnableNotifications;
            }
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            SaveSettings();
            
            // Update SystemTrayManager if notification service is available
            var notificationService = ServiceLocator.NotificationService;
            if (notificationService != null)
            {
                // This will update the tray manager's notification state
                notificationService.NotificationsEnabled = _settings.EnableNotifications;
            }
        }

        private void buttonRestoreDefaults_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to restore default notification settings?",
                "Restore Defaults",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _settings = new NotificationSettings(); // Reset to defaults
                LoadSettings();
            }
        }
    }
}
