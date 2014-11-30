using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Script
{
    public static class ScriptManager
    {
        private static LinkedList<ScriptParser> _list = new LinkedList<ScriptParser>();

        public static void RunScript(ScriptParser scriptParser)
        {
            if (scriptParser != null && scriptParser.IsOk)
            {
                if (!Globals.CacheScriptFile)
                {
                    scriptParser = new ScriptParser(scriptParser.FilePath, 
                        scriptParser.BelongObject);
                }
                scriptParser.Begin();
                _list.AddLast(scriptParser);
            }
        }

        public static void Update(GameTime gameTime)
        {
            for (var node = _list.First; node != null;)
            {
                var next = node.Next;
                var script = node.Value;
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