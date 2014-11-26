using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Engine.Gui;
using Engine.ListManager;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Script
{
    public static class ScriptExecuter
    {
        private static readonly Dictionary<string, int> Variables = new Dictionary<string, int>();
        private static float _fadeTransparence;
        private static int _talkStartIndex;
        private static int _talkEndIndex;
        private static int _talkCurrentIndex;

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

        public static bool IsInFadeOut;
        public static bool IsInFadeIn;
        public static bool IsInTalk;

        private static void GetTargetAndScript(string nameWithQuotes,
            string scriptFileNameWithQuotes,
            object belongObject,
            out Character target,
            out ScriptParser script)
        {
            GetTarget(nameWithQuotes, belongObject, out target);
            var scriptFileName = Utils.RemoveStringQuotes(scriptFileNameWithQuotes);
            script = null;
            if (!string.IsNullOrEmpty(scriptFileName))
                script = new ScriptParser(Utils.GetScriptFilePath(scriptFileName), target);
        }

        private static void GetTarget(string nameWithQuotes,
            object belongObject,
            out Character target)
        {
            var name = Utils.RemoveStringQuotes(nameWithQuotes);
            target = belongObject as Character;
            if (!string.IsNullOrEmpty(name))
            {
                if (Globals.ThePlayer.Name == name)
                    target = Globals.ThePlayer;
                else target = NpcManager.GetNpc(name);
            }
        }

        private static void GetNextTalkTextDeatil(out TalkTextDetail detail)
        {
            detail = null;
            for (; _talkCurrentIndex <= _talkEndIndex; _talkCurrentIndex++)
            {
                detail = TalkTextList.GetTextDetail(_talkCurrentIndex);
                if (detail != null)
                {
                    _talkCurrentIndex++; // Finded, move to next index
                    break;
                }
            }
        }

        public static void Update(GameTime gameTime)
        {
            if (IsInFadeOut && FadeTransparence < 1f)
            {
                FadeTransparence += 0.02f;
            }
            else if (IsInFadeIn && FadeTransparence > 0f)
            {
                FadeTransparence -= 0.02f;
                if (FadeTransparence <= 0f) IsInFadeIn = false;
            }

            if (IsInTalk)
            {
                if (GuiManager.IsDialogEnd())
                {
                    TalkTextDetail detail;
                    GetNextTalkTextDeatil(out detail);
                    if (detail != null)
                    {
                        GuiManager.ShowDialog(detail.Text, detail.PortraitIndex);
                    }
                    else
                    {
                        IsInTalk = false;
                    }
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {

        }

        public static void Say(List<string> parameters)
        {
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
        }

        private static readonly Regex IfParameterPatten = new Regex(@"(\$[a-zA-Z0-9]+) *([><=]+) *(.+)");
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
                if (Variables.ContainsKey(variable))
                {
                    switch (compare)
                    {
                        case "==":
                            return Variables[variable] == value;
                        case ">>":
                            return Variables[variable] > value;
                        case ">=":
                            return Variables[variable] >= value;
                        case "<<":
                            return Variables[variable] < value;
                        case "<=":
                            return Variables[variable] <= value;
                        case "<>":
                            return Variables[variable] != value;
                    }

                }
            }
            return false;
        }

        public static void Add(List<string> parameters)
        {
            var variable = parameters[0];
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
            return FadeTransparence >= 1f;
        }

        public static void FadeIn()
        {
            IsInFadeOut = false;
            IsInFadeIn = true;
            FadeTransparence = 1f;
        }

        public static bool IsFadeInEnd()
        {
            return !IsInFadeIn;
        }

        public static void DrawFade(SpriteBatch spriteBatch)
        {
            var fadeTextrue = TextureGenerator.GetColorTexture(
                    Color.Black * ScriptExecuter.FadeTransparence,
                    1,
                    1);
            spriteBatch.Draw(fadeTextrue,
                new Rectangle(0, 0, Globals.WindowWidth, Globals.WindowHeight),
                Color.White);
        }

        public static void DeleteNpc(List<string> parameters)
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
            if (parameters == null)
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

        public static void SetObjScript(List<string> parameters, object belongObject)
        {
            var name = Utils.RemoveStringQuotes(parameters[0]);
            var scriptFileName = Utils.RemoveStringQuotes(parameters[1]);
            var target = belongObject as Obj;
            ScriptParser script = null;
            if (!string.IsNullOrEmpty(name))
                target = ObjManager.GetObj(name);
            if (!string.IsNullOrEmpty(scriptFileName))
                script = new ScriptParser(Utils.GetScriptFilePath(scriptFileName), target);
            if (target != null)
            {
                target.ScriptFile = script;
            }
        }

        public static void SetNpcScript(List<string> parameters, object belongObject)
        {
            Character target;
            ScriptParser script;
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
            ScriptParser script;
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
            var target = belongObject as Character;
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
            var x = int.Parse(parameters[0]);
            var y = int.Parse(parameters[1]);
            Globals.ThePlayer.TilePosition = new Vector2(x, y);
        }

        public static void SetPlayerDir(List<string> parameters)
        {
            Globals.ThePlayer.SetDirection(int.Parse(parameters[0]));
        }

        public static void LoadMap(List<string> parameters)
        {
            Globals.TheMap.LoadMap(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void LoadNpc(List<string> parameters)
        {
            NpcManager.Load(Utils.RemoveStringQuotes(parameters[0]));
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
            else
            {
                GuiManager.ShowMessage("错误");
            }
            GuiManager.UpdateGoodsView();
        }

        public static void AddRandGoods(List<string> parameters)
        {
            var fileName = GetRandItem(@"ini\buy\" + Utils.RemoveStringQuotes(parameters[0]));
            if(string.IsNullOrEmpty(fileName)) return;
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
            int index;
            Magic magic;
            var result = MagicListManager.AddMagicToList(
                Utils.RemoveStringQuotes(parameters[0]),
                out index,
                out magic);
            if (result)
            {
                GuiManager.ShowMessage("你学会了" + magic.Name);
                GuiManager.UpdateMagicView();
            }
            else
            {
                if (magic != null)
                {
                    GuiManager.ShowMessage("你已经学会了" + magic.Name);
                }
                else
                {
                    GuiManager.ShowMessage("错误");
                }
            }
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
                int.Parse(parameters[3]));
        }

        public static void Talk(List<string> parameters)
        {
            IsInTalk = true;
            _talkStartIndex = int.Parse(parameters[0]);
            _talkEndIndex = int.Parse(parameters[1]);
            _talkCurrentIndex = _talkStartIndex;
            TalkTextDetail detail;
            GetNextTalkTextDeatil(out detail);
            if (detail != null)
            {
                GuiManager.ShowDialog(detail.Text, detail.PortraitIndex);
            }
            else
            {
                IsInTalk = false;
            }
        }

        public static void Memo(List<string> parameters)
        {
            GuiManager.AddMemo(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void AddToMemo(List<string> parameters)
        {
            var detail = TalkTextList.GetTextDetail(int.Parse(parameters[0]));
            if(detail == null) return;
            GuiManager.AddMemo(detail.Text);
        }
    }
}