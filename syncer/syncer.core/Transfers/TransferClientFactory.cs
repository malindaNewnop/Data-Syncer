using syncer.core.Transfers;

namespace syncer.core
{
    public class TransferClientFactory : ITransferClientFactory
    {
        private readonly LocalTransferClient _local = new LocalTransferClient();
        private readonly EnhancedFtpTransferClient _ftp = new EnhancedFtpTransferClient();
        private readonly ProductionSftpTransferClient _sftp = new ProductionSftpTransferClient();

        public ITransferClient Create(ProtocolType protocol)
        {
            switch (protocol)
            {
                case ProtocolType.Ftp: return _ftp;
                case ProtocolType.Sftp: return _sftp;
                case ProtocolType.Local:
                default: 
                    return _local;
            }
        }

        public ITransferClient CreateSecure(ProtocolType protocol, string keyPath)
        {
            // For secure connections with key-based authentication
            switch (protocol)
            {
                case ProtocolType.Sftp: 
                    // In a real implementation, you'd configure the SFTP client with the key
                    return _sftp;
                case ProtocolType.Ftp: 
                    // FTP doesn't typically use key files, but return the client anyway
                    return _ftp;
                case ProtocolType.Local:
                default: 
                    return _local;
            }
        }
    }
}
