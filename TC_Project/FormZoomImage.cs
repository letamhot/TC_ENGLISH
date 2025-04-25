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
    public partial class FormZoomImage: Form
    {
        private static FormZoomImage instance;

        private FormZoomImage()
        {
            InitializeComponent();
        }

        public static void ShowImage(Image img)
        {
            if (instance == null || instance.IsDisposed)
            {
                instance = new FormZoomImage();
            }

            instance.pictureBoxZoom.Image = img;
            instance.pictureBoxZoom.SizeMode = PictureBoxSizeMode.Zoom;
            instance.StartPosition = FormStartPosition.CenterScreen;
            instance.Show();
            instance.BringToFront();
        }

        private void pbClose_Click(object sender, EventArgs e)
        {
            this.Close();

        }
    }
}
