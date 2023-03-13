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
    public partial class FrmCaliWin : Form
    {
        FrmMain frmMain = new FrmMain();

        public FrmCaliWin(FrmMain frmmain)
        {
            frmMain = frmmain;
            InitializeComponent();
        }

        private void btIndentationCalibration_Click(object sender, EventArgs e)
        {
            this.Close();
            FrmIndentationCalibration showdialog = new FrmIndentationCalibration(frmMain);
            showdialog.ShowDialog();
        }

        private void btnStiffnessCalibration_Click(object sender, EventArgs e)
        {
            this.Close();
            FrmStiffnessCalibration frmStiffnessCalibration = new FrmStiffnessCalibration(frmMain);
            frmStiffnessCalibration.ShowDialog();
        }

        private void bntCalibration_Click(object sender, EventArgs e)
        {
            this.Close();
            FrmSensortivity bntCalibrate = new FrmSensortivity(frmMain);
            bntCalibrate.ShowDialog();
        }
    }
}
