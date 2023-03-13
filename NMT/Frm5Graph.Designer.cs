namespace NMT
{
    partial class Frm5Graph
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
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSaveStiffness = new System.Windows.Forms.Button();
            this.btAlign = new System.Windows.Forms.Button();
            this.lbStiffness = new System.Windows.Forms.Label();
            this.txbStiffness = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbgraph0 = new System.Windows.Forms.CheckBox();
            this.cbgraph1 = new System.Windows.Forms.CheckBox();
            this.zedGraphDataprocess = new ZedGraph.ZedGraphControl();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.bntSaveData = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(41, 513);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(123, 32);
            this.btnClose.TabIndex = 45;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnSaveStiffness
            // 
            this.btnSaveStiffness.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveStiffness.Location = new System.Drawing.Point(41, 410);
            this.btnSaveStiffness.Name = "btnSaveStiffness";
            this.btnSaveStiffness.Size = new System.Drawing.Size(123, 35);
            this.btnSaveStiffness.TabIndex = 44;
            this.btnSaveStiffness.Text = "存储刚度";
            this.btnSaveStiffness.UseVisualStyleBackColor = true;
            this.btnSaveStiffness.Click += new System.EventHandler(this.btnSaveStiffness_Click);
            // 
            // btAlign
            // 
            this.btAlign.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btAlign.Location = new System.Drawing.Point(41, 356);
            this.btAlign.Name = "btAlign";
            this.btAlign.Size = new System.Drawing.Size(123, 38);
            this.btAlign.TabIndex = 43;
            this.btAlign.Text = "图像显示";
            this.btAlign.UseVisualStyleBackColor = true;
            this.btAlign.Click += new System.EventHandler(this.btAlign_Click);
            // 
            // lbStiffness
            // 
            this.lbStiffness.AutoSize = true;
            this.lbStiffness.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbStiffness.Location = new System.Drawing.Point(12, 229);
            this.lbStiffness.Name = "lbStiffness";
            this.lbStiffness.Size = new System.Drawing.Size(67, 24);
            this.lbStiffness.TabIndex = 42;
            this.lbStiffness.Text = "刚度：";
            // 
            // txbStiffness
            // 
            this.txbStiffness.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txbStiffness.Location = new System.Drawing.Point(74, 270);
            this.txbStiffness.Name = "txbStiffness";
            this.txbStiffness.Size = new System.Drawing.Size(56, 29);
            this.txbStiffness.TabIndex = 41;
            this.txbStiffness.Text = "3750";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cbgraph0);
            this.groupBox1.Controls.Add(this.cbgraph1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(16, 23);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(186, 172);
            this.groupBox1.TabIndex = 40;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "选择需要显示的曲线：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(49, 57);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(86, 24);
            this.label7.TabIndex = 25;
            this.label7.Text = "基准曲线";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Black;
            this.label6.ForeColor = System.Drawing.Color.Blue;
            this.label6.Location = new System.Drawing.Point(21, 57);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(20, 24);
            this.label6.TabIndex = 24;
            this.label6.Text = "  ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(21, 133);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 24);
            this.label2.TabIndex = 19;
            this.label2.Text = "  ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Blue;
            this.label1.ForeColor = System.Drawing.Color.Blue;
            this.label1.Location = new System.Drawing.Point(21, 95);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 24);
            this.label1.TabIndex = 18;
            this.label1.Text = "  ";
            // 
            // cbgraph0
            // 
            this.cbgraph0.AutoSize = true;
            this.cbgraph0.Location = new System.Drawing.Point(53, 94);
            this.cbgraph0.Name = "cbgraph0";
            this.cbgraph0.Size = new System.Drawing.Size(115, 28);
            this.cbgraph0.TabIndex = 13;
            this.cbgraph0.Text = "压痕曲线1";
            this.cbgraph0.UseVisualStyleBackColor = true;
            // 
            // cbgraph1
            // 
            this.cbgraph1.AutoSize = true;
            this.cbgraph1.Location = new System.Drawing.Point(53, 133);
            this.cbgraph1.Name = "cbgraph1";
            this.cbgraph1.Size = new System.Drawing.Size(115, 28);
            this.cbgraph1.TabIndex = 14;
            this.cbgraph1.Text = "压痕曲线2";
            this.cbgraph1.UseVisualStyleBackColor = true;
            // 
            // zedGraphDataprocess
            // 
            this.zedGraphDataprocess.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.zedGraphDataprocess.IsAutoScrollRange = true;
            this.zedGraphDataprocess.IsShowHScrollBar = true;
            this.zedGraphDataprocess.IsShowPointValues = true;
            this.zedGraphDataprocess.IsShowVScrollBar = true;
            this.zedGraphDataprocess.Location = new System.Drawing.Point(208, 1);
            this.zedGraphDataprocess.Name = "zedGraphDataprocess";
            this.zedGraphDataprocess.ScrollGrace = 0D;
            this.zedGraphDataprocess.ScrollMaxX = 0D;
            this.zedGraphDataprocess.ScrollMaxY = 0D;
            this.zedGraphDataprocess.ScrollMaxY2 = 0D;
            this.zedGraphDataprocess.ScrollMinX = 0D;
            this.zedGraphDataprocess.ScrollMinY = 0D;
            this.zedGraphDataprocess.ScrollMinY2 = 0D;
            this.zedGraphDataprocess.Size = new System.Drawing.Size(931, 586);
            this.zedGraphDataprocess.TabIndex = 46;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(36, 270);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(32, 29);
            this.button1.TabIndex = 47;
            this.button1.Text = "+";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(139, 270);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(32, 29);
            this.button2.TabIndex = 48;
            this.button2.Text = "-";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // bntSaveData
            // 
            this.bntSaveData.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bntSaveData.Location = new System.Drawing.Point(41, 460);
            this.bntSaveData.Name = "bntSaveData";
            this.bntSaveData.Size = new System.Drawing.Size(123, 36);
            this.bntSaveData.TabIndex = 49;
            this.bntSaveData.Text = "数据保存";
            this.bntSaveData.UseVisualStyleBackColor = true;
            this.bntSaveData.Click += new System.EventHandler(this.bntSaveData_Click);
            // 
            // Frm5Graph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1151, 592);
            this.Controls.Add(this.bntSaveData);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.zedGraphDataprocess);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSaveStiffness);
            this.Controls.Add(this.btAlign);
            this.Controls.Add(this.lbStiffness);
            this.Controls.Add(this.txbStiffness);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Frm5Graph";
            this.Text = "精度标定";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Frm5Graph_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSaveStiffness;
        private System.Windows.Forms.Button btAlign;
        private System.Windows.Forms.Label lbStiffness;
        private System.Windows.Forms.TextBox txbStiffness;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cbgraph0;
        private System.Windows.Forms.CheckBox cbgraph1;
        private ZedGraph.ZedGraphControl zedGraphDataprocess;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button bntSaveData;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}