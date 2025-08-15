using System;
using System.IO;

namespace syncer.core
{
    /// <summary>
    /// Factory class to help create service instances
    /// </summary>
    public static class ServiceFactory
    {
        public static IJobRepository CreateJobRepository()
        {
            ILogService logService = CreateLogService();
            return new XmlJobRepository(logService);
        }

        public static ILogService CreateLogService()
        {
            return new FileLogService();
        }

        public static ITransferClientFactory CreateTransferClientFactory()
        {
            return new TransferClientFactory();
        }

        public static IFileEnumerator CreateFileEnumerator()
        {
            return new FileEnumerator();
        }

        public static IJobRunner CreateJobRunner(ITransferClientFactory factory, ILogService logService, IFileEnumerator fileEnumerator)
        {
            return new JobRunner(factory, logService, fileEnumerator);
        }

        /// <summary>
        /// Creates a multi-job runner that can execute multiple jobs concurrently
        /// </summary>
        public static IMultiJobRunner CreateMultiJobRunner()
        {
            var logService = CreateLogService();
            var jobRepository = CreateJobRepository();
            var jobQueueService = CreateJobQueueService(logService);
            var transferClientFactory = CreateTransferClientFactory();
            var fileEnumerator = CreateFileEnumerator();

            return new MultiJobRunner(
                jobRepository,
                jobQueueService,
                logService,
                transferClientFactory,
                fileEnumerator
            );
        }

        /// <summary>
        /// Creates a job queue service for managing job execution queues
        /// </summary>
        public static IJobQueueService CreateJobQueueService(ILogService logService = null)
        {
            return new XmlJobQueueService(logService ?? CreateLogService());
        }

        /// <summary>
        /// Creates a multi-job configuration service
        /// </summary>
        public static MultiJobConfigurationService CreateMultiJobConfigurationService()
        {
            return new MultiJobConfigurationService(CreateLogService());
        }

        /// <summary>
        /// Creates the appropriate job runner based on configuration
        /// Use this method to get either single or multi-job runner based on system settings
        /// </summary>
        public static IJobRunner CreateJobRunnerFromConfiguration()
        {
            try
            {
                var configService = CreateMultiJobConfigurationService();
                var config = configService.GetConfiguration();

                // If global max concurrent jobs is set to 1, use single job runner for compatibility
                if (config.GlobalMaxConcurrentJobs <= 1)
                {
                    var factory = CreateTransferClientFactory();
                    var logService = CreateLogService();
                    var fileEnumerator = CreateFileEnumerator();
                    return CreateJobRunner(factory, logService, fileEnumerator);
                }
                else
                {
                    // Use multi-job runner for concurrent execution
                    return CreateMultiJobRunner();
                }
            }
            catch (Exception)
            {
                // Fallback to single job runner if there's any issue with configuration
                var factory = CreateTransferClientFactory();
                var logService = CreateLogService();
                var fileEnumerator = CreateFileEnumerator();
                return CreateJobRunner(factory, logService, fileEnumerator);
            }
        }

        public static IPreviewService CreatePreviewService(IFileEnumerator fileEnumerator, ITransferClientFactory factory)
        {
            return new PreviewService(fileEnumerator, factory);
        }

        public static IJobScheduler CreateJobScheduler(IJobRepository repository, IJobRunner jobRunner, ILogService logService)
        {
            return new JobScheduler(repository, jobRunner, logService);
        }

        /// <summary>
        /// Creates an enhanced job scheduler with multi-job support
        /// </summary>
        public static IJobScheduler CreateEnhancedJobScheduler()
        {
            var repository = CreateJobRepository();
            var jobRunner = CreateJobRunnerFromConfiguration();
            var logService = CreateLogService();
            
            return new JobScheduler(repository, jobRunner, logService);
        }
    }
}