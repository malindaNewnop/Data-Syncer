using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using FTPSyncer.core;
using FTPSyncer.core.Services;
using FTPSyncer.core.Transfers;
using FTPSyncer.ui;

namespace FTPSyncer.ui.Examples
{
    /// <summary>
    /// Example form demonstrating the enhanced FTP/SFTP functionality
    /// Shows how to use the Transfer Engine, Settings Service, and enhanced transfer clients
    /// </summary>
    public partial class TransferDemoForm : Form
    {
        private readonly TransferEngine _transferEngine;
        private readonly EnhancedSettingsService _settingsService;
        private readonly ILogService _logService;

        // UI Controls
        private TabControl tabControl;
        private TabPage tabConnection;
        private TabPage tabTransfer;
        private TabPage tabSettings;
        private TabPage tabLogs;

        // Connection Tab Controls
        private GroupBox grpSource;
        private ComboBox cmbSourceProtocol;
        private TextBox txtSourceHost;
        private TextBox txtSourcePort;
        private TextBox txtSourceUsername;
        private TextBox txtSourcePassword;
        private TextBox txtSourcePath;
        private Button btnTestSource;

        private GroupBox grpDestination;
        private ComboBox cmbDestProtocol;
        private TextBox txtDestHost;
        private TextBox txtDestPort;
        private TextBox txtDestUsername;
        private TextBox txtDestPassword;
        private TextBox txtDestPath;
        private Button btnTestDest;

        // Transfer Tab Controls
        private Button btnStartTransfer;
        private Button btnCancelTransfer;
        private ProgressBar progressBar;
        private Label lblProgress;
        private Label lblCurrentFile;
        private Label lblSpeed;
        private Label lblTimeRemaining;
        private CheckBox chkOverwrite;
        private CheckBox chkIncludeSubfolders;

        // Settings Tab Controls
        private NumericUpDown numRetryCount;
        private NumericUpDown numRetryDelay;
        private NumericUpDown numTimeout;
        private CheckBox chkEnableLogging;
        private CheckBox chkValidateTransfer;
        private Button btnSaveSettings;
        private Button btnResetSettings;

        // Logs Tab Controls
        private RichTextBox txtLogs;
        private Button btnClearLogs;

        private SyncJob _currentJob;

        public TransferDemoForm()
        {
            InitializeComponent();
            InitializeServices();
            LoadSettings();
            SetupEventHandlers();
        }

        private void InitializeServices()
        {
            // Initialize services with production implementations
            _settingsService = new EnhancedSettingsService();
            _logService = new FileLogService();
            var clientFactory = new EnhancedTransferClientFactory(useProductionClients: true);
            var fileFilterService = new FileFilterService(_logService);
            _transferEngine = new TransferEngine(clientFactory, _logService, _settingsService, fileFilterService);
        }

        private void InitializeComponent()
        {
            this.Text = "FTPSyncer - FTP/SFTP Transfer Demo";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(600, 400);

            // Create main tab control
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            this.Controls.Add(tabControl);

            CreateConnectionTab();
            CreateTransferTab();
            CreateSettingsTab();
            CreateLogsTab();
        }

