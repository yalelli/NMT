namespace NMT
{
    partial class FrmCaliWin
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
            this.bntCalibration = new System.Windows.Forms.Button();
            this.btIndentationCalibration = new System.Windows.Forms.Button();
            this.btnStiffnessCalibration = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // bntCalibration
            // 
            this.bntCalibration.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bntCalibration.Location = new System.Drawing.Point(126, 66);
            this.bntCalibration.Name = "bntCalibration";
            this.bntCalibration.Size = new System.Drawing.Size(126, 45);
            this.bntCalibration.TabIndex = 277;
            this.bntCalibration.Text = "灵敏度标定";
            this.bntCalibration.UseVisualStyleBackColor = true;
            this.bntCalibration.Click += new System.EventHandler(this.bntCalibration_Click);
            // 
            // btIndentationCalibration
            // 
            this.btIndentationCalibration.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btIndentationCalibration.Location = new System.Drawing.Point(126, 188);
            this.btIndentationCalibration.Name = "btIndentationCalibration";
            this.btIndentationCalibration.Size = new System.Drawing.Size(126, 37);
            this.btIndentationCalibration.TabIndex = 278;
            this.btIndentationCalibration.Text = "精度标定";
            this.btIndentationCalibration.UseVisualStyleBackColor = true;
            this.btIndentationCalibration.Click += new System.EventHandler(this.btIndentationCalibration_Click);
            // 
            // btnStiffnessCalibration
            // 
            this.btnStiffnessCalibration.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnStiffnessCalibration.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStiffnessCalibration.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnStiffnessCalibration.Location = new System.Drawing.Point(126, 128);
            this.btnStiffnessCalibration.Name = "btnStiffnessCalibration";
            this.btnStiffnessCalibration.Size = new System.Drawing.Size(126, 39);
            this.btnStiffnessCalibration.TabIndex = 276;
            this.btnStiffnessCalibration.Text = "刚度标定";
            this.btnStiffnessCalibration.UseVisualStyleBackColor = true;
            this.btnStiffnessCalibration.Click += new System.EventHandler(this.btnStiffnessCalibration_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(36, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(162, 24);
            this.label1.TabIndex = 279;
            this.label1.Text = "请选择标定模式：";
            // 
            // FrmCaliWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(377, 237);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bntCalibration);
            this.Controls.Add(this.btIndentationCalibration);
            this.Controls.Add(this.btnStiffnessCalibration);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmCaliWin";
            this.Text = "标定模式选择";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bntCalibration;
        private System.Windows.Forms.Button btIndentationCalibration;
        private System.Windows.Forms.Button btnStiffnessCalibration;
        private System.Windows.Forms.Label label1;
    }
}