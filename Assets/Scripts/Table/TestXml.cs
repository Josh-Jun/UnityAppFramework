using System;
using System.Collections.Generic;
using System.Xml.Serialization;
namespace TableData
{
	[Serializable]
	[XmlRoot("TestXml")]
	public class TestXml
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
