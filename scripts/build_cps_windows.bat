@echo off
REM ClosedGD77 - Build CPS for Windows
REM Requires: Visual Studio or MSBuild in PATH
REM
REM Usage: build_cps_windows.bat [Debug|Release]

set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Release

echo === ClosedGD77 CPS Windows Build ===
echo Configuration: %CONFIG%
echo.

REM Find MSBuild
where msbuild >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    REM Try VS paths
    set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    if not exist "!MSBUILD!" set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
    if not exist "!MSBUILD!" set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    if not exist "!MSBUILD!" (
        echo ERROR: MSBuild not found. Install Visual Studio or Build Tools.
        exit /b 1
    )
) else (
    set "MSBUILD=msbuild"
)

echo MSBuild: %MSBUILD%
echo.

REM Restore NuGet packages
echo --- Restoring NuGet packages ---
cd /d "%~dp0..\cps"
nuget restore OpenGD77CPS.sln 2>nul || echo   (nuget not found, skipping - packages may already be restored)
echo.

REM Build
echo --- Building CPS (%CONFIG%) ---
%MSBUILD% OpenGD77CPS.sln /p:Configuration=%CONFIG% /p:Platform=x86 /v:minimal

if %ERRORLEVEL% EQU 0 (
    echo.
    echo === Build complete ===
    echo Output: cps\bin\%CONFIG%\OpenGD77CPS.exe
    echo.
    echo Run: cps\bin\%CONFIG%\OpenGD77CPS.exe
) else (
    echo.
    echo === Build FAILED ===
    exit /b 1
)
