using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "AppTableConfig", menuName = "App/AppTableConfig")]
[Serializable]
public class AppTableConfig : ScriptableObject
{
    [Header("App Table Config")]
    [Tooltip("Table列表")]
    public List<AppTable> AppTable;
}

[Serializable]
public class AppTable
{
    [Tooltip("Table名称")]
    public string TableName;
    [Tooltip("Table类型")]
    public TableMold TableMold;
}