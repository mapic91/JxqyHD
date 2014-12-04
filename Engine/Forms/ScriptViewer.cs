using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Engine.Forms
{
    public partial class ScriptViewer : Form
    {
        public ScriptViewer()
        {
            InitializeComponent();
        }

        private void ScriptViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        public void AppendLine(string line)
        {
            messageTextBox.AppendText(line + Environment.NewLine);
        }

        public void SetFileContent(string path)
        {
            var contnet = new StringBuilder();
            var filePathInfo = "【" + path + "】";
            contnet.AppendLine(filePathInfo);
            try
            {
                var lines = File.ReadAllLines(path, Globals.SimpleChinaeseEncoding);
                var count = lines.Count();
                for (var i = 0; i < count; i++)
                {
                    contnet.AppendLine((i + 1) + "  " +  lines[i]);
                }
            }
            catch (Exception)
            {
                fileContenTextBox.Text = (filePathInfo + "  读取失败！");
                return;
            }
            fileContenTextBox.Text = contnet.ToString();
        }
    }
}
