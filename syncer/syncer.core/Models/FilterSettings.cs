using System;
using System.Collections.Generic;

namespace syncer.core.Models
{
    /// <summary>
    /// Settings for file filtering and validation - Unified model for UI and Core
    /// Compatible with .NET 3.5
    /// </summary>
    public class FilterSettings
    {
        // Core filtering properties
        public List<string> IncludeExtensions { get; set; }
        public List<string> ExcludeExtensions { get; set; }
        public string IncludePattern { get; set; }
        public string ExcludePattern { get; set; }
        public long MinSizeKB { get; set; }
        public long MaxSizeKB { get; set; }
        public DateTime ModifiedAfter { get; set; }
        public DateTime ModifiedBefore { get; set; }
        public bool RecursiveSearch { get; set; }
        public bool ValidateAfterTransfer { get; set; }
        public ValidationOptions ValidationOptions { get; set; }
        public RelocationOptions SourceFileHandling { get; set; }
        public string CustomRelocationPath { get; set; }
        
        // UI compatibility properties
        public bool FiltersEnabled { get; set; }
        public string[] AllowedFileTypes { get; set; }
        public decimal MinFileSize { get; set; }
        public decimal MaxFileSize { get; set; }
        public bool IncludeHiddenFiles { get; set; }
        public bool IncludeSystemFiles { get; set; }
        public bool IncludeReadOnlyFiles { get; set; }
        public string ExcludePatterns { get; set; }
        public bool IncludeSubfolders { get; set; }
        
        public FilterSettings()
        {
            // Initialize collections (.NET 3.5 compatible)
            IncludeExtensions = new List<string>();
            ExcludeExtensions = new List<string>();
            
            // Set defaults
            FiltersEnabled = true;
            MinFileSize = 0;
            MaxFileSize = 100;
            MinSizeKB = 0;
            MaxSizeKB = 102400; // 100MB in KB
            IncludeReadOnlyFiles = true;
            IncludeSubfolders = true;
            RecursiveSearch = true;
            ValidateAfterTransfer = true;
            ModifiedAfter = DateTime.MinValue;
            ModifiedBefore = DateTime.MinValue;
            ValidationOptions = ValidationOptions.Existence | ValidationOptions.FileSize;
            SourceFileHandling = RelocationOptions.None;
        }

        
        /// <summary>
        /// Convert UI file type selections to core extension lists
        /// </summary>
        public void UpdateExtensionsFromFileTypes()
        {
            IncludeExtensions.Clear();
            
            if (AllowedFileTypes != null)
            {
                foreach (string fileType in AllowedFileTypes)
                {
                    // Extract extension from display format like ".txt - Text files"
                    string extension = ExtractExtensionFromDisplayString(fileType);
                    if (!string.IsNullOrEmpty(extension) && !IncludeExtensions.Contains(extension))
                    {
                        IncludeExtensions.Add(extension);
                    }
                }
            }
        }
        
        /// <summary>
        /// Extract extension from display string format
        /// </summary>
        private string ExtractExtensionFromDisplayString(string displayString)
        {
            if (string.IsNullOrEmpty(displayString)) return string.Empty;
            
            // Handle format like ".txt - Text files" or just ".txt"
            int dashIndex = displayString.IndexOf(" - ");
            string extension = dashIndex > 0 ? displayString.Substring(0, dashIndex).Trim() : displayString.Trim();
            
            // Ensure it starts with a dot and contains only valid characters
            if (!extension.StartsWith(".")) extension = "." + extension;
            
            return extension.ToLowerInvariant();
        }
        
        /// <summary>
        /// Update size properties to maintain consistency between UI and Core
        /// </summary>
        public void UpdateSizeProperties()
        {
            // Convert MB to KB for core filtering
            MinSizeKB = (long)(MinFileSize * 1024);
            MaxSizeKB = (long)(MaxFileSize * 1024);
            
            // Update exclude patterns
            ExcludePattern = ExcludePatterns;
            
            // Update recursive search
            RecursiveSearch = IncludeSubfolders;
        }

        public FilterSettings Clone()
        {
            var clone = new FilterSettings
            {
                IncludePattern = this.IncludePattern,
                ExcludePattern = this.ExcludePattern,
                MinSizeKB = this.MinSizeKB,
                MaxSizeKB = this.MaxSizeKB,
                ModifiedAfter = this.ModifiedAfter,
                ModifiedBefore = this.ModifiedBefore,
                RecursiveSearch = this.RecursiveSearch,
                ValidateAfterTransfer = this.ValidateAfterTransfer,
                ValidationOptions = this.ValidationOptions,
                SourceFileHandling = this.SourceFileHandling,
                CustomRelocationPath = this.CustomRelocationPath,
                // UI properties
                FiltersEnabled = this.FiltersEnabled,
                MinFileSize = this.MinFileSize,
                MaxFileSize = this.MaxFileSize,
                IncludeHiddenFiles = this.IncludeHiddenFiles,
                IncludeSystemFiles = this.IncludeSystemFiles,
                IncludeReadOnlyFiles = this.IncludeReadOnlyFiles,
                ExcludePatterns = this.ExcludePatterns,
                IncludeSubfolders = this.IncludeSubfolders
            };

            // Deep copy lists
            foreach (var ext in this.IncludeExtensions)
                clone.IncludeExtensions.Add(ext);

            foreach (var ext in this.ExcludeExtensions)
                clone.ExcludeExtensions.Add(ext);
                
            // Deep copy allowed file types
            if (this.AllowedFileTypes != null)
            {
                clone.AllowedFileTypes = new string[this.AllowedFileTypes.Length];
                for (int i = 0; i < this.AllowedFileTypes.Length; i++)
                {
                    clone.AllowedFileTypes[i] = this.AllowedFileTypes[i];
                }
            }

            return clone;
        }
    }
}
