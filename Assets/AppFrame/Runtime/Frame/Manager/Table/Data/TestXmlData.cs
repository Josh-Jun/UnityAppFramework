using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AppFrame.Data.Xml
{
    [System.Serializable]
    public class TestXmlData
    {
        [XmlElement("Test")]
        public List<Test> Tests = new List<Test>();
    }
    [System.Serializable]
    public class Test
    {
        [XmlAttribute("UserId")]
        public int UserId;
        [XmlAttribute("PhoneNumber")]
        public long PhoneNumber;
        [XmlAttribute("NickName")]
        public string NickName;
        [XmlAttribute("Sex")]
        public int Sex;
        [XmlAttribute("Age")]
        public int Age;
    }
}
