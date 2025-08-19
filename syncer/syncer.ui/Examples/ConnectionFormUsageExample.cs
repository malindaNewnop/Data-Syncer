using System;
using System.Windows.Forms;
using syncer.ui.Forms;

namespace syncer.ui.Examples
{
    /// <summary>
    /// Example showing how to use the new comprehensive connection form
    /// This replaces the need for separate FormSimplifiedConnection and FormKeyGeneration dialogs
    /// </summary>
    public class ConnectionFormUsageExample
    {
        /// <summary>
        /// Example: Create a new connection
        /// </summary>
        public static void ShowCreateNewConnection()
        {
            using (var form = new FormComprehensiveConnection())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var settings = form.ConnectionSettings;
                    
                    // Use the connection settings
                    MessageBox.Show($"Connection created for {settings.Host}:{settings.Port}", 
                        "Connection Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Save settings, test connection, etc.
                }
            }
        }

        /// <summary>
        /// Example: Edit an existing connection
        /// </summary>
        public static void ShowEditConnection(syncer.core.ConnectionSettings existingSettings)
        {
            using (var form = new FormComprehensiveConnection(existingSettings))
            {
                form.Text = "Edit Connection - " + existingSettings.Host;
                
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var updatedSettings = form.ConnectionSettings;
                    
                    // Use the updated connection settings
                    MessageBox.Show("Connection settings updated successfully!", 
                        "Settings Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Example: Integration with existing forms
        /// </summary>
        public static void IntegrateWithExistingButton(Button existingButton)
        {
            existingButton.Text = "Connection & SSH Keys...";
            existingButton.Click += (sender, e) => ShowCreateNewConnection();
        }
    }
}
