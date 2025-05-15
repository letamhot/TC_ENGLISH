using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.Common.EntitySql;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_Project.Model;

delegate void AddMessage(string sNewMessage);

namespace TC_Project
{
    /*public class BufferedPictureBox : PictureBox
    {
        public BufferedPictureBox()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
        }
    }*/
    public partial class TrinhChieu : Form
    {
        QuaMienDiSanEntities _entities = new QuaMienDiSanEntities();
        private Socket sock;
        static string message;
        private int thoiGianConLai = 0;
        private int cuocthiId = 0;
        private byte[] byBuff = new byte[256];
        private string currentPath = Directory.GetCurrentDirectory();
        List<ds_goicaudiscovery> lsCauHoiPhuCP = new List<ds_goicaudiscovery>();
        private event AddMessage addMessage;
        int[] ttGoiKD = { 0, 0, 0, 0, 0, 0 };
        int[] ttGoiVD = { 0, 0, 0, 0, 0, 0 };
        int id = 0;
        ds_doi ds_Doi = new ds_doi();
        SqlDataAccess sqlObject = new SqlDataAccess();
        private Image imageChinhPhucChinh;
        private FormZoomImage _currentZoomForm;

        public TrinhChieu()
        {
            InitializeComponent();
            
        }

        public TrinhChieu(int doiId)
        {
            id = doiId;
            ds_Doi = _entities.ds_doi.Find(id);
            InitializeComponent();
            string sql = "SELECT noidungchude FROM ds_goicaudiscovery WHERE trangthai = 1 AND vitri = 0";
            string urlhinhanh = "";

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["QMDS_Connection"].ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    urlhinhanh = (string)command.ExecuteScalar();
                }
            }

