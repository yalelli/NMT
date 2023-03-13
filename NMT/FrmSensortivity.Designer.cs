namespace NMT
{
    partial class FrmSensortivity
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.bntCalculate = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDistance = new System.Windows.Forms.TextBox();
            this.bntCalibrate = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bntMoveto = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txbDistanceStep = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.bntMove = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txbConstantTime = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.bntCalculate);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.txtDistance);
            this.groupBox2.Controls.Add(this.bntCalibrate);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(273, 234);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(310, 128);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "计算参数：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(250, 43);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 24);
            this.label7.TabIndex = 17;
            this.label7.Text = "(um)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(27, 44);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(105, 24);
            this.label6.TabIndex = 16;
            this.label6.Text = "测量距离：";
            // 
            // bntCalculate
            // 
            this.bntCalculate.Location = new System.Drawing.Point(135, 78);
            this.bntCalculate.Name = "bntCalculate";
            this.bntCalculate.Size = new System.Drawing.Size(110, 34);
            this.bntCalculate.TabIndex = 15;
            this.bntCalculate.Text = "计算";
            this.bntCalculate.UseVisualStyleBackColor = true;
            this.bntCalculate.Click += new System.EventHandler(this.bntCalculate_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(380, 155);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 24);
            this.label2.TabIndex = 4;
            this.label2.Text = "cs:";
            // 
            // txtDistance
            // 
            this.txtDistance.Location = new System.Drawing.Point(134, 43);
            this.txtDistance.Name = "txtDistance";
            this.txtDistance.Size = new System.Drawing.Size(111, 29);
            this.txtDistance.TabIndex = 2;
            this.txtDistance.Text = "0";
            // 
            // bntCalibrate
            // 
            this.bntCalibrate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bntCalibrate.Location = new System.Drawing.Point(401, 199);
            this.bntCalibrate.Name = "bntCalibrate";
            this.bntCalibrate.Size = new System.Drawing.Size(89, 23);
            this.bntCalibrate.TabIndex = 6;
            this.bntCalibrate.Text = "计算";
            this.bntCalibrate.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.bntMoveto);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txbDistanceStep);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.bntMove);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txbConstantTime);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(273, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(310, 174);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "请输入标定参数：";
            // 
            // bntMoveto
            // 
            this.bntMoveto.Location = new System.Drawing.Point(135, 123);
            this.bntMoveto.Name = "bntMoveto";
            this.bntMoveto.Size = new System.Drawing.Size(110, 33);
            this.bntMoveto.TabIndex = 14;
            this.bntMoveto.Text = "开始移动";
            this.bntMoveto.UseVisualStyleBackColor = true;
            this.bntMoveto.Click += new System.EventHandler(this.bntMoveto_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(250, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 24);
            this.label4.TabIndex = 13;
            this.label4.Text = "(um)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 80);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 24);
            this.label5.TabIndex = 12;
            this.label5.Text = "移动距离：";
            // 
            // txbDistanceStep
            // 
            this.txbDistanceStep.Location = new System.Drawing.Point(134, 77);
            this.txbDistanceStep.Name = "txbDistanceStep";
            this.txbDistanceStep.Size = new System.Drawing.Size(111, 29);
            this.txbDistanceStep.TabIndex = 11;
            this.txbDistanceStep.Text = "10";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(250, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 24);
            this.label3.TabIndex = 10;
            this.label3.Text = "(s)";
            // 
            // bntMove
            // 
            this.bntMove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bntMove.Location = new System.Drawing.Point(401, 94);
            this.bntMove.Name = "bntMove";
            this.bntMove.Size = new System.Drawing.Size(89, 23);
            this.bntMove.TabIndex = 7;
            this.bntMove.Text = "Move";
            this.bntMove.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 24);
            this.label1.TabIndex = 9;
            this.label1.Text = "保持时间：";
            // 
            // txbConstantTime
            // 
            this.txbConstantTime.Location = new System.Drawing.Point(134, 46);
            this.txbConstantTime.Name = "txbConstantTime";
            this.txbConstantTime.Size = new System.Drawing.Size(111, 29);
            this.txbConstantTime.TabIndex = 8;
            this.txbConstantTime.Text = "5";
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(408, 368);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(110, 32);
            this.btnClose.TabIndex = 14;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // FrmSensortivity
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 425);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmSensortivity";
            this.Text = "灵敏度标定";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDistance;
        private System.Windows.Forms.Button bntCalibrate;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txbDistanceStep;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button bntMove;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txbConstantTime;
        private System.Windows.Forms.Button bntMoveto;
        private System.Windows.Forms.Button bntCalculate;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnClose;
    }
}