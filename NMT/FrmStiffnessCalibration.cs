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
    public partial class FrmStiffnessCalibration : Form
    {
        FrmMain frmMain = new FrmMain();
        public Thread mThread_AutoApproach_1;
        public Thread mThread_AutoApproach_2;
        public Thread mThread_Indentation;
        public Thread mThread_ApproachMonitoring;
        public Thread mThread_IndentationMonitoring;
        public Thread mThread_RecordData;
        public PointPairList listRecordData = new PointPairList();
        public bool bStop = false;
        public bool bApproach_1 = true, bWhileApproach_1 = true, bWhileAdjust_1 = true;
        public bool bApproach_2 = true, bWhileApproach_2 = true, bWhileAdjust_2 = true;

        public bool bApproach_1_Firstin = false, bApproach_2_Firstin = false;

        public bool bIndent = true, bIndentation = true, bWhileIndent = true, bWhileIndentWithdraw = true;
        public int iApproachDelay = 0, iAdjustNum = 0, iIndentationDelay = 0;
        public uint uiIndentationSpeed = 0;
        public double dApproachStep = 0, dApproachForceThreshold = 0, dApproachRetreatDistance = 0, dMoveX = 0, dIndentationForceThreshold = 0, dIndentationRetreatDistance = 0;
        public string strLang;

        //SAM Add
        public uint cycleCount = 0;
        public bool smbApproachOver = true;
        public bool smbIndentOver = true;
        private FrmStiffnessCalibration frmStiff;


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

        public FrmStiffnessCalibration(FrmMain newFrmMain)
        {
            this.frmMain = newFrmMain;
            InitializeComponent();

            strLang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        public FrmStiffnessCalibration()
        {
            InitializeComponent();

        }

        //函数功能：点一下自动执行5次
        //开始stiffness标定
        //autoapproach+indentation的操作
        //先autoapproach,往上抬5um
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Text == "Start" || btnStart.Text == "开始")
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

                listRecordData = new PointPairList();

                //如果界面上的是start就执行autoapproach
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

                    ////从新开了个监测
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
                bWhileIndent = false;       //  
                bWhileIndentWithdraw = false;

                //防止压缩时的抖动
                bApproach_1_Firstin = false;
                bApproach_2_Firstin = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        public void ThreadFunction_Approach_1()
        {
            //AutoApproach到特定位置
            while (bWhileApproach_1)
            {
                if (!bStop)
                {
                    if (FrmMain.dForce >= Math.Abs(dApproachForceThreshold))
                    {
                        Thread.Sleep(1000);
                        bApproach_1_Firstin = true;

                        if (FrmMain.dForce >= Math.Abs(dApproachForceThreshold))
                            bApproach_1 = false;

                        bApproach_1_Firstin = false;
                    }

                    //Delay(iApproachDelay);
                    Thread.Sleep(iApproachDelay);

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
                        //FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop_Absolute(FrmMain.mCaxis_Z, FrmMain.iPositionZ);
                        break;
                    }
                }
            }

            //如果速度过快就会超出，所以需要用adjust进行调整。
            while (bWhileAdjust_1)
            {            
                if (!bStop)
                {
                    //Delay(25);
                    Thread.Sleep(25);
                
                    //如果快到的时候有数据超过阈值，则
                    if (FrmMain.dForce - Math.Abs(dApproachForceThreshold) < -10)//the same direction of approach//Sensor A:0.5,Sensor C:10
                    {
                        iAdjustNum = 0;
                        uint uiResult = FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, 4 * FrmMain.iYP);
                    }//力如果超出
                    else if (FrmMain.dForce - Math.Abs(dApproachForceThreshold) > 10)//the negative direction of approach//Sensor A:0.5,Sensor C:10
                    {
                        iAdjustNum = 0;
                        uint uiResult = FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, 4 * FrmMain.iYN);
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

            //step1:往上抬5个um
            if (!bStop)
            {
                FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, 2000 * FrmMain.iYN);
            }

            FrmMain.Delay(200);

            smbIndentOver = false;
            smbApproachOver = false;

            bApproach_2 = true;
            bWhileApproach_2 = true;
            bWhileAdjust_2 = true;

            bIndent = true;
            bIndentation = true;
            bWhileIndent = true;
            bWhileIndentWithdraw = true;
            //bStop = false;

            //step2：开始
            //x轴往旁边挪10um
            if (!bStop)
            {
                FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_X, dMoveX);
            }

            FrmMain.Delay(500);
            dApproachStep = FrmMain.dStepSecond;
            iApproachDelay = FrmMain.iDelaySecond;
            dApproachForceThreshold = FrmMain.dForceSecond;
            dMoveX = 0;
            dApproachRetreatDistance = 0;
            if (!bStop)
            {
                frmMain.btnZeroing_Click(null, null);
            }
            FrmMain.Delay(500);

            if (!bStop)
            {
                mThread_ApproachMonitoring = new Thread(new ThreadStart(ThreadFunction_ApproachMonitoring_2));
                mThread_ApproachMonitoring.Start();
            }

            if (!bStop)
            {
                mThread_AutoApproach_2 = new Thread(new ThreadStart(ThreadFunction_Approach_2));
                mThread_AutoApproach_2.Start();
            }
        }

        public void ThreadFunction_ApproachMonitoring_1()
        {
            while (bApproach_1)
            {
                Thread.Sleep(1);


            }
        }

        public void ThreadFunction_Approach_2()
        {
            //第二步的自动接近
            while (bWhileApproach_2)
            {
                if (!bStop)
                {
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
                        //FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop_Absolute(FrmMain.mCaxis_Z, FrmMain.iPositionZ);
                        break;
                    }
                }
            }

            //第二步的调整
            while (bWhileAdjust_2)
            {            
                if (!bStop)
                {
                    //Delay(25);
                    Thread.Sleep(25);
                    if (FrmMain.dForce - Math.Abs(dApproachForceThreshold) < -5)//the same direction of approach//Sensor A:0.5,Sensor C:10
                    {
                        iAdjustNum = 0;
                        uint uiResult = FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, 4 * FrmMain.iYP);
                    }
                    else if (FrmMain.dForce - Math.Abs(dApproachForceThreshold) > 5)//the negative direction of approach//Sensor A:0.5,Sensor C:10
                    {
                        iAdjustNum = 0;
                        uint uiResult = FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, 4 * FrmMain.iYN);
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

            //在这里添加标定刚度的参数
            FrmMain.Delay(500);
            uiIndentationSpeed          = FrmMain.uiStepThird;      //压缩速度
            iIndentationDelay           = FrmMain.iDelayThird;      //
            dIndentationForceThreshold  = FrmMain.dForceThird;      //压缩力的阈值
            dIndentationRetreatDistance = 1 * Math.Pow(10, 3);

            if (!bStop)
            {
                frmMain.btnZeroing_Click(null, null);
            }

            FrmMain.bStabilization      = false;

            FrmMain.Delay(500);
            smbApproachOver             = true;

            //indentation的监控
            if (!bStop)
            {
                mThread_IndentationMonitoring = new Thread(new ThreadStart(ThreadFunction_IndentationMonitoring));
                mThread_IndentationMonitoring.Start();
            }

            //indentation的执行过程
            if (!bStop)
            {
                mThread_Indentation = new Thread(new ThreadStart(ThreadFunction_Indentation));
                mThread_Indentation.SetApartmentState(ApartmentState.STA);
                mThread_Indentation.IsBackground = true;
                mThread_Indentation.Start();
            }

            //存储数据
            if (!bStop)
            {
                mThread_RecordData = new Thread(new ThreadStart(ThreadFunction_RecordData));
                mThread_RecordData.Start();
            }
        }


        public void ThreadFunction_ApproachMonitoring_2()
        {
            while (bApproach_2)
            {
                Thread.Sleep(1);

                if (FrmMain.dForce >= Math.Abs(dApproachForceThreshold))
                {
                    Thread.Sleep(1000);
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
            while (bWhileIndent)
            {
                if (!bStop)
                {
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
                    }
                }
            }

            while (bWhileIndentWithdraw)
            {
                if (!bStop)
                {
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
            btnCancel.Enabled = true;

            if (!bStop)
            {
                FrmMain.mCCoarsePositioner[0].MoveDistance_CloseLoop(FrmMain.mCaxis_Y, dIndentationRetreatDistance * FrmMain.iYN);
            }

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

            //恢复原位
            FrmMain.dSetDAC = FrmMain.dInitValue_d;
            FrmMain.bStabilization = true;

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
                //while (mThread_AutoApproach_2.ThreadState != System.Threading.ThreadState.Running) ;//判断线程的状态是不是在执行
                //this.Close();

                FrmEnquiry frmEnquiry = new FrmEnquiry(frmMain, listRecordData, this);
                frmEnquiry.ShowDialog();
                //if (frmEnquiry.ShowDialog() == DialogResult.OK)
                //{
                //    this.Close();
                //}
            }
        }

        public void ThreadFunction_IndentationMonitoring()
        {
            while (bIndent)
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

            while (bIndentation)
            {
                Thread.Sleep(25);

                double x = FrmMain.dDisplacement_nm_ori;
                double y = FrmMain.dForce;
                listRecordData.Add(x, y);
            }
        }

    }
}
