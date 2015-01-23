using System;
using System.IO;
using System.Linq;

namespace Engine
{
    public static class Log
    {
        private const string LogFilename = "Log.txt";
        private static bool LogOn = false;

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
                    op = "保存";
                    break;
                case FileOpration.Load:
                    op = "读取";
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
                " 错误: \n" +
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
            //File.Create(LogFilename).Dispose();
        }

        public static void LogMessageToFile(string msg)
        {
            if(!LogOn && !Globals.TheGame.IsInEditMode) return;
            msg = string.Format("{0:G}: {1}{2}", DateTime.Now, msg, Environment.NewLine);
            Globals.TheMessageSender.SendLogMessage(msg);
            if (LogOn)
            {
                File.AppendAllText(LogFilename, msg);
            }
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
