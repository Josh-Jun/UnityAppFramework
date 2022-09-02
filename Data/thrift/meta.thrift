//	Only support following types
//	-----------------------------
//	i8: An 8-bit signed integer
//	i16: A 16-bit signed integer
//	i32: A 32-bit signed integer
//	i64: A 64-bit signed integer
//	double: A 64-bit floating point number
//	string: A text string encoded using UTF-8 encoding
//	-----------------------------

struct TestThrift
{
	1 : required i32 id;
	2 : optional string skuNo;
	3 : optional i32 lanter_type;
	4 : optional i32  isunique;
	5 : optional string model_name;
	6 : optional i32 flytime;
	7 : optional i32 isdestroy;
}

struct TestThriftTable
{
	1 : required list<TestThrift> data;
}