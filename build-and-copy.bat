@echo off
setlocal

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

if "%~1"=="" (
    echo To copy the DLL to your Unity project, run this script with the path to your Unity project:
    echo.
    echo     build-and-copy.bat "C:\Path\To\Your\UnityProject"
    echo.
) else (
    set UNITY_PLUGINS_FOLDER=%~1\Assets\Plugins
    
    if not exist "%UNITY_PLUGINS_FOLDER%" (
        echo Creating Plugins folder in your Unity project...
        mkdir "%UNITY_PLUGINS_FOLDER%"
    )
    
    echo Copying DLL to %UNITY_PLUGINS_FOLDER%...
    copy /Y "%DLL_PATH%" "%UNITY_PLUGINS_FOLDER%\"
    
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