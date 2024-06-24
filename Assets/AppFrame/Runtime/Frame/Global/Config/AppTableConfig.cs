using System;
using System.Collections.Generic;
using AppFrame.Enum;
using UnityEngine;

namespace AppFrame.Config
{
    [CreateAssetMenu(fileName = "AppTableConfig", menuName = "App/AppTableConfig")]
    [Serializable]
    public class AppTableConfig : ScriptableObject
    {
        [Header("App Table Config")] [Tooltip("Table列表")]
        public List<AppTable> AppTable;
    }

    [Serializable]
    public class AppTable
    {
        [Tooltip("Table名称")] public string TableName;
        [Tooltip("Table类型")] public TableMold TableMold;
    }
}