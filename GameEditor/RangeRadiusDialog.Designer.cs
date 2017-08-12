namespace GameEditor
{
    partial class RangeRadiusDialog
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
            this._cancle = new System.Windows.Forms.Button();
            this._ok = new System.Windows.Forms.Button();
            this.RangeRadiusText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.NpcName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _cancle
            // 
            this._cancle.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancle.Location = new System.Drawing.Point(176, 113);
            this._cancle.Name = "_cancle";
            this._cancle.Size = new System.Drawing.Size(75, 23);
            this._cancle.TabIndex = 4;
            this._cancle.Text = "取消";
            this._cancle.UseVisualStyleBackColor = true;
            // 
            // _ok
            // 
            this._ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._ok.Location = new System.Drawing.Point(38, 113);
            this._ok.Name = "_ok";
            this._ok.Size = new System.Drawing.Size(75, 23);
            this._ok.TabIndex = 3;
            this._ok.Text = "确定";
            this._ok.UseVisualStyleBackColor = true;
            // 
            // RangeRadiusText
            // 
            this.RangeRadiusText.Location = new System.Drawing.Point(64, 77);
            this.RangeRadiusText.Name = "RangeRadiusText";
            this.RangeRadiusText.Size = new System.Drawing.Size(155, 20);
            this.RangeRadiusText.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(64, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "半径:";
            // 
            // NpcName
            // 
            this.NpcName.Location = new System.Drawing.Point(66, 33);
            this.NpcName.Name = "NpcName";
            this.NpcName.Size = new System.Drawing.Size(155, 20);
            this.NpcName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(65, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "NPC名字：（默认主角）";
            // 
            // RangeRadiusDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 158);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.NpcName);
            this.Controls.Add(this._cancle);
            this.Controls.Add(this._ok);
            this.Controls.Add(this.RangeRadiusText);
            this.Controls.Add(this.label1);
            this.Name = "RangeRadiusDialog";
            this.Text = "设置半径";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancle;
        private System.Windows.Forms.Button _ok;
        public System.Windows.Forms.TextBox RangeRadiusText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox NpcName;
    }
}