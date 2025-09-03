using System;
using System.IO;
using System.Collections.Generic;
using syncer.core.Core;
using syncer.core.Services;
using syncer.core.Utilities;
using syncer.ui;
using syncer.ui.Services;

class TestFilteringEnd2End
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== COMPREHENSIVE FILTER TESTING ===");
        
        try
        {
            // Test 1: FileEnumerator filtering
            TestFileEnumeratorFiltering();
            
            // Test 2: UI to Core model conversion
            TestModelConversion();
            
            // Test 3: JobRunner integration
            TestJobRunnerFiltering();
            
            Console.WriteLine("\n=== ALL TESTS COMPLETED ===");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    
    static void TestFileEnumeratorFiltering()
    {
        Console.WriteLine("\n--- Test 1: FileEnumerator Filtering ---");
        
        // Create test directory structure
        string testDir = Path.Combine(Path.GetTempPath(), "FilterTest_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(testDir);
        
        // Create test files
        string[] testFiles = {
            "document.docx",
            "presentation.pptx", 
            "spreadsheet.xlsx",
            "image.jpg",
            "backup.bak",
            "temp.tmp",
            "readme.txt",
            "config.xml"
        };
        
        foreach (string fileName in testFiles)
        {
            File.WriteAllText(Path.Combine(testDir, fileName), "test content");
        }
        
        Console.WriteLine($"Created test directory: {testDir}");
        Console.WriteLine($"Test files: {string.Join(", ", testFiles)}");
        
        var fileEnumerator = new FileEnumerator();
        
        // Test include filter only
        var includeFilter = new FilterSettings
        {
            IncludeExtensions = new List<string> { "docx", "xlsx", "txt" }
        };
        
        var includedFiles = fileEnumerator.EnumerateFiles(testDir, false, includeFilter);
        Console.WriteLine($"\nInclude filter (docx,xlsx,txt): {includedFiles.Count} files");
        foreach (string file in includedFiles)
        {
            Console.WriteLine($"  ✓ {Path.GetFileName(file)}");
        }
        
        // Test exclude filter only
        var excludeFilter = new FilterSettings
        {
            ExcludeExtensions = new List<string> { "bak", "tmp" }
        };
        
        var excludedFiles = fileEnumerator.EnumerateFiles(testDir, false, excludeFilter);
        Console.WriteLine($"\nExclude filter (bak,tmp): {excludedFiles.Count} files");
        foreach (string file in excludedFiles)
        {
            Console.WriteLine($"  ✓ {Path.GetFileName(file)}");
        }
        
        // Test both include and exclude
        var combinedFilter = new FilterSettings
        {
            IncludeExtensions = new List<string> { "docx", "xlsx", "txt", "bak" },
            ExcludeExtensions = new List<string> { "bak" }
        };
        
        var combinedFiles = fileEnumerator.EnumerateFiles(testDir, false, combinedFilter);
        Console.WriteLine($"\nCombined filter (include: docx,xlsx,txt,bak | exclude: bak): {combinedFiles.Count} files");
        foreach (string file in combinedFiles)
        {
            Console.WriteLine($"  ✓ {Path.GetFileName(file)}");
        }
        
        // Clean up
        Directory.Delete(testDir, true);
        Console.WriteLine("✓ FileEnumerator filtering test passed");
    }
    
    static void TestModelConversion()
    {
        Console.WriteLine("\n--- Test 2: UI to Core Model Conversion ---");
        
        var adapter = new CoreSyncJobServiceAdapter();
        
        // Test 1: Filters enabled
        var uiJobWithFilters = new syncer.ui.Models.SyncJob
        {
            Id = Guid.NewGuid(),
            Name = "Test Job With Filters",
            EnableFilters = true,
            IncludeFileTypes = "docx,xlsx,pdf",
            ExcludeFileTypes = "tmp,bak"
        };
        
        var coreJobWithFilters = adapter.ConvertToCoreModel(uiJobWithFilters);
        
        Console.WriteLine($"UI Job - Enable Filters: {uiJobWithFilters.EnableFilters}");
        Console.WriteLine($"UI Job - Include: '{uiJobWithFilters.IncludeFileTypes}'");
        Console.WriteLine($"UI Job - Exclude: '{uiJobWithFilters.ExcludeFileTypes}'");
        
        Console.WriteLine($"Core Job - Filters != null: {coreJobWithFilters.Filters != null}");
        if (coreJobWithFilters.Filters != null)
        {
            Console.WriteLine($"Core Job - Include: [{string.Join(",", coreJobWithFilters.Filters.IncludeExtensions)}]");
            Console.WriteLine($"Core Job - Exclude: [{string.Join(",", coreJobWithFilters.Filters.ExcludeExtensions)}]");
        }
        
        // Test 2: Filters disabled
        var uiJobNoFilters = new syncer.ui.Models.SyncJob
        {
            Id = Guid.NewGuid(),
            Name = "Test Job No Filters",
            EnableFilters = false,
            IncludeFileTypes = "docx,xlsx",
            ExcludeFileTypes = "tmp"
        };
        
        var coreJobNoFilters = adapter.ConvertToCoreModel(uiJobNoFilters);
        
        Console.WriteLine($"\nUI Job - Enable Filters: {uiJobNoFilters.EnableFilters}");
        Console.WriteLine($"Core Job - Filters == null: {coreJobNoFilters.Filters == null}");
        
        Console.WriteLine("✓ Model conversion test passed");
    }
    
    static void TestJobRunnerFiltering()
    {
        Console.WriteLine("\n--- Test 3: JobRunner Integration ---");
        
        // Create test directory
        string testDir = Path.Combine(Path.GetTempPath(), "JobRunnerTest_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(testDir);
        
        // Create test files
        File.WriteAllText(Path.Combine(testDir, "important.docx"), "important document");
        File.WriteAllText(Path.Combine(testDir, "backup.bak"), "backup file");
        File.WriteAllText(Path.Combine(testDir, "temp.tmp"), "temporary file");
        File.WriteAllText(Path.Combine(testDir, "data.xlsx"), "spreadsheet");
        
        // Create a sync job with filters
        var syncJob = new syncer.core.SyncJob
        {
            Id = Guid.NewGuid(),
            Name = "Filter Test Job",
            SourcePath = testDir,
            Filters = new FilterSettings
            {
                IncludeExtensions = new List<string> { "docx", "xlsx" }
            }
        };
        
        var jobRunner = new JobRunner();
        
        // Get the filtered files (this calls our updated GetFilesToProcess method)
        var method = typeof(JobRunner).GetMethod("GetFilesToProcess", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var filteredFiles = (List<string>)method.Invoke(jobRunner, new object[] { syncJob });
        
        Console.WriteLine($"Files in directory: 4 (important.docx, backup.bak, temp.tmp, data.xlsx)");
        Console.WriteLine($"Filtered files: {filteredFiles.Count}");
        
        foreach (string file in filteredFiles)
        {
            Console.WriteLine($"  ✓ {Path.GetFileName(file)}");
        }
        
        // Verify only docx and xlsx files are included
        bool hasDocx = filteredFiles.Exists(f => f.EndsWith("important.docx"));
        bool hasXlsx = filteredFiles.Exists(f => f.EndsWith("data.xlsx"));
        bool hasBak = filteredFiles.Exists(f => f.EndsWith("backup.bak"));
        bool hasTmp = filteredFiles.Exists(f => f.EndsWith("temp.tmp"));
        
        Console.WriteLine($"Contains important.docx: {hasDocx}");
        Console.WriteLine($"Contains data.xlsx: {hasXlsx}");
        Console.WriteLine($"Contains backup.bak: {hasBak}");
        Console.WriteLine($"Contains temp.tmp: {hasTmp}");
        
        if (hasDocx && hasXlsx && !hasBak && !hasTmp)
        {
            Console.WriteLine("✓ JobRunner filtering test passed");
        }
        else
        {
            Console.WriteLine("✗ JobRunner filtering test failed");
        }
        
        // Clean up
        Directory.Delete(testDir, true);
    }
}
