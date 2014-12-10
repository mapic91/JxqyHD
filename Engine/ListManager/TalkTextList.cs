using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Engine.ListManager
{
    public static class TalkTextList
    {
        public static readonly Dictionary<int, TalkTextDetail> List = new Dictionary<int, TalkTextDetail>();

        /// <summary>
        /// Initialize list from file
        /// </summary>
        public static void Initialize()
        {
            const string path = @"Content\TalkIndex.txt";
            try
            {
                var lines = File.ReadAllLines(path, Globals.SimpleChinaeseEncoding);
                var regex = new Regex(@"^\[([0-9]+),([0-9]+)\](.*)");
                foreach (var line in lines)
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        var groups = match.Groups;
                        var index = int.Parse(groups[1].Value);
                        var portraitIndex = int.Parse(groups[2].Value);
                        var text = groups[3].Value;
                        List[index] = new TalkTextDetail(portraitIndex, text);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("TalkIndex", path, exception);
            }
        }

        /// <summary>
        /// Get TextDetail from list, if index not contains return null
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>If index not contains return null</returns>
        public static TalkTextDetail GetTextDetail(int index)
        {
            return List.ContainsKey(index) ? List[index] : null;
        }
    }
}