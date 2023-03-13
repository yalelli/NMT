using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using NMT.Nators;
using NMT.Joystick;
using ZedGraph;
using NMT.SmarAct;

namespace NMT
{
    public partial class FrmMain : Form
    {
        private string str = "";
        public string strStiffnessTxt
        {
            get
            {
                return str;
            }
            set
            {
                str = value;
                this.txbStiffness.Text = str;
            }
        }

        public FrmHighFreqWaiting frmHighFreqWaiting;
        public FrmCapture frmCapture;

        public static CCoarsePositioner[] mCCoarsePositioner = new CCoarsePositioner[2];
        public static CScanPositioner mCScanPositioner;
        public CParameter mCParameter = new CParameter();

        public GerneralFunction mGnlFunction = new GerneralFunction();
        public GetFunctions mGetFunctions = new GetFunctions();

        public static SerialPort serialPort = new SerialPort();
        public Byte[] TransmitBuf = new Byte[300000];
        public DateTime dClickStart = new DateTime();

        public delegate void Displaydelegate(byte[] InputBuf);
        public Displaydelegate disp_delegate;

        public static PointPairList listCur_F = new PointPairList();
        public static PointPairList listCur_d = new PointPairList();
        public static PointPairList listRecord_F = new PointPairList();
        public static PointPairList listRecord_d = new PointPairList();
        public PointPairList listIndentExtend = new PointPairList();
        public PointPairList listIndentWithdraw = new PointPairList();
        public PointPairList listIndentExtend_ori = new PointPairList();
        public PointPairList listIndentWithdraw_ori = new PointPairList();
        public PointPairList listIndentNoMissing = new PointPairList();
        public PointPairList listIndentNoMissing_ori = new PointPairList();
        public static PointPairList[] listRecordData = new PointPairList[2];           //Sam
        public static PointPairList[] listRecordDataRong = new PointPairList[2];           //Sam

        public SiApp.SpwRetVal res;
        public SiApp.SiDeviceName devName = new SiApp.SiDeviceName();
        public IntPtr hWnd = IntPtr.Zero;
        public IntPtr devHdl = IntPtr.Zero;

        public Thread mThread_StateMonitoring;
        public Thread mThread_PositionMonitoring;
        public Thread mThread_PositionDisplay;
        public Thread mThread_MovementMonitoring;
        public Thread mThread_JoyStick;
        public Thread mThread_AutoApproach;
        public Thread mThread_ScannerStabilization_PID;
        public Thread mThread_ShowCurve;
        public Thread mThread_Indent;
        public Thread mThread_GetFinePosition;
        public Thread mThread_ShowForceSensorCurve;
        public Thread mThread_Calibrate;

        public float xvalues, yvalues;
        public const string appName = "Test Siapp";
        public const string templateTR = "TX: {1,-7}{0}TY: {2,-7}{0}TZ: {3,-7}{0}RX: {4,-7}{0}RY: {5,-7}{0}RZ: {6,-7}{0}P: {7}";
        public const string logFileName = "log.txt";
        //public string[] strID = new string[2] { "0789663041", "4101006099" };
        //public string[] strID = new string[2] { "9527862264", "9491989489" };
        //public string[] strID = new string[2] { "2553517363", "9491989489" };
        public string[] strControllerID = new string[4];
        public string strTime;
        public static string strFilePathTemporary;
        public static string strControllerIDPath, strSensorDirectionPath, strRangeSettingsPath, strCurveSelectionPath, strDACRangePath, strAxisDirectionPath, strStiffnessCalibrationPath, strSensorDefaultValuePath, strZgcDefaultValuePath, strApproachIndentDefaultValuePath, strPIDDefaultValuePath;
        public string[] strStiffnessCalibration = new string[6];
        public string[] strAxisDirection = new string[6];
        public static string[] strSensorDefaultValue = new string[19];
        public string[] strZgcDefaultValue = new string[7];
        public string[] strApproachIndentDefaultValue = new string[22];
        public string[] strPIDDefaultValue = new string[3];
        public static string strSensitivityLast_F, strSensitivityLast_d, strStiffnessLast, strSensorForceThresholdLast;     //Sam static
        public string strMaxDurationLast, strMinForceLast_F, strMaxForceLast_F, strMinForceLast_d, strMaxForceLast_d;
        public string strSuffix_F = "b", strSuffix_d = "d";
        public List<string> strHighFreqListExtend = new List<string>();
        public List<string> strHighFreqListWithdraw = new List<string>();

