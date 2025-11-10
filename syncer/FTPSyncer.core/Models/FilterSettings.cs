using System;
using System.Collections.Generic;

namespace FTPSyncer.core
{
    /// <summary>
    /// Settings for file filtering and validation
    /// .NET 3.5 Compatible
    /// </summary>
    [Serializable]
    public class FilterSettings
    {
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
        
        public FilterSettings()
        {
            // Default constructor with empty lists
            IncludeExtensions = new List<string>();
            ExcludeExtensions = new List<string>();
            IncludePattern = "";
            ExcludePattern = "";
            MinSizeKB = 0;
            MaxSizeKB = 0;
            ModifiedAfter = DateTime.MinValue;
            ModifiedBefore = DateTime.MinValue;
            RecursiveSearch = true;
            ValidateAfterTransfer = true;
            ValidationOptions = ValidationOptions.Existence | ValidationOptions.FileSize;
            SourceFileHandling = RelocationOptions.None;
            CustomRelocationPath = "";
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

    [Flags]
    public enum ValidationOptions
    {
        None = 0,
        Existence = 1,
        FileSize = 2,
        Checksum = 4,
        ModificationDate = 8,
        All = Existence | FileSize | Checksum | ModificationDate
    }

    public enum RelocationOptions
    {
        None,
        Delete,
        Archive,
        CustomFolder
    }
}





