echo off
rem step1 generate
set exe=bin\protoc.exe
for %%i in (proto/*.proto) do (
	%exe% --csharp_out=./output ./proto/%%i
	echo From %%i To %%~ni.cs is Successfully!
)
rem step2 copy
set target_dir=..\..\Assets\App\Scripts\Frame\Core\Master\Network\Socket\Data
copy .\output %target_dir%
pause