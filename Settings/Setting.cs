using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Engine;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework.Graphics;

namespace Settings
{
    public partial class Setting : Form
    {
        private List<DisplayMode> _collection;
        public Setting()
        {
            InitializeComponent();

            var width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            var height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            var windowMode = false;
            if (File.Exists(Globals.GameIniFilePath))
            {
                try
                {
                    var data = new FileIniDataParser().ReadFile(
                        Globals.GameIniFilePath, Globals.LocalEncoding);
                    var section = data[Globals.SettingSectionName];
                    var w = int.Parse(section["Width"]);
                    var h = int.Parse(section["Height"]);
                    var wm = int.Parse(section["FullScreen"]) == 0;
                    width = w;
                    height = h;
                    windowMode = wm;
                }
                catch (Exception)
                {
                    //do nothing
                }
            }

            _windowMode.Checked = windowMode;

            _collection = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.ToList();
            for (var i = 0; i < _collection.Count; i++)
            {
                var mode = _collection[i];
                _resolutionList.Items.Add(mode.Width + " x " + mode.Height);
                if (mode.Width == width && mode.Height == height)
                {
                    _resolutionList.SelectedIndex = i;
                }
            }
        }

        private void _saveButton_Click(object sender, EventArgs e)
        {
            IniData data;
            if (!File.Exists(Globals.GameIniFilePath))
            {
                data = new IniData();
            }
            else
            {
                data = new FileIniDataParser().ReadFile(Globals.GameIniFilePath, Globals.LocalEncoding);
            }
            var section = data[Globals.SettingSectionName];
            if (section == null)
            {
                data.Sections.AddSection(Globals.SettingSectionName);
                section = data[Globals.SettingSectionName];
            }

            var mode = _collection[_resolutionList.SelectedIndex];
            section["Width"] = mode.Width.ToString();
            section["Height"] = mode.Height.ToString();
            section["FullScreen"] = _windowMode.Checked ? "0" : "1";

            File.WriteAllText(Globals.GameIniFilePath, data.ToString(), Globals.LocalEncoding);
        }

        private void _beginGameButton_Click(object sender, EventArgs e)
        {
            Process.Start("Jxqy.exe");
            Close();
        }
    }
}
