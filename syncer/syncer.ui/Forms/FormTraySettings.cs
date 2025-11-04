using System;
using System.Windows.Forms;
using syncer.core.Services;

namespace syncer.ui.Forms
{
    public partial class FormTraySettings : Form
    {
        private IConfigurationService _configService;
        private AutoStartService _autoStartService;
        
        public FormTraySettings()
        {
            InitializeComponent();
            
            try
            {
                _configService = ServiceLocator.ConfigurationService;
                
                // Initialize auto-start service
                var coreLogService = new syncer.core.FileLogService();
                _autoStartService = new AutoStartService("FTPSyncer", coreLogService);
                
                LoadSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading settings: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void LoadSettings()
        {
            if (_configService == null) return;
            
            try
            {
                // Load notification settings
                checkBoxNotificationsEnabled.Checked = _configService.GetSetting("NotificationsEnabled", true);
                checkBoxStartupNotification.Checked = _configService.GetSetting("ShowStartupNotification", true);
                numericUpDownDelay.Value = _configService.GetSetting("NotificationDelay", 3000);
                
                // Load tray behavior settings
                checkBoxMinimizeToTray.Checked = _configService.GetSetting("MinimizeToTray", true);
                checkBoxStartMinimized.Checked = _configService.GetSetting("StartMinimized", false);
                
                // Load auto-start setting
                if (_autoStartService != null)
                {
                    checkBoxAutoStart.Checked = _autoStartService.IsAutoStartEnabled();
                }
                
                // Update control states
                UpdateControlStates();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading tray settings: " + ex.Message);
            }
        }
        
        private void SaveSettings()
        {
            if (_configService == null) return;
            
            try
            {
                // Save notification settings
                _configService.SaveSetting("NotificationsEnabled", checkBoxNotificationsEnabled.Checked);
                _configService.SaveSetting("ShowStartupNotification", checkBoxStartupNotification.Checked);
                _configService.SaveSetting("NotificationDelay", (int)numericUpDownDelay.Value);
                
                // Save tray behavior settings
                _configService.SaveSetting("MinimizeToTray", checkBoxMinimizeToTray.Checked);
                _configService.SaveSetting("StartMinimized", checkBoxStartMinimized.Checked);
                
                // Handle auto-start setting
                if (_autoStartService != null)
                {
                    bool autoStartEnabled = _autoStartService.IsAutoStartEnabled();
                    bool autoStartRequested = checkBoxAutoStart.Checked;
                    
                    if (autoStartEnabled != autoStartRequested)
                    {
                        bool success = false;
                        if (autoStartRequested)
                        {
                            success = _autoStartService.EnableAutoStart();
                        }
                        else
                        {
                            success = _autoStartService.DisableAutoStart();
                        }
                        
                        if (!success)
                        {
                            MessageBox.Show("Failed to update auto-start setting. Please check permissions.", 
                                "Auto-start Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                
                // Save all settings to registry/config
                _configService.SaveAllSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving settings: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void UpdateControlStates()
        {
            // Enable/disable notification-related controls based on notifications enabled
            bool notificationsEnabled = checkBoxNotificationsEnabled.Checked;
            
            checkBoxStartupNotification.Enabled = notificationsEnabled;
            labelNotificationDelay.Enabled = notificationsEnabled;
            numericUpDownDelay.Enabled = notificationsEnabled;
            labelSeconds.Enabled = notificationsEnabled;
        }
        
        private void checkBoxNotificationsEnabled_CheckedChanged(object sender, EventArgs e)
        {
            UpdateControlStates();
        }
        
        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                SaveSettings();
                
                // Show confirmation
                MessageBox.Show("Settings saved successfully. Some changes may require restarting the application to take effect.", 
                    "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
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
}
