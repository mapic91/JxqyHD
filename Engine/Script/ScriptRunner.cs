using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Engine.Map;

namespace Engine.Script
{
    public class ScriptRunner
    {
        private ScriptParser _targetScript;

        private int _currentIndex;
        private ScriptParser.Code _currentCode;
        private bool _isEnd = true;

        public object BelongObject { private set; get; }

        public ScriptParser TargetScript
        {
            get { return _targetScript; }
            set { _targetScript = value; }
        }

        public bool IsEnd
        {
            get { return _isEnd; }
            private set { _isEnd = value; }
        }

        private static string ToMessageString(ScriptParser.Code code)
        {
            return DateTime.Now.ToString("HH:mm:ss.fff") + "    " + code.Literal + "    [" + code.LineNumber + "]";
        }

        public ScriptRunner(ScriptParser scriptParser, object belongObject = null)
        {
            _targetScript = scriptParser;
            BelongObject = belongObject;
        }

        public void Begin()
        {
            IsEnd = _targetScript == null || !_targetScript.IsOk;
            _currentIndex = 0;
            _currentCode = null;
        }

        public bool Continue()
        {
            if (IsEnd) return false;
            var count = _targetScript.Codes.Count();
            for (; _currentIndex < count; _currentIndex++)
            {
                string gotoPosition;
                bool scriptEnd;
                if (!RunCode(_currentIndex, out gotoPosition, out scriptEnd)) break;
                if (scriptEnd)
                {
                    IsEnd = true;
                    _currentIndex = count;
                    return false;
                }
                if (!string.IsNullOrEmpty(gotoPosition))
                {
                    gotoPosition += ":";
                    _currentIndex = 0;//Scan from begin
                    while (_currentIndex < count)
                    {
                        var code = _targetScript.GetCode(_currentIndex, true);
                        if (code != null &&
                            code.IsGoto &&
                            code.Name == gotoPosition)
                        {
                            Globals.TheMessageSender.SendFunctionCallMessage(ToMessageString(code));
                            break;
                        }
                        _currentIndex++;
                    }
                }
            }
            IsEnd = (_currentIndex == count);
            return !IsEnd;
        }

