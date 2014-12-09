using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Jxqy;

namespace GameEditor
{
    public partial class GameEditor : Form
    {
        public Game TheGame;
        public GameEditor()
        {
            InitializeComponent();
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
    }
}
