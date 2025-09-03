using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Timers;
using Core = syncer.core;

namespace syncer.service
{
    public partial class Service1 : ServiceBase
    {
        private Timer _timer;
        private bool _running;

        private Core.IJobRepository _repo;
        private Core.ILogService _log;
        private Core.ITransferClientFactory _factory;
        private Core.IFileEnumerator _fileEnumerator;
        private Core.IJobRunner _runner;

        public Service1()
        {
            InitializeComponent();
            this.ServiceName = "FTPSyncerService"; // Set the service name
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                // Using enhanced service factory methods for multi-job support
                _repo = Core.ServiceFactory.CreateJobRepository();
                _log = Core.ServiceFactory.CreateLogService();
                _factory = Core.ServiceFactory.CreateTransferClientFactory();
                _fileEnumerator = Core.ServiceFactory.CreateFileEnumerator();
                
                // Use the new multi-job runner from configuration
                _runner = Core.ServiceFactory.CreateJobRunnerFromConfiguration();

                _timer = new Timer();
                _timer.Interval = 60 * 1000;
                _timer.AutoReset = true;
                _timer.Elapsed += OnTick;
                _timer.Start();

                _log.LogInfo(null, "FTPSyncer Service started with multi-job support");
                OnTick(this, null);
            }
            catch (Exception ex)
            {
                if (_log != null)
                    _log.LogError(null, "Service start failed: " + ex.Message);
                throw; // Re-throw to indicate service start failure
            }
        }

        protected override void OnStop()
        {
            try
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Elapsed -= OnTick;
                    _timer.Dispose();
                    _timer = null;
                }
                if (_log != null) _log.LogInfo(null, "Service stopped");
            }
            catch (Exception ex)
            {
                if (_log != null)
                    _log.LogError(null, "Service stop error: " + ex.Message);
            }
        }

        private void OnTick(object sender, ElapsedEventArgs e)
        {
            if (_running) return;
            _running = true;
            try
            {
                List<Core.SyncJob> jobs = _repo.LoadAll();
                DateTime now = DateTime.Now;
                for (int i = 0; i < jobs.Count; i++)
                {
                    Core.SyncJob job = jobs[i];
                    if (!job.IsEnabled) continue;
                    if (!IsDue(job, now)) continue;

                    try
                    {
                        // Use StartJob method which is available in IJobRunner
                        if (_runner.StartJob(job.Id))
                        {
                            job.LastRun = now;
                            _log.LogInfo(job.Id, $"Started job '{job.Name}'");
                        }
                        else
                        {
                            _log.LogWarning(job.Id, $"Failed to start job '{job.Name}' - may be already running or queued");
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(job.Id, "Job execution failed: " + ex.Message);
                    }
                }
                _repo.SaveAll(jobs);
            }
            catch (Exception ex)
            {
                if (_log != null) _log.LogError(null, "Tick error: " + ex.Message);
            }
            finally
            {
                _running = false;
            }
        }

        private static bool IsDue(Core.SyncJob job, DateTime now)
        {
            if (job.Schedule == null) return true;
            if (job.LastRun == DateTime.MinValue) return now >= job.Schedule.StartTime;
            TimeSpan interval = GetInterval(job);
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