        public static bool bCN = false;
        public static bool bLeft = false;
        public static bool bPause = false;
        public bool bMainFormRunning = true;
        public bool bFirstReceived = true;
        public bool bCkbSafetyModeCheckedChangedLast = false, bCkbAutoScaleCheckedChangedLast_F = false, bCkbAutoScaleCheckedChangedLast_d = false;
        public static bool bInit_F = true, bInit_d = true;
        public bool bRecord = false;
        public bool bEnableX = true, bEnableY = true, bEnableZ = true;
        public bool bMoveX = true, bMoveY = true, bMoveZ = true, bMoveXDown = true, bMoveYDown = true, bMoveZDown = true;
        public bool bContinuous = true, bStep = false;
        public bool bStartDisplay = false;
        public bool bJoystickXCur = false, bJoystickYCur = false, bJoystickZCur = false;
        public bool bJoystickXLast = false, bJoystickYLast = false, bJoystickZLast = false;
        public bool bApproach = false, bWhileApproach = true, bWhileAdjust = true, bStopApproach = false;
        public bool bApproachFirstIn = false;

        //显示withdraw1还是withdraw2的图像
        //bExtend:压缩到压缩保持阶段的标志位
        public bool bShowCurve = false, bExtend = false, bWithdraw1 = false, bWithdraw2 = false;
        public bool bIndent = false, bIndentation = false, bWhileIndent = true, bWhileIndentWithdraw = true;
        public bool bClockwiseDown = false, bAnticlockwiseDown = false;
        public bool bHighFreq = false;
        public bool bFirstWithdraw_1 = true, bFirstWithdraw_2 = true, bFirstIndent = true;
        public bool bGetPoint = false;
        public bool bSample = false, bScanner = true;
        public static bool bStabilization = true, bOpenCL = true;
        public bool bExtendDelay = false, bWithdrawDelay = false;
        public bool bTestRecord = false;
        public bool bIndentExtend = false;
        public bool bCompression = false;

        public decimal delta = 0, deltaLast = 0, dSum = 0;
        public decimal u = 0.001m;
        public decimal P = 0, I = 0, D = 0;//P(比例):0.1622,sensor:9959012~11575068=>voltage:0~262143,P = voltage / sensor;I(积分):值越大越不容易稳定;D(微分)
        public decimal P_ori = 0, I_ori = 0.1m, D_ori = 0m;

