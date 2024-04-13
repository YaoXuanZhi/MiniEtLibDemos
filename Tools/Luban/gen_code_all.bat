@echo off
cd /d %~dp0
set WORKSPACE=..\..

set LUBAN_DLL=%WORKSPACE%\Tools\Luban\LubanRelease\Luban.dll
set CONF_ROOT=%WORKSPACE%\UnityDemo\Assets\Config\Excel

::Client
dotnet %LUBAN_DLL% ^
    --customTemplateDir CustomTemplate ^
    -t client ^
    -c cs-bin ^
    -d bin ^
    -d json ^
    --conf %CONF_ROOT%\__luban__.conf ^
    -x outputCodeDir=%WORKSPACE%\UnityDemo\Assets\Scripts\Codes\Model\Generate\Client\Config ^
    -x bin.outputDataDir=%WORKSPACE%\Config\Excel\c ^
    -x json.outputDataDir=%WORKSPACE%\Config\Json\c ^
    -x lineEnding=CRLF ^
    

echo ==================== FuncConfig : GenClientFinish ====================

if %ERRORLEVEL% NEQ 0 (
    echo An error occurred, press any key to exit.
    pause
    exit /b
)

::Server
dotnet %LUBAN_DLL% ^
    --customTemplateDir CustomTemplate ^
    -t all ^
    -c cs-bin ^
    -d bin ^
    -d json ^
    --conf %CONF_ROOT%\__luban__.conf ^
    -x outputCodeDir=%WORKSPACE%\UnityDemo\Assets\Scripts\Codes\Model\Generate\Server\Config ^
    -x bin.outputDataDir=%WORKSPACE%\Config\Excel\s ^
    -x json.outputDataDir=%WORKSPACE%\Config\Json\s ^
    -x lineEnding=CRLF ^
    

echo ==================== FuncConfig : GenServerFinish ====================

if %ERRORLEVEL% NEQ 0 (
    echo An error occurred, press any key to exit.
    pause
    exit /b
)

::StartConfig Release
dotnet %LUBAN_DLL% ^
    --customTemplateDir CustomTemplate ^
    -t all ^
    -c cs-bin ^
    -d bin ^
    -d json ^
    --conf %CONF_ROOT%\StartConfig\Release\__luban__.conf ^
    -x outputCodeDir=%WORKSPACE%\UnityDemo\Assets\Scripts\Codes\Model\Generate\Server\Config\StartConfig ^
    -x bin.outputDataDir=%WORKSPACE%\Config\Excel\s\StartConfig\Release ^
    -x json.outputDataDir=%WORKSPACE%\Config\Json\s\StartConfig\Release ^
    -x lineEnding=CRLF ^
    

echo ==================== StartConfig : GenReleaseFinish ====================

if %ERRORLEVEL% NEQ 0 (
    echo An error occurred, press any key to exit.
    pause
    exit /b
)

::StartConfig Benchmark
dotnet %LUBAN_DLL% ^
    --customTemplateDir CustomTemplate ^
    -t all ^
    -d bin ^
    -d json ^
    --conf %CONF_ROOT%\StartConfig\Benchmark\__luban__.conf ^
    -x bin.outputDataDir=%WORKSPACE%\Config\Excel\s\StartConfig\Benchmark ^
    -x json.outputDataDir=%WORKSPACE%\Config\Json\s\StartConfig\Benchmark ^
    

echo ==================== StartConfig : GenBenchmarkFinish ====================

if %ERRORLEVEL% NEQ 0 (
    echo An error occurred, press any key to exit.
    pause
    exit /b
)

::StartConfig Localhost
dotnet %LUBAN_DLL% ^
    --customTemplateDir CustomTemplate ^
    -t all ^
    -d bin ^
    -d json ^
    --conf %CONF_ROOT%\StartConfig\Localhost\__luban__.conf ^
    -x bin.outputDataDir=%WORKSPACE%\Config\Excel\s\StartConfig\Localhost ^
    -x json.outputDataDir=%WORKSPACE%\Config\Json\s\StartConfig\Localhost ^
    

echo ==================== StartConfig : GenLocalhostFinish ====================

if %ERRORLEVEL% NEQ 0 (
    echo An error occurred, press any key to exit.
    pause
    exit /b
)

