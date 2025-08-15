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
            return new SqliteJobRepository(logService);
        }

        public static ILogService CreateLogService()
        {
            return new SqliteLogService();
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
            return new ParallelJobRunner(CreateJobRepository(), logService);
        }

        public static IPreviewService CreatePreviewService(IFileEnumerator fileEnumerator, ITransferClientFactory factory)
        {
            return new PreviewService(fileEnumerator, factory);
        }

        public static IJobScheduler CreateJobScheduler(IJobRepository repository, IJobRunner jobRunner, ILogService logService)
        {
            return new JobScheduler(repository, jobRunner, logService);
        }
    }
}
