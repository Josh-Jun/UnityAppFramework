using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using ThriftTable;
using UnityEngine;

public class TableManager : SingletonMono<TableManager>
{
    private Dictionary<string, Table> m_TablePairs = new Dictionary<string, Table>();
    private AppTableConfig appTableConfig;
    public override void InitParent(Transform parent)
    {
        base.InitParent(parent);
        
        RegiterType();
        RegisterFloat();
        RegisterVector3();
        RegisterVector2();
        RegisterQuaternion();
    }
    public void InitConfig()
    {
        appTableConfig = AssetsManager.Instance.LoadAsset<AppTableConfig>(AssetsPathConfig.AppTableConfig);
        for (int i = 0; i < appTableConfig.AppTable.Count; i++)
        {
            var path = $"Table/{appTableConfig.AppTable[i].TableMold}/{appTableConfig.AppTable[i].TableName}";
            var text = AssetsManager.Instance.LoadAsset<TextAsset>(path).text;
            var bytes = AssetsManager.Instance.LoadAsset<TextAsset>(path).bytes;
            Table table = new Table(text, bytes, path, appTableConfig.AppTable[i].TableMold);
            if (m_TablePairs.ContainsKey(appTableConfig.AppTable[i].TableName))
            {
                m_TablePairs[appTableConfig.AppTable[i].TableName] = table;
            }
            else
            {
                m_TablePairs.Add(appTableConfig.AppTable[i].TableName, table);
            }
        }
        for (var en = ThriftTableHolder.ETXT_NAME._Min + 1; en < ThriftTableHolder.ETXT_NAME._Max; en++)
        {
            if (m_TablePairs.ContainsKey(en.ToString()))
            {
                ThriftTableHolder.m_TableDic[en].ParseData(m_TablePairs[en.ToString()].bytes);
            }
        }
    }
    public T GetTable<T>(string tableName) where T : class
    {
        var table = m_TablePairs[tableName];
        T t = null;
        switch (table.mold)
        {
            case TableMold.Xml:
                t = XmlTools.ProtoDeSerialize<T>(table.bytes);
                break;
            case TableMold.Json:
                t = JsonMapper.ToObject<T>(table.text);
                break;
            case TableMold.Thrift:
                ThriftTableHolder.ETXT_NAME en = (ThriftTableHolder.ETXT_NAME)Enum.Parse(typeof(ThriftTableHolder.ETXT_NAME), tableName);
                t = ThriftTableHolder.GetTable(en) as T;
                break;
            
        }
        return t;
    }
    public static void RegiterType()
    {
        void Exporter(Type obj, JsonWriter writer)
        {
            writer.Write(obj.FullName);
        }

        JsonMapper.RegisterExporter((ExporterFunc<Type>)Exporter);

        Type Importer(string obj)
        {
            return Type.GetType(obj);
        }

        JsonMapper.RegisterImporter((ImporterFunc<string, Type>)Importer);
    }
    private static void RegisterFloat()
    {
        void Exporter(float obj, JsonWriter writer)
        {
            writer.Write(obj);
        }

        JsonMapper.RegisterExporter((ExporterFunc<float>)Exporter);

        float Importer(double obj)
        {
            return (float)obj;
        }

        JsonMapper.RegisterImporter((ImporterFunc<double, float>)Importer);
    }
    private static void RegisterVector3()
    {
        void Exporter(Vector3 obj, JsonWriter writer)
        {
            writer.WriteObjectStart();

            writer.WritePropertyName("x");//写入属性名
            writer.Write(obj.x);//写入值
            writer.WritePropertyName("y");
            writer.Write(obj.y);
            writer.WritePropertyName("z");
            writer.Write(obj.z);

            writer.WriteObjectEnd();
        }

        JsonMapper.RegisterExporter((ExporterFunc<Vector3>)Exporter);//序列化
    }

    private static void RegisterVector2()
    {
        void Exporter(Vector2 obj, JsonWriter writer)
        {
            writer.WriteObjectStart();

            writer.WritePropertyName("x");//写入属性名
            writer.Write(obj.x);//写入值
            writer.WritePropertyName("y");
            writer.Write(obj.y);

            writer.WriteObjectEnd();
        }

        JsonMapper.RegisterExporter((ExporterFunc<Vector2>)Exporter);//序列化
    }

    private static void RegisterQuaternion()
    {
        void Exporter(Quaternion obj, JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("x");//写入属性名
            writer.Write(obj.x);//写入值
            writer.WritePropertyName("y");
            writer.Write(obj.y);
            writer.WritePropertyName("z");
            writer.Write(obj.z);
            writer.WritePropertyName("w");
            writer.Write(obj.w);
            writer.WriteObjectEnd();
        }
        JsonMapper.RegisterExporter((ExporterFunc<Quaternion>)Exporter);//序列化
    }
}

public class Table
{
    public string text;
    public byte[] bytes;
    public string path;
    public TableMold mold;

    public Table(string text, byte[] bytes, string path, TableMold mold)
    {
        this.text = text;
        this.bytes = bytes;
        this.path = path;
        this.mold = mold;
    }
}
