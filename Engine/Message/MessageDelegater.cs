using System;

namespace Engine.Message
{
    public class MessageDelegater
    {
        public event Action<string> OnFunctionCall;
        public event Action<string> OnScriptFileChange; 

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
    }
}