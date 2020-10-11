using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Engine.Gui;
using Engine.ListManager;
using Engine.Map;
using Engine.Storage;
using Engine.Weather;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Engine.Script
{
    public static class ScriptExecuter
    {
        private static readonly Dictionary<string, int> Variables = new Dictionary<string, int>();
        private static float _fadeTransparence;
        private static LinkedList<TalkTextDetail> _talkList; 
        private static float _sleepingMilliseconds;
        private static Color _videoDrawColor;
        private static Video _video;
        private static VideoPlayer _videoPlayer;
        private static bool _isTimeScriptSet;
        private static int _timeScriptSeconds;
        private static string _timeScriptFileName;

        #region Public property
        public static bool IsInFadeOut;
        public static bool IsInFadeIn;
        public static bool IsInTalk;

        public static float FadeTransparence
        {
            get { return _fadeTransparence; }
            private set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                _fadeTransparence = value;
            }
        }

        public static bool IsInPlayingMovie
        {
            get
            {
                return (_videoPlayer != null &&
                        _videoPlayer.State != MediaState.Stopped);
            }
        }

        public static bool IsTimeScriptSet
        {
            get { return _isTimeScriptSet; }
        }

        public static int TimeScriptSeconds
        {
            get { return _timeScriptSeconds; }
        }

        public static string TimeScriptFileName
        {
            get { return _timeScriptFileName; }
        }

        #endregion Public property

        #region Private
        private static void GetTargetAndScript(string nameWithQuotes,
            string scriptFileNameWithQuotes,
            object belongObject,
            out Character target,
            out string script)
        {
            GetTarget(nameWithQuotes, belongObject, out target);
            script = Utils.RemoveStringQuotes(scriptFileNameWithQuotes);
        }

        private static Character GetPlayerOrNpc(string name)
        {
            if (Globals.ThePlayer != null &&
                Globals.ThePlayer.Name == name)
            {
                return Globals.ThePlayer;
            }
            return NpcManager.GetNpc(name);
        }

        private static List<Character> GetPlayerAndAllNpcs(string name, object defaultCharacter)
        {
            if (string.IsNullOrEmpty(name))
            {
                var npc = defaultCharacter as Character;
                if (npc == null)
                {
                    return new List<Character>();
                }
                return new List<Character>{npc};
            }

            var list = NpcManager.GetAllNpcs(name);
            if (Globals.ThePlayer != null &&
                Globals.ThePlayer.Name == name)
            {
                list.Add(Globals.ThePlayer);
            }
            return list;
        }

        private static void GetTarget(string nameWithQuotes,
            object belongObject,
            out Character target)
        {
            var name = Utils.RemoveStringQuotes(nameWithQuotes);
            target = belongObject as Character;
            if (!string.IsNullOrEmpty(name))
            {
                target = GetPlayerOrNpc(name);
            }
        }

        private static bool IsPlayerNull()
        {
            return Globals.PlayerKindCharacter == null;
        }

        /// <summary>
        /// Used for type: name,x, y or x,y
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="belongObject"></param>
        /// <param name="target"></param>
        /// <param name="value"></param>
        private static void GetTargetAndValue3(List<string> parameters,
            object belongObject,
            out Character target,
            out Vector2 value)
        {
            target = belongObject as Character;
            value = Vector2.Zero;
            if (parameters.Count == 3)
            {
                target = GetPlayerOrNpc(Utils.RemoveStringQuotes(parameters[0]));
                value = new Vector2(
                    int.Parse(parameters[1]),
                    int.Parse(parameters[2]));
            }
            else if (parameters.Count == 2)
            {
                value = new Vector2(
                    int.Parse(parameters[0]),
                    int.Parse(parameters[1]));
            }
        }

        /// <summary>
        /// Used for type: name,x or x
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="belongObject"></param>
        /// <param name="target"></param>
        /// <param name="value"></param>
        private static void GetTargetAndValue2(List<string> parameters,
            object belongObject,
            out Character target,
            out int value)
        {
            target = belongObject as Character;
            value = 0;
            if (parameters.Count == 2)
            {
                target = GetPlayerOrNpc(Utils.RemoveStringQuotes(parameters[0]));
                value = int.Parse(parameters[1]);
            }
            else if (parameters.Count == 1)
            {
                value = int.Parse(parameters[0]);
            }
        }

        /// <summary>
        /// Used for type: name,x or x
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="belongObject"></param>
        /// <param name="target"></param>
        /// <param name="value"></param>
        private static void GetTargetAndValue2(List<string> parameters,
            object belongObject,
            out Character target,
            out string value)
        {
            target = belongObject as Character;
            value = "";
            if (parameters.Count == 2)
            {
                target = GetPlayerOrNpc(Utils.RemoveStringQuotes(parameters[0]));
                value = Utils.RemoveStringQuotes(parameters[1]);
            }
            else if (parameters.Count == 1)
            {
                value = Utils.RemoveStringQuotes(parameters[0]);
            }
        }

        /// <summary>
        /// Used for type: name,x, y or x,y
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="belongObject"></param>
        /// <param name="target"></param>
        /// <param name="value"></param>
        private static void GetTargetAndValue3(List<string> parameters,
            object belongObject,
            out Obj target,
            out Vector2 value)
        {
            target = belongObject as Obj;
            value = Vector2.Zero;
            if (parameters.Count == 3)
            {
                target = ObjManager.GetObj(Utils.RemoveStringQuotes(parameters[0]));
                value = new Vector2(
                    int.Parse(parameters[1]),
                    int.Parse(parameters[2]));
            }
            else if (parameters.Count == 2)
            {
                value = new Vector2(
                    int.Parse(parameters[0]),
                    int.Parse(parameters[1]));
            }
        }

        /// <summary>
        /// Check character whether moved to destination.If end enable input.
        /// </summary>
        /// <param name="character">The character to check</param>
        /// <param name="destinationTilePosition">Destination to move</param>
        /// <param name="isRun">Whether run to.Otherwise walk to</param>
        /// <returns>True end.False not end.</returns>
        private static bool IsCharacterMoveEndAndStanding(Character character, Vector2 destinationTilePosition, bool isRun)
        {
            var isEnd = true;
            if (character != null &&
                character.TilePosition != destinationTilePosition)
            {
                //Is character is standing, walk to destination
                if (character.IsStanding())
                {
                    if (isRun)
                    {
                        character.RunTo(destinationTilePosition);
                    }
                    else
                    {
                        character.WalkTo(destinationTilePosition);
                    }
                }
                //Check moveable
                if (character.Path == null ||
                    (character.Path.Count == 2 && 
                    character.TilePosition != MapBase.ToTilePosition(character.Path.First.Next.Value) &&
                    character.HasObstacle(MapBase.ToTilePosition(character.Path.First.Next.Value))))
                {
                    character.StandingImmediately();
                }
                else
                {
                    isEnd = false;
                }
            }
            else if (character != null &&
                character.TilePosition == destinationTilePosition &&
                !character.IsStanding())
            {
                //At destination tile keep moving
                isEnd = false;
            }
            if (isEnd)
            {
                Globals.IsInputDisabled = false;
            }
            return isEnd;
        }

        private static bool IsCharacterStanding(Character character)
        {
            if (character != null &&
                !character.IsStanding())
            {
                return false;
            }
            Globals.IsInputDisabled = false;
            return true;
        }

        private static bool IsCharacterGotoDirEnd(Character character)
        {
            if (character != null &&
                character.IsInStepMove)
            {
                return false;
            }
            Globals.IsInputDisabled = false;
            return true;
        }
        #endregion Private

        public static void Init()
        {
            IsInFadeIn = false;
            IsInFadeOut = false;
            IsInTalk = false;
            _sleepingMilliseconds = 0;
            _isTimeScriptSet = false;
            _timeScriptFileName = string.Empty;
            StopMovie();
            Variables.Clear();
        }

        #region Update Draw
        public static void Update(GameTime gameTime)
        {
            Globals.TheMessageSender.SendScriptVariablesMessage(Variables);

            if (IsInFadeOut && FadeTransparence < 1f)
            {
                FadeTransparence += 0.03f;
            }
            else if (IsInFadeIn && FadeTransparence > 0f)
            {
                FadeTransparence -= 0.03f;
                if (FadeTransparence <= 0f) IsInFadeIn = false;
            }

            if (IsInTalk)
            {
                if (GuiManager.IsDialogEnd())
                {
                    if (_talkList == null)
                    {
                        IsInTalk = false;
                    }
                    else
                    {
                        _talkList.RemoveFirst();
                        if (_talkList.Count > 0)
                        {
                            var detail = _talkList.First.Value;
                            if (detail != null)
                            {
                                GuiManager.ShowDialog(detail.Text, detail.PortraitIndex);
                            }
                        }
                        else
                        {
                            IsInTalk = false;
                        }
                    }           
                }
            }

            if (_sleepingMilliseconds > 0)
            {
                _sleepingMilliseconds -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_sleepingMilliseconds <= 0)
                {
                    _sleepingMilliseconds = 0;
                    Globals.IsInputDisabled = false;
                }
            }

            if (_isTimeScriptSet &&
                GuiManager.IsTimerStarted())
            {
                if (_timeScriptSeconds == GuiManager.GetTimerCurrentSeconds())
                {
                    ScriptManager.RunScript(Utils.GetScriptParser(_timeScriptFileName));
                    _isTimeScriptSet = false;
                    _timeScriptFileName = string.Empty;
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {

        }
        #endregion Update Draw

        public static void LoadVariables(KeyDataCollection keyDataCollection)
        {
            Variables.Clear();
            if (keyDataCollection == null) return;
            foreach (var keys in keyDataCollection)
            {
                Variables['$' + keys.KeyName] = int.Parse(keys.Value);
            }
        }

        public static void SaveVariables(KeyDataCollection keyDataCollection)
        {
            foreach (var key in Variables.Keys)
            {
                keyDataCollection.AddKey(key.Substring(1), Variables[key].ToString());
            }
        }

        public static int GetVariablesValue(string key)
        {
            if(Variables.ContainsKey(key))
            {
                return Variables[key];
            }
            return 0;
        }

        public static void Say(List<string> parameters)
        {
            Globals.IsInputDisabled = true;
            switch (parameters.Count)
            {
                case 1:
                    GuiManager.ShowDialog(Utils.RemoveStringQuotes(
                        parameters[0]));
                    break;
                case 2:
                    GuiManager.ShowDialog(Utils.RemoveStringQuotes(
                        parameters[0]),
                        int.Parse(parameters[1]));
                    break;
            }
            Globals.PlayerKindCharacter.ToNonFightingState();
        }

        public static bool IsSayEnd()
        {
            if (GuiManager.IsDialogEnd())
            {
                Globals.IsInputDisabled = false;
                return true;
            }
            return false;
        }

        private static readonly Regex IfParameterPatten = new Regex(@"(\$[_a-zA-Z0-9]+) *([><=]+).*?([-]?[0-9]+)");
        public static bool If(List<string> parameters)
        {
            var parmeter = parameters[0];
            var match = IfParameterPatten.Match(parmeter);
            if (match.Success)
            {
                var groups = match.Groups;
                var variable = groups[1].Value;
                var compare = groups[2].Value;
                var value = int.Parse(groups[3].Value);
                //default variable value
                var variableValue = 0;
                if (Variables.ContainsKey(variable))
                {
                    variableValue = Variables[variable];
                }
                switch (compare)
                {
                    case "==":
                        return variableValue == value;
                    case ">>":
                        return variableValue > value;
                    case ">=":
                        return variableValue >= value;
                    case "<<":
                        return variableValue < value;
                    case "<=":
                        return variableValue <= value;
                    case "<>":
                        return variableValue != value;
                }
            }
            return false;
        }

        public static void Add(List<string> parameters)
        {
            var variable = parameters[0];
            if (variable[0] != '$') variable = "$" + variable;
            var value = int.Parse(parameters[1]);
            if (!Variables.ContainsKey(variable))
                Variables[variable] = 0;
            Variables[variable] += value;
        }

        public static void Assign(List<string> parameters)
        {
            var variable = parameters[0];
            var value = int.Parse(parameters[1]);
            Variables[variable] = value;
        }

        public static void FadeOut()
        {
            IsInFadeOut = true;
            IsInFadeIn = false;
            FadeTransparence = 0f;
        }

        public static bool IsFadeOutEnd()
        {
            if (FadeTransparence >= 1f)
            {
                return true;
            }
            return false;
        }

        public static void FadeIn()
        {
            IsInFadeOut = false;
            IsInFadeIn = true;
            FadeTransparence = 1f;
        }

        public static bool IsFadeInEnd()
        {
            if (!IsInFadeIn)
            {
                return true;
            }
            return false;
        }

        public static void DrawFade(SpriteBatch spriteBatch)
        {
            var fadeTextrue = TextureGenerator.GetColorTexture(
                    Color.Black * FadeTransparence,
                    1,
                    1);
            spriteBatch.Draw(fadeTextrue,
                new Rectangle(0, 
                    0, 
                    Globals.TheGame.GraphicsDevice.PresentationParameters.BackBufferWidth,
                    Globals.TheGame.GraphicsDevice.PresentationParameters.BackBufferHeight),
                Color.White);
        }

        public static void DelNpc(List<string> parameters)
        {
            NpcManager.DeleteNpc(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void ClearBody()
        {
            ObjManager.ClearBody();
        }

        public static void StopMusic()
        {
            BackgroundMusic.Stop();
        }

        public static void PlayMusic(List<string> parameters)
        {
            BackgroundMusic.Play(Utils.RemoveStringQuotes(parameters[0]));
        }


        public static void PlaySound(List<string> parameters, object belongObject)
        {
            var fileName = Utils.RemoveStringQuotes(parameters[0]);
            var sound = Utils.GetSoundEffect(fileName);
            var soundPosition = Globals.ListenerPosition;
            var sprit = belongObject as Sprite;
            if (sprit != null) soundPosition = sprit.PositionInWorld;

            SoundManager.Play3DSoundOnece(sound, soundPosition - Globals.ListenerPosition);
        }

        public static void OpenBox(List<string> parameters, object belongObject)
        {
            if (parameters.Count == 0)
            {
                var obj = belongObject as Obj;
                if (obj != null)
                {
                    obj.OpenBox();
                }
            }
            else
            {
                var obj = ObjManager.GetObj(Utils.RemoveStringQuotes(parameters[0]));
                if (obj != null)
                {
                    obj.OpenBox();
                }
            }
        }

        public static void CloseBox(List<string> parameters, object belongObject)
        {
            var target = belongObject as Obj;
            if (parameters.Count != 0)
            {
                target = ObjManager.GetObj(Utils.RemoveStringQuotes(parameters[0]));
            }
            if (target != null)
            {
                target.CloseBox();
            }
        }

        public static void SetObjScript(List<string> parameters, object belongObject)
        {
            var name = Utils.RemoveStringQuotes(parameters[0]);
            var scriptFileName = Utils.RemoveStringQuotes(parameters[1]);
            var target = belongObject as Obj;
            if (!string.IsNullOrEmpty(name))
                target = ObjManager.GetObj(name);
            if (target != null)
            {
                target.ScriptFile = scriptFileName;
            }
        }

        public static void SetNpcScript(List<string> parameters, object belongObject)
        {
            Character target;
            string script;
            GetTargetAndScript(parameters[0],
                parameters[1],
                belongObject,
                out target,
                out script);
            if (target != null)
            {
                target.ScriptFile = script;
            }
        }

        public static void SetNpcDeathScript(List<string> parameters, object belongObject)
        {
            Character target;
            string script;
            GetTargetAndScript(parameters[0],
                parameters[1],
                belongObject,
                out target,
                out script);
            if (target != null)
            {
                target.DeathScript = script;
            }
        }

        public static void SetNpcLevel(List<string> parameters, object belongObject)
        {
            Character target;
            GetTarget(parameters[0],
                belongObject,
                out target);
            var value = int.Parse(parameters[1]);
            if (target != null)
            {
                target.SetLevelTo(value);
            }
        }

        public static void SetLevelFile(List<string> parameters, object belongObject)
        {
            var target = Globals.PlayerKindCharacter;
            if (target != null)
            {
                var path = @"ini\level\" + Utils.RemoveStringQuotes(parameters[0]);
                target.LevelIni = Utils.GetLevelLists(path);
            }
        }

        public static void AddRandMoney(List<string> parameters)
        {
            var min = int.Parse(parameters[0]);
            var max = int.Parse(parameters[1]) + 1;
            var money = Globals.TheRandom.Next(min, max);
            Globals.ThePlayer.AddMoney(money);
        }

        public static void AddLife(List<string> parameters)
        {
            var value = int.Parse(parameters[0]);
            Globals.ThePlayer.AddLife(value);
        }

        public static void AddThew(List<string> parameters)
        {
            var value = int.Parse(parameters[0]);
            Globals.ThePlayer.AddThew(value);
        }

        public static void AddMana(List<string> parameters)
        {
            var value = int.Parse(parameters[0]);
            Globals.ThePlayer.AddMana(value);
        }

        public static void AddExp(List<string> parameters)
        {
            Globals.ThePlayer.AddExp(int.Parse(parameters[0]));
        }

        public static void SetPlayerPos(List<string> parameters)
        {
            int x, y;
            Character character = null;
            if (parameters.Count == 3)
            {
                var name = Utils.RemoveStringQuotes(parameters[0]);
                if (Globals.ThePlayer != null && Globals.ThePlayer.Name == name)
                {
                    character = Globals.ThePlayer;
                }
                else
                {
                    character = NpcManager.GetNpc(name);
                }
                            
                x = int.Parse(parameters[1]);
                y = int.Parse(parameters[2]);
            }
            else
            {
                character = Globals.PlayerKindCharacter;
                x = int.Parse(parameters[0]);
                y = int.Parse(parameters[1]);
            }
            
            if(character == null) return;
            character.SetPosition(new Vector2(x, y));
            Globals.TheCarmera.CenterPlayerInCamera();
            //Reset parter position relate to player position
            if (Globals.ThePlayer != null)
            {
                Globals.ThePlayer.ResetPartnerPosition();
                Globals.ThePlayer.CheckMapTrap();
            }
        }

        public static void SetPlayerDir(List<string> parameters)
        {
            Globals.ThePlayer.SetDirectionValue(int.Parse(parameters[0]));
        }

        public static void LoadMap(List<string> parameters)
        {
            LoadMap(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void LoadMap(string filename)
        {
            WeatherManager.StopRain();
            MapBase.OpenMap(filename);
            Utils.ClearScriptParserCache();
            NpcManager.ClearAllNpcAndKeepPartner();
            ObjManager.ClearAllObjAndFileName();
        }

        public static void LoadNpc(List<string> parameters)
        {
            NpcManager.Load(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void MergeNpc(List<string> parameters)
        {
            NpcManager.Merge(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void LoadObj(List<string> parameters)
        {
            ObjManager.Load(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void AddGoods(List<string> parameters)
        {
            AddGoods(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void AddGoods(string fileName)
        {
            int index;
            Good good;
            var result = GoodsListManager.AddGoodToList(
                fileName,
                out index,
                out good);
            if (result && good != null)
            {
                GuiManager.ShowMessage("你获得了" + good.Name);
            }
            GuiManager.UpdateGoodsView();
        }

        public static void AddRandGoods(List<string> parameters)
        {
            var fileName = GetRandItem(@"ini\buy\" + Utils.RemoveStringQuotes(parameters[0]));
            if (string.IsNullOrEmpty(fileName)) return;
            AddGoods(fileName);
        }

        public static string GetRandItem(string filePath)
        {
            try
            {
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(filePath);
                var count = int.Parse(data["Header"]["Count"]);
                var rand = Globals.TheRandom.Next(1, count + 1);
                return data[rand.ToString()]["IniFile"];
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static void AddMagic(List<string> parameters)
        {
            Globals.ThePlayer.AddMagic(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void AddMoney(List<string> parameters)
        {
            Globals.ThePlayer.AddMoney(int.Parse(parameters[0]));
        }

        public static void AddNpc(List<string> parameters)
        {
            NpcManager.AddNpc(Utils.RemoveStringQuotes(parameters[0]),
                int.Parse(parameters[1]),
                int.Parse(parameters[2]),
                int.Parse(parameters[3]));
        }

        public static void AddObj(List<string> parameters)
        {
            ObjManager.AddObj(Utils.RemoveStringQuotes(parameters[0]),
                int.Parse(parameters[1]),
                int.Parse(parameters[2]),
                parameters.Count == 3 ? 0 : int.Parse(parameters[3]));
        }

        public static void Talk(List<string> parameters)
        {
            IsInTalk = true;
            _talkList = TalkTextList.GetTextDetails(int.Parse(parameters[0]),
                int.Parse(parameters[1]));
            
            if (_talkList != null && _talkList.Count > 0)
            {
                var detail = _talkList.First.Value;
                GuiManager.ShowDialog(detail.Text, detail.PortraitIndex);
            }
            else
            {
                IsInTalk = false;
            }
            Globals.PlayerKindCharacter.ToNonFightingState();
        }

        public static void Memo(List<string> parameters)
        {
            GuiManager.AddMemo(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void AddToMemo(List<string> parameters)
        {
            var detail = TalkTextList.GetTextDetail(int.Parse(parameters[0]));
            if (detail == null) return;
            GuiManager.AddMemo(detail.Text);
        }

        public static void DelGoods(List<string> parameters, object belongObject)
        {
            if (parameters.Count == 0)
            {
                var good = belongObject as Good;
                if (good != null)
                {
                    GuiManager.DeleteGood(good.FileName);
                }
            }
            else
            {
                GuiManager.DeleteGood(Utils.RemoveStringQuotes(parameters[0]));
            }
        }

        public static void DelCurObj(object belongObject)
        {
            var obj = belongObject as Obj;
            if (obj == null) return;
            obj.IsRemoved = true;
        }

        public static void DelObj(List<string> parameters, object belongObject)
        {
            ObjManager.DeleteObj(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void FreeMap()
        {
            if (MapBase.Instance != null)
            {
                MapBase.Free();
            }
        }

        public static void SetTrap(List<string> parameters)
        {
            MapBase.Instance.SetMapTrap(int.Parse(parameters[1]),
                Utils.RemoveStringQuotes(parameters[2]),
                Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void SetMapTrap(List<string> parameters)
        {
            MapBase.Instance.SetMapTrap(int.Parse(parameters[0]),
                Utils.RemoveStringQuotes(parameters[1]));
        }

        public static void FullLife()
        {
            if (Globals.ThePlayer != null)
            {
                Globals.ThePlayer.FullLife();
            }
        }

        public static void FullMana()
        {
            if (Globals.ThePlayer != null)
            {
                Globals.ThePlayer.FullMana();
            }
        }

        public static void FullThew()
        {
            if (Globals.ThePlayer != null)
            {
                Globals.ThePlayer.FullThew();
            }
        }

        public static void ShowNpc(List<string> parameters)
        {
            var name = Utils.RemoveStringQuotes(parameters[0]);
            var show = (int.Parse(parameters[1]) != 0);
            NpcManager.ShowNpc(name, show);
        }

        public static void Sleep(List<string> parameters)
        {
            _sleepingMilliseconds = int.Parse(parameters[0]);
            Globals.IsInputDisabled = true;
        }

        public static bool IsSleepEnd()
        {
            if (_sleepingMilliseconds > 0)
            {
                return false;
            }
            Globals.IsInputDisabled = false;
            return true;
        }

        public static void ShowMessage(List<string> parameters)
        {
            var detail = TalkTextList.GetTextDetail(int.Parse(parameters[0]));
            if (detail != null)
            {
                GuiManager.ShowMessage(detail.Text);
            }
        }

        public static void DisplayMessage(List<string> parameters)
        {
            GuiManager.ShowMessage(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void SetMagicLevel(List<string> parameters)
        {
            var fileName = Utils.RemoveStringQuotes(parameters[0]);
            var level = int.Parse(parameters[1]);
            MagicListManager.SetMagicLevel(fileName, level);
            GuiManager.XiuLianInterface.UpdateItem();
        }

        public static void ShowSnow(List<string> parameters)
        {
            var isShow = (int.Parse(parameters[0]) != 0);
            WeatherManager.ShowSnow(isShow);
        }

        public static void BeginRain(List<string> parameters)
        {
            WeatherManager.BeginRain(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void EndRain()
        {
            WeatherManager.StopRain();
        }

        public static void ChangeMapColor(List<string> parameters)
        {
            var color = new Color(int.Parse(parameters[0]),
                int.Parse(parameters[1]),
                int.Parse(parameters[2]));
            MapBase.DrawColor = color;
        }

        public static void ChangeAsfColor(List<string> parameters)
        {
            var color = new Color(int.Parse(parameters[0]),
                int.Parse(parameters[1]),
                int.Parse(parameters[2]));
            Sprite.DrawColor = color;
        }

        public static void Choose(List<string> parameters)
        {
            GuiManager.Selection(Utils.RemoveStringQuotes(parameters[0]),
                Utils.RemoveStringQuotes(parameters[1]),
                Utils.RemoveStringQuotes(parameters[2]));
        }

        public static void Select(List<string> parameters)
        {
            GuiManager.Selection(TalkTextList.GetTextDetail(int.Parse(parameters[0])).Text,
                TalkTextList.GetTextDetail(int.Parse(parameters[1])).Text,
                TalkTextList.GetTextDetail(int.Parse(parameters[2])).Text);
        }

        public static void ChooseEx(List<string> parameters)
        {
            var selections = new List<string>();
            for (int i = 1; i < parameters.Count - 1; i++)
            {
                selections.Add(Utils.RemoveStringQuotes(parameters[i]));
            }
            GuiManager.ChooseEx(Utils.RemoveStringQuotes(parameters[0]),selections);
        }

        public static bool IsChooseEnd(List<string> parameters)
        {
            if (GuiManager.IsSelectionEnd())
            {
                Variables[parameters[3]] = GuiManager.GetSelection();
                return true;
            }
            return false;
        }

        public static bool IsChooseExEnd(List<string> parameters)
        {
            if (GuiManager.IsChooseExEnd())
            {
                Variables[parameters[parameters.Count-1]] = GuiManager.GetMultiSelectionResult();
                return true;
            }
            return false;
        }

        public static void RunScript(List<string> parameters, object belongObject)
        {
            RunScript(Utils.RemoveStringQuotes(parameters[0]), belongObject);
        }

        public static void RunParallelScript(List<string> parameters, object belongObject)
        {
            ScriptManager.RunParallelScript(Utils.GetScriptFilePath(Utils.RemoveStringQuotes(parameters[0])),
                parameters.Count == 2 ? int.Parse(parameters[1]) : 0);
        }

        public static void RunScript(string fileName, object belongObject = null)
        {
            ScriptManager.RunScript(Utils.GetScriptParser(fileName), belongObject);
        }

        public static void PlayMovie(string fileName, Color drawColor)
        {
            _video = Utils.GetVideo(fileName);
            if (_video == null) return;
            _videoPlayer = new VideoPlayer();
            if (_videoPlayer == null) return;
            _videoDrawColor = drawColor;

            _videoPlayer.Play(_video);
        }

        public static void PlayMovie(string fileName)
        {
            PlayMovie(fileName, Color.White);
        }

        public static void PlayMovie(List<string> parameters)
        {
            var fileName = Utils.RemoveStringQuotes(parameters[0]);
            var color = Color.White;
            //if (parameters.Count == 4)
            //{
            //    color = new Color(
            //        int.Parse(parameters[1]),
            //        int.Parse(parameters[2]),
            //        int.Parse(parameters[3]));
            //}
            PlayMovie(fileName, color);
        }

        public static void StopMovie()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
            }
        }

        public static void DrawVideo(SpriteBatch spriteBatch)
        {
            if (_videoPlayer != null && _videoPlayer.State != MediaState.Stopped)
            {
                var texture = _videoPlayer.GetTexture();
                if (texture != null)
                {
                    spriteBatch.Draw(texture,
                        new Rectangle(0, 0, Globals.WindowWidth, Globals.WindowHeight),
                        _videoDrawColor);
                }
            }
        }

        public static bool IsMovePlayEnd()
        {
            if (_videoPlayer != null)
            {
                return _videoPlayer.State == MediaState.Stopped;
            }
            return true;
        }

        public static void SaveMapTrap()
        {
            MapBase.SaveTrap(@"save\game\Traps.ini");
        }

        public static void SaveNpc(List<string> parameters)
        {
            string fileName = null;
            if (parameters.Count == 1)
            {
                fileName = Utils.RemoveStringQuotes(parameters[0]);
            }
            NpcManager.SaveNpc(fileName);
        }

        public static void SaveObj(List<string> parameters)
        {
            string fileName = null;
            if (parameters.Count == 1)
            {
                fileName = Utils.RemoveStringQuotes(parameters[0]);
            }
            ObjManager.Save(fileName);
        }

        public static void GetRandNum(List<string> parameters)
        {
            Variables[parameters[0]] = Globals.TheRandom.Next(
                int.Parse(parameters[1]),
                int.Parse(parameters[2]) + 1);
        }

        public static void SetNpcDir(List<string> parameters, object belongObject)
        {
            Character target;
            int value;
            GetTargetAndValue2(parameters, belongObject, out target, out value);
            if (target != null)
            {
                target.SetDirectionValue(value);
            }
        }

        public static void SetNpcKind(List<string> parameters, object belongObject)
        {
            var list = GetPlayerAndAllNpcs(Utils.RemoveStringQuotes(parameters[0]), belongObject);
            int value = int.Parse(parameters[1]);
            foreach (var character in list)
            {
                character.SetKind(value);
            }
        }

        public static void SetNpcMagicFile(List<string> parameters, object belongObject)
        {
            Character target = null;
            var fileName = "";
            if (parameters.Count == 1)
            {
                target = belongObject as Character;
                fileName = Utils.RemoveStringQuotes(parameters[0]);
            }
            else if (parameters.Count == 2)
            {
                target = GetPlayerOrNpc(Utils.RemoveStringQuotes(parameters[0]));
                fileName = Utils.RemoveStringQuotes(parameters[1]);
            }
            if (target != null)
            {
                target.SetMagicFile(fileName);
            }
        }

        public static void SetNpcPos(List<string> parameters, object belongObject)
        {
            Character target;
            Vector2 value;
            GetTargetAndValue3(parameters, belongObject, out target, out value);
            if (target != null)
            {
                target.SetPosition(value);
            }
        }

        public static void SetNpcRelation(List<string> parameters, object belongObject)
        {
            var list = GetPlayerAndAllNpcs(Utils.RemoveStringQuotes(parameters[0]), belongObject);
            var value = int.Parse(parameters[1]);
            foreach (var character in list)
            {
                character.SetRelation(value);
            }
        }

        public static void SetNpcRes(List<string> parameters, object belongObject)
        {
            Character target = null;
            var fileName = "";
            if (parameters.Count == 1)
            {
                target = belongObject as Character;
                fileName = Utils.RemoveStringQuotes(parameters[0]);
            }
            else if (parameters.Count == 2)
            {
                target = GetPlayerOrNpc(Utils.RemoveStringQuotes(parameters[0]));
                fileName = Utils.RemoveStringQuotes(parameters[1]);
            }
            if (target != null)
            {
                target.SetRes(fileName);
            }
        }

        public static void SetNpcAction(List<string> parameters)
        {
            var target = GetPlayerOrNpc(Utils.RemoveStringQuotes(parameters[0]));
            var action = (CharacterState)int.Parse(parameters[1]);
            var destination = Vector2.Zero;
            if (parameters.Count == 4)
            {
                destination = new Vector2(int.Parse(parameters[2]),
                    int.Parse(parameters[3]));
            }
            if (target == null) return;
            switch (action)
            {
                case CharacterState.Stand:
                case CharacterState.Stand1:
                    target.NextStepStaning();
                    break;
                case CharacterState.Walk:
                    target.WalkTo(destination);
                    break;
                case CharacterState.Run:
                    target.RunTo(destination);
                    break;
                case CharacterState.Jump:
                    target.JumpTo(destination);
                    break;
                case CharacterState.Attack:
                case CharacterState.Attack1:
                case CharacterState.Attack2:
                    target.PerformeAttack(destination);
                    break;
                case CharacterState.Magic:
                    target.UseMagic(target.FlyIni, destination);
                    break;
                case CharacterState.Sit:
                    target.Sitdown();
                    break;
                case CharacterState.Hurt:
                    target.Hurting();
                    break;
                case CharacterState.Death:
                    target.Death();
                    break;
                case CharacterState.FightStand:
                    target.NextStepStaning();
                    target.ToFightingState();
                    break;
                case CharacterState.FightWalk:
                    target.WalkTo(destination);
                    target.ToFightingState();
                    break;
                case CharacterState.FightRun:
                    target.RunTo(destination);
                    target.ToFightingState();
                    break;
                case CharacterState.FightJump:
                    target.JumpTo(destination);
                    target.ToFightingState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void SetNpcActionFile(List<string> parameters)
        {
            var target = GetPlayerOrNpc(Utils.RemoveStringQuotes(parameters[0]));
            var state = (CharacterState)int.Parse(parameters[1]);
            var fileName = Utils.RemoveStringQuotes(parameters[2]);
            if (target != null)
            {
                target.SetNpcActionFile(state, fileName);
            }
        }

        public static void SetNpcActionType(List<string> parameters, object belongObject)
        {
            Character target;
            int value;
            GetTargetAndValue2(parameters, belongObject, out target, out value);
            if (target != null)
            {
                target.SetNpcActionType(value);
            }
        }

        public static void Watch(List<string> parameters)
        {
            var character1 = GetPlayerOrNpc(Utils.RemoveStringQuotes(parameters[0]));
            var character2 = GetPlayerOrNpc(Utils.RemoveStringQuotes(parameters[1]));
            if (character1 == null || character2 == null) return;
            var watchType = 0;
            if (parameters.Count == 3)
            {
                watchType = int.Parse(parameters[2]);
            }
            var isC1 = false;
            var isC2 = false;
            switch (watchType)
            {
                case 0:
                    isC1 = true;
                    isC2 = true;
                    break;
                case 1:
                    isC1 = true;
                    break;
            }
            if (isC1)
            {
                character1.SetDirection(character2.PositionInWorld - character1.PositionInWorld);
            }
            if (isC2)
            {
                character2.SetDirection(character1.PositionInWorld - character2.PositionInWorld);
            }
        }

        public static void SetObjOfs(List<string> parameters, object belongObject)
        {
            Obj target;
            Vector2 value;
            GetTargetAndValue3(parameters, belongObject, out target, out value);
            if (target != null)
            {
                target.SetOffSet(value);
            }
        }

        public static void DisableFight()
        {
            if (Globals.ThePlayer != null)
            {
                Globals.ThePlayer.DisableFight();
            }
        }

        public static void EnableFight()
        {
            if (Globals.ThePlayer != null)
            {
                Globals.ThePlayer.EnableFight();
            }
        }

        public static void DisableInput()
        {
            Globals.IsInputDisabled = true;
        }

        public static void EnableInput()
        {
            Globals.IsInputDisabled = false;
        }

        public static void DisableJump()
        {
            if (Globals.ThePlayer != null)
            {
                Globals.ThePlayer.DisableJump();
            }
        }

        public static void EnableJump()
        {
            if (Globals.ThePlayer != null)
            {
                Globals.ThePlayer.EnableJump();
            }
        }

        public static void DisableNpcAI()
        {
            Npc.DisableAI();
        }

        public static void EnableNpcAI()
        {
            Npc.EnableAI();
        }
        
        public static void DisableRun()
        {
            if (Globals.ThePlayer != null)
            {
                Globals.ThePlayer.DisableRun();
            }
        }

        public static void EnableRun()
        {
            if (Globals.ThePlayer != null)
            {
                Globals.ThePlayer.EnableRun();
            }
        }

        public static void SetPlayerState(List<string> parameters)
        {
            if (Globals.ThePlayer != null)
            {
                Globals.ThePlayer.SetFightState(int.Parse(parameters[0]) != 0);
            }
        }

        public static void OpenWaterEffect()
        {
            Globals.IsWaterEffectEnabled = true;
        }

        public static void CloseWaterEffect()
        {
            Globals.IsWaterEffectEnabled = false;
        }

        private static Vector2 _playerGotoDesitination;
        public static void PlayerGoto(List<string> parameters)
        {
            var tilePosition = new Vector2(
                int.Parse(parameters[0]),
                int.Parse(parameters[1]));
            if (Globals.PlayerKindCharacter != null)
            {
                Globals.PlayerKindCharacter.WalkTo(tilePosition);
                _playerGotoDesitination = tilePosition;
                Globals.IsInputDisabled = true;
            }
        }

        public static bool IsPlayerGotoEnd()
        {
            return IsCharacterMoveEndAndStanding(Globals.PlayerKindCharacter, _playerGotoDesitination, false);
        }

        public static void PlayerGotoDir(List<string> parameters)
        {
            if (IsPlayerNull()) return;
            Globals.PlayerKindCharacter.WalkToDirection(int.Parse(parameters[0]),
                int.Parse(parameters[1]));
            Globals.IsInputDisabled = true;
        }

        public static bool IsPlayerGotoDirEnd()
        {
            return IsCharacterGotoDirEnd(Globals.PlayerKindCharacter);
        }

        public static void PlayerGotoEx(List<string> parameters)
        {
            if (Globals.PlayerKindCharacter != null)
            {
                Globals.PlayerKindCharacter.WalkTo(new Vector2(
                    int.Parse(parameters[0]),
                    int.Parse(parameters[1])));
            }
        }

        public static void PlayerJumpTo(List<string> parameters)
        {
            if (IsPlayerNull()) return;
            Globals.IsInputDisabled = true;
            Globals.PlayerKindCharacter.JumpTo(new Vector2(
                int.Parse(parameters[0]),
                int.Parse(parameters[1])));
        }

        public static bool IsPlayerJumpToEnd()
        {
            return IsCharacterStanding(Globals.PlayerKindCharacter);
        }

        private static Vector2 _playerRunToDestination;
        public static void PlayerRunTo(List<string> parameters)
        {
            if (IsPlayerNull()) return;
            Globals.IsInputDisabled = true;
            _playerRunToDestination = new Vector2(
                int.Parse(parameters[0]),
                int.Parse(parameters[1]));
            Globals.PlayerKindCharacter.RunTo(_playerRunToDestination);

        }

        public static bool IsPlayerRunToEnd()
        {
            return IsCharacterMoveEndAndStanding(Globals.PlayerKindCharacter, _playerRunToDestination, true);
        }

        public static void PlayerRunToEx(List<string> parameters)
        {
            if (IsPlayerNull()) return;
            Globals.PlayerKindCharacter.RunTo(new Vector2(
                int.Parse(parameters[0]),
                int.Parse(parameters[1])));
        }

        private static Vector2 _npcGotoDestionation;
        private static Character _npcGotoCharacter;
        public static void NpcGoto(List<string> parameters, object belongObject)
        {
            Character target;
            Vector2 destination;
            GetTargetAndValue3(parameters, belongObject, out target, out destination);
            if (target != null)
            {
                _npcGotoDestionation = destination;
                _npcGotoCharacter = target;
                Globals.IsInputDisabled = true;
                target.WalkTo(destination);
            }
        }

        public static bool IsNpcGotoEnd(List<string> parameters, object belongObject)
        {
            return IsCharacterMoveEndAndStanding(_npcGotoCharacter, _npcGotoDestionation, false);
        }

        public static void NpcGotoDir(List<string> parameters, object belongObject)
        {
            Character target;
            Vector2 value;
            GetTargetAndValue3(parameters, belongObject, out target, out value);
            if (target != null)
            {
                Globals.IsInputDisabled = true;
                target.WalkToDirection((int)value.X,
                    (int)value.Y);
            }
        }

        public static bool IsNpcGotoDirEnd(List<string> parameters, object belongObject)
        {
            Character target;
            Vector2 value;
            GetTargetAndValue3(parameters, belongObject, out target, out value);
            return IsCharacterGotoDirEnd(target);
        }

        public static void NpcGotoEx(List<string> parameters, object belongObject)
        {
            Character target;
            Vector2 position;
            GetTargetAndValue3(parameters, belongObject, out target, out position);
            if (target != null)
            {
                target.WalkTo(position);
            }
        }

        public static void SetMoneyNum(List<string> parameters)
        {
            if (IsPlayerNull()) return;
            Globals.ThePlayer.SetMoney(int.Parse(parameters[0]));
        }

        public static void GetMoneyNum(List<string> parameters)
        {
            var name = "$MoneyNum";
            if (parameters.Count != 0)
            {
                name = parameters[0];
            }
            var value = 0;
            if (!IsPlayerNull())
            {
                value = Globals.ThePlayer.GetMoneyAmount();
            }
            Variables[name] = value;
        }

        public static void SetPlayerScn()
        {
            if (Globals.TheCarmera != null)
            {
                Globals.TheCarmera.CenterPlayerInCamera();
            }
        }

        public static void MoveScreen(List<string> parameters)
        {
            if (Globals.TheCarmera == null) return;
            var direction = Utils.GetDirection8(int.Parse(
                parameters[0]));
            Globals.TheCarmera.MoveTo(direction,
                int.Parse(parameters[1]),
                int.Parse(parameters[2]));
            Globals.IsInputDisabled = true;
        }

        public static bool IsMoveScreenEnd()
        {
            if (Globals.TheCarmera != null &&
                Globals.TheCarmera.IsInMove)
            {
                return false;
            }
            Globals.IsInputDisabled = false;
            return true;
        }

        public static void MoveScreenEx(List<string> parameters)
        {
            if (Globals.TheCarmera == null) return;
            Globals.TheCarmera.MoveTo(new Vector2(
                int.Parse(parameters[0]),
                int.Parse(parameters[1])),
                int.Parse(parameters[2]));
            Globals.IsInputDisabled = true;
        }

        public static bool IsMoveScreenExEnd()
        {
            if (Globals.TheCarmera != null &&
                Globals.TheCarmera.IsInMoveTo)
            {
                return false;
            }
            Globals.IsInputDisabled = false;
            return true;
        }

        public static void SetMapPos(List<string> parameters)
        {
            if (Globals.TheCarmera == null) return;
            Globals.TheCarmera.CarmeraBeginPositionInWorld =
                MapBase.ToPixelPosition(int.Parse(parameters[0]),
                    int.Parse(parameters[1]));
        }

        public static void EquipGoods(List<string> parameters)
        {
            var index = int.Parse(parameters[0]);
            var part = (Good.EquipPosition)int.Parse(parameters[1]);
            GuiManager.EquipGoods(index, part);
        }

        public static void BuyGoods(List<string> parameters, bool canSellSelfGoods)
        {
            GuiManager.BuyGoods(Utils.RemoveStringQuotes(parameters[0]), canSellSelfGoods);
            Globals.IsInputDisabled = true;
        }

        public static bool IsBuyGoodsEnd()
        {
            if (GuiManager.IsBuyGoodsEnd())
            {
                Globals.IsInputDisabled = false;
                return true;
            }
            return false;
        }

        public static void OpenTimeLimit(List<string> parameters)
        {
           OpenTimeLimit(int.Parse(parameters[0]));
        }

        public static void OpenTimeLimit(int time)
        {
            GuiManager.OpenTimeLimit(time);
        }

        public static void CloseTimeLimit()
        {
            GuiManager.CloseTimeLimit();
            _isTimeScriptSet = false;
        }

        public static void HideTimerWnd()
        {
            GuiManager.HideTimerWindow();
        }

        public static void SetTimeScript(List<string> parameters)
        {
            SetTimeScript(int.Parse(parameters[0]),
                Utils.RemoveStringQuotes(parameters[1]));
        }

        public static void SetTimeScript(int time, string scriptFileName)
        {
            if (!GuiManager.IsTimerStarted()) return;
            _timeScriptSeconds = time;
            _timeScriptFileName = scriptFileName;
            _isTimeScriptSet = true;
        }

        public static void GetGoodsNum(List<string> parameters)
        {
            Variables["$GoodsNum"] = GoodsListManager.GetGoodsNum(
                Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void GetNpcCount(List<string> parameters)
        {
            Variables["$NpcCount"] = NpcManager.GetNpcCount(int.Parse(parameters[0]),
                int.Parse(parameters[1]));
        }

        public static void LimitMana(List<string> parameters)
        {
            if (IsPlayerNull()) return;
            Globals.ThePlayer.ManaLimit = (int.Parse(parameters[0]) != 0);
        }

        public static void NpcAttack(List<string> parameters, object belongObject)
        {
            Character target;
            Vector2 value;
            GetTargetAndValue3(parameters, belongObject, out target, out value);
            if (target != null)
            {
                target.PerformeAttack(MapBase.ToPixelPosition(value));
            }
        }

        public static void FollowNpc(List<string> parameters, object belongObject)
        {
            var character = belongObject as Character;
            Character target = null;
            if (parameters.Count == 1)
            {
                target = NpcManager.GetNpc(Utils.RemoveStringQuotes(parameters[0]));
            }
            else if (parameters.Count == 2)
            {
                character = GetPlayerOrNpc(Utils.RemoveStringQuotes(parameters[0]));
                target = GetPlayerOrNpc(Utils.RemoveStringQuotes(parameters[1]));
            }

            if (character == null || target == null) return;
            character.Follow(target);
        }

        public static void NpcSpecialAction(List<string> parameters, object belongObject)
        {
            Character target;
            string value;
            GetTargetAndValue2(parameters, belongObject, out target, out value);
            if (target != null)
            {
                target.SetSpecialAction(value);
            }
        }

        public static void NpcSpecialActionEx(List<string> parameters, object belongObject)
        {
            Character target;
            string value;
            GetTargetAndValue2(parameters, belongObject, out target, out value);
            if (target != null)
            {
                target.SetSpecialAction(value);
                Globals.IsInputDisabled = true;
            }
        }

        public static bool IsNpcSpecialActionExEnd(List<string> parameters, object belongObject)
        {
            Character target;
            string value;
            GetTargetAndValue2(parameters, belongObject, out target, out value);
            if (target != null && target.IsInSpecialAction)
            {
                return false;
            }
            Globals.IsInputDisabled = false;
            return true;
        }

        public static void LoadGame(List<string> parameters)
        {
            Loader.LoadGame(int.Parse(parameters[0]));
        }

        public static void PlayerChange(List<string> parameters)
        {
            Loader.ChangePlayer(int.Parse(parameters[0]));
        }

        public static void ReturnToTitle()
        {
            ScriptManager.ClearParallelScript();
            GuiManager.ShowTitle();
            GameState.State = GameState.StateType.Title;
        }

        public static void GetPartnerIdx(List<string> parameters)
        {
            var variable = parameters[0];
            var partners = NpcManager.GetAllPartner();
            var value = 0;
            if (partners.Count > 0)
            {
                value = PartnerList.GetIndex(partners[0].Name);
            }

            Variables[variable] = value;
        }


        public static void GetPlayerExp(List<string> parameters)
        {
            var variable = parameters[0];
            Variables[variable] = Globals.ThePlayer.Exp;
        }

        public static void ClearAllVar(List<string> parameters)
        {
            var keeps = new Dictionary<string, int>();
            foreach (var key in parameters)
            {
                if (Variables.ContainsKey(key))
                {
                    keeps[key] = Variables[key];
                }
            }
            Variables.Clear();
            foreach (var kv in keeps)
            {
                Variables[kv.Key] = kv.Value;
            }
        }

        public static void SetDropIni(List<string> parameters)
        {
            Character target = null;
            var fileName = "";
            target = GetPlayerOrNpc(Utils.RemoveStringQuotes(parameters[0]));
            fileName = Utils.RemoveStringQuotes(parameters[1]);
            if (target != null)
            {
                target.DropIni = fileName;
            }
        }

        public static void ClearAllSave(List<string> parameters)
        {
            StorageBase.DeleteAllSaveData();
        }

        public static void EnableSave(List<string> parameters)
        {
            Globals.IsSaveDisabled = false;
        }

        public static void DisableSave(List<string> parameters)
        {
            Globals.IsSaveDisabled = true;
        }

        public static void CheckFreeGoodsSpace(List<string> parameters)
        {
            Variables[parameters[0]] = GoodsListManager.HasFreeItemSpace() ? 1 : 0;
        }

        public static void CheckFreeMagicSpace(List<string> parameters)
        {
            Variables[parameters[0]] = MagicListManager.GetFreeIndex() == -1 ? 0 : 1;
        }

        public static void GetPlayerState(List<string> parameters)
        {
            if (parameters.Count == 2)
            {
                var value = 0;
                switch (parameters[0])
                {
                    case "Level":
                        value = Globals.ThePlayer.Level;
                        break;
                    case "Attack":
                        value = Globals.ThePlayer.Attack;
                        break;
                    case "Defend":
                        value = Globals.ThePlayer.Defend;
                        break;
                    case "Evade":
                        value = Globals.ThePlayer.Evade;
                        break;
                    case "Life":
                        value = Globals.ThePlayer.Life;
                        break;
                    case "Thew":
                        value = Globals.ThePlayer.Thew;
                        break;
                    case "Mana":
                        value = Globals.ThePlayer.Mana;
                        break;
                }
                Variables[parameters[1]] = value;
            }
        }

        public static void GetPlayerMagicLevel(List<string> parameters)
        {
            if (parameters.Count == 2)
            {
                Variables[parameters[1]] = MagicListManager.GetMagicLevel(Utils.RemoveStringQuotes(parameters[0]));
            }
        }

        public static void EnabelDrop(List<string> parameters)
        {
            Globals.IsDropGoodWhenDefeatEnemyDisabled = false;
        }

        public static void DisableDrop(List<string> parameters)
        {
            Globals.IsDropGoodWhenDefeatEnemyDisabled = true;
        }

        public static void ClearGoods(List<string> parameters)
        {
            GoodsListManager.ClearAllGoods(Globals.ThePlayer);
        }

        public static void ClearMagic(List<string> parameters)
        {
            MagicListManager.ClearLearnedMagic(Globals.ThePlayer);
        }

        public static void AddMoveSpeedPercent(List<string> parameters)
        {
            if (Globals.ThePlayer != null)
            {
                Globals.ThePlayer.AddMoveSpeedPercent += int.Parse(parameters[0]);
            }
        }

        public static void UseMagic(List<string> parameters)
        {
            var magicFileName = Utils.RemoveStringQuotes(parameters[0]);
            var mapX = 0;
            var mapY = 0;
            if (parameters.Count >= 3)
            {
                mapX = int.Parse(parameters[1]);
                mapY = int.Parse(parameters[2]);
            }
            else
            {
                var dest = PathFinder.FindAllNeighbors(Globals.ThePlayer.TilePosition)[Globals.ThePlayer.CurrentDirection];
                mapX = (int)dest.X;
                mapY = (int)dest.Y;
            }
            var magicInfo = MagicListManager.GetMagic(magicFileName);
            if (magicInfo != null)
            {
                Globals.ThePlayer.UseMagic(magicInfo.TheMagic, new Vector2(mapX, mapY));
            }
        }

        public static void IsEquipWeapon(List<string> parameters)
        {
            Variables[parameters[0]] = GoodsListManager.Get(205) == null ? 0 : 1;
        }

        public static void AddAttack(List<string> parameters)
        {
            var type = 1;
            if (parameters.Count == 2)
            {
                type = int.Parse(parameters[1]);
            }
            Globals.ThePlayer.AddAttack(int.Parse(parameters[0]), type);
        }

        public static void AddDefend(List<string> parameters)
        {
            var type = 1;
            if (parameters.Count == 2)
            {
                type = int.Parse(parameters[1]);
            }
            Globals.ThePlayer.AddDefend(int.Parse(parameters[0]), type);
        }

        public static void AddEvade(List<string> parameters)
        {
            Globals.ThePlayer.AddEvade(int.Parse(parameters[0]));
        }

        public static void AddLifeMax(List<string> parameters)
        {
            Globals.ThePlayer.AddLifeMax(int.Parse(parameters[0]));
        }

        public static void AddManaMax(List<string> parameters)
        {
            Globals.ThePlayer.AddManaMax(int.Parse(parameters[0]));
        }

        public static void AddThewMax(List<string> parameters)
        {
            Globals.ThePlayer.AddThewMax(int.Parse(parameters[0]));
        }

        public static void DelMagic(List<string> parameters)
        {
            MagicListManager.DelMagic(Utils.RemoveStringQuotes(parameters[0]), Globals.ThePlayer);
        }
    }
}