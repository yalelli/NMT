using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;
using System.IO;
using System.Diagnostics;
using System.Management;
using System.Threading;

namespace NMT
{
    public partial class FrmEnquiry : Form
    {
        FrmMain frmMain = new FrmMain();
        public static double dCalculatedStiffness = 0;
        public static bool bCN = false;
        public static string strFilePathTemporary;
        public string strTime;

        private FrmStiffnessCalibration frmStiff;

        public FrmEnquiry(FrmMain newFrmMain, PointPairList listRecordData, FrmStiffnessCalibration frm1)
        {
            this.frmMain = newFrmMain;
            InitializeComponent();

            zgcApproachInitialize(listRecordData);
            dCalculatedStiffness = Slope(listRecordData);
            frmStiff = frm1;

            Control.CheckForIllegalCrossThreadCalls = false;//忽略错误线程
            Process.GetCurrentProcess().PriorityBoostEnabled = true;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
        }

        public void zgcApproachInitialize(PointPairList listRecordData)
        {
            string strLang = System.Globalization.CultureInfo.CurrentUICulture.Name;
            if (strLang == "zh-CN")
            {
                bCN = true;
            }

            if (!bCN)
            {
                zgcApproach.GraphPane.Title.Text = "Force - Displacement";
            }
            else
            {
                zgcApproach.GraphPane.Title.Text = "力 - 位移";
            }
            zgcApproach.GraphPane.Title.FontSpec.Size = 20f;

            if (!bCN)
            {
                zgcApproach.GraphPane.XAxis.Title.Text = "Displacement(nm)";//X轴标题
            }
            else
            {
                zgcApproach.GraphPane.XAxis.Title.Text = "位移(nm)";//X轴标题
            }
            zgcApproach.GraphPane.XAxis.Title.FontSpec.Size = 19f;
            //zgcIndentation.GraphPane.XAxis.Type = ZedGraph.AxisType.LinearAsOrdinal;
            zgcApproach.GraphPane.XAxis.Scale.MaxAuto = true;
            zgcApproach.GraphPane.XAxis.CrossAuto = true;//容许x轴的自动放大或缩小

            if (!bCN)
            {
                zgcApproach.GraphPane.YAxis.Title.Text = "Force(uN)";       //Y轴标题
            }
            else
            {
                zgcApproach.GraphPane.YAxis.Title.Text = "力(uN)";          //Y轴标题
            }
            zgcApproach.GraphPane.YAxis.Title.FontSpec.Size = 19f;

            zgcApproach.GraphPane.CurveList.Clear();
            zgcApproach.GraphPane.AddCurve("", listRecordData, Color.Blue, SymbolType.None);
            zgcApproach.AxisChange();   //画到zedGraphControl1控件中
            zgcApproach.Refresh();      //重新刷新
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Hide();

            FrmCalculate frmCalculate = new FrmCalculate(frmMain);
            frmCalculate.ShowDialog();

            this.Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #region Calculate Slope

        public static double Slope(PointPairList listRecordData)
        {
            List<double> input_x = new List<double>();
            List<double> input_y = new List<double>();
            for (int i = 0; i < listRecordData.Count; i++)
            {
                if (listRecordData[i].Y > 200 && listRecordData[i].Y < 7800)
                {
                    input_x.Add(listRecordData[i].X);
                    input_y.Add(listRecordData[i].Y);
                }
            }

            var copyInputValues_x = input_x.ToList();
            var copyInputValues_y = input_y.ToList();
            List<double> arr_x = new List<double>();
            List<double> arr_y = new List<double>();
            List<double> arr_xx = new List<double>();
            List<double> arr_xy = new List<double>();

            //arr_x = copyInputValues_x;
            for (int j = copyInputValues_y.Count - copyInputValues_y.Count; j < copyInputValues_y.Count; j++)
            {
                arr_x.Add(copyInputValues_x[j]);
                arr_y.Add(copyInputValues_y[j]);
            }

            double x_arr_dataAv = arr_x.Take(arr_x.Count).Average();
            double y_arr_dataAv = arr_y.Take(arr_y.Count).Average();
            for (int i = 0; i < arr_x.Count; i++)
            {
                arr_x[i] = arr_x[i] - x_arr_dataAv;
                arr_y[i] = arr_y[i] - y_arr_dataAv;
                arr_xx.Add(arr_x[i] * arr_x[i]);
                arr_xy.Add(arr_y[i] * arr_x[i]);
            }

            double sumxx = arr_xx.Sum();
            double sumxy = arr_xy.Sum();
            return sumxy / sumxx;
        }

        #endregion

        private void FrmEnquiry_Load(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            strTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string strTxt = strTime + ".txt";

            saveFileDialog1.Filter = "|*.xls;*.xlsx|(*.et;*.xls;*.xlsx)|*.et;*.xls;*.xlsx|All files(*.*)|*.*|txt files(*.txt)|*.txt|*.word|";//文本筛选,excel
            //SaveFile.Filter = "All files(*.*)|*.*|txt files(*.txt)|*.txt";//文本筛选,txt
            saveFileDialog1.FilterIndex = 3;//文本筛选器索引，选择第一项就是1
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                WritePointsToTxt(frmStiff.listRecordData, saveFileDialog1.FileName, strTxt);
            }
        }

        /// <summary>
        /// WritePointsToTxt
        /// </summary>
        /// <param name="list"></param>
        /// <param name="strFilePath"></param>
        /// <param name="strTxt"></param>
        public static void WritePointsToTxt(PointPairList list, string strFilePath, string strTxt)
        {
            string strFileFullPath = strFilePath+".txt";

            if (File.Exists(strFileFullPath))
            {
                File.Delete(strFileFullPath);
            }

            FileStream fileStream = new System.IO.FileStream(strFileFullPath, FileMode.Create);//using System.IO;
            StreamWriter streamWriter = new StreamWriter(fileStream);
            streamWriter.WriteLine("dDisplacement_nm_ori     dForce");

            for (int i = 0; i < list.Count; i++)
            {
                double dX = list[i].X;
                double dY = list[i].Y;
                string xjPointLine = dX.ToString() + "     " + dY.ToString();//行：x y
                streamWriter.WriteLine(xjPointLine);
            }




            streamWriter.Flush();//清空缓冲区
            streamWriter.Close();//关闭流
            fileStream.Close();
        }
    }
}
