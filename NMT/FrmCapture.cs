using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QCAP.NET;

namespace NMT
{
    public partial class FrmCapture : Form
    {
        public float xvalues;
        public float yvalues;
        public float orixvalues;
        public float oriyvalues;
        public uint m_hCapDev = 0x00000000; // STREAM CAPTURE DEVICE

        /// <summary>
        /// disable close button
        /// </summary>
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        public FrmCapture()
        {
            InitializeComponent();
        }

        private void FrmCapture_Load(object sender, EventArgs e)
        {
            int x = Screen.PrimaryScreen.WorkingArea.Size.Width - this.Width;
            int y = (Screen.PrimaryScreen.WorkingArea.Size.Height - this.Height) / 2;
            this.SetDesktopLocation(x, y);

            this.Resize += new EventHandler(MainForm_Resize); //添加窗体拉伸重绘事件
            orixvalues = xvalues = this.Width;//记录窗体初始大小
            oriyvalues = yvalues = this.Height;
            SetTag(this);

            HwInitialize();
        }

        private void FrmCapture_FormClosing(object sender, FormClosingEventArgs e)
        {
            HwUnInitialize();
        }

        #region 窗体拉伸重绘

        private void MainForm_Resize(object sender, EventArgs e)//重绘事件
        {
            float newX = this.Width / xvalues;//获得比例
            float newY = this.Height / yvalues;
            SetControls(newX, newY, this);
        }

        private void SetControls(float newX, float newY, Control cons)//改变控件的大小
        {
            foreach (Control con in cons.Controls)
            {
                string[] mytag = con.Tag.ToString().Split(new char[] { ':' });
                float a = Convert.ToSingle(mytag[0]) * newX;
                con.Width = (int)a;
                a = Convert.ToSingle(mytag[1]) * newY;
                con.Height = (int)a;
                a = Convert.ToSingle(mytag[2]) * newX;
                con.Left = (int)a;
                a = Convert.ToSingle(mytag[3]) * newY;
                con.Top = (int)a;
                Single currentSize = Convert.ToSingle(mytag[4]) * newY;

                con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                if (con.Controls.Count > 0)
                {
                    SetControls(newX, newY, con);
                }
            }
        }

        /// <summary>
        /// 遍历窗体中控件函数
        /// </summary>
        /// <param name="cons"></param>
        private void SetTag(Control cons)
        {
            foreach (Control con in cons.Controls)  //遍历窗体中的控件,记录控件初始大小
            {
                con.Tag = con.Width + ":" + con.Height + ":" + con.Left + ":" + con.Top + ":" + con.Font.Size;
                if (con.Controls.Count > 0)
                {
                    SetTag(con);
                }
            }
        }

        #endregion

        #region 视频采集和保存

        public bool HwInitialize()
        {
            // CREATE CAPTURE DEVICE
            string str_chip_name = "CY3014 USB";
            EXPORTS.QCAP_CREATE(ref str_chip_name, 0, (uint)pictureBox.Handle.ToInt32(), ref m_hCapDev);

            if (m_hCapDev == 0)
            {
                return false;
            }

            EXPORTS.QCAP_SET_VIDEO_DEINTERLACE(m_hCapDev, 0);
            EXPORTS.QCAP_RUN(m_hCapDev);
            return true;
        }

        public bool HwUnInitialize()
        {
            if (m_hCapDev != 0)
            {
                EXPORTS.QCAP_STOP(m_hCapDev);

                EXPORTS.QCAP_DESTROY(m_hCapDev);
            }

            return true;
        }

        public void StartRecording(bool bRecord, string strFilePath)
        {
            if (bRecord)
            {
                if (m_hCapDev != 0)
                {
                    //EXPORTS.QCAP_SET_AUDIO_RECORD_PROPERTY(m_hCapDev, 0, (uint)EXPORTS.EncoderTypeEnum.QCAP_ENCODER_TYPE_SOFTWARE, (uint)EXPORTS.AudioEncoderFormatEnum.QCAP_ENCODER_FORMAT_PCM);//AVI
                    EXPORTS.QCAP_SET_AUDIO_RECORD_PROPERTY(m_hCapDev, 0, (uint)EXPORTS.EncoderTypeEnum.QCAP_ENCODER_TYPE_SOFTWARE, (uint)EXPORTS.AudioEncoderFormatEnum.QCAP_ENCODER_FORMAT_AAC);//MP4

                    EXPORTS.QCAP_SET_VIDEO_RECORD_PROPERTY(m_hCapDev, 0, (uint)EXPORTS.EncoderTypeEnum.QCAP_ENCODER_TYPE_SOFTWARE, (uint)EXPORTS.VideoEncoderFormatEnum.QCAP_ENCODER_FORMAT_H264, (uint)EXPORTS.RecordModeEnum.QCAP_RECORD_MODE_CBR, 8000, 12582912, 30, 0, 0, (uint)EXPORTS.DownScaleModeEnum.QCAP_DOWNSCALE_MODE_OFF);
                    //string str_avi_name1 = m_strCurrentDir + "\\CH01_1.AVI";
                    string str_avi_name1 = strFilePath;
                    string pszNULL = null;
                    EXPORTS.QCAP_START_RECORD(m_hCapDev, 0, ref str_avi_name1, (uint)EXPORTS.RecordFlagEnum.QCAP_RECORD_FLAG_FULL, 0.0, 0.0, 0.0, 0, ref pszNULL);
                }
            }
            else
            {
                if (m_hCapDev != 0)
                {
                    EXPORTS.QCAP_STOP_RECORD(m_hCapDev, 0);
                }
            }
        }

        #endregion
    }
}
