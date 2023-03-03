@echo off
rem ----------------------
rem 注意使用GBK编码编辑此文件
rem ----------------------

cd /d %~dp0

call config.bat

echo ======================= Client ==========================
%GEN_CLIENT% --template_search_path CustomTemplate -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_code_dir %OUTPUT_CODE_DIR%\Client\Config ^
 --output_data_dir %OUTPUT_DATA_DIR%\c\GameConfig ^
 --output:exclude_tags s ^
 --gen_types data_bin ^
 -s client
 
if %ERRORLEVEL% NEQ 0 exit

@REM echo ======================= Client Json ==========================
@REM %GEN_CLIENT% --template_search_path CustomTemplate -j cfg --^
@REM  -d %CONF_ROOT%\Defines\__root__.xml ^
@REM  --input_data_dir %CONF_ROOT%\Datas ^
@REM  --output_code_dir %OUTPUT_CODE_DIR%\Client\Config ^
@REM  --output_data_dir %OUTPUT_JSON_DIR%\c\GameConfig ^
@REM  --output:exclude_tags s ^
@REM  --gen_types data_json ^
@REM  -s client

::if %ERRORLEVEL% NEQ 0 exit
pause