using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Engine.Gui;

namespace Engine.Script
{
    public class ScriptParser
    {
        private List<Code> _codes;
        private int _currentIndex;
        private Code _currentCode;
        public string FilePath { private set; get; }
        public bool IsOk { private set; get; }
        public object BelongObject { private set; get; }
        public bool IsEnd { private set; get; }

        public ScriptParser() { }

        public ScriptParser(string filePath, object belongObject)
        {
            ReadFile(filePath, belongObject);
        }

        private static readonly Regex RegGoto = new Regex(@"^@([a-zA-Z0-9]+):");
        private static readonly Regex RegComment = new Regex(@"^//.*");
        private static readonly Regex RegFunction = new Regex(@"^([a-zA-Z]+)(.*);");
        private static readonly Regex RegParameter = new Regex(@"^\((.+)\)(.*)");
        private static readonly Regex RegResult = new Regex(@"^@[a-zA-Z0-9]+");
        private void ParserLine(string line)
        {
            var code = new Code();
            line = line.Trim();
            if (line.Length < 2) return;

            if (RegGoto.IsMatch(line))
            {
                var match = RegGoto.Match(line);
                code.IsGoto = true;
                code.Name = match.Value;
            }
            else if (RegComment.IsMatch(line)) 
            {
                return;
            }
            else if (RegFunction.IsMatch(line))
            {
                var matchFunction = RegFunction.Match(line);
                code.Name = matchFunction.Groups[1].Value;
                var matchParmeter = RegParameter.Match(matchFunction.Groups[2].Value.Trim());
                if (matchParmeter.Success)
                {
                    code.Parameters = ParserParameter(matchParmeter.Groups[1].Value);
                }
                var matchResult = RegResult.Match(matchParmeter.Success
                    ? matchParmeter.Groups[2].Value.Trim()
                    : matchFunction.Groups[2].Value.Trim());
                if (matchResult.Success)
                {
                    code.Result = matchResult.Value;
                }
            }

            code.Literal = line;
            _codes.Add(code);
        }

        private List<string> ParserParameter(string str)
        {
            str = str.Trim();
            if (str.Length == 0) return null;
            var parameters = new List<string>();
            var temp = new StringBuilder();
            for (var i = 0; i < str.Length;i++)
            {
                if (str[i] == '"')
                {
                    temp.Append(str[i]);
                    while (str[++i] != '"')
                    {
                        temp.Append(str[i]);
                    }
                    temp.Append(str[i]);
                    parameters.Add(temp.ToString());
                    temp.Clear();
                }
                else if(!char.IsWhiteSpace(str[i]))
                {
                    if (str[i] == ',')
                    {
                        if (temp.Length != 0)
                        {
                            parameters.Add(temp.ToString());
                            temp.Clear();
                        }
                    }
                    else
                    {
                        temp.Append(str[i]);
                    }
                }
            }
            if (temp.Length != 0)
            {
                parameters.Add(temp.ToString());
                temp.Clear();
            }
            return parameters;
        }

        public bool ReadFile(string filePath, object belongObject)
        {
            BelongObject = belongObject;
            IsOk = false;
            FilePath = filePath;
            try
            {
                IsOk = ReadFromLines(File.ReadAllLines(filePath, Globals.SimpleChinaeseEncoding));
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Script", filePath, exception);
                return false;
            }
            return IsOk;
        }

        public bool ReadFromLines(string[] lines)
        {
            _codes = new List<Code>(lines.Count());
            foreach (var line in lines)
            {
                ParserLine(line);
            }
            return true;
        }

        public void Begin()
        {
            _currentIndex = 0;
            _currentCode = null;
        }

        public bool Continue()
        {
            var count = _codes.Count;
            for (; _currentIndex < count; _currentIndex++)
            {
                string gotoPosition;
                bool scriptEnd;
                if (!RunCode(_codes[_currentIndex], out gotoPosition, out scriptEnd)) break;
                if (scriptEnd)
                {
                    IsEnd = true;
                    _currentIndex = count;
                    return false;
                }
                if (!string.IsNullOrEmpty(gotoPosition))
                {
                    gotoPosition += ":";
                    _currentIndex++;
                    while (_currentIndex < count)
                    {
                        if (_codes[_currentIndex].IsGoto &&
                            _codes[_currentIndex].Name == gotoPosition)
                        {
                            break;
                        }
                        else _currentIndex++;
                    }
                }
            }
            IsEnd = (_currentIndex == count);
            return !IsEnd;
        }

