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
 
echo ======================= Client ==========================
%GEN_CLIENT% --template_search_path CustomTemplate -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_code_dir %OUTPUT_CODE_DIR%\Client\Config ^
 --output_data_dir %OUTPUT_DATA_DIR%\c\GameConfig ^
 --output:exclude_tags s ^
 --gen_types data_bin ^
 -s client

echo ======================= ClientServer ==========================
%GEN_CLIENT% --template_search_path CustomTemplate -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_code_dir %OUTPUT_CODE_DIR%\ClientServer\Config ^
 --output_data_dir %OUTPUT_DATA_DIR%\cs\GameConfig ^
 --gen_types code_cs_bin,data_bin ^
 -s all