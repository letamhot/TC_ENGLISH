﻿using System;
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
    public partial class fmHienThiChiTiet: Form
    {
        private Socket _socket;
        private int _cauhoiid;
        private QuaMienDiSanEntities _entities = new QuaMienDiSanEntities();
        public fmHienThiChiTiet()
        {
            InitializeComponent();
        }
        public fmHienThiChiTiet(Socket sock, int cauhoiid)
        {
            InitializeComponent();
            _socket = sock;
            _cauhoiid = cauhoiid;
            loadUC(_cauhoiid);
        }
        public void loadUC(int cauhoiid)
        {
            int cuocthiId = _entities.ds_cuocthi.FirstOrDefault(x => x.trangthai == true).cuocthiid;

            ds_cauhoithuthach ds = _entities.ds_cauhoithuthach.FirstOrDefault(x => x.cauhoiid == cauhoiid && x.cuocthiid == cuocthiId);
            if (ds != null)
            {
                labelDapAnCT.Text = "ANSWER QUESTION " + ds.vitri+":";
                labelDapAnCT.ForeColor = Color.DodgerBlue;
                txtDapAnCT.ForeColor = Color.Black;
                txtDapAnCT.Text = ds.dapantext;
                txtDapAnCT.Font = new Font("Arial", ds.dapantext.Length > 1000 ? 20 : ds.dapantext.Length < 100 ? 30 : 26, FontStyle.Bold);
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

        private void fmHienThiChiTiet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();

            }
        }
    }
}
