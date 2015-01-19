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

            Globals.LoadSetting();

            _windowMode.Checked = !Globals.IsFullScreen;

            _collection = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.ToList();
            for (var i = 0; i < _collection.Count; i++)
            {
                var mode = _collection[i];
                _resolutionList.Items.Add(mode.Width + " x " + mode.Height);
                if (mode.Width == Globals.WindowWidth && 
                    mode.Height == Globals.WindowHeight)
                {
                    _resolutionList.SelectedIndex = i;
                }
            }
        }

        private void _saveButton_Click(object sender, EventArgs e)
        {
            var mode = _collection[_resolutionList.SelectedIndex];

            Globals.WindowWidth =  mode.Width;
            Globals.WindowHeight = mode.Height;
            Globals.IsFullScreen = !_windowMode.Checked;

            Globals.SaveAllSetting();
        }

        private void _beginGameButton_Click(object sender, EventArgs e)
        {
            Process.Start("Jxqy.exe");
            Close();
        }
    }
}
