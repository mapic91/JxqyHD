using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Engine.Map;
using Engine.Script;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Engine
{
    static public class Utils
    {
        public static Dictionary<string, Asf> AsfFiles = new Dictionary<string, Asf>();
        public static Dictionary<KeyValuePair<string, string>, Mpc> MpcFiles = new Dictionary<KeyValuePair<string, string>, Mpc>();
        public static Dictionary<string, Magic> Magics = new Dictionary<string, Magic>();
        public static Dictionary<string, Good> Goods = new Dictionary<string, Good>();

        //static public int GetBigEndianIntegerFromByteArray(byte[] data, ref int startIndex)
        //{
        //    var ret = (data[startIndex] << 24)
        //         | (data[startIndex + 1] << 16)
        //         | (data[startIndex + 2] << 8)
        //         | data[startIndex + 3];
        //    startIndex += 4;
        //    return ret;
        //}

        static public int GetLittleEndianIntegerFromByteArray(byte[] data, ref int startIndex)
        {
            var ret = (data[startIndex + 3] << 24)
                 | (data[startIndex + 2] << 16)
                 | (data[startIndex + 1] << 8)
                 | data[startIndex];
            startIndex += 4;
            return ret;
        }

        //return string[0] is name, string[1] is value
        //nameValue patten is "name=value"
        //If failed return empty string array
        static public string[]  GetNameValue(string nameValue)
        {
            var result = new String[2];
            var groups = Regex.Match(nameValue, "(.*?)=(.*)").Groups;
            if (groups[0].Success)
            {
                result[0] = groups[1].Value;
                result[1] = groups[2].Value;
            }
            return result;
        }

        static public Texture2D LoadTexture2D(string contentPath)
        {
            try
            {
                return Globals.TheGame.Content.Load<Texture2D>(contentPath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        static public Texture2D LoadTexture2DFromFile(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    return Texture2D.FromStream(Globals.TheGame.GraphicsDevice, stream);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        static public Asf GetAsf(string basePath, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            try
            {
                var path = string.IsNullOrEmpty(basePath) ? fileName : Path.Combine(basePath, fileName);
                if (!string.IsNullOrEmpty(path) && path[0] == '\\')
                {
                    path = path.Substring(1);
                }
                if (AsfFiles.ContainsKey(path))
                    return AsfFiles[path];
                else
                {
                    var asf = new Asf(path);
                    if (asf.IsOk)
                    {
                        AsfFiles[path] = asf;
                        return asf;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        static public Mpc GetMpc(string basePath, string fileName, string shdFileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            try
            {
                var path = string.IsNullOrEmpty(basePath) ? fileName : Path.Combine(basePath, fileName);
                var pair = new KeyValuePair<string, string>(path, shdFileName);
                if (MpcFiles.ContainsKey(pair))
                    return MpcFiles[pair];
                else
                {
                    var mpc = new Mpc(path, shdFileName);
                    if (mpc.IsOk)
                    {
                        MpcFiles[pair] = mpc;
                        return mpc;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        static public void ClearTextureCache()
        {
            AsfFiles.Clear();
            MpcFiles.Clear();
        }

        static public Asf GetCharacterAsf(string fileName)
        {
            return GetAsf(ResFile.GetAsfFilePathBase(fileName, ResType.Npc),
                fileName);
        }

        static public SoundEffect GetSoundEffect(string wavFileName)
        {
            if (string.IsNullOrEmpty(wavFileName)) return null;
            try
            {
                var path = @"sound\" + Path.GetFileNameWithoutExtension(wavFileName);
                return Globals.TheGame.Content.Load<SoundEffect>(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        static public Video GetVideo(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            try
            {
                var path = @"video\" + Path.GetFileNameWithoutExtension(fileName);
                return Globals.TheGame.Content.Load<Video>(path);
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Movie", fileName, exception);
                return null;
            }
        }

        /// <summary>
        /// Get Magic from file name
        /// </summary>
        /// <param name="fileName">Magic file name</param>
        /// <param name="shared">Use shared cached <see cref="Magic"/> class if true.Otherwise use new <see cref="Magic"/> class</param>
        /// <returns>Magic class</returns>
        static public Magic GetMagic(string fileName, bool shared = true)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            try
            {
                var filePath = @"ini\magic\" + fileName;
                if (shared && Magics.ContainsKey(filePath))
                    return Magics[filePath];
                else
                {
                    var magic = new Magic(filePath);
                    if (magic.IsOk)
                    {
                        if (shared) Magics[filePath] = magic;
                        return magic;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public static Good GetGood(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            try
            {
                var filePath = @"ini\goods\" + fileName;
                if (Goods.ContainsKey(filePath))
                    return Goods[filePath];
                else
                {
                    var good = new Good(filePath);
                    if (good.IsOk)
                    {
                        Goods[filePath] = good;
                        return good;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public class LevelDetail
        {
            public int LevelUpExp;
            public int LifeMax;
            public int ThewMax;
            public int ManaMax;
            public int Attack;
            public int Attack2;
            public int Defend;
            public int Defend2;
            public int Evade;
            public string NewMagic;
            public string NewGood;
            public int Exp;
            public int Life;
        }
        private static readonly Dictionary<int, Dictionary<int, LevelDetail>> LevelList =
            new Dictionary<int, Dictionary<int, LevelDetail>>();
        public static Dictionary<int, LevelDetail> GetLevelLists(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            var hashCode = filePath.GetHashCode();
            if (LevelList.ContainsKey(hashCode))
                return LevelList[hashCode];
            var lists = new Dictionary<int, LevelDetail>();

            try
            {
                var lines = File.ReadAllLines(filePath, Globals.LocalEncoding);
                var counts = lines.Length;
                for (var i = 0; i < counts; )
                {
                    var groups = Regex.Match(lines[i], @"\[Level([0-9]+)\]").Groups;
                    i++;
                    if (groups[0].Success)
                    {
                        var detail = new LevelDetail();
                        var index = int.Parse(groups[1].Value);
                        while (i < counts && !string.IsNullOrEmpty(lines[i]))
                        {
                            var nameValue = GetNameValue(lines[i]);
                            int value;
                            int.TryParse(nameValue[1], out value);
                            switch (nameValue[0])
                            {
                                case "Exp":
                                    detail.Exp = value;
                                    break;
                                case "LevelUpExp":
                                    detail.LevelUpExp = value;
                                    break;
                                case "LifeMax":
                                    detail.LifeMax = value;
                                    break;
                                case "Life":
                                    detail.Life = value;
                                    break;
                                case "ThewMax":
                                    detail.ThewMax = value;
                                    break;
                                case "ManaMax":
                                    detail.ManaMax = value;
                                    break;
                                case "Attack":
                                    detail.Attack = value;
                                    break;
                                case "Attack2":
                                    detail.Attack2 = value;
                                    break;
                                case "Defend":
                                    detail.Defend = value;
                                    break;
                                case "Defend2":
                                    detail.Defend2 = value;
                                    break;
                                case "Evade":
                                    detail.Evade = value;
                                    break;
                                case "NewMagic":
                                    detail.NewMagic = nameValue[1];
                                    break;
                                case "NewGood":
                                    detail.NewGood = nameValue[1];
                                    break;
                            }
                            i++;
                        }
                        lists[index] = detail;
                    }
                }

            }
            catch (Exception)
            {
                return lists;
            }

            LevelList[hashCode] = lists;
            return lists;
        }

        //axes:x point to (1,0), y point to is (1,0) in game axes
        //direction: 0-31 clockwise, 0 point to (0,1)
        private static List<Vector2> _direction32List;
        public static Vector2 GetDirection32(int direction)
        {
            if (direction < 0 || direction > 31)
                direction = 0;
            return GetDirection32List()[direction];
        }

        public static List<Vector2> GetDirection32List()
        {
            if (_direction32List == null)
            {
                _direction32List = new List<Vector2>();
                var angle = Math.PI * 2 / 32;
                for (var i = 0; i < 32; i++)
                {
                    _direction32List.Add(new Vector2((float)-Math.Sin(angle * i), (float)Math.Cos(angle * i))); ;
                }
            }
            return _direction32List;
        }

        //axes:x point to (1,0), y point to is (1,0) in game axes
        //direction: 0-7 clockwise, 0 point to (0,1)
        private static List<Vector2> _direction8List;
        public static Vector2 GetDirection8(int direction)
        {
            if (direction < 0 || direction > 7)
                direction = 0;
            return GetDirection8List()[direction];
        }

        public static List<Vector2> GetDirection8List()
        {
            if (_direction8List == null)
            {
                _direction8List = new List<Vector2>()
                {
                    new Vector2(0, 1),//0
                    new Vector2(-1, 1),//1
                    new Vector2(-1, 0),//2
                    new Vector2(-1, -1),//3
                    new Vector2(0, -1),//4
                    new Vector2(1, -1),//5
                    new Vector2(1, 0),//6
                    new Vector2(1, 1)//7
                };
                //Normalize
                for (var i = 0; i < 8; i++)
                {
                    var dir = _direction8List[i];
                    dir.Normalize();
                    _direction8List[i] = dir;
                }
            }
            return _direction8List;
        }

        //Please see ../Helper/SetDirection.jpg
        public static int GetDirectionIndex(Vector2 direction, int directionCount)
        {
            if (direction == Vector2.Zero || directionCount < 1) return 0;
            const double twoPi = Math.PI * 2;
            direction.Normalize();
            var angle = Math.Acos(Vector2.Dot(direction, new Vector2(0, 1)));
            if (direction.X > 0) angle = twoPi - angle;

            // 2*PI/2*directionCount
            var halfAnglePerDirection = Math.PI / directionCount;
            var region = (int)(angle / halfAnglePerDirection);
            if (region % 2 != 0) region++;
            region %= 2 * directionCount;
            return region / 2;
        }

        /// <summary>
        /// Get mouse state just position, button is released, wheel scroll is 0
        /// </summary>
        /// <param name="mouseState">The mouse state</param>
        /// <returns></returns>
        public static MouseState GetMouseStateJustPosition(MouseState mouseState)
        {
            return new MouseState(mouseState.X,
                mouseState.Y,
                0,
                ButtonState.Released,
                ButtonState.Released,
                ButtonState.Released,
                ButtonState.Released,
                ButtonState.Released);
        }

        /// <summary>
        /// Get script file path from file name.
        /// </summary>
        /// <param name="fileName">Script file name</param>
        /// <param name="mapName">Map name used to get script path, use current map name if null or empty</param>
        /// <param name="category">Category of script belongs.</param>
        /// <returns></returns>
        public static string GetScriptFilePath(string fileName, string mapName = null, ScriptCategory category = ScriptCategory.Normal)
        {
            switch (category)
            {
                case ScriptCategory.Normal:
                {
                    if (string.IsNullOrEmpty(mapName))
                        mapName = MapBase.MapFileNameWithoutExtension;
                    var path = @"script\map\" + mapName + @"\" + fileName;
                    if (!File.Exists(path))
                    {
                        return @"script\common\" + fileName;
                    }
                    return path;
                }
                case ScriptCategory.Good:
                    return @"script\goods\" + fileName;
                default:
                    throw new ArgumentOutOfRangeException("category");
            }
        }

        public enum ScriptCategory
        {
            Normal,
            Good
        }

        private static Dictionary<string, ScriptParser> _scriptParserCache = new Dictionary<string, ScriptParser>();
        private static Dictionary<string, DateTime> _scriptFileLastWriteTime = new Dictionary<string, DateTime>(); 

        /// <summary>
        /// Get ScriptParser from file name.
        /// </summary>
        /// <param name="fileName">Script file name</param>
        /// <param name="mapName">Map name used to get script path, use current map name if null or empty</param>
        /// <param name="category">Category of script belongs.</param>
        /// <returns></returns>
        public static ScriptParser GetScriptParser(string fileName, string mapName = null, ScriptCategory category = ScriptCategory.Normal)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            return GetScriptParserFromPath(GetScriptFilePath(fileName, mapName, category));
        }

        public static ScriptParser GetScriptParserFromPath(string filePath)
        {
            if (!Globals.TheGame.IsInEditMode ||
                (_scriptFileLastWriteTime.ContainsKey(filePath) &&
                 _scriptFileLastWriteTime[filePath].Equals(File.GetLastWriteTime(filePath))))
            {
                //Use cached script parser:
                // *In play mode or
                // *If file is cached and not modified in edit mode.
                if (_scriptParserCache.ContainsKey(filePath))
                {
                    return _scriptParserCache[filePath];
                }
            }
            else if (Globals.TheGame.IsInEditMode)
            {
                try
                {
                    // Update last write time
                    _scriptFileLastWriteTime[filePath] = File.GetLastWriteTime(filePath);
                }
                catch (Exception)
                {

                }
            }

            //No script parser in cache, create new and add to cache.
            var parser = new ScriptParser(filePath);
            _scriptParserCache[filePath] = parser;
            return parser;
        }

        public static void ClearScriptParserCache()
        {
            _scriptParserCache.Clear();
            _scriptFileLastWriteTime.Clear();
        }

        public static string RemoveStringQuotes(string str)
        {
            try
            {
                return str.Substring(1, str.Length - 2);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetNpcObjFilePath(string fileName)
        {
            fileName = fileName == null ? "" : fileName;
            var path = @"save\game\" + fileName;
            if (!File.Exists(path))
            {
                path = @"ini\save\" + fileName;
            }
            return path;
        }

        /// <summary>
        /// Splite long string to short string by character count
        /// </summary>
        /// <param name="text">Long string</param>
        /// <param name="charCount">Character count</param>
        /// <returns>Splited strings</returns>
        public static List<string> SpliteStringInCharCount(string text, int charCount)
        {
            var lines = new List<string>();
            if (string.IsNullOrEmpty(text) || charCount < 1) return lines;
            while (text.Length > charCount)
            {
                lines.Add(text.Substring(0, charCount));
                text = text.Substring(charCount);
            }
            lines.Add(text);
            return lines;
        }

        public static bool EqualNoCase(string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get all KeyDataCollection from Npc Obj list file
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="sectionNameBegin">Npc is "NPC", Obj is "OBJ"</param>
        /// <returns></returns>
        public static List<KeyDataCollection> GetAllKeyDataCollection(string filePath, string sectionNameBegin)
        {
            var list = new List<KeyDataCollection>();
            var data = new FileIniDataParser().ReadFile(filePath, Globals.LocalEncoding);
            var count = int.Parse(data["Head"]["Count"]);
            for (var i = 0; i < count; i++)
            {
                list.Add(data[sectionNameBegin + string.Format("{0:000}", i)]);
            }
            return list;
        }

        public static KeyDataCollection GetFirstSection(IniData data)
        {
            return data.Sections.Select(section => section.Keys).FirstOrDefault();
        }

        public static int GetCharacterDeathExp(Character theKiller, Character theDead)
        {
            if (theDead == null || theKiller == null) return 1;
            var exp = theKiller.Level*theDead.Level + theDead.ExpBonus;
            return exp < 4 ? 4 : exp;
        }

        private static readonly Dictionary<int, int> MagicExp = new Dictionary<int, int>();
        public static float XiuLianMagicExpFraction;
        public static float UseMagicExpFraction;

        public static void LoadMagicExpList()
        {
            const string path = @"ini\level\MagicExp.ini";
            try
            {
                var data = new FileIniDataParser().ReadFile(path, Globals.LocalEncoding);
                var section = data["Exp"];
                foreach (var keyData in section)
                {
                    MagicExp[int.Parse(keyData.KeyName)] = int.Parse(keyData.Value);
                }
                XiuLianMagicExpFraction = float.Parse(data["XiuLianMagicExp"]["Fraction"]);
                UseMagicExpFraction = float.Parse(data["UseMagicExp"]["Fraction"]);
            }
            catch (Exception e)
            {
                Log.LogFileLoadError("MagicExp.ini", path, e);
            }
        }

        public static int GetMagicExp(int hitedCharacterLevel)
        {
            return MagicExp.ContainsKey(hitedCharacterLevel) ? MagicExp[hitedCharacterLevel] : MagicExp.Last().Value;
        }

        private static readonly Regex RegColor = new Regex(@"^([0-9]*),([0-9]*),([0-9]*),([0-9]*)");
        /// <summary>
        /// Cast r,g,b color string to Color.
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns>Return casted color, if someting wrong at the rgb string, black is returned.</returns>
        public static Color GetColor(string rgb)
        {
            Color color = Color.Black;
            if (RegColor.IsMatch(rgb))
            {
                var matchs = RegColor.Match(rgb);

                if (matchs.Groups.Count == 5)
                {
                    var r = matchs.Groups[1].Value;
                    var g = matchs.Groups[2].Value;
                    var b = matchs.Groups[3].Value;
                    var a = matchs.Groups[4].Value;
                    int rv = 0, gv = 0, bv = 0, av = 0;
                    int.TryParse(r, out rv);
                    int.TryParse(g, out gv);
                    int.TryParse(b, out bv);
                    int.TryParse(a, out av);
                    color = new Color(rv, gv, bv) * (av/255f);
                }
            }
            return color;
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
    }
}
