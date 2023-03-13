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
using System.Text.RegularExpressions;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;
using NMT.Joystick;
using NMT.Nators;
using NMT.SmarAct;
using NMT;

namespace NMT
{
    public partial class Frm5Graph : Form
    {
        //窗体放大缩小
        float xvalues, yvalues = 0;

        FrmMain frmMain = new FrmMain();
        FrmIndentationCalibration FrmIndentCalibrat;

        //偏差值
        public double[] Deltax = new double[2];
        public double[] Deltay = new double[2];
        public static bool bCN = false;

        //对齐后的曲线
        public PointPairList[] graphalign = new PointPairList[2];
        //基准曲线
        public PointPairList GraphAlignStandard = new PointPairList(); 

        public Frm5Graph(FrmMain newFrmMain, FrmIndentationCalibration FrmIndentCalibration)
        {
            //this.Resize += new EventHandler(MainForm_Resize); //添加窗体拉伸重绘事件
            //xvalues = this.Width;//记录窗体初始大小
            //yvalues = this.Height;
            //SetTag(this);

            //将当前主窗体控件传入当前窗体中
            this.frmMain = newFrmMain;
            FrmIndentCalibrat = FrmIndentCalibration;

            //窗体初始化
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;//忽略错误线程
            Process.GetCurrentProcess().PriorityBoostEnabled = true;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

            //当前zgc控件初始化
            zgcApproachInitialize();
            //加载基准曲线 GraphAlignStandard
            LoadDatumCurve();

            //选中两个曲线
            cbgraph0.Checked = true;
            cbgraph1.Checked = true;


            /*每次按下重新做差值*/
            for (int i = 0; i < 2; i++)
            {
                //x,y的差值
                Deltax[i] = DataTransform(FrmMain.listRecordData[i][0].X, FrmMain.listRecordData[i][0].Y) - GraphAlignStandard[0].X;
                Deltay[i] = FrmMain.listRecordData[i][0].Y - GraphAlignStandard[0].Y;
                //把每个曲线都刷新
                graphalign[i] = new PointPairList();
            }

            //对每条曲线与基准曲线对齐向图表数据添加图像点
            for (int j = 0; j < 2; j++)
            {
                //曲线添加曲线点
                for (int i = 0; i < FrmMain.listRecordData[j].Count; i++)
                    graphalign[j].Add(DataTransform(FrmMain.listRecordData[j][i].X, FrmMain.listRecordData[j][i].Y) - Deltax[j], FrmMain.listRecordData[j][i].Y - Deltay[j]);
            }

            zedGraphDataprocess.GraphPane.CurveList.Clear();

            //基准曲线
            zedGraphDataprocess.GraphPane.AddCurve("", GraphAlignStandard, Color.Black, SymbolType.None);

            if (cbgraph0.Checked)
            {
                zedGraphDataprocess.GraphPane.AddCurve("", graphalign[0], Color.Blue, SymbolType.None);
            }

            if (cbgraph1.Checked)
            {
                zedGraphDataprocess.GraphPane.AddCurve("", graphalign[1], Color.Red, SymbolType.None);
            }

            zedGraphDataprocess.AxisChange();           //画到zedGraphControl1控件中
            zedGraphDataprocess.Refresh();              //重新刷新
        }

        //关闭按钮
        private void btnClose_Click(object sender, EventArgs e)
        {
            FrmIndentCalibrat.Close();
            this.Close();
        }

        //加载基准曲线到 GraphAlignStandard
        public void LoadDatumCurve()
        {
            string path = Application.StartupPath + "\\3" + ".txt";

            if (File.Exists(path))
            {
                using (FileStream fsReadControllerID = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //创建缓冲区的大小
                    byte[] bufControllerID = new byte[fsReadControllerID.Length];
                    //向bufControllerID中写入
                    fsReadControllerID.Read(bufControllerID, 0, bufControllerID.Length);
                    //将字节数组转换成字符串，在字符串中进行字符分割，去空，转化成double数组后
                    string[] words = Encoding.Default.GetString(bufControllerID).Split(new char[4] { '\t', '\r', '\n',' ' });
                    words = words.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                    double[] doubleArray = Array.ConvertAll<string, double>(words, s => double.Parse(s));

                    //基准曲线点添加完成
                    for (int i = 0; i < doubleArray.Length; i = i + 2)
                        GraphAlignStandard.Add(doubleArray[i] - doubleArray[i + 1]* 1000 / 3750.0, doubleArray[i + 1]);
                }
            }
        }

