base_dir=$(dirname $0)
# step1 generate
for file in ${base_dir}/proto/*.proto
do
	${base_dir}/bin/protoc -I=${base_dir}/proto --csharp_out=${base_dir}/output $file
	name=${file##*/}
	echo From $name To ${name%%.*}.cs is Successfully!
done
# step2 copy
base_target_dir=$(dirname $(dirname ${base_dir}))
target_dir=${base_target_dir}/Assets/App/Scripts/Frame/Core/Master/Network/Data/Protobuf/
for file in ${base_dir}/output/*.cs
do
	cp $file ${target_dir}
done
