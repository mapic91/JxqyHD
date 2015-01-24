namespace GameStarter
{
    partial class Form1
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
            this._startGame = new System.Windows.Forms.Button();
            this._gameSetting = new System.Windows.Forms.Button();
            this._debugGame = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _startGame
            // 
            this._startGame.Location = new System.Drawing.Point(82, 27);
            this._startGame.Name = "_startGame";
            this._startGame.Size = new System.Drawing.Size(75, 23);
            this._startGame.TabIndex = 0;
            this._startGame.Text = "启动游戏";
            this._startGame.UseVisualStyleBackColor = true;
            this._startGame.Click += new System.EventHandler(this._startGame_Click);
            // 
            // _gameSetting
            // 
            this._gameSetting.Location = new System.Drawing.Point(82, 75);
            this._gameSetting.Name = "_gameSetting";
            this._gameSetting.Size = new System.Drawing.Size(75, 23);
            this._gameSetting.TabIndex = 1;
            this._gameSetting.Text = "游戏设置";
            this._gameSetting.UseVisualStyleBackColor = true;
            this._gameSetting.Click += new System.EventHandler(this._gameSetting_Click);
            // 
            // _debugGame
            // 
            this._debugGame.Location = new System.Drawing.Point(82, 128);
            this._debugGame.Name = "_debugGame";
            this._debugGame.Size = new System.Drawing.Size(75, 23);
            this._debugGame.TabIndex = 2;
            this._debugGame.Text = "调试模式";
            this._debugGame.UseVisualStyleBackColor = true;
            this._debugGame.Click += new System.EventHandler(this._debugGame_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(233, 172);
            this.Controls.Add(this._debugGame);
            this.Controls.Add(this._gameSetting);
            this.Controls.Add(this._startGame);
            this.Name = "Form1";
            this.Text = "启动游戏";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _startGame;
        private System.Windows.Forms.Button _gameSetting;
        private System.Windows.Forms.Button _debugGame;
    }
}

