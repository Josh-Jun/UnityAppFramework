/* *
 * ===============================================
 * author      : Josh@macbook
 * e-mail      : shijun_z@163.com
 * create time : 2025年4月20 18:41
 * function    : 
 * ===============================================
 * */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.Core.Master;
using SQLite4Unity3d;

namespace App.Core.Tools
{
    public class DataBase
    {
        private SQLiteConnection _connection;

        public DataBase(string dbName)
        {
            if (!Directory.Exists(PlatformMaster.Instance.GetDataPath("DataBase")))
            {
                Directory.CreateDirectory(PlatformMaster.Instance.GetDataPath("DataBase"));
            }
            var dbPath = $"{PlatformMaster.Instance.GetDataPath("DataBase")}/{dbName}";
            #if !UNITY_ANDROID
            dbPath = $"URI=file:{PlatformMaster.Instance.GetDataPath("DataBase")}/{dbName}";
            #endif
            _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
        }

        public void Close()
        {
            _connection.Close();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public int CreateTable<T>()
        {
            return _connection.CreateTable<T>();
        }

        public int DropTable<T>()
        {
            return _connection.DropTable<T>();
        }

        public int ExistTable<T>()
        {
            var mapping = _connection.GetMapping<T>();
            var query = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{mapping.TableName}';";
            return _connection.ExecuteScalar<int>(query);
        }

        public int Insert<T>(T entity)
        {
            return _connection.Insert(entity);
        }

        public int InsertAll(System.Collections.IEnumerable entities)
        {
            return _connection.InsertAll(entities);
        }

        public int Delete<T>(T entity)
        {
            return _connection.Delete(entity);
        }

        public int DeleteAll<T>()
        {
            return _connection.DeleteAll<T>();
        }

        public T Get<T>(object primaryKey) where T : new()
        {
            return _connection.Get<T>(primaryKey);
        }

        public List<T> GetAll<T>() where T : new()
        {
            return _connection.Table<T>().ToList();
        }

        public int Update<T>(T entity)
        {
            return _connection.Update(entity);
        }

        public int UpdateAll(System.Collections.IEnumerable entities)
        {
            return _connection.UpdateAll(entities);
        }
    }
}