        //曲线显示按钮
        private void btAlign_Click(object sender, EventArgs e)
        {
            /*每次按下重新做差值*/
            for (int i = 0; i < 2; i++)
            {
                //x,y的差值
                Deltax[i] = DataTransform(FrmMain.listRecordData[i][0].X, FrmMain.listRecordData[i][0].Y) - GraphAlignStandard[0].X;
                Deltay[i] = FrmMain.listRecordData[i][0].Y - GraphAlignStandard[0].Y;
                //把每个曲线都刷新
                graphalign[i] = new PointPairList();
            }

            //对每条曲线与基准曲线对齐向图表数据添加图像点
            for (int j = 0; j < 2; j++)
            {
                //曲线添加曲线点
                for (int i = 0; i < FrmMain.listRecordData[j].Count; i++)
                    graphalign[j].Add(DataTransform(FrmMain.listRecordData[j][i].X, FrmMain.listRecordData[j][i].Y) - Deltax[j], FrmMain.listRecordData[j][i].Y - Deltay[j]);
            }

            zedGraphDataprocess.GraphPane.CurveList.Clear();

            //基准曲线
            zedGraphDataprocess.GraphPane.AddCurve("", GraphAlignStandard, Color.Black, SymbolType.None);

            if (cbgraph0.Checked)
            {
                zedGraphDataprocess.GraphPane.AddCurve("", graphalign[0], Color.Blue, SymbolType.None);
            }

            if (cbgraph1.Checked)
            {
                zedGraphDataprocess.GraphPane.AddCurve("", graphalign[1], Color.Red, SymbolType.None);
            }

            zedGraphDataprocess.AxisChange();           //画到zedGraphControl1控件中
            zedGraphDataprocess.Refresh();              //重新刷新
        }

        //计算压痕公式
        public double DataTransform(double x, double y)
        {
            //位移 - 力/刚度
            double z = x - y * 1000 / Double.Parse(txbStiffness.Text);
            return z;
        }

        private void btnSaveStiffness_Click(object sender, EventArgs e)
        {
            //main窗体里的刚度extra设置为：5graph - main
            this.frmMain.txbExtraStiffness.Text = (Convert.ToDouble(txbStiffness.Text) - Convert.ToDouble(this.frmMain.txbStiffness.Text)).ToString();
            this.frmMain.UpdateSensorDefaultValue();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txbStiffness.Text = (Convert.ToDouble(txbStiffness.Text) + 1).ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txbStiffness.Text = (Convert.ToDouble(txbStiffness.Text) - 1).ToString();
        }

        public void zgcApproachInitialize()
        {
            string strLang = System.Globalization.CultureInfo.CurrentUICulture.Name;
            if (strLang == "zh-CN")
            {
                bCN = true;
            }

            if (!bCN)
            {
                zedGraphDataprocess.GraphPane.Title.Text = "Force - Displacement";
            }
            else
            {
                zedGraphDataprocess.GraphPane.Title.Text = "力 - 压痕";
            }
            zedGraphDataprocess.GraphPane.Title.FontSpec.Size = 20f;

            if (!bCN)
            {
                zedGraphDataprocess.GraphPane.XAxis.Title.Text = "Displacement(nm)";//X轴标题
            }
            else
            {
                zedGraphDataprocess.GraphPane.XAxis.Title.Text = "压痕(nm)";//X轴标题
            }
            zedGraphDataprocess.GraphPane.XAxis.Title.FontSpec.Size = 19f;
            //zgcIndentation.GraphPane.XAxis.Type = ZedGraph.AxisType.LinearAsOrdinal;
            zedGraphDataprocess.GraphPane.XAxis.Scale.MaxAuto = true;
            zedGraphDataprocess.GraphPane.XAxis.CrossAuto = true;//容许x轴的自动放大或缩小

            if (!bCN)
            {
                zedGraphDataprocess.GraphPane.YAxis.Title.Text = "Force(uN)";       //Y轴标题
            }
            else
            {
                zedGraphDataprocess.GraphPane.YAxis.Title.Text = "力(uN)";          //Y轴标题
            }
            zedGraphDataprocess.GraphPane.YAxis.Title.FontSpec.Size = 19f;

            zedGraphDataprocess.GraphPane.CurveList.Clear();
            zedGraphDataprocess.AxisChange();   //画到zedGraphControl1控件中
            zedGraphDataprocess.Refresh();      //重新刷新
        }

        private void Frm5Graph_FormClosed(object sender, FormClosedEventArgs e)
        {
            FrmIndentCalibrat.Close();
            this.Close();
        }

