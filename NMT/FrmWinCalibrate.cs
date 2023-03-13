using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NMT
{
    public partial class FrmWinCalibrate : Form
    {
        FrmMain WinMain = new FrmMain();

        public FrmWinCalibrate(FrmMain Main)
        {
            WinMain = Main;
            InitializeComponent();
        }

        private void bntStart_Click(object sender, EventArgs e)
        {
            this.Close();
            FrmSensortivity bntCalibrate = new FrmSensortivity(WinMain);
            bntCalibrate.ShowDialog();
        }

        private void bntCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
