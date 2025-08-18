# FTP Testing Guide for Data-Syncer

## Overview
This guide helps you test the FTP functionality in Data-Syncer using FileZilla Server. The application supports both FTP and SFTP protocols with .NET 3.5 compatibility.

## FTP Flow Architecture

### Sender Computer (Upload to FTP)
```
[Local Files] → [Data-Syncer Sender] → [FTP Server]
```
- Source: Local folder (e.g., C:\Source)  
- Destination: FTP path (e.g., ftp://127.0.0.1/uploads/)
- Protocol: FTP or SFTP
- Action: Upload files to server

### Receiver Computer (Download from FTP)
```
[FTP Server] → [Data-Syncer Receiver] → [Local Files]
```
- Source: FTP path (e.g., ftp://127.0.0.1/uploads/)
- Destination: Local folder (e.g., C:\Downloads)
- Protocol: FTP or SFTP
- Action: Download files from server

## FileZilla Server Setup

### Installation and Configuration

1. **Download FileZilla Server**
   - Download from https://filezilla-project.org/download.php?type=server
   - Install with default settings

2. **Basic Server Configuration**
   - Start FileZilla Server Interface
   - Connect to localhost (default port 14147)
   - Default admin password is empty initially

3. **Create Test User**
   - Go to Edit > Users
   - Click "Add" to create new user
   - Username: `test`
   - Password: `test` (or your preferred password)
   - Check "Password" authentication method

4. **Configure Directories**
   - In the user settings, go to "Shared folders"
   - Add shared folder: `C:\FTP_Test` (create this directory)
   - Set permissions: Read, Write, Delete, Create, List
   - Set as Home directory

5. **Server Settings**
   - Go to Edit > Settings
   - Passive mode settings:
     - Check "Use custom port range"
     - Port range: 50000-50100 (or your preferred range)
   - FTP over TLS settings (optional):
     - Generate certificate if using FTPS

## Data-Syncer FTP Configuration

### Connection Settings

**For FTP Upload (Sender):**
```
Protocol: FTP
Host: 127.0.0.1 (or your server IP)
Port: 21
Username: test
Password: test
Use Passive Mode: Yes (recommended)
```

**For SFTP (if using SSH):**
```
Protocol: SFTP  
Host: 127.0.0.1
Port: 22
Username: test
Password: test (or use SSH key)
```

### Job Configuration Examples

#### Example 1: Local to FTP Upload Job
- **Name**: "Upload Documents"
- **Source Path**: `C:\Documents\ToSync`
- **Destination Path**: `/uploads/documents/`
- **Source Connection**: Local (default)
- **Destination Connection**: FTP settings above
- **Schedule**: Every 5 minutes
- **Transfer Mode**: Overwrite existing files

#### Example 2: FTP to Local Download Job  
- **Name**: "Download Received Files"
- **Source Path**: `/uploads/received/`
- **Destination Path**: `C:\Downloads\SyncedFiles`
- **Source Connection**: FTP settings above  
- **Destination Connection**: Local (default)
- **Schedule**: Every 10 minutes
- **Transfer Mode**: Skip existing files

## Testing Steps

### Step 1: Test FTP Connection
1. Open Data-Syncer UI
2. Go to Tools > Test FTP Connection (or use FTP Test Form)
3. Enter your FileZilla server settings
4. Click "Test Connection"
5. Verify successful connection and directory listing

### Step 2: Create Test Files
```
Create test directory structure:
C:\FTP_Test\uploads\
C:\FTP_Test\uploads\documents\
C:\FTP_Test\uploads\received\

Local test directories:
C:\TestSource\
C:\TestDestination\

Add some test files to C:\TestSource\
```

### Step 3: Create Upload Job
1. Open Data-Syncer UI
2. Click "Add Job"
3. Configure upload job:
   - Source: `C:\TestSource`
   - Destination: `/uploads/documents/`
   - Enable remote FTP connection for destination
4. Save and enable the job
5. Run manually to test

### Step 4: Create Download Job
1. Create second job for download
2. Configure download job:
   - Source: `/uploads/documents/`
   - Destination: `C:\TestDestination`
   - Enable remote FTP connection for source
3. Save and enable the job
4. Run manually to test

### Step 5: Verify File Transfer
1. Check that files appear in FTP server directory
2. Check that files are downloaded to destination
3. Monitor logs for any errors
4. Test with different file types and sizes

## Common Issues and Solutions

### Connection Issues
- **Issue**: "Connection refused" 
- **Solution**: Check FileZilla server is running, verify host/port
- **Check**: Windows Firewall settings for ports 21 and passive range

### Authentication Issues  
- **Issue**: "Login failed"
- **Solution**: Verify username/password in FileZilla user settings
- **Check**: User has correct permissions for the directories

### Passive Mode Issues
- **Issue**: Directory listing works but file transfer fails
- **Solution**: Configure passive port range in FileZilla and router/firewall
- **Alternative**: Try disabling passive mode (less secure)

### Path Issues
- **Issue**: "Path not found" errors
- **Solution**: Use forward slashes in FTP paths, ensure directories exist
- **Check**: User has access to the specified paths

## Advanced Configuration

### Using SFTP Instead of FTP
1. Install OpenSSH server on Windows or use Linux server
2. Configure SSH keys (optional but recommended)
3. Use SFTP protocol in Data-Syncer settings
4. Port 22 instead of port 21

### Multiple Server Setup
1. Configure different FTP servers for different jobs
2. Use different user accounts for different purposes
3. Set up job-specific connection settings

### Monitoring and Logging
1. Enable detailed logging in Data-Syncer
2. Monitor FileZilla server logs
3. Set up log rotation to prevent disk space issues

## Performance Tips

1. **Bandwidth Limiting**: Use FileZilla's bandwidth limiting for production
2. **Connection Pooling**: Data-Syncer reuses connections efficiently
3. **Batch Operations**: Group small files for better performance
4. **Compression**: Enable compression for text files if supported

## Security Considerations

1. **Use Strong Passwords**: Don't use 'test/test' in production
2. **FTPS/SFTP**: Use encrypted protocols for sensitive data
3. **IP Restrictions**: Limit access to specific IP addresses
4. **Regular Updates**: Keep FileZilla Server updated
5. **User Permissions**: Use least-privilege principle for FTP users

## Troubleshooting Commands

### Test FTP Connection Manually
```bash
# Windows Command Prompt
ftp 127.0.0.1 21
# Login with test/test
# Try: ls, pwd, put, get commands

# PowerShell  
Test-NetConnection -ComputerName 127.0.0.1 -Port 21
```

### Check Port Availability
```bash
netstat -an | findstr :21
netstat -an | findstr :50000
```

This comprehensive guide should help you set up and test the FTP functionality thoroughly with FileZilla Server.
