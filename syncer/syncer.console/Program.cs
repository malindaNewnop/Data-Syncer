using System;
using System.Threading;
using Core = syncer.core;

namespace syncer.console
{
    internal static class Program
    {
        private static ManualResetEvent _quit = new ManualResetEvent(false);

        private static void Main(string[] args)
        {
            Console.Title = "DataSyncer Console Runner";
            Console.WriteLine("Starting DataSyncer (console mode) with multi-job support. Press Ctrl+C to stop...");

            // Wire Ctrl+C
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                _quit.Set();
            };

            // Create core services via factory with multi-job support
            var repo = Core.ServiceFactory.CreateJobRepository();
            var log = Core.ServiceFactory.CreateLogService();
            var runner = Core.ServiceFactory.CreateJobRunnerFromConfiguration();

            log.LogInfo(null, "DataSyncer console started with multi-job capabilities");

            // Basic loop: run due jobs every minute (mirrors service timer)
            var timer = new Timer(_ => Tick(repo, runner, log), null, 0, 60 * 1000);

            // Wait until Ctrl+C
            _quit.WaitOne();

            timer.Dispose();
            log.LogInfo(null, "Console runner stopped");
        }

        private static void Tick(Core.IJobRepository repo, Core.IJobRunner runner, Core.ILogService log)
        {
            try
            {
                var jobs = repo.LoadAll();
                var now = DateTime.Now;
                for (int i = 0; i < jobs.Count; i++)
                {
                    var job = jobs[i];
                    if (!job.IsEnabled) continue;
                    if (!IsDue(job, now)) continue;

                    try
                    {
                        // Use StartJob method which is available in IJobRunner
                        if (runner.StartJob(job.Id))
                        {
                            job.LastRun = now;
                            log.LogInfo(job.Id, $"Started job '{job.Name}'");
                        }
                        else
                        {
                            log.LogWarning(job.Id, $"Failed to start job '{job.Name}' - may be already running or queued");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogError(job.Id, "Job execution failed: " + ex.Message);
                    }
                }
                repo.SaveAll(jobs);
            }
            catch (Exception ex)
            {
                log.LogError(null, "Tick error: " + ex.Message);
            }
        }

        private static bool IsDue(Core.SyncJob job, DateTime now)
        {
            if (job.Schedule == null) return true;
            if (job.LastRun == DateTime.MinValue) return now >= job.Schedule.StartTime;
            var interval = GetInterval(job);
            return job.LastRun.Add(interval) <= now;
        }

        private static TimeSpan GetInterval(Core.SyncJob job)
        {
            int every = (job.Schedule == null) ? 60 : job.Schedule.RepeatEvery;
            Core.TimeUnit unit = (job.Schedule == null) ? Core.TimeUnit.Minutes : job.Schedule.Unit;
            if (every <= 0) every = 60;
            switch (unit)
            {
                case Core.TimeUnit.Hours: return TimeSpan.FromHours(every);
                default: return TimeSpan.FromMinutes(every);
            }
        }
    }
}
