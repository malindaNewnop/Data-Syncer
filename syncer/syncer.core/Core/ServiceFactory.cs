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
            return new XmlJobRepository();
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
