using System;
using System.IO;
using System.Linq;

namespace Engine
{
    public static class Log
    {
        private const string LogFilename = "Log.txt";
        public static bool LogOn;

        private enum FileOpration
        {
            Save, 
            Load
        }

        private static void LogFileOperationError(FileOpration opration,
            string msg,
            string filePath,
            Exception exception)
        {
            string op = null;
            switch (opration)
            {
                case FileOpration.Save:
                    op = "save";
                    break;
                case FileOpration.Load:
                    op = "load";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("opration");
            }
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
                "] " + 
                op + 
                " error: \n" +
                exception.Message +
                "\n" +
                GetLastLine(exception));
        }

        public static string GetLastLine(Exception exception)
        {
            if (exception == null) return "";
            var text = exception.StackTrace;
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
            LogFileOperationError(FileOpration.Load, msg, filePath, exception);
        }

        public static void LogFileSaveError(string msg, string filePath, Exception exception)
        {
            LogFileOperationError(FileOpration.Save, msg, filePath, exception);
        }
    }
}
