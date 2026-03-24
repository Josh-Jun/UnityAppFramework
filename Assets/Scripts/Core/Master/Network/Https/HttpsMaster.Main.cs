/* *
 * ===============================================
 * author      : Junzi@macbook
 * e-mail      : shijun_z@163.com
 * create time : 2026年3月24 8:34
 * function    : 
 * ===============================================
 * */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Core.Master
{
    public partial class HttpsMaster
    {
        public const string TableName = "TokenTable";
        public const string USER_ID = "UserID";
        public const string TOKEN = "Token";
        protected override void OnSingletonMonoInit()
        {
            if (!DataBase.ExistsTable(TableName))
            {
                DataBase.CreateTable(TableName, new[] { USER_ID, TOKEN }, new[] { "TEXT", "TEXT" });
            }
        }
    }
}
