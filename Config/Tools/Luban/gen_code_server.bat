@echo off
rem ----------------------
rem 注意使用GBK编码编辑此文件
rem ----------------------

cd /d %~dp0

call config.bat

echo ======================= Server ==========================
%GEN_CLIENT% --template_search_path CustomTemplate -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_code_dir %OUTPUT_CODE_DIR%\Server\Config ^
 --output_data_dir %OUTPUT_DATA_DIR%\s\GameConfig ^
 --output:exclude_tags c ^
 --gen_types data_bin ^
 -s server
 
if %ERRORLEVEL% NEQ 0 exit

@REM echo ======================= Server Json ==========================
@REM %GEN_CLIENT% --template_search_path CustomTemplate -j cfg --^
@REM  -d %CONF_ROOT%\Defines\__root__.xml ^
@REM  --input_data_dir %CONF_ROOT%\Datas ^
@REM  --output_code_dir %OUTPUT_CODE_DIR%\Server\Config ^
@REM  --output_data_dir %OUTPUT_JSON_DIR%\s\GameConfig ^
@REM  --output:exclude_tags c ^
@REM  --gen_types data_json ^
@REM  -s server

::if %ERRORLEVEL% NEQ 0 exit
pause