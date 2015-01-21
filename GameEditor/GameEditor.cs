using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Engine;

namespace GameEditor
{
    public partial class GameEditor : Form
    {
        public JxqyGame TheJxqyGame;
        public GameEditor()
        {
            InitializeComponent();
            FunctionRunStateAppendLine("[时间]\t[函数]\t[行数]");
        }

        private void GameEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            TheJxqyGame.Exit();
        }

        private void GameEditor_Activated(object sender, EventArgs e)
        {
            if(TheJxqyGame == null) return;
            TheJxqyGame.IsPaused = false;
        }

        private void GameEditor_Deactivate(object sender, EventArgs e)
        {
            if (TheJxqyGame == null) return;
            TheJxqyGame.IsPaused = true;
        }

        private void DrawSurface_MouseEnter(object sender, EventArgs e)
        {
            Cursor.Hide();
            DrawSurface.Select();
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
                var lines = File.ReadAllLines(path, Globals.LocalEncoding);
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

        private void _allEnemyDie_Click(object sender, EventArgs e)
        {
            NpcManager.AllEnemyDie();
        }
    }
}
