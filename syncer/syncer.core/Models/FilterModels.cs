using System;
using System.Collections.Generic;

namespace syncer.core.Models
{
    /// <summary>
    /// Settings for file filtering and validation
    /// </summary>
    public class FilterSettings
    {
        public List<string> IncludeExtensions { get; set; } = new List<string>();
        public List<string> ExcludeExtensions { get; set; } = new List<string>();
        public string IncludePattern { get; set; }
        public string ExcludePattern { get; set; }
        public long MinSizeKB { get; set; }
        public long MaxSizeKB { get; set; }
        public DateTime ModifiedAfter { get; set; } = DateTime.MinValue;
        public DateTime ModifiedBefore { get; set; } = DateTime.MinValue;
        public bool RecursiveSearch { get; set; } = true;
        public bool ValidateAfterTransfer { get; set; } = true;
        public ValidationOptions ValidationOptions { get; set; } = ValidationOptions.Existence | ValidationOptions.FileSize;
        public RelocationOptions SourceFileHandling { get; set; } = RelocationOptions.None;
        public string CustomRelocationPath { get; set; }
        
        public FilterSettings()
        {
            // Default constructor with empty lists
            IncludeExtensions = new List<string>();
            ExcludeExtensions = new List<string>();
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
                CustomRelocationPath = this.CustomRelocationPath
            };

            // Deep copy lists
            foreach (var ext in this.IncludeExtensions)
                clone.IncludeExtensions.Add(ext);

            foreach (var ext in this.ExcludeExtensions)
                clone.ExcludeExtensions.Add(ext);

            return clone;
        }
    }

    // Validation and Relocation option enums are now defined in the core namespace
}
