using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Table2CSharpWindowEditor : EditorWindow
{
    private GUIStyle titleStyle;
    private Vector2 scrollPosition;

    private TableMold TableMold = TableMold.Json;

    private string tablePath = "";
    private string outPutPath = "";

    [MenuItem("Tools/My ToolsWindow/Table to CSharp", false, 4)]
    public static void OpenWindow()
    {
        Table2CSharpWindowEditor window = GetWindow<Table2CSharpWindowEditor>("Table to CSharp");
        window.Show();
    }

    public void OnEnable()
    {
        titleStyle = new GUIStyle("OL Title")
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };

        tablePath = $"Resources/AssetsFolder/Table/{TableMold}";
        outPutPath = "Scripts/Table";
        GetFiles();
    }

    static List<bool> m_ExportList = new List<bool>();
    static List<string> m_DataList = new List<string>();

    private void OnGUI()
    {
        GUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("TableMold"));
        GUILayout.FlexibleSpace();
        TableMold = (TableMold)EditorGUILayout.EnumPopup(TableMold, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();

        tablePath = $"Resources/AssetsFolder/Table/{TableMold}";

        EditorGUILayout.BeginHorizontal();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(128));
        {
            GetFiles();
            for (int i = 0; i < m_DataList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                m_ExportList[i] = EditorGUILayout.Toggle(m_ExportList[i], GUILayout.MaxWidth(32));
                GUILayout.Label(m_DataList[i]);
                EditorGUILayout.EndHorizontal();
            }
        }
        GUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();


        if (GUILayout.Button("生成C#类"))
        {
            Create();
        }

        GUILayout.EndVertical();
    }

    private void Create()
    {
        List<FileInfo> fileInfos = GetFiles();
        switch (TableMold)
        {
            case TableMold.Xml:
                for (int i = 0; i < fileInfos.Count; i++)
                {
                    if (m_ExportList[i])
                    {
                        CreateClass(fileInfos[i]);
                    }
                }
                break;
            case TableMold.Json:
                for (int i = 0; i < fileInfos.Count; i++)
                {
                    if (m_ExportList[i])
                    {
                        CreateClass(fileInfos[i].Name);
                    }
                }
                break;
        }
    }

    private List<FileInfo> GetFiles()
    {
        m_ExportList.Clear();
        m_DataList.Clear();
        string tablePath = string.Format("{0}/{1}", Application.dataPath, this.tablePath);
        List<FileInfo> files = new List<FileInfo>();
        if (Directory.Exists(tablePath))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(tablePath);
            FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < fileInfos.Length; i++)
            {
                if (fileInfos[i].Name.EndsWith(".meta")) continue;
                files.Add(fileInfos[i]);
                m_ExportList.Add(true);
                m_DataList.Add(fileInfos[i].Name);
            }
        }

        return files;
    }

    private void CreateClass(FileInfo fileInfo)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(
            "using System;\r\nusing System.Collections.Generic;\r\nusing System.Xml.Serialization;\r\nnamespace TableData\r\n{\r\n\t{classText}\r\n}");
        builder.Replace("{namespace}", "TableData");
        StringBuilder classText = new StringBuilder();

        try
        {
            XElement element = XElement.Load(fileInfo.FullName);
            //子节点类集合
            List<string> classList = new List<string>();

            Dictionary<string, int> classAttrCountDic = new Dictionary<string, int>();

            foreach (var item in element.DescendantsAndSelf())
            {
                Dictionary<string, StringBuilder> itemDics = new Dictionary<string, StringBuilder>();

                var _class = classList.Where(o => o == item.Name.ToString()).FirstOrDefault();
                var _classAttr = classAttrCountDic
                    .Where(o => o.Key == item.Name.ToString() && o.Value != item.Elements().Count()).FirstOrDefault();

                StringBuilder classText1 = new StringBuilder();
                if (item.Elements().Count() == 0)
                {
                    classList.Add(item.Name.ToString());

                    if (_class != null)
                    {
                        continue;
                    }

                    //如果此节点下没有属性则直接跳过
                    if (item.Attributes().Count() == 0)
                    {
                        StringBuilder childTxt = new StringBuilder();
                        childTxt.Append("[XmlElement(").Append($@"""{item.Name}"")]");
                        childTxt.Append($"\r\n\t\tpublic string {item.Name}");
                        childTxt.Append(" { get; set; }");
                        classText1.Append("\r\n\t\t{").Append(item.Name.ToString()).Append("}");
                        itemDics.Add(item.Name.ToString(), childTxt);
                        continue;
                    }

                    //没有子节点了
                    classText1.Append("\r\n\t[Serializable]");
                    classText1.Append("\r\n\t[XmlRoot(").Append($@"""{item.Name}"")]");
                    classText1.Append($"\r\n\tpublic class {item.Name}");
                    classText1.Append("\r\n\t{");
                    //classList.Add(item.Name.ToString());

                    //创建该节点对应的属性
                    if (item.Attributes().Count() > 0)
                    {
                        foreach (var itemAttr in item.Attributes())
                        {
                            classText1.Append("\r\n\t\t[XmlAttribute(").Append($@"""{itemAttr.Name}"")]");
                            classText1.Append($"\r\n\t\tpublic string {itemAttr.Name}");
                            classText1.Append(" { get; set; }");
                        }
                    }

                    classText1.Append("\r\n\t}");
                }
                else
                {
                    if (_class != null)
                    {
                        if (_classAttr.Key != null)
                        {
                            //返回先前创建类在字符串中的位置
                            int indexStr = classText.ToString().IndexOf($@"[XmlRoot(""{item.Name}"")]");
                            StringBuilder itemStr = new StringBuilder();
                            //itemStr.Append("\t");
                            itemStr.Append($@"[XmlRoot(""{item.Name.ToString()}"")]");
                            int i = 0;
                            while (true)
                            {
                                string lenStr =
                                    classText.ToString(indexStr + ($@"[XmlRoot(""{item.Name}"")]").Length + i, 1);
                                i = i + 1;
                                itemStr.Append(lenStr);
                                if (itemStr.ToString().Contains("\r\n\t}"))
                                {
                                    break;
                                }
                            }

                            classText.Replace(itemStr.ToString(), null);
                            classList.Remove(item.Name.ToString());
                            classAttrCountDic.Remove(item.Name.ToString());
                        }
                        else
                        {
                            continue;
                        }
                    }

                    classList.Add(item.Name.ToString());
                    classAttrCountDic.Add(item.Name.ToString(), item.Elements().Count());

                    //创建该节点对应的类
                    classText1.Append("[Serializable]");
                    classText1.Append("\r\n\t[XmlRoot(").Append($@"""{item.Name}"")]");
                    classText1.Append($"\r\n\tpublic class {item.Name}");
                    classText1.Append("\r\n\t{");

                    //创建该节点子节点对应的类
                    foreach (var itemElement in item.Elements())
                    {
                        StringBuilder childTxt = new StringBuilder();

                        var itemElementDic = itemDics.Where(o => o.Key == itemElement.Name.ToString());
                        if (itemElementDic.Count() > 0)
                        {
                            childTxt.Append("[XmlElement(").Append($@"""{itemElement.Name}"")]");
                            childTxt.Append($"\r\n\t\tpublic List<{itemElement.Name}> {itemElement.Name}");
                            childTxt.Append(" { get; set; }");
                            itemDics.Remove(itemElement.Name.ToString());
                            itemDics.Add(itemElement.Name.ToString(), childTxt);
                        }
                        else
                        {
                            if (itemElement.Elements().Count() == 0)
                            {
                                childTxt.Append("\r\n\t\t[XmlElement(").Append($@"""{itemElement.Name}"")]");
                                childTxt.Append($"\r\n\t\tpublic string {itemElement.Name}");
                                childTxt.Append(" { get; set; }");
                                classText1.Append("\r\n\t\t{").Append(itemElement.Name.ToString()).Append("}");
                                itemDics.Add(itemElement.Name.ToString(), childTxt);
                            }
                            else
                            {
                                childTxt.Append("\r\n\t\t[XmlElement(").Append($@"""{itemElement.Name}"")]");
                                childTxt.Append($"\r\n\t\tpublic {itemElement.Name} {itemElement.Name}");
                                childTxt.Append(" { get; set; }");
                                classText1.Append("\r\n\t\t{").Append(itemElement.Name.ToString()).Append("}");
                                itemDics.Add(itemElement.Name.ToString(), childTxt);
                            }
                        }
                    }

                    //创建该节点对应的属性
                    if (item.Attributes().Count() > 0)
                    {
                        foreach (var itemAttr in item.Attributes())
                        {
                            classText1.Append("\r\n\t\t[XmlAttribute(").Append($@"""{itemAttr.Name}"")]");
                            classText1.Append($"\r\n\t\tpublic string {itemAttr.Name}");
                            classText1.Append(" { get; set; }");
                        }
                    }

                    classText1.Append("\r\n\t}");
                }

                classText.Append(classText1);

                foreach (var itemDic in itemDics)
                {
                    classText.Replace("{" + itemDic.Key + "}", itemDic.Value.ToString());
                }
            }

            builder.Replace("{classText}", classText.ToString());
            string output = string.Format("{0}/{1}/{2}.cs", Application.dataPath, outPutPath,
                fileInfo.Name.Split('.')[0]);
            FileStream fs1 = new FileStream(output, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs1);
            sw.WriteLine(builder.ToString()); //开始写入值
            sw.Close();
            fs1.Close();

            AssetDatabase.Refresh();
            Debug.Log(builder.ToString());
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void CreateClass(string filename)
    {
        string path = $"AssetsFolder/Table/{TableMold}/{filename.Split('.')[0]}";
        var jsonString = Resources.Load<TextAsset>(path).text; //读取json文件里的内容

        var jObject = JObject.Parse(jsonString); //Newtonsoft.Json中的JObject.Parse转换成json对象

        Dictionary<string, string> classDicts = new Dictionary<string, string>(); //key为类名，value为类中的所有属性定义的字符串
        classDicts.Add(filename.Split('.')[0], GetClassDefinion(jObject)); //拼接顶层的类
        foreach (var item in jObject.Properties())
        {
            classDicts.Add(item.Name, GetClassDefinion(item.Value));
            GetClasses(item.Value, classDicts);
        }

        //下面是将所有的类定义完整拼接起来
        StringBuilder sb = new StringBuilder(1024);
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("");
        sb.AppendLine("namespace TableData");
        sb.AppendLine("{");
        foreach (var item in classDicts)
        {
            sb.Append($"    [System.Serializable]" + Environment.NewLine);
            sb.Append($"    public class {item.Key}" + Environment.NewLine);
            sb.Append("    {" + Environment.NewLine);
            sb.Append(item.Value);
            sb.Append("    }" + Environment.NewLine);
        }

        sb.AppendLine("}");

        string output = string.Format("{0}/{1}/{2}.cs", Application.dataPath, outPutPath, filename.Split('.')[0]);
        FileStream fs1 = new FileStream(output, FileMode.Create, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs1);
        sw.WriteLine(sb.ToString()); //开始写入值
        sw.Close();
        fs1.Close();

        AssetDatabase.Refresh();
    }

    //递归遍历json节点，把需要定义的类存入classes
    void GetClasses(JToken jToken, Dictionary<string, string> classes)
    {
        if (jToken is JValue)
        {
            return;
        }

        var childToken = jToken.First;
        while (childToken != null)
        {
            if (childToken.Type == JTokenType.Property)
            {
                var p = (JProperty)childToken;
                Debug.Log(p.Value.Type);
                var valueType = p.Value.Type;
                if (valueType == JTokenType.Object)
                {
                    classes.Add(p.Name, GetClassDefinion(p.Value));
                    GetClasses(p.Value, classes);
                }
                else if (valueType == JTokenType.Array)
                {
                    foreach (var item in (JArray)p.Value)
                    {
                        if (item.Type == JTokenType.Object)
                        {
                            if (!classes.ContainsKey(p.Name))
                            {
                                classes.Add(p.Name, GetClassDefinion(item));
                            }

                            GetClasses(item, classes);
                        }
                        else
                        {
                        }
                    }
                }
            }

            childToken = childToken.Next;
        }
    }

    //获取类中的所有的属性
    string GetClassDefinion(JToken jToken)
    {
        if (!jToken.HasValues)
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder(1024);
        var subValueToken = jToken.First();
        while (subValueToken != null)
        {
            if (subValueToken.Type == JTokenType.Property)
            {
                var p = (JProperty)subValueToken;
                var valueType = p.Value.Type;
                if (valueType == JTokenType.Object)
                {
                    sb.Append("        public " + p.Name + " " + p.Name + ";" + Environment.NewLine);
                }
                else if (valueType == JTokenType.Array)
                {
                    var arr = (JArray)p.Value;
                    //a.First

                    switch (arr.First().Type)
                    {
                        case JTokenType.Object:
                            sb.Append($"        public List<{p.Name}> " + p.Name + ";" + Environment.NewLine);
                            break;
                        case JTokenType.Integer:
                            sb.Append($"        public List<int> " + p.Name + ";" + Environment.NewLine);
                            break;
                        case JTokenType.Float:
                            sb.Append($"        public List<float> " + p.Name + ";" + Environment.NewLine);
                            break;
                        case JTokenType.String:
                            sb.Append($"        public List<string> " + p.Name + ";" + Environment.NewLine);
                            break;
                        case JTokenType.Boolean:
                            sb.Append($"        public List<bool> " + p.Name + ";" + Environment.NewLine);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (valueType)
                    {
                        case JTokenType.Integer:
                            sb.Append($"        public int " + p.Name + ";" + Environment.NewLine);
                            break;
                        case JTokenType.Float:
                            sb.Append($"        public float " + p.Name + ";" + Environment.NewLine);
                            break;
                        case JTokenType.String:
                            sb.Append($"        public string " + p.Name + ";" + Environment.NewLine);
                            break;
                        case JTokenType.Boolean:
                            sb.Append($"        public bool " + p.Name + ";" + Environment.NewLine);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (subValueToken.Type == JTokenType.Object)
            {
                var str = GetClassDefinion(subValueToken).ToString();
                if (!sb.ToString().Contains(str))
                {
                    sb.Append(str);
                }
            }

            subValueToken = subValueToken.Next;
        }

        return sb.ToString();
    }
}