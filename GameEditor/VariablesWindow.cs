using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GameEditor
{
    public partial class VariablesWindow : Form
    {
        public VariablesWindow()
        {
            InitializeComponent();
        }

        private void VariablesWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            e.Cancel = true;
        }

        private void _find_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(_findText.Text))return;

            var index = VariablesList.Text.IndexOf(_findText.Text, StringComparison.Ordinal);

            if (index == -1) return;
            VariablesList.Select();
            VariablesList.SelectionStart = index;
            VariablesList.SelectionLength = _findText.Text.Length;
            VariablesList.ScrollToCaret();
        }
    }
}
