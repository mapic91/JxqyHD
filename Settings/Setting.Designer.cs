namespace Settings
{
    partial class Setting
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
            this.label1 = new System.Windows.Forms.Label();
            this._resolutionList = new System.Windows.Forms.ComboBox();
            this._saveButton = new System.Windows.Forms.Button();
            this._beginGameButton = new System.Windows.Forms.Button();
            this._windowMode = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._soundEffectVolume = new System.Windows.Forms.TrackBar();
            this._musicVolume = new System.Windows.Forms.TrackBar();
            this._textSoundEffectVolume = new System.Windows.Forms.Label();
            this._textMusicVolume = new System.Windows.Forms.Label();
            this._lblMaxMagicUnit = new System.Windows.Forms.Label();
            this._maxMagicUintCtl = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.label5 = new System.Windows.Forms.Label();
            this._runSpeed = new System.Windows.Forms.TrackBar();
            this._textRunSpeed = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._soundEffectVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._musicVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._maxMagicUintCtl)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._runSpeed)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(48, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "分辨率：";
            // 
            // _resolutionList
            // 
            this._resolutionList.FormattingEnabled = true;
            this._resolutionList.Location = new System.Drawing.Point(121, 27);
            this._resolutionList.Name = "_resolutionList";
            this._resolutionList.Size = new System.Drawing.Size(131, 21);
            this._resolutionList.TabIndex = 1;
            // 
            // _saveButton
            // 
            this._saveButton.Location = new System.Drawing.Point(61, 343);
            this._saveButton.Name = "_saveButton";
            this._saveButton.Size = new System.Drawing.Size(75, 23);
            this._saveButton.TabIndex = 2;
            this._saveButton.Text = "保存设置";
            this._saveButton.UseVisualStyleBackColor = true;
            this._saveButton.Click += new System.EventHandler(this._saveButton_Click);
            // 
            // _beginGameButton
            // 
            this._beginGameButton.Location = new System.Drawing.Point(261, 343);
            this._beginGameButton.Name = "_beginGameButton";
            this._beginGameButton.Size = new System.Drawing.Size(75, 23);
            this._beginGameButton.TabIndex = 3;
            this._beginGameButton.Text = "开始游戏";
            this._beginGameButton.UseVisualStyleBackColor = true;
            this._beginGameButton.Click += new System.EventHandler(this._beginGameButton_Click);
            // 
            // _windowMode
            // 
            this._windowMode.AutoSize = true;
            this._windowMode.Location = new System.Drawing.Point(121, 71);
            this._windowMode.Name = "_windowMode";
            this._windowMode.Size = new System.Drawing.Size(15, 14);
            this._windowMode.TabIndex = 4;
            this._windowMode.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "窗口模式：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "音效音量：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(36, 150);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "音乐音量：";
            // 
            // _soundEffectVolume
            // 
            this._soundEffectVolume.Location = new System.Drawing.Point(109, 108);
            this._soundEffectVolume.Maximum = 100;
            this._soundEffectVolume.Name = "_soundEffectVolume";
            this._soundEffectVolume.Size = new System.Drawing.Size(227, 45);
            this._soundEffectVolume.TabIndex = 9;
            this._soundEffectVolume.Scroll += new System.EventHandler(this._soundEffectVolume_Scroll);
            // 
            // _musicVolume
            // 
            this._musicVolume.Location = new System.Drawing.Point(109, 150);
            this._musicVolume.Maximum = 100;
            this._musicVolume.Name = "_musicVolume";
            this._musicVolume.Size = new System.Drawing.Size(227, 45);
            this._musicVolume.TabIndex = 10;
            this._musicVolume.Scroll += new System.EventHandler(this._musicVolume_Scroll);
            // 
            // _textSoundEffectVolume
            // 
            this._textSoundEffectVolume.AutoSize = true;
            this._textSoundEffectVolume.Location = new System.Drawing.Point(342, 110);
            this._textSoundEffectVolume.Name = "_textSoundEffectVolume";
            this._textSoundEffectVolume.Size = new System.Drawing.Size(33, 13);
            this._textSoundEffectVolume.TabIndex = 15;
            this._textSoundEffectVolume.Text = "100%";
            // 
            // _textMusicVolume
            // 
            this._textMusicVolume.AutoSize = true;
            this._textMusicVolume.Location = new System.Drawing.Point(342, 150);
            this._textMusicVolume.Name = "_textMusicVolume";
            this._textMusicVolume.Size = new System.Drawing.Size(33, 13);
            this._textMusicVolume.TabIndex = 16;
            this._textMusicVolume.Text = "100%";
            // 
            // _lblMaxMagicUnit
            // 
            this._lblMaxMagicUnit.AutoSize = true;
            this._lblMaxMagicUnit.Location = new System.Drawing.Point(6, 19);
            this._lblMaxMagicUnit.Name = "_lblMaxMagicUnit";
            this._lblMaxMagicUnit.Size = new System.Drawing.Size(205, 13);
            this._lblMaxMagicUnit.TabIndex = 17;
            this._lblMaxMagicUnit.Text = "武功单位个数限制（0表示不限制）：";
            // 
            // _maxMagicUintCtl
            // 
            this._maxMagicUintCtl.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this._maxMagicUintCtl.Location = new System.Drawing.Point(216, 17);
            this._maxMagicUintCtl.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this._maxMagicUintCtl.Name = "_maxMagicUintCtl";
            this._maxMagicUintCtl.Size = new System.Drawing.Size(114, 20);
            this._maxMagicUintCtl.TabIndex = 18;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._lblMaxMagicUnit);
            this.groupBox1.Controls.Add(this._maxMagicUintCtl);
            this.groupBox1.Location = new System.Drawing.Point(39, 236);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(336, 94);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "性能";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(39, 197);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "跑步速度：";
            // 
            // _runSpeed
            // 
            this._runSpeed.LargeChange = 2;
            this._runSpeed.Location = new System.Drawing.Point(109, 201);
            this._runSpeed.Maximum = 8;
            this._runSpeed.Minimum = 2;
            this._runSpeed.Name = "_runSpeed";
            this._runSpeed.Size = new System.Drawing.Size(227, 45);
            this._runSpeed.TabIndex = 21;
            this._runSpeed.Value = 2;
            this._runSpeed.Scroll += new System.EventHandler(this._runSpeed_Scroll);
            // 
            // _textRunSpeed
            // 
            this._textRunSpeed.AutoSize = true;
            this._textRunSpeed.Location = new System.Drawing.Point(342, 197);
            this._textRunSpeed.Name = "_textRunSpeed";
            this._textRunSpeed.Size = new System.Drawing.Size(13, 13);
            this._textRunSpeed.TabIndex = 22;
            this._textRunSpeed.Text = "2";
            // 
            // Setting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 378);
            this.Controls.Add(this._textRunSpeed);
            this.Controls.Add(this._runSpeed);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._textMusicVolume);
            this.Controls.Add(this._textSoundEffectVolume);
            this.Controls.Add(this._musicVolume);
            this.Controls.Add(this._soundEffectVolume);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._windowMode);
            this.Controls.Add(this._beginGameButton);
            this.Controls.Add(this._saveButton);
            this.Controls.Add(this._resolutionList);
            this.Controls.Add(this.label1);
            this.Name = "Setting";
            this.Text = "游戏设置";
            ((System.ComponentModel.ISupportInitialize)(this._soundEffectVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._musicVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._maxMagicUintCtl)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._runSpeed)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox _resolutionList;
        private System.Windows.Forms.Button _saveButton;
        private System.Windows.Forms.Button _beginGameButton;
        private System.Windows.Forms.CheckBox _windowMode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar _soundEffectVolume;
        private System.Windows.Forms.TrackBar _musicVolume;
        private System.Windows.Forms.Label _textSoundEffectVolume;
        private System.Windows.Forms.Label _textMusicVolume;
        private System.Windows.Forms.Label _lblMaxMagicUnit;
        private System.Windows.Forms.NumericUpDown _maxMagicUintCtl;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ToolTip _toolTip;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar _runSpeed;
        private System.Windows.Forms.Label _textRunSpeed;
    }
}