using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public struct AttrInt
    {
        private int _value;
        private int _start;
        private int _end;
        private bool _isRand;
        private List<int> _randsValue;
        private string _str;

        public  AttrInt(string str)
        {
            _str = str;
            _randsValue = new List<int>();
            if (str.Contains(">"))
            {
                var ints = str.Split('>');
                _isRand = true;
                _start = int.Parse(ints[0]);
                _end = int.Parse(ints[1]);
                if (_start > _end)
                {
                    var t = _start;
                    _start = _end;
                    _end = t;
                }
                _value = 0;
            }
            else if (str.Contains(","))
            {
                var ints = str.Split(',');
                foreach (var i in ints)
                {
                    _randsValue.Add(int.Parse(i));
                }
                _isRand = true;
                _value = 0;
                _start = _value;
                _end = _value;
            }
            else
            {
                _isRand = false;
                _value = int.Parse(str);
                _start = _value;
                _end = _value;
            }
        }

        public  AttrInt(int value)
        {
            _isRand = false;
            _randsValue = new List<int>();
            _value = value;
            _start = value;
            _end = value;
            _str = "";
        }

        public bool IsRand()
        {
            return _isRand;
        }

        public int GetOneValue()
        {
            if (_isRand)
            {
                if (_randsValue.Count > 0)
                {
                    return _randsValue[Globals.TheRandom.Next(0, _randsValue.Count)];
                }
                return Globals.TheRandom.Next(_start, _end + 1);
            }
            else
            {
                return _value;
            }
        }

        public int GetMaxValue()
        {
            if (_isRand)
            {
                if (_randsValue.Count > 0)
                {
                    return _randsValue[_randsValue.Count - 1];
                }
                return _end;
            }

            return _value;
        }

        public AttrInt GetNonRandom()
        {
            return new AttrInt(GetOneValue());
        }

        public string GetString()
        {
            if (_isRand)
            {
                if (_randsValue.Count > 0)
                {
                    return string.Join(",", _randsValue);
                }

                return _start + "-" + _end;
            }
            return _value.ToString();
        }

        public string GetUIString()
        {
            if (_isRand)
            {
                if (_randsValue.Count > 0)
                {
                    return string.Join(",", _randsValue);
                }
                if (_start < 0)
                {
                    return _end.ToString("+#;-#") + ">" + Math.Abs(_start);
                }
                return _start.ToString("+#;-#") + ">" + Math.Abs(_end);
            }

            return _value.ToString("+#;-#");
        }
    }
}
