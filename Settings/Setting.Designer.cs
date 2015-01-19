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
            this.label1 = new System.Windows.Forms.Label();
            this._resolutionList = new System.Windows.Forms.ComboBox();
            this._saveButton = new System.Windows.Forms.Button();
            this._beginGameButton = new System.Windows.Forms.Button();
            this._windowMode = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(48, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "分辨率：";
            // 
            // _resolutionList
            // 
            this._resolutionList.FormattingEnabled = true;
            this._resolutionList.Location = new System.Drawing.Point(121, 32);
            this._resolutionList.Name = "_resolutionList";
            this._resolutionList.Size = new System.Drawing.Size(121, 21);
            this._resolutionList.TabIndex = 1;
            // 
            // _saveButton
            // 
            this._saveButton.Location = new System.Drawing.Point(51, 98);
            this._saveButton.Name = "_saveButton";
            this._saveButton.Size = new System.Drawing.Size(75, 23);
            this._saveButton.TabIndex = 2;
            this._saveButton.Text = "保存设置";
            this._saveButton.UseVisualStyleBackColor = true;
            this._saveButton.Click += new System.EventHandler(this._saveButton_Click);
            // 
            // _beginGameButton
            // 
            this._beginGameButton.Location = new System.Drawing.Point(197, 98);
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
            this._windowMode.Location = new System.Drawing.Point(121, 60);
            this._windowMode.Name = "_windowMode";
            this._windowMode.Size = new System.Drawing.Size(15, 14);
            this._windowMode.TabIndex = 4;
            this._windowMode.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "窗口模式：";
            // 
            // Setting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(322, 138);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._windowMode);
            this.Controls.Add(this._beginGameButton);
            this.Controls.Add(this._saveButton);
            this.Controls.Add(this._resolutionList);
            this.Controls.Add(this.label1);
            this.Name = "Setting";
            this.Text = "游戏设置";
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
    }
}