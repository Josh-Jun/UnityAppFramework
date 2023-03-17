using System;
using System.Collections.Generic;

namespace AppFrame.Data
{
    [System.Serializable]
    public class UserDataJson
    {
        public List<UserData> UserData = new List<UserData>();
    }
    [System.Serializable]
    public class UserData
    {
        public int UserId;
        public long PhoneNumber;
        public string NickName;
        public int Sex;
        public int Age;
    }
}
