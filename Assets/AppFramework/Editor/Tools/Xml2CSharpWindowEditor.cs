using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

public class Xml2CSharpWindowEditor : EditorWindow
{
    private GUIStyle titleStyle;

    private string xmlPath = "";
    private string outPutPath = "";

    [MenuItem("Tools/My ToolsWindow/Xml to CSharp", false, 4)]
    public static void OpenWindow()
    {
        Xml2CSharpWindowEditor window = GetWindow<Xml2CSharpWindowEditor>("Xml to CSharp");
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

        xmlPath = "Resources/AssetsFolder/Table";
        outPutPath = "Scripts/Table";
    }
    private void OnGUI()
    {
        GUILayout.BeginVertical();

        EditorGUILayout.Space();
        xmlPath = EditorGUILayout.TextField("Xml Path", xmlPath);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
        {
            var newPath = EditorUtility.OpenFolderPanel("Bundle Folder", outPutPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                xmlPath = newPath;
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        outPutPath = EditorGUILayout.TextField("Out Put Path", outPutPath);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
        {
            var newPath = EditorUtility.OpenFolderPanel("Bundle Folder", outPutPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                outPutPath = newPath;
            }
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("生成C#类"))
        {
            EditorApplication.delayCall += Play;
        }
        GUILayout.EndVertical();
    }
    private void Play()
    {
        List<FileInfo> fileInfos = GetXmlFiles();
        for (int i = 0; i < fileInfos.Count; i++)
        {
            CreateClass(fileInfos[i]);
        }
    }
    private List<FileInfo> GetXmlFiles()
    {
        string xmlPath = string.Format("{0}/{1}", Application.dataPath, this.xmlPath);
        List<FileInfo> files = new List<FileInfo>();
        if (Directory.Exists(xmlPath))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(xmlPath);
            FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < fileInfos.Length; i++)
            {
                if (fileInfos[i].Name.EndsWith(".meta")) continue;
                if (fileInfos[i].Name.EndsWith(".xml"))
                {
                    files.Add(fileInfos[i]);
                }
            }
        }
        return files;
    }
    private void CreateClass(FileInfo fileInfo)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("using System;\r\nusing System.Collections.Generic;\r\nusing System.Xml.Serialization;\r\nnamespace TableData\r\n{\r\n\t{classText}\r\n}");
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
                var _classAttr = classAttrCountDic.Where(o => o.Key == item.Name.ToString() && o.Value != item.Elements().Count()).FirstOrDefault();

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
                                string lenStr = classText.ToString(indexStr + ($@"[XmlRoot(""{item.Name}"")]").Length + i, 1);
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
            string output = string.Format("{0}/{1}/{2}.cs", Application.dataPath, outPutPath, fileInfo.Name.Split('.')[0]);
            FileStream fs1 = new FileStream(output, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs1);
            sw.WriteLine(builder.ToString());//开始写入值
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

}
