using System;
using System.Collections.Generic;
using System.IO;
using IniParser;

namespace Engine.ListManager
{
    public static class PartnerList
    {
        private static readonly Dictionary<int, string> _list = new Dictionary<int, string>();
        private const string ListFilePath = @"Content\PartnerIdx.ini";

        /// <summary>
        /// Load from file.
        /// </summary>
        public static void Load()
        {
            _list.Clear();
            try
            {
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(ListFilePath, Globals.LocalEncoding);
                var section = Utils.GetFirstSection(data);
                foreach (var keyData in section)
                {
                    _list[int.Parse(keyData.KeyName)] = keyData.Value;
                }
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Partner list", ListFilePath, exception);
            }
        }

        /// <summary>
        /// Total item count.
        /// </summary>
        /// <returns></returns>
        public static int GetCount()
        {
            return _list.Count;
        }

        /// <summary>
        /// Get the index of character named name.
        /// </summary>
        /// <param name="name">Character name.</param>
        /// <returns>The character index.If not found,total item count plus 1 will returned.</returns>
        public static int GetIndex(string name)
        {
            foreach (var key in _list.Keys)
            {
                if (_list[key] == name) return key;
            }
            return GetCount()+1;
        }

        /// <summary>
        /// Get the character name at index.
        /// </summary>
        /// <param name="index">Index in list.</param>
        /// <returns>Character name.If not found, return empty string.</returns>
        public static string GetName(int index)
        {
            if (_list.ContainsKey(index))
            {
                return _list[index];
            }

            return string.Empty;
        }
    }
}