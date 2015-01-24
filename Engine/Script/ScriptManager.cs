using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Script
{
    public static class ScriptManager
    {
        private static LinkedList<ScriptParser> _list = new LinkedList<ScriptParser>();
        private static string _lastFilePath = "";

        public static ScriptParser RunScript(ScriptParser scriptParser)
        {
            if (scriptParser != null)
            {
                scriptParser.Begin();
                _list.AddLast(scriptParser);
            }
            return scriptParser;
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
                var script = node.Value;
                if (script.FilePath != _lastFilePath)
                {
                    Globals.TheMessageSender.SendFunctionCallMessage(Environment.NewLine + 
                        "【" +
                        script.FilePath + "】" +
                        (script.IsOk ? "" : "  读取失败"));
                    Globals.TheMessageSender.SendScriptFileChangeMessage(script.FilePath);
                }
                _lastFilePath = script.FilePath;

                if (!script.Continue())
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