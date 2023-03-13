using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;

namespace NMT
{
    public partial class FrmSensortivity : Form
    {
        //浅拷贝Main
        FrmMain frmMain = new FrmMain();

        public Thread mThread_CalibrateIndentation;
        public Thread mThread_CalibrateIndentationMonitoring;

        public bool bStop = false;
        public bool bIndent = true, bWhileIndent = true, bWhileIndentWithdraw = true;//Sam
        public int iApproachDelay = 0, iAdjustNum = 0, iIndentationDelay = 0;
        public uint uiIndentationSpeed = 0;
        public double dApproachStep = 0, dApproachForceThreshold = 0, dApproachRetreatDistance = 0, dMoveX = 0, dIndentationForceThreshold = 0, dIndentationRetreatDistance = 0;
        public double dStartDistance_ori = 0, dIndentDistance = 0;  //dIndentDistance每次移动的距离  dMoveEachTimeStep = dIndentDistance*1000；
        public string strLang;

        public double dMoveEachTimeStep;                //移动阈值um
        public int EachTimeDelayTime;                   //每一次移动的暂停时长ms

        /*速度对应表*/
        //CSScannerSpeed  MovingEachTimeDelayTime  velocity(nm/s)
        //71 30 100
        //110 10 300
        //106 20 150
        //60 40 50
        //47 90 20
        //23 100 10
        //4 180 1
        //12 100 5
        public uint CSScannerSpeed = 916;
        public int MovingEachTimeDelayTime = 10;     //71 30 100nm/s
        //关闭indent线程标志位
        public bool endCycle = false;
        public double CalibrationMax = 0; 

        public FrmSensortivity(FrmMain WinMain)
        {
            frmMain = WinMain;

            InitializeComponent();
            //cmbIndentSpeed.SelectedIndex = 0;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            FrmMain.bStabilization = false;
            bIndent = false;
            bWhileIndent = false;
            bWhileIndentWithdraw = true;
            endCycle = true;

            bntMoveto.Text = "开始移动";

            int iBackStep = 16;
            double dStepBit = 0;

            if (FrmMain.uiCurrentBit >= FrmMain.uiStartBit)
            {
                dStepBit = (FrmMain.uiCurrentBit - FrmMain.uiStartBit) * 1.0 / iBackStep;
                for (int i = 0; i < iBackStep; i++)
                {
                    Delay(25);
                    FrmMain.uiCurrentBit -= (uint)dStepBit;
                    FrmMain.mCScanPositioner.MoveToFinePosition(FrmMain.mCaxis_F, FrmMain.uiCurrentBit, 10, 20);
                }
            }
            else
            {
                dStepBit = (FrmMain.uiStartBit - FrmMain.uiCurrentBit) * 1.0 / iBackStep;
                for (int i = 0; i < iBackStep; i++)
                {
                    Delay(25);
                    FrmMain.uiCurrentBit += (uint)dStepBit;
                    FrmMain.mCScanPositioner.MoveToFinePosition(FrmMain.mCaxis_F, FrmMain.uiCurrentBit, 10, 20);
                }
            }

            Delay(100);
            FrmMain.dSetDAC = FrmMain.dInitValue_d;
            FrmMain.bStabilization = true;

            this.Close();
        }

        public void Delay(int milliSecond)
        {
            int start = Environment.TickCount;

            while (Math.Abs(Environment.TickCount - start) < milliSecond)   //毫秒
            {
                Application.DoEvents();
            }
        }


