using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Audio;

namespace Engine
{
    static public class Utils
    {
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
        static public string[] GetNameValue(string nameValue)
        {
            var result = new String[2];
            var groups = Regex.Match(nameValue, "(.+)=(.+)").Groups;
            if (groups[0].Success)
            {
                result[0] = groups[1].Value;
                result[1] = groups[2].Value;
            }
            return result;
        }

        static public Asf GetAsf(string path)
        {
            if (Globals.AsfFiles.ContainsKey(path))
                return Globals.AsfFiles[path];
            else
            {
                var asf = new Asf(path);
                if (asf.IsOk)
                {
                    Globals.AsfFiles[path] = asf;
                    return asf;
                }
                else
                {
                    return null;
                }
            }
        }

        static public SoundEffect GetSoundEffect(string assertName)
        {
            try
            {
                return Globals.TheGame.Content.Load<SoundEffect>(assertName);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
