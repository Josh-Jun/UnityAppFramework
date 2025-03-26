/* *
 * ===============================================
 * author      : Josh@macbook
 * e-mail      : shijun_z@163.com
 * create time : 2025年3月25 10:3
 * function    : 
 * ===============================================
 * */

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using App.Core.Tools;
using UnityEngine;

namespace App.Core.Master
{
    public class DebugMaster : SingletonMono<DebugMaster>
    {
        private StringBuilder LogBuilder;
        private StringBuilder CacheLogBuilder;
        private ObjectPool<StringBuilder> StringBuilderPool;
        private StreamWriter logWriter;
        private string LogFilePath;
        
        private const int MaxLength = 10000;
        private const int MaxFileCount = 10;
        
        private const string Pattern = @"<([a-zA-Z/][^>]*)>";
        
        private void Awake()
        {
            LogFilePath = PlatformMaster.Instance.GetDataPath("Logs");
            StringBuilderPool = new ObjectPool<StringBuilder>(() => new StringBuilder());
            LogBuilder = StringBuilderPool.Get();
            Application.logMessageReceived += HandleLogMsg;
        }

        private void HandleLogMsg(string msg, string stacktrace, LogType type)
        {
            LogBuilder.AppendLine($"{TimeNow} {GetLogType(type)} {Regex.Replace(msg, Pattern, string.Empty)}");
            if (type == LogType.Exception || type == LogType.Assert || type == LogType.Error)
            {
                LogBuilder.Append(stacktrace);
            }
            CheckLog();
        }
        
        private void CheckLog()
        {
            if (LogBuilder.Length > MaxLength)
            {
                CacheLogBuilder = StringBuilderPool.Get();
                CacheLogBuilder.Append(LogBuilder.ToString());
                StringBuilderPool.GiveBack(LogBuilder, sb => sb.Length = 0);
                LogBuilder = StringBuilderPool.Get();
                WriteLogToFile(CacheLogBuilder);
            }
        }

        private void WriteLogToFile(StringBuilder builder)
        {
            if (builder.Length == 0) return;
            if (!Directory.Exists(LogFilePath))
            {
                Directory.CreateDirectory(LogFilePath);
            }

            ClearLogFile(LogFilePath);

            if (logWriter == null)
            {
                var filename = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log";
                var filepath = $"{LogFilePath}/{filename}";
                logWriter = File.AppendText(filepath);
                logWriter.AutoFlush = true;
                logWriter.Write(builder.ToString());
            }
            else
            {
                logWriter.Write(builder.ToString());
            }
            StringBuilderPool.GiveBack(builder, sb => sb.Length = 0);
        }

        private void ClearLogFile(string logFilePath)
        {
            var direction = new DirectoryInfo(logFilePath);
            var files = direction.GetFiles("*", SearchOption.AllDirectories);
            if (files.Length >= MaxFileCount)
            {
                var file = files.OrderBy(f => f.CreationTime).First();
                FileTools.DeleteFile(file.FullName);
            }
        }

        private string TimeNow => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]";

        private string GetLogType(LogType type)
        {
            var logType = string.Empty;
            switch (type)
            {
                case LogType.Log:
                    logType = "[I]";
                    break;
                case LogType.Warning:
                    logType = "[W]";
                    break;
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    logType = "[E]";
                    break;
            }
            return logType;
        }

        private void CloseLog()
        {
            if (logWriter != null)
            {
                try
                {
                    logWriter.Flush();
                    logWriter.Close();
                    logWriter.Dispose();
                    logWriter = null;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private void OnApplicationQuit()
        {
            Application.logMessageReceived -= HandleLogMsg;
            WriteLogToFile(LogBuilder);
            CloseLog();
        }
    }
}
