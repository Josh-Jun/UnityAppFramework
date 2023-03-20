using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AppFrame.Data.Xml
{
    [System.Serializable]
    public class UserDataXml
    {
        [XmlElement("UserData")]
        public List<UserData> UserData = new List<UserData>();
    }
    [System.Serializable]
    public class UserData
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