        //数据存储按钮
        private void bntSaveData_Click(object sender, EventArgs e)
        {
            //开启数据存储
            PointPairList list0 = new PointPairList();      //数据点
            PointPairList list1 = new PointPairList();      //数据点

            for (int i = 0; i < FrmMain.listRecordDataRong[0].Count; i++)
            {
                list0.Add(FrmMain.listRecordDataRong[0][i].X, FrmMain.listRecordDataRong[0][i].Y);
            }

            for (int i = 1; i < FrmMain.listRecordDataRong[1].Count; i++)
            {
                list1.Add(FrmMain.listRecordDataRong[1][i].X, FrmMain.listRecordDataRong[1][i].Y);
            }

            string strTxt0_ori = "list0_ori.txt";
            FrmMain.WritePointsToTxt_Indent(list0, FrmMain.strFilePathTemporary, strTxt0_ori);

            string strTxt1_ori = "list1_ori.txt";
            FrmMain.WritePointsToTxt_Indent(list1, FrmMain.strFilePathTemporary, strTxt1_ori);

            /********************** 保存多次回撤数据的原始值及处理后的值 *********************/
            ///*每次按下重新做差值*/
            //for (int i = 0; i < 2; i++)
            //{
            //    //x,y的差值
            //    Deltax[i] = DataTransform(FrmMain.listRecordData[i][0].X, FrmMain.listRecordData[i][0].Y) - GraphAlignStandard[0].X;
            //    Deltay[i] = FrmMain.listRecordData[i][0].Y - GraphAlignStandard[0].Y;
            //    //把每个曲线都刷新
            //    graphalign[i] = new PointPairList();
            //}

            ////对每条曲线与基准曲线对齐，向图表数据添加图像点
            //for (int j = 0; j < 2; j++)
            //{
            //    //曲线添加曲线点
            //    for (int i = 0; i < FrmMain.listRecordData[j].Count; i++)
            //        graphalign[j].Add(DataTransform(FrmMain.listRecordData[j][i].X, FrmMain.listRecordData[j][i].Y) - Deltax[j], FrmMain.listRecordData[j][i].Y - Deltay[j]);
            //}

            ////开启数据存储
            //PointPairList listIndent0_ori = new PointPairList();//数据点
            //PointPairList listIndent1_ori = new PointPairList();//数据点

            //PointPairList listIndent0 = new PointPairList();//数据点
            //PointPairList listIndent1 = new PointPairList();//数据点

            //for (int i = 0; i < FrmMain.listRecordData[0].Count; i++)
            //{
            //    listIndent0_ori.Add(FrmMain.listRecordData[0][i].X, FrmMain.listRecordData[0][i].Y);
            //}

            //for (int i = 1; i < FrmMain.listRecordData[1].Count; i++)
            //{
            //    listIndent1_ori.Add(FrmMain.listRecordData[1][i].X, FrmMain.listRecordData[1][i].Y);
            //}

            //for (int i = 0; i < graphalign[0].Count; i++)
            //{
            //    listIndent0.Add(graphalign[0][i].X, graphalign[0][i].Y);
            //}

            //for (int i = 1; i < graphalign[1].Count; i++)
            //{
            //    listIndent1.Add(graphalign[1][i].X, graphalign[1][i].Y);
            //}

            //string strTxt0_ori = "listIndent0_ori.txt";
            //FrmMain.WritePointsToTxt_Indent(listIndent0_ori, FrmMain.strFilePathTemporary, strTxt0_ori);

            //string strTxt1_ori = "listIndent1_ori.txt";
            //FrmMain.WritePointsToTxt_Indent(listIndent1_ori, FrmMain.strFilePathTemporary, strTxt1_ori);

            //string strTxt0 = "listIndent0.txt";
            //FrmMain.WritePointsToTxt_Indent(listIndent0, FrmMain.strFilePathTemporary, strTxt0);

            //string strTxt1 = "listIndent1.txt";
            //FrmMain.WritePointsToTxt_Indent(listIndent1, FrmMain.strFilePathTemporary, strTxt1);     
        }

        private void btnOther_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "文本文件| *.txt";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK && saveFileDialog1.FileName.Length > 0)
            {
                string strSourceFileName0, strSourceFileName1,strSourceFileName0_ori, strSourceFileName1_ori, strDestFileName, strDestFileName_ori;
                strSourceFileName0_ori = FrmMain.strFilePathTemporary + "\\" + "listIndent0_ori.txt";
                strSourceFileName1_ori = FrmMain.strFilePathTemporary + "\\" + "listIndent1_ori.txt";
                strSourceFileName0 = FrmMain.strFilePathTemporary + "\\" + "listIndent0.txt";
                strSourceFileName1 = FrmMain.strFilePathTemporary + "\\" + "listIndent1.txt";

                strDestFileName_ori = saveFileDialog1.FileName.Remove(saveFileDialog1.FileName.Length - 4) + "_ori.txt";
                
                if (File.Exists(strSourceFileName0_ori))
                {
                    File.Move(strSourceFileName0_ori, strDestFileName_ori);
                }

                if (File.Exists(strSourceFileName1_ori))
                {
                    if (File.Exists(strDestFileName_ori))
                    {
                        File.Delete(strDestFileName_ori);
                    }

                    File.Move(strSourceFileName1_ori, strDestFileName_ori);
                }

                strDestFileName = saveFileDialog1.FileName;
                if (File.Exists(strSourceFileName0))
                {
                    if (File.Exists(strDestFileName))
                    {
                        File.Delete(strDestFileName);
                    }

                    File.Move(strSourceFileName0, strDestFileName);
                }

                if (File.Exists(strSourceFileName1))
                {
                    if (File.Exists(strDestFileName))
                    {
                        File.Delete(strDestFileName);
                    }

                    File.Move(strSourceFileName1, strDestFileName);
                }
            }
        }
    }
}