        #region Code run
        /// <summary>
        /// RunCode
        /// </summary>
        /// <param name="code">Code to run</param>
        /// <param name="gotoPosition"></param>
        /// <param name="scriptEnd">Script is returned if true</param>
        /// <returns>True if code runed, false if code is running</returns>
        private bool RunCode(int lineIndex, out string gotoPosition, out bool scriptEnd)
        {
            gotoPosition = null;
            scriptEnd = false;

            var code = _targetScript.GetCode(lineIndex);
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
                        case "ChooseEx":
                            isEnd = ScriptExecuter.IsChooseExEnd(_currentCode.Parameters);
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
                        case "BuyGoodsOnly":
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
                    Globals.TheMessageSender.SendFunctionCallMessage(ToMessageString(_currentCode));

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
                        case "DelMemo":
                            ScriptExecuter.DelMemo(parameters);
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
                            MapBase.MapTime = int.Parse(parameters[0]);
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
                        case "DisplayMessage":
                            ScriptExecuter.DisplayMessage(parameters);
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
                        case "ChooseEx":
                            ScriptExecuter.ChooseEx(parameters);
                            isEnd = false;
                            break;
                        case "RunScript":
                            ScriptExecuter.RunScript(parameters, BelongObject);
                            break;
                        case "RunParallelScript":
                            ScriptExecuter.RunParallelScript(parameters, BelongObject);
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
                            ScriptExecuter.BuyGoods(parameters, true, BelongObject);
                            isEnd = ScriptExecuter.IsBuyGoodsEnd();
                            break;
                        case "BuyGoodsOnly":
                            ScriptExecuter.BuyGoods(parameters, false, BelongObject);
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
                        case "GetPartnerIdx":
                            ScriptExecuter.GetPartnerIdx(parameters);
                            break;
                        case "GetExp":
                            ScriptExecuter.GetPlayerExp(parameters);
                            break;
                        case "ClearAllVar":
                            ScriptExecuter.ClearAllVar(parameters);
                            break;
                        case "SetDropIni":
                            ScriptExecuter.SetDropIni(parameters);
                            break;
                        case "ClearAllSave":
                            ScriptExecuter.ClearAllSave(parameters);
                            break;
                        case "EnableSave":
                            ScriptExecuter.EnableSave(parameters);
                            break;
                        case "DisableSave":
                            ScriptExecuter.DisableSave(parameters);
                            break;
                        case "CheckFreeGoodsSpace":
                            ScriptExecuter.CheckFreeGoodsSpace(parameters);
                            break;
                        case "CheckFreeMagicSpace":
                            ScriptExecuter.CheckFreeMagicSpace(parameters);
                            break;
                        case "GetPlayerState":
                            ScriptExecuter.GetPlayerState(parameters);
                            break;
                        case "GetPlayerMagicLevel":
                            ScriptExecuter.GetPlayerMagicLevel(parameters);
                            break;
                        case "EnabelDrop":
                            ScriptExecuter.EnabelDrop(parameters);
                            break;
                        case "DisableDrop":
                            ScriptExecuter.DisableDrop(parameters);
                            break;
                        case "ClearGoods":
                            ScriptExecuter.ClearGoods(parameters);
                            break;
                        case "ClearMagic":
                            ScriptExecuter.ClearMagic(parameters);
                            break;
                        case "AddMoveSpeedPercent":
                            ScriptExecuter.AddMoveSpeedPercent(parameters);
                            break;
                        case "UseMagic":
                            ScriptExecuter.UseMagic(parameters);
                            break;
                        case "IsEquipWeapon":
                            ScriptExecuter.IsEquipWeapon(parameters);
                            break;
                        case "AddAttack":
                            ScriptExecuter.AddAttack(parameters);
                            break;
                        case "AddDefend":
                            ScriptExecuter.AddDefend(parameters);
                            break;
                        case "AddEvade":
                            ScriptExecuter.AddEvade(parameters);
                            break;
                        case "AddLifeMax":
                            ScriptExecuter.AddLifeMax(parameters);
                            break;
                        case "AddManaMax":
                            ScriptExecuter.AddManaMax(parameters);
                            break;
                        case "AddThewMax":
                            ScriptExecuter.AddThewMax(parameters);
                            break;
                        case "DelMagic":
                            ScriptExecuter.DelMagic(parameters);
                            break;
                        case "SetPlayerMagicToUseWhenBeAttacked":
                            ScriptExecuter.SetPlayerMagicToUseWhenBeAttacked(parameters);
                            break;
                        case "SetNpcMagicToUseWhenBeAttacked":
                            ScriptExecuter.SetNpcMagicToUseWhenBeAttacked(parameters);
                            break;
                        case "LoadOneNpc":
                            ScriptExecuter.LoadOneNpc(parameters);
                            break;
                        case "AddNpcProperty":
                            ScriptExecuter.AddNpcProperty(parameters);
                            break;
                        case "ChangeFlyIni":
                            ScriptExecuter.ChangeFlyIni(parameters);
                            break;
                        case "ChangeFlyIni2":
                            ScriptExecuter.ChangeFlyIni2(parameters);
                            break;
                        case "AddFlyInis":
                            ScriptExecuter.AddFlyInis(parameters);
                            break;
                        case "SetShowMapPos":
                            ScriptExecuter.SetShowMapPos(parameters);
                            break;
                        case "SetNpcDestination":
                            ScriptExecuter.SetNpcDestination(parameters);
                            break;
                        case "SetWalkIsRun":
                            ScriptExecuter.SetWalkIsRun(parameters);
                            break;
                        default:
                            throw new Exception("无此函数");
                    }
                }
            }
            catch (Exception exception)
            {
                string fullPath;
                try
                {
                    fullPath = Path.GetFullPath(_targetScript.FilePath);
                }
                catch (Exception)
                {
                    fullPath = _targetScript.FilePath;
                }
                var message = "Script error! File: " + fullPath + ".";
                if (_currentCode != null)
                {
                    message += ("\nCode: " + _currentCode.Literal);
                }
                message += "\n" + exception.Message + "\n" + Log.GetLastLine(exception);
                Log.LogMessage(message);
                if (Globals.TheGame.IsInEditMode)
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
