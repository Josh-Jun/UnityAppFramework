echo off
rem step1 generate
set exe=bin\protoc.exe
%exe% --csharp_out=./output ./proto/AppProtoData.proto

rem step2 copy
copy ./output/AppProtoData.cs ../../Assets/AppFrame/Runtime/Frame/Manager/Network/Data/
copy ./proto/AppProtoData.proto ../../Assets/AppFrame/Runtime/Frame/Manager/Network/Data/
pause