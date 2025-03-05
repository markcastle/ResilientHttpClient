@echo off
echo Building ResilientHttpClient in Release mode...
echo.

dotnet build ResilientHttpClient.Core/ResilientHttpClient.Core.csproj -c Release

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build successful!
    echo.
    echo DLL location: ResilientHttpClient.Core\bin\Release\netstandard2.1\ResilientHttpClient.Core.dll
    echo.
    echo You can copy this DLL to your Unity project's Assets/Plugins folder.
) else (
    echo.
    echo Build failed with error code %ERRORLEVEL%
)

echo.
pause 