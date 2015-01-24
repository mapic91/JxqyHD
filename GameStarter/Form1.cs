using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameStarter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CenterToScreen();
        }

        private void _startGame_Click(object sender, EventArgs e)
        {
            Process.Start("Jxqy.exe");
            Close();
        }

        private void _gameSetting_Click(object sender, EventArgs e)
        {
            Process.Start("Settings.exe");
            Close();
        }

        private void _debugGame_Click(object sender, EventArgs e)
        {
            Process.Start("GameEditor.exe");
            Close();
        }
    }
}