            if (!string.IsNullOrEmpty(urlhinhanh))
            {
                imageChinhPhucChinh = Image.FromFile(currentPath + "\\Resources\\pic\\" + urlhinhanh);
                this.DoubleBuffered = true;

            }
            connecServer();
            addMessage = new AddMessage(OnAddMessage);
        }

        private void connecServer()
        {
            Cursor cursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (sock != null && sock.Connected)
                {
                    sock.Shutdown(SocketShutdown.Both);
                    System.Threading.Thread.Sleep(10);
                    sock.Close();
                }
                string server_ip = ConfigurationManager.AppSettings["IPServer"];
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint epServer = new IPEndPoint(IPAddress.Parse(server_ip), 399); //192.168.2.117
                sock.Blocking = false;
                AsyncCallback onconnect = new AsyncCallback(OnConnect);
                sock.BeginConnect(epServer, onconnect, sock);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Server Connect failed!");
            }
            Cursor.Current = cursor;
        }
        public void OnConnect(IAsyncResult ar)
        {

            Socket sock = (Socket)ar.AsyncState;
            try
            {
                if (sock.Connected)
                {
                    SetupRecieveCallback(sock);
                    SendEvent(id.ToString() + ",cli,connected,on");
                }
                else
                    MessageBox.Show(this, "khong cho phep connect den may o xa", "loi ket noi!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "loi khi thuc hien connect!");
            }
        }
        public void SetupRecieveCallback(Socket sock)
        {
            try
            {
                AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
                sock.BeginReceive(byBuff, 0, byBuff.Length, SocketFlags.None, recieveData, sock);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Setup Recieve Callback failed!");
            }
        }

        public void OnRecievedData(IAsyncResult ar)
        {
            Socket socks = (Socket)ar.AsyncState;
            try
            {
                int nBytesRec = socks.EndReceive(ar);
                if (nBytesRec > 0)
                {
                    string sRecieved = Encoding.ASCII.GetString(byBuff, 0, nBytesRec);

                    // Kiểm tra xem form đã được khởi tạo và có handle chưa
                    if (this.IsHandleCreated)
                    {
                        this.BeginInvoke(addMessage, new string[] { sRecieved });
                    }
                    else
                    {
                        Console.WriteLine("Handle chưa được tạo.");
                    }

                    SetupRecieveCallback(socks);
                }
                else
                {
                    Console.WriteLine("Client {0}, disconnected", socks.RemoteEndPoint);
                    socks.Shutdown(SocketShutdown.Both);
                    socks.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Lỗi xảy ra khi nhận kết quả trả về!");
            }
        }


        public void OnAddMessage(string sMessage)
        {
            //Cấu trúc tin nhắn từ server x,y,z
            //Trong đó x: id đội (x = 0: tin nhắn all broadcast, x = 5: MC, x = 6: Trình chiếu)
            //y: cli: tin nhắn từ thí sinh, mc, trình chiếu. ser: tin nhắn từ điều khiển chương trình
            layCuocThiHienTai();
            message = sMessage;
            string[] spl = message.Split(',');

            string src = spl[1];
            if (src.Equals("ser"))
            {
                if (spl[0] == "0")
                {

                    if (spl[2] == "playgioithieu")
                    {
                        onoffTimeMath(false);
                        onoffChinhPhuc(false);
                        onoffflowPanelSentences(false);
                        onOffUc(5, true);
                        //lblThoiGiankg.Visible = false;
                        hienthicauhoichinh(false);
                        //pnlNoiDung.Controls.Clear();
                        this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group6\\gt_qmds.png");
                        this.BackgroundImageLayout = ImageLayout.Stretch;




                    }

                    frmTongDiem frmTongDiem;
                    bool dapan = false;
                    if (spl[2] == "playkhoidong")
                    {
                        CloseFormsByName("frmTongDiem", "frmKhamGia", "frmDapAnKP", "frmtraloi", "frmDapAnChiTiet");

                        layCuocThiHienTai();
                        lblThoiGian.Enabled = true;
                        onoffKhanGia(true);
                        onoffChinhPhuc(false);
                        onoffflowPanelSentences(false);

                        onoffTimeMath(false);
                        lblThoiGian.Visible = true;

                        //lblThoiGiankg.Visible = false;
                        //onoffLabelCauTraLoiKD(false);
                        //pnlNoiDung.Controls.Clear();
                        this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group\\kd_tl.png");
                        this.BackgroundImageLayout = ImageLayout.Stretch;
                        onOffUc(1, true);
                        pnlDiemSo.Visible = false;
                        lblThoiGian.Visible = false;
                        hienthicauhoichinh(false);


                    }
                    if (spl[2] == "playthuthach")
                    {
                        layCuocThiHienTai();
                        // Giả sử frmTongDiem là form con, kiểm tra và đóng form nếu đang mở
                        CloseFormsByName("frmTongDiem", "frmKhamGia", "frmDapAnKP", "frmtraloi", "frmDapAnChiTiet");
                        lblThoiGian.Visible = true;
                        onoffKhanGia(true);
                        hienthicauhoichinh(false);
                        onoffChinhPhuc(false);
                        onoffflowPanelSentences(true);

                        onoffTimeMath(true);
                        frmDapAnKP frmDapAnKP;
                        fmHienThiChiTiet fmhienthichitiet;

                        lblThoiGian.Location = new Point(64, 277);

                        //lblThoiGiankg.Visible = false;

                        if (spl[3] == "0")
                        {

                            //frmTongDiem = new frmTongDiem();
                            //this.Hide();
                            //pnlNoiDung.Controls.Clear();
                            this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group\\kp_tl.png");
                            this.BackgroundImageLayout = ImageLayout.Stretch;
                            onOffUc(2, true);
                            pnlDiemSo.Visible = false;
                            lblThoiGian.Visible = false;
                            //this.Show();
                        }
                        else
                        {
                            pnlDiemSo.Visible = true;
                            lblThoiGian.Visible = true;
                            layCuocThiHienTai();
                            //this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group6\\tc_mc_kp.png");
                            //this.BackgroundImageLayout = ImageLayout.Stretch;
                            onOffUc(2, false);
                            thoiGianConLai = 30;
                            if (spl[4] == "ready")
                            {

                                // Giả sử frmTongDiem là form con, kiểm tra và đóng form nếu đang mở
                                //if (Application.OpenForms["frmDapAnKP"] != null)
                                //{
                                //    Application.OpenForms["frmDapAnKP"].Close();
                                //}
                                //// Giả sử frmTongDiem là form con, kiểm tra và đóng form nếu đang mở
                                //if (Application.OpenForms["frmDapAnKP"] != null)
                                //{
                                //    Application.OpenForms["frmDapAnKP"].Close();
                                //}
                                CloseFormsByName("frmDapAnKP", "frmKhamGia");

                                //frmTongDiem = new frmTongDiem();
                                //this.Hide();
                                //frmDapAnKP = new frmDapAnKP();
                                //this.Hide();
                                this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group6\\tc_mc_kp.png");
                                this.BackgroundImageLayout = ImageLayout.Stretch;
                                timerTC.Enabled = false;
                                //onoffLabelCauTraLoiKP(false);
                                thoiGianConLai = 30;
                                lblThoiGian.Text = thoiGianConLai.ToString();
                                /*pnlNoiDung.Controls.Clear();
                                pnlNoiDung.Controls.Add(new ucKhamPha(sock, id, int.Parse(spl[3]), false, 0));*/
                                processThuThach(id, int.Parse(spl[3]), false, 0);
                                //this.Show();
                            }
                            if (spl[4] == "start")
                            {
                                /*pnlNoiDung.Controls.Clear();
                                pnlNoiDung.Controls.Add(new ucKhamPha(sock, id, int.Parse(spl[3]), true, int.Parse(spl[5])));*/
                                processThuThach(id, int.Parse(spl[3]), true, int.Parse(spl[5]));
                                //Thread.Sleep(1000);
                                timerTC.Enabled = true;
                                System.Media.SoundPlayer sound = new System.Media.SoundPlayer(currentPath + "\\Resources\\30s1.wav");
                                //Windows_XP_Menu_Command_SuKienClickButtom là file wav đã add ref vào Reources
                                sound.Play();
                            }
                            if (spl[4] == "stop")
                            {
                                // Giả sử frmTongDiem là form con, kiểm tra và đóng form nếu đang mở
                                //if (Application.OpenForms["fmHienThiChiTiet"] != null)
                                //{
                                //    Application.OpenForms["fmHienThiChiTiet"].Close();
                                //}
                                CloseFormsByName("fmHienThiChiTiet");

                                frmDapAnKP = new frmDapAnKP(sock, int.Parse(spl[3]), false);
                                frmDapAnKP.Show();
                                frmDapAnKP.Focus();
                                timerTC.Enabled = false;
                                thoiGianConLai = 30;
                                lblThoiGian.Text = thoiGianConLai.ToString() ;
                            }
                            if (spl[4] == "hienthidiemKP")
                            {
                                // Giả sử frmTongDiem là form con, kiểm tra và đóng form nếu đang mở
                                //if (Application.OpenForms["fmHienThiChiTiet"] != null)
                                //{
                                //    Application.OpenForms["fmHienThiChiTiet"].Close();
                                //}
                                CloseFormsByName("fmHienThiChiTiet");

                                frmDapAnKP = new frmDapAnKP(sock, int.Parse(spl[3]), true);
                                frmDapAnKP.Show();
                                frmDapAnKP.Focus();
                                timerTC.Enabled = false;
                            }
                            if (spl[4] == "hienthidapanCT")
                            {
                                fmhienthichitiet = new fmHienThiChiTiet(sock, int.Parse(spl[3]));
                                fmhienthichitiet.Show();
                                timerTC.Enabled = false;
                            }
                            if (spl[4] == "capNhatDienManHinhTT")
                            {
                                layCuocThiHienTai();
                                processThuThach(id, int.Parse(spl[3]), false, 0);
                            }
                        }
                        lblThoiGian.Text = thoiGianConLai.ToString();
                        lblThoiGian.Font = new Font("Showcard Gothic", 58, FontStyle.Bold);

                    }
                    if (spl[2] == "playkhamphachiase")
                    {
                        layCuocThiHienTai();
                        // Giả sử frmTongDiem là form con, kiểm tra và đóng form nếu đang mở
                        CloseFormsByName("frmTongDiem", "frmKhamGia", "frmDapAnKP", "frmtraloi", "frmDapAnChiTiet");

                        onoffKhanGia(true);
                        onoffflowPanelSentences(false);
                        frmKhamGia frmKhamGia;
                        if (spl[3] == "0")
                        {

                            //frmTongDiem = new frmTongDiem();
                            //frmKhamGia = new frmKhamGia();
                            //this.Hide();
                            this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group\\cp_tl.png");
                            this.BackgroundImageLayout = ImageLayout.Stretch;
                            onOffUc(3, true);
                            //pnlNoiDung.Controls.Clear();
                            pnlDiemSo.Visible = false;
                            lblThoiGian.Visible = false;
                            hienthicauhoichinh(false);


                            //this.Show();
                        }
                        else
                        {
                            
                            onoffChinhPhuc(true);
                            hienthicauhoichinh(true);
                            lblThoiGian.Visible = true;
                            pnAnhCauHoi.Visible = false;
                            lblThoiGian.Location = new Point(84, 271);
                            lblThoiGian.ForeColor = Color.White;
                            onOffUc(3, false);
                            bool diemGK = false;
                            bool trangthailat = false;
                            if (spl[5] == "ready")
                            {
                                // Giả sử frmTongDiem là form con, kiểm tra và đóng form nếu đang mở
                                
                                CloseFormsByName("FormZoomImage");

                                timerTC.Enabled = false;
                                thoiGianConLai = 180;
                                lblThoiGian.Text = thoiGianConLai.ToString();

                                if (int.Parse(spl[4]) == 0)
                                {
                                    this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group6\\tc_mc_cp.png");
                                    this.BackgroundImageLayout = ImageLayout.Stretch;
                                    processKhamPhaChiaSe(id, int.Parse(spl[3]), int.Parse(spl[4]), false, false, trangthailat, diemGK, true);
                                }
                                else
                                {
                                    processKhamPhaChiaSe(id, int.Parse(spl[3]), int.Parse(spl[4]), false, false, trangthailat, diemGK, true);
                                }

                            }
                            if (spl[5] == "start")
                            {
                                int cauhoiId = Convert.ToInt32(spl[3]);
                                var khamPha = _entities.ds_goicaudiscovery.FirstOrDefault(x => x.cauhoiid == cauhoiId && x.trangthai == true);
                                if (!string.IsNullOrWhiteSpace(khamPha.noidungthisinh))
                                {
                                    timerTC.Enabled = true;
                                    load6NutMacDinh();
                                    ReloadPanelAndPictures();
                                    processKhamPhaChiaSe(id, int.Parse(spl[3]), int.Parse(spl[4]), true, true, true, diemGK, false);


                                }
                                else
                                {
                                    timerTC.Enabled = true;
                                    processKhamPhaChiaSe(id, int.Parse(spl[3]), int.Parse(spl[4]), false, false, trangthailat, diemGK, false);

                                }

                            }
                            if (spl[5] == "stopTime")
                            {
                                // Giả sử frmTongDiem là form con, kiểm tra và đóng form nếu đang mở
                                CloseFormsByName("FormZoomImage");

                                int cauhoiId = Convert.ToInt32(spl[3]);
                                var khamPha = _entities.ds_goicaudiscovery.FirstOrDefault(x => x.cauhoiid == cauhoiId && x.trangthai == true);
                                if (!string.IsNullOrWhiteSpace(khamPha.noidungthisinh))
                                {
                                    timerTC.Enabled = false;

                                    processKhamPhaChiaSe(id, int.Parse(spl[3]), int.Parse(spl[4]), false,true, trangthailat, diemGK, true);
                                }
                                else
                                {
                                    timerTC.Enabled = false;

                                    processKhamPhaChiaSe(id, int.Parse(spl[3]), int.Parse(spl[4]), false, false, trangthailat, diemGK, true);
                                }

                            }
                            if (spl[5] == "hienthianhthisinh")
                            {
                                this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group6\\tc_mc_cp.png");
                                this.BackgroundImageLayout = ImageLayout.Stretch;
                                processKhamPhaChiaSe(id, int.Parse(spl[3]), int.Parse(spl[4]), false, true, trangthailat, diemGK, true);
                            }
                            if (spl[5] == "hienthimanh")
                            {
                                processKhamPhaChiaSe(id, int.Parse(spl[3]), int.Parse(spl[4]), true, false, trangthailat, diemGK, true);
                            }
                            if (spl[5] == "load6nut")
                            {
                                ReloadPanelAndPictures();
                                load6NutMacDinh(); // <- gọi ở đây
                                processKhamPhaChiaSe(id, int.Parse(spl[3]), int.Parse(spl[4]), true, false,true, diemGK, true);
                            }
                            if (spl[5] == "capnhatTongDiem")
                            {
                                int cauhoiId = Convert.ToInt32(spl[3]);
                                var khamPha = _entities.ds_goicaudiscovery.FirstOrDefault(x => x.cauhoiid == cauhoiId && x.trangthai == true);
                                if (!string.IsNullOrWhiteSpace(khamPha.noidungthisinh))
                                {
                                    processKhamPhaChiaSe(id, int.Parse(spl[3]), int.Parse(spl[4]), false, true, trangthailat, true, true);
                                    layCuocThiHienTai();
                                }
                                else
                                {
                                    processKhamPhaChiaSe(id, int.Parse(spl[3]), int.Parse(spl[4]), false, false, trangthailat, true, true);
                                    layCuocThiHienTai();
                                }

                            }

                        }
                        lblThoiGian.Text = thoiGianConLai.ToString();

                    }
                    if (spl[2] == "playtoasang")
                    {
                        layCuocThiHienTai();
                        if (spl[5] == "0")
                        {
                            // Giả sử frmTongDiem là form con, kiểm tra và đóng form nếu đang mở
                            
                            CloseFormsByName("frmTongDiem", "frmKhamGia","frmDapAnKP", "frmtraloi", "frmDapAnChiTiet");


                            lblThoiGian.Visible = false;
                            hienthicauhoichinh(false);
                            onoffKhanGia(false);
                            onoffChinhPhuc(false);
                            onoffTimeMath(false);
                            onoffflowPanelSentences(false);

                            this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group\\vd_tl.png");
                            this.BackgroundImageLayout = ImageLayout.Stretch;
                            onOffUc(4, true);
                            //pnlNoiDung.Controls.Clear();
                            pnlDiemSo.Visible = false;
                            hienthicauhoichinh(false);
                        }
                        else
                        {
                            layCuocThiHienTai();

                            thoiGianConLai = 20;
                            lblThoiGian.Text = thoiGianConLai.ToString();
                            onOffUc(4, false);
                            lblThoiGian.Location = new Point(64, 277);
                            lblThoiGian.ForeColor = Color.White;

                            hienthicauhoichinh(false);
                            onoffChinhPhuc(false);
                            onoffTimeMath(true);
                            onoffflowPanelSentences(false);
                            bool x2 = false;
                            bool da = false;
                            bool tt = false;
                            bool tt_file = false;
                            onoffKhanGia(true);
                            this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group6\\tc_mc_vd.png");
                            this.BackgroundImageLayout = ImageLayout.Stretch;
                            if (spl[5] == "start")
                            {
                                timerTC.Enabled = true;
                                tt = true;
                                tt_file = false;
                                System.Media.SoundPlayer sound = new System.Media.SoundPlayer(currentPath + "\\Resources\\20s.wav");
                                sound.Play();



                                processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, false, false, false, false, false);
                            }
                            if (spl[5] == "start_ngoisaohivong")
                            {
                                ds_goicauhoishining vd = _entities.ds_goicauhoishining.Find(int.Parse(spl[4]));
                                x2 = true;
                                System.Media.SoundPlayer sound = new System.Media.SoundPlayer(currentPath + "\\Resources\\ngoisao.wav");
                                sound.Play();
                                loadNutDangChon(int.Parse(spl[4]), true);
                                processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, x2, false, true, false, true);


                            }

                            if (spl[5] == "ready")
                            {

                                timerTC.Enabled = false;
                                tt = false;
                                tt_file = true;
                                x2 = false;
                                da = false;
                                thoiGianConLai = 20;
                                if (int.Parse(spl[4]) != 0)
                                {
                                    var cauhoiVD = _entities.ds_goicauhoishining.Find(int.Parse(spl[4]));

                                    lblThoiGian.Text = thoiGianConLai.ToString();
                                    processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, false, false, true, false, true);

                                }
                                else
                                {
                                    this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group6\\tc_mc_vd.png");
                                    this.BackgroundImageLayout = ImageLayout.Stretch;
                                    processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, false, false, true, false, true);
                                }

                            }
                            if (spl[5] == "hienthi5NutCauHoi")
                            {
                                this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group6\\tc_mc_vd.png");
                                this.BackgroundImageLayout = ImageLayout.Stretch;
                                timerTC.Enabled = false;
                                processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, false, false, false, false);
                            }
                            else if (spl[5] == "forceanswer")
                            {
                                // Hiển thị đáp án khi thí sinh 1 trả lời đúng hoặc thí sinh 2 dành quyền
                                layCuocThiHienTai();
                                da = true;
                                if (x2)
                                {
                                    processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, x2, da, false, false, false);

                                }
                                else
                                {
                                    processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, x2, da, false, false, false);

                                }
                            }

                        }

                        lblThoiGian.Text = thoiGianConLai.ToString();
                        lblThoiGian.Font = new Font("Showcard Gothic", 58, FontStyle.Bold);

                    }
                    if (spl[2] == "playkhangia")
                    {
                        
                        CloseFormsByName("frmDapAnKP");


                        frmKhamGia frmKhamGia;
                        /*this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group6\\tc_mc_kg.jpg");
                        this.BackgroundImageLayout = ImageLayout.Stretch;*/
                        onoffKhanGia(false);
                        onoffTimeMath(false);
                        onoffChinhPhuc(false);

                        if (spl[3] == "0")
                        {
                            /* frmTongDiem = new frmTongDiem();
                             this.Hide();*/
                            frmKhamGia = new frmKhamGia(sock, 0, 0, false);
                            frmKhamGia.Show();
                            frmKhamGia.Focus();
                            /*pnlNoiDung.Controls.Clear();
                            pnlNoiDung.Controls.Add(new ucKhanGia(sock, 0, 0, false));*/
                            //this.Show();
                        }
                        else
                        {
                            if (spl[5] == "ready")
                            {/*
                                frmTongDiem = new frmTongDiem();
                                this.Hide();*/
                                frmKhamGia = new frmKhamGia(sock, int.Parse(spl[3]), int.Parse(spl[4]), false);
                                frmKhamGia.Show();
                                frmKhamGia.Focus();
                                /*pnlNoiDung.Controls.Clear();
                                pnlNoiDung.Controls.Add(new ucKhanGia(sock, int.Parse(spl[3]), int.Parse(spl[4]), false));*/
                                //this.Show();
                            }
                            if (spl[5] == "start")
                            {
                                timerTC.Enabled = true;
                                dapan = false;
                                frmKhamGia = new frmKhamGia(sock, int.Parse(spl[3]), int.Parse(spl[4]), dapan);
                                frmKhamGia.Show();
                                /* pnlNoiDung.Controls.Clear();
                                 pnlNoiDung.Controls.Add(new ucKhanGia(sock, int.Parse(spl[3]), int.Parse(spl[4]), dapan));*/

                            }
                            if (spl[5] == "hienthidapan")
                            {
                                timerTC.Enabled = false;
                                dapan = true;
                                //thoiGianConLai = 10;
                                frmKhamGia = new frmKhamGia(sock, int.Parse(spl[3]), int.Parse(spl[4]), dapan);
                                frmKhamGia.Show();
                                frmKhamGia.Focus();

                                /* pnlNoiDung.Controls.Clear();
                                 pnlNoiDung.Controls.Add(new ucKhanGia(sock, int.Parse(spl[3]), int.Parse(spl[4]), dapan));*/

                            }

                            //lblThoiGiankg.Text = thoiGianConLai.ToString();
                        }




                    }
                    if (spl[2] == "tongdiem")
                    {
                        CloseFormsByName("frmDapAnKP", "frmtraloi", "frmKhamGia");
                        frmTongDiem = new frmTongDiem();
                        frmTongDiem.Show();
                        pnlDiemSo.Visible = false;
                        lblThoiGian.Visible = false;
                        frmTongDiem.Focus();

                    }
                }
                else
                {
                    frmTongDiem frmTongDiem;
                    if (spl[2] == "tongdiem")
                    {
                        CloseFormsByName("frmDapAnKP", "frmtraloi", "frmKhamGia");

                        
                        frmTongDiem = new frmTongDiem();
                        frmTongDiem.Show();
                        //pnlDiemSo.Visible = false;
                        //lblThoiGian.Visible = false;
                    }
                    if (spl[2] == "playkhoidong")
                    {
                        layCuocThiHienTai();
                        //pnlKhoiDong.Visible = true;
                        //pnlNoiDung.Visible = false;
                        onOffUc(1, false);
                        hienthicauhoichinh(false);
                        onoffChinhPhuc(false);
                        onoffflowPanelSentences(false);

                        onoffTimeMath(true);
                        lblThoiGian.Location = new Point(64, 277);
                        lblThoiGian.Font = new Font("Showcard Gothic", 58, FontStyle.Bold);
                        lblThoiGian.ForeColor = Color.White;
                        onOffUc(1, false);
                        frmtraloi frmtraloi;
                        onoffKhanGia(true);
                        lblThoiGian.Font = new Font("Showcard Gothic", 58, FontStyle.Bold);
                        if (spl[5] == "start")
                        {
                            thoiGianConLai = 60;
                            timerTC.Enabled = true;
                            //start = true;
                            System.Media.SoundPlayer sound = new System.Media.SoundPlayer(currentPath + "\\Resources\\nhacKD.wav");
                            //Windows_XP_Menu_Command_SuKienClickButtom là file wav đã add ref vào Reources
                            sound.Play();
                            /*pnlNoiDung.Controls.Clear();
                            pnlNoiDung.Controls.Add(new ucKhoiDong(sock, int.Parse(spl[0]), int.Parse(spl[3]), int.Parse(spl[4]), ttGoiKD, false));*/
                            processKhoiDong(int.Parse(spl[0]), int.Parse(spl[3]), int.Parse(spl[4]), ttGoiKD, false);
                            lblThoiGian.Text = thoiGianConLai.ToString();

                        }
                        else if (spl[5] == "stop")
                        {
                            // kỳ thêm
                            ttGoiKD[int.Parse(spl[4]) - 1] = 1;
                            thoiGianConLai = 60;
                            timerTC.Enabled = false;
                            //start = false;
                            layCuocThiHienTai();
                            thoiGianConLai = 60;
                            lblThoiGian.Text = thoiGianConLai.ToString();
                            System.Media.SoundPlayer sound = new System.Media.SoundPlayer(currentPath + "\\Resources\\nhacKD.wav");
                            //Windows_XP_Menu_Command_SuKienClickButtom là file wav đã add ref vào Reources
                            sound.Stop();
                            /*pnlNoiDung.Controls.Clear();
                            pnlNoiDung.Controls.Add(new ucKhoiDong(sock, int.Parse(spl[0]), int.Parse(spl[3]), int.Parse(spl[4]), ttGoiKD, true));*/
                            processKhoiDong(int.Parse(spl[0]), int.Parse(spl[3]), int.Parse(spl[4]), ttGoiKD);
                        }
                        else if (spl[5] == "hienthicautraloi")
                        {
                            ds_doi teamplaying = _entities.ds_doi.Find(int.Parse(spl[0]));
                            ds_doi teamnext = _entities.ds_doi.Where(x => x.vitridoi == teamplaying.vitridoi + 1).FirstOrDefault();
                            lblthele.Text = teamnext != null
                            ? $"Congratulations to candidate {teamplaying.tennguoichoi.ToString().ToUpper()} completed the Warm-up section\nCandidate {teamnext.tennguoichoi.ToString().ToUpper()} preparing for the section"
                            : $"Congratulations to candidate {teamplaying.tennguoichoi.ToString().ToUpper()} has completed the Warm-up section";
                            
                            thoiGianConLai = 60;
                            timerTC.Enabled = false;
                            frmtraloi = new frmtraloi(sock, int.Parse(spl[0]), int.Parse(spl[3]), int.Parse(spl[4]), ttGoiKD, false);
                            frmtraloi.Show();
                            frmtraloi.Focus();
                            lblThoiGian.Text = thoiGianConLai.ToString();
                        }
                        else if (spl[5] == "ready")
                        {
                            // Giả sử frmTongDiem là form con, kiểm tra và đóng form nếu đang mở
                            //if (Application.OpenForms["frmtraloi"] != null)
                            //{
                            //    Application.OpenForms["frmtraloi"].Close();
                            //}
                            CloseFormsByName("frmtraloi");

                            if (int.Parse(spl[4]) > 0)
                            {

                                ttGoiKD[int.Parse(spl[4]) - 1] = 2;
                            }
                            else
                            {
                                this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group6\\tc_mc_kd.png");
                                this.BackgroundImageLayout = ImageLayout.Stretch;
                            }

                            thoiGianConLai = 60;
                            timerTC.Enabled = false;
                            processKhoiDong(int.Parse(spl[0]), int.Parse(spl[3]), int.Parse(spl[4]), ttGoiKD);
                            lblThoiGian.Text = thoiGianConLai.ToString();
                        }
                        else if (spl[5] == "next")
                        {
                            processKhoiDong(int.Parse(spl[0]), int.Parse(spl[3]), int.Parse(spl[4]), ttGoiKD, false);
                        }
                        else
                        {
                            processKhoiDong(int.Parse(spl[0]), int.Parse(spl[3]), int.Parse(spl[4]), ttGoiKD);
                        }


                    }
                    if (spl[2] == "playtoasang")
                    {
                        layCuocThiHienTai();
                        thoiGianConLai = 20;
                        lblThoiGian.Text = thoiGianConLai.ToString() ;
                        onOffUc(4, false);
                        lblThoiGian.Location = new Point(64, 277);
                        lblThoiGian.ForeColor = Color.White;

                        hienthicauhoichinh(false);
                        onoffChinhPhuc(false);
                        onoffTimeMath(true);
                        onoffflowPanelSentences(false);
                        bool x2 = false;
                        bool da = false;
                        bool tt = false;
                        bool tt_file = false;

                        if (spl.Length > 5 && spl[5] == "showanswer")
                        {
                            // Hiển thị đáp án khi thí sinh 1 trả lời đúng hoặc thí sinh 2 dành quyền
                            layCuocThiHienTai();
                            da = true;
                            if (x2)
                            {
                                processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, x2, da, false, false, false);

                            }
                            else
                            {
                                processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, x2, da, false, false, false);
                            }
                        }

                        else if (spl.Length > 5 && spl[5] == "noanswer")
                        {
                            // Không hiển thị đáp án khi thí sinh 1 trả lời sai
                            layCuocThiHienTai();
                            da = false;
                            if (x2)
                            {
                                processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, x2, da, false, false, false);

                            }
                            else
                            {
                                processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, x2, da, false, false, false);

                            }
                        }
                        else if (spl[5] == "capNhatDiemManHinhTS")
                        {
                            da = false;
                            layCuocThiHienTai();
                            processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, x2, da, false, false, false);

                        }
                        else if (spl[5] == "start_ngoisaohivong")
                        {
                            int cauhoiId = Convert.ToInt32(spl[4]);
                            ds_goicauhoishining vd = _entities.ds_goicauhoishining.Find(cauhoiId);
                            x2 = true;
                            System.Media.SoundPlayer sound = new System.Media.SoundPlayer(currentPath + "\\Resources\\ngoisao.wav");
                            sound.Play();
                            loadNutDangChon(cauhoiId, x2);
                            processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, x2, false, true, false, false);


                        }
                        else if (spl[5] == "start_Nongoisaohivong")
                        {
                            int cauhoiId = Convert.ToInt32(spl[4]);
                            ds_goicauhoishining vd = _entities.ds_goicauhoishining.Find(cauhoiId);
                            x2 = false;
                            
                            loadNutDangChon(cauhoiId, x2);
                            processToaSang(int.Parse(spl[0]), int.Parse(spl[4]), tt, x2, false, true, false, false);

                        }


                        lblThoiGian.Text = thoiGianConLai.ToString() ;
                    }
                }
            }

        }
        public void CloseFormsByName(params string[] formNames)
        {
            foreach (Form form in Application.OpenForms.Cast<Form>().ToList())
            {
                if (formNames.Contains(form.Name))
                {
                    form.Close();
                }
            }
        }

        private void SendEvent(string str)
        {
            // Check we are connected
            if (sock == null || !sock.Connected)
            {
                MessageBox.Show(this, "Must be connected to Send a message");
                return;
            }
            // Read the message from the text box and send it
            try
            {
                // Convert to byte array and send.
                Byte[] byteDateLine = Encoding.ASCII.GetBytes(str.ToCharArray());
                sock.Send(byteDateLine, byteDateLine.Length, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Send lenh dieu khien loi!");
            }

        }

        private void timerTC_Tick(object sender, EventArgs e)
        {
            if (thoiGianConLai > 1)
            {
                thoiGianConLai = thoiGianConLai - 1;
                lblThoiGian.Text = thoiGianConLai.ToString();
            }
            else
            {
                if (!string.IsNullOrEmpty(message))
                {
                    string[] spl = message.Split(',');

                    // Đảm bảo mảng có ít nhất 3 phần tử trước khi truy cập spl[2]
                    if (spl.Length > 2 && spl[2] == "playkhamphachiase")
                    {
                        thoiGianConLai--; // Giảm thêm 1 giây nếu điều kiện đúng
                        lblThoiGian.Text = thoiGianConLai.ToString();
                    }
                    else
                    {
                        timerTC.Enabled = false;
                        lblThoiGian.Text = "END";
                        lblThoiGian.ForeColor = Color.Red;
                    }
                }
                lblThoiGian.Font = new Font("Showcard Gothic", 58, FontStyle.Bold);

            }
        }
        private void hienthicauhoichinh(bool onoff)
        {
            string sqlGetCuocthiId = "SELECT cuocthiid FROM ds_cuocthi WHERE trangthai = 1";
            string sqlGetNoiDungCauHoi = "SELECT chude FROM ds_goicaudiscovery WHERE cauhoichaid IS NULL AND cuocthiid = @CuocthiId AND trangthai = 1";
            string sqlGetUrlHinhAnh = "SELECT noidungchude FROM ds_goicaudiscovery WHERE cauhoichaid IS NULL AND cuocthiid = @CuocthiId AND trangthai = 1";
            string sqlGetUrlHinhAnhTS = "SELECT noidungthisinh FROM ds_goicaudiscovery WHERE cauhoichaid IS NULL AND cuocthiid = @CuocthiId AND trangthai = 1";

            int cuocthiId = 0;
            string noidungCauHoi = "";
            string urlhinhanhchude = "";
            string urlhinhanhthisinh = "";
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["QMDS_Connection"].ConnectionString))
            {
                connection.Open();

                try
                {
                    // Lấy cuocthiid
                    using (var command = new SqlCommand(sqlGetCuocthiId, connection))
                    {
                        cuocthiId = (int)command.ExecuteScalar();
                    }

                    if (cuocthiId > 0)
                    {
                        // Lấy nội dung câu hỏi chính
                        using (var command = new SqlCommand(sqlGetNoiDungCauHoi, connection))
                        {
                            command.Parameters.AddWithValue("@CuocthiId", cuocthiId);
                            noidungCauHoi = command.ExecuteScalar() as string;
                        }

                        // Lấy URL hình ảnh chủ đề
                        using (var command = new SqlCommand(sqlGetUrlHinhAnh, connection))
                        {
                            command.Parameters.AddWithValue("@CuocthiId", cuocthiId);
                            urlhinhanhchude = command.ExecuteScalar() as string;
                        }

                        // Lấy URL hình ảnh thí sinh (có thể null)
                        using (var command = new SqlCommand(sqlGetUrlHinhAnhTS, connection))
                        {
                            command.Parameters.AddWithValue("@CuocthiId", cuocthiId);
                            var result = command.ExecuteScalar();
                            urlhinhanhthisinh = result != DBNull.Value ? (string)result : null;
                        }
                    }
                }
                finally
                {
                    // Đóng kết nối khi hoàn tất
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }



            if (!string.IsNullOrEmpty(noidungCauHoi))
            {
                //lblCauhoichinhCP.Text = noidungCauHoi;

                string imagePath;

                if (!string.IsNullOrWhiteSpace(urlhinhanhthisinh) && File.Exists(currentPath + "\\Resources\\pic\\" + urlhinhanhthisinh))
                {
                    imagePath = Path.Combine(currentPath, "Resources", "pic", urlhinhanhthisinh);
                }
                else
                {
                    imagePath = Path.Combine(currentPath, "Resources", "pic", urlhinhanhchude);
                }

                imageChinhPhucChinh = Image.FromFile(imagePath);
            }
        }

        private void onoffChinhPhuc(bool onoff)
        {
            pnlDiemSoCP.Visible = onoff;
        }
        private void onoffTimeMath(bool onoff)
        {
            pnlDiemSo.Visible = onoff;
            lblThoiGian.Visible = onoff;
        }
        private void onoffKhanGia(bool onoff)
        {
            lblThoiGian.Visible = onoff;
            lblThoiGian.Enabled = onoff;
            label1.Visible = onoff;
            label1.Enabled = onoff;
            label2.Visible = onoff;
            label2.Enabled = onoff;
            label3.Visible = onoff;
            label3.Enabled = onoff;
            label4.Visible = onoff;
            label4.Enabled = onoff;
            lblTongDiem1.Visible = onoff;
            lblTongDiem1.Enabled = onoff;
            lblTongDiem2.Visible = onoff;
            lblTongDiem2.Enabled = onoff;
            lblTongDiem3.Visible = onoff;
            lblTongDiem3.Enabled = onoff;
            lblTongDiem4.Visible = onoff;
            lblTongDiem4.Enabled = onoff;
            ds_cuocthi dscuocthi = _entities.ds_cuocthi.FirstOrDefault(x => x.trangthai == true);
            var ten = "";
            var dsTen = _entities.ds_doi.Where(x => x.vaitro == "TS" && x.cuocthiid == dscuocthi.cuocthiid).ToList();
            if (dsTen != null && dsTen.Count > 0)
            {
                for (int i = 0; i < dsTen.Count; i++)
                {
                    var lbl = this.Controls.Find("label" + (i + 1), true).FirstOrDefault() as Label;
                    var lbl1 = this.Controls.Find("ts" + (i + 1), true).FirstOrDefault() as Label;
                    var tachten = dsTen[i].tennguoichoi.Split(' ');
                    for (int j = 1; j < tachten.Length; j++)
                    {
                        ten = tachten[j - 1] + " " + tachten[j];
                    }
                    lbl.Text = ten;
                    lbl1.Text = ten;
                    lbl.Text = lbl.Text.ToUpper();
                    lbl1.Text = lbl.Text.ToUpper();


                }
            }


        }
        private void layCuocThiHienTai()
        {
            lblTongDiem1.Text = lblTongDiem2.Text = lblTongDiem3.Text = lblTongDiem4.Text = "0";
            lblDiem1.Text = lblDiem2.Text = lblDiem3.Text = lblDiem4.Text = "0";

            ds_cuocthi cuocThi = _entities.ds_cuocthi.FirstOrDefault(x => x.trangthai == true);
            if (cuocThi != null)
            {
                cuocthiId = cuocThi.cuocthiid;
                string sql = "SELECT doiid, sum(sodiem) as tongdiem from ds_diem WHERE cuocthiid = " + cuocthiId + " GROUP BY cuocthiid, doiid";
                DataTable dt = sqlObject.getDataFromSql(sql, "").Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        ds_doi doiChoi = _entities.ds_doi.Find(int.Parse(dr["doiid"].ToString()));
                        if (doiChoi != null)
                        {
                            if (doiChoi.vitridoi == 1)
                            {
                                lblTongDiem1.Text = dr["tongdiem"].ToString();
                                lblDiem1.Text = dr["tongdiem"].ToString();
                            }
                            if (doiChoi.vitridoi == 2)
                            {
                                lblTongDiem2.Text = dr["tongdiem"].ToString();
                                lblDiem2.Text = dr["tongdiem"].ToString();
                            }
                            if (doiChoi.vitridoi == 3)
                            {
                                lblTongDiem3.Text = dr["tongdiem"].ToString();
                                lblDiem3.Text = dr["tongdiem"].ToString();
                            }
                            if (doiChoi.vitridoi == 4)
                            {
                                lblTongDiem4.Text = dr["tongdiem"].ToString();
                                lblDiem4.Text = dr["tongdiem"].ToString();
                            }
                        }
                    }
                }

            }
            _entities.SaveChanges();

        }
        private void TrinhChieu_Load(object sender, EventArgs e)
        {
            //onoffLabelCauTraLoiKD(false);
            onoffTimeMath(false);
            onoffChinhPhuc(false);


            this.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group6\\gt_qmds.png");
            this.BackgroundImageLayout = ImageLayout.Stretch;
            pbClose.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\close11.png");
            pbMini.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\mini1.png");
            this.BackgroundImageLayout = ImageLayout.Stretch;

        }

        private void pbClose_Click(object sender, EventArgs e)
        {
            try
            {
                // Gửi thông điệp đóng nếu cần
                SendEvent(id.ToString() + ",cli,connected,off");

                // Đảm bảo socket đã khởi tạo và chưa bị dispose
                if (sock != null && sock.Connected)
                {
                    try
                    {
                        sock.Shutdown(SocketShutdown.Both);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("Socket shutdown error: " + ex.Message);
                    }

                    try
                    {
                        sock.Close();
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("Socket close error: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on pbClose: " + ex.Message);
            }
            finally
            {
                // Thoát ứng dụng sau khi đã xử lý mọi thứ
                Application.Exit();
            }
        }


        private void pbMini_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void processKhoiDong(int doiid, int cauhoiid, int goicauhoiid, int[] ttgoi, bool loadButton = true)
        {
            onoffflowPanelSentences(false);
            ds_doi teamplaying = _entities.ds_doi.Find(doiid);
            if (goicauhoiid == 0)
            {
                invisibleGui();
                //lblthele.Text = "Thể lệ phần thi:";
                lblthele.Text = "Question packages!";
            }
            else
            {
                if (cauhoiid > 0)
                {
                    //flpNoiDung.Visible = false;
                    //txtNoiDungCauHoi.Text = _entities.ds_goicauhoikhoidong.Find(_cauhoiid).noidungcauhoi;
                    visibleGui();
                    if (teamplaying != null)
                    {
                        lblthele.Text = "Candidate " + teamplaying.tennguoichoi.ToUpper() + " is doing the section";
                    }
                    ds_goicauhoikhoidong cauhoi = _entities.ds_goicauhoikhoidong.Find(cauhoiid);
                    _entities.Entry(cauhoi).Reload(); // ⚠️ Nạp lại từ DB

                    lblNoiDungCauHoi.Text = cauhoi.noidungcauhoi;
                    labelNoiDungCauHoi.Text = "Question " + cauhoi.vitri + ":";
                }
                else
                {
                    visibleGui1();
                    if (teamplaying != null)
                    {
                        lblthele.Text = "Candidate " + teamplaying.tennguoichoi.ToString().ToUpper() + " chooses question package number " + goicauhoiid + "\n";

                    }
                    //flpNoiDung.Visible = true;
                }
                if (loadButton)
                {
                    disableButton(ttgoi);
                    selectedButton(ttgoi);
                }
            }
        }

        private void processThuThach(int doiid, int cauhoiid, bool start, int cuocthiid)
        {
            onoffflowPanelSentences(true);
            displayUCThuThach(cauhoiid, start);
        }

        private void processKhamPhaChiaSe(int doiid, int cauhoichude, int cauhoiphuid, bool trangthai, bool start, bool trangthailat, bool diemGK, bool isReadyOrOther = false)
        {
            onoffflowPanelSentences(false);

            if (cauhoichude == 0)
            {
                invisibleGuiCP();
                return;
            }

            ds_goicaudiscovery goi2 = null;
            ds_doi thisinh = null;
            ds_goicaudiscovery cauHoiChinhCP = _entities.ds_goicaudiscovery.Find(cauhoichude);

            if (cauHoiChinhCP == null) return;

            if (cauHoiChinhCP.cauhoichaid != null)
                goi2 = _entities.ds_goicaudiscovery.FirstOrDefault(x => x.cauhoichaid == cauhoichude);
            else
                goi2 = _entities.ds_goicaudiscovery.FirstOrDefault(x => x.cauhoiid == cauhoichude);

            if (goi2 != null)
            {
                thisinh = _entities.ds_doi.Find(goi2.doithiid);
                if (thisinh != null)
                {
                    lblCauHoiManhGhepCP.Text = "TOPIC: '" + cauHoiChinhCP.chude + "'";
                    lblCauHoiManhGhepCP.Visible = true;
                }
            }

            lblCauHoiManhGhepCP.Font = new Font("Arial", 16, FontStyle.Bold);
            lblCauHoiManhGhepCP.BackColor = Color.FromArgb(64, 224, 208);
            lblCauHoiManhGhepCP.ForeColor = Color.FromArgb(0, 82, 136);
            lblCauHoiManhGhepCP.BorderStyle = BorderStyle.Fixed3D;
            lblCauHoiManhGhepCP.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, lblCauHoiManhGhepCP.Width, lblCauHoiManhGhepCP.Height, 15, 15));

            if (trangthai)
            {
                visibleGuiCP();
                if (trangthailat)
                {
                    ReloadPanelAndPictures();
                    load6NutMacDinh();
                }

                string fileName = cauHoiChinhCP.noidungchude;
                string imagePath = Path.Combine(currentPath, "Resources", "pic", fileName);
                string videoPath = Path.Combine(currentPath, "Resources", "Video", fileName);
                string extension = Path.GetExtension(fileName).ToLower();

                pBCauHoiChinhCP.Visible = false;
                axWindowsMediaPlayer1.Visible = false;

                if (extension == ".mp4" || extension == ".avi" || extension == ".mov" || extension == ".wmv" || extension == ".mkv")
                {
                    if (File.Exists(videoPath))
                    {
                        pnlKhungTranh.Visible = false;

                        pBCauHoiChinhCP.Visible = false;
                        axWindowsMediaPlayer1.uiMode = "none";
                        axWindowsMediaPlayer1.URL = videoPath;
                        axWindowsMediaPlayer1.Visible = true;
                        axWindowsMediaPlayer1.settings.volume = 100;

                        if (start)
                            axWindowsMediaPlayer1.Ctlcontrols.play();
                        else
                            axWindowsMediaPlayer1.Ctlcontrols.stop();
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy file video: " + videoPath);
                    }
                }
                else
                {
                    if (File.Exists(imagePath))
                    {

                        pBCauHoiChinhCP.BackgroundImage = Image.FromFile(imagePath);
                        pBCauHoiChinhCP.BackgroundImageLayout = ImageLayout.Stretch;
                        pBCauHoiChinhCP.Visible = true;
                        axWindowsMediaPlayer1.Visible = false;
                        pnlKhungTranh.Visible = true;

                        axWindowsMediaPlayer1.Ctlcontrols.stop();
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy file ảnh: " + imagePath);
                    }
                }

                if (cauhoiphuid > 0)
                {
                    lsCauHoiPhuCP = _entities.ds_goicaudiscovery.Where(x => x.cauhoichaid == cauHoiChinhCP.cauhoiid).ToList();
                    ds_goicaudiscovery cauHoiPhu = lsCauHoiPhuCP.FirstOrDefault(x => x.cauhoiid == cauhoiphuid);
                    // >>>> Giải phóng ảnh cũ trước khi Load ảnh phụ
                    if (pBCauHoiChinhCP.BackgroundImage != null)
                    {
                        pBCauHoiChinhCP.BackgroundImage.Dispose();
                        pBCauHoiChinhCP.BackgroundImage = null;
                    }
                    LoadAnhPhuDaLat(cauhoichude, thisinh.doiid);

                    if (diemGK)
                    {
                        var diemList = _entities.ds_diem
                            .Where(x => x.doiid == thisinh.doiid && x.phanthiid == 2 && x.cauhoiid == cauhoichude)
                            .ToList();

                        for (int i = 0; i < diemList.Count && i < 3; i++)
                        {
                            var diem = diemList[i];
                            var chiTietList = _entities.ds_chitietdiem.Where(c => c.diemid == diem.diemid).ToList();
                            for (int j = 0; j < chiTietList.Count && j < 3; j++)
                            {
                                var txt = this.Controls.Find("txtGK" + (j + 1), true).FirstOrDefault() as RichTextBox;
                                if (txt != null)
                                    txt.Text = chiTietList[j].sodiem.ToString();
                            }
                        }
                    }
                }
            }
            else
            {
                invisibleGuiCP();

                string fileName = start ? cauHoiChinhCP.noidungthisinh : cauHoiChinhCP.noidungchude;
                string imagePath = Path.Combine(currentPath, "Resources", "pic", fileName);
                string videoPath = Path.Combine(currentPath, "Resources", "Video", fileName);
                string extension = Path.GetExtension(fileName).ToLower();
                pBCauHoiChinhCP.Visible = false;
                axWindowsMediaPlayer1.Visible = false;

                if (extension == ".mp4" || extension == ".avi" || extension == ".mov" || extension == ".wmv" || extension == ".mkv")
                {
                    if (File.Exists(videoPath))
                    {
                        pBCauHoiChinhCP.Visible = false;
                        pnlKhungTranh.Visible = false;
                        axWindowsMediaPlayer1.uiMode = "none";
                        axWindowsMediaPlayer1.URL = videoPath;
                        axWindowsMediaPlayer1.Visible = true;
                        axWindowsMediaPlayer1.settings.volume = 100;

                        if (isReadyOrOther)
                            axWindowsMediaPlayer1.Ctlcontrols.stop();
                        else
                            axWindowsMediaPlayer1.Ctlcontrols.play();
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy file video: " + videoPath);
                    }
                }
                else
                {
                    if (File.Exists(imagePath))
                    {
                        if (Application.OpenForms["FormZoomImage"] != null)
                        {
                            Application.OpenForms["FormZoomImage"].Close();
                        }
                        pBCauHoiChinhCP.BackgroundImage = Image.FromFile(imagePath);
                        pBCauHoiChinhCP.BackgroundImageLayout = ImageLayout.Stretch;
                        pBCauHoiChinhCP.Visible = true;
                        axWindowsMediaPlayer1.Visible = false;
                        pnlKhungTranh.Visible = true;

                        axWindowsMediaPlayer1.Ctlcontrols.stop();
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy file ảnh: " + imagePath);
                    }
                }

                this.BackgroundImageLayout = ImageLayout.Stretch;
                this.DoubleBuffered = true;
            }
        }



        private void processToaSang(int doiid, int cauhoiid, bool isStart, bool x2, bool da, bool trangthai, bool resetgoi, bool isReady = false)
        {
            onoffflowPanelSentences(false);

            ds_doi doiDangChoi = _entities.ds_doi.Find(doiid);
            if (cauhoiid == 0)
            {
                VisibleGuiVD();
                lblTheLeVD.Text = " Four Candidates will answer five questions";
            }
            else
            {

                if (cauhoiid > 0)
                {
                    ds_goicauhoishining vd = _entities.ds_goicauhoishining.Find(cauhoiid);
                    _entities.Entry(vd).Reload(); // ⚠️ Nạp lại từ DB

                    displaytoasang(cauhoiid, (int)vd.vitri, x2);
                    loadNutDangChon(cauhoiid, x2);

                    lblTheLeVD.Text = "Question " + vd.vitri + ": (" + vd.sodiem + " points)";
                    thoiGianConLai = 20;

                    if ((bool)!vd.isvideo)
                    {
                        axWinCauHoiHinhAnh.Visible = false;
                        if (vd.noidungcauhoi.Length > 200)
                        {
                            lblNoiDungCauHoiVD.Font = new Font("Arial", 20);
                        }
                        else if (vd.noidungcauhoi.Length >= 1 && vd.noidungcauhoi.Length < 30)
                        {
                            lblNoiDungCauHoiVD.Font = new Font("Arial", 22);

                        }
                        else
                        {
                            lblNoiDungCauHoiVD.Font = new Font("Arial", 22);

                        }
                        lblNoiDungCauHoiVD.Text = vd.noidungcauhoi;
                        lblNoiDungCauHoiVD.Visible = true;
                        pbImage.Visible = false;

                        if (da)
                        {
                            loadNutDaChon(cauhoiid, x2);
                            //pbDapanCH.Visible = true;
                            pbDATS.Visible = true;

                            //pbDapanCH.BackgroundImageLayout = ImageLayout.Stretch;
                            pbDATS.BackgroundImageLayout = ImageLayout.Stretch;
                            lblDA1.Visible = true;
                            if (vd.dapan.Length > 130)
                            {
                                lblDA1.Font = new Font("Arial", 16, FontStyle.Bold);
                            }
                            else if (vd.dapan.Length >= 1 && vd.dapan.Length < 10)
                            {
                                lblDA1.Font = new Font("Arial", 28, FontStyle.Bold);

                            }
                            else
                            {
                                lblDA1.Font = new Font("Arial", 20, FontStyle.Bold);

                            }
                            lblDA1.Text = vd.dapan;
                        }
                        else
                        {
                            //pbDapanCH.Visible = false;
                            lblDA1.Visible = false;
                            pbDATS.Visible = false;

                        }
                    }
                    else
                    {
                        if (trangthai)
                        {
                            if (vd.urlhinhanh != null && vd.urlhinhanh != "")
                            {
                                var url = vd.urlhinhanh.Split('.');
                                if (url.Length > 0)
                                {
                                    if (url[1] == "png" || url[1] == "jpg")
                                    {
                                        pbImage.Visible = true;
                                        axWinCauHoiHinhAnh.Visible = false;

                                        pbImage.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\pic\\" + vd.urlhinhanh);
                                        pbImage.BackgroundImageLayout = ImageLayout.Stretch;

                                        if (da)
                                        {
                                            loadNutDaChon(cauhoiid, x2);
                                            //pbDapanCH.Visible = true;
                                            pbDATS.Visible = true;

                                            //pbDapanCH.BackgroundImageLayout = ImageLayout.Stretch;
                                            pbDATS.BackgroundImageLayout = ImageLayout.Stretch;
                                            lblDA1.Visible = true;
                                            if (vd.dapan.Length > 130)
                                            {
                                                lblDA1.Font = new Font("Arial", 16, FontStyle.Bold);
                                            }
                                            else if (vd.dapan.Length >= 1 && vd.dapan.Length < 10)
                                            {
                                                lblDA1.Font = new Font("Arial", 28, FontStyle.Bold);

                                            }
                                            else
                                            {
                                                lblDA1.Font = new Font("Arial", 22, FontStyle.Bold);

                                            }
                                            lblDA1.Text = vd.dapan;
                                        }
                                        else
                                        {
                                            //pbDapanCH.Visible = false;
                                            lblDA1.Visible = false;
                                            pbDATS.Visible = false;

                                        }

                                    }
                                    else
                                    {
                                        pbImage.Visible = false;
                                        axWinCauHoiHinhAnh.settings.volume = 100;

                                        if (isReady)
                                        {
                                            axWinCauHoiHinhAnh.uiMode = "none";
                                            axWinCauHoiHinhAnh.Visible = true;
                                            axWinCauHoiHinhAnh.URL = Path.Combine(currentPath, "Resources", "Video", vd.urlhinhanh);
                                            axWinCauHoiHinhAnh.settings.volume = 100;
                                            axWinCauHoiHinhAnh.Ctlcontrols.play();

                                            // Bắt sự kiện khi MediaPlayer đã sẵn sàng để phát
                                            axWinCauHoiHinhAnh.PlayStateChange += (s, e) =>
                                            {
                                                // 3: Playing
                                                if (e.newState == 3)
                                                {
                                                    Thread.Sleep(1500);

                                                    axWinCauHoiHinhAnh.fullScreen = true;
                                                }
                                            };
                                        }

                                        if (isStart)
                                        {
                                            if (da)
                                            {
                                                loadNutDaChon(cauhoiid, x2);
                                                axWinCauHoiHinhAnh.Visible = false;
                                                axWinCauHoiHinhAnh.Ctlcontrols.stop();
                                                //pbDapanCH.Visible = true;
                                                pbDATS.Visible = true;

                                                //pbDapanCH.BackgroundImageLayout = ImageLayout.Stretch;
                                                pbDATS.BackgroundImageLayout = ImageLayout.Stretch;
                                                lblDA1.Visible = true;
                                                if (vd.dapan.Length > 130)
                                                {
                                                    lblDA1.Font = new Font("Arial", 16, FontStyle.Bold);
                                                }
                                                else if (vd.dapan.Length >= 1 && vd.dapan.Length < 10)
                                                {
                                                    lblDA1.Font = new Font("Arial", 28, FontStyle.Bold);

                                                }
                                                else
                                                {
                                                    lblDA1.Font = new Font("Arial", 20, FontStyle.Bold);

                                                }
                                                lblDA1.Text = vd.dapan;
                                            }
                                            else
                                            {
                                                axWinCauHoiHinhAnh.Visible = false;
                                                //pbDapanCH.Visible = false;
                                                pbDATS.Visible = false;
                                                lblDA1.Visible = false;
                                            }

                                        }
                                        else
                                        {
                                            axWinCauHoiHinhAnh.Visible = false;
                                            axWinCauHoiHinhAnh.URL = currentPath + "\\Resources\\Video\\" + vd.urlhinhanh;
                                            axWinCauHoiHinhAnh.Ctlcontrols.stop();
                                            if (da)
                                            {
                                                loadNutDaChon(cauhoiid, x2);
                                                axWinCauHoiHinhAnh.Visible = false;
                                                axWinCauHoiHinhAnh.Ctlcontrols.stop();
                                                //pbDapanCH.Visible = true;
                                                pbDATS.Visible = true;

                                                //pbDapanCH.BackgroundImageLayout = ImageLayout.Stretch;
                                                pbDATS.BackgroundImageLayout = ImageLayout.Stretch;
                                                lblDA1.Visible = true;
                                                if (vd.dapan.Length > 130)
                                                {
                                                    lblDA1.Font = new Font("Arial", 16, FontStyle.Bold);
                                                }
                                                else if (vd.dapan.Length >= 1 && vd.dapan.Length < 10)
                                                {
                                                    lblDA1.Font = new Font("Arial", 28, FontStyle.Bold);

                                                }
                                                else
                                                {
                                                    lblDA1.Font = new Font("Arial", 20, FontStyle.Bold);

                                                }
                                                lblDA1.Text = vd.dapan;
                                            }
                                            else
                                            {
                                                axWinCauHoiHinhAnh.Visible = false;
                                                pbImage.Visible = false;
                                                //pbDapanCH.Visible = false;
                                                pbDATS.Visible = false;
                                                lblDA1.Visible = false;
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        lblNoiDungCauHoiVD.Visible = true;
                        if (vd.noidungcauhoi.Length > 200)
                        {
                            lblNoiDungCauHoiVD.Font = new Font("Arial", 20);
                        }
                        else if (vd.noidungcauhoi.Length >= 1 && vd.noidungcauhoi.Length < 30)
                        {
                            lblNoiDungCauHoiVD.Font = new Font("Arial", 22);

                        }
                        else
                        {
                            lblNoiDungCauHoiVD.Font = new Font("Arial", 22);

                        }
                        lblNoiDungCauHoiVD.Text = vd.noidungcauhoi;
                        /*pbDapanCH.Visible = false;
                        lblDA1.Visible = false;*/
                        if (vd.urlhinhanh != null && vd.urlhinhanh != "")
                        {
                            var url = vd.urlhinhanh.Split('.');
                            if (url.Length > 0)
                            {
                                if (url[1] == "png" || url[1] == "jpg")
                                {
                                    pbImage.Visible = true;
                                    axWinCauHoiHinhAnh.Visible = false;

                                    pbImage.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\pic\\" + vd.urlhinhanh);
                                    axWinCauHoiHinhAnh.settings.volume = 100;
                                    pbImage.BackgroundImageLayout = ImageLayout.Stretch;

                                    if (da)
                                    {
                                        loadNutDaChon(cauhoiid, x2);
                                        //pbDapanCH.Visible = true;
                                        pbDATS.Visible = true;

                                        //pbDapanCH.BackgroundImageLayout = ImageLayout.Stretch;
                                        pbDATS.BackgroundImageLayout = ImageLayout.Stretch;
                                        lblDA1.Visible = true;
                                        if (vd.dapan.Length > 130)
                                        {
                                            lblDA1.Font = new Font("Arial", 16, FontStyle.Bold);
                                        }
                                        else if (vd.dapan.Length >= 1 && vd.dapan.Length < 10)
                                        {
                                            lblDA1.Font = new Font("Arial", 28, FontStyle.Bold);

                                        }
                                        else
                                        {
                                            lblDA1.Font = new Font("Arial", 20, FontStyle.Bold);

                                        }
                                        lblDA1.Text = vd.dapan;
                                    }
                                    else
                                    {
                                        //pbDapanCH.Visible = false;
                                        lblDA1.Visible = false;
                                        pbDATS.Visible = false;

                                    }

                                }
                                else
                                {
                                    if (isReady)
                                    {
                                        axWinCauHoiHinhAnh.uiMode = "none";
                                        axWinCauHoiHinhAnh.Visible = true;
                                        axWinCauHoiHinhAnh.URL = Path.Combine(currentPath, "Resources", "Video", vd.urlhinhanh);
                                        axWinCauHoiHinhAnh.settings.volume = 100;
                                        axWinCauHoiHinhAnh.Ctlcontrols.play();

                                        // Bắt sự kiện khi MediaPlayer đã sẵn sàng để phát
                                        axWinCauHoiHinhAnh.PlayStateChange += (s, e) =>
                                        {
                                            // 3: Playing
                                            if (e.newState == 3)
                                            {
                                                Thread.Sleep(1500);

                                                axWinCauHoiHinhAnh.fullScreen = true;
                                            }
                                        };
                                    }

                                    if (isStart)
                                    {
                                        pbImage.Visible = false;
                                        if (da)
                                        {
                                            loadNutDaChon(cauhoiid, x2);
                                            axWinCauHoiHinhAnh.Visible = false;
                                            axWinCauHoiHinhAnh.Ctlcontrols.stop();
                                            //pbDapanCH.Visible = true;
                                            pbDATS.Visible = true;

                                            //pbDapanCH.BackgroundImageLayout = ImageLayout.Stretch;
                                            pbDATS.BackgroundImageLayout = ImageLayout.Stretch; lblDA1.Visible = true;
                                            if (vd.dapan.Length > 130)
                                            {
                                                lblDA1.Font = new Font("Arial", 16, FontStyle.Bold);
                                            }
                                            else if (vd.dapan.Length >= 1 && vd.dapan.Length < 10)
                                            {
                                                lblDA1.Font = new Font("Arial", 28, FontStyle.Bold);

                                            }
                                            else
                                            {
                                                lblDA1.Font = new Font("Arial", 20, FontStyle.Bold);

                                            }
                                            lblDA1.Text = vd.dapan;
                                        }
                                        else
                                        {
                                            axWinCauHoiHinhAnh.Visible = true;
                                            //pbDapanCH.Visible = false;
                                            pbDATS.Visible = false;
                                            lblDA1.Visible = false;
                                        }

                                    }
                                    else
                                    {
                                        if (da)
                                        {
                                            loadNutDaChon(cauhoiid, x2);
                                            //pbDapanCH.Visible = true;
                                            pbDATS.Visible = true;

                                            //pbDapanCH.BackgroundImageLayout = ImageLayout.Stretch;
                                            pbDATS.BackgroundImageLayout = ImageLayout.Stretch;
                                            lblDA1.Visible = true;
                                            if (vd.dapan.Length > 130)
                                            {
                                                lblDA1.Font = new Font("Arial", 16, FontStyle.Bold);
                                            }
                                            else if (vd.dapan.Length >= 1 && vd.dapan.Length < 10)
                                            {
                                                lblDA1.Font = new Font("Arial", 28, FontStyle.Bold);

                                            }
                                            else
                                            {
                                                lblDA1.Font = new Font("Arial", 20, FontStyle.Bold);

                                            }
                                            lblDA1.Text = vd.dapan;
                                        }
                                        else
                                        {
                                            axWinCauHoiHinhAnh.Visible = true;
                                            //pbDapanCH.Visible = false;
                                            pbDATS.Visible = false;
                                            lblDA1.Visible = false;
                                        }

                                    }
                                }
                            }
                        }
                        
                    }

                    EnabledGuiVD();

                }
                else
                {
                    EnabledGui1VD();
                    //pbDapanCH.Visible = false;
                    lblDA1.Visible = false;
                }


            }

        }

        #region ucKhamPhaChiaSe

        

        private void LoadAnhPhuDaLat(int cauchude, int doiid)
        {
            var dsAnhDaLat = _entities.ds_goicaudiscovery
                .Where(x => x.cauhoichaid == cauchude && x.doithiid == doiid && x.trangthailatAnhPhu == 1)
                .ToList();

            // Hide all the small picture boxes
            pbCau1.Visible = false;
            pbCau2.Visible = false;
            pbCau3.Visible = false;
            pbCau4.Visible = false;
            pbCau5.Visible = false;
            pbCau6.Visible = false;
            if (pBCauHoiChinhCP.BackgroundImage != null)
            {
                pBCauHoiChinhCP.BackgroundImage.Dispose();
                pBCauHoiChinhCP.BackgroundImage = null;
            }
            // Create a combined image for all secondary images
            if (dsAnhDaLat.Count > 0)
            {

                for (int i = 0; i < dsAnhDaLat.Count; i++)
                {
                    _entities.Entry(dsAnhDaLat[i]).Reload(); // ⚠️ Nạp lại từ DB

                    string imagePath = Path.Combine(currentPath, "Resources", "pic", dsAnhDaLat[i].noidungchude);
                    if (File.Exists(imagePath))
                    {
                        Image img = Image.FromFile(imagePath);

                        pBCauHoiChinhCP.BackgroundImage = img;
                        pBCauHoiChinhCP.BackgroundImageLayout = ImageLayout.Stretch;
                    }
                }

                
            }
            else
            {
                // If no secondary images, show the main topic image
                var cauHoiChinh = _entities.ds_goicaudiscovery.Find(cauchude);
                if (cauHoiChinh != null && !string.IsNullOrEmpty(cauHoiChinh.noidungchude))
                {
                    string mainImagePath = Path.Combine(currentPath, "Resources", "pic", cauHoiChinh.noidungchude);
                    if (File.Exists(mainImagePath))
                    {
                        pBCauHoiChinhCP.BackgroundImage = Image.FromFile(mainImagePath);
                        pBCauHoiChinhCP.BackgroundImageLayout = ImageLayout.Stretch;
                    }
                }
            }
        }
        private void invisibleGuiCP()
        {
            lblCauhoiphu.Visible = false;
            lblCauHoiManhGhepCP.Visible = true;
            pbCau1.Visible = false;
            pbCau2.Visible = false;
            pbCau3.Visible = false;
            pbCau4.Visible = false;
            pbCau5.Visible = false;
            pbCau6.Visible = false;
            pnlKhungTranh.Visible = true;
            pBCauHoiChinhCP.Visible = true;
            //labelGK1.Visible = true;
            //labelGK2.Visible = true;
            //labelGK3.Visible = true;
            //txtGK1.Visible = true;
            //txtGK2.Visible = true;
            //txtGK3.Visible = true;
            //txtGK1.Enabled = false;
            //txtGK2.Enabled = false;
            //txtGK3.Enabled = false;

        }
        private Image LoadImageSafe(string path)
        {
            using (var img = Image.FromFile(path))
            {
                return new Bitmap(img); // tạo bản sao để tránh lock
            }
        }
        private void ReloadPanelAndPictures()
        {
            string basePath = currentPath + "\\Resources\\group4\\";

            

            // Mảng PictureBox cần reload
            PictureBox[] listPB = { pbCau1, pbCau2, pbCau3, pbCau4, pbCau5, pbCau6 };

            for (int i = 0; i < listPB.Length; i++)
            {
                var pb = listPB[i];
                if (pb != null)
                {
                    pb.BackgroundImage = null;
                    pb.Image = null;
                    Application.DoEvents(); // đảm bảo clear ảnh cũ
                    string imagePath = basePath + $"{i + 1}-dis.png";
                    pb.BackgroundImage = LoadImageSafe(imagePath);
                    pb.Refresh();
                }
            }

            // Cuối cùng refresh cả panel
            pBCauHoiChinhCP.Invalidate();
            pBCauHoiChinhCP.Update();
        }

        private void load6NutMacDinh()
        {
            string nutPath = Path.Combine(currentPath, "Resources", "group4");

            pbCau1.BackgroundImage = Image.FromFile(Path.Combine(nutPath, "1-dis.png"));
            pbCau2.BackgroundImage = Image.FromFile(Path.Combine(nutPath, "2-dis.png"));
            pbCau3.BackgroundImage = Image.FromFile(Path.Combine(nutPath, "3-dis.png"));
            pbCau4.BackgroundImage = Image.FromFile(Path.Combine(nutPath, "4-dis.png"));
            pbCau5.BackgroundImage = Image.FromFile(Path.Combine(nutPath, "5-dis.png"));
            pbCau6.BackgroundImage = Image.FromFile(Path.Combine(nutPath, "6-dis.png"));

            pbCau1.BackgroundImageLayout = pbCau2.BackgroundImageLayout =
            pbCau3.BackgroundImageLayout = pbCau4.BackgroundImageLayout =
            pbCau5.BackgroundImageLayout = pbCau6.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void visibleGuiCP()
        {
            lblCauHoiManhGhepCP.Visible = true;
            pnlKhungTranh.Visible = true;
            pBCauHoiChinhCP.Visible = true;
            pbCau1.Visible = true;
            pbCau2.Visible = true;
            pbCau3.Visible = true;
            pbCau4.Visible = true;
            pbCau5.Visible = true;
            pbCau6.Visible = true;
            pBCauHoiChinhCP.BackgroundImage = null;
            //labelGK1.Visible = true;
            //labelGK2.Visible = true;
            //labelGK3.Visible = true;
            //txtGK1.Visible = true;
            //txtGK2.Visible = true;
            //txtGK3.Visible = true;
            //txtGK1.Enabled = false;
            //txtGK2.Enabled = false;
            //txtGK3.Enabled = false;
        }

        #endregion

        #region ucKhoiDong
        private void invisibleGui()
        {
            lblthele.Visible = true;
            lblNoiDungCauHoi.Visible = false;
            labelNoiDungCauHoi.Visible = false;
        }
        private void visibleGui1()
        {
            lblthele.Visible = true;
            lblNoiDungCauHoi.Visible = false;
            labelNoiDungCauHoi.Visible = false;
        }
        private void visibleGui()
        {
            lblthele.Visible = true;
            lblNoiDungCauHoi.Visible = true;
            labelNoiDungCauHoi.Visible = true;
        }

        private void disableButton(int[] _ttgoi)
        {
            if (_ttgoi[0] == 1)
            {
                pbGoi1.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group4\\1-dis.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;

            }
            if (_ttgoi[1] == 1)
            {
                pbGoi2.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group4\\2-dis.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;

            }
            if (_ttgoi[2] == 1)
            {
                pbGoi3.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group4\\3-dis.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;

            }
            if (_ttgoi[3] == 1)
            {
                pbGoi4.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group4\\4-dis.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;

            }
            if (_ttgoi[4] == 1)
            {
                pbGoi5.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group4\\5-dis.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;

            }
            if (_ttgoi[5] == 1)
            {
                pbGoi6.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group4\\6-dis.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;

            }
        }
        private void selectedButton(int[] _ttgoi)
        {
            if (_ttgoi[0] == 2)
            {
                pbGoi1.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group4\\Group_221.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;

            }
            if (_ttgoi[1] == 2)
            {
                pbGoi2.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group4\\Group_222.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;

            }
            if (_ttgoi[2] == 2)
            {
                pbGoi3.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group4\\Group_223.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;

            }
            if (_ttgoi[3] == 2)
            {
                pbGoi4.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group4\\Group_224.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;

            }
            if (_ttgoi[4] == 2)
            {
                pbGoi5.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group4\\Group_225.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;

            }
            if (_ttgoi[5] == 2)
            {
                pbGoi6.BackgroundImage = Image.FromFile(currentPath + "\\Resources\\group4\\Group_226.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;

            }
        }
        #endregion

        #region ucThuThach

        private void displayUCThuThach(int cauhoiid, bool _isStart)
        {
            if (cauhoiid == 0)
            {
                VisibleGuiKP();
                return;
            }

            EnabledGuiKP();
            ds_cauhoithuthach khamPha = _entities.ds_cauhoithuthach.Find(cauhoiid);
            if (khamPha == null) return;
            _entities.Entry(khamPha).Reload(); // ⚠️ Nạp lại từ DB

            //Hiển thị loại câu hỏi theo vị trí
            if (khamPha.vitri == 1 || khamPha.vitri == 2)
            {
                lblCauHoiTT.Text = "Question " + khamPha.vitri + ": Rearrange the following words or phrases to make a complete sentence";

            }
            else if (khamPha.vitri == 3 || khamPha.vitri == 4)
            {
                lblCauHoiTT.Text = "Question " + khamPha.vitri + ": Rearrange the following sentences to make a meaningful conversation";

            }
            else
            {
                lblCauHoiTT.Text = "Question " + khamPha.vitri + ": Rearrange the following sentences to make a meaningful paragraph";

            }

            // Tối ưu hiển thị, tránh nháy bằng cách bật DoubleBuffered
            EnableDoubleBuffering(flowPanelSentences);

            flowPanelSentences.SuspendLayout();
            flowPanelSentences.Controls.Clear();

            flowPanelSentences.FlowDirection = FlowDirection.TopDown;
            flowPanelSentences.WrapContents = false;
            flowPanelSentences.AutoScroll = true;
            flowPanelSentences.Padding = new Padding(10);

            string[] sentences = khamPha.noidung.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string[] answerLabels = { "A", "B", "C", "D", "E" };

            Color primaryColor = Color.FromArgb(52, 152, 219);
            Color hoverColor = Color.FromArgb(41, 128, 185);
            Font btnFont = new Font("Segoe UI", 11, FontStyle.Bold);
            Color textColor = Color.White;
            int buttonWidth = flowPanelSentences.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 10;

            for (int i = 0; i < sentences.Length && i < answerLabels.Length; i++)
            {
                string text = answerLabels[i] + ". " + sentences[i].Trim();

                Button btn = new Button
                {
                    Text = text,
                    Font = btnFont,
                    ForeColor = textColor,
                    BackColor = primaryColor,
                    FlatStyle = FlatStyle.Flat,
                    Width = buttonWidth,
                    Height = 60, // Base height
                    Margin = new Padding(0, 0, 0, 10),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(15, 10, 10, 10),
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false,
                    AutoSize = false, // QUAN TRỌNG: Không dùng AutoSize để tự điều chỉnh chiều ngang
                };

                // Tính toán chiều cao theo nội dung (nếu có dòng dài)
                Size textSize = TextRenderer.MeasureText(btn.Text, btn.Font, new Size(buttonWidth - btn.Padding.Horizontal, int.MaxValue), TextFormatFlags.WordBreak);
                btn.Height = Math.Max(60, textSize.Height + btn.Padding.Vertical + 10);

                // Tối ưu FlatAppearance
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = hoverColor;
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(32, 102, 155);

                // Bo góc
                btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 15, 15));

                // Hover effect
                btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
                btn.MouseLeave += (s, e) => btn.BackColor = primaryColor;

                flowPanelSentences.Controls.Add(btn);
            }

            flowPanelSentences.ResumeLayout();
        }
        private void EnableDoubleBuffering(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        // Hàm tạo region bo góc (thêm vào class)
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        private void onoffflowPanelSentences(bool v)
        {
            flowPanelSentences.Visible = v;
        }
        private void VisibleGuiKP()
        {
            labelNoiDungCauHoi.Visible = true;
            flowPanelSentences.Visible = true;
            lblCauhoi.Visible = false;
            lblCauHoiTT.Visible = false;
        }
        private void EnabledGuiKP()
        {
            flowPanelSentences.Visible = true;
            labelNoiDungCauHoi.Visible = false;
            lblCauhoi.Visible = false;
            lblCauHoiTT.Visible = true;
        }
        #endregion

        #region ucToaSang
        public void VisibleGuiVD()
        {
            lblTheLeVD.Visible = true;
            lblNoiDungCauHoiVD.Visible = false;
            pbImage.Visible = false;
            axWinCauHoiHinhAnh.Visible = false;
            //pbDapanCH.Visible = false;
            pbDATS.Visible = false;
            lblDA1.Visible = false;
            pnlGoiCauHoiVD.Visible = true;
            pbGoi1VD.Visible = true;
            pbGoi4VD.Visible = true;
            pbGoi2VD.Visible = true;
            pbGoi5VD.Visible = true;
            pbGoi3VD.Visible = true;
        }
        public void EnabledGuiVD()
        {
            lblTheLeVD.Visible = true;
            lblNoiDungCauHoiVD.Visible = true;
            pbGoi1VD.Visible = true;
            pbGoi4VD.Visible = true;
            pbGoi2VD.Visible = true;
            pbGoi5VD.Visible = true;
            pbGoi3VD.Visible = true;
            pnlGoiCauHoiVD.Visible = true;

        }
        public void EnabledGui1VD()
        {
            lblTheLeVD.Visible = true;
            lblNoiDungCauHoiVD.Visible = false;
            pnlGoiCauHoiVD.Visible = true;
            pbGoi1VD.Visible = true;
            pbGoi4VD.Visible = true;
            pbGoi2VD.Visible = true;
            pbGoi5VD.Visible = true;
            pbGoi3VD.Visible = true;

        }
        // Danh sách các ID câu hỏi đã hiển thị trước đó
        private HashSet<int> dsCauHoiDaHienThi = new HashSet<int>();
        void displaytoasang(int cauhoiid, int vitri, bool isX2)
        {
            var cauhoiTS = _entities.ds_goicauhoishining.Find(cauhoiid);
            if (cauhoiTS != null)
            {

                if (cauhoiTS.vitri == vitri)
                {
                    if (cauhoiTS.trangThai == 1)
                    {
                        if (isX2)
                        {

                            switch (cauhoiTS.vitri)
                            {
                                case 1:
                                    pbGoi1VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                    pbGoi1VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\1-star.png");
                                    break;
                                case 2:
                                    pbGoi2VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                    pbGoi2VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\2-star.png");
                                    break;
                                case 3:
                                    pbGoi3VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                    pbGoi3VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\3-star.png"); ;
                                    break;
                                case 4:
                                    pbGoi4VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                    pbGoi4VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\4-star.png"); ;
                                    break;
                                case 5:
                                    pbGoi5VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                    pbGoi5VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\5-star.png"); ;
                                    break;

                            }
                        }
                        else
                        {
                            switch (cauhoiTS.vitri)
                            {
                                case 1:
                                    pbGoi1VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                    pbGoi1VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\ts1_in.png");
                                    break;
                                case 2:
                                    pbGoi2VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                    pbGoi2VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\ts2_in.png");
                                    break;
                                case 3:
                                    pbGoi3VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                    pbGoi3VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\ts3_in.png"); ;
                                    break;
                                case 4:
                                    pbGoi4VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                    pbGoi4VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\ts4_in.png"); ;
                                    break;
                                case 5:
                                    pbGoi5VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                    pbGoi5VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\ts5_in.png"); ;
                                    break;

                            }
                        }
                    }
                    else if (cauhoiTS.trangThai == 0)
                    {
                        switch (cauhoiTS.vitri)
                        {
                            case 1:
                                pbGoi1VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                pbGoi1VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\ts1.png");
                                break;
                            case 2:
                                pbGoi2VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                pbGoi2VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\ts2.png");
                                break;
                            case 3:
                                pbGoi3VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                pbGoi3VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\ts3.png"); ;
                                break;
                            case 4:
                                pbGoi4VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                pbGoi4VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\ts4.png"); ;
                                break;
                            case 5:
                                pbGoi5VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                pbGoi5VD.Image = System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\ts5.png"); ;
                                break;

                        }
                    }
                    else
                    {
                        switch (cauhoiTS.vitri)
                        {
                            case 1:
                                pbGoi1VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                pbGoi1VD.Image = isX2 ? System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\1-star.png") : System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\1-dis.png");
                                break;
                            case 2:
                                pbGoi2VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                pbGoi2VD.Image = isX2 ? System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\2-star.png") : System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\2-dis.png");
                                break;
                            case 3:
                                pbGoi3VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                pbGoi3VD.Image = isX2 ? System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\3-star.png") : System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\3-dis.png");
                                break;
                            case 4:
                                pbGoi4VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                pbGoi4VD.Image = isX2 ? System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\4-star.png") : System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\4-dis.png"); ;
                                break;
                            case 5:
                                pbGoi5VD.SizeMode = PictureBoxSizeMode.StretchImage;
                                pbGoi5VD.Image = isX2 ? System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\5-star.png") : System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\5-dis.png"); ;
                                break;

                        }
                    }
                    // Thêm câu hỏi này vào danh sách đã hiển thị
                    dsCauHoiDaHienThi.Add(cauhoiid);

                }
            }


        }
        private void loadNutDaChon(int cauhoiid, bool isX2)
        {
            var dsCauDaChon = _entities.ds_goicauhoishining
                .Where(x => x.cauhoiid == cauhoiid && x.trangThai == 2)
                .ToList();
            foreach (var cauHoi in dsCauDaChon)
            {
                switch (cauHoi.vitri)
                {
                    case 1:
                        pbGoi1VD.SizeMode = PictureBoxSizeMode.StretchImage;
                        pbGoi1VD.Image = isX2 ? System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\1-star.png"): System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\1-dis.png");
                        break;
                    case 2:
                        pbGoi2VD.SizeMode = PictureBoxSizeMode.StretchImage;
                        pbGoi2VD.Image = isX2 ? System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\2-star.png") : System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\2-dis.png");
                        break;
                    case 3:
                        pbGoi3VD.SizeMode = PictureBoxSizeMode.StretchImage;
                        pbGoi3VD.Image = isX2 ? System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\3-star.png") : System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\3-dis.png");
                        break;
                    case 4:
                        pbGoi4VD.SizeMode = PictureBoxSizeMode.StretchImage;
                        pbGoi4VD.Image = isX2 ? System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\4-star.png") : System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\4-dis.png"); ;
                        break;
                    case 5:
                        pbGoi5VD.SizeMode = PictureBoxSizeMode.StretchImage;
                        pbGoi5VD.Image = isX2 ? System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\5-star.png") : System.Drawing.Image.FromFile(currentPath + "\\Resources\\group4\\5-dis.png"); ;
                        break;

                }
            }
        }
        private void loadNutDangChon(int cauhoiid, bool isX2)
        {
            try
            {
                var dsCauChon = _entities.ds_goicauhoishining
                    .Where(x => x.cauhoiid == cauhoiid && x.trangThai == 1)
                    .ToList();

                // Đảm bảo UI được cập nhật đồng bộ
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate { loadNutDangChon(cauhoiid, isX2); });
                    return;
                }

                foreach (var cauHoi in dsCauChon)
                {
                    string imagePath = "";
                    PictureBox targetPb = null;

                    // Xác định PictureBox và đường dẫn ảnh tương ứng
                    switch (cauHoi.vitri)
                    {
                        case 1:
                            targetPb = pbGoi1VD;
                            imagePath = isX2 ? "1-star.png" : "ts1_in.png";
                            break;
                        case 2:
                            targetPb = pbGoi2VD;
                            imagePath = isX2 ? "2-star.png" : "ts2_in.png";
                            break;
                        case 3:
                            targetPb = pbGoi3VD;
                            imagePath = isX2 ? "3-star.png" : "ts3_in.png";
                            break;
                        case 4:
                            targetPb = pbGoi4VD;
                            imagePath = isX2 ? "4-star.png" : "ts4_in.png";
                            break;
                        case 5:
                            targetPb = pbGoi5VD;
                            imagePath = isX2 ? "5-star.png" : "ts5_in.png";
                            break;
                    }

                    // Kiểm tra và cập nhật PictureBox
                    if (targetPb != null && !string.IsNullOrEmpty(imagePath))
                    {
                        string fullPath = Path.Combine(currentPath, "Resources", "group4", imagePath);

                        if (File.Exists(fullPath))
                        {
                            // Đảm bảo PictureBox sẵn sàng nhận ảnh mới
                            if (targetPb.Image != null)
                            {
                                targetPb.Image.Dispose();
                                targetPb.Image = null;
                            }

                            targetPb.SizeMode = PictureBoxSizeMode.StretchImage;
                            targetPb.Image = Image.FromFile(fullPath);
                            targetPb.Refresh(); // Cập nhật ngay lập tức
                        }
                        else
                        {
                            Console.WriteLine($"File not found: {fullPath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in loadNutDangChon: {ex.Message}");
                // Có thể thêm hiển thị thông báo lỗi nếu cần
            }
        }
        #endregion

        private void onOffUc(int uc, bool self)
        {
            if (uc == 1)
            {
                if (self)
                {
                    pnlKhoiDong.Visible = false;
                }
                else
                {
                    pnlKhoiDong.Visible = true;
                }
                pnlKhamPha.Visible = false;
                pnlChinhPhuc.Visible = false;
                pnlVedich.Visible = false;
            }
            if (uc == 2)
            {
                if (self)
                {
                    pnlKhamPha.Visible = false;
                }
                else
                {
                    pnlKhamPha.Visible = true;
                }
                pnlKhoiDong.Visible = false;
                pnlChinhPhuc.Visible = false;
                pnlVedich.Visible = false;
            }
            if (uc == 3)
            {
                if (self)
                {
                    pnlChinhPhuc.Visible = false;
                }
                else
                {
                    pnlChinhPhuc.Visible = true;
                }
                pnlKhoiDong.Visible = false;
                pnlKhamPha.Visible = false;
                pnlVedich.Visible = false;
            }
            if (uc == 4)
            {
                if (self)
                {
                    pnlVedich.Visible = false;
                }
                else
                {
                    pnlVedich.Visible = true;
                }
                pnlKhoiDong.Visible = false;
                pnlKhamPha.Visible = false;
                pnlChinhPhuc.Visible = false;
            }
            if (uc == 5)
            {
                pnlVedich.Visible = false;
                pnlKhoiDong.Visible = false;
                pnlKhamPha.Visible = false;
                pnlChinhPhuc.Visible = false;
            }
        }

        private DataTable ExecuteQuery(string sql)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["QMDS_Connection"].ConnectionString))
            {
                using (var command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        return table;
                    }
                }
            }
        }
    }
}