        private void bntMoveto_Click(object sender, EventArgs e)
        {
            if (mThread_CalibrateIndentation != null)
                mThread_CalibrateIndentation.Abort();

            if (bntMoveto.Text == "开始移动")
            {
                endCycle = false;
                bntMoveto.Text = "停止移动";

                //给用户设定的变量赋初值
                EachTimeDelayTime = Convert.ToInt32(txbConstantTime.Text) * 1000;
                dMoveEachTimeStep = Convert.ToInt32(txbDistanceStep.Text);

                //记录开始位置
                FrmMain.uiStartBit = FrmMain.uiCurrentBit = FrmMain.mCScanPositioner.GetFinePosition(FrmMain.mCaxis_F);
                //移动阈值 um
                dIndentDistance = dMoveEachTimeStep * Math.Pow(10, 3);
                //记录当前起始位置
                dStartDistance_ori = FrmMain.dDisplacement_nm_ori;

                Delay(100);
                this.frmMain.btnZeroing_Click(null, null);
                Delay(100);

                FrmMain.bStabilization = false;
                bIndent = true;
                bWhileIndent = true;
                bWhileIndentWithdraw = true;

                mThread_CalibrateIndentation = new Thread(new ThreadStart(ThreadFunction_CalibrateIndentation));
                mThread_CalibrateIndentation.Start();
            }
            else
            {
                FrmMain.bStabilization = false;
                bIndent = false;
                bWhileIndent = false;
                bWhileIndentWithdraw = true;

                endCycle = true;

                bntMoveto.Text = "开始移动";

                int iBackStep = 16;
                double dStepBit = 0;

                if (FrmMain.uiCurrentBit >= FrmMain.uiStartBit)
                {
                    dStepBit = (FrmMain.uiCurrentBit - FrmMain.uiStartBit) * 1.0 / iBackStep;
                    for (int i = 0; i < iBackStep; i++)
                    {
                        Delay(25);
                        FrmMain.uiCurrentBit -= (uint)dStepBit;
                        FrmMain.mCScanPositioner.MoveToFinePosition(FrmMain.mCaxis_F, FrmMain.uiCurrentBit, 10, 20);
                    }
                }
                else
                {
                    dStepBit = (FrmMain.uiStartBit - FrmMain.uiCurrentBit) * 1.0 / iBackStep;
                    for (int i = 0; i < iBackStep; i++)
                    {
                        Delay(25);
                        FrmMain.uiCurrentBit += (uint)dStepBit;
                        FrmMain.mCScanPositioner.MoveToFinePosition(FrmMain.mCaxis_F, FrmMain.uiCurrentBit, 10, 20);
                    }
                }

                Delay(100);
                FrmMain.dSetDAC = FrmMain.dInitValue_d;
                FrmMain.bStabilization = true;
            }
        }

