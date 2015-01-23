using System;
using System.Collections.Generic;

namespace Engine.Message
{
    public class MessageDelegater
    {
        public event Action<string> OnFunctionCall;
        public event Action<string> OnScriptFileChange;
        public event Action<Dictionary<string, int>> OnScriptVariables;
        public event Action<string> OnLog;

        public void SendFunctionCallMessage(string functionDetail)
        {
            if (OnFunctionCall != null)
            {
                OnFunctionCall(functionDetail);
            }
        }

        public void SendScriptFileChangeMessage(string filePath)
        {
            if (OnScriptFileChange != null)
            {
                OnScriptFileChange(filePath);
            }
        }

        public void SendScriptVariablesMessage(Dictionary<string, int> variables)
        {
            if (OnScriptVariables != null)
            {
                OnScriptVariables(variables);
            }
        }

        public void SendLogMessage(string message)
        {
            if (OnLog != null)
            {
                OnLog(message);
            }
        }
    }
}