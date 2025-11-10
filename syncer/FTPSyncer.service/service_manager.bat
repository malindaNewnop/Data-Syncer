@echo off
REM FTPSyncer Service Installation and Management Script
REM This script helps install, uninstall, start, and stop the FTPSyncer Windows service

echo ============================================
echo FTPSyncer Service Management
echo ============================================

if "%1"=="install" goto install
if "%1"=="uninstall" goto uninstall
if "%1"=="start" goto start
if "%1"=="stop" goto stop
if "%1"=="restart" goto restart
if "%1"=="status" goto status
if "%1"=="test" goto test
goto help

:install
echo Installing FTPSyncer Service...
echo.
echo Step 1: Building the service...
cd /d "%~dp0"
if exist "bin\Release" (
    echo Using Release build...
    "%WINDIR%\Microsoft.NET\Framework\v2.0.50727\installutil.exe" "bin\Release\FTPSyncer.service.exe"
) else if exist "bin\Debug" (
    echo Using Debug build...
    "%WINDIR%\Microsoft.NET\Framework\v2.0.50727\installutil.exe" "bin\Debug\FTPSyncer.service.exe"
) else (
    echo ERROR: No compiled service found. Please build the project first.
    goto end
)

echo.
echo Step 2: Starting the service...
net start FTPSyncerService
if %errorlevel%==0 (
    echo.
    echo SUCCESS: FTPSyncer Service installed and started successfully!
    echo The service will automatically start when Windows starts.
    echo.
    echo You can now use the FTP Syncer application. Any running jobs will
    echo be automatically resumed if the system is restarted.
) else (
    echo.
    echo WARNING: Service installed but failed to start. Check Windows Event Log for details.
)
goto end

:uninstall
echo Uninstalling FTPSyncer Service...
echo.
echo Step 1: Stopping the service...
net stop FTPSyncerService 2>nul

echo.
echo Step 2: Uninstalling the service...
if exist "bin\Release" (
    "%WINDIR%\Microsoft.NET\Framework\v2.0.50727\installutil.exe" /u "bin\Release\FTPSyncer.service.exe"
) else if exist "bin\Debug" (
    "%WINDIR%\Microsoft.NET\Framework\v2.0.50727\installutil.exe" /u "bin\Debug\FTPSyncer.service.exe"
) else (
    echo ERROR: No service executable found.
    goto end
)

echo.
echo SUCCESS: FTPSyncer Service uninstalled successfully!
goto end

:start
echo Starting FTPSyncer Service...
net start FTPSyncerService
if %errorlevel%==0 (
    echo SUCCESS: FTPSyncer Service started successfully!
) else (
    echo ERROR: Failed to start FTPSyncer Service. Check if it's installed and check Windows Event Log.
)
goto end

:stop
echo Stopping FTPSyncer Service...
net stop FTPSyncerService
if %errorlevel%==0 (
    echo SUCCESS: FTPSyncer Service stopped successfully!
) else (
    echo ERROR: Failed to stop FTPSyncer Service or service was not running.
)
goto end

:restart
echo Restarting FTPSyncer Service...
net stop FTPSyncerService 2>nul
timeout /t 3 /nobreak >nul
net start FTPSyncerService
if %errorlevel%==0 (
    echo SUCCESS: FTPSyncer Service restarted successfully!
) else (
    echo ERROR: Failed to restart FTPSyncer Service.
)
goto end

:status
echo Checking FTPSyncer Service status...
sc query FTPSyncerService
echo.
echo Recent service events from Event Log:
wevtutil qe Application /c:5 /f:text /q:"*[System[Provider[@Name='FTPSyncer Service']]]" 2>nul
if %errorlevel%!=0 (
    echo No recent events found or insufficient permissions to access Event Log.
)
goto end

:test
echo Testing FTPSyncer Service functionality...
echo.
echo 1. Checking if service is installed...
sc query FTPSyncerService >nul 2>&1
if %errorlevel%==0 (
    echo   ✓ Service is installed
) else (
    echo   ✗ Service is NOT installed
    echo   Run: service_manager.bat install
    goto end
)

echo.
echo 2. Checking if service is running...
sc query FTPSyncerService | find "RUNNING" >nul
if %errorlevel%==0 (
    echo   ✓ Service is running
) else (
    echo   ✗ Service is NOT running
    echo   Run: service_manager.bat start
    goto end
)

echo.
echo 3. Testing job recovery persistence...
if exist "%PROGRAMDATA%\FTPSyncer\ServiceState.xml" (
    echo   ✓ Service state file exists
) else (
    echo   ! No service state file found (normal if no jobs have run)
)

echo.
echo 4. Checking registry configuration...
reg query "HKLM\SOFTWARE\FTPSyncer\Service" >nul 2>&1
if %errorlevel%==0 (
    echo   ✓ Registry configuration exists
) else (
    echo   ! No registry configuration found (will be created when jobs run)
)

echo.
echo ✓ Service appears to be functioning correctly!
echo.
echo To test job recovery:
echo 1. Start a sync job in the UI
echo 2. Restart the service: service_manager.bat restart
echo 3. Check if the job continues running
goto end

:help
echo.
echo Usage: service_manager.bat [command]
echo.
echo Commands:
echo   install    - Install and start the FTPSyncer Service
echo   uninstall  - Stop and uninstall the FTPSyncer Service  
echo   start      - Start the FTPSyncer Service
echo   stop       - Stop the FTPSyncer Service
echo   restart    - Stop and start the FTPSyncer Service
echo   status     - Show service status and recent events
echo   test       - Test service installation and functionality
echo.
echo Examples:
echo   service_manager.bat install
echo   service_manager.bat test
echo   service_manager.bat restart
echo.
echo Note: This script must be run as Administrator for install/uninstall operations.
echo.

:end
echo.
pause
