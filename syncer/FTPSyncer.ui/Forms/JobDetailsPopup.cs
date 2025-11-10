using System;
using System.Drawing;
using System.Windows.Forms;
using FTPSyncer.ui.Interfaces;

namespace FTPSyncer.ui.Forms
{
    /// <summary>
    /// Popup form to display detailed information about a running job
    /// Compatible with .NET 3.5
    /// </summary>
    public partial class JobDetailsPopup : Form
    {
        private long _jobId;
        private ITimerJobManager _timerJobManager;
        private Timer _refreshTimer;

        public JobDetailsPopup(long jobId)
        {
            _jobId = jobId;
            _timerJobManager = ServiceLocator.TimerJobManager;
            
            InitializeComponent();
            InitializeRefreshTimer();
            LoadJobDetails();
        }

        private void InitializeRefreshTimer()
        {
            // Refresh job details every 2 seconds
            _refreshTimer = new Timer();
            _refreshTimer.Interval = 2000;
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            LoadJobDetails();
        }

        private void LoadJobDetails()
        {
            if (_timerJobManager == null)
            {
                lblStatus.Text = "Timer job manager not available";
                return;
            }

            try
            {
                // Get job basic information
                string jobName = _timerJobManager.GetTimerJobName(_jobId);
                string folderPath = _timerJobManager.GetTimerJobFolderPath(_jobId);
                string remotePath = _timerJobManager.GetTimerJobRemotePath(_jobId);
                double intervalMs = _timerJobManager.GetTimerJobInterval(_jobId);
                
                // Get job status information
                bool isRunning = _timerJobManager.IsTimerJobRunning(_jobId);
                bool isDownloadJob = _timerJobManager.IsTimerJobDownloadJob(_jobId);
                bool isUploading = _timerJobManager.IsTimerJobUploading(_jobId);
                bool isDownloading = _timerJobManager.IsTimerJobDownloading(_jobId);
                
                // Get timing information
                DateTime? lastUpload = _timerJobManager.GetLastUploadTime(_jobId);
                DateTime? lastDownload = _timerJobManager.GetLastDownloadTime(_jobId);
                DateTime? uploadStartTime = _timerJobManager.GetTimerJobUploadStartTime(_jobId);
                DateTime? downloadStartTime = _timerJobManager.GetTimerJobDownloadStartTime(_jobId);
                
                // Get configuration information
                bool includeSubfolders = _timerJobManager.GetTimerJobIncludeSubfolders(_jobId);
                bool deleteSourceAfterTransfer = _timerJobManager.GetTimerJobDeleteSourceAfterTransfer(_jobId);
                bool enableFilters = _timerJobManager.GetTimerJobEnableFilters(_jobId);
                
                // Update UI elements
                lblJobId.Text = _jobId.ToString();
                lblJobName.Text = jobName ?? "N/A";
                lblSourcePath.Text = folderPath ?? "N/A";
                lblDestinationPath.Text = remotePath ?? "N/A";
                
                // Format interval
                string intervalText = FormatInterval(intervalMs);
                lblInterval.Text = intervalText;
                
                // Update job type
                lblJobType.Text = isDownloadJob ? "Download (Remote → Local)" : "Upload (Local → Remote)";
                
                // Update status with detailed information
                string statusText = GetDetailedStatus(isRunning, isDownloadJob, isUploading, isDownloading, 
                    uploadStartTime, downloadStartTime);
                lblStatus.Text = statusText;
                
                // Update last transfer time
                DateTime? lastTransfer = isDownloadJob ? lastDownload : lastUpload;
                lblLastTransfer.Text = lastTransfer.HasValue ? 
                    lastTransfer.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Never";
                
                // Update configuration details
                lblIncludeSubfolders.Text = includeSubfolders ? "Yes" : "No";
                lblDeleteSource.Text = deleteSourceAfterTransfer ? "Yes" : "No";
                lblFiltersEnabled.Text = enableFilters ? "Yes" : "No";
                
                // Update filter details if enabled
                if (enableFilters)
                {
                    var includeExtensions = _timerJobManager.GetTimerJobIncludeExtensions(_jobId);
                    var excludeExtensions = _timerJobManager.GetTimerJobExcludeExtensions(_jobId);
                    
                    lblIncludeExtensions.Text = (includeExtensions != null && includeExtensions.Count > 0) ?
                        string.Join(", ", includeExtensions.ToArray()) : "None";
                    lblExcludeExtensions.Text = (excludeExtensions != null && excludeExtensions.Count > 0) ?
                        string.Join(", ", excludeExtensions.ToArray()) : "None";
                }
                else
                {
                    lblIncludeExtensions.Text = "N/A";
                    lblExcludeExtensions.Text = "N/A";
                }
                
                // Calculate next run time if job is running
                if (isRunning && lastTransfer.HasValue && intervalMs > 0)
                {
                    DateTime nextRun = lastTransfer.Value.AddMilliseconds(intervalMs);
                    lblNextRun.Text = nextRun > DateTime.Now ? 
                        nextRun.ToString("yyyy-MM-dd HH:mm:ss") : "Due now";
                }
                else if (isRunning)
                {
                    lblNextRun.Text = "Running";
                }
                else
                {
                    lblNextRun.Text = "Not scheduled";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading job details: " + ex.Message;
            }
        }

        private string FormatInterval(double intervalMs)
        {
            if (intervalMs <= 0)
                return "N/A";
                
            if (intervalMs < 60000) // Less than a minute
            {
                return string.Format("{0:0} Seconds", intervalMs / 1000);
            }
            else if (intervalMs < 3600000) // Less than an hour
            {
                return string.Format("{0:0} Minutes", intervalMs / 60000);
            }
            else // Hours
            {
                return string.Format("{0:0.0} Hours", intervalMs / 3600000);
            }
        }

        private string GetDetailedStatus(bool isRunning, bool isDownloadJob, bool isUploading, bool isDownloading,
            DateTime? uploadStartTime, DateTime? downloadStartTime)
        {
            if (!isRunning)
                return "Stopped";
                
            if (isDownloadJob)
            {
                if (isDownloading && downloadStartTime.HasValue)
                {
                    TimeSpan duration = DateTime.Now - downloadStartTime.Value;
                    return string.Format("Downloading... (Running for {0:mm\\:ss})", duration);
                }
                else
                {
                    return "Running (Waiting for next download)";
                }
            }
            else
            {
                if (isUploading && uploadStartTime.HasValue)
                {
                    TimeSpan duration = DateTime.Now - uploadStartTime.Value;
                    return string.Format("Uploading... (Running for {0:mm\\:ss})", duration);
                }
                else
                {
                    return "Running (Waiting for next upload)";
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadJobDetails();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
            }
            base.OnFormClosed(e);
        }
    }
}