        public class Code
        {
            public string Name;
            public List<string> Parameters;
            public string Result;
            public string Literal;
            public bool IsGoto;
        }

        private enum State
        {
            Normal,
            Comment,
            Function,
            Parmeter,
            Goto,
            Result
        }

        #region Code run
        /// <summary>
        /// RunCode
        /// </summary>
        /// <param name="code">Code to run</param>
        /// <param name="gotoPosition"></param>
        /// <param name="scriptEnd">Script is returned if true</param>
        /// <returns>True if code runed, false if code is running</returns>
        private bool RunCode(Code code, out string gotoPosition, out bool scriptEnd)
        {
            gotoPosition = null;
            scriptEnd = false;
            if (code == null ||
                code.IsGoto ||
                string.IsNullOrEmpty(code.Name)) return true;

            var isEnd = true;
            try
            {
                if (_currentCode != null)
                {
                    switch (_currentCode.Name)
                    {
                        case "Say":
                            isEnd = GuiManager.IsDialogEnd();
                            break;
                        case "FadeOut":
                            isEnd = ScriptExecuter.IsFadeOutEnd();
                            break;
                        case "FadeIn":
                            isEnd = ScriptExecuter.IsFadeInEnd();
                            break;
                        case "Talk":
                            isEnd = !ScriptExecuter.IsInTalk;
                            break;
                        case "Choose":
                            isEnd = ScriptExecuter.IsChooseEnd(_currentCode.Parameters);
                            break;
                    }
                }
                else
                {
                    _currentCode = code;
                    var parameters = _currentCode.Parameters;
                    switch (_currentCode.Name)
                    {
                        case "Say":
                            ScriptExecuter.Say(parameters);
                            isEnd = GuiManager.IsDialogEnd();
                            break;
                        case "If":
                            if (ScriptExecuter.If(parameters))
                                if (_currentCode.Result == "Return")
                                    scriptEnd = true;
                                else gotoPosition = _currentCode.Result;
                            break;
                        case "Return":
                            scriptEnd = true;
                            break;
                        case "Goto":
                            gotoPosition = _currentCode.Parameters != null
                                ? _currentCode.Parameters[0]
                                : _currentCode.Result;
                            break;
                        case "Add":
                            ScriptExecuter.Add(parameters);
                            break;
                        case "Assign":
                            ScriptExecuter.Assign(parameters);
                            break;
                        case "FadeOut":
                            ScriptExecuter.FadeOut();
                            isEnd = ScriptExecuter.IsFadeOutEnd();
                            break;
                        case "FadeIn":
                            ScriptExecuter.FadeIn();
                            isEnd = ScriptExecuter.IsFadeInEnd();
                            break;
                        case "DelNpc":
                            ScriptExecuter.DelNpc(parameters);
                            break;
                        case "ClearBody":
                            ScriptExecuter.ClearBody();
                            break;
                        case "StopMusic":
                            ScriptExecuter.StopMusic();
                            break;
                        case "PlayMusic":
                            ScriptExecuter.PlayMusic(parameters);
                            break;
                        case "PlaySound":
                            ScriptExecuter.PlaySound(parameters, BelongObject);
                            break;
                        case "OpenBox":
                            ScriptExecuter.OpenBox(parameters, BelongObject);
                            break;
                        case "OpenObj":
                            ScriptExecuter.OpenBox(parameters, BelongObject);
                            break;
                        case "SetObjScript":
                            ScriptExecuter.SetObjScript(parameters, BelongObject);
                            break;
                        case "SetNpcScript":
                            ScriptExecuter.SetNpcScript(parameters, BelongObject);
                            break;
                        case "SetNpcDeathScript":
                            ScriptExecuter.SetNpcDeathScript(parameters, BelongObject);
                            break;
                        case "SetNpcLevel":
                            ScriptExecuter.SetNpcLevel(parameters, BelongObject);
                            break;
                        case "SetLevelFile":
                            ScriptExecuter.SetLevelFile(parameters, BelongObject);
                            break;
                        case "AddRandMoney":
                            ScriptExecuter.AddRandMoney(parameters);
                            break;
                        case "AddLife":
                            ScriptExecuter.AddLife(parameters);
                            break;
                        case "AddThew":
                            ScriptExecuter.AddThew(parameters);
                            break;
                        case "AddMana":
                            ScriptExecuter.AddMana(parameters);
                            break;
                        case "AddExp":
                            ScriptExecuter.AddExp(parameters);
                            break;
                        case "SetPlayerPos":
                            ScriptExecuter.SetPlayerPos(parameters);
                            break;
                        case "SetPlayerDir":
                            ScriptExecuter.SetPlayerDir(parameters);
                            break;
                        case "LoadMap":
                            ScriptExecuter.LoadMap(parameters);
                            break;
                        case "LoadNpc":
                            ScriptExecuter.LoadNpc(parameters);
                            break;
                        case "LoadObj":
                            ScriptExecuter.LoadObj(parameters);
                            break;
                        case "AddNpc":
                            ScriptExecuter.AddNpc(parameters);
                            break;
                        case "AddObj":
                            ScriptExecuter.AddObj(parameters);
                            break;
                        case "AddGoods":
                            ScriptExecuter.AddGoods(parameters);
                            break;
                        case "AddRandGoods":
                            ScriptExecuter.AddRandGoods(parameters);
                            break;
                        case "AddMagic":
                            ScriptExecuter.AddMagic(parameters);
                            break;
                        case "AddMoney":
                            ScriptExecuter.AddMoney(parameters);
                            break;
                        case "Talk":
                            ScriptExecuter.Talk(parameters);
                            isEnd = !ScriptExecuter.IsInTalk;
                            break;
                        case "Memo":
                            ScriptExecuter.Memo(parameters);
                            break;
                        case "AddToMemo":
                            ScriptExecuter.AddToMemo(parameters);
                            break;
                        case "DelGoods":
                            ScriptExecuter.DelGoods(parameters, BelongObject);
                            break;
                        case "DelCurObj":
                            ScriptExecuter.DelCurObj(BelongObject);
                            break;
                        case "DelObj":
                            ScriptExecuter.DelObj(parameters, BelongObject);
                            break;
                        case "FreeMap":
                            ScriptExecuter.FreeMap();
                            break;
                        case "SetTrap":
                            ScriptExecuter.SetTrap(parameters);
                            break;
                        case "SetMapTrap":
                            ScriptExecuter.SetMapTrap(parameters);
                            break;
                        case "FullLife":
                            ScriptExecuter.FullLife();
                            break;
                        case "FullMana":
                            ScriptExecuter.FullMana();
                            break;
                        case "FullThew":
                            ScriptExecuter.FullThew();
                            break;
                        case "ShowNpc":
                            ScriptExecuter.ShowNpc(parameters);
                            break;
                        case "Sleep":
                            ScriptExecuter.Sleep(parameters);
                            break;
                        case "ShowMessage":
                            ScriptExecuter.ShowMessage(parameters);
                            break;
                        case "SetMagicLevel":
                            ScriptExecuter.SetMagicLevel(parameters);
                            break;
                        case "ShowSnow":
                            ScriptExecuter.ShowSnow(parameters);
                            break;
                        case "BeginRain":
                            ScriptExecuter.BeginRain(parameters);
                            break;
                        case "EndRain":
                            ScriptExecuter.EndRain();
                            break;
                        case "ChangeMapColor":
                            ScriptExecuter.ChangeMapColor(parameters);
                            break;
                        case "ChangeAsfColor":
                            ScriptExecuter.ChangeAsfColor(parameters);
                            break;
                        case "Choose":
                            ScriptExecuter.Choose(parameters);
                            isEnd = false;
                            break;
                        case "RunScript":
                            ScriptExecuter.RunScript(parameters, BelongObject);
                            break;
                    }
                }
            }
            catch (Exception)
            {
                var message = "Script error! File: " + Path.GetFullPath(FilePath) +".";
                if (_currentCode != null)
                {
                    message += ("Code: " + _currentCode.Literal);
                }
                Log.LogMessageToFile(message);
            }

            if (isEnd)
            {
                _currentCode = null;
            }
            return isEnd;
        }
        #endregion Code run
    }
}