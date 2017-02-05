using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Engine.Script
{
    public class ScriptParser
    {
        private Code[] _codes;
        private string[] _lines;
        
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public bool IsOk { private set; get; }

        public Code[] Codes
        {
            get { return _codes; }
        }

        public ScriptParser() { }

        public ScriptParser(string filePath)
        {
            ReadFile(filePath);
        }

        private static readonly Regex RegGoto = new Regex(@"^@([a-zA-Z0-9]+):");
        private static readonly Regex RegComment = new Regex(@"^//.*");
        private static readonly Regex RegFunction = new Regex(@"^([a-zA-Z]+)(.*);*");
        private static readonly Regex RegParameter = new Regex(@"^\((.+)\)(.*)");
        private static readonly Regex RegResult = new Regex(@"^@[a-zA-Z0-9]+");

        /// <summary>
        /// Line string to Code
        /// </summary>
        /// <param name="lineNumber">Line index</param>
        /// <param name="findGoto">Just find goto.</param>
        /// <returns></returns>
        private Code ParserLine(int lineNumber, bool findGoto)
        {
            var line = _lines[lineNumber];
            var code = new Code { LineNumber = lineNumber+1, Literal = line};
            line = line.Trim();
            if (line.Length < 2) return code;

            var isGoto = RegGoto.IsMatch(line);
            if (isGoto || findGoto)
            {
                if (!isGoto) return null;
                var match = RegGoto.Match(line);
                code.IsGoto = true;
                code.Name = match.Value;
            }
            else if (RegComment.IsMatch(line))
            {
                //do nothing
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

            return code;
        }

        public Code GetCode(int lineIndex, bool findGoto = false)
        {
            return _codes[lineIndex] ?? (_codes[lineIndex] = ParserLine(lineIndex, findGoto));
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

        public bool ReadFile(string filePath)
        {
            IsOk = false;
            FilePath = filePath;
            try
            {
                FileName = Path.GetFileName(filePath);
                ReadFromLines(File.ReadAllLines(filePath, Globals.LocalEncoding));
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Script", filePath, exception);
                return false;
            }
            return IsOk;
        }

        private bool ReadFromLines(string[] lines)
        {
            _codes = new Code[lines.Count()];
            _lines = lines;
            IsOk = true;
            return true;
        }

        /// <summary>
        /// Used only for debug purpose.
        /// </summary>
        /// <param name="lines">Script contents.</param>
        /// <param name="filePath">Script file path to set.</param>
        /// <returns>Return ture if read success.Otherwise false.</returns>
        public bool ReadFromLines(string[] lines, string filePath)
        {
            try
            {
                FilePath = filePath;
                return ReadFromLines(lines);
            }
            catch (Exception)
            {
                return false;
            }
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
    }
}