using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Engine.ListManager
{
    public static class TalkTextList
    {
        public static readonly List<TalkTextDetail> List = new List<TalkTextDetail>();

        /// <summary>
        /// Initialize list from file
        /// </summary>
        public static void Initialize()
        {
            const string path = @"Content\TalkIndex.txt";
            try
            {
                var lines = File.ReadAllLines(path, Globals.LocalEncoding);
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
                        List.Add(new TalkTextDetail(index, portraitIndex, text));
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
            var count = List.Count;
            for (var i = 0; i < count; i++)
            {
                var idx = List[i].Index;
                if (idx == index)
                {
                    return List[i];
                }
                
                if (idx > index)
                {
                    break;
                }
            }
            return null;
        }

        public static LinkedList<TalkTextDetail> GetTextDetails(int from, int to)
        {
            var count = List.Count;
            var i = 0;
            var idx = -1;
            for (; i < count; i++)
            {
                idx = List[i].Index;
                if (idx >= from)
                {
                    break;
                }
            }

            if (i >= count || idx < from)
            {
                return null;
            }

            var list = new LinkedList<TalkTextDetail>();
            for (var j = i; j < count; j++)
            {
                if (List[j].Index <= to)
                {
                    list.AddLast(List[j]);
                }
                else
                {
                    break;
                }
            }
            return list;
        } 
    }
}