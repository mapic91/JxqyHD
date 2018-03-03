using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Script
{
    public static class ScriptManager
    {
        private class ParallelScriptItem
        {
            public string FilePath;
            public float WaitMilliseconds;
            public ScriptRunner ScriptInRun;

            public ParallelScriptItem(string filePath, int waitMilliseconds)
            {
                FilePath = filePath;
                WaitMilliseconds = waitMilliseconds;
                ScriptInRun = null;
            }

            public void CheckCreateScriptRunner()
            {
                if (ScriptInRun == null)
                {
                    ScriptInRun = new ScriptRunner(Utils.GetScriptParserFromPath(FilePath));
                    ScriptInRun.Begin();
                }
            }
        }

        private static LinkedList<ScriptRunner> _list = new LinkedList<ScriptRunner>();
        private static LinkedList<ParallelScriptItem> _parallelListDelayed = new LinkedList<ParallelScriptItem>();
        private static LinkedList<ParallelScriptItem> _parallelListImmediately = new LinkedList<ParallelScriptItem>();
        private static string _lastFilePath = "";

        public static bool IsInRunningScript
        {
            get { return _list.Count > 0; }
        }

        private static void LogScriptFilePath(ScriptRunner runner)
        {
            if (runner.TargetScript.FilePath != _lastFilePath)
            {
                Globals.TheMessageSender.SendFunctionCallMessage(Environment.NewLine +
                                                                 "【" +
                                                                 runner.TargetScript.FilePath + "】" +
                                                                 (runner.TargetScript.IsOk ? "" : "  读取失败"));
                Globals.TheMessageSender.SendScriptFileChangeMessage(runner.TargetScript.FilePath);
            }
            _lastFilePath = runner.TargetScript.FilePath;
        }

        public static ScriptRunner RunScript(ScriptParser scriptParser, object belongObject = null)
        {
            ScriptRunner runner = null;
            if (scriptParser != null)
            {
                runner = new ScriptRunner(scriptParser, belongObject);
                runner.Begin();
                _list.AddLast(runner);
            }
            return runner;
        }

        public static void RunParallelScript(string scriptFilePath, int delayMilliseconds)
        {
            var item = new ParallelScriptItem(scriptFilePath, delayMilliseconds);
            if (delayMilliseconds <= 0)
            {
                _parallelListImmediately.AddLast(item);
            }
            else
            {
                _parallelListDelayed.AddLast(item);
            }
        }

        public static void SaveParallelScript(KeyDataCollection keyDataCollection)
        {
            var i = 0;
            foreach (var parallelScriptItem in _parallelListDelayed)
            {
                keyDataCollection.AddKey(i.ToString(),
                    parallelScriptItem.FilePath + ":" + (int)parallelScriptItem.WaitMilliseconds);
                i++;
            }

            foreach (var parallelScriptItem in _parallelListImmediately)
            {
                keyDataCollection.AddKey(i.ToString(),
                    parallelScriptItem.FilePath + ":0");
                i++;
            }
        }

        public static void LoadParallelScript(KeyDataCollection keyDataCollection)
        {
            _parallelListDelayed.Clear();
            _parallelListImmediately.Clear();

            if (keyDataCollection == null) return;
            foreach (var keys in keyDataCollection)
            {
                var infos = keys.Value.Split(':');
                var delay = int.Parse(infos[1]);
                var item = new ParallelScriptItem(infos[0], delay);
                if (delay == 0)
                {
                    _parallelListImmediately.AddLast(item);
                }
                else
                {
                    _parallelListDelayed.AddLast(item);
                }
            }
        }

        public static void Clear()
        {
            _list.Clear();
            ClearParallelScript();
        }

        public static void ClearParallelScript()
        {
            _parallelListImmediately.Clear();
            _parallelListDelayed.Clear();
        }

        public static void Update(GameTime gameTime)
        {
            //To avoid Sleep() script function end early, update ScriptExecuter first.
            ScriptExecuter.Update(gameTime);

            for (var node = _list.First; node != null; )
            {
                var runner = node.Value;
                LogScriptFilePath(runner);

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

            //New item may added when script run, count items added before this frame.
            var itemSum = _parallelListDelayed.Count;
            var count = 0; 

            for (var node = _parallelListDelayed.First; node != null && count < itemSum; count++)
            {
                var item = node.Value;
                if (item.WaitMilliseconds > 0)
                {
                    item.WaitMilliseconds -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }

                if (item.WaitMilliseconds <= 0)
                {
                    item.CheckCreateScriptRunner();
                }

                if (item.ScriptInRun != null)
                {
                    LogScriptFilePath(item.ScriptInRun);
                    if (!item.ScriptInRun.Continue())
                    {
                        _parallelListDelayed.Remove(node);
                    }
                }

                node = node.Next;
            }

            for (var node = _parallelListImmediately.First; node != null;)
            {
                var item = node.Value;
                item.CheckCreateScriptRunner();

                LogScriptFilePath(item.ScriptInRun);
                var continueRun = item.ScriptInRun.Continue();
                var next = node.Next; //Get next here, new item may added after script run.
                if (!continueRun)
                {
                    _parallelListImmediately.Remove(node);
                }
                node = next;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}