using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Engine;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Settings
{
    public partial class Setting : Form
    {
        private List<DisplayMode> _collection;
        public Setting()
        {
            InitializeComponent();

            _toolTip.SetToolTip(_lblMaxMagicUnit, "限制能同时存在的最大武功单位个数\n比如一个球是一个武功单位，限制最多200\n则如果当前有200个武功单位，就不能再增加武功单位，即此时释放武功等于没释放武功");

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

            _soundEffectVolume.Value = (int)(SoundEffect.MasterVolume*100);
            _musicVolume.Value = (int) (BackgroundMusic.GetVolume()*100);
            _maxMagicUintCtl.Value = MagicManager.MaxMagicUnit;
            _runSpeed.Value = Globals.RunSpeedFold;
            UpdateLabelText();
            CenterToScreen();
        }

        private void _saveButton_Click(object sender, EventArgs e)
        {
            var mode = _collection[_resolutionList.SelectedIndex];

            Globals.WindowWidth =  mode.Width;
            Globals.WindowHeight = mode.Height;
            Globals.IsFullScreen = !_windowMode.Checked;
            try
            {
                SoundEffect.MasterVolume = _soundEffectVolume.Value / 100f;
                BackgroundMusic.SetVolume(_musicVolume.Value / 100f);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
            MagicManager.MaxMagicUnit = (int)_maxMagicUintCtl.Value;
            Globals.RunSpeedFold = _runSpeed.Value;

            Globals.SaveAllSetting();
        }

        private void _beginGameButton_Click(object sender, EventArgs e)
        {
            Process.Start("Jxqy.exe");
            Close();
        }

        private void _soundEffectVolume_Scroll(object sender, EventArgs e)
        {
            UpdateLabelText();
        }

        private void _musicVolume_Scroll(object sender, EventArgs e)
        {
            UpdateLabelText();
        }

        private void _gameSpeed_Scroll(object sender, EventArgs e)
        {
            UpdateLabelText();
        }

        private void UpdateLabelText()
        {
            _textSoundEffectVolume.Text = _soundEffectVolume.Value + "%";
            _textMusicVolume.Text = _musicVolume.Value + "%";
            _textRunSpeed.Text = _runSpeed.Value.ToString();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            UpdateLabelText();
        }

        private void _runSpeed_Scroll(object sender, EventArgs e)
        {
            UpdateLabelText();
        }
    }
}
