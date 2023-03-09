echo off
rem step1 generate
set exe=bin\protoc.exe
%exe% --csharp_out=./output ./proto/AppProtoData.proto

rem step2 copy
copy output\AppProtoData.cs ..\..\Assets\AppFrame\Runtime\Frame\Global\
pause