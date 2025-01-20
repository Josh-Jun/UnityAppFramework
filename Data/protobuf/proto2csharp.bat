echo off
rem step1 generate
set exe=bin\protoc.exe
%exe% --csharp_out=./output ./proto/AppProtoData.proto

rem step2 copy
copy ./output/AppProtoData.cs ../../Assets/App/Scripts/Frame/Core/Master/Network/Data/Protobuf/
copy ./proto/AppProtoData.proto ../../Assets/App/Scripts/Frame/Core/Master/Network/Data/Protobuf/
pause