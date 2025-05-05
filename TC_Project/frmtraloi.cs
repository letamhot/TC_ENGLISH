using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_Project.Model;

namespace TC_Project
{
    public partial class frmtraloi : Form
    {
        private Socket _socket;
        private int _doiid = 0;
        private int _cauhoiid = 0;
        private int _goicauhoiid = 0;
        private int[] _ttgoi;
        private bool _isStart = false;
        private string doiChoi = "";
        private int cuocthiId = 0;
        private string currentPath = Directory.GetCurrentDirectory();
        QuaMienDiSanEntities _entities = new QuaMienDiSanEntities();
        SqlDataAccess sqlObject = new SqlDataAccess();

        public frmtraloi()
        {
            InitializeComponent();


        }

        public frmtraloi(Socket sock, int doiid, int cauhoiid, int goicauhoiid, int[] ttgoi, bool isStart)
        {
            InitializeComponent();
            _socket = sock;
            _doiid = doiid;
            _cauhoiid = cauhoiid;
            _goicauhoiid = goicauhoiid;
            _ttgoi = ttgoi;
            _isStart = isStart;
            loadUC();

        }
        private void loadUC()
        {
            this.Focus();
            //pnlDiemSo.Visible = true;
            //layCuocThiHienTai();
            ds_cuocthi cuocThiHienTai = _entities.ds_cuocthi.FirstOrDefault(x => x.trangthai == true);

            int idCuocThiHienTai = cuocThiHienTai.cuocthiid;
            var lsKetQua = _entities.ds_hienthicautraloi.Where(x => x.doiid == _doiid && x.phanthiid == 1 && x.cuocthiid == idCuocThiHienTai).ToList();
            if (lsKetQua != null && lsKetQua.Count > 0)
            {
                try
                {
                    if (lsKetQua.Count > 5)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            ds_hienthicautraloi item = lsKetQua[i];
                            var lbl = this.Controls.Find("lblDapAn" + (i + 1), true).FirstOrDefault() as Label;
                            var lbl1 = this.Controls.Find("lbltraloi" + (i + 1), true).FirstOrDefault() as Label;
                            lbl.Font = new Font("Arial", lsKetQua[i].dapan.Length > 50 ? 18 : lsKetQua[i].dapan.Length < 10 ? 22 : 20, FontStyle.Bold);

                            lbl.Text = lsKetQua[i].dapan;
                            lbl.ForeColor = Color.DodgerBlue;

                            lbl.Visible = true;

                            lbl1.BackgroundImage = lsKetQua[i].traloi == true ? Image.FromFile(currentPath + "\\Resources\\group4\\true.png") : Image.FromFile(currentPath + "\\Resources\\group4\\false.png");
                            
                            lbl1.Visible = true;

                        }
                    }
                    else
                    {
                        for (int i = 0; i < lsKetQua.Count; i++)
                        {
                            ds_hienthicautraloi item = lsKetQua[i];
                            var lbl = this.Controls.Find("lblDapAn" + (i + 1), true).FirstOrDefault() as Label;
                            var lbl1 = this.Controls.Find("lbltraloi" + (i + 1), true).FirstOrDefault() as Panel;
                            lbl.Text = lsKetQua[i].dapan;
                            lbl.ForeColor = Color.DodgerBlue;

                            lbl.Visible = true;

                            lbl1.BackgroundImage = lsKetQua[i].traloi == true ? Image.FromFile(currentPath + "\\Resources\\group4\\true.png") : Image.FromFile(currentPath + "\\Resources\\group4\\false.png");
                            lbl1.Visible = true;



                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Server failed!");
                }



            }
            

        }
        

        private void frmtraloi_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void pbMini_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;

        }

        private void pbClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmtraloi_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();

            }
        }
    }
}