        private void CreateConnectionTab()
        {
            tabConnection = new TabPage("Connections");
            tabControl.TabPages.Add(tabConnection);

            // Source Group
            grpSource = new GroupBox("Source Connection");
            grpSource.Location = new Point(10, 10);
            grpSource.Size = new Size(360, 200);
            tabConnection.Controls.Add(grpSource);

            var lblSourceProtocol = new Label { Text = "Protocol:", Location = new Point(10, 25), Size = new Size(60, 20) };
            cmbSourceProtocol = new ComboBox 
            { 
                Location = new Point(80, 23), 
                Size = new Size(100, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSourceProtocol.Items.AddRange(new string[] { "Local", "FTP", "SFTP" });
            cmbSourceProtocol.SelectedIndex = 0;

            var lblSourceHost = new Label { Text = "Host:", Location = new Point(10, 55), Size = new Size(60, 20) };
            txtSourceHost = new TextBox { Location = new Point(80, 53), Size = new Size(200, 20) };

            var lblSourcePort = new Label { Text = "Port:", Location = new Point(290, 55), Size = new Size(30, 20) };
            txtSourcePort = new TextBox { Location = new Point(320, 53), Size = new Size(40, 20), Text = "21" };

            var lblSourceUsername = new Label { Text = "Username:", Location = new Point(10, 85), Size = new Size(60, 20) };
            txtSourceUsername = new TextBox { Location = new Point(80, 83), Size = new Size(120, 20) };

            var lblSourcePassword = new Label { Text = "Password:", Location = new Point(10, 115), Size = new Size(60, 20) };
            txtSourcePassword = new TextBox { Location = new Point(80, 113), Size = new Size(120, 20), PasswordChar = '*' };

            var lblSourcePath = new Label { Text = "Path:", Location = new Point(10, 145), Size = new Size(60, 20) };
            txtSourcePath = new TextBox { Location = new Point(80, 143), Size = new Size(200, 20) };

            btnTestSource = new Button { Text = "Test", Location = new Point(290, 143), Size = new Size(70, 23) };

            grpSource.Controls.AddRange(new Control[] 
            {
                lblSourceProtocol, cmbSourceProtocol, lblSourceHost, txtSourceHost,
                lblSourcePort, txtSourcePort, lblSourceUsername, txtSourceUsername,
                lblSourcePassword, txtSourcePassword, lblSourcePath, txtSourcePath,
                btnTestSource
            });

            // Destination Group
            grpDestination = new GroupBox("Destination Connection");
            grpDestination.Location = new Point(380, 10);
            grpDestination.Size = new Size(360, 200);
            tabConnection.Controls.Add(grpDestination);

            var lblDestProtocol = new Label { Text = "Protocol:", Location = new Point(10, 25), Size = new Size(60, 20) };
            cmbDestProtocol = new ComboBox 
            { 
                Location = new Point(80, 23), 
                Size = new Size(100, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDestProtocol.Items.AddRange(new string[] { "Local", "FTP", "SFTP" });
            cmbDestProtocol.SelectedIndex = 1;

            var lblDestHost = new Label { Text = "Host:", Location = new Point(10, 55), Size = new Size(60, 20) };
            txtDestHost = new TextBox { Location = new Point(80, 53), Size = new Size(200, 20) };

            var lblDestPort = new Label { Text = "Port:", Location = new Point(290, 55), Size = new Size(30, 20) };
            txtDestPort = new TextBox { Location = new Point(320, 53), Size = new Size(40, 20), Text = "21" };

            var lblDestUsername = new Label { Text = "Username:", Location = new Point(10, 85), Size = new Size(60, 20) };
            txtDestUsername = new TextBox { Location = new Point(80, 83), Size = new Size(120, 20) };

            var lblDestPassword = new Label { Text = "Password:", Location = new Point(10, 115), Size = new Size(60, 20) };
            txtDestPassword = new TextBox { Location = new Point(80, 113), Size = new Size(120, 20), PasswordChar = '*' };

            var lblDestPath = new Label { Text = "Path:", Location = new Point(10, 145), Size = new Size(60, 20) };
            txtDestPath = new TextBox { Location = new Point(80, 143), Size = new Size(200, 20) };

            btnTestDest = new Button { Text = "Test", Location = new Point(290, 143), Size = new Size(70, 23) };

            grpDestination.Controls.AddRange(new Control[] 
            {
                lblDestProtocol, cmbDestProtocol, lblDestHost, txtDestHost,
                lblDestPort, txtDestPort, lblDestUsername, txtDestUsername,
                lblDestPassword, txtDestPassword, lblDestPath, txtDestPath,
                btnTestDest
            });
        }

        private void CreateTransferTab()
        {
            tabTransfer = new TabPage("Transfer");
            tabControl.TabPages.Add(tabTransfer);

            // Transfer options
            chkOverwrite = new CheckBox { Text = "Overwrite existing files", Location = new Point(20, 20), Size = new Size(200, 20), Checked = true };
            chkIncludeSubfolders = new CheckBox { Text = "Include subfolders", Location = new Point(20, 50), Size = new Size(200, 20), Checked = true };

            // Transfer controls
            btnStartTransfer = new Button { Text = "Start Transfer", Location = new Point(20, 90), Size = new Size(100, 30) };
            btnCancelTransfer = new Button { Text = "Cancel", Location = new Point(130, 90), Size = new Size(100, 30), Enabled = false };

            // Progress controls
            var lblProgressTitle = new Label { Text = "Progress:", Location = new Point(20, 140), Size = new Size(60, 20) };
            progressBar = new ProgressBar { Location = new Point(20, 160), Size = new Size(400, 25) };
            lblProgress = new Label { Location = new Point(430, 163), Size = new Size(100, 20), Text = "0%" };

            lblCurrentFile = new Label { Location = new Point(20, 195), Size = new Size(500, 20), Text = "Ready..." };
            lblSpeed = new Label { Location = new Point(20, 220), Size = new Size(200, 20), Text = "" };
            lblTimeRemaining = new Label { Location = new Point(250, 220), Size = new Size(200, 20), Text = "" };

            tabTransfer.Controls.AddRange(new Control[]
            {
                chkOverwrite, chkIncludeSubfolders, btnStartTransfer, btnCancelTransfer,
                lblProgressTitle, progressBar, lblProgress, lblCurrentFile, lblSpeed, lblTimeRemaining
            });
        }

        private void CreateSettingsTab()
        {
            tabSettings = new TabPage("Settings");
            tabControl.TabPages.Add(tabSettings);

            var lblRetryCount = new Label { Text = "Retry Count:", Location = new Point(20, 25), Size = new Size(80, 20) };
            numRetryCount = new NumericUpDown { Location = new Point(110, 23), Size = new Size(60, 20), Minimum = 0, Maximum = 10, Value = 3 };

            var lblRetryDelay = new Label { Text = "Retry Delay (s):", Location = new Point(20, 55), Size = new Size(85, 20) };
            numRetryDelay = new NumericUpDown { Location = new Point(110, 53), Size = new Size(60, 20), Minimum = 1, Maximum = 300, Value = 5 };

            var lblTimeout = new Label { Text = "Timeout (s):", Location = new Point(20, 85), Size = new Size(80, 20) };
            numTimeout = new NumericUpDown { Location = new Point(110, 83), Size = new Size(60, 20), Minimum = 10, Maximum = 300, Value = 30 };

            chkEnableLogging = new CheckBox { Text = "Enable logging", Location = new Point(20, 115), Size = new Size(150, 20), Checked = true };
            chkValidateTransfer = new CheckBox { Text = "Validate transfers", Location = new Point(20, 145), Size = new Size(150, 20), Checked = true };

            btnSaveSettings = new Button { Text = "Save Settings", Location = new Point(20, 185), Size = new Size(100, 30) };
            btnResetSettings = new Button { Text = "Reset to Defaults", Location = new Point(130, 185), Size = new Size(120, 30) };

            tabSettings.Controls.AddRange(new Control[]
            {
                lblRetryCount, numRetryCount, lblRetryDelay, numRetryDelay,
                lblTimeout, numTimeout, chkEnableLogging, chkValidateTransfer,
                btnSaveSettings, btnResetSettings
            });
        }

        private void CreateLogsTab()
        {
            tabLogs = new TabPage("Logs");
            tabControl.TabPages.Add(tabLogs);

            txtLogs = new RichTextBox 
            { 
                Location = new Point(10, 10), 
                Size = new Size(760, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly = true,
                Font = new Font("Consolas", 9F, FontStyle.Regular)
            };

            btnClearLogs = new Button 
            { 
                Text = "Clear Logs", 
                Location = new Point(10, 420),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            tabLogs.Controls.AddRange(new Control[] { txtLogs, btnClearLogs });
        }

        private void SetupEventHandlers()
        {
            // Connection tests
            btnTestSource.Click += BtnTestSource_Click;
            btnTestDest.Click += BtnTestDest_Click;

            // Protocol change events
            cmbSourceProtocol.SelectedIndexChanged += CmbSourceProtocol_SelectedIndexChanged;
            cmbDestProtocol.SelectedIndexChanged += CmbDestProtocol_SelectedIndexChanged;

            // Transfer events
            btnStartTransfer.Click += BtnStartTransfer_Click;
            btnCancelTransfer.Click += BtnCancelTransfer_Click;

            // Settings events
            btnSaveSettings.Click += BtnSaveSettings_Click;
            btnResetSettings.Click += BtnResetSettings_Click;

            // Log events
            btnClearLogs.Click += BtnClearLogs_Click;

            // Transfer engine events
            _transferEngine.TransferStarted += TransferEngine_TransferStarted;
            _transferEngine.TransferProgress += TransferEngine_TransferProgress;
            _transferEngine.TransferCompleted += TransferEngine_TransferCompleted;
        }

        private void LoadSettings()
        {
            try
            {
                var settings = _settingsService.GetEnhancedSettings();
                numRetryCount.Value = settings.DefaultRetryCount;
                numRetryDelay.Value = settings.RetryDelaySeconds;
                numTimeout.Value = settings.ConnectionTimeoutSeconds;
                chkEnableLogging.Checked = settings.EnableLogging;
                chkValidateTransfer.Checked = settings.ValidateChecksums;

                AddLog("Settings loaded successfully.");
            }
            catch (Exception ex)
            {
                AddLog($"Failed to load settings: {ex.Message}");
            }
        }

        private void BtnTestSource_Click(object sender, EventArgs e)
        {
            TestConnection(GetSourceConnectionSettings(), "Source");
        }

        private void BtnTestDest_Click(object sender, EventArgs e)
        {
            TestConnection(GetDestinationConnectionSettings(), "Destination");
        }

        private void TestConnection(FTPSyncer.core.ConnectionSettings settings, string connectionType)
        {
            try
            {
                AddLog($"Testing {connectionType} connection...");
                
                var factory = new EnhancedTransferClientFactory();
                var client = factory.Create(settings.Protocol);
                
                string error;
                var success = client.TestConnection(settings, out error);
                
                if (success)
                {
                    AddLog($"{connectionType} connection test successful!");
                    MessageBox.Show($"{connectionType} connection test successful!", "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    AddLog($"{connectionType} connection test failed: {error}");
                    MessageBox.Show($"{connectionType} connection test failed:\n{error}", "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                AddLog($"{connectionType} connection test error: {ex.Message}");
                MessageBox.Show($"{connectionType} connection test error:\n{ex.Message}", "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStartTransfer_Click(object sender, EventArgs e)
        {
            try
            {
                _currentJob = new SyncJob
                {
                    Id = Guid.NewGuid().GetHashCode(),
                    Name = "Demo Transfer Job",
                    SourcePath = txtSourcePath.Text,
                    DestinationPath = txtDestPath.Text,
                    OverwriteExisting = chkOverwrite.Checked,
                    IncludeSubFolders = chkIncludeSubfolders.Checked,
                    SourceConnection = ConvertToCore(GetSourceConnectionSettings()),
                    DestinationConnection = ConvertToCore(GetDestinationConnectionSettings()),
                    MaxRetries = (int)numRetryCount.Value,
                    RetryDelaySeconds = (int)numRetryDelay.Value,
                    ValidateTransfer = chkValidateTransfer.Checked
                };

                // Validate the job
                var validationErrors = _currentJob.ValidateConfiguration();
                if (validationErrors.Count > 0)
                {
                    var errorMsg = "Job validation failed:\n" + string.Join("\n", validationErrors.ToArray());
                    MessageBox.Show(errorMsg, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnStartTransfer.Enabled = false;
                btnCancelTransfer.Enabled = true;
                progressBar.Value = 0;
                lblProgress.Text = "0%";
                lblCurrentFile.Text = "Starting transfer...";

                // Execute transfer on background thread
                var worker = new System.ComponentModel.BackgroundWorker();
                worker.DoWork += (s, args) =>
                {
                    var result = _transferEngine.ExecuteJob(_currentJob);
                    args.Result = result;
                };
                worker.RunWorkerCompleted += (s, args) =>
                {
                    btnStartTransfer.Enabled = true;
                    btnCancelTransfer.Enabled = false;
                    
                    if (args.Result is TransferResultEnhanced result)
                    {
                        var message = result.Success 
                            ? $"Transfer completed successfully!\n\nFiles transferred: {result.SuccessfulFiles}\nData transferred: {result.GetFormattedDataSize()}\nDuration: {result.Duration.TotalSeconds:F1} seconds\nAverage speed: {result.GetFormattedSpeed()}"
                            : $"Transfer failed or completed with errors.\n\nFiles transferred: {result.SuccessfulFiles}\nFiles failed: {result.FailedFiles}\nErrors: {result.Errors.Count}";
                            
                        MessageBox.Show(message, "Transfer Result", MessageBoxButtons.OK, 
                            result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    }
                };
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                btnStartTransfer.Enabled = true;
                btnCancelTransfer.Enabled = false;
                AddLog($"Transfer start error: {ex.Message}");
                MessageBox.Show($"Failed to start transfer:\n{ex.Message}", "Transfer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancelTransfer_Click(object sender, EventArgs e)
        {
            if (_currentJob != null)
            {
                _transferEngine.CancelJob(_currentJob.Id.ToString());
                AddLog("Transfer cancellation requested...");
            }
        }

        private void BtnSaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                var settings = _settingsService.GetEnhancedSettings();
                settings.DefaultRetryCount = (int)numRetryCount.Value;
                settings.RetryDelaySeconds = (int)numRetryDelay.Value;
                settings.ConnectionTimeoutSeconds = (int)numTimeout.Value;
                settings.EnableLogging = chkEnableLogging.Checked;
                settings.ValidateChecksums = chkValidateTransfer.Checked;

                if (_settingsService.SaveEnhancedSettings(settings))
                {
                    AddLog("Settings saved successfully.");
                    MessageBox.Show("Settings saved successfully!", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    AddLog("Failed to save settings.");
                    MessageBox.Show("Failed to save settings!", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Settings save error: {ex.Message}");
                MessageBox.Show($"Failed to save settings:\n{ex.Message}", "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnResetSettings_Click(object sender, EventArgs e)
        {
            try
            {
                if (_settingsService.ResetToDefaults())
                {
                    LoadSettings();
                    AddLog("Settings reset to defaults.");
                    MessageBox.Show("Settings reset to defaults!", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Settings reset error: {ex.Message}");
                MessageBox.Show($"Failed to reset settings:\n{ex.Message}", "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClearLogs_Click(object sender, EventArgs e)
        {
            txtLogs.Clear();
        }

        private void CmbSourceProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateProtocolUI(cmbSourceProtocol, txtSourceHost, txtSourcePort, txtSourceUsername, txtSourcePassword);
        }

        private void CmbDestProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateProtocolUI(cmbDestProtocol, txtDestHost, txtDestPort, txtDestUsername, txtDestPassword);
        }

        private void UpdateProtocolUI(ComboBox cmbProtocol, TextBox txtHost, TextBox txtPort, TextBox txtUsername, TextBox txtPassword)
        {
            var isLocal = cmbProtocol.SelectedIndex == 0;
            
            txtHost.Enabled = !isLocal;
            txtPort.Enabled = !isLocal;
            txtUsername.Enabled = !isLocal;
            txtPassword.Enabled = !isLocal;
            
            if (cmbProtocol.SelectedIndex == 2) // SFTP
            {
                txtPort.Text = "22";
            }
            else if (cmbProtocol.SelectedIndex == 1) // FTP
            {
                txtPort.Text = "21";
            }
        }

        private void TransferEngine_TransferStarted(object sender, TransferStartedEventArgs e)
        {
            this.BeginInvoke((Action)(() =>
            {
                AddLog($"Transfer started: {e.Job.Name}");
                lblCurrentFile.Text = "Transfer started...";
            }));
        }

        private void TransferEngine_TransferProgress(object sender, TransferProgressEventArgs e)
        {
            this.BeginInvoke((Action)(() =>
            {
                progressBar.Value = Math.Min(e.ProgressPercent, 100);
                lblProgress.Text = $"{e.ProgressPercent}%";
                
                if (!string.IsNullOrEmpty(e.CurrentFile))
                {
                    lblCurrentFile.Text = $"Current: {Path.GetFileName(e.CurrentFile)}";
                }
                
                if (e.Result != null)
                {
                    lblSpeed.Text = e.Result.GetFormattedSpeed();
                    
                    var remaining = e.Result.Duration.TotalSeconds > 0 
                        ? TimeSpan.FromSeconds((100 - e.ProgressPercent) * e.Result.Duration.TotalSeconds / e.ProgressPercent)
                        : TimeSpan.Zero;
                    lblTimeRemaining.Text = $"ETA: {remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
                }
            }));
        }

        private void TransferEngine_TransferCompleted(object sender, TransferCompletedEventArgs e)
        {
            this.BeginInvoke((Action)(() =>
            {
                progressBar.Value = 100;
                lblProgress.Text = "100%";
                lblCurrentFile.Text = e.Result.Success ? "Transfer completed successfully!" : "Transfer completed with errors.";
                
                AddLog($"Transfer completed: {e.Job.Name} - {(e.Result.Success ? "Success" : "Failed")}");
                AddLog($"Files transferred: {e.Result.SuccessfulFiles}, Failed: {e.Result.FailedFiles}");
                if (e.Result.TransferredBytes > 0)
                {
                    AddLog($"Data transferred: {e.Result.GetFormattedDataSize()} at {e.Result.GetFormattedSpeed()}");
                }
            }));
        }

        private FTPSyncer.core.ConnectionSettings GetSourceConnectionSettings()
        {
            return new FTPSyncer.core.ConnectionSettings
            {
                Protocol = (FTPSyncer.core.ProtocolType)cmbSourceProtocol.SelectedIndex,
                Host = txtSourceHost.Text,
                Port = int.TryParse(txtSourcePort.Text, out int port) ? port : (cmbSourceProtocol.SelectedIndex == 2 ? 22 : 21),
                Username = txtSourceUsername.Text,
                Password = txtSourcePassword.Text
            };
        }

        private FTPSyncer.core.ConnectionSettings GetDestinationConnectionSettings()
        {
            return new FTPSyncer.core.ConnectionSettings
            {
                Protocol = (FTPSyncer.core.ProtocolType)cmbDestProtocol.SelectedIndex,
                Host = txtDestHost.Text,
                Port = int.TryParse(txtDestPort.Text, out int port) ? port : (cmbDestProtocol.SelectedIndex == 2 ? 22 : 21),
                Username = txtDestUsername.Text,
                Password = txtDestPassword.Text
            };
        }

        private FTPSyncer.core.ConnectionSettings ConvertToCore(FTPSyncer.core.ConnectionSettings settings)
        {
            return settings; // Already core type
        }

        private void AddLog(string message)
        {
            this.BeginInvoke((Action)(() =>
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                txtLogs.AppendText($"[{timestamp}] {message}\n");
                txtLogs.ScrollToCaret();
            }));
        }

        protected override void OnFormClosing(System.ComponentModel.CancelEventArgs e)
        {
            _transferEngine?.Dispose();
            base.OnFormClosing(e);
        }
    }
}





