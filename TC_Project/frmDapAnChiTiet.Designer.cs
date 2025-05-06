namespace TC_Project
{
    partial class fmHienThiChiTiet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fmHienThiChiTiet));
            this.pbMini = new System.Windows.Forms.PictureBox();
            this.pbClose = new System.Windows.Forms.PictureBox();
            this.labelDapAnCT = new System.Windows.Forms.Label();
            this.txtDapAnCT = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbMini)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbClose)).BeginInit();
            this.SuspendLayout();
            // 
            // pbMini
            // 
            this.pbMini.BackColor = System.Drawing.Color.Transparent;
            this.pbMini.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbMini.BackgroundImage")));
            this.pbMini.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbMini.Location = new System.Drawing.Point(1302, 12);
            this.pbMini.Name = "pbMini";
            this.pbMini.Size = new System.Drawing.Size(16, 17);
            this.pbMini.TabIndex = 87;
            this.pbMini.TabStop = false;
            this.pbMini.Click += new System.EventHandler(this.pbMini_Click);
            // 
            // pbClose
            // 
            this.pbClose.BackColor = System.Drawing.Color.Transparent;
            this.pbClose.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbClose.BackgroundImage")));
            this.pbClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbClose.Location = new System.Drawing.Point(1324, 12);
            this.pbClose.Name = "pbClose";
            this.pbClose.Size = new System.Drawing.Size(16, 17);
            this.pbClose.TabIndex = 86;
            this.pbClose.TabStop = false;
            this.pbClose.Click += new System.EventHandler(this.pbClose_Click);
            // 
            // labelDapAnCT
            // 
            this.labelDapAnCT.BackColor = System.Drawing.Color.Transparent;
            this.labelDapAnCT.Font = new System.Drawing.Font("Arial", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDapAnCT.ForeColor = System.Drawing.Color.Black;
            this.labelDapAnCT.Location = new System.Drawing.Point(207, 223);
            this.labelDapAnCT.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDapAnCT.Name = "labelDapAnCT";
            this.labelDapAnCT.Size = new System.Drawing.Size(877, 46);
            this.labelDapAnCT.TabIndex = 85;
            this.labelDapAnCT.Text = "QUESTION  3:";
            // 
            // txtDapAnCT
            // 
            this.txtDapAnCT.BackColor = System.Drawing.Color.Transparent;
            this.txtDapAnCT.Font = new System.Drawing.Font("Arial", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDapAnCT.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.txtDapAnCT.Location = new System.Drawing.Point(207, 283);
            this.txtDapAnCT.Name = "txtDapAnCT";
            this.txtDapAnCT.Size = new System.Drawing.Size(951, 423);
            this.txtDapAnCT.TabIndex = 88;
            this.txtDapAnCT.Text = resources.GetString("txtDapAnCT.Text");
            // 
            // fmHienThiChiTiet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1366, 768);
            this.Controls.Add(this.txtDapAnCT);
            this.Controls.Add(this.pbMini);
            this.Controls.Add(this.pbClose);
            this.Controls.Add(this.labelDapAnCT);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "fmHienThiChiTiet";
            this.Text = "Đáp án chi tiết";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fmHienThiChiTiet_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pbMini)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbClose)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox pbMini;
        private System.Windows.Forms.PictureBox pbClose;
        private System.Windows.Forms.Label labelDapAnCT;
        private System.Windows.Forms.Label txtDapAnCT;
    }
}