using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace syncer.ui.Services
{
    /// <summary>
    /// Centralized Job ID Generator that creates human-readable IDs in format MMDD-XXX
    /// Example: 1110-001, 1110-002 for November 10th
    /// </summary>
    public class JobIdGenerator
    {
        private static JobIdGenerator _instance;
        private static readonly object _lock = new object();
        
        private readonly string _stateFilePath;
        private Dictionary<string, int> _dailyCounters; // Key: MMDD, Value: last counter
        private DateTime _lastResetCheck;

        private JobIdGenerator()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string syncerFolder = Path.Combine(appDataPath, "DataSyncer");
            _stateFilePath = Path.Combine(syncerFolder, "job_id_state.json");
            
            _dailyCounters = new Dictionary<string, int>();
            _lastResetCheck = DateTime.Now.Date;
            
            LoadState();
            CleanupOldCounters();
        }

        /// <summary>
        /// Gets the singleton instance of the JobIdGenerator
        /// </summary>
        public static JobIdGenerator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new JobIdGenerator();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Generates a new Job ID in format MMDD-XXX
        /// </summary>
        /// <returns>Job ID string like "1110-001"</returns>
        public string GenerateJobId()
        {
            lock (_lock)
            {
                CheckAndResetIfNewDay();
                
                string datePrefix = DateTime.Now.ToString("MMdd");
                
                // Get current counter for today
                if (!_dailyCounters.ContainsKey(datePrefix))
                {
                    _dailyCounters[datePrefix] = 0;
                }
                
                // Increment counter
                _dailyCounters[datePrefix]++;
                int counter = _dailyCounters[datePrefix];
                
                // Format: MMDD-XXX
                string jobId = $"{datePrefix}-{counter:D3}";
                
                // Save state
                SaveState();
                
                return jobId;
            }
        }

        /// <summary>
        /// Generates a numeric Job ID (long) based on the formatted ID
        /// This maintains backward compatibility with systems expecting long IDs
        /// </summary>
        /// <returns>Long representation: MMDDxxx (e.g., 1110001 for 1110-001)</returns>
        public long GenerateNumericJobId()
        {
            string formattedId = GenerateJobId();
            string numericPart = formattedId.Replace("-", "");
            return long.Parse(numericPart);
        }

        /// <summary>
        /// Converts a formatted Job ID to its numeric representation
        /// </summary>
        /// <param name="formattedId">Job ID like "1110-001"</param>
        /// <returns>Numeric ID like 1110001</returns>
        public static long ToNumericId(string formattedId)
        {
            if (string.IsNullOrEmpty(formattedId))
                throw new ArgumentException("Job ID cannot be null or empty");
            
            string numericPart = formattedId.Replace("-", "");
            return long.Parse(numericPart);
        }

        /// <summary>
        /// Converts a numeric Job ID back to formatted string
        /// </summary>
        /// <param name="numericId">Numeric ID like 1110001</param>
        /// <returns>Formatted ID like "1110-001"</returns>
        public static string ToFormattedId(long numericId)
        {
            string numericStr = numericId.ToString();
            if (numericStr.Length < 7)
                numericStr = numericStr.PadLeft(7, '0');
            
            string datePrefix = numericStr.Substring(0, 4);
            string counter = numericStr.Substring(4);
            
            return $"{datePrefix}-{counter}";
        }

        /// <summary>
        /// Gets the current counter value for today without incrementing
        /// </summary>
        public int GetTodayCounter()
        {
            lock (_lock)
            {
                string datePrefix = DateTime.Now.ToString("MMdd");
                return _dailyCounters.ContainsKey(datePrefix) ? _dailyCounters[datePrefix] : 0;
            }
        }

        /// <summary>
        /// Resets the counter for a specific date (useful for testing)
        /// </summary>
        public void ResetCounter(string datePrefix = null)
        {
            lock (_lock)
            {
                if (datePrefix == null)
                    datePrefix = DateTime.Now.ToString("MMdd");
                
                _dailyCounters[datePrefix] = 0;
                SaveState();
            }
        }

        /// <summary>
        /// Checks if we've moved to a new day and performs cleanup
        /// </summary>
        private void CheckAndResetIfNewDay()
        {
            DateTime now = DateTime.Now.Date;
            if (now > _lastResetCheck)
            {
                _lastResetCheck = now;
                CleanupOldCounters();
            }
        }

        /// <summary>
        /// Removes counters older than 30 days to keep state file small
        /// </summary>
        private void CleanupOldCounters()
        {
            var cutoffDate = DateTime.Now.AddDays(-30);
            var keysToRemove = new List<string>();
            
            foreach (var key in _dailyCounters.Keys)
            {
                try
                {
                    int month = int.Parse(key.Substring(0, 2));
                    int day = int.Parse(key.Substring(2, 2));
                    var counterDate = new DateTime(DateTime.Now.Year, month, day);
                    
                    if (counterDate < cutoffDate)
                    {
                        keysToRemove.Add(key);
                    }
                }
                catch
                {
                    // If we can't parse it, remove it
                    keysToRemove.Add(key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                _dailyCounters.Remove(key);
            }
            
            if (keysToRemove.Count > 0)
            {
                SaveState();
            }
        }

        /// <summary>
        /// Loads the state from disk
        /// </summary>
        private void LoadState()
        {
            try
            {
                if (File.Exists(_stateFilePath))
                {
                    string json = File.ReadAllText(_stateFilePath);
                    var state = JsonConvert.DeserializeObject<JobIdGeneratorState>(json);
                    
                    if (state != null && state.DailyCounters != null)
                    {
                        _dailyCounters = state.DailyCounters;
                    }
                }
            }
            catch (Exception ex)
            {
                // If we can't load state, start fresh
                ServiceLocator.LogService?.LogWarning(
                    $"Failed to load Job ID state: {ex.Message}. Starting with fresh state.",
                    "JobIdGenerator"
                );
                _dailyCounters = new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Saves the state to disk
        /// </summary>
        private void SaveState()
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(_stateFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                var state = new JobIdGeneratorState
                {
                    DailyCounters = _dailyCounters,
                    LastUpdated = DateTime.Now
                };
                
                string json = JsonConvert.SerializeObject(state, Formatting.Indented);
                File.WriteAllText(_stateFilePath, json);
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService?.LogError(
                    $"Failed to save Job ID state: {ex.Message}",
                    "JobIdGenerator"
                );
            }
        }

        /// <summary>
        /// State class for serialization
        /// </summary>
        [Serializable]
        private class JobIdGeneratorState
        {
            public Dictionary<string, int> DailyCounters { get; set; }
            public DateTime LastUpdated { get; set; }
        }
    }
}
