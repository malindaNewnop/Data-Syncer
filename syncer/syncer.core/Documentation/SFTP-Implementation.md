# Enhanced SFTP Implementation Documentation

This document provides comprehensive documentation for the enhanced SFTP implementation in the Data Syncer project.

## Overview

The enhanced SFTP implementation includes:

1. **Secure Key Handling**: Support for RSA, DSA, and ECDSA keys with passphrase protection
2. **Bandwidth Throttling**: Configurable transfer speed limits
3. **Transfer Resumption**: Automatic resumption of interrupted transfers
4. **Advanced Error Handling**: Retry logic with exponential backoff
5. **Progress Tracking**: Real-time transfer progress with detailed statistics
6. **Integrity Verification**: SHA-256 checksum validation
7. **Configuration Management**: Profile-based configuration with import/export
8. **Utilities**: Key generation, server information, and connection testing

## Classes and Components

### Core Classes

1. **SftpTransferClient**: Base SFTP client with full SSH.NET implementation
2. **EnhancedSftpTransferClient**: Extended client with advanced features
3. **ThrottledStream**: Stream wrapper for bandwidth throttling
4. **SftpConfiguration**: Configuration settings for advanced features
5. **SftpConfigurationManager**: Profile management and configuration storage
6. **SftpUtilities**: Utility methods for key management and server testing

### Configuration Classes

1. **SftpProfile**: Named connection profile with usage statistics
2. **SftpGlobalConfiguration**: Global settings and defaults
3. **TransferStatistics**: Transfer metrics and performance data
4. **SftpServerInfo**: Server capabilities and information

## Basic Usage Examples

### Simple File Transfer

```csharp
// Basic SFTP client usage
var client = new SftpTransferClient();
var settings = new ConnectionSettings
{
    Protocol = ProtocolType.Sftp,
    Host = "sftp.example.com",
    Port = 22,
    Username = "user",
    Password = "password"  // Or use SshKeyPath for key-based auth
};

// Test connection
if (client.TestConnection(settings, out string error))
{
    Console.WriteLine("Connection successful");
    
    // Upload file
    if (client.UploadFile(settings, @"C:\local\file.txt", "/remote/file.txt", true, out error))
    {
        Console.WriteLine("Upload successful");
    }
    else
    {
        Console.WriteLine($"Upload failed: {error}");
    }
}
else
{
    Console.WriteLine($"Connection failed: {error}");
}
```

### Enhanced Features Usage

```csharp
// Enhanced SFTP client with advanced features
var config = new SftpConfiguration
{
    BandwidthLimitBytesPerSecond = 1024 * 1024, // 1 MB/s limit
    EnableTransferResumption = true,
    VerifyTransferIntegrity = true,
    MaxRetryAttempts = 5
};

var enhancedClient = new EnhancedSftpTransferClient(config);

// Event handlers for progress tracking
enhancedClient.TransferProgress += (sender, e) =>
{
    Console.WriteLine($"Progress: {e.ProgressPercentage:F1}% " +
                     $"Speed: {e.SpeedBytesPerSecond / 1024:F1} KB/s " +
                     $"ETA: {e.EstimatedTimeRemaining}");
};

enhancedClient.TransferCompleted += (sender, e) =>
{
    if (e.Success)
    {
        Console.WriteLine($"Transfer completed successfully");
        Console.WriteLine($"Average speed: {e.Statistics.AverageSpeedBytesPerSecond / 1024:F1} KB/s");
        Console.WriteLine($"Duration: {e.Statistics.Duration}");
    }
    else
    {
        Console.WriteLine($"Transfer failed: {e.ErrorMessage}");
    }
};

// Upload with enhanced features
if (enhancedClient.UploadFileEnhanced(settings, @"C:\large-file.zip", 
    "/remote/large-file.zip", true, out error, out var stats))
{
    Console.WriteLine("Enhanced upload completed");
}
```

### Key-Based Authentication

```csharp
// Generate SSH key pair
string publicKey = SftpUtilities.GenerateKeyPair(@"C:\keys\mykey", "mypassphrase", 2048);
Console.WriteLine($"Generated public key: {publicKey}");

// Use key for authentication
var settings = new ConnectionSettings
{
    Protocol = ProtocolType.Sftp,
    Host = "sftp.example.com",
    Port = 22,
    Username = "user",
    SshKeyPath = @"C:\keys\mykey",
    Password = "mypassphrase"  // Key passphrase
};

// Test different authentication methods
if (SftpUtilities.TestConnectionMethods(settings, out var testResults))
{
    foreach (var result in testResults)
    {
        Console.WriteLine($"{result.Key}: {result.Value}");
    }
}
```

### Profile Management

```csharp
// Create configuration manager
var configManager = new SftpConfigurationManager();

// Create and save a profile
var profile = new SftpProfile
{
    Name = "Production Server",
    Description = "Main production SFTP server",
    ConnectionSettings = new ConnectionSettings
    {
        Host = "prod-sftp.company.com",
        Username = "deploy",
        SshKeyPath = @"C:\keys\deploy-key"
    },
    TransferConfiguration = new SftpConfiguration
    {
        BandwidthLimitBytesPerSecond = 5 * 1024 * 1024, // 5 MB/s
        EnableTransferResumption = true,
        VerifyTransferIntegrity = true
    }
};

configManager.SaveProfile(profile);

// Use profile for transfers
var savedProfile = configManager.GetProfile("Production Server");
var client = new EnhancedSftpTransferClient(savedProfile.TransferConfiguration);

// Update usage statistics
configManager.UpdateProfileUsage("Production Server", 1024 * 1024 * 100, 2.5 * 1024 * 1024);
```

### Directory Synchronization

