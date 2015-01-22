namespace GameEditor
{
    partial class VariablesWindow
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
            this.VariablesList = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this._findText = new System.Windows.Forms.TextBox();
            this._find = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // VariablesList
            // 
            this.VariablesList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VariablesList.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.VariablesList.Location = new System.Drawing.Point(3, 3);
            this.VariablesList.Multiline = true;
            this.VariablesList.Name = "VariablesList";
            this.VariablesList.ReadOnly = true;
            this.VariablesList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.VariablesList.Size = new System.Drawing.Size(291, 304);
            this.VariablesList.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.VariablesList, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(297, 347);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.22581F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.77419F));
            this.tableLayoutPanel2.Controls.Add(this._findText, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this._find, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 313);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(291, 31);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // _findText
            // 
            this._findText.Dock = System.Windows.Forms.DockStyle.Fill;
            this._findText.Location = new System.Drawing.Point(3, 3);
            this._findText.Name = "_findText";
            this._findText.Size = new System.Drawing.Size(207, 20);
            this._findText.TabIndex = 0;
            // 
            // _find
            // 
            this._find.Location = new System.Drawing.Point(216, 3);
            this._find.Name = "_find";
            this._find.Size = new System.Drawing.Size(72, 23);
            this._find.TabIndex = 1;
            this._find.Text = "查找";
            this._find.UseVisualStyleBackColor = true;
            this._find.Click += new System.EventHandler(this._find_Click);
            // 
            // VariablesWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(297, 347);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "VariablesWindow";
            this.Text = "变量";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VariablesWindow_FormClosing);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TextBox VariablesList;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TextBox _findText;
        private System.Windows.Forms.Button _find;


    }
}