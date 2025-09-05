using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;

namespace syncer.core.Services
{
    /// <summary>
    /// Service for managing bandwidth control settings across the application
    /// </summary>
    public class BandwidthControlService
    {
        private static BandwidthControlService _instance;
        private static readonly object _lock = new object();
        
        private long _globalUploadLimitBytesPerSecond = 0; // 0 = unlimited
        private long _globalDownloadLimitBytesPerSecond = 0; // 0 = unlimited
        private bool _isBandwidthControlEnabled = false;
        
        // Current speed tracking
        private double _currentUploadSpeedBytesPerSecond = 0;
        private double _currentDownloadSpeedBytesPerSecond = 0;
        private DateTime _lastSpeedUpdate = DateTime.Now;
        private long _totalBytesUploaded = 0;
        private long _totalBytesDownloaded = 0;
        private DateTime _sessionStartTime = DateTime.Now;
        
        private readonly string _configFilePath;
        
        public static BandwidthControlService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new BandwidthControlService();
                    }
                }
                return _instance;
            }
        }
        
        private BandwidthControlService()
        {
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BandwidthSettings.xml");
            LoadSettings();
        }
        
        /// <summary>
        /// Global upload speed limit in bytes per second (0 = unlimited)
        /// </summary>
        public long GlobalUploadLimitBytesPerSecond
        {
            get { return _globalUploadLimitBytesPerSecond; }
            set
            {
                _globalUploadLimitBytesPerSecond = value;
                OnBandwidthSettingsChanged();
            }
        }
        
        /// <summary>
        /// Global download speed limit in bytes per second (0 = unlimited)
        /// </summary>
        public long GlobalDownloadLimitBytesPerSecond
        {
            get { return _globalDownloadLimitBytesPerSecond; }
            set
            {
                _globalDownloadLimitBytesPerSecond = value;
                OnBandwidthSettingsChanged();
            }
        }
        
        /// <summary>
        /// Whether bandwidth control is enabled
        /// </summary>
        public bool IsBandwidthControlEnabled
        {
            get { return _isBandwidthControlEnabled; }
            set
            {
                _isBandwidthControlEnabled = value;
                OnBandwidthSettingsChanged();
            }
        }
        
        /// <summary>
        /// Event fired when bandwidth settings change
        /// </summary>
        public event EventHandler BandwidthSettingsChanged;
        
        /// <summary>
        /// Get upload speed limit in KB/s for display
        /// </summary>
        public long GetUploadLimitKBps()
        {
            return _globalUploadLimitBytesPerSecond / 1024;
        }
        
        /// <summary>
        /// Get download speed limit in KB/s for display
        /// </summary>
        public long GetDownloadLimitKBps()
        {
            return _globalDownloadLimitBytesPerSecond / 1024;
        }
        
        /// <summary>
        /// Set upload speed limit from KB/s value
        /// </summary>
        public void SetUploadLimitKBps(long kbps)
        {
            GlobalUploadLimitBytesPerSecond = kbps * 1024;
        }
        
        /// <summary>
        /// Set download speed limit from KB/s value
        /// </summary>
        public void SetDownloadLimitKBps(long kbps)
        {
            GlobalDownloadLimitBytesPerSecond = kbps * 1024;
        }
        
        /// <summary>
        /// Get current upload speed in bytes per second
        /// </summary>
        public double GetCurrentUploadSpeedBytesPerSecond()
        {
            return _currentUploadSpeedBytesPerSecond;
        }
        
        /// <summary>
        /// Get current download speed in bytes per second
        /// </summary>
        public double GetCurrentDownloadSpeedBytesPerSecond()
        {
            return _currentDownloadSpeedBytesPerSecond;
        }
        
        /// <summary>
        /// Get current upload speed formatted as string (KB/s, MB/s, etc.)
        /// </summary>
        public string GetCurrentUploadSpeedFormatted()
        {
            return FormatBytesPerSecond((long)_currentUploadSpeedBytesPerSecond);
        }
        
        /// <summary>
        /// Get current download speed formatted as string (KB/s, MB/s, etc.)
        /// </summary>
        public string GetCurrentDownloadSpeedFormatted()
        {
            return FormatBytesPerSecond((long)_currentDownloadSpeedBytesPerSecond);
        }
        
        /// <summary>
        /// Update current upload speed with bytes transferred and duration
        /// </summary>
        public void UpdateUploadSpeed(long bytesTransferred, double durationSeconds)
        {
            try
            {
                if (durationSeconds > 0)
                {
                    _currentUploadSpeedBytesPerSecond = bytesTransferred / durationSeconds;
                    _lastSpeedUpdate = DateTime.Now;
                    _totalBytesUploaded += bytesTransferred;
                }
            }
            catch (Exception)
            {
                // Don't let speed calculation errors crash the application
                _currentUploadSpeedBytesPerSecond = 0;
            }
        }
        
        /// <summary>
        /// Update current upload speed based on pre-calculated bytes per second
        /// </summary>
        public void UpdateUploadSpeed(long bytesPerSecond)
        {
            try
            {
                _currentUploadSpeedBytesPerSecond = bytesPerSecond;
                _lastSpeedUpdate = DateTime.Now;
            }
            catch (Exception)
            {
                // Don't let speed calculation errors crash the application
                _currentUploadSpeedBytesPerSecond = 0;
            }
        }
        
        /// <summary>
        /// Update current download speed with bytes transferred and duration
        /// </summary>
        public void UpdateDownloadSpeed(long bytesTransferred, double durationSeconds)
        {
            try
            {
                if (durationSeconds > 0)
                {
                    _currentDownloadSpeedBytesPerSecond = bytesTransferred / durationSeconds;
                    _lastSpeedUpdate = DateTime.Now;
                    _totalBytesDownloaded += bytesTransferred;
                }
            }
            catch (Exception)
            {
                // Don't let speed calculation errors crash the application
                _currentDownloadSpeedBytesPerSecond = 0;
            }
        }
        
        /// <summary>
        /// Update current download speed based on pre-calculated bytes per second
        /// </summary>
        public void UpdateDownloadSpeed(long bytesPerSecond)
        {
            try
            {
                _currentDownloadSpeedBytesPerSecond = bytesPerSecond;
                _lastSpeedUpdate = DateTime.Now;
            }
            catch (Exception)
            {
                // Don't let speed calculation errors crash the application
                _currentDownloadSpeedBytesPerSecond = 0;
            }
        }
        
        /// <summary>
        /// Reset speed tracking counters
        /// </summary>
        public void ResetSpeedTracking()
        {
            _currentUploadSpeedBytesPerSecond = 0;
            _currentDownloadSpeedBytesPerSecond = 0;
            _totalBytesUploaded = 0;
            _totalBytesDownloaded = 0;
            _sessionStartTime = DateTime.Now;
            _lastSpeedUpdate = DateTime.Now;
        }
        
        /// <summary>
        /// Apply speed decay when no transfers are active (call periodically)
        /// </summary>
        public void ApplySpeedDecay(double decayFactor = 0.8)
        {
            try
            {
                // Check if it's been more than 5 seconds since last update
                if ((DateTime.Now - _lastSpeedUpdate).TotalSeconds > 5)
                {
                    _currentUploadSpeedBytesPerSecond *= decayFactor;
                    _currentDownloadSpeedBytesPerSecond *= decayFactor;
                    
                    // If speed is very low, set to zero to avoid displaying tiny values
                    if (_currentUploadSpeedBytesPerSecond < 100) // Less than 100 B/s
                        _currentUploadSpeedBytesPerSecond = 0;
                        
                    if (_currentDownloadSpeedBytesPerSecond < 100) // Less than 100 B/s
                        _currentDownloadSpeedBytesPerSecond = 0;
                }
            }
            catch (Exception)
            {
                // Don't let decay errors crash the application
                _currentUploadSpeedBytesPerSecond = 0;
                _currentDownloadSpeedBytesPerSecond = 0;
            }
        }
        
        /// <summary>
        /// Get total bytes uploaded in current session
        /// </summary>
        public long GetTotalBytesUploaded()
        {
            return _totalBytesUploaded;
        }
        
        /// <summary>
        /// Get total bytes downloaded in current session
        /// </summary>
        public long GetTotalBytesDownloaded()
        {
            return _totalBytesDownloaded;
        }
        
        /// <summary>
        /// Get session duration
        /// </summary>
        public TimeSpan GetSessionDuration()
        {
            return DateTime.Now - _sessionStartTime;
        }

        /// <summary>
        /// Get formatted upload speed limit for display
        /// </summary>
        public string GetFormattedUploadLimit()
        {
            if (_globalUploadLimitBytesPerSecond == 0)
                return "Unlimited";
            
            return FormatBytesPerSecond(_globalUploadLimitBytesPerSecond);
        }
        
        /// <summary>
        /// Get formatted download speed limit for display
        /// </summary>
        public string GetFormattedDownloadLimit()
        {
            if (_globalDownloadLimitBytesPerSecond == 0)
                return "Unlimited";
            
            return FormatBytesPerSecond(_globalDownloadLimitBytesPerSecond);
        }
        
        /// <summary>
        /// Apply bandwidth limits to SFTP configuration
        /// </summary>
        public void ApplyLimitsToSftpConfig(Configuration.SftpConfiguration config, bool isUpload)
        {
            if (!_isBandwidthControlEnabled)
            {
                config.BandwidthLimitBytesPerSecond = 0;
                return;
            }
            
            if (isUpload && _globalUploadLimitBytesPerSecond > 0)
            {
                config.BandwidthLimitBytesPerSecond = _globalUploadLimitBytesPerSecond;
            }
            else if (!isUpload && _globalDownloadLimitBytesPerSecond > 0)
            {
                config.BandwidthLimitBytesPerSecond = _globalDownloadLimitBytesPerSecond;
            }
            else
            {
                config.BandwidthLimitBytesPerSecond = 0; // Unlimited
            }
        }
        
        /// <summary>
        /// Save current settings to configuration file
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                var doc = new XmlDocument();
                var root = doc.CreateElement("BandwidthSettings");
                doc.AppendChild(root);
                
                var uploadLimit = doc.CreateElement("UploadLimitBytesPerSecond");
                uploadLimit.InnerText = _globalUploadLimitBytesPerSecond.ToString();
                root.AppendChild(uploadLimit);
                
                var downloadLimit = doc.CreateElement("DownloadLimitBytesPerSecond");
                downloadLimit.InnerText = _globalDownloadLimitBytesPerSecond.ToString();
                root.AppendChild(downloadLimit);
                
                var enabled = doc.CreateElement("IsBandwidthControlEnabled");
                enabled.InnerText = _isBandwidthControlEnabled.ToString();
                root.AppendChild(enabled);
                
                doc.Save(_configFilePath);
            }
            catch (Exception ex)
            {
                // Log error but don't throw to prevent application crashes
                System.Diagnostics.Debug.WriteLine($"Error saving bandwidth settings: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load settings from configuration file
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    // Use default settings
                    _globalUploadLimitBytesPerSecond = 0;
                    _globalDownloadLimitBytesPerSecond = 0;
                    _isBandwidthControlEnabled = false;
                    return;
                }
                
                var doc = new XmlDocument();
                doc.Load(_configFilePath);
                
                var root = doc.DocumentElement;
                if (root == null || root.Name != "BandwidthSettings")
                    return;
                
                var uploadLimitNode = root.SelectSingleNode("UploadLimitBytesPerSecond");
                if (uploadLimitNode != null)
                {
                    long.TryParse(uploadLimitNode.InnerText, out _globalUploadLimitBytesPerSecond);
                }
                
                var downloadLimitNode = root.SelectSingleNode("DownloadLimitBytesPerSecond");
                if (downloadLimitNode != null)
                {
                    long.TryParse(downloadLimitNode.InnerText, out _globalDownloadLimitBytesPerSecond);
                }
                
                var enabledNode = root.SelectSingleNode("IsBandwidthControlEnabled");
                if (enabledNode != null)
                {
                    bool.TryParse(enabledNode.InnerText, out _isBandwidthControlEnabled);
                }
            }
            catch (Exception ex)
            {
                // Log error and use defaults
                System.Diagnostics.Debug.WriteLine($"Error loading bandwidth settings: {ex.Message}");
                _globalUploadLimitBytesPerSecond = 0;
                _globalDownloadLimitBytesPerSecond = 0;
                _isBandwidthControlEnabled = false;
            }
        }
        
        /// <summary>
        /// Format bytes per second for display
        /// </summary>
        private string FormatBytesPerSecond(long bytesPerSecond)
        {
            if (bytesPerSecond >= 1073741824) // GB/s
                return string.Format("{0:F1} GB/s", (double)bytesPerSecond / 1073741824);
            if (bytesPerSecond >= 1048576) // MB/s
                return string.Format("{0:F1} MB/s", (double)bytesPerSecond / 1048576);
            if (bytesPerSecond >= 1024) // KB/s
                return string.Format("{0:F0} KB/s", (double)bytesPerSecond / 1024);
            return string.Format("{0} B/s", bytesPerSecond);
        }
        
        /// <summary>
        /// Fire the settings changed event
        /// </summary>
        private void OnBandwidthSettingsChanged()
        {
            SaveSettings();
            BandwidthSettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            _globalUploadLimitBytesPerSecond = 0;
            _globalDownloadLimitBytesPerSecond = 0;
            _isBandwidthControlEnabled = false;
            OnBandwidthSettingsChanged();
        }
        
        /// <summary>
        /// Check if current transfer speed exceeds limits
        /// </summary>
        public bool IsSpeedWithinLimits(double currentSpeedBytesPerSecond, bool isUpload)
        {
            if (!_isBandwidthControlEnabled)
                return true;
                
            long limit = isUpload ? _globalUploadLimitBytesPerSecond : _globalDownloadLimitBytesPerSecond;
            if (limit == 0) // Unlimited
                return true;
                
            return currentSpeedBytesPerSecond <= limit;
        }
    }
}
