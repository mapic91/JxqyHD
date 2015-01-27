using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;

namespace XNAConverter
{
    public class BuildEngine : IBuildEngine
    {
        string logFile = "";
        StreamWriter logWriter;
        List<string> errors;
        public bool log;

        public BuildEngine()
        {
            errors = new List<string>();
            log = true;
        }

        public BuildEngine(string logFile)
        {
            this.logFile = logFile;
            log = true;
            try
            {
                logWriter = new StreamWriter(logFile, true);
            }
            catch { log = false; }
        }

        public void Begin()
        {
            if (log)
            {
                errors = new List<string>();
            }
        }

        private void Log(string message)
        {
            if (log)
            {
                try
                {
                    logWriter.WriteLine(message);
                }
                catch { }
            }
        }

        public void End()
        {
            if (log)
            {
                try
                {
                    logWriter.Flush();
                    logWriter.Close();
                }
                catch { }
            }
        }

        /// <summary>
        /// Returns a list of the errors recorded while processing files.
        /// </summary>
        public List<string> GetErrors()
        {
            return errors;
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs)
        {
            //We don't need this, but we need it to be defined.
            return true;
        }

        public int ColumnNumberOfTaskNode
        {
            //We don't need this, but we need it to be defined.
            get { return 0; }
        }

        public bool ContinueOnError
        {
            //We don't need this, but we need it to be defined.
            get { return true; }
        }

        public int LineNumberOfTaskNode
        {
            //We don't need this, but we need it to be defined.
            get { return 0; }
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            if (log)
            {
                Log("Custom Event at " + DateTime.Now.ToString() + ": " + e.Message);
            }
        }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            if (log)
            {
                Log("Error at " + DateTime.Now.ToString() + ": " + e.Message);
            }
            errors.Add(DateTime.Now.ToString() + ": " + e.Message);
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            if (log)
            {
                Log("Message at " + DateTime.Now.ToString() + ": " + e.Message);
            }
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            if (log)
            {
                Log("Warning at " + DateTime.Now.ToString() + ": " + e.Message);
            }
        }

        public string ProjectFileOfTaskNode
        {
            //We don't need this, but we need it to be defined.
            get { return string.Empty; }
        }
    }
}
