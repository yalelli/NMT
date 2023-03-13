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
    public partial class FrmCalculate : Form
    {
        FrmMain frmMain = new FrmMain();

        public FrmCalculate(FrmMain newFrmMain)
        {
            this.frmMain = newFrmMain;
            InitializeComponent();

            lblCalculatedStiffness.Text = (FrmEnquiry.dCalculatedStiffness * Math.Pow(10, 3)).ToString("0");
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            this.frmMain.strStiffnessTxt = (Convert.ToDouble(lblCalculatedStiffness.Text) - FrmMain.dExtraStiffness).ToString();
            this.frmMain.UpdateSensorDefaultValue();
            this.Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
