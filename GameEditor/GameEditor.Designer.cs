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
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.CheatMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._fullLifeThewMana = new System.Windows.Forms.ToolStripMenuItem();
            this._levelUp = new System.Windows.Forms.ToolStripMenuItem();
            this._addMoney1000 = new System.Windows.Forms.ToolStripMenuItem();
            this._allEnemyDie = new System.Windows.Forms.ToolStripMenuItem();
            this.DebugMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._changePlayerPos = new System.Windows.Forms.ToolStripMenuItem();
            this._runScriptMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._variablesMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.DrawSurface = new System.Windows.Forms.PictureBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this._functionText = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._fileText = new System.Windows.Forms.TextBox();
            this._scriptFilePath = new System.Windows.Forms.Button();
            this.TheToolTip = new System.Windows.Forms.ToolTip(this.components);
            this._logMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DrawSurface)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CheatMenu,
            this.DebugMenu});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(928, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // CheatMenu
            // 
            this.CheatMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fullLifeThewMana,
            this._levelUp,
            this._addMoney1000,
            this._allEnemyDie});
            this.CheatMenu.Name = "CheatMenu";
            this.CheatMenu.Size = new System.Drawing.Size(45, 20);
            this.CheatMenu.Text = "作弊";
            // 
            // _fullLifeThewMana
            // 
            this._fullLifeThewMana.Name = "_fullLifeThewMana";
            this._fullLifeThewMana.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this._fullLifeThewMana.Size = new System.Drawing.Size(194, 22);
            this._fullLifeThewMana.Text = "生体内全满";
            this._fullLifeThewMana.Click += new System.EventHandler(this._fullLifeThewMana_Click);
            // 
            // _levelUp
            // 
            this._levelUp.Name = "_levelUp";
            this._levelUp.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this._levelUp.Size = new System.Drawing.Size(194, 22);
            this._levelUp.Text = "升一级";
            this._levelUp.Click += new System.EventHandler(this._levelUp_Click);
            // 
            // _addMoney1000
            // 
            this._addMoney1000.Name = "_addMoney1000";
            this._addMoney1000.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this._addMoney1000.Size = new System.Drawing.Size(194, 22);
            this._addMoney1000.Text = "加钱1000";
            this._addMoney1000.Click += new System.EventHandler(this._addMoney1000_Click);
            // 
            // _allEnemyDie
            // 
            this._allEnemyDie.Name = "_allEnemyDie";
            this._allEnemyDie.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this._allEnemyDie.Size = new System.Drawing.Size(194, 22);
            this._allEnemyDie.Text = "所有敌人死亡";
            this._allEnemyDie.Click += new System.EventHandler(this._allEnemyDie_Click);
            // 
            // DebugMenu
            // 
            this.DebugMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._changePlayerPos,
            this._runScriptMenu,
            this._variablesMenu,
            this._logMenu});
            this.DebugMenu.Name = "DebugMenu";
            this.DebugMenu.Size = new System.Drawing.Size(45, 20);
            this.DebugMenu.Text = "调试";
            // 
            // _changePlayerPos
            // 
            this._changePlayerPos.Name = "_changePlayerPos";
            this._changePlayerPos.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this._changePlayerPos.Size = new System.Drawing.Size(176, 22);
            this._changePlayerPos.Text = "瞬移...";
            this._changePlayerPos.Click += new System.EventHandler(this._changePlayerPos_Click);
            // 
            // _runScriptMenu
            // 
            this._runScriptMenu.Name = "_runScriptMenu";
            this._runScriptMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this._runScriptMenu.Size = new System.Drawing.Size(176, 22);
            this._runScriptMenu.Text = "运行脚本...";
            this._runScriptMenu.Click += new System.EventHandler(this._runScriptMenu_Click);
            // 
            // _variablesMenu
            // 
            this._variablesMenu.Name = "_variablesMenu";
            this._variablesMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this._variablesMenu.Size = new System.Drawing.Size(176, 22);
            this._variablesMenu.Text = "变量列表...";
            this._variablesMenu.Click += new System.EventHandler(this._variablesMenu_Click);
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
            this.splitContainer2.Panel2.Controls.Add(this.tableLayoutPanel1);
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
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this._fileText, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this._scriptFilePath, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(244, 354);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // _fileText
            // 
            this._fileText.Dock = System.Windows.Forms.DockStyle.Fill;
            this._fileText.Location = new System.Drawing.Point(3, 28);
            this._fileText.Multiline = true;
            this._fileText.Name = "_fileText";
            this._fileText.ReadOnly = true;
            this._fileText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._fileText.Size = new System.Drawing.Size(238, 323);
            this._fileText.TabIndex = 0;
            this._fileText.WordWrap = false;
            // 
            // _scriptFilePath
            // 
            this._scriptFilePath.Dock = System.Windows.Forms.DockStyle.Fill;
            this._scriptFilePath.Location = new System.Drawing.Point(3, 3);
            this._scriptFilePath.MaximumSize = new System.Drawing.Size(0, 25);
            this._scriptFilePath.Name = "_scriptFilePath";
            this._scriptFilePath.Size = new System.Drawing.Size(238, 19);
            this._scriptFilePath.TabIndex = 1;
            this._scriptFilePath.Text = "文件路径";
            this._scriptFilePath.UseVisualStyleBackColor = true;
            this._scriptFilePath.Click += new System.EventHandler(this._scriptFilePath_Click);
            // 
            // _logMenu
            // 
            this._logMenu.Name = "_logMenu";
            this._logMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this._logMenu.Size = new System.Drawing.Size(176, 22);
            this._logMenu.Text = "日志...";
            this._logMenu.Click += new System.EventHandler(this._logMenu_Click);
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
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GameEditor_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GameEditor_FormClosed);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GameEditor_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.GameEditor_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GameEditor_KeyUp);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DrawSurface)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
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
        private System.Windows.Forms.Button _scriptFilePath;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolTip TheToolTip;
        private System.Windows.Forms.ToolStripMenuItem CheatMenu;
        private System.Windows.Forms.ToolStripMenuItem _fullLifeThewMana;
        private System.Windows.Forms.ToolStripMenuItem _levelUp;
        private System.Windows.Forms.ToolStripMenuItem _addMoney1000;
        private System.Windows.Forms.ToolStripMenuItem _allEnemyDie;
        private System.Windows.Forms.ToolStripMenuItem DebugMenu;
        private System.Windows.Forms.ToolStripMenuItem _changePlayerPos;
        private System.Windows.Forms.ToolStripMenuItem _runScriptMenu;
        private System.Windows.Forms.ToolStripMenuItem _variablesMenu;
        private System.Windows.Forms.ToolStripMenuItem _logMenu;
    }
}