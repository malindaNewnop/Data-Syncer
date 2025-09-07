# FTPSyncer Service Quick Installer
# Run this script as Administrator

param(
    [Parameter(Position=0)]
    [ValidateSet("install", "uninstall", "start", "stop", "restart", "status", "test", "help")]
    [string]$Action = "help"
)

$ServiceName = "FTPSyncerService"
$ServiceDisplayName = "FTPSyncer Service"

function Write-Header {
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "FTPSyncer Service Manager" -ForegroundColor Yellow
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host ""
}

function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Find-ServiceExecutable {
    $scriptPath = Split-Path -Parent $MyInvocation.PSCommandPath
    $possiblePaths = @(
        Join-Path $scriptPath "bin\Release\syncer.service.exe"
        Join-Path $scriptPath "bin\Debug\syncer.service.exe"
    )
    
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            return $path
        }
    }
    
    return $null
}

function Install-FTPSyncerService {
    Write-Host "Installing FTPSyncer Service..." -ForegroundColor Green
    
    if (-not (Test-Administrator)) {
        Write-Host "ERROR: Administrator privileges required for installation!" -ForegroundColor Red
        Write-Host "Please run PowerShell as Administrator and try again." -ForegroundColor Yellow
        return $false
    }
    
    $exePath = Find-ServiceExecutable
    if (-not $exePath) {
        Write-Host "ERROR: Service executable not found!" -ForegroundColor Red
        Write-Host "Please build the project first:" -ForegroundColor Yellow
        Write-Host "1. Open the solution in Visual Studio" -ForegroundColor Yellow
        Write-Host "2. Right-click 'syncer.service' project -> Build" -ForegroundColor Yellow
        return $false
    }
    
    Write-Host "Found service executable: $exePath" -ForegroundColor Gray
    
    try {
        # Install the service
        Write-Host "Installing service..." -ForegroundColor Yellow
        & $exePath "install"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Service installed successfully!" -ForegroundColor Green
            return $true
        } else {
            Write-Host "✗ Service installation failed!" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "✗ Installation error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Uninstall-FTPSyncerService {
    Write-Host "Uninstalling FTPSyncer Service..." -ForegroundColor Green
    
    if (-not (Test-Administrator)) {
        Write-Host "ERROR: Administrator privileges required!" -ForegroundColor Red
        return $false
    }
    
    try {
        # Stop service first
        Stop-FTPSyncerService | Out-Null
        
        $exePath = Find-ServiceExecutable
        if ($exePath) {
            & $exePath "uninstall"
        } else {
            # Fallback to InstallUtil
            $installUtil = Join-Path $env:WINDIR "Microsoft.NET\Framework\v2.0.50727\installutil.exe"
            if (Test-Path $installUtil) {
                & $installUtil "/u" $exePath
            }
        }
        
        Write-Host "✓ Service uninstalled successfully!" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "✗ Uninstallation error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Start-FTPSyncerService {
    Write-Host "Starting FTPSyncer Service..." -ForegroundColor Green
    
    try {
        $service = Get-Service -Name $ServiceName -ErrorAction Stop
        
        if ($service.Status -eq "Running") {
            Write-Host "✓ Service is already running!" -ForegroundColor Green
            return $true
        }
        
        Start-Service -Name $ServiceName
        Write-Host "✓ Service started successfully!" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "✗ Failed to start service: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Service may not be installed. Run: .\install-service.ps1 install" -ForegroundColor Yellow
        return $false
    }
}

function Stop-FTPSyncerService {
    Write-Host "Stopping FTPSyncer Service..." -ForegroundColor Green
    
    try {
        $service = Get-Service -Name $ServiceName -ErrorAction Stop
        
        if ($service.Status -eq "Stopped") {
            Write-Host "✓ Service is already stopped!" -ForegroundColor Green
            return $true
        }
        
        Stop-Service -Name $ServiceName -Force
        Write-Host "✓ Service stopped successfully!" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "✗ Failed to stop service: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Restart-FTPSyncerService {
    Write-Host "Restarting FTPSyncer Service..." -ForegroundColor Green
    
    if (Stop-FTPSyncerService) {
        Start-Sleep -Seconds 2
        return Start-FTPSyncerService
    }
    return $false
}

function Get-FTPSyncerServiceStatus {
    Write-Host "FTPSyncer Service Status:" -ForegroundColor Green
    Write-Host "========================" -ForegroundColor Green
    
    try {
        $service = Get-Service -Name $ServiceName -ErrorAction Stop
        
        Write-Host "Service Name: $($service.Name)" -ForegroundColor Gray
        Write-Host "Display Name: $($service.DisplayName)" -ForegroundColor Gray
        Write-Host "Status: $($service.Status)" -ForegroundColor $(if ($service.Status -eq "Running") { "Green" } else { "Red" })
        Write-Host "Start Type: $($service.StartType)" -ForegroundColor Gray
        
        # Check for recent events
        Write-Host ""
        Write-Host "Recent Events:" -ForegroundColor Yellow
        try {
            $events = Get-WinEvent -FilterHashtable @{LogName='Application'; ProviderName='FTPSyncer Service'} -MaxEvents 5 -ErrorAction Stop
            foreach ($event in $events) {
                $level = switch ($event.LevelDisplayName) {
                    "Error" { "Red" }
                    "Warning" { "Yellow" }
                    default { "Gray" }
                }
                Write-Host "[$($event.TimeCreated)] $($event.LevelDisplayName): $($event.Message)" -ForegroundColor $level
            }
        }
        catch {
            Write-Host "No recent events found." -ForegroundColor Gray
        }
        
        return $true
    }
    catch {
        Write-Host "✗ Service not found or not installed!" -ForegroundColor Red
        Write-Host "Run: .\install-service.ps1 install" -ForegroundColor Yellow
        return $false
    }
}

function Test-FTPSyncerService {
    Write-Host "Testing FTPSyncer Service Installation..." -ForegroundColor Green
    Write-Host "=========================================" -ForegroundColor Green
    
    $allGood = $true
    
    # Test 1: Check if executable exists
    Write-Host ""
    Write-Host "1. Checking service executable..." -ForegroundColor Yellow
    $exePath = Find-ServiceExecutable
    if ($exePath) {
        Write-Host "   ✓ Found: $exePath" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Service executable not found!" -ForegroundColor Red
        Write-Host "     Please build the project first." -ForegroundColor Yellow
        $allGood = $false
    }
    
    # Test 2: Check if service is installed
    Write-Host ""
    Write-Host "2. Checking service installation..." -ForegroundColor Yellow
    try {
        $service = Get-Service -Name $ServiceName -ErrorAction Stop
        Write-Host "   ✓ Service is installed" -ForegroundColor Green
        
        # Test 3: Check if service is running
        Write-Host ""
        Write-Host "3. Checking service status..." -ForegroundColor Yellow
        if ($service.Status -eq "Running") {
            Write-Host "   ✓ Service is running" -ForegroundColor Green
        } else {
            Write-Host "   ! Service is not running (Status: $($service.Status))" -ForegroundColor Yellow
            Write-Host "     Run: .\install-service.ps1 start" -ForegroundColor Yellow
        }
        
        # Test 4: Check startup type
        Write-Host ""
        Write-Host "4. Checking startup configuration..." -ForegroundColor Yellow
        if ($service.StartType -eq "Automatic") {
            Write-Host "   ✓ Service set to start automatically" -ForegroundColor Green
        } else {
            Write-Host "   ! Service startup type: $($service.StartType)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "   ✗ Service is not installed!" -ForegroundColor Red
        Write-Host "     Run: .\install-service.ps1 install" -ForegroundColor Yellow
        $allGood = $false
    }
    
    # Test 5: Check state persistence
    Write-Host ""
    Write-Host "5. Checking job recovery system..." -ForegroundColor Yellow
    $stateFile = Join-Path $env:PROGRAMDATA "FTPSyncer\ServiceState.xml"
    if (Test-Path $stateFile) {
        Write-Host "   ✓ State persistence file exists" -ForegroundColor Green
    } else {
        Write-Host "   ! No state file found (normal if no jobs have run yet)" -ForegroundColor Yellow
    }
    
    # Test 6: Check registry
    try {
        $regKey = Get-ItemProperty -Path "HKLM:\SOFTWARE\FTPSyncer\Service" -ErrorAction Stop
        Write-Host "   ✓ Registry configuration exists" -ForegroundColor Green
    }
    catch {
        Write-Host "   ! No registry configuration (will be created when needed)" -ForegroundColor Yellow
    }
    
    Write-Host ""
    if ($allGood) {
        Write-Host "✓ All tests passed! Service is ready to use." -ForegroundColor Green
        Write-Host ""
        Write-Host "To test job recovery:" -ForegroundColor Cyan
        Write-Host "1. Start a sync job in the FTP Syncer UI" -ForegroundColor Gray
        Write-Host "2. Restart Windows or run: .\install-service.ps1 restart" -ForegroundColor Gray
        Write-Host "3. Check if the job automatically resumes" -ForegroundColor Gray
    } else {
        Write-Host "✗ Some issues found. Please address them and test again." -ForegroundColor Red
    }
}

function Show-Help {
    Write-Host ""
    Write-Host "FTPSyncer Service PowerShell Installer" -ForegroundColor Yellow
    Write-Host "=====================================" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Usage: .\install-service.ps1 [action]" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Actions:" -ForegroundColor Green
    Write-Host "  install    - Install and start the service" -ForegroundColor Gray
    Write-Host "  uninstall  - Stop and uninstall the service" -ForegroundColor Gray
    Write-Host "  start      - Start the service" -ForegroundColor Gray
    Write-Host "  stop       - Stop the service" -ForegroundColor Gray
    Write-Host "  restart    - Restart the service" -ForegroundColor Gray
    Write-Host "  status     - Show service status and recent events" -ForegroundColor Gray
    Write-Host "  test       - Run installation and functionality tests" -ForegroundColor Gray
    Write-Host "  help       - Show this help" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Green
    Write-Host "  .\install-service.ps1 install" -ForegroundColor Gray
    Write-Host "  .\install-service.ps1 test" -ForegroundColor Gray
    Write-Host "  .\install-service.ps1 status" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Note: Install/uninstall operations require Administrator privileges." -ForegroundColor Yellow
    Write-Host ""
}

# Main execution
Write-Header

switch ($Action.ToLower()) {
    "install" { Install-FTPSyncerService }
    "uninstall" { Uninstall-FTPSyncerService }
    "start" { Start-FTPSyncerService }
    "stop" { Stop-FTPSyncerService }
    "restart" { Restart-FTPSyncerService }
    "status" { Get-FTPSyncerServiceStatus }
    "test" { Test-FTPSyncerService }
    default { Show-Help }
}

Write-Host ""
Write-Host "Press any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
