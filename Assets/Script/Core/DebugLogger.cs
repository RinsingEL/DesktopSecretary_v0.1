using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Core.Framework.Utility
{
    public class DebugLogger : MonoBehaviour
    {
        private static DebugLogger _instance;
        public static DebugLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("DebugLogger");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<DebugLogger>();
                }
                return _instance;
            }
        }

        private List<LogEntry> logEntries = new List<LogEntry>();
        public string logFilePath;

        private void Awake()
        {
            logFilePath = Path.Combine(Application.persistentDataPath, "DebugLog.txt");
            Application.logMessageReceived += HandleLog;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                var entry = new LogEntry
                {
                    Message = condition,
                    StackTrace = ParseStackTrace(stackTrace),
                    Timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                logEntries.Add(entry);
                SaveToFile(entry); // 实时保存
            }
        }

        private List<StackFrameInfo> ParseStackTrace(string stackTrace)
        {
            List<StackFrameInfo> frames = new List<StackFrameInfo>();
            string[] lines = stackTrace.Split('\n');

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // 示例堆栈行：at MyClass.MyMethod() in C:\path\to\file.cs:line 42
                string cleanedLine = line.Trim();
                int fileIndex = cleanedLine.IndexOf("in ");
                int lineIndex = cleanedLine.LastIndexOf(":line ");

                if (fileIndex >= 0 && lineIndex >= 0)
                {
                    string method = cleanedLine.Substring(0, fileIndex).Trim();
                    string filePath = cleanedLine.Substring(fileIndex + 3, lineIndex - (fileIndex + 3)).Trim();
                    string lineNumber = cleanedLine.Substring(lineIndex + 6).Trim();

                    frames.Add(new StackFrameInfo
                    {
                        Method = method,
                        FilePath = filePath,
                        LineNumber = lineNumber
                    });
                }
                else
                {
                    frames.Add(new StackFrameInfo { Method = cleanedLine });
                }
            }
            return frames;
        }

        private void SaveToFile(LogEntry entry)
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"[{entry.Timestamp}] {entry.Message}");
                foreach (var frame in entry.StackTrace)
                {
                    writer.WriteLine($"  {frame.Method} in {frame.FilePath}:line {frame.LineNumber}");
                }
                writer.WriteLine();
            }
        }

        public List<LogEntry> GetLogEntries() => logEntries;
    }

    [System.Serializable]
    public class LogEntry
    {
        public string Message;
        public List<StackFrameInfo> StackTrace;
        public string Timestamp;
    }

    [System.Serializable]
    public class StackFrameInfo
    {
        public string Method;
        public string FilePath = "Unknown";
        public string LineNumber = "Unknown";
    }
}