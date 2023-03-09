PROTO_OUT=./output/AppProtoData.Proto
CSHARP_DLL_OUT_PATH=./dll
PROJECT_COPY_DIR=../../Assets/AppFrame/Runtime/Frame/Global

rm -rf $PROTO_OUT
mkdir $PROTO_OUT


if [ ! -d $CSHARP_DLL_OUT_PATH ];then 
   mkdir $CSHARP_DLL_OUT_PATH
   echo $CSHARP_DLL_OUT_PATH
fi

protoc ./proto/*.proto -I=./proto \
    --csharp_out=$PROTO_OUT
if [ "$?" -ne "0" ]; then
  echo "Failure  check proto files"
  exit 1
fi

cd $PROTO_OUT

dotnet new classlib --language C#  --framework "netstandard2.0"
rm Class1.cs 
dotnet add package Google.Protobuf -v 3.17.3

dotnet publish  -o $CSHARP_DLL_OUT_PATH

if [ "$?" -ne "0" ]; then
  echo "Failure  check log"
  exit 1
fi

cp -af ./dll/ $PROJECT_COPY_DIR