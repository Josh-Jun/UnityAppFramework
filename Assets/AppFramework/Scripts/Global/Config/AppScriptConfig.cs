using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "AppScriptConfig", menuName = "App/AppScriptConfig")]
[Serializable]
public class AppScriptConfig : ScriptableObject
{
    [Header("App Script Config")]
    
    [Tooltip("主场景名(首次进入的场景)")]
    public string MainSceneName;//主场景名(首次进入的场景)
    [Tooltip("脚本集合")]
    public List<RootScript> RootScript;//脚本集合
}

[Serializable]
public class RootScript
{
    [Tooltip("所属场景名称(用来初始化Root脚本Begin方法)")]
    public string SceneName;//所属场景名称(用来初始化Root脚本Begin方法)
    [Tooltip("脚本名称(包含命名空间)")]
    public string ScriptName;//脚本名称(包含命名空间)
}