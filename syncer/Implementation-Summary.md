# Data Syncer - SQLite and Multiple Jobs Implementation

## Overview
This document summarizes the implementation of SQLite-based storage replacing XML files and the addition of support for running multiple jobs in parallel.

## Key Components Added/Modified

### 1. Core Components
- **SqliteJobRepository** - Replaces XmlJobRepository for job persistence
- **SqliteLogService** - Replaces FileLogService for logging
- **DatabaseHelper** - New centralized utility for database operations
- **ParallelJobRunner** - New implementation supporting multiple concurrent jobs
- **ServiceFactory** - Updated to use the new SQLite-based components
- **IJobRunner** - Updated interface to support multiple jobs

### 2. Database Structure
The SQLite database includes the following tables:
- **Jobs** - Stores all job configuration and status information
- **Logs** - Stores application and job logs
- **Settings** - Stores application settings

### 3. Migration Support
- Automatic migration from XML to SQLite on first run
- Backup of XML file after successful migration

### 4. Features Added
- Multiple jobs can run simultaneously (parallel execution)
- More robust data storage with transaction support
- Automatic database backups
- Database maintenance utilities
- Improved logging with filtering capabilities

## Implementation Details

### Database Location
The SQLite database is stored in the local application data folder:
```
%LOCALAPPDATA%\DataSyncer\syncer.db
```

### Job Persistence
- Jobs are now stored in the SQLite database
- Complex objects (like Connection settings, Filters, etc.) are serialized as JSON

### Logging
- Logs are now stored in the SQLite database
- Logs can be queried by level, source, or job ID
- Automatic cleanup of old logs based on retention settings

### Parallel Job Execution
- Each job runs in its own thread
- Jobs can be individually started, stopped, and monitored
- Central management through ParallelJobRunner

## How to Use

### Running Multiple Jobs
Jobs can now be added, scheduled, and run independently without interfering with each other.

### Viewing Logs
Logs are stored in the database and can be viewed through the application's log viewer.

### Database Maintenance
The database is automatically maintained, including:
- Backups before significant operations
- Cleanup of old logs
- Database optimization (VACUUM and ANALYZE)

## Technical Notes

### Dependencies
- System.Data.SQLite.Core (1.0.115.5)
- Newtonsoft.Json (for serializing complex objects)

### Performance Considerations
- Connection pooling is used for database access
- Transactions are used for batch operations
- Proper indexing on frequently queried columns

### Error Handling
- All database operations have proper error handling
- Transaction rollback on failure
- Detailed logging of errors

## Troubleshooting

### Common Issues
1. **Database Access Errors**
   - Ensure the application has write access to the %LOCALAPPDATA%\DataSyncer folder
   - Check for database file corruption

2. **Missing Jobs After Upgrade**
   - Check for backup files in %LOCALAPPDATA%\DataSyncer\Backups
   - Verify successful migration from XML

3. **Performance Issues**
   - Try running database maintenance manually
   - Check disk space and I/O performance

### Diagnostics
- Enable debug logging for more detailed information
- Check the application logs for database-related errors

## Enhanced SFTP Implementation Summary

The previous implementation focused on SFTP features. This new implementation adds SQLite storage and multiple job support while maintaining all the previous SFTP capabilities.

### Security Considerations
- **Secure credential handling** with encrypted storage options
- **Key passphrase protection** and secure memory handling
- **Connection encryption** using SSH protocol
- **Host key verification** support
- **Audit trail** via comprehensive logging

### Error Recovery
- **Graceful connection handling** with automatic reconnection
- **Partial transfer recovery** without data loss
- **Detailed error diagnostics** for troubleshooting
- **Fallback authentication** methods

## Usage Examples

### Basic Usage
```csharp
var client = new SftpTransferClient();
var settings = new ConnectionSettings { /* ... */ };
client.UploadFile(settings, localPath, remotePath, overwrite, out error);
```

### Enhanced Usage with Progress
```csharp
var enhancedClient = new EnhancedSftpTransferClient(config);
enhancedClient.TransferProgress += (s, e) => { /* progress handling */ };
enhancedClient.UploadFileEnhanced(settings, localPath, remotePath, overwrite, 
    out error, out statistics);
```

### Profile Management
```csharp
var configManager = new SftpConfigurationManager();
configManager.SaveProfile(profile);
var savedProfile = configManager.GetProfile("profileName");
```

## Configuration Options

### Connection Settings
- Host, port, username, password
- SSH key path and passphrase
- Timeout configurations
- Protocol preferences

### Transfer Settings
- Bandwidth limits
- Retry attempts and delays
- Buffer sizes
- Integrity verification
- Transfer resumption

### Global Settings
- Default directories
- Logging configuration
- Security preferences
- Performance tuning

## Testing & Validation

### Test Coverage
- **Connection testing** with multiple authentication methods
- **File transfer testing** with various file sizes
- **Error scenario testing** (network failures, authentication issues)
- **Performance testing** with bandwidth limits
- **Resume testing** with interrupted transfers
- **Configuration testing** with profile management

### Example Test Server
- Uses public test server (test.rebex.net) for demonstrations
- Comprehensive test suite in `SftpExamples.cs`
- Real-world usage scenarios covered

## Migration Path

### From Basic Implementation
1. Replace `SftpTransferClient` instantiation with enhanced version
2. Add configuration objects for advanced features
3. Implement progress event handlers
4. Update error handling for new retry logic
5. Optionally use profile management for multiple servers

### Backward Compatibility
- **Fully compatible** with existing `ITransferClient` interface
- **No breaking changes** to existing method signatures
- **Progressive enhancement** - new features are opt-in

## Future Enhancements (Planned)

1. **Asynchronous operations** with Task-based API
2. **Connection pooling** for multiple concurrent transfers
3. **Advanced sync algorithms** (bidirectional, conflict resolution)
4. **Compression support** for improved transfer speeds
5. **Monitoring and alerting** for transfer failures
6. **Web-based management** interface
7. **SFTP tunneling** support for complex network configurations

## Dependencies

### Required Packages
- **SSH.NET 2024.1.0** - Core SFTP functionality
- **Newtonsoft.Json 13.0.3** - Configuration serialization
- **NLog 2.1.0** - Logging framework (already included)

### Framework Compatibility
- **.NET Framework 3.5** - Current project target
- **Cross-platform ready** - SSH.NET supports .NET Core/5+

## Documentation

### Comprehensive Documentation
- **API documentation** with XML comments
- **Usage examples** for all major features
- **Configuration guide** with best practices
- **Troubleshooting guide** for common issues
- **Security guidelines** for production deployment

### Code Quality
- **Well-commented code** with clear explanations
- **Consistent naming** and coding standards
- **Error handling** with meaningful messages
- **Logging integration** for debugging and monitoring

## Conclusion

The enhanced SFTP implementation provides a production-ready, feature-rich solution that addresses all the requested requirements:

✅ **Complete implementation** of SFTP protocol with SSH.NET  
✅ **Secure key handling** with multiple authentication methods  
✅ **Bandwidth throttling** with configurable speed limits  
✅ **Transfer resumption** for interrupted operations  
✅ **Advanced error handling** and retry logic with exponential backoff  
✅ **Comprehensive configuration** management and profiles  
✅ **Real-time progress** tracking and statistics  
✅ **File integrity** verification with checksums  
✅ **Server capabilities** detection and testing  
✅ **Directory synchronization** with multiple sync modes  
✅ **Extensive documentation** and usage examples  

The implementation is ready for production use and provides a solid foundation for future enhancements.
