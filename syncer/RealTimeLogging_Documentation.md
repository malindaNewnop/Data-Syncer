# Enhanced Real-Time CSV Logging Feature

## Overview
The Data Syncer application now includes a comprehensive real-time logging feature that allows users to log all system activities to custom CSV files in real-time. This feature provides detailed information similar to the main log viewer table.

## Features

### 1. Real-Time Logging Controls
- **Enable/Disable Toggle**: Checkbox to enable or disable real-time logging
- **File Path Selection**: Text field with browse button to select custom CSV file location
- **Status Indicator**: Shows current logging status (enabled/disabled/error)

### 2. Enhanced CSV Output
The CSV file now includes comprehensive information with the following columns:

- **Timestamp**: Precise date and time with milliseconds (yyyy-MM-dd HH:mm:ss.fff)
- **Level**: Log level (INFO, WARNING, ERROR)
- **Source**: Source module or component
- **JobName**: Name of the sync job (if applicable)
- **JobId**: Unique job identifier (if applicable)
- **Message**: Detailed log message
- **FileName**: File being processed (for transfer operations)
- **FileSize**: Size of file in bytes (for transfer operations)
- **Duration**: Time taken for operation in seconds (for transfer operations)
- **SourcePath**: Source file/directory path (for transfer operations)
- **DestinationPath**: Destination file/directory path (for transfer operations)
- **Status**: Operation status (Success/Failed/In Progress)
- **Exception**: Exception details (if error occurred)

### 3. Supported Log Types

#### System Logs
- General information messages
- Warning messages
- Error messages with exception details

#### Job Logs
- Job start events with job details
- Job progress updates
- Job completion (success/failure)
- Job error events

#### Transfer Logs
- File upload/download operations
- File copy operations
- Transfer progress and statistics
- Transfer success/failure with details

## Usage Instructions

### Enabling Real-Time Logging
1. Open the Log Viewer form
2. In the "Real-time CSV Logging" section:
   - Check the "Enable" checkbox
   - Select or enter a CSV file path
   - Click "Browse..." to choose a location
3. Click OK when prompted
4. The status will show "Real-time logging enabled" in green

### Disabling Real-Time Logging
1. Uncheck the "Enable" checkbox
2. The status will show "Real-time logging disabled" in gray

### CSV File Location
- Default location: `Documents\DataSyncerLogs\syncer_realtime_YYYYMMDD.csv`
- Custom location: User can select any accessible directory
- File is created automatically if it doesn't exist
- Data is appended if file already exists

## Technical Implementation

### Architecture
- **Interface**: `ILogService` extended with real-time logging methods
- **Implementation**: `CoreLogServiceAdapter` provides the main functionality
- **UI**: Enhanced `FormLogs` with new controls
- **File Handling**: Thread-safe CSV writing with proper escaping

### Thread Safety
- All real-time logging operations are thread-safe
- CSV file writes are synchronized using locks
- UI updates are properly marshaled to the main thread

### Error Handling
- Graceful handling of file access errors
- Automatic cleanup on application exit
- Non-blocking error reporting

### Performance
- Minimal performance impact on main application
- Immediate CSV file writing for real-time monitoring
- Efficient string formatting and CSV escaping

## Sample CSV Output

```csv
Timestamp,Level,Source,JobName,JobId,Message,FileName,FileSize,Duration,SourcePath,DestinationPath,Status,Exception
2025-09-01 14:30:15.123,INFO,System,,,System startup completed,,,,,,,
2025-09-01 14:30:16.234,INFO,JobManager,Test Sync Job,TEST001,Job started,,,,,,,
2025-09-01 14:30:17.345,INFO,JobManager,Test Sync Job,TEST001,Processing files...,,,,,,,
2025-09-01 14:30:18.456,INFO,Transfer,Test Sync Job,TEST001,File transferred successfully,document.pdf,1048576,2.5,C:\Source\document.pdf,C:\Destination\document.pdf,Success,
2025-09-01 14:30:19.567,INFO,JobManager,Test Sync Job,TEST001,All files transferred successfully,,,,,,,
```

## Benefits

1. **Real-Time Monitoring**: Immediate visibility into system operations
2. **Detailed Information**: Comprehensive data for analysis and troubleshooting
3. **Custom Storage**: User-controlled file location and naming
4. **External Analysis**: CSV format allows easy import into Excel, databases, etc.
5. **Historical Tracking**: Persistent logging across application sessions
6. **No Performance Impact**: Minimal overhead on main application performance

## Compatibility
- Compatible with .NET Framework 3.5
- Works with all existing log services
- Backward compatible with existing logging functionality
- Thread-safe for multi-threaded operations

## Future Enhancements
- Log rotation based on file size/age
- Multiple simultaneous CSV outputs
- Real-time log filtering options
- Integration with external monitoring systems
