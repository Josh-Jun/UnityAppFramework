using System;
using System.Collections.Generic;
using System.Xml.Serialization;
namespace TableData
{
	[Serializable]
	[XmlRoot("TestTableData")]
	public class TestTableData
	{
		[XmlElement("TestTable")]
		public List<TestTable> TestTable { get; set; }
	}
	[Serializable]
	[XmlRoot("TestTable")]
	public class TestTable
	{
		[XmlAttribute("Test")]
		public string Test { get; set; }
	}
}