        //无限循环压缩
        public void ThreadFunction_CalibrateIndentation()
        {
            while (!endCycle)
            {
                //在Indentation中
                while (bWhileIndent)
                {
                    if (bIndent && !endCycle)
                    {
                        //无限循环的监控
                        if ((FrmMain.dDisplacement_nm_ori >= dStartDistance_ori + Math.Abs(dIndentDistance)) || (FrmMain.dForce >= this.frmMain.dIndentForceThresholdMax))
                        {
                            bIndent = false;
                            bWhileIndent = false;
                        }

                        //一直加压
                        FrmMain.uiCurrentBit += CSScannerSpeed;
                        //当电压超过我们所设阈值
                        if (FrmMain.uiCurrentBit >= FrmMain.uiMaxADCValue)
                        {
                            FrmMain.uiCurrentBit = FrmMain.uiMaxADCValue;
                            bIndent = false;
                            bWhileIndent = false;
                        }
                        FrmMain.mCScanPositioner.MoveToFinePosition(FrmMain.mCaxis_F, FrmMain.uiCurrentBit, 10, 20);

                        Thread.Sleep(MovingEachTimeDelayTime);
                    }
                    else
                    {
                        bWhileIndent = false;
                    }
                }

                //indent结束，等待设定的时间
                if (!endCycle)
                {
                    //等待EachTimeDelayTime ms
                    FrmMain.dSetDAC = FrmMain.dReadDAC;
                    FrmMain.bStabilization = true;
                    Thread.Sleep(EachTimeDelayTime);

                    //获取当前indentation最大值
                    CalibrationMax = FrmMain.dDisplacement_nm_ori;
                    FrmMain.bStabilization = false;
                }

                //开始回退，结束时回退到初始位置
                while (bWhileIndentWithdraw)
                {
                    //当当前读取值小于Scanner初始值时，回退结束
                    if (FrmMain.dReadDAC > FrmMain.dInitValue_d && !endCycle)
                    {
                        //当当前电压值比在indentation中设定的速度值还小，将当前电压值置0，并关闭indentation的回退
                        if (FrmMain.uiCurrentBit <= FrmMain.uiIndentSpeed)
                        {
                            FrmMain.uiCurrentBit = 0;
                            bWhileIndentWithdraw = false;
                        }
                        else
                        {
                            FrmMain.uiCurrentBit -= CSScannerSpeed;
                        }

                        FrmMain.mCScanPositioner.MoveToFinePosition(FrmMain.mCaxis_F, FrmMain.uiCurrentBit, 10, 20);
                        Delay(MovingEachTimeDelayTime);
                    }
                    else
                    {
                        bWhileIndentWithdraw = false;
                    }
                }

                if (!endCycle)
                {
                    //完成恢复初始位置后等待EachTimeDelayTime设定的时间
                    Delay(50);
                    FrmMain.dSetDAC = FrmMain.dInitValue_d;
                    FrmMain.bStabilization = true;
                    Thread.Sleep(EachTimeDelayTime);

                    //从头开始，恢复标志位
                    FrmMain.bStabilization = false;
                    bIndent = true;
                    bWhileIndent = true;
                    bWhileIndentWithdraw = true;
                }
            }
        }

        //数据计算
        private void bntCalculate_Click(object sender, EventArgs e)
        {
            if (Regex.IsMatch(txtDistance.Text.ToString(), "^([0-9][0-9]*)+(\\.[0-9]{1,3})?$"))
            {
                this.frmMain.txbSensitivity_d.Text = (1000 * Convert.ToDouble(txtDistance.Text) / CalibrationMax * Convert.ToDouble(this.frmMain.txbSensitivity_d.Text)).ToString();
                this.frmMain.UpdateSensorDefaultValue();
            }
            else
                MessageBox.Show("请输入数字。","注意");
        }

        private void cmbIndentSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (cmbIndentSpeed.SelectedIndex == 0)//1nm/s
            //{
            //    CSScannerSpeed = 4;
            //    MovingEachTimeDelayTime = 180;
            //}
            //else if (cmbIndentSpeed.SelectedIndex == 1)//5nm/s
            //{
            //    CSScannerSpeed = 12;
            //    MovingEachTimeDelayTime = 100;
            //}
            //else if (cmbIndentSpeed.SelectedIndex == 2)//10nm/s
            //{
            //    CSScannerSpeed = 23;
            //    MovingEachTimeDelayTime = 100;
            //}
            //else if (cmbIndentSpeed.SelectedIndex == 3)//20nm/s
            //{
            //    CSScannerSpeed = 47;
            //    MovingEachTimeDelayTime = 90;
            //}
            //else if (cmbIndentSpeed.SelectedIndex == 4)//50nm/s
            //{
            //    CSScannerSpeed = 60;
            //    MovingEachTimeDelayTime = 40;
            //}
            //else if (cmbIndentSpeed.SelectedIndex == 5)//100nm/s
            //{
            //    CSScannerSpeed = 71;
            //    MovingEachTimeDelayTime = 30;
            //}
            //else if (cmbIndentSpeed.SelectedIndex == 6)//150nm/s
            //{
            //    CSScannerSpeed = 106;
            //    MovingEachTimeDelayTime = 20;
            //}
            //else /*if (cmbIndentSpeed.SelectedIndex == 7)//300nm/s*/
            //{
            //    CSScannerSpeed = 110;
            //    MovingEachTimeDelayTime = 10;
            //}   
        }
    }
}

