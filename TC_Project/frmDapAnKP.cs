using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_Project.Model;

namespace TC_Project
{
    public partial class frmDapAnKP : Form
    {
        private Socket _socket;
        private int _doiid = 0;
        private int _cauhoiid = 0;
        private int _goicauhoiid = 0;
        private int[] _ttgoi;
        private bool _diem = false;
        private string doiChoi = "";
        private int cuocthiId = 0;
        QuaMienDiSanEntities _entities = new QuaMienDiSanEntities();
        SqlDataAccess sqlObject = new SqlDataAccess();
        public frmDapAnKP()
        {
            InitializeComponent();
        }
        public frmDapAnKP(Socket sock, int cauhoiid, bool diem)
        {
            InitializeComponent();
            _socket = sock;
            _cauhoiid = cauhoiid;
            _diem = diem;
            loadUC(cauhoiid, diem);

        }
        private void loadUC(int cauHoiId, bool hienthidiem)
        {
            this.Focus();
            onoffLabelCauTraLoiKP(true);

            onoffDapAn(hienthidiem);

            int cuocthiId = _entities.ds_cuocthi.FirstOrDefault(x => x.trangthai == true).cuocthiid;
            string sql = "SELECT * from ds_diem WHERE cuocthiid = " + cuocthiId + " and cauhoiid = " + cauHoiId + " and phanthiid = 3";
            DataTable dt = sqlObject.getDataFromSql(sql, "").Tables[0];

            ds_cauhoithuthach ds = _entities.ds_cauhoithuthach.FirstOrDefault(x => x.cauhoiid == cauHoiId && x.cuocthiid == cuocthiId);
            if (ds != null)
            {
                lblDA.ForeColor = Color.DarkGoldenrod;
                lblDA.Font = new Font("Arial", ds.dapanABC.Length > 50 ? 22 : ds.dapanABC.Length < 10 ? 26 : 24, FontStyle.Bold);
                lblDA.Text = ds.dapanABC.ToUpper();
                
            }

            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow item = dt.Rows[i];
                    ds_doi doiChoi = _entities.ds_doi.Find(int.Parse(item["doiid"].ToString()));

                    Label lblDapAn = null;
                    Label lblDiem = null;

                    switch (doiChoi.vitridoi)
                    {
                        case 1:
                            lblDapAn = lblDapAn1;
                            lblDiem = lblDiem1;
                            break;
                        case 2:
                            lblDapAn = lblDapAn2;
                            lblDiem = lblDiem2;
                            break;
                        case 3:
                            lblDapAn = lblDapAn3;
                            lblDiem = lblDiem3;
                            break;
                        case 4:
                            lblDapAn = lblDapAn4;
                            lblDiem = lblDiem4;
                            break;
                    }

                    if (lblDapAn != null && lblDiem != null)
                    {
                        if (string.IsNullOrEmpty(item["thoigiantraloi"].ToString()) || string.IsNullOrEmpty(item["cautraloi"].ToString()))
                        {
                            lblDiem.Text = "0";
                        }
                        else
                        {
                            lblDapAn.Text = item["thoigiantraloi"].ToString() + "s / " + item["cautraloi"].ToString();
                            lblDapAn.Font = new Font("Arial", item["cautraloi"].ToString().Length > 40 ? 16 : item["cautraloi"].ToString().Length < 10 ? 26 : 24);

                            if (hienthidiem)
                            {
                                lblDiem.Text = string.IsNullOrEmpty(item["sodiem"].ToString()) || lblDapAn.Text == "" ? "0" : item["sodiem"].ToString();
                            }
                        }
                    }
                }
            }

            // Set lblDiem1, lblDiem2, lblDiem3, lblDiem4 to "0" if lblDapAn1, lblDapAn2, lblDapAn3, or lblDapAn4 are empty or null
            if (hienthidiem)
            {
                if (string.IsNullOrEmpty(lblDapAn1.Text)) lblDiem1.Text = "0";
                if (string.IsNullOrEmpty(lblDapAn2.Text)) lblDiem2.Text = "0";
                if (string.IsNullOrEmpty(lblDapAn3.Text)) lblDiem3.Text = "0";
                if (string.IsNullOrEmpty(lblDapAn4.Text)) lblDiem4.Text = "0";
            }
        }
        private void frmDapAnKP_Load(object sender, EventArgs e)
        {
            
        }
        private void onoffLabelCauTraLoiKP(bool onOff)
        {
           
            lblDiem1.Visible = onOff;
            lblDiem2.Visible = onOff;
            lblDiem3.Visible = onOff;
            lblDiem4.Visible = onOff;
            lblDapAn1.Visible = onOff;
            lblDapAn2.Visible = onOff;
            lblDapAn3.Visible = onOff;
            lblDapAn4.Visible = onOff;
            ds_cuocthi dscuocthi = _entities.ds_cuocthi.FirstOrDefault(x => x.trangthai == true);
            var ten = "";
            var dsTen = _entities.ds_doi.Where(x => x.vaitro == "TS" && x.cuocthiid == dscuocthi.cuocthiid).ToList();
            if (dsTen != null && dsTen.Count > 0)
            {
                for (int i = 0; i < dsTen.Count; i++)
                {
                    var lbl = this.Controls.Find("lblTenTS" + (i + 1), true).FirstOrDefault() as Label;
                    var tachten = dsTen[i].tennguoichoi.Split(' ');
                    for (int j = 1; j < tachten.Length; j++)
                    {
                        ten = tachten[j - 1] + " " + tachten[j];
                    }
                    lbl.Text = ten;
                    


                }
            }
        }


        private void onoffDapAn(bool onoff)
        {
            pbDA.Visible = onoff;
            lblDA.Visible = onoff;
            panel5.Visible = onoff;
        }

        private void pbClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmDapAnKP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();

            }
        }
    }
}
