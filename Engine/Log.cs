using System;
using System.IO;
using System.Linq;

namespace Engine
{
    public static class Log
    {
        private const string LogFilename = "Log.txt";
        public static bool LogOn;

        private static string GetLastLine(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            string[] lines = text.Replace("\r", "").Split('\n');
            return lines.Length > 0 ? lines.Last() : "";
        }

        public static void Initialize()
        {
            File.Create(LogFilename).Dispose();
        }

        public static void LogMessageToFile(string msg)
        {
            if(!LogOn) return;
            msg = string.Format("{0:G}: {1}{2}", DateTime.Now, msg, Environment.NewLine);
            File.AppendAllText(LogFilename, msg);
        }

        public static void LogFileLoadError(string msg, string filePath, Exception exception)
        {
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(filePath);
            }
            catch (Exception)
            {
                fullPath = filePath;
            }
            LogMessageToFile(msg + 
                " [" +
                fullPath + 
                "] load error: \n" + 
                exception.Message + 
                "\n" +
                GetLastLine(exception.StackTrace));
        }
    }
}
