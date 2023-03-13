using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace NMT
{
    public partial class FrmIndentationCalibration : Form
    {
        FrmMain frmMain = new FrmMain();
        public Thread mThread_AutoApproach_1;
        public Thread mThread_AutoApproach_2;
        public Thread mThread_Indentation;
        public Thread mThread_ApproachMonitoring;
        public Thread mThread_IndentationMonitoring;
        public Thread mThread_RecordData;

        public bool bStop = false;
        public bool bApproach_1 = true, bWhileApproach_1 = true, bWhileAdjust_1 = true;
        public bool bApproach_1_Firstin = false, bApproach_2_Firstin = false;
        public bool bApproach_2 = true, bWhileApproach_2 = true, bWhileAdjust_2 = true;
        public bool bIndent = true, bIndentation = true, bWhileIndent = true, bWhileIndentWithdraw = true, bLoadData = false, bLoadDataRong = false;//Sam
        public int iApproachDelay = 0, iAdjustNum = 0, iIndentationDelay = 0;
        public uint uiIndentationSpeed = 0;
        public double dApproachStep = 0, dApproachForceThreshold = 0, dApproachRetreatDistance = 0, dMoveX = 0, dIndentationForceThreshold = 0, dIndentationRetreatDistance = 0;
        public string strLang;

        //SAM Add
        public uint cycleCount = 0;
        public bool sApproachOver = true;
        public bool sStep3IndentOver = true;
        public int diffDelay = 700;

        public FrmIndentationCalibration(FrmMain newFrmMain)
        {
            frmMain = newFrmMain;

            InitializeComponent();

            //每次进入窗体都初始化该list
            for(int i=0; i<2;i++)
            {
                FrmMain.listRecordData[i] = new PointPairList();    //Sam
                FrmMain.listRecordDataRong[i] = new PointPairList();    //Sam
            }

            strLang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }



        private void btnStart_Click(object sender, EventArgs e)
        {
            if (mThread_AutoApproach_1 != null)
            {
                mThread_AutoApproach_1.Abort();
            }

            if (mThread_AutoApproach_2 != null)
            {
                mThread_AutoApproach_2.Abort();
            }

            if (mThread_Indentation != null)
            {
                mThread_Indentation.Abort();
            }

            if (mThread_ApproachMonitoring != null)
            {
                mThread_ApproachMonitoring.Abort();
            }

            if (mThread_IndentationMonitoring != null)
            {
                mThread_IndentationMonitoring.Abort();
            }

            if (mThread_RecordData != null)
            {
                mThread_RecordData.Abort();
            }

            if (btnStart.Text == "Start" || btnStart.Text == "开始")
            {
                if (FrmMain.mCCoarsePositioner[0].IsConnected && FrmMain.mCScanPositioner.IsConnected && FrmMain.serialPort.IsOpen)
                {
                    if (strLang != "zh-CN")
                    {
                        btnStart.Text = "Stop";
                    }
                    else
                    {
                        btnStart.Text = "停止";
                    }

                    btnCancel.Enabled = false;

                    //三个过程初始标志
                    bApproach_1 = true;
                    bWhileApproach_1 = true;
                    bWhileAdjust_1 = true;

                    bApproach_2 = true;
                    bWhileApproach_2 = true;
                    bWhileAdjust_2 = true;

                    bIndent = true;
                    bIndentation = true;
                    bWhileIndent = true;
                    bWhileIndentWithdraw = true;
                    bStop = false;

                    dApproachStep = FrmMain.dStepFirst;
                    iApproachDelay = FrmMain.iDelayFirst;
                    dApproachForceThreshold = FrmMain.dForceFirst;
                    dMoveX = 10 * Math.Pow(10, 3);
                    dApproachRetreatDistance = 5 * Math.Pow(10, 3);
                    frmMain.btnZeroing_Click(null, null);
                    FrmMain.Delay(100);

                    //从新开了个监测
                    //mThread_ApproachMonitoring = new Thread(new ThreadStart(ThreadFunction_ApproachMonitoring_1));
                    //mThread_ApproachMonitoring.Start();

                    //开启第一次approach
                    mThread_AutoApproach_1 = new Thread(new ThreadStart(ThreadFunction_Approach_1));
                    mThread_AutoApproach_1.Start(); 
                }
            }
            else
            {   //停止所有操作   
                if (strLang != "zh-CN")
                {
                    btnStart.Text = "Start";
                }
                else
                {
                    btnStart.Text = "开始";
                }

                //将循环计数清零
                cycleCount = 0;

                btnCancel.Enabled = true;
                bStop = true;
                //第一步
                bApproach_1 = false;
                bWhileApproach_1 = false;
                bWhileAdjust_1 = false;
                //第二步
                bApproach_2 = false;        //用于监控
                bWhileApproach_2 = false;
                bWhileAdjust_2 = false;
                bIndent = false;            //bIndent indent过程的监控，看是否需要再往下
                bIndentation = false;       //是为了记录数据  
                bWhileIndent = false;
                bWhileIndentWithdraw = false;
                sStep3IndentOver = false;
                sApproachOver = false;
                bLoadData = false;
                bLoadDataRong = false;

                bApproach_1_Firstin = false;
                bApproach_2_Firstin = false;

                txblog.Text = "停止.\r\n ";
                //恢复三个控制器的初始位置
                //FrmMain.dSetDAC = FrmMain.dInitValue_d;
                //FrmMain.bStabilization = true;
                //FrmMain.mCSmarActPositioner.MoveDistance_CloseLoop_Absolute(FrmMain.mCaxis_Z, 0);
                //FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop_Absolute(FrmMain.mCaxis_X, 0);
            }
        }

        //函数功能：取消按钮
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        //第一次的AutoApproach
        public void ThreadFunction_Approach_1()
        {
            int approachadjust1i = 0;

            if (!bStop)
            {
                //精度标定日志
                txblog.Text += "开始Step 1的自动接近.\r\n ";
            }

            //AutoApproach到特定位置
            while (bWhileApproach_1)
            {
                if (!bStop)
                {
                    //step delay velocity
                    //5 100 50nm/s
                    //10 50 200nm/s
                    //20 30 660nm/s
                    //40 20 2000nm/s
                    Thread.Sleep(iApproachDelay);

                    if (FrmMain.dForce >= Math.Abs(dApproachForceThreshold))
                    {
                        Thread.Sleep(diffDelay);
                        bApproach_1_Firstin = true;

                        if (FrmMain.dForce >= Math.Abs(dApproachForceThreshold))
                            bApproach_1 = false;

                        bApproach_1_Firstin = false;
                    }

                    if (bApproach_1)
                    {
                        if (bApproach_1_Firstin)
                            FrmMain.mCCoarsePositioner[0].Stop(FrmMain.mCaxis_Y);
                        else
                            FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, dApproachStep * FrmMain.iYP);//dApproachStep
                    }
                    else
                    {
                        bWhileApproach_1 = false;

                        break;
                    }
                }
            }

            if (!bStop)
            {
                //精度标定日志
                txblog.Text += "Step 1接近结束，开始接近后的调整.\r\n ";
            }

            //如果速度过快就会超出，所以需要用Adjust进行调整。
            while (bWhileAdjust_1)
            {
                if (!bStop)
                {
                    //Delay(25);
                    Thread.Sleep(25);

                    approachadjust1i++;
                    if (approachadjust1i == 20)
                    {
                        approachadjust1i = 0;

                        //精度标定日志
                        txblog.Text += "正在调整（step1）.\r\n ";
                    }

                    //如果快到的时候有数据超过阈值，则
                    if (FrmMain.dForce - Math.Abs(dApproachForceThreshold) < -10)//the same direction of approach//Sensor A:0.5,Sensor C:10
                    {
                        iAdjustNum = 0;
                        uint uiResult = FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, 4 * FrmMain.iYP);//SAM
                    }//力如果超出
                    else if (FrmMain.dForce - Math.Abs(dApproachForceThreshold) > 10)//the negative direction of approach//Sensor A:0.5,Sensor C:10
                    {
                        iAdjustNum = 0;
                        uint uiResult = FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, 4 * FrmMain.iYN);//SAM
                    }
                    else
                    {   //检测到有10次在范围内，就默认已经到达。
                        iAdjustNum++;
                        if (iAdjustNum == 10)
                        {
                            iAdjustNum = 0;
                            bWhileAdjust_1 = false;
                        }
                    }
                }
            }

            //往上抬5um
            if (!bStop)
            {
                //精度标定日志
                txblog.Text += "Step1结束, 等待4s.\r\n ";
                Thread.Sleep(4000);

                //精度标定日志
                txblog.Text += "回退5um.\r\n";

                int step1RetreatStep = 2000;
                int waitTimeBetweenStep = 0;

                //精度标定日志
                txblog.Text += "Step1结束,后撤2um。\r\n ";

                FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, step1RetreatStep * FrmMain.iYN);//SAM
                FrmMain.Delay(waitTimeBetweenStep);
                
                //FrmMain.mCSmarActPositioner.MoveDistance_CloseLoop(FrmMain.mCaxis_Z, dApproachRetreatDistance * FrmMain.iZP);
            }

            FrmMain.Delay(200);

            //Sam
            while (cycleCount < 2)
            {
                Thread.Sleep(200);

                if (sStep3IndentOver && sApproachOver)
                {
                    //完成一次后对标志位重新赋值和计数++
                    cycleCount++;
                    sStep3IndentOver = false;
                    sApproachOver = false;

                    bApproach_2 = true;
                    bWhileApproach_2 = true;
                    bWhileAdjust_2 = true;

                    bIndent = true;
                    bIndentation = true;
                    bWhileIndent = true;
                    bWhileIndentWithdraw = true;
                    bStop = false;
                    bLoadData = false;
                    bLoadDataRong = false;

                    //step2：开始
                    //x轴往旁边挪10um
                    if (!bStop)
                    {
                        txblog.Text += "第" + cycleCount + "次标定。"+"x轴往旁边挪10um。\r\n ";

                        FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_X, dMoveX);
                        txblog.Text += "第" + cycleCount + "次标定开始。\r\n ";
                    }

                    FrmMain.Delay(500);
                    dApproachStep = FrmMain.dStepSecond;
                    iApproachDelay = FrmMain.iDelaySecond;
                    dApproachForceThreshold = FrmMain.dForceSecond;
                    //dMoveX = 0;
                    dApproachRetreatDistance = 0;
                    frmMain.btnZeroing_Click(null, null);
                    FrmMain.Delay(500);

                    if (!bStop)
                    {
                        //第二次AutoApproach的监控
                        mThread_ApproachMonitoring = new Thread(new ThreadStart(ThreadFunction_ApproachMonitoring_2));
                        mThread_ApproachMonitoring.Start();
                    }

                    if (!bStop)
                    {   //第二次AutoApproach的移动
                        mThread_AutoApproach_2 = new Thread(new ThreadStart(ThreadFunction_Approach_2));
                        mThread_AutoApproach_2.Start();
                    }
                }
            }
        }


        public void ThreadFunction_ApproachMonitoring_1()
        {
            while (bApproach_1)
            {
                Thread.Sleep(1);

                if (FrmMain.dForce >= Math.Abs(dApproachForceThreshold))
                {
                    Thread.Sleep(diffDelay);
                    bApproach_1_Firstin = true;

                    if (FrmMain.dForce >= Math.Abs(dApproachForceThreshold))
                        bApproach_1 = false;

                    bApproach_1_Firstin = false;
                }
            }
        }


        //函数功能：开启第二次AutoApproach及indentation
        public void ThreadFunction_Approach_2()
        {
            //循环20次一显示
            int approach1i = 0;
            int adjust1i = 0;

            //第二步的自动接近
            while (bWhileApproach_2)
            {
                if (!bStop)
                {
                    approach1i++;
                    if (approach1i == 20)
                    {
                        approach1i = 0;
                        txblog.Text += "第" + cycleCount + "次的自动接近中... ...\r\n ";
                    }
                    //Delay(iApproachDelay);
                    Thread.Sleep(iApproachDelay);

                    if (bApproach_2)
                    {
                        if (bApproach_2_Firstin)
                            FrmMain.mCCoarsePositioner[0].Stop(FrmMain.mCaxis_Y);//SAM
                        else
                            FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, dApproachStep * FrmMain.iYP);//SAM
                    }
                    else
                    {
                        bWhileApproach_2 = false;
                        //FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop_Absolute(FrmMain.mCaxis_Y, FrmMain.iPositionZ);//SAM
                        //这句话的意义是什么
                        //FrmMain.mCSmarActPositioner.MoveDistance_CloseLoop_Absolute(FrmMain.mCaxis_Z, FrmMain.iPositionZ);
                        break;
                    }
                }
            }

            //Sam
            //第二步的调整
            while (bWhileAdjust_2)
            {
                if (!bStop)
                {
                    adjust1i++;
                    if (adjust1i == 20)
                    {
                        adjust1i = 0;
                        txblog.Text += "第" + cycleCount + "次的自动接近后的调整中... ...\r\n ";
                    }

                    //Delay(25);
                    Thread.Sleep(25);
                    if (FrmMain.dForce - Math.Abs(dApproachForceThreshold) < -10)//the same direction of approach//Sensor A:0.5,Sensor C:10
                    {
                        iAdjustNum = 0;
                        uint uiResult = FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, 4 * FrmMain.iYP);//SAM
                        //FrmMain.mCSmarActPositioner.MoveDistance_CloseLoop(FrmMain.mCaxis_Z, 1 * FrmMain.iZN);
                    }
                    else if (FrmMain.dForce - Math.Abs(dApproachForceThreshold) > 10)//the negative direction of approach//Sensor A:0.5,Sensor C:10
                    {
                        iAdjustNum = 0;
                        uint uiResult = FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, 4 * FrmMain.iYN);//SAM
                        //FrmMain.mCSmarActPositioner.MoveDistance_CloseLoop(FrmMain.mCaxis_Z, 1 * FrmMain.iZP);
                    }
                    else
                    {
                        iAdjustNum++;
                        if (iAdjustNum == 10)
                        {
                            iAdjustNum = 0;
                            bWhileAdjust_2 = false;
                        }
                    }
                }
            }

            Thread.Sleep(5000);

            //在这里添加标定刚度的参数
            //FrmMain.Delay(500);
            uiIndentationSpeed = FrmMain.uiStepThird;               //压缩速度
            iIndentationDelay = FrmMain.iDelayThird;                //
            dIndentationForceThreshold = FrmMain.dForceThird;       //压缩力的阈值
            dIndentationRetreatDistance = 1 * Math.Pow(10, 3);

            //indentation的监控
            if (!bStop)
            {
                frmMain.btnZeroing_Click(null, null);

                //放开Scanner的保持
                FrmMain.bStabilization = false;

                sApproachOver = true;       

                txblog.Text += "第" + cycleCount + "次的自动接近结束。\r\n 等待5s。\r\n";
                txblog.Text += "第" + cycleCount + "次的压缩开始。\r\n ";

                mThread_IndentationMonitoring = new Thread(new ThreadStart(ThreadFunction_IndentationMonitoring));
                mThread_IndentationMonitoring.Start();
            }

            //第三步indentation的执行过程
            if (!bStop)
            {
                bLoadDataRong = true;
                mThread_Indentation = new Thread(new ThreadStart(ThreadFunction_Indentation));
                mThread_Indentation.Start();           
            }

            //SAM
            //存储数据
            if (!bStop)
            {
                mThread_RecordData = new Thread(new ThreadStart(ThreadFunction_RecordData));
                mThread_RecordData.Start();
            }
        }


        //第二次AutoApproach的监控
        public void ThreadFunction_ApproachMonitoring_2()
        {
            while (bApproach_2)
            {
                Thread.Sleep(1);

                if (FrmMain.dForce >= Math.Abs(dApproachForceThreshold))
                {
                    Thread.Sleep(diffDelay);
                    bApproach_2_Firstin = true;

                    if (FrmMain.dForce >= Math.Abs(dApproachForceThreshold))
                    {
                        bApproach_2 = false;
                    }
                    bApproach_2_Firstin = false;
                }
            }
        }


        public void ThreadFunction_Indentation()
        {
            int adjust1i = 0;

            while (bWhileIndent)
            {
                if (!bStop)
                {
                    adjust1i++;
                    if (adjust1i == 20)
                    {
                        adjust1i = 0;
                        txblog.Text += "第" + cycleCount + "次的压缩中。\r\n ";
                    }

                    if (bIndent)
                    {
                        FrmMain.uiCurrentBit += uiIndentationSpeed;

                        //当到最大值时
                        if (FrmMain.uiCurrentBit >= FrmMain.uiMaxADCValue)
                        {
                            FrmMain.uiCurrentBit = FrmMain.uiMaxADCValue;
                            bIndent = false;
                            bWhileIndent = false;
                        }

                        FrmMain.mCScanPositioner.MoveToFinePosition(FrmMain.mCaxis_F, FrmMain.uiCurrentBit, 10, 20);
                        FrmMain.Delay(iIndentationDelay);
                    }
                    else
                    {
                        bWhileIndent = false;
                        //加载保持时间
                        Thread.Sleep(5000);
                        //开始存储数据
                        bLoadData = true;
                    }
                }
            }

            adjust1i = 0;

            while (bWhileIndentWithdraw)
            {
                if (!bStop)
                {
                    adjust1i++;
                    if (adjust1i == 20)
                    {
                        adjust1i = 0;
                        txblog.Text += "第" + cycleCount + "次的压缩调整中... ...\r\n ";
                    }

                    if (FrmMain.dReadDAC > FrmMain.dInitValue_d)
                    {
                        if (FrmMain.uiCurrentBit <= uiIndentationSpeed)
                        {
                            FrmMain.uiCurrentBit = 0;
                            bWhileIndentWithdraw = false;
                        }
                        else
                        {
                            FrmMain.uiCurrentBit -= uiIndentationSpeed;
                        }

                        FrmMain.mCScanPositioner.MoveToFinePosition(FrmMain.mCaxis_F, FrmMain.uiCurrentBit, 10, 20);
                        FrmMain.Delay(iIndentationDelay);
                    }
                    else
                    {
                        bWhileIndentWithdraw = false;
                    }
                }
            }

            bIndent = false;
            bIndentation = false;

            if (!bStop)
            {
                txblog.Text += "第" + cycleCount + "次的压缩结束后向回撤1um.\r\n";
            
                //压完再往回撤1um Sam改
                FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, 1000 * FrmMain.iYN);//Sam  dIndentationRetreatDistance
            }

            //这个函数的作用?
            if (bStop)
            {
                int iBackStep = 16;
                double dStepBit = 0;

                if (FrmMain.uiCurrentBit >= FrmMain.uiStartBit)
                {
                    dStepBit = (FrmMain.uiCurrentBit - FrmMain.uiStartBit) * 1.0 / iBackStep;
                    for (int i = 0; i < iBackStep; i++)
                    {
                        FrmMain.Delay(25);
                        FrmMain.uiCurrentBit -= (uint)dStepBit;
                        FrmMain.mCScanPositioner.MoveToFinePosition(FrmMain.mCaxis_F, FrmMain.uiCurrentBit, 10, 20);
                    }
                }
                else
                {
                    dStepBit = (FrmMain.uiStartBit - FrmMain.uiCurrentBit) * 1.0 / iBackStep;
                    for (int i = 0; i < iBackStep; i++)
                    {
                        FrmMain.Delay(25);
                        FrmMain.uiCurrentBit += (uint)dStepBit;
                        FrmMain.mCScanPositioner.MoveToFinePosition(FrmMain.mCaxis_F, FrmMain.uiCurrentBit, 10, 20);
                    }
                }
            }

            //scanner恢复原位
            FrmMain.dSetDAC = FrmMain.dInitValue_d;
            FrmMain.bStabilization = true;

            if (!bStop)
            {
                txblog.Text += "第" + cycleCount + "次的压缩结束。\r\n 等待5s\r\n";
                Thread.Sleep(5000);

                sStep3IndentOver = true;
            }

            //2，3步，5次结束后
            if (cycleCount == 2)
            {
                if (strLang != "zh-CN")
                {
                    btnStart.Text = "Start";
                }
                else
                {
                    btnStart.Text = "开始";
                }

                if (!bStop)
                {
                    FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_X, dMoveX);
                }

                //Sam   
                if (!bStop)
                {
                    //while (mThread_AutoApproach_2.ThreadState != System.Threading.ThreadState.Running) ;//判断线程的状态是不是在执行
                    //this.Close();
                    //FrmMain.listRecordData[cycleCount - 1].Add(0, 0);
                    Frm5Graph frmEnquiry = new Frm5Graph(frmMain,this);
                    frmEnquiry.ShowDialog();

                    //恢复Cancel按钮
                    btnCancel.Enabled = true;
                    //if (frmEnquiry.ShowDialog() == DialogResult.OK)
                    //{
                    //    this.Close();
                    //}
                }
            }
        }


        public void ThreadFunction_IndentationMonitoring()
        {
            while (bWhileIndentWithdraw)
            {
                Thread.Sleep(1);

                if (FrmMain.dForce >= Math.Abs(dIndentationForceThreshold))
                {
                    bIndent = false;
                   
                }
            }
        }


        public void ThreadFunction_RecordData()
        {
            while (bWhileIndentWithdraw)
            {
                Thread.Sleep(25);

                if (bLoadData)
                {
                    double x = FrmMain.dDisplacement_nm_ori;
                    double y = FrmMain.dForce;

                    FrmMain.listRecordData[cycleCount - 1].Add(x, y);
                }

                if (bLoadDataRong)
                {
                    double x1 = FrmMain.dDisplacement_nm_ori;
                    double y1 = FrmMain.dForce;

                    FrmMain.listRecordDataRong[cycleCount - 1].Add(x1, y1);
                }
            }
        }

        private void txblog_TextChanged(object sender, EventArgs e)
        {
            this.txblog.SelectionStart = this.txblog.Text.Length;
            this.txblog.ScrollToCaret();
        }
    }
}
