using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_Project.Model;

namespace TC_Project
{
    public partial class frmTongDiem : Form
    {
        private Socket _socket;
        private int _doiid = 0;
        private int _cauhoiid = 0;
        private int _goicauhoiid = 0;
        private int[] _ttgoi;
        private bool _x2 = false;
        private bool _da = false;
        private bool _isStart = false;
        private string currentPath = Directory.GetCurrentDirectory();
        QuaMienDiSanEntities _entities = new QuaMienDiSanEntities();
        SqlDataAccess sqlObject = new SqlDataAccess();

        public frmTongDiem()
        {
            InitializeComponent();
            loadUC();
        }
        public void loadUC()
        {
            this.Focus();
            string spl = "Select doiid , phanthiid, sum(sodiem) as tongdiem from ds_diem GROUP BY phanthiid, doiid";
            DataTable dt = sqlObject.getDataFromSql(spl, "").Tables[0];
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    for(int j = 0; j<4; j++)
                    {
                        var lbl = this.Controls.Find("lbl" +(i + 1)+(j + 1), true).FirstOrDefault() as Label;
                        var dr = dt.Select("doiid =" + (i + 1) + " AND phanthiid =" + (j + 1));
                        if(dr.Count() > 0)
                        {
                            lbl.Text = dt.Select("doiid =" + (i + 1) + " AND phanthiid =" + (j + 1))[0]["tongdiem"].ToString();
                        }
                        else
                        {
                            lbl.Text = "0";
                        }
                    }
                }

            }
            ds_cuocthi dscuocthi = _entities.ds_cuocthi.FirstOrDefault(x => x.trangthai == true);
            var ten = "";
            var dsTen = _entities.ds_doi.Where(x => x.vaitro == "TS" && x.cuocthiid == dscuocthi.cuocthiid).ToList();
            if (dsTen != null && dsTen.Count > 0)
            {
                for (int i = 0; i < dsTen.Count; i++)
                {
                    var lbl = this.Controls.Find("lblTS" + (i + 1), true).FirstOrDefault() as Label;
                    // Đặt màu chữ thành màu vàng gold
                    lbl.ForeColor = Color.Yellow; // Màu vàng gold (R=255, G=215, B=0)

                    // Vẽ viền cho chữ
                    Graphics g = lbl.CreateGraphics();
                    g.DrawString(lbl.Text, lbl.Font, new SolidBrush(Color.YellowGreen), lbl.Left - 1, lbl.Top - 1); // Viền đen (có thể thay đổi giá trị Offset để điều chỉnh độ dày của viền)
                    g.DrawString(lbl.Text, lbl.Font, new SolidBrush(Color.YellowGreen), lbl.Left + 1, lbl.Top + 1);
                    g.DrawString(lbl.Text, lbl.Font, new SolidBrush(Color.YellowGreen), lbl.Left - 1, lbl.Top + 1);
                    g.DrawString(lbl.Text, lbl.Font, new SolidBrush(Color.YellowGreen), lbl.Left + 1, lbl.Top - 1);

                    // Vẽ lại chữ với màu vàng gold
                    g.DrawString(lbl.Text, lbl.Font, new SolidBrush(Color.FromArgb(255, 255, 215, 0)), lbl.Left, lbl.Top);
                    var tachten = dsTen[i].tennguoichoi.Split(' ');
                    for (int j = 1; j < tachten.Length; j++)
                    {
                        ten = tachten[j - 1] + " " + tachten[j];
                    }
                    lbl.Text = ten;
                    /*for (int vt = 0; vt < 4; vt++)
                    {
                        if (dsTen[i].vitridoi == vt)
                        {
                            lbl.Text = ten;
                        }
                    }*/


                }
            }
            string spl1 = "Select doiid , sum(sodiem) as tongdiem from ds_diem GROUP BY doiid";
            DataTable dt1 = sqlObject.getDataFromSql(spl1, "").Tables[0];
            if (dt1.Rows.Count > 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        var lblTD = this.Controls.Find("lblTongDiem" + (i + 1), true).FirstOrDefault() as Label;
                        var dr1 = dt1.Select("doiid =" + (i + 1) + "");
                        if (dr1.Count() > 0)
                        {
                            lblTD.Text = dt1.Select("doiid =" + (i + 1) + "")[0]["tongdiem"].ToString();

                        }
                        else
                        {
                            lblTD.Text = "0";
                        }

                    }
                }

            }
        }

        private void pbMini_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        
        private void pbClose_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void frmTongDiem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();

            }
        }
    }
}