::StartConfig RouterTest
dotnet %LUBAN_DLL% ^
    --customTemplateDir CustomTemplate ^
    -t all ^
    -d bin ^
    -d json ^
    --conf %CONF_ROOT%\StartConfig\RouterTest\__luban__.conf ^
    -x bin.outputDataDir=%WORKSPACE%\Config\Excel\s\StartConfig\RouterTest ^
    -x json.outputDataDir=%WORKSPACE%\Config\Json\s\StartConfig\RouterTest ^
    

echo ==================== StartConfig : GenRouterTestFinish ====================


if %ERRORLEVEL% NEQ 0 (
    echo An error occurred, press any key to exit.
    pause
    exit /b
)

::Server
dotnet %LUBAN_DLL% ^
    --customTemplateDir CustomTemplate ^
    -t all ^
    -c cs-bin ^
    -d bin ^
    -d json ^
    --conf %CONF_ROOT%\__luban__.conf ^
    -x outputCodeDir=%WORKSPACE%\UnityDemo\Assets\Scripts\Codes\Model\Generate\ClientServer\Config ^
    -x bin.outputDataDir=%WORKSPACE%\Config\Excel\cs ^
    -x json.outputDataDir=%WORKSPACE%\Config\Json\cs ^
    -x lineEnding=CRLF ^
    

echo ==================== FuncConfig : GenClientServerFinish ====================

if %ERRORLEVEL% NEQ 0 (
    echo An error occurred, press any key to exit.
    pause
    exit /b
)

::StartConfig Release
dotnet %LUBAN_DLL% ^
    --customTemplateDir CustomTemplate ^
    -t all ^
    -c cs-bin ^
    -d bin ^
    -d json ^
    --conf %CONF_ROOT%\StartConfig\Release\__luban__.conf ^
    -x outputCodeDir=%WORKSPACE%\UnityDemo\Assets\Scripts\Codes\Model\Generate\ClientServer\Config\StartConfig ^
    -x bin.outputDataDir=%WORKSPACE%\Config\Excel\cs\StartConfig\Release ^
    -x json.outputDataDir=%WORKSPACE%\Config\Json\cs\StartConfig\Release ^
    -x lineEnding=CRLF ^
    

echo ==================== StartConfig : GenReleaseFinish ====================

if %ERRORLEVEL% NEQ 0 (
    echo An error occurred, press any key to exit.
    pause
    exit /b
)

::StartConfig Benchmark
dotnet %LUBAN_DLL% ^
    --customTemplateDir CustomTemplate ^
    -t all ^
    -d bin ^
    -d json ^
    --conf %CONF_ROOT%\StartConfig\Benchmark\__luban__.conf ^
    -x bin.outputDataDir=%WORKSPACE%\Config\Excel\cs\StartConfig\Benchmark ^
    -x json.outputDataDir=%WORKSPACE%\Config\Json\cs\StartConfig\Benchmark ^
    

echo ==================== StartConfig : GenBenchmarkFinish ====================

if %ERRORLEVEL% NEQ 0 (
    echo An error occurred, press any key to exit.
    pause
    exit /b
)

::StartConfig Localhost
dotnet %LUBAN_DLL% ^
    --customTemplateDir CustomTemplate ^
    -t all ^
    -d bin ^
    -d json ^
    --conf %CONF_ROOT%\StartConfig\Localhost\__luban__.conf ^
    -x bin.outputDataDir=%WORKSPACE%\Config\Excel\cs\StartConfig\Localhost ^
    -x json.outputDataDir=%WORKSPACE%\Config\Json\cs\StartConfig\Localhost ^
    

echo ==================== StartConfig : GenLocalhostFinish ====================

if %ERRORLEVEL% NEQ 0 (
    echo An error occurred, press any key to exit.
    pause
    exit /b
)

::StartConfig RouterTest
dotnet %LUBAN_DLL% ^
    --customTemplateDir CustomTemplate ^
    -t all ^
    -d bin ^
    -d json ^
    --conf %CONF_ROOT%\StartConfig\RouterTest\__luban__.conf ^
    -x bin.outputDataDir=%WORKSPACE%\Config\Excel\cs\StartConfig\RouterTest ^
    -x json.outputDataDir=%WORKSPACE%\Config\Json\cs\StartConfig\RouterTest ^
    

echo ==================== StartConfig : GenRouterTestFinish ====================

::pause