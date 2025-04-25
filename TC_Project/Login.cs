using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TC_Project
{
    public partial class Login : Form
    {
        SqlDataAccess sqlObj = new SqlDataAccess();
        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtUsername.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!");
                txtUsername.Focus();
                return;
            }
            else if (txtPass.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!");
                txtPass.Focus();
                return;
            }
            DataTable dt = new DataTable();
            string sql = "SELECT * FROM ds_doi WHERE tendangnhap = '" + txtUsername.Text + "' AND matkhau = '" + txtPass.Text + "'";
            dt = sqlObj.getDataFromSql(sql, "").Tables[0];
            if (dt.Rows.Count > 0)
            {
                Hide();
                TrinhChieu trinhChieu = new TrinhChieu(int.Parse(dt.Rows[0]["doiid"].ToString()));
                trinhChieu.FormClosed += new FormClosedEventHandler(trinhChieuForm_FormClosed);
                trinhChieu.ShowDialog();
                Close();
            }
            else
            {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!");
                txtUsername.Focus();
                return;
            }
        }

        private void trinhChieuForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            this.KeyDown += Login_KeyDown;
            groupBox1.BackColor = Color.FromArgb(100, 0, 0, 0);
            btnLogin.BackColor = Color.FromArgb(100, 0, 0, 0);
            btnExit.BackColor = Color.FromArgb(100, 0, 0, 0);

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
