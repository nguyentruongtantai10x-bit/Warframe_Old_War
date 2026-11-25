@echo off
chcp 65001 >nul
title WARFRAME Game API Launcher

echo ============================================
echo  WARFRAME - ASM1 Game API Launcher
echo ============================================
echo.

powershell -NoProfile -Command "$c = Get-NetTCPConnection -LocalPort 5080 -State Listen -ErrorAction SilentlyContinue; if ($c) { exit 1 } else { exit 0 }"
if %errorlevel%==1 (
    echo [OK] API dang chay san tai http://localhost:5080
    echo Mo Swagger...
    start "" http://localhost:5080/swagger
    ping -n 4 127.0.0.1 >nul
    exit /b 0
)

set "DOTNET=C:\Users\Admin\.dotnet9\dotnet.exe"
if not exist "%DOTNET%" (
    echo [LOI] Khong tim thay .NET SDK 9:
    echo       %DOTNET%
    echo Cai SDK 9.0.203 hoac sua duong dan trong file nay.
    pause
    exit /b 1
)

echo [1/2] Khoi dong API trong cua so rieng...
start "WARFRAME Game API" /D "%~dp0GameApi" "%DOTNET%" run --project GameApi.csproj --no-launch-profile --urls http://localhost:5080

echo [2/2] Doi server san sang...
ping -n 5 127.0.0.1 >nul
start "" http://localhost:5080/swagger

echo.
echo [XONG] API: http://localhost:5080
echo        Swagger da mo trong trinh duyet.
echo        Dong cua so "WARFRAME Game API" de tat server.
echo.
ping -n 5 127.0.0.1 >nul
exit /b 0
