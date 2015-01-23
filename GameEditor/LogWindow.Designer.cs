namespace GameEditor
{
    partial class LogWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LogTextCtrl = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // LogTextCtrl
            // 
            this.LogTextCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogTextCtrl.Location = new System.Drawing.Point(0, 0);
            this.LogTextCtrl.Multiline = true;
            this.LogTextCtrl.Name = "LogTextCtrl";
            this.LogTextCtrl.ReadOnly = true;
            this.LogTextCtrl.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.LogTextCtrl.Size = new System.Drawing.Size(542, 137);
            this.LogTextCtrl.TabIndex = 0;
            this.LogTextCtrl.WordWrap = false;
            // 
            // LogWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 137);
            this.Controls.Add(this.LogTextCtrl);
            this.Name = "LogWindow";
            this.Text = "日志";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogWindow_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox LogTextCtrl;

    }
}