base_dir=$(dirname $0)
filename="AppProtoData"
# step1 generate
${base_dir}/bin/protoc -I=${base_dir}/proto --csharp_out=${base_dir}/output ${base_dir}/proto/${filename}.proto
# step2 copy
base_target_dir=$(dirname $(dirname ${base_dir}))
target_dir=${base_target_dir}/Assets/AppFrame/Runtime/Frame/Manager/Network/Data/
cp ${base_dir}/output/${filename}.cs ${target_dir}
cp ${base_dir}/proto/${filename}.proto ${target_dir}
