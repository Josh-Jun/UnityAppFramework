using System;
using System.Collections.Generic;

namespace AppFrame.Data.Json
{
    [System.Serializable]
    public class UserJsonData
    {
        public List<User> User = new List<User>();
    }
    [System.Serializable]
    public class User
    {
        public int UserId;
        public long PhoneNumber;
        public string NickName;
        public int Sex;
        public int Age;
    }
}
