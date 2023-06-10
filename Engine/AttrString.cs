using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Engine
{
    public struct AttrString
    {
        private struct RandItemInfo
        {
            public string Value;
            public float Weight;

            public RandItemInfo(string value, float weight)
            {
                Value = value;
                Weight = weight;
            }

            public override string ToString()
            {
                if (float.IsNaN(Weight))
                {
                    return Value;
                }
                else
                {
                    return string.Format("{0}[{1}]", Value, Weight);
                }
            }
        }

        private string _value;
        private bool _isRand;
        private List<RandItemInfo> _randsValue;
        private List<float> _probabilitys;

        public AttrString(string str)
        {
            _randsValue = new List<RandItemInfo>();
            _probabilitys = new List<float>();
            if (str.Contains(","))
            {
                _isRand = true;
                _value = "";
                var items = str.Split(',');
                try
                {
                    foreach (var i in items)
                    {
                        var item = i;
                        var weight = float.NaN;
                        if (i.EndsWith("]"))
                        {
                            var start = i.LastIndexOf("[", StringComparison.Ordinal);
                            item = i.Substring(0, start);
                            weight = int.Parse(i.Substring(start + 1, i.Length - start - 2));
                        }
                        _randsValue.Add(new RandItemInfo(item, weight));
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("解析出错，请确认格式是否正确：" + str, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                _isRand = false;
                _value = str;
            }

            var total = 0f;
            foreach (var r in _randsValue)
            {
                total += float.IsNaN(r.Weight) ? 1 : r.Weight;
            }

            var p = 0f;
            for (var i = 0; i < _randsValue.Count; i++)
            {
                p += (float.IsNaN(_randsValue[i].Weight) ? 1 : _randsValue[i].Weight) / total;
                _probabilitys.Add(p);
            }
        }

        public bool IsRand()
        {
            return _isRand;
        }

        public string GetOneValue()
        {
            if (_isRand)
            {
                if (_randsValue.Count > 0)
                {
                    var rand = Globals.TheRandom.NextDouble();
                    for (var i = 0; i < _randsValue.Count; i++)
                    {
                        if (_probabilitys[i] >= rand)
                        {
                            return _randsValue[i].Value;
                        }
                    }
                }

                return "";
            }
            else
            {
                return _value;
            }
        }

        public string GetValue()
        {
            if (_isRand)
            {
                MessageBox.Show("属性是随机的：" + GetString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return _value;
        }

        public string GetString()
        {
            if (_isRand)
            {
                if (_randsValue.Count > 0)
                {
                    return string.Join(",", _randsValue);
                }
            }
            return _value;
        }
    }
}
