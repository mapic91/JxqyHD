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
                if (!Globals.CacheScriptFile)
                {
                    scriptParser = new ScriptParser(scriptParser.FilePath,
                        scriptParser.BelongObject);
                }
                scriptParser.Begin();
                _list.AddLast(scriptParser);
            }
            return scriptParser;
        }

        public static void Update(GameTime gameTime)
        {
            for (var node = _list.First; node != null; )
            {
                var next = node.Next;
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
                    _list.Remove(node);
                    node = next;
                }
                else
                {
                    //Can run one script only
                    break;
                }

            }
            ScriptExecuter.Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}