using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Engine.Gui;
using Engine.Storage;
using Microsoft.Xna.Framework.GamerServices;

namespace Engine.Script
{
    public class ScriptParser
    {
        private List<Code> _codes;
        private int _currentIndex;
        private Code _currentCode;
        private bool _isEnd = true;
        private int _lineNumber;
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public bool IsOk { private set; get; }
        public object BelongObject { private set; get; }

        public bool IsEnd
        {
            get { return _isEnd; }
            private set { _isEnd = value; }
        }

        public ScriptParser() { }

        public ScriptParser(string filePath, object belongObject)
        {
            ReadFile(filePath, belongObject);
        }

        private static readonly Regex RegGoto = new Regex(@"^@([a-zA-Z0-9]+):");
        private static readonly Regex RegComment = new Regex(@"^//.*");
        private static readonly Regex RegFunction = new Regex(@"^([a-zA-Z]+)(.*);*");
        private static readonly Regex RegParameter = new Regex(@"^\((.+)\)(.*)");
        private static readonly Regex RegResult = new Regex(@"^@[a-zA-Z0-9]+");
        private void ParserLine(string line)
        {
            var code = new Code { LineNumber = _lineNumber };
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
            var parameters = new List<string>();
            if (str.Length == 0) return parameters;
            var temp = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
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
                else if (!char.IsWhiteSpace(str[i]))
                {
                    if (str[i] == ',' || str[i] == '，')
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
                FileName = Path.GetFileName(filePath);
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
            _lineNumber = 1;
            _codes = new List<Code>(lines.Count());
            foreach (var line in lines)
            {
                ParserLine(line);
                _lineNumber++;
            }
            return true;
        }

        public void Begin()
        {
            IsEnd = !IsOk;
            _currentIndex = 0;
            _currentCode = null;
        }

        public bool Continue()
        {
            if (IsEnd) return false;
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
            public List<string> Parameters = new List<string>();
            public string Result;
            public string Literal;
            public int LineNumber;
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
                            isEnd = ScriptExecuter.IsSayEnd();
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
                        case "Select":
                            isEnd = ScriptExecuter.IsChooseEnd(_currentCode.Parameters);
                            break;
                        case "PlayMovie":
                            isEnd = !ScriptExecuter.IsInPlayingMovie;
                            break;
                        case "PlayerGoto":
                            isEnd = ScriptExecuter.IsPlayerGotoEnd();
                            break;
                        case "PlayerGotoDir":
                            isEnd = ScriptExecuter.IsPlayerGotoDirEnd();
                            break;
                        case "PlayerJumpTo":
                            isEnd = ScriptExecuter.IsPlayerJumpToEnd();
                            break;
                        case "PlayerRunTo":
                            isEnd = ScriptExecuter.IsPlayerRunToEnd();
                            break;
                        case "NpcGoto":
                            isEnd = ScriptExecuter.IsNpcGotoEnd(_currentCode.Parameters,
                                BelongObject);
                            break;
                        case "NpcGotoDir":
                            isEnd = ScriptExecuter.IsNpcGotoDirEnd(_currentCode.Parameters,
                                BelongObject);
                            break;
                        case "MoveScreen":
                            isEnd = ScriptExecuter.IsMoveScreenEnd();
                            break;
                        case "MoveScreenEx":
                            isEnd = ScriptExecuter.IsMoveScreenExEnd();
                            break;
                        case "SellGoods":
                        case "BuyGoods":
                            isEnd = ScriptExecuter.IsBuyGoodsEnd();
                            break;
                        case "NpcSpecialActionEx":
                            isEnd = ScriptExecuter.IsNpcSpecialActionExEnd(_currentCode.Parameters, BelongObject);
                            break;
                        case "Sleep":
                            isEnd = ScriptExecuter.IsSleepEnd();
                            break;
                    }
                }
                else
                {
                    _currentCode = code;
                    var parameters = _currentCode.Parameters;

                    //Send message to register
                    Globals.TheMessageSender.SendFunctionCallMessage(DateTime.Now.ToString("T") +
                        "\t" + _currentCode.Literal + "\t[" + _currentCode.LineNumber + "]");

                    switch (_currentCode.Name)
                    {
                        case "Say":
                            ScriptExecuter.Say(parameters);
                            isEnd = ScriptExecuter.IsSayEnd();
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
                            gotoPosition = _currentCode.Parameters.Count != 0
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
                        case "CloseBox":
                            ScriptExecuter.CloseBox(parameters, BelongObject);
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
                        case "MergeNpc":
                            ScriptExecuter.MergeNpc(parameters);
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
                        case "SetMapTime":
                            Map.MapTime = int.Parse(parameters[0]);
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
                            isEnd = ScriptExecuter.IsSleepEnd();
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
                        case "Select":
                            ScriptExecuter.Select(parameters);
                            isEnd = false;
                            break;
                        case "RunScript":
                            ScriptExecuter.RunScript(parameters, BelongObject);
                            break;
                        case "PlayMovie":
                            ScriptExecuter.PlayMovie(parameters);
                            isEnd = !ScriptExecuter.IsInPlayingMovie;
                            break;
                        case "SaveMapTrap":
                            ScriptExecuter.SaveMapTrap();
                            break;
                        case "SaveNpc":
                            ScriptExecuter.SaveNpc(parameters);
                            break;
                        case "SaveObj":
                            ScriptExecuter.SaveObj(parameters);
                            break;
                        case "GetRandNum":
                            ScriptExecuter.GetRandNum(parameters);
                            break;
                        case "SetNpcDir":
                            ScriptExecuter.SetNpcDir(parameters, BelongObject);
                            break;
                        case "SetNpcKind":
                            ScriptExecuter.SetNpcKind(parameters, BelongObject);
                            break;
                        case "SetNpcMagicFile":
                            ScriptExecuter.SetNpcMagicFile(parameters, BelongObject);
                            break;
                        case "SetNpcPos":
                            ScriptExecuter.SetNpcPos(parameters, BelongObject);
                            break;
                        case "SetNpcRelation":
                            ScriptExecuter.SetNpcRelation(parameters, BelongObject);
                            break;
                        case "SetNpcRes":
                            ScriptExecuter.SetNpcRes(parameters, BelongObject);
                            break;
                        case "SetNpcAction":
                            ScriptExecuter.SetNpcAction(parameters);
                            break;
                        case "SetNpcActionFile":
                            ScriptExecuter.SetNpcActionFile(parameters);
                            break;
                        case "SetNpcActionType":
                            ScriptExecuter.SetNpcActionType(parameters, BelongObject);
                            break;
                        case "Watch":
                            ScriptExecuter.Watch(parameters);
                            break;
                        case "SetObjOfs":
                            ScriptExecuter.SetObjOfs(parameters, BelongObject);
                            break;
                        case "DisableFight":
                            ScriptExecuter.DisableFight();
                            break;
                        case "EnableFight":
                            ScriptExecuter.EnableFight();
                            break;
                        case "DisableInput":
                            ScriptExecuter.DisableInput();
                            break;
                        case "EnableInput":
                            ScriptExecuter.EnableInput();
                            break;
                        case "DisableJump":
                            ScriptExecuter.DisableJump();
                            break;
                        case "EnableJump":
                            ScriptExecuter.EnableJump();
                            break;
                        case "DisableNpcAI":
                            ScriptExecuter.DisableNpcAI();
                            break;
                        case "EnableNpcAI":
                            ScriptExecuter.EnableNpcAI();
                            break;
                        case "DisableRun":
                            ScriptExecuter.DisableRun();
                            break;
                        case "EnableRun":
                            ScriptExecuter.EnableRun();
                            break;
                        case "SetPlayerState":
                            ScriptExecuter.SetPlayerState(parameters);
                            break;
                        case "OpenWaterEffect":
                            ScriptExecuter.OpenWaterEffect();
                            break;
                        case "CloseWaterEffect":
                            ScriptExecuter.CloseWaterEffect();
                            break;
                        case "PlayerGoto":
                            ScriptExecuter.PlayerGoto(parameters);
                            isEnd = ScriptExecuter.IsPlayerGotoEnd();
                            break;
                        case "PlayerGotoDir":
                            ScriptExecuter.PlayerGotoDir(parameters);
                            isEnd = ScriptExecuter.IsPlayerGotoDirEnd();
                            break;
                        case "PlayerGotoEx":
                            ScriptExecuter.PlayerGotoEx(parameters);
                            break;
                        case "PlayerJumpTo":
                            ScriptExecuter.PlayerJumpTo(parameters);
                            isEnd = ScriptExecuter.IsPlayerJumpToEnd();
                            break;
                        case "PlayerRunTo":
                            ScriptExecuter.PlayerRunTo(parameters);
                            isEnd = ScriptExecuter.IsPlayerRunToEnd();
                            break;
                        case "PlayerRunToEx":
                            ScriptExecuter.PlayerRunToEx(parameters);
                            break;
                        case "NpcGoto":
                            ScriptExecuter.NpcGoto(parameters, BelongObject);
                            isEnd = ScriptExecuter.IsNpcGotoEnd(parameters, BelongObject);
                            break;
                        case "NpcGotoDir":
                            ScriptExecuter.NpcGotoDir(parameters, BelongObject);
                            isEnd = ScriptExecuter.IsNpcGotoDirEnd(parameters, BelongObject);
                            break;
                        case "NpcGotoEx":
                            ScriptExecuter.NpcGotoEx(parameters, BelongObject);
                            break;
                        case "SetMoneyNum":
                            ScriptExecuter.SetMoneyNum(parameters);
                            break;
                        case "GetMoneyNum":
                            ScriptExecuter.GetMoneyNum(parameters);
                            break;
                        case "SetPlayerScn":
                            ScriptExecuter.SetPlayerScn();
                            break;
                        case "MoveScreen":
                            ScriptExecuter.MoveScreen(parameters);
                            isEnd = ScriptExecuter.IsMoveScreenEnd();
                            break;
                        case "MoveScreenEx":
                            ScriptExecuter.MoveScreenEx(parameters);
                            isEnd = ScriptExecuter.IsMoveScreenExEnd();
                            break;
                        case "SetMapPos":
                            ScriptExecuter.SetMapPos(parameters);
                            break;
                        case "EquipGoods":
                            ScriptExecuter.EquipGoods(parameters);
                            break;
                        case "SellGoods":
                        case "BuyGoods":
                            ScriptExecuter.BuyGoods(parameters);
                            isEnd = ScriptExecuter.IsBuyGoodsEnd();
                            break;
                        case "OpenTimeLimit":
                            ScriptExecuter.OpenTimeLimit(parameters);
                            break;
                        case "CloseTimeLimit":
                            ScriptExecuter.CloseTimeLimit();
                            break;
                        case "HideTimerWnd":
                            ScriptExecuter.HideTimerWnd();
                            break;
                        case "SetTimeScript":
                            ScriptExecuter.SetTimeScript(parameters);
                            break;
                        case "GetGoodsNum":
                            ScriptExecuter.GetGoodsNum(parameters);
                            break;
                        case "GetNpcCount":
                            ScriptExecuter.GetNpcCount(parameters);
                            break;
                        case "LimitMana":
                            ScriptExecuter.LimitMana(parameters);
                            break;
                        case "NpcAttack":
                            ScriptExecuter.NpcAttack(parameters, BelongObject);
                            break;
                        case "FollowNpc":
                            ScriptExecuter.FollowNpc(parameters, BelongObject);
                            break;
                        case "NpcSpecialAction":
                            ScriptExecuter.NpcSpecialAction(parameters, BelongObject);
                            break;
                        case "NpcSpecialActionEx":
                            ScriptExecuter.NpcSpecialActionEx(parameters, BelongObject);
                            isEnd = ScriptExecuter.IsNpcSpecialActionExEnd(parameters, BelongObject);
                            break;
                        case "LoadGame":
                            ScriptExecuter.LoadGame(parameters);
                            break;
                        case "PlayerChange":
                            ScriptExecuter.PlayerChange(parameters);
                            break;
                        case "ReturnToTitle":
                            ScriptExecuter.ReturnToTitle();
                            break;
                        default:
                            throw new Exception("无此函数");
                    }
                }
            }
            catch (Exception exception)
            {
                var message = "Script error! File: " + Path.GetFullPath(FilePath) + ".";
                if (_currentCode != null)
                {
                    message += ("\nCode: " + _currentCode.Literal);
                }
                message += "\n" + exception.Message + "\n" + Log.GetLastLine(exception);
                Log.LogMessageToFile(message);
                if (Globals.ShowScriptWindow)
                {
                    MessageBox.Show(message);
                }
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