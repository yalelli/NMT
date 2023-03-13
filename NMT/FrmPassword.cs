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
    public partial class FrmPassword : Form
    {
        public FrmPassword()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string strLang = System.Globalization.CultureInfo.CurrentUICulture.Name;

            if (txbPassword.Text != "" && txbPassword.Text != null)
            {
                if (txbPassword.Text == "Kimi1234")
                {
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    if (strLang != "zh-CN")
                    {
                        MessageBox.Show("Password error!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("密码错误!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            else
            {
                if (strLang != "zh-CN")
                {
                    MessageBox.Show("Password can not be empty, please enter complete!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("密码不能为空，请输入完整！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txbPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                btnOK_Click(null, null);
            }
        }
    }
}