```csharp
var enhancedClient = new EnhancedSftpTransferClient();
var syncOptions = new SyncOptions
{
    SyncMode = SyncMode.TimestampAndSize,
    IncludeSubdirectories = true,
    PreserveTimestamps = true,
    ExcludePatterns = new List<string> { "*.tmp", "*.log" }
};

if (enhancedClient.SynchronizeDirectory(settings, @"C:\local\folder", 
    "/remote/folder", syncOptions, out error, out var transferStats))
{
    Console.WriteLine($"Synchronized {transferStats.Count} files");
    foreach (var stat in transferStats)
    {
        Console.WriteLine($"  {stat.TotalBytes} bytes in {stat.Duration}");
    }
}
```

### Server Information and Capabilities

```csharp
// Get server information
if (SftpUtilities.GetServerInfo(settings, out var serverInfo))
{
    Console.WriteLine($"Server: {serverInfo.ServerVersion}");
    Console.WriteLine($"Protocol: {serverInfo.ProtocolVersion}");
    Console.WriteLine($"Working Directory: {serverInfo.WorkingDirectory}");
    Console.WriteLine($"Supports POSIX Rename: {serverInfo.SupportsPosixRename}");
    Console.WriteLine($"Supports Hard Links: {serverInfo.SupportsHardLink}");
}

// Get detailed directory listing
var client = new EnhancedSftpTransferClient();
if (client.GetDirectoryListingDetailed(settings, "/remote/path", out var files, out error))
{
    foreach (var file in files)
    {
        Console.WriteLine($"{file.Name} - {file.Size} bytes - {file.LastModified} - {file.Permissions}");
    }
}
```

## Configuration Options

### SftpConfiguration Properties

- **ConnectionTimeoutMs**: Connection timeout (default: 30000ms)
- **OperationTimeoutMs**: Operation timeout (default: 60000ms)
- **MaxRetryAttempts**: Maximum retry attempts (default: 3)
- **RetryDelayMs**: Initial retry delay (default: 1000ms)
- **UseExponentialBackoff**: Enable exponential backoff (default: true)
- **BandwidthLimitBytesPerSecond**: Transfer speed limit (0 = unlimited)
- **EnableTransferResumption**: Allow resuming interrupted transfers (default: true)
- **VerifyTransferIntegrity**: Enable SHA-256 verification (default: false)
- **PreserveTimestamps**: Preserve file timestamps (default: true)
- **EnableCompression**: Enable SSH compression (default: false)

### Authentication Methods

1. **Password Authentication**: Username and password
2. **Public Key Authentication**: SSH private key with optional passphrase
3. **Keyboard Interactive**: Interactive authentication prompts

```csharp
// Multiple authentication methods
var settings = new ConnectionSettings
{
    Host = "sftp.example.com",
    Username = "user",
    Password = "password",          // Password auth
    SshKeyPath = @"C:\keys\mykey"   // Key auth (fallback)
};
```

## Error Handling and Retry Logic

The implementation includes comprehensive error handling:

### Retryable Errors
- Network timeouts
- Connection failures
- SSH connection exceptions
- Temporary server errors

### Non-Retryable Errors
- Authentication failures
- File not found errors
- Permission denied errors
- Invalid configuration

### Retry Strategy
1. Exponential backoff: 1s, 2s, 4s, 8s...
2. Maximum retry attempts configurable
3. Different strategies for different error types

## Performance Features

### Bandwidth Throttling
- Configurable speed limits
- Per-transfer or global limits
- Smooth throttling without blocking

### Transfer Resumption
- Automatic detection of partial transfers
- Resume from last byte transferred
- Configurable minimum file size for resumption

### Progress Tracking
- Real-time progress updates
- Speed calculations
- ETA estimates
- Detailed transfer statistics

## Security Features

### Key Management
- Secure key generation
- Passphrase protection
- Secure key deletion (multiple overwrites)
- Key validation

### Connection Security
- Multiple authentication methods
- Configurable cipher preferences
- Host key verification
- Connection encryption

### Data Integrity
- SHA-256 checksum verification
- File size validation
- Timestamp preservation
- Permission preservation

## Best Practices

1. **Use Key-Based Authentication**: More secure than passwords
2. **Enable Transfer Resumption**: For large files and unreliable connections
3. **Set Appropriate Timeouts**: Based on network conditions
4. **Use Bandwidth Throttling**: To avoid overwhelming the network
5. **Enable Integrity Verification**: For critical data transfers
6. **Configure Retry Logic**: For handling temporary failures
7. **Use Profiles**: For managing multiple server configurations
8. **Monitor Progress**: For long-running transfers
9. **Secure Key Storage**: Protect private keys with passphrases
10. **Regular Backups**: Of configuration profiles and keys

## Troubleshooting

### Common Issues

1. **Connection Timeouts**
   - Increase connection timeout
   - Check firewall settings
   - Verify server availability

2. **Authentication Failures**
   - Verify credentials
   - Check key file permissions
   - Test different auth methods

3. **Transfer Failures**
   - Enable retry logic
   - Check disk space
   - Verify file permissions

4. **Performance Issues**
   - Adjust buffer sizes
   - Check bandwidth limits
   - Monitor network conditions

### Logging and Debugging

Enable debug logging for detailed troubleshooting:

```csharp
var config = new SftpConfiguration
{
    EnableDebugLogging = true
};

var globalConfig = new SftpGlobalConfiguration
{
    EnableLogging = true,
    LogLevel = "Debug"
};
```

## Migration from Basic Implementation

To upgrade from the basic SFTP implementation:

1. Replace `SftpTransferClient` with `EnhancedSftpTransferClient`
2. Create `SftpConfiguration` with desired settings
3. Add progress event handlers if needed
4. Update error handling for new retry logic
5. Consider using profile management for multiple servers

The enhanced implementation is backward compatible with the basic interface.
