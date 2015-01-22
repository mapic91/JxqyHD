namespace GameEditor
{
    partial class PlayerPosDialog
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
            this.label2 = new System.Windows.Forms.Label();
            this.MapX = new System.Windows.Forms.TextBox();
            this.MapY = new System.Windows.Forms.TextBox();
            this._ok = new System.Windows.Forms.Button();
            this._cancle = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "X:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Y:";
            // 
            // MapX
            // 
            this.MapX.Location = new System.Drawing.Point(67, 29);
            this.MapX.Name = "MapX";
            this.MapX.Size = new System.Drawing.Size(155, 20);
            this.MapX.TabIndex = 2;
            // 
            // MapY
            // 
            this.MapY.Location = new System.Drawing.Point(67, 72);
            this.MapY.Name = "MapY";
            this.MapY.Size = new System.Drawing.Size(155, 20);
            this.MapY.TabIndex = 3;
            // 
            // _ok
            // 
            this._ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._ok.Location = new System.Drawing.Point(24, 126);
            this._ok.Name = "_ok";
            this._ok.Size = new System.Drawing.Size(75, 23);
            this._ok.TabIndex = 4;
            this._ok.Text = "确定";
            this._ok.UseVisualStyleBackColor = true;
            // 
            // _cancle
            // 
            this._cancle.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancle.Location = new System.Drawing.Point(162, 126);
            this._cancle.Name = "_cancle";
            this._cancle.Size = new System.Drawing.Size(75, 23);
            this._cancle.TabIndex = 5;
            this._cancle.Text = "取消";
            this._cancle.UseVisualStyleBackColor = true;
            // 
            // PlayerPosDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(253, 160);
            this.Controls.Add(this._cancle);
            this.Controls.Add(this._ok);
            this.Controls.Add(this.MapY);
            this.Controls.Add(this.MapX);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "PlayerPosDialog";
            this.Text = "瞬移到...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox MapX;
        public System.Windows.Forms.TextBox MapY;
        private System.Windows.Forms.Button _ok;
        private System.Windows.Forms.Button _cancle;
    }
}