        public static double dStepFirst = 0, dForceFirst = 0, dStepSecond = 0, dForceSecond = 0, dForceThird = 0;
        public static double dReadDAC = 0, dSetDAC = 0, dSetDAC_ori = 0;
        public double dDACMin = 0, dDACMax = 0, dDACMin_Zoom = 0, dDACMax_Zoom = 0;
        public double dPointXLast_ori_1 = 0, dPointYLast_ori_1 = 0, dPointXLast_1 = 0, dPointYLast_1 = 0;
        public double dPointXLast_ori_2 = 0, dPointYLast_ori_2 = 0, dPointXLast_2 = 0, dPointYLast_2 = 0;
        public double deltaX_ori_1 = 0, deltaX_1 = 0, deltaY_ori_1 = 0, deltaY_1 = 0;
        public double deltaX_ori_2 = 0, deltaX_2 = 0, deltaY_ori_2 = 0, deltaY_2 = 0;
        public double deltaX_Sam = 0, deltaY_Sam = 0;
        public double dIndentDistanceMin = 0, dIndentDistanceMax = 0, dIndentForceThresholdMin = 0, dIndentForceThresholdMax = 0;
        public static double dInitValue_F = 0, dInitValue_d = 0;
        public static double dForce = 0, dDisplacement_nm_ori = 0, dDisplacement_nm_Indent = 0, dDisplacement_um_ori = 0, dDisplacement_um_Indent = 0;
        public static double dSensitivity_F = 0, dSensitivity_d = 0, dStiffness = 0, dExtraStiffness = 0;
        public double dPositionX = 0, dPositionY = 0, dPositionZ = 0;
        public double dApproachStep = 0;
        public double dSensorForceThreshold = 0, dApproachForceThreshold = 0, dApproachRetreatDistance = 0;
        public double dStartDistance_ori = 0, dStartDistance_Indent = 0, dIndentDistance = 0, dIndentStartForce = 0, dIndentForceThreshold = 0, dForceDistance_test=0;
        public double dDistanceMax = 0, dStep = 0;
        public double dStopDistance = 0;
        public double dApproachByCL = 0;
        public double dWithdrawDisp = 0;
        public double dIndentApproximateSpeed_nm = 0;
        public double[,] dSAF_OL = new double[15, 3] {{  1,  1600,  100 }, {  1,  2200,  100 }, {   1,  2800,   100 }, {   1, 3400,  100 }, 
                                                      {  1,  4095,  100 }, {  2,  4095,  200 }, {   4,  4095,   300 }, {  10, 4095,  500 }, 
                                                      {  20, 4095, 1200 }, {  50, 4095, 3000 }, {  100, 4095,  5000 }, { 300, 4095, 7000 },
                                                      { 500, 4095, 8000 }, { 800, 4095, 9000 }, { 1000, 4095, 10000 }};
        public double[] dDistance_CL = new double[15] { 10, 20, 50, 100, 500, 1000, 1500, 4000, 4375, 4500, 4610, 4700, 4800, 4875, 5000 };//nm
        public double[] dStep_CL = new double[15] { 0.001, 0.01, 0.1, 0.5, 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000 };//um

        public const int WM_DEVICE_CHANGE = 0x219;              //设备改变           
        public const int DBT_DEVICEARRIVAL = 0x8000;            //设备插入
        public const int DBT_DEVICE_REMOVE_COMPLETE = 0x8004;   //设备移除
        public static int iXP = 1, iYP = 1, iZP = 1, iXN = -1, iYN = -1, iZN = 1, iClockwise = 1, iAnticlockwise = -1;
        public static int iDelayFirst = 0, iDelaySecond = 0, iDelayThird = 0;
        public int iDirection_F = 1, iDirection_d = 1;
        public int iComportIndexLast = -1;
        public int iGetNum = 0, iGetNumLast = 0;
        public static int iPointCount_F = 0, iPointCount_d = 0;
        public int iMaxDurationTime = 5, iNumPerSec = 50;
        public int iBtnNum;//摇杆按键返回的数字
        public int iXData = 0, iYData = 0, iZData = 0;
        public int iBtnSteps = 0, iCurSteps = 0;
        public static int iPositionX = 0, iPositionY = 0, iPositionZ = 0;
        public int iIndentTypeIndex = -1;
        public int iSpeed = 0, iContinuousSpeed = 7, iStepSpeed = 7, iSpeedTotalLevel = 15;
        public int iApproachDirectionIndex = 0, iApproachDelay = 0, iAdjustNum = 0;
        public int iIndentDelay = 0;
        public int iExtendDelay = 0, iWithdrawDelay = 0;

        public static uint uiStepThird = 0;
        public static uint mCaxis_X = 0, mCaxis_Y = 1, mCaxis_Z = 2, mCaxis_F = 0, mCaxis_R = 0, uiChannelNum = 0;
        public uint uiAmplitude = 0, uiFrequencyMax = 0;
        public static uint uiIndentSpeed = 0, uiInitBit = 25000, uiStartBit = 0, uiCurrentBit = 0;
        public uint uiVoltage = 0;

        public int bMainLoadOver = -1;      //Sam

        //输入的最大值
        public static uint uiMinADCValue = 0, uiMaxADCValue = 262143;//2^18-1
        public uint uiIndentSpeedIndex = 0;

        public byte[] NtLocator1, NtLocator2, ScanLocator;
        private List<byte> buffer = new List<byte>(4096);

        //静态类在整个项目中是资源共享的。
        public static CSmarActPositioner mCSmarActPositioner;                              //SAM ADD
    }
}
