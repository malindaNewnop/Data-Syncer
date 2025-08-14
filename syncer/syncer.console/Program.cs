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
            Console.WriteLine("Starting DataSyncer (console mode). Press Ctrl+C to stop...");

            // Wire Ctrl+C
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                _quit.Set();
            };

            // Create core services via factory
            var repo = Core.ServiceFactory.CreateJobRepository();
            var log = Core.ServiceFactory.CreateLogService();
            var factory = Core.ServiceFactory.CreateTransferClientFactory();
            var fileEnum = Core.ServiceFactory.CreateFileEnumerator();
            var runner = Core.ServiceFactory.CreateJobRunner(factory, log, fileEnum);

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
                        runner.RunJob(job);
                        job.LastRun = now;
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
