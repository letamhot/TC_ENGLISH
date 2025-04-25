namespace TC_Project
{
    partial class frmKhamGia
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmKhamGia));
            this.labelCauHoi = new System.Windows.Forms.Label();
            this.lblCauHoi = new System.Windows.Forms.Label();
            this.pbDA = new System.Windows.Forms.Panel();
            this.lblDapAn = new System.Windows.Forms.Label();
            this.pbClose = new System.Windows.Forms.PictureBox();
            this.pbDA.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbClose)).BeginInit();
            this.SuspendLayout();
            // 
            // labelCauHoi
            // 
            this.labelCauHoi.BackColor = System.Drawing.Color.Transparent;
            this.labelCauHoi.Font = new System.Drawing.Font("Arial", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCauHoi.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.labelCauHoi.Location = new System.Drawing.Point(279, 136);
            this.labelCauHoi.Name = "labelCauHoi";
            this.labelCauHoi.Size = new System.Drawing.Size(841, 41);
            this.labelCauHoi.TabIndex = 0;
            this.labelCauHoi.Text = "Câu hỏi";
            this.labelCauHoi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCauHoi
            // 
            this.lblCauHoi.BackColor = System.Drawing.Color.Transparent;
            this.lblCauHoi.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblCauHoi.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCauHoi.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblCauHoi.Location = new System.Drawing.Point(286, 177);
            this.lblCauHoi.Name = "lblCauHoi";
            this.lblCauHoi.Size = new System.Drawing.Size(841, 318);
            this.lblCauHoi.TabIndex = 1;
            this.lblCauHoi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbDA
            // 
            this.pbDA.BackColor = System.Drawing.Color.Transparent;
            this.pbDA.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbDA.BackgroundImage")));
            this.pbDA.Controls.Add(this.lblDapAn);
            this.pbDA.Location = new System.Drawing.Point(166, 498);
            this.pbDA.Name = "pbDA";
            this.pbDA.Size = new System.Drawing.Size(1106, 208);
            this.pbDA.TabIndex = 4;
            // 
            // lblDapAn
            // 
            this.lblDapAn.Font = new System.Drawing.Font("Arial", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDapAn.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblDapAn.Location = new System.Drawing.Point(254, 39);
            this.lblDapAn.Name = "lblDapAn";
            this.lblDapAn.Size = new System.Drawing.Size(725, 134);
            this.lblDapAn.TabIndex = 0;
            this.lblDapAn.Text = "label3";
            this.lblDapAn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbClose
            // 
            this.pbClose.BackColor = System.Drawing.Color.Transparent;
            this.pbClose.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbClose.BackgroundImage")));
            this.pbClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbClose.Location = new System.Drawing.Point(1336, 31);
            this.pbClose.Name = "pbClose";
            this.pbClose.Size = new System.Drawing.Size(32, 31);
            this.pbClose.TabIndex = 93;
            this.pbClose.TabStop = false;
            this.pbClose.Click += new System.EventHandler(this.pbClose_Click);
            // 
            // frmKhamGia
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1368, 768);
            this.Controls.Add(this.pbClose);
            this.Controls.Add(this.pbDA);
            this.Controls.Add(this.lblCauHoi);
            this.Controls.Add(this.labelCauHoi);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmKhamGia";
            this.Text = "frmKhamGia";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmKhamGia_KeyDown);
            this.pbDA.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbClose)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelCauHoi;
        private System.Windows.Forms.Label lblCauHoi;
        private System.Windows.Forms.Panel pbDA;
        private System.Windows.Forms.Label lblDapAn;
        private System.Windows.Forms.PictureBox pbClose;
    }
}