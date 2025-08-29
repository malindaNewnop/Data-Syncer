using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using syncer.ui.Services;
using syncer.ui.Interfaces;

namespace syncer.ui.Forms
{
    public partial class FormQuickLaunch : Form
    {
        private ISavedJobConfigurationService _configService;
        private FormMain _parentForm;
        private List<QuickLaunchItem> _quickLaunchItems;

        public FormQuickLaunch(FormMain parentForm)
        {
            InitializeComponent();
            _parentForm = parentForm;
            _configService = ServiceLocator.SavedJobConfigurationService;
            _quickLaunchItems = new List<QuickLaunchItem>();
            
            // Make the form movable
            this.MouseDown += FormQuickLaunch_MouseDown;
            this.MouseMove += FormQuickLaunch_MouseMove;
            this.MouseUp += FormQuickLaunch_MouseUp;
            
            LoadQuickLaunchConfigurations();
        }

        #region Movable Form Implementation
        private bool _isDragging = false;
        private Point _dragStartPoint;

        private void FormQuickLaunch_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _dragStartPoint = e.Location;
                this.Cursor = Cursors.SizeAll;
            }
        }

        private void FormQuickLaunch_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentScreenPos = PointToScreen(e.Location);
                this.Location = new Point(currentScreenPos.X - _dragStartPoint.X, currentScreenPos.Y - _dragStartPoint.Y);
            }
        }

        private void FormQuickLaunch_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            this.Cursor = Cursors.Default;
        }
        #endregion

        private void LoadQuickLaunchConfigurations()
        {
            try
            {
                if (_configService != null)
                {
                    _quickLaunchItems = _configService.GetQuickLaunchItems();
                    lbQuickLaunch.DataSource = null;
                    lbQuickLaunch.DataSource = _quickLaunchItems;
                    lbQuickLaunch.DisplayMember = "DisplayName";
                    lbQuickLaunch.ValueMember = "ConfigurationId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Quick Launch configurations: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadAndStartJob_Click(object sender, EventArgs e)
        {
            try
            {
                if (lbQuickLaunch.SelectedValue != null)
                {
                    string configId = lbQuickLaunch.SelectedValue.ToString();
                    var configuration = _configService.GetConfiguration(configId);
                    
                    if (configuration != null)
                    {
                        // Load the configuration in the parent form and start the job
                        _parentForm?.LoadConfigurationAndStart(configuration);
                        
                        // Show notification
                        MessageBox.Show($"Configuration '{configuration.Name}' loaded and job started successfully!", 
                            "Job Started", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Selected configuration not found.", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a configuration from the list.", 
                        "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading and starting job: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEditSelectedJob_Click(object sender, EventArgs e)
        {
            try
            {
                if (lbQuickLaunch.SelectedValue != null)
                {
                    string configId = lbQuickLaunch.SelectedValue.ToString();
                    var configuration = _configService.GetConfiguration(configId);
                    
                    if (configuration != null)
                    {
                        // Hide the Quick Launch form before opening the edit dialog
                        this.Hide();
                        
                        // Open the edit configuration form
                        using (var editForm = new FormEditConfiguration(configuration))
                        {
                            DialogResult result = editForm.ShowDialog();
                            
                            // Show the Quick Launch form again after the edit dialog is closed
                            this.Show();
                            this.BringToFront();
                            
                            if (result == DialogResult.OK)
                            {
                                // Refresh the list after editing
                                LoadQuickLaunchConfigurations();
                                MessageBox.Show("Configuration updated successfully!", 
                                    "Configuration Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Selected configuration not found.", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a configuration to edit.", 
                        "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                // Make sure the Quick Launch form is shown again even if there's an error
                this.Show();
                this.BringToFront();
                
                MessageBox.Show($"Error opening edit form: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnManageConfigurations_Click(object sender, EventArgs e)
        {
            try
            {
                // Hide the Quick Launch form before opening the manage dialog
                this.Hide();
                
                // Open the full configuration management form
                using (var manageForm = new FormSimpleLoadConfiguration(_configService))
                {
                    DialogResult result = manageForm.ShowDialog();
                    
                    // Show the Quick Launch form again after the manage dialog is closed
                    this.Show();
                    this.BringToFront();
                    
                    if (result == DialogResult.OK)
                    {
                        // Refresh the list after any changes
                        LoadQuickLaunchConfigurations();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening configuration manager: " + ex.Message, 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Make sure to show the form again even if there's an error
                this.Show();
                this.BringToFront();
            }
        }

        // Override ProcessCmdKey to handle Escape key
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Hide();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Method to show the form at a specific position
        public void ShowAtPosition(Point position)
        {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = position;
            this.Show();
            this.BringToFront();
            LoadQuickLaunchConfigurations();
        }

        // Method to toggle visibility
        public void ToggleVisibility()
        {
            if (this.Visible)
            {
                this.Hide();
            }
            else
            {
                this.Show();
                this.BringToFront();
                LoadQuickLaunchConfigurations();
            }
        }
    }
}
