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
        private Core.IJobRunner _runner;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _repo = new Core.XmlJobRepository();
            _log = new Core.FileLogService();
            _factory = new Core.TransferClientFactory();
            _runner = new Core.JobRunner(_log, _factory);

            _timer = new Timer();
            _timer.Interval = 60 * 1000;
            _timer.AutoReset = true;
            _timer.Elapsed += OnTick;
            _timer.Start();

            OnTick(this, null);
        }

        protected override void OnStop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= OnTick;
                _timer.Dispose();
                _timer = null;
            }
            if (_log != null) _log.Info("Service stopped", "Service");
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
                    if (!job.Enabled) continue;
                    if (!IsDue(job, now)) continue;

                    try
                    {
                        int count; string error;
                        bool ok = _runner.Run(job, out count, out error);
                        job.LastRun = now;
                        if (ok) _log.Info("Job completed, files transferred: " + count, job.Name);
                        else _log.Warning("Job finished with errors: " + error, job.Name);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Job execution failed: " + ex.Message, job.Name, ex);
                    }
                }
                _repo.SaveAll(jobs);
            }
            catch (Exception ex)
            {
                if (_log != null) _log.Error("Tick error: " + ex.Message, "Service", ex);
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
