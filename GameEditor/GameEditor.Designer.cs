namespace GameEditor
{
    partial class GameEditor
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.DrawSurface = new System.Windows.Forms.PictureBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this._functionText = new System.Windows.Forms.TextBox();
            this._fileText = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DrawSurface)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(928, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.DrawSurface);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(928, 639);
            this.splitContainer1.SplitterDistance = 680;
            this.splitContainer1.TabIndex = 1;
            // 
            // DrawSurface
            // 
            this.DrawSurface.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrawSurface.Location = new System.Drawing.Point(0, 0);
            this.DrawSurface.Name = "DrawSurface";
            this.DrawSurface.Size = new System.Drawing.Size(680, 639);
            this.DrawSurface.TabIndex = 0;
            this.DrawSurface.TabStop = false;
            this.DrawSurface.MouseEnter += new System.EventHandler(this.DrawSurface_MouseEnter);
            this.DrawSurface.MouseLeave += new System.EventHandler(this.DrawSurface_MouseLeave);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this._functionText);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this._fileText);
            this.splitContainer2.Size = new System.Drawing.Size(244, 639);
            this.splitContainer2.SplitterDistance = 281;
            this.splitContainer2.TabIndex = 0;
            // 
            // _functionText
            // 
            this._functionText.Dock = System.Windows.Forms.DockStyle.Fill;
            this._functionText.Location = new System.Drawing.Point(0, 0);
            this._functionText.Multiline = true;
            this._functionText.Name = "_functionText";
            this._functionText.ReadOnly = true;
            this._functionText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._functionText.Size = new System.Drawing.Size(244, 281);
            this._functionText.TabIndex = 0;
            this._functionText.WordWrap = false;
            // 
            // _fileText
            // 
            this._fileText.Dock = System.Windows.Forms.DockStyle.Fill;
            this._fileText.Location = new System.Drawing.Point(0, 0);
            this._fileText.Multiline = true;
            this._fileText.Name = "_fileText";
            this._fileText.ReadOnly = true;
            this._fileText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._fileText.Size = new System.Drawing.Size(244, 354);
            this._fileText.TabIndex = 0;
            this._fileText.WordWrap = false;
            // 
            // GameEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(928, 663);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "GameEditor";
            this.Text = "GameEditor";
            this.Activated += new System.EventHandler(this.GameEditor_Activated);
            this.Deactivate += new System.EventHandler(this.GameEditor_Deactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GameEditor_FormClosed);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DrawSurface)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        public System.Windows.Forms.PictureBox DrawSurface;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TextBox _functionText;
        private System.Windows.Forms.TextBox _fileText;
    }
}