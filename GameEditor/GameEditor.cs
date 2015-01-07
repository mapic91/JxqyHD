using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Engine;
using Jxqy;

namespace GameEditor
{
    public partial class GameEditor : Form
    {
        public Game TheGame;
        public GameEditor()
        {
            InitializeComponent();
            FunctionRunStateAppendLine("[时间]\t[函数]\t[行数]");
        }

        private void GameEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            TheGame.Exit();
        }

        private void GameEditor_Activated(object sender, EventArgs e)
        {
            if(TheGame == null) return;
            TheGame.IsPaused = false;
        }

        private void GameEditor_Deactivate(object sender, EventArgs e)
        {
            if (TheGame == null) return;
            TheGame.IsPaused = true;
        }

        private void DrawSurface_MouseEnter(object sender, EventArgs e)
        {
            Cursor.Hide();
        }

        private void DrawSurface_MouseLeave(object sender, EventArgs e)
        {
            Cursor.Show();
        }

        public void FunctionRunStateAppendLine(string line)
        {
            _functionText.AppendText(line + Environment.NewLine);
        }

        public void SetScriptFileContent(string path)
        {
            SetScriptFilePath(path);

            var contnet = new StringBuilder();
            var filePathInfo = "【" + path + "】";
            try
            {
                var lines = File.ReadAllLines(path, Globals.SimpleChineseEncoding);
                var count = lines.Count();
                for (var i = 0; i < count; i++)
                {
                    contnet.AppendLine((i + 1) + "  " + lines[i]);
                }
            }
            catch (Exception)
            {
                _fileText.Text = (filePathInfo + "  读取失败！");
                return;
            }
            _fileText.Text = contnet.ToString();
        }

        private void SetScriptFilePath(string path)
        {
            _scriptFilePath.Text = path;
            TheToolTip.SetToolTip(_scriptFilePath, path);
        }

        private void _scriptFilePath_Click(object sender, EventArgs e)
        {
            var path = _scriptFilePath.Text;
            if (File.Exists(path))
            {
                Process.Start("explorer", '"' + path + '"');
            }
        }

        private void _fullLifeThewMana_Click(object sender, EventArgs e)
        {
            Globals.ThePlayer.FullLife();
            Globals.ThePlayer.FullMana();
            Globals.ThePlayer.FullThew();
        }

        private void _levelUp_Click(object sender, EventArgs e)
        {
            Globals.ThePlayer.LevelUp();
        }

        private void _addMoney1000_Click(object sender, EventArgs e)
        {
            Globals.ThePlayer.AddMoney(1000);
        }

        private void GameEditor_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;
        }

        private void GameEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = false;
        }

        private void GameEditor_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = false;
        }
    }
}
