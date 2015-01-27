namespace XNAFormatter
{
    partial class XNAFormatter
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
            this.sourceFileLabel = new System.Windows.Forms.Label();
            this.sourceFileTextBox = new System.Windows.Forms.TextBox();
            this.sourceFileBrowseButton = new System.Windows.Forms.Button();
            this.platformBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.profileBox = new System.Windows.Forms.ComboBox();
            this.convertButton = new System.Windows.Forms.Button();
            this.compressBox = new System.Windows.Forms.CheckBox();
            this.audioBox = new System.Windows.Forms.CheckBox();
            this.statusLabel = new System.Windows.Forms.Label();
            this.outputBrowseButton = new System.Windows.Forms.Button();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.outputLabel = new System.Windows.Forms.Label();
            this.logBox = new System.Windows.Forms.CheckBox();
            this._toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // sourceFileLabel
            // 
            this.sourceFileLabel.AutoSize = true;
            this.sourceFileLabel.Location = new System.Drawing.Point(20, 12);
            this.sourceFileLabel.Name = "sourceFileLabel";
            this.sourceFileLabel.Size = new System.Drawing.Size(46, 13);
            this.sourceFileLabel.TabIndex = 0;
            this.sourceFileLabel.Text = "源文件:";
            // 
            // sourceFileTextBox
            // 
            this.sourceFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceFileTextBox.Location = new System.Drawing.Point(100, 9);
            this.sourceFileTextBox.Name = "sourceFileTextBox";
            this.sourceFileTextBox.ReadOnly = true;
            this.sourceFileTextBox.Size = new System.Drawing.Size(187, 20);
            this.sourceFileTextBox.TabIndex = 1;
            // 
            // sourceFileBrowseButton
            // 
            this.sourceFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceFileBrowseButton.Location = new System.Drawing.Point(293, 7);
            this.sourceFileBrowseButton.Name = "sourceFileBrowseButton";
            this.sourceFileBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.sourceFileBrowseButton.TabIndex = 2;
            this.sourceFileBrowseButton.Text = "选择...";
            this.sourceFileBrowseButton.UseVisualStyleBackColor = true;
            this.sourceFileBrowseButton.Click += new System.EventHandler(this.sourceFileBrowseButton_Click);
            // 
            // platformBox
            // 
            this.platformBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.platformBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.platformBox.FormattingEnabled = true;
            this.platformBox.Items.AddRange(new object[] {
            "Windows",
            "Xbox360",
            "WindowsPhone"});
            this.platformBox.Location = new System.Drawing.Point(100, 63);
            this.platformBox.Name = "platformBox";
            this.platformBox.Size = new System.Drawing.Size(187, 21);
            this.platformBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "目标平台:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 93);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "目标模式:";
            // 
            // profileBox
            // 
            this.profileBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.profileBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.profileBox.FormattingEnabled = true;
            this.profileBox.Items.AddRange(new object[] {
            "Reach",
            "HiDef"});
            this.profileBox.Location = new System.Drawing.Point(100, 90);
            this.profileBox.Name = "profileBox";
            this.profileBox.Size = new System.Drawing.Size(187, 21);
            this.profileBox.TabIndex = 5;
            // 
            // convertButton
            // 
            this.convertButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.convertButton.Location = new System.Drawing.Point(176, 135);
            this.convertButton.Name = "convertButton";
            this.convertButton.Size = new System.Drawing.Size(111, 35);
            this.convertButton.TabIndex = 7;
            this.convertButton.Text = "转换";
            this.convertButton.UseVisualStyleBackColor = true;
            this.convertButton.Click += new System.EventHandler(this.convertButton_Click);
            // 
            // compressBox
            // 
            this.compressBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.compressBox.AutoSize = true;
            this.compressBox.Location = new System.Drawing.Point(12, 123);
            this.compressBox.Name = "compressBox";
            this.compressBox.Size = new System.Drawing.Size(110, 17);
            this.compressBox.TabIndex = 9;
            this.compressBox.Text = "压缩输出文件？";
            this.compressBox.UseVisualStyleBackColor = true;
            // 
            // audioBox
            // 
            this.audioBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.audioBox.AutoSize = true;
            this.audioBox.Location = new System.Drawing.Point(12, 142);
            this.audioBox.Name = "audioBox";
            this.audioBox.Size = new System.Drawing.Size(158, 17);
            this.audioBox.TabIndex = 10;
            this.audioBox.Text = "声音文件转成背景音乐？";
            this.audioBox.UseVisualStyleBackColor = true;
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(293, 120);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 13);
            this.statusLabel.TabIndex = 11;
            // 
            // outputBrowseButton
            // 
            this.outputBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.outputBrowseButton.Location = new System.Drawing.Point(293, 33);
            this.outputBrowseButton.Name = "outputBrowseButton";
            this.outputBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.outputBrowseButton.TabIndex = 14;
            this.outputBrowseButton.Text = "选择...";
            this.outputBrowseButton.UseVisualStyleBackColor = true;
            this.outputBrowseButton.Click += new System.EventHandler(this.outputBrowseButton_Click);
            // 
            // outputTextBox
            // 
            this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputTextBox.Location = new System.Drawing.Point(100, 35);
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.Size = new System.Drawing.Size(187, 20);
            this.outputTextBox.TabIndex = 13;
            // 
            // outputLabel
            // 
            this.outputLabel.AutoSize = true;
            this.outputLabel.Location = new System.Drawing.Point(7, 38);
            this.outputLabel.Name = "outputLabel";
            this.outputLabel.Size = new System.Drawing.Size(70, 13);
            this.outputLabel.TabIndex = 12;
            this.outputLabel.Text = "输出文件夹:";
            // 
            // logBox
            // 
            this.logBox.AutoSize = true;
            this.logBox.Location = new System.Drawing.Point(12, 160);
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(86, 17);
            this.logBox.TabIndex = 15;
            this.logBox.Text = "显示日志？";
            this.logBox.UseVisualStyleBackColor = true;
            // 
            // XNAFormatter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 177);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.outputBrowseButton);
            this.Controls.Add(this.outputTextBox);
            this.Controls.Add(this.outputLabel);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.audioBox);
            this.Controls.Add(this.compressBox);
            this.Controls.Add(this.convertButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.profileBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.platformBox);
            this.Controls.Add(this.sourceFileBrowseButton);
            this.Controls.Add(this.sourceFileTextBox);
            this.Controls.Add(this.sourceFileLabel);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(4000, 216);
            this.MinimumSize = new System.Drawing.Size(400, 216);
            this.Name = "XNAFormatter";
            this.Text = "XNA Formatter - 汉化版";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label sourceFileLabel;
        private System.Windows.Forms.TextBox sourceFileTextBox;
        private System.Windows.Forms.Button sourceFileBrowseButton;
        private System.Windows.Forms.ComboBox platformBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox profileBox;
        private System.Windows.Forms.Button convertButton;
        private System.Windows.Forms.CheckBox compressBox;
        private System.Windows.Forms.CheckBox audioBox;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button outputBrowseButton;
        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.Label outputLabel;
        private System.Windows.Forms.CheckBox logBox;
        private System.Windows.Forms.ToolTip _toolTip;
    }
}

