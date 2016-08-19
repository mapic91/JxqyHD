using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Script
{
    public static class ScriptManager
    {
        private static LinkedList<ScriptRunner> _list = new LinkedList<ScriptRunner>();
        private static string _lastFilePath = "";

        public static bool IsInRunningScript
        {
            get { return _list.Count > 0; }
        }

        public static ScriptRunner RunScript(ScriptParser scriptParser)
        {
            ScriptRunner runner = null;
            if (scriptParser != null)
            {
                runner = new ScriptRunner(scriptParser);
                runner.Begin();
                _list.AddLast(runner);
            }
            return runner;
        }

        public static void Clear()
        {
            _list.Clear();
        }

        public static void Update(GameTime gameTime)
        {
            //To avoid Sleep() script function end early, update ScriptExecuter first.
            ScriptExecuter.Update(gameTime);

            for (var node = _list.First; node != null; )
            {
                var runner = node.Value;
                if (runner.TargetScript.FilePath != _lastFilePath)
                {
                    Globals.TheMessageSender.SendFunctionCallMessage(Environment.NewLine + 
                        "【" +
                        runner.TargetScript.FilePath + "】" +
                        (runner.TargetScript.IsOk ? "" : "  读取失败"));
                    Globals.TheMessageSender.SendScriptFileChangeMessage(runner.TargetScript.FilePath);
                }
                _lastFilePath = runner.TargetScript.FilePath;

                if (!runner.Continue())
                {
                    //Remove current
                    _list.Remove(node);
                    //Get next
                    node = _list.First;
                }
                else
                {
                    //Can't run multiple script at same time
                    break;
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}