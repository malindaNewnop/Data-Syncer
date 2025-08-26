using System;
using System.Data;
using System.Windows.Forms;
using syncer.ui;

namespace LogTest
{
    class Program
    {
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                // Create a simple test log service
                var logService = new TestLogService();
                
                // Create and show the FormLogs
                var formLogs = new FormLogs(logService);
                
                Console.WriteLine("FormLogs created successfully - no NullReferenceException!");
                
                // Test loading logs
                formLogs.RefreshLogs();
                Console.WriteLine("Logs refreshed successfully!");
                
                // Run the form
                Application.Run(formLogs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
    
    public class TestLogService : syncer.core.Interfaces.ILogService
    {
        public DataTable GetLogs()
        {
            var table = new DataTable();
            table.Columns.Add("DateTime", typeof(DateTime));
            table.Columns.Add("Level", typeof(string));
            table.Columns.Add("Job", typeof(string));
            table.Columns.Add("File", typeof(string));
            table.Columns.Add("Status", typeof(string));
            table.Columns.Add("Message", typeof(string));
            
            // Add some test data
            var row1 = table.NewRow();
            row1["DateTime"] = DateTime.Now.AddMinutes(-5);
            row1["Level"] = "INFO";
            row1["Job"] = "TestJob";
            row1["File"] = "test.txt";
            row1["Status"] = "Success";
            row1["Message"] = "File uploaded successfully";
            table.Rows.Add(row1);
            
            var row2 = table.NewRow();
            row2["DateTime"] = DateTime.Now.AddMinutes(-3);
            row2["Level"] = "WARNING";
            row2["Job"] = "BackupJob";
            row2["File"] = "backup.zip";
            row2["Status"] = "Partial";
            row2["Message"] = "Backup completed with warnings";
            table.Rows.Add(row2);
            
            var row3 = table.NewRow();
            row3["DateTime"] = DateTime.Now.AddMinutes(-1);
            row3["Level"] = "ERROR";
            row3["Job"] = "SyncJob";
            row3["File"] = "sync.dat";
            row3["Status"] = "Failed";
            row3["Message"] = "Connection timeout during sync operation";
            table.Rows.Add(row3);
            
            return table;
        }
        
        public void LogInfo(string message, string source = "")
        {
            Console.WriteLine($"INFO [{source}]: {message}");
        }
        
        public void LogWarning(string message, string source = "")
        {
            Console.WriteLine($"WARNING [{source}]: {message}");
        }
        
        public void LogError(string message, string source = "")
        {
            Console.WriteLine($"ERROR [{source}]: {message}");
        }
        
        public void LogDebug(string message, string source = "")
        {
            Console.WriteLine($"DEBUG [{source}]: {message}");
        }
    }
}
