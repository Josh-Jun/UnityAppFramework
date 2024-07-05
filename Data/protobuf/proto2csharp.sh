base_dir=$(pwd)
filename="AppProtoData"
# step1 generate
./bin/protoc -I=${base_dir}/proto --csharp_out=${base_dir}/output ${base_dir}/proto/${filename}.proto
# step2 copy
base_target_dir=$(dirname $(dirname $(pwd)))
target_dir=${base_target_dir}/Assets/AppFrame/Runtime/Frame/Manager/Network/Data/
cp ${base_dir}/output/${filename}.cs ${target_dir}
cp ${base_dir}/proto/${filename}.proto ${target_dir}
