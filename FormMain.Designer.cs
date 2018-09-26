namespace dotaplus_desktop
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.btnPredict = new System.Windows.Forms.Button();
            this.textBoxTable = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.textBoxTeam0 = new System.Windows.Forms.TextBox();
            this.textBoxTeam1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnPredict
            // 
            this.btnPredict.Location = new System.Drawing.Point(320, 381);
            this.btnPredict.Name = "btnPredict";
            this.btnPredict.Size = new System.Drawing.Size(75, 23);
            this.btnPredict.TabIndex = 0;
            this.btnPredict.Text = "预测";
            this.btnPredict.UseVisualStyleBackColor = true;
            this.btnPredict.Click += new System.EventHandler(this.btnPredict_Click);
            // 
            // textBoxTable
            // 
            this.textBoxTable.Location = new System.Drawing.Point(12, 41);
            this.textBoxTable.Multiline = true;
            this.textBoxTable.Name = "textBoxTable";
            this.textBoxTable.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxTable.Size = new System.Drawing.Size(694, 281);
            this.textBoxTable.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(474, 383);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Location = new System.Drawing.Point(12, 328);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.Size = new System.Drawing.Size(694, 47);
            this.textBoxOutput.TabIndex = 3;
            // 
            // textBoxTeam0
            // 
            this.textBoxTeam0.Location = new System.Drawing.Point(12, 14);
            this.textBoxTeam0.Name = "textBoxTeam0";
            this.textBoxTeam0.Size = new System.Drawing.Size(343, 21);
            this.textBoxTeam0.TabIndex = 4;
            // 
            // textBoxTeam1
            // 
            this.textBoxTeam1.Location = new System.Drawing.Point(361, 14);
            this.textBoxTeam1.Name = "textBoxTeam1";
            this.textBoxTeam1.Size = new System.Drawing.Size(345, 21);
            this.textBoxTeam1.TabIndex = 5;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(723, 418);
            this.Controls.Add(this.textBoxTeam1);
            this.Controls.Add(this.textBoxTeam0);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBoxTable);
            this.Controls.Add(this.btnPredict);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Homemade DotaPlus";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPredict;
        private System.Windows.Forms.TextBox textBoxTable;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.TextBox textBoxTeam0;
        private System.Windows.Forms.TextBox textBoxTeam1;
    }
}

