using System;
using System.IO;

namespace Engine
{
    public static class Log
    {
        private const string LogFilename = "Log.txt";
        public static bool DebugOn;

        public static void LogMessageToFile(string msg)
        {
            if(!DebugOn) return;
            msg = string.Format("{0:G}: {1}{2}", DateTime.Now, msg, Environment.NewLine);
            File.AppendAllText(LogFilename, msg);
        }

        public static void LogFileLoadError(string msg, string filePath, Exception exception)
        {
            LogMessageToFile(msg + " [" + filePath + "] load error: " + exception);
        }
    }
}
