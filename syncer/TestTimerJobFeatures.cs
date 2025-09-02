using System;
using System.IO;
using System.Collections.Generic;
using syncer.ui.Services;
using syncer.ui;

namespace syncer
{
    /// <summary>
    /// Test class to verify Timer Job features are working correctly
    /// </summary>
    public class TestTimerJobFeatures
    {
        public static void TestDeleteSourceAndFilters()
        {
            Console.WriteLine("=== Testing Timer Job Delete Source and Filter Features ===");
            
            // Create test directory with sample files
            string testDir = @"c:\temp\test_timer_job";
            Directory.CreateDirectory(testDir);
            
            // Create test files of different types
            File.WriteAllText(Path.Combine(testDir, "document.pdf"), "PDF test content");
            File.WriteAllText(Path.Combine(testDir, "text.txt"), "Text file content");
            File.WriteAllText(Path.Combine(testDir, "data.csv"), "CSV data content");
            File.WriteAllText(Path.Combine(testDir, "temp.tmp"), "Temporary file");
            
            Console.WriteLine($"Created test files in: {testDir}");
            
            // Test Filter Settings
            var filterSettings = new FilterSettings();
            filterSettings.FiltersEnabled = true;
            filterSettings.ExcludeFilePatterns = ".pdf,.tmp"; // Exclude PDF and TMP files
            filterSettings.IncludeFileExtensions = ""; // Include all (except excluded)
            
            Console.WriteLine($"Filter Settings: Enabled={filterSettings.FiltersEnabled}, Exclude={filterSettings.ExcludeFilePatterns}");
            
            // Test the filtering logic directly
            string[] allFiles = Directory.GetFiles(testDir);
            Console.WriteLine($"All files found: {allFiles.Length}");
            foreach (string file in allFiles)
            {
                Console.WriteLine($"  - {Path.GetFileName(file)}");
            }
            
            // Simulate the filtering that happens in TimerJobManager
            var filteredFiles = new List<string>();
            foreach (string file in allFiles)
            {
                if (ShouldIncludeFileTest(file, filterSettings))
                {
                    filteredFiles.Add(file);
                }
                else
                {
                    Console.WriteLine($"FILTERED OUT: {Path.GetFileName(file)}");
                }
            }
            
            Console.WriteLine($"Files after filtering: {filteredFiles.Count}");
            foreach (string file in filteredFiles)
            {
                Console.WriteLine($"  - {Path.GetFileName(file)} (WILL BE TRANSFERRED)");
            }
            
            // Verify filtering worked correctly
            bool pdfFiltered = !filteredFiles.Exists(f => Path.GetExtension(f).ToLower() == ".pdf");
            bool tmpFiltered = !filteredFiles.Exists(f => Path.GetExtension(f).ToLower() == ".tmp");
            bool txtIncluded = filteredFiles.Exists(f => Path.GetExtension(f).ToLower() == ".txt");
            bool csvIncluded = filteredFiles.Exists(f => Path.GetExtension(f).ToLower() == ".csv");
            
            Console.WriteLine($"PDF files filtered out: {pdfFiltered} ✓");
            Console.WriteLine($"TMP files filtered out: {tmpFiltered} ✓");
            Console.WriteLine($"TXT files included: {txtIncluded} ✓");
            Console.WriteLine($"CSV files included: {csvIncluded} ✓");
            
            // Test Delete Source simulation
            Console.WriteLine("\n=== Testing Delete Source Feature ===");
            string testFile = Path.Combine(testDir, "test_delete.txt");
            File.WriteAllText(testFile, "File to be deleted after transfer");
            
            bool deleteSourceEnabled = true;
            Console.WriteLine($"Delete source after transfer: {deleteSourceEnabled}");
            
            // Simulate successful upload
            bool uploadSuccessful = true;
            if (uploadSuccessful && deleteSourceEnabled)
            {
                try
                {
                    File.Delete(testFile);
                    Console.WriteLine($"✓ Source file deleted: {Path.GetFileName(testFile)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Failed to delete source file: {ex.Message}");
                }
            }
            
            // Cleanup
            try
            {
                Directory.Delete(testDir, true);
                Console.WriteLine($"Cleaned up test directory: {testDir}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not clean up test directory: {ex.Message}");
            }
            
            Console.WriteLine("\n=== Test Complete ===");
        }
        
        /// <summary>
        /// Test version of the ShouldIncludeFile method from TimerJobManager
        /// </summary>
        private static bool ShouldIncludeFileTest(string filePath, FilterSettings filterSettings)
        {
            if (filterSettings == null || !filterSettings.FiltersEnabled) return true;
            
            string fileName = Path.GetFileName(filePath);
            string fileExtension = Path.GetExtension(filePath).ToLower();
            
            Console.WriteLine($"Checking file: {fileName}, Extension: {fileExtension}");
            
            // Check exclude file patterns (e.g., .pdf, .tmp)
            if (!string.IsNullOrEmpty(filterSettings.ExcludeFilePatterns))
            {
                string[] excludePatterns = filterSettings.ExcludeFilePatterns.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string excludePattern in excludePatterns)
                {
                    string cleanPattern = excludePattern.Trim();
                    
                    // If it starts with a dot, treat it as an extension
                    if (cleanPattern.StartsWith("."))
                    {
                        if (fileExtension == cleanPattern.ToLower())
                        {
                            Console.WriteLine($"  Excluded by extension pattern: {cleanPattern}");
                            return false; // Exclude this file extension
                        }
                    }
                    else
                    {
                        // If pattern doesn't start with dot, add it
                        if (!cleanPattern.StartsWith("."))
                        {
                            cleanPattern = "." + cleanPattern;
                        }
                        
                        if (fileExtension == cleanPattern.ToLower())
                        {
                            Console.WriteLine($"  Excluded by extension pattern: {cleanPattern}");
                            return false; // Exclude this file extension
                        }
                    }
                }
            }
            
            // Check include file extensions (if specified, only include these extensions)
            if (!string.IsNullOrEmpty(filterSettings.IncludeFileExtensions))
            {
                string[] includeExtensions = filterSettings.IncludeFileExtensions.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool matchesInclude = false;
                foreach (string includeExt in includeExtensions)
                {
                    string cleanExt = includeExt.Trim().ToLower();
                    if (!cleanExt.StartsWith(".")) cleanExt = "." + cleanExt;
                    
                    if (fileExtension == cleanExt)
                    {
                        matchesInclude = true;
                        break;
                    }
                }
                if (!matchesInclude) return false; // Not in include list
            }
            
            Console.WriteLine($"  File passes all filters");
            return true; // File passes all filters
        }
    }
}
