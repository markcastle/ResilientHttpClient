@echo off
setlocal EnableDelayedExpansion

echo Building ResilientHttpClient in Release mode...
echo.

dotnet build ResilientHttpClient.Core/ResilientHttpClient.Core.csproj -c Release

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build failed with error code %ERRORLEVEL%
    echo.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Build successful!
echo.
echo DLL location: ResilientHttpClient.Core\bin\Release\netstandard2.1\ResilientHttpClient.Core.dll
echo.

set DLL_PATH=ResilientHttpClient.Core\bin\Release\netstandard2.1\ResilientHttpClient.Core.dll

:: Check for command line argument first (highest priority)
if not "%~1"=="" (
    set UNITY_PROJECT_PATH=%~1
    goto CopyDLL
)

:: Check for .env file (second priority)
set ENV_FILE=.env
if exist %ENV_FILE% (
    echo Reading Unity project path from .env file...
    for /F "tokens=1,2 delims==" %%a in (%ENV_FILE%) do (
        if "%%a"=="UNITY_PROJECT_PATH" (
            set UNITY_PROJECT_PATH=%%b
            echo Found Unity project path: !UNITY_PROJECT_PATH!
        )
    )
)

:: If we have a path from either source, proceed with copy
:CopyDLL
if not defined UNITY_PROJECT_PATH (
    echo No Unity project path specified.
    echo.
    echo You can specify a path in one of two ways:
    echo 1. Create a .env file in the same directory with the line:
    echo    UNITY_PROJECT_PATH=C:\Path\To\Your\UnityProject
    echo.
    echo 2. Run this script with the path as an argument:
    echo    build-and-copy.bat "C:\Path\To\Your\UnityProject"
    echo.
    echo The command line argument takes precedence over the .env file.
) else (
    set UNITY_PLUGINS_FOLDER=!UNITY_PROJECT_PATH!\Assets\Plugins
    
    if not exist "!UNITY_PLUGINS_FOLDER!" (
        echo Creating Plugins folder in your Unity project...
        mkdir "!UNITY_PLUGINS_FOLDER!"
    )
    
    echo Copying DLL to !UNITY_PLUGINS_FOLDER!...
    copy /Y "%DLL_PATH%" "!UNITY_PLUGINS_FOLDER!\"
    
    if %ERRORLEVEL% EQU 0 (
        echo.
        echo DLL successfully copied to your Unity project!
    ) else (
        echo.
        echo Failed to copy DLL to your Unity project.
    )
)

echo.
pause
endlocal 