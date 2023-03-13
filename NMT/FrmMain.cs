using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;
using NMT.Joystick;
using NMT.Nators;
using ZedGraph;
using NMT.SmarAct;

namespace NMT
{
    public partial class FrmMain : Form, IMessageFilter
    {
        public FrmMain()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;//忽略错误线程
            Process.GetCurrentProcess().PriorityBoostEnabled = true;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            LoadDatumCurve();

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
                }
            }
        }
                
        #region IMessageFilter 成员

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == 522)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Main Form Events

        private void FrmMain_Load(object sender, EventArgs e)
        {
            string strLang = System.Globalization.CultureInfo.CurrentUICulture.Name;
            if (strLang == "zh-CN")
            {
                bCN = true;
            }

            this.Resize += new EventHandler(MainForm_Resize); //添加窗体拉伸重绘事件
            xvalues = this.Width;//记录窗体初始大小
            yvalues = this.Height;
            SetTag(this);

            Application.AddMessageFilter(this);//屏蔽鼠标滚轮相关事件

            //tabPageNanoindentation.Parent = null;
            tabPageTension.Parent = null;

            GetComList();

            uiStartBit = uiCurrentBit = uiInitBit;
            hWnd = Handle;
            frmHighFreqWaiting = new FrmHighFreqWaiting();

            // Prepare log file
            File.Create(logFileName).Close();

            strControllerIDPath = Application.StartupPath + "\\ControllerID.lfd";
            strSensorDirectionPath = Application.StartupPath + "\\SensorDirection.lfd";
            strRangeSettingsPath = Application.StartupPath + "\\RangeSettings.lfd";
            strCurveSelectionPath = Application.StartupPath + "\\CurveSelection.lfd";
            strDACRangePath = Application.StartupPath + "\\DACRange.lfd";
            strAxisDirectionPath = Application.StartupPath + "\\AxisDirection.lfd";
            strStiffnessCalibrationPath = Application.StartupPath + "\\StiffnessCalibration.lfd";

            strSensorDefaultValuePath = Application.StartupPath + "\\SensorDefaultValue.lfd";
            strZgcDefaultValuePath = Application.StartupPath + "\\ZgcDefaultValue.lfd";
            strApproachIndentDefaultValuePath = Application.StartupPath + "\\ApproachIndentDefaultValue.lfd";

            strPIDDefaultValuePath = Application.StartupPath + "\\PIDDefaultValue.lfd";

            cmbSensorVersion.SelectedIndex = 0;
            cmbApproachSpeed.SelectedIndex = 0;
            cmbApproachMotionDirection.SelectedIndex = 0;
            cmbIndentSpeed.SelectedIndex = 0;
            cmbIndentType.SelectedIndex = 2;
            cmbCompressSpeed.SelectedIndex = 0;
            cmbCompressType.SelectedIndex = 2;

            LoadControllerID();
            LoadSensorDirection();
            LoadRangeSettings();
            LoadCurveSelection();
            LoadDACRange();
            LoadAxisDirection();
            LoadStiffnessCalibration();

            LoadSensorDefaultValue();
            LoadZgcDefaultValue();
            LoadApproachIndentDefaultValue();

            LoadPIDDefaultValue();

            mCCoarsePositioner[0] = new CCoarsePositioner(mCParameter.CoarsePositionSensorConnected, mCParameter.Coarse_Max_StepSize_nm, mCParameter.Coarse_Max_DACValue_Bit);
            mCCoarsePositioner[1] = new CCoarsePositioner(mCParameter.CoarsePositionSensorConnected, mCParameter.Coarse_Max_StepSize_nm, mCParameter.Coarse_Max_DACValue_Bit);
            //SAM add
            //mCSmarActPositioner = new CSmarActPositioner(mCParameter.CoarsePositionSensorConnected, mCParameter.Coarse_Max_StepSize_nm, mCParameter.Coarse_Max_DACValue_Bit);//SAM

            mCScanPositioner = new CScanPositioner(mCParameter.CoarsePositionSensorConnected, mCParameter.Coarse_Max_StepSize_nm, mCParameter.Coarse_Max_DACValue_Bit);

            disp_delegate = new Displaydelegate(DispUI);

            CreatSaveDirectory();

            GeneralControlInitialize();
            zgcForceSensorInitialize();
            zgcIndentationInitialize();
            zgcCompressionInitialize();

            iComportIndexLast = cmbComport.SelectedIndex;

            //for (int i = 0; i < 10; i++)
            {
                //Delay(100);
                mCScanPositioner.MoveToFinePosition(mCaxis_F, uiInitBit, 10, 20);
                //Delay(10);
                //uiVoltage = mCScanPositioner.GetFinePosition(mCaxis_F);
                //textBox1.Text = uiVoltage.ToString();
            }

            //btnIndentation_Click(null, null);
            //Delay(100);
            //btnIndentation_Click(null, null);

            //zgcIndentation_ori.GraphPane.CurveList.Clear();
            //zgcIndentation_ori.AxisChange();//画到zedGraphControl1控件中
            //zgcIndentation_ori.Refresh();//重新刷新
            //zgcIndentation.GraphPane.CurveList.Clear();
            //zgcIndentation.AxisChange();//画到zedGraphControl1控件中
            //zgcIndentation.Refresh();//重新刷新

            //btnZeroing_Click(null, null);

            //暂停按钮使能
            bPauseIndent.Enabled = false;
            bIndentPause.Enabled = false;

            Delay(100);
            BackgroundThread_Initialization();

            //frmCapture = new FrmCapture();
            //frmCapture.Show();

        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            bMainFormRunning = false;
            UpdateSensorDefaultValue();
            UpdateZgcDefaultValue();
            UpdateApproachIndentDefaultValue();
            UpdatePIDDefaultValue();
            CloseForceSensor();
            CloseSiApp();

            //while (Thread.CurrentThread.IsAlive)
            //{
            //    Thread.CurrentThread.Abort();
            //}

            Environment.Exit(0);
        }

        #endregion

        #region Form Stretch Redraw

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

        #region Controller Connection

        public void AFM_CoarsePositioner_CheckReconnection_1()
        {
            NtLocator1 = mCCoarsePositioner[0].GetSystemLocator();

            if (mCCoarsePositioner[0].IsConnected == false)
            {
                if (mCCoarsePositioner[0].Initialize(strControllerID[0]) != 0)
                {
                    mGnlFunction.MY_DEBUG("CoarsePositioner 1 Initialize fail.");
                }

                //Delay(10);
            }
        }

        //SAM SmarAct
        public void AFM_SmarActPositioner_CheckReconnection()
        {
            if (mCSmarActPositioner.IsConnected == false)
            {
                if (mCSmarActPositioner.Initialize(strControllerID[1]) != 0)
                {
                    mGnlFunction.MY_DEBUG("SmarActPositioner Initialize fail.");
                }
            }
        }

        public void AFM_CoarsePositioner_CheckReconnection_2()
        {
            //SAM
            NtLocator2 = mCCoarsePositioner[1].GetSystemLocator();

            if (mCCoarsePositioner[1].IsConnected == false)
            {
                if (mCCoarsePositioner[1].Initialize(strControllerID[1]) != 0)
                {
                    mGnlFunction.MY_DEBUG("CoarsePositioner 2 Initialize fail.");
                }

                //Delay(10);
            }
        }

        public void AFM_CoarsePositioner_CheckReconnection_3()
        {
            ScanLocator = mCScanPositioner.GetSystemLocator();

            if (mCScanPositioner.IsConnected == false)
            {
                if (mCScanPositioner.Initialize(strControllerID[2]) != 0)
                {
                    mGnlFunction.MY_DEBUG("CoarsePositioner 2 Initialize fail.");
                }
                else
                {
                    btnAdcReset_Click(null, null);
                }

                //Delay(10);
            }
        }

        public void ForceSensor_CheckReconnection()
        {
            if (!serialPort.IsOpen)
            {
                ConnectForceSensor(true);

                //if (ConnectForceSensor())
                //{
                //    Delay(10);
                //    if (serialPort.IsOpen)
                //    {
                //        serialPort.Write("5f\r\n");
                //        Delay(10);
                //        serialPort.Write("7t\r\n");
                //        Delay(10);
                //    }
                //}
            }
        }

        public void Joystick_CheckReconnection()
        {
            bool bFound = false;
            string[] strArr = GetHarewareInfo(HardwareEnum.Win32_PnPEntity, "Name");
            for (int i = 0; i < strArr.Length; i++)
            {
                //if (strArr[i] == "3Dconnexion SpaceMouse Pro")
                if (strArr[i] == "HID-compliant device")
                {
                    bFound = true;
                    break;
                }
            }

            if (!bFound)
            {
                devName.name = "";
            }

            if (bFound && devName.name == "")
            {
                //初始化驱动程序
                if (InitializeSiApp())
                {
                    //向驱动程序输出应用程序命令
                    ExportApplicationCommands();

                    ////设置在3Dconnexion设备上使用按钮库/操作集
                    //SiApp.SiAppCmdActivateActionSet(devHdl, @"ACTION_SET_ID");
                }
            }
        }

        #endregion

        #region Get Comport Number of Force Sensor

        public void GetComList()
        {
            RegistryKey keyCom = Registry.LocalMachine.OpenSubKey("Hardware\\DeviceMap\\SerialComm");
            if (keyCom != null)
            {
                string[] sSubKeys = keyCom.GetValueNames();
                cmbComport.Items.Clear();
                foreach (string sName in sSubKeys)
                {
                    string sValue = (string)keyCom.GetValue(sName);
                    cmbComport.Items.Add(sValue);
                }

                if (serialPort.IsOpen)
                {
                    for (int i = 0; i < cmbComport.Items.Count; i++)
                    {
                        if (cmbComport.Items[i].ToString() == serialPort.PortName)
                        {
                            cmbComport.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the target com num.
        /// </summary>
        /// <returns></returns>
        public static int GetComNum()
        {
            int iCountNum = 0;
            int comNum = -1;
            string[] strArr = GetHarewareInfo(HardwareEnum.Win32_PnPEntity, "Name");
            foreach (string s in strArr)
            {
                Debug.WriteLine(s);

                if (s.Length >= 23 && s.Contains("CH340"))
                {
                    int start = s.IndexOf("(") + 3;
                    int end = s.IndexOf(")");
                    comNum = Convert.ToInt32(s.Substring(start + 1, end - start - 1));
                    iCountNum++;
                }
            }

            if (iCountNum == 1)//有且只有一个符合的COM口
            {
                return comNum;
            }
            else if (iCountNum > 1)//有两个及以上符合的COM口
            {
                return 0;
            }
            else//没有符合的COM口
            {
                return -1;
            }
        }

        #endregion

        #region Get the system devices information

        /// <summary>
        /// Get the system devices information with windows api.
        /// </summary>
        /// <param name="hardType">Device type.</param>
        /// <param name="propKey">the property of the device.</param>
        /// <returns></returns>
        private static string[] GetHarewareInfo(HardwareEnum hardType, string propKey)
        {
            List<string> strs = new List<string>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + hardType))
                {
                    var hardInfos = searcher.Get();
                    foreach (var hardInfo in hardInfos)
                    {
                        if (hardInfo.Properties[propKey].Value != null)
                        {
                            String str = hardInfo.Properties[propKey].Value.ToString();
                            strs.Add(str);
                        }

                    }
                }
                return strs.ToArray();
            }
            catch
            {
                return null;
            }
            finally
            {
                strs = null;
            }
        }//end of func GetHarewareInfo().

        /// <summary>
        /// 枚举win32 api
        /// </summary>
        public enum HardwareEnum
        {
            // 硬件
            Win32_Processor, // CPU 处理器
            Win32_PhysicalMemory, // 物理内存条
            Win32_Keyboard, // 键盘
            Win32_PointingDevice, // 点输入设备，包括鼠标。
            Win32_FloppyDrive, // 软盘驱动器
            Win32_DiskDrive, // 硬盘驱动器
            Win32_CDROMDrive, // 光盘驱动器
            Win32_BaseBoard, // 主板
            Win32_BIOS, // BIOS 芯片
            Win32_ParallelPort, // 并口
            Win32_SerialPort, // 串口
            Win32_SerialPortConfiguration, // 串口配置
            Win32_SoundDevice, // 多媒体设置，一般指声卡。
            Win32_SystemSlot, // 主板插槽 (ISA & PCI & AGP)
            Win32_USBController, // USB 控制器
            Win32_NetworkAdapter, // 网络适配器
            Win32_NetworkAdapterConfiguration, // 网络适配器设置
            Win32_Printer, // 打印机
            Win32_PrinterConfiguration, // 打印机设置
            Win32_PrintJob, // 打印机任务
            Win32_TCPIPPrinterPort, // 打印机端口
            Win32_POTSModem, // MODEM
            Win32_POTSModemToSerialPort, // MODEM 端口
            Win32_DesktopMonitor, // 显示器
            Win32_DisplayConfiguration, // 显卡
            Win32_DisplayControllerConfiguration, // 显卡设置
            Win32_VideoController, // 显卡细节。
            Win32_VideoSettings, // 显卡支持的显示模式。

            // 操作系统
            Win32_TimeZone, // 时区
            Win32_SystemDriver, // 驱动程序
            Win32_DiskPartition, // 磁盘分区
            Win32_LogicalDisk, // 逻辑磁盘
            Win32_LogicalDiskToPartition, // 逻辑磁盘所在分区及始末位置。
            Win32_LogicalMemoryConfiguration, // 逻辑内存配置
            Win32_PageFile, // 系统页文件信息
            Win32_PageFileSetting, // 页文件设置
            Win32_BootConfiguration, // 系统启动配置
            Win32_ComputerSystem, // 计算机信息简要
            Win32_OperatingSystem, // 操作系统信息
            Win32_StartupCommand, // 系统自动启动程序
            Win32_Service, // 系统安装的服务
            Win32_Group, // 系统管理组
            Win32_GroupUser, // 系统组帐号
            Win32_UserAccount, // 用户帐号
            Win32_Process, // 系统进程
            Win32_Thread, // 系统线程
            Win32_Share, // 共享
            Win32_NetworkClient, // 已安装的网络客户端
            Win32_NetworkProtocol, // 已安装的网络协议
            Win32_PnPEntity,//all device
        }

        #endregion

        #region Force Sensor Data Display

        public bool ConnectForceSensor()
        {
            int iComNum = GetComNum();
            if (iComNum > 0)//有且只有一个符合的COM口
            {
                serialPort = new SerialPort();

                serialPort.PortName = "COM" + iComNum;
                serialPort.BaudRate = 921600;
                serialPort.Parity = Parity.None;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;

                for (int i = 0; i < cmbComport.Items.Count; i++)
                {
                    if (cmbComport.Items[i].ToString() == serialPort.PortName)
                    {
                        cmbComport.SelectedIndex = i;
                        break;
                    }
                }

                try
                {
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(Comm_DataReceived);
                    serialPort.Open();

                    bFirstReceived = true;
                }
                catch (Exception ex)
                {
                    return false;
                }

                return true;
            }
            else if (iComNum == 0)//有两个及以上符合的COM口
            {
                //if (strComport != "" && strComport != null)
                if (cmbComport.Text != "" && cmbComport.Text != null)
                {
                    serialPort = new SerialPort();

                    serialPort.PortName = cmbComport.Text;
                    serialPort.BaudRate = 921600;
                    serialPort.Parity = Parity.None;
                    serialPort.DataBits = 8;
                    serialPort.StopBits = StopBits.One;

                    try
                    {
                        serialPort.DataReceived += new SerialDataReceivedEventHandler(Comm_DataReceived);
                        serialPort.Open();

                        bFirstReceived = true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else//没有符合的COM口
            {
                cmbComport.SelectedIndex = -1;
                return false;
            }
        }

        public void CloseForceSensor()
        {
            if (serialPort.IsOpen)
            {
                try
                {
                    serialPort.Close();
                }
                catch (Exception ex)
                {

                }
            }
        }
        //原版先注释掉
        //连接到力传感器
        /*public bool ConnectForceSensor(bool bConnect = true)
        {
            bool bGotCom = false;
            for (int i = 0; i < cmbComport.Items.Count; i++)
            {
                serialPort = new SerialPort();

                serialPort.PortName = cmbComport.Items[i].ToString();
                serialPort.BaudRate = 921600;
                serialPort.Parity = Parity.None;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;

                try
                {
                    serialPort.Open();
                    serialPort.Write("read id\r\n");
                    Delay(10);

                    int count = serialPort.BytesToRead;
                    Byte[] InputBuf = new Byte[count];

                    serialPort.Read(InputBuf, 0, count);

                    //判断ID
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    string strInput = encoding.GetString(InputBuf);
                    if (strInput == strControllerID[3])
                    {
                        cmbComport.SelectedIndex = i;
                        iComportIndexLast = cmbComport.SelectedIndex;
                        bGotCom = true;
                        break;
                    }
                    else
                    {
                        serialPort.Close();
                    }
                }
                catch (Exception ex)
                { }
            }

            if (bGotCom)
            {
                try
                {
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(Comm_DataReceived);

                    serialPort.Write("5f\r\n");
                    Delay(100);
                    serialPort.Write("read data\r\n");
                    Delay(100);
                    btnZeroing_Click(null, null);
                }
                catch (Exception ex)
                {
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        */
        //SAM for test
        public bool ConnectForceSensor(bool bConnect = true)
        {
            //获取串口标号/个数
            int iComNum = GetComNum();

            //这些串口有没有符合CH340驱动的
            if (iComNum > 0)                //有且只有一个符合的COM口
            {   //此时iComNum就是串口ID标号
                serialPort = new SerialPort();
                serialPort.PortName = "COM" + iComNum;
                serialPort.BaudRate = 921600;
                serialPort.Parity = Parity.None;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;

                //下拉窗口有多少Item
                for (int i = 0; i < cmbComport.Items.Count; i++)
                {
                    if (cmbComport.Items[i].ToString() == serialPort.PortName)
                    {
                        cmbComport.SelectedIndex = i;
                        break;
                    }
                }

                try
                {
                    //开启串口接收中断
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(Comm_DataReceived);
                    //开启串口
                    serialPort.Open();

                    //是否是第一次进入
                    bFirstReceived = true;
                }
                catch (Exception ex)
                {
                    return false;
                }

                return true;
            }
            else if (iComNum == 0)  //有两个及以上符合的COM口，那就直接将串口号等于当前选择的串口号
            {
                //if (strComport != "" && strComport != null)
                if (cmbComport.Text != "" && cmbComport.Text != null)
                {
                    serialPort = new SerialPort();
                    serialPort.PortName = cmbComport.Text;
                    serialPort.BaudRate = 921600;
                    serialPort.Parity = Parity.None;
                    serialPort.DataBits = 8;
                    serialPort.StopBits = StopBits.One;

                    try
                    {
                        serialPort.DataReceived += new SerialDataReceivedEventHandler(Comm_DataReceived);
                        serialPort.Open();

                        bFirstReceived = true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else     //没有符合的COM口
            {
                cmbComport.SelectedIndex = -1;
                return false;
            }
        }

        public void Comm_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (bFirstReceived)
            {
                bFirstReceived = false;
                return;
            }

            try
            {
                int count = serialPort.BytesToRead;
                Byte[] InputBuf = new Byte[count];
                serialPort.Read(InputBuf, 0, count);

                ////TransmitBuf = InputBuf;
                ////iGetNum++;
                //DispUI(InputBuf);

                //1.缓存数据 
                buffer.AddRange(InputBuf);
                DispUI(buffer);

                if (bIndentation && bHighFreq && bShowCurve)
                {
                    if (bExtend)
                    {
                        strHighFreqListExtend.Add(new ASCIIEncoding().GetString(InputBuf));
                    }
                    else
                    {
                        strHighFreqListWithdraw.Add(new ASCIIEncoding().GetString(InputBuf));
                    }
                }
            }
            catch (Exception ex)         //超时处理
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void DispUI(List<byte> bufferList)
        {
            //2.完整性判断
            while (bufferList.Count >= 10)
            {
                if (bufferList[8] == 0x0D && bufferList[9] == 0x0A)
                {
                    Byte[] ReceiveBytes = new Byte[8];
                    bufferList.CopyTo(0, ReceiveBytes, 0, 8);

                    ASCIIEncoding encoding = new ASCIIEncoding();
                    string strInput = encoding.GetString(ReceiveBytes);
                    string strAdd = strInput.Substring(0, 6);

                    if (String.Equals(strInput[7].ToString(), strSuffix_F, StringComparison.CurrentCultureIgnoreCase))//忽略大小写
                    {
                        try
                        {
                            double dRead = Convert.ToDouble(Convert.ToInt64(strAdd, 16));

                            while (bInit_F)
                            {
                                dInitValue_F = dRead;
                                bInit_F = false;
                            }

                            double dChange = Convert.ToDouble(dRead) - dInitValue_F;
                            dForce = Convert.ToDouble((dChange * dSensitivity_F * Math.Pow(10, 6)).ToString("0.0000")) * iDirection_F;

                            bGetPoint = true;
                            iPointCount_F++;

                            if (listCur_F.Count > iMaxDurationTime * iNumPerSec)
                            {
                                //listCur_F.RemoveAt(0);
                                listCur_F.RemoveRange(0, listCur_F.Count - iMaxDurationTime * iNumPerSec);
                            }

                            double x = Convert.ToDouble((iPointCount_F * 1.0 / iNumPerSec).ToString("f4"));
                            double y = dForce;
                            listCur_F.Add(x, y);
                            //zgcForceSensor_F.GraphPane.CurveList.Clear();
                            //zgcForceSensor_F.GraphPane.AddCurve("", listCur_F, Color.Blue, SymbolType.None);
                            //zgcForceSensor_F.AxisChange();//画到zedGraphControl1控件中
                            //zgcForceSensor_F.Refresh();//重新刷新

                            if (bRecord)
                            {
                                listRecord_F.Add(x, y);
                            }

                            bufferList.RemoveRange(0, 10);
                        }
                        catch (Exception ex)
                        {
                            bufferList.RemoveRange(0, 10);
                            //MessageBox.Show("Data conversion error:" + strAdd, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else if (String.Equals(strInput[7].ToString(), strSuffix_d, StringComparison.CurrentCultureIgnoreCase))//忽略大小写
                    {
                        try
                        {
                            dReadDAC = Convert.ToDouble(Convert.ToInt64(strAdd, 16));
                            while (bInit_d)
                            {
                                dInitValue_d = dReadDAC;
                                dSetDAC = dReadDAC;
                                bInit_d = false;
                            }

                            double dChange = Convert.ToDouble(dReadDAC) - dInitValue_d;
                            dDisplacement_um_ori = Convert.ToDouble((dChange * dSensitivity_d * Math.Pow(10, 3)).ToString("f6")) * iDirection_d;//um
                            //dDisplacement_um_Indent = Convert.ToDouble((dChange * dSensitivity_d * Math.Pow(10, 3) - Math.Abs(dForce / dStiffness)).ToString("f4")) * iDirection_d;//um
                            dDisplacement_um_Indent = Convert.ToDouble((dChange * dSensitivity_d * Math.Pow(10, 3) * iDirection_d - dForce / dStiffness).ToString("f6"));//um
                            dDisplacement_nm_ori = dDisplacement_um_ori * 1000.0;//nm
                            dDisplacement_nm_Indent = dDisplacement_um_Indent * 1000.0;//nm

                            bGetPoint = false;
                            iPointCount_d++;

                            if (listCur_d.Count > iMaxDurationTime * iNumPerSec)
                            {
                                listCur_d.RemoveRange(0, listCur_d.Count - iMaxDurationTime * iNumPerSec);
                            }

                            double x = iPointCount_d * 1.0 / iNumPerSec;
                            double y = dDisplacement_nm_ori;
                            listCur_d.Add(x, y);
                            //zgcForceSensor_d.GraphPane.CurveList.Clear();
                            //zgcForceSensor_d.GraphPane.AddCurve("", listCur_d, Color.Blue, SymbolType.None);
                            //zgcForceSensor_d.AxisChange();//画到zedGraphControl1控件中
                            //zgcForceSensor_d.Refresh();//重新刷新

                            if (bRecord)
                            {
                                listRecord_d.Add(x, y);
                            }

                            bufferList.RemoveRange(0, 10);
                        }
                        catch (Exception ex)
                        {
                            bufferList.RemoveRange(0, 10);
                            //MessageBox.Show("Data conversion error:" + strAdd, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        bufferList.RemoveRange(0, 10);
                    }

                    if (bIndentation && !bHighFreq && bShowCurve && bGetPoint)
                    {
                        double x1 = (double)((decimal)dDisplacement_nm_ori - (decimal)dStartDistance_ori);
                        double x2 = (double)((decimal)dDisplacement_nm_Indent - (decimal)dStartDistance_Indent);
                        double y = dForce;

                        listIndentNoMissing_ori.Add(x1, y);
                        listIndentNoMissing.Add(x2, y);
                    }
                }
                else
                {
                    bufferList.RemoveAt(0);
                }
            }
        }

        public void DispUI(byte[] InputBuf)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            string strInput = encoding.GetString(InputBuf);

            string[] strSplit = strInput.Replace(" ", "").Replace("\r\n", " ").Trim().Split(' ');
            //string[] strSplit = Regex.Split(strInput, "0b\r\n", RegexOptions.IgnoreCase);
            for (int m = 0; m < strSplit.Length; m++)
            {
                if (strSplit[m].Length == 8)
                {
                    string strAdd = strSplit[m].Substring(0, strSplit[m].Length - 2);

                    //if (!bIndentation || (bIndentation && !bHighFreq) || (bIndentation && bHighFreq && (cmbIndentType.SelectedIndex == 1)))
                    {
                        if (String.Equals(strSplit[m][strSplit[m].Length - 1].ToString(), strSuffix_F, StringComparison.CurrentCultureIgnoreCase))//忽略大小写
                        {
                            try
                            {
                                double dRead = Convert.ToDouble(Convert.ToInt64(strAdd, 16));
                                while (bInit_F)
                                {
                                    dInitValue_F = dRead;
                                    bInit_F = false;
                                }

                                double dChange = Convert.ToDouble(dRead) - dInitValue_F;
                                dForce = Convert.ToDouble((dChange * dSensitivity_F * Math.Pow(10, 6)).ToString("f4")) * iDirection_F;//uN

                                bGetPoint = true;
                                iPointCount_F++;

                                if (listCur_F.Count > iMaxDurationTime * iNumPerSec)
                                {
                                    listCur_F.RemoveRange(0, listCur_F.Count - iMaxDurationTime * iNumPerSec);
                                }

                                double x = iPointCount_F * 1.0 / iNumPerSec;
                                double y = dForce;
                                listCur_F.Add(x, y);
                                zgcForceSensor_F.GraphPane.CurveList.Clear();
                                zgcForceSensor_F.GraphPane.AddCurve("", listCur_F, Color.Blue, SymbolType.None);
                                zgcForceSensor_F.AxisChange();//画到zedGraphControl1控件中
                                zgcForceSensor_F.Refresh();//重新刷新

                                if (bRecord)
                                {
                                    listRecord_F.Add(x, y);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (!bCN)
                                {
                                    MessageBox.Show("Data conversion error:" + strAdd, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    MessageBox.Show("数据转换错误：" + strAdd, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }

                    //if (!bIndentation || (bIndentation && !bHighFreq) || (bIndentation && bHighFreq && (cmbIndentType.SelectedIndex == 0)))
                    {
                        if (String.Equals(strSplit[m][strSplit[m].Length - 1].ToString(), strSuffix_d, StringComparison.CurrentCultureIgnoreCase))//忽略大小写
                        {
                            try
                            {
                                dReadDAC = Convert.ToDouble(Convert.ToInt64(strAdd, 16));
                                while (bInit_d)
                                {
                                    dInitValue_d = dReadDAC;
                                    dSetDAC = dReadDAC;
                                    bInit_d = false;
                                }

                                double dChange = Convert.ToDouble(dReadDAC) - dInitValue_d;
                                dDisplacement_um_ori = Convert.ToDouble((dChange * dSensitivity_d * Math.Pow(10, 3)).ToString("f6")) * iDirection_d;//um
                                //dDisplacement_um_Indent = Convert.ToDouble((dChange * dSensitivity_d * Math.Pow(10, 3) - Math.Abs(dForce / dStiffness)).ToString("f4")) * iDirection_d;//um
                                dDisplacement_um_Indent = Convert.ToDouble((dChange * dSensitivity_d * Math.Pow(10, 3) * iDirection_d - dForce / dStiffness).ToString("f6"));//um
                                dDisplacement_nm_ori = dDisplacement_um_ori * 1000.0;//nm
                                dDisplacement_nm_Indent = dDisplacement_um_Indent * 1000.0;//nm

                                bGetPoint = false;
                                iPointCount_d++;

                                if (listCur_d.Count > iMaxDurationTime * iNumPerSec)
                                {
                                    listCur_d.RemoveRange(0, listCur_d.Count - iMaxDurationTime * iNumPerSec);
                                }

                                double x = iPointCount_d * 1.0 / iNumPerSec;
                                double y = dDisplacement_nm_ori;
                                listCur_d.Add(x, y);
                                zgcForceSensor_d.GraphPane.CurveList.Clear();
                                zgcForceSensor_d.GraphPane.AddCurve("", listCur_d, Color.Blue, SymbolType.None);
                                zgcForceSensor_d.AxisChange();//画到zedGraphControl1控件中
                                zgcForceSensor_d.Refresh();//重新刷新

                                if (bRecord)
                                {
                                    listRecord_d.Add(x, y);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (!bCN)
                                {
                                    MessageBox.Show("Data conversion error:" + strAdd, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    MessageBox.Show("数据转换错误：" + strAdd, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }

                    if (bIndentation && !bHighFreq && bShowCurve && bGetPoint)
                    {
                        double x1 = (double)((decimal)dDisplacement_nm_ori - (decimal)dStartDistance_ori);
                        double x2 = (double)((decimal)dDisplacement_nm_Indent - (decimal)dStartDistance_Indent);
                        double y = dForce;

                        listIndentNoMissing_ori.Add(x1, y);
                        listIndentNoMissing.Add(x2, y);
                    }
                }
            }
        }

        #endregion

        #region Joystick Functions

        private bool InitializeSiApp()
        {
            res = SiApp.SiInitialize();
            if (res != SiApp.SpwRetVal.SPW_NO_ERROR)
            {
                //MessageBox.Show("Initialize function failed");
                return false;
            }
            Log("SiInitialize", res.ToString());

            SiApp.SiOpenData openData = new SiApp.SiOpenData();
            SiApp.SiOpenWinInit(openData, hWnd);
            if (openData.hWnd == IntPtr.Zero)
            {
                if (!bCN)
                {
                    MessageBox.Show("Handle is empty");
                }
                else
                {
                    MessageBox.Show("句柄为空");
                }
            }
            Log("SiOpenWinInit", openData.hWnd + "(window handle)");

            devHdl = SiApp.SiOpen(appName, SiApp.SI_ANY_DEVICE, IntPtr.Zero, SiApp.SI_EVENT, openData);
            if (devHdl == IntPtr.Zero)
            {
                if (!bCN)
                {
                    MessageBox.Show("Open returns empty device handle");
                }
                else
                {
                    MessageBox.Show("打开返回空的设备句柄");
                }
            }
            Log("SiOpen", devHdl + "(device handle)");
            return (devHdl != IntPtr.Zero);
        }

        private void CloseSiApp()
        {
            if (devHdl != IntPtr.Zero)
            {
                SiApp.SpwRetVal res = SiApp.SiClose(devHdl);
                Log("SiClose", res.ToString());
                int r = SiApp.SiTerminate();
                Log("SiTerminate", r.ToString());
            }
        }

        private void TrackMouseEvents(Message msg)
        {
            SiApp.SiGetDeviceName(devHdl, devName);

            if (!SiApp.IsSpaceMouseMessage(msg.Msg))
                return;

            SiApp.SiGetEventData eventData = new SiApp.SiGetEventData();
            SiApp.SiGetEventWinInit(eventData, msg.Msg, msg.WParam, msg.LParam);

            SiApp.SiSpwEvent spwEvent = new SiApp.SiSpwEvent();
            SiApp.SpwRetVal val = SiApp.SiGetEvent(devHdl, SiApp.SI_AVERAGE_EVENTS, eventData, spwEvent);

            if (val == SiApp.SpwRetVal.SI_IS_EVENT)
            {
                Log("SiGetEventWinInit", eventData.msg.ToString());

                switch (spwEvent.type)
                {
                    case 0:
                        break;

                    case SiApp.SiEventType.SI_BUTTON_EVENT:
                        int iReleased = (int)(spwEvent.spwData.bData.released);
                        if (iReleased != 0 && GetFlag(iReleased))
                        {
                            iBtnNum = log2(iReleased);
                            JoystickButtonReleasedEvent(iBtnNum);
                            //Print("Button event Released: " + iBtnNum);
                            Log("Button event Released: ", iBtnNum.ToString());
                        }

                        break;

                    case SiApp.SiEventType.SI_MOTION_EVENT:
                        string motionData = string.Format(templateTR, "",
                                            spwEvent.spwData.mData[0], spwEvent.spwData.mData[1], spwEvent.spwData.mData[2], // TX, TY, TZ
                                            spwEvent.spwData.mData[3], spwEvent.spwData.mData[4], spwEvent.spwData.mData[5], // RX, RY, RZ
                                            spwEvent.spwData.period); // Period (normally 16 ms)

                        iXData = spwEvent.spwData.mData[5];
                        iYData = spwEvent.spwData.mData[3];
                        iZData = spwEvent.spwData.mData[1];

                        //Print("Motion event " + motionData);
                        Log("SI_APP_EVENT: ", "Motion event " + motionData);
                        break;

                    case SiApp.SiEventType.SI_ZERO_EVENT:
                        Log("SI_APP_EVENT: ", "Zero event");
                        break;

                    case SiApp.SiEventType.SI_CMD_EVENT:
                        Log("SI_APP_EVENT: ", string.Format("V3DCMD = {0}, pressed = {1}", spwEvent.cmdEventData.functionNumber, spwEvent.cmdEventData.pressed > 0));
                        break;

                    case SiApp.SiEventType.SI_APP_EVENT:
                        Log("SI_APP_EVENT: ", string.Format("appCmdID = \"{0}\", pressed = {1}", spwEvent.appCommandData.id.appCmdID, spwEvent.appCommandData.pressed > 0));
                        break;

                    default:
                        break;
                }

                Log("SiGetEvent", string.Format("{0}({1})", spwEvent.type, val));
            }
        }

        /// <summary>
        /// Method to export the application commands to 3dxware so that they can be assigned to
        /// 3Dconnexion device buttons.
        /// 将应用程序命令导出到3dxware，以便将它们分配给3Dconnexion设备按钮。
        /// </summary>
        private void ExportApplicationCommands()
        {
            string imagesPath = string.Empty;
            string resDllPath = string.Empty;
            string dllName = "3DxService.dll";

            string homePath = Get3DxWareHomeDirectory();

            if (!string.IsNullOrEmpty(homePath))
            {
                imagesPath = Path.Combine(homePath, @"Cfg\Images\3DxService\{0}");
                resDllPath = Path.Combine(homePath, @"en-US\" + dllName);
            }

            using (ActionCache cache = new ActionCache())
            {
                //An action set can also be considered to be a buttonbank, a menubar, or a set of toolbars
                //动作集也可以被认为是一个按钮库、菜单条或一组工具栏。
                ActionNode buttonBank = cache.Add(new ActionSet("ACTION_SET_ID", "Custom action set"));

                //Add a couple of categories / menus / tabs to the buttonbank/menubar/toolbar
                //向按钮库/菜单栏/工具栏添加几个类别/菜单/选项卡
                ActionNode fileNode = buttonBank.Add(new Category("CAT_ID_FILE", "File"));
                ActionNode editNode = buttonBank.Add(new Category("CAT_ID_EDIT", "Edit"));

                // Add menu items to the menus. When the button on the 3D mouse is pressed the id will be sent to the application
                // in the SI_APP_EVENT event structure in the SiAppCmdID.appCmdID field

                // Export a menu item / action using a bitmap from an external dll resource
                fileNode.Add(new NMT.Joystick.Action("ID_ABOUT", "About", "Info about the program", ImageItem.FromResource(resDllPath, "#172", "#2")));

                // Add menu items / actions using bitmaps located on the harddrive
                editNode.Add(new NMT.Joystick.Action("ID_CUT", "Cut", "Shortcut is Ctrl + X", ImageItem.FromFile(string.Format(imagesPath, "Macro_Cut.png"))));
                editNode.Add(new NMT.Joystick.Action("ID_COPY", "Copy", "Shortcut is Ctrl + C", ImageItem.FromFile(string.Format(imagesPath, "Macro_Copy.png"))));
                editNode.Add(new NMT.Joystick.Action("ID_PASTE", "Paste", "Shortcut is Ctrl + V", ImageItem.FromFile(string.Format(imagesPath, "Macro_Paste.png"))));

                // Add a menu item without an image associated with it
                editNode.Add(new NMT.Joystick.Action("ID_UNDO", "Undo", "Shortcut is Ctrl + Z"));

                // Now add an image and associate it with the menu item ID_UNDO by using the same id as the menu item / action
                cache.Add(ImageItem.FromFile(string.Format(imagesPath, "Macro_Undo.png"), 0, @"ID_UNDO"));
                try
                {
                    // Write the complete cache to the driver
                    cache.SaveTo3Dxware(devHdl);
                }
                catch (Exception e)
                {
                    Log("cache.SaveTo3Dxware", e.Message);
                }
            }
        }

        private string Get3DxWareHomeDirectory()
        {
            string softwareKeyName = string.Empty;
            string homeDirectory = string.Empty;

            if (IntPtr.Size == 8)
            {
                softwareKeyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\3Dconnexion\3DxWare";
            }
            else
            {
                softwareKeyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\3Dconnexion\3DxWare";
            }

            object regValue = Microsoft.Win32.Registry.GetValue(softwareKeyName, "Home Directory", null);
            if (regValue != null)
            {
                homeDirectory = regValue.ToString();
            }

            return homeDirectory;
        }

        private T PtrToStructure<T>(IntPtr ptr) where T : struct
        {
            return (T)Marshal.PtrToStructure(ptr, typeof(T));
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message msg)
        {
            TrackMouseEvents(msg);

            switch (msg.Msg)                                  //判断消息类型
            {
                case WM_DEVICE_CHANGE:                      //设备改变消息
                    {
                        GetComList();                    //枚举串口
                    }
                    break;
            }

            base.WndProc(ref msg);
        }

        public void JoystickButtonReleasedEvent(int buttonNumber)
        {
            switch (buttonNumber)
            {
                case 1://Menu
                    bEnableX = !bEnableX;
                    label_Axis_X.Enabled = label_Encoder_X.Enabled = bEnableX;
                    break;

                case 2://Fit
                    bEnableY = !bEnableY;
                    label_Axis_Y.Enabled = label_Encoder_Y.Enabled = bEnableY;
                    break;

                case 3://T
                    if (bContinuous)
                    {
                        btnContinuousUp_Click(null, null);
                    }

                    if (bStep)
                    {
                        btnStepUp_Click(null, null);
                    }
                    break;

                case 5://R
                    if (bContinuous)
                    {
                        btnContinuousDown_Click(null, null);
                    }

                    if (bStep)
                    {
                        btnStepDown_Click(null, null);
                    }
                    break;

                case 6://F
                    break;

                case 9://Roll+
                    break;

                case 13://1
                    break;

                case 14://2
                    break;

                case 15://3
                    break;

                case 16://4
                    break;

                case 23://Esc
                    bContinuous = !bContinuous;
                    bStep = !bStep;
                    btnContinuous.BackColor = bContinuous ? Color.DodgerBlue : Color.Transparent;
                    btnStep.BackColor = bStep ? Color.DodgerBlue : Color.Transparent;
                    btnXN.Enabled = btnXP.Enabled = bStep;
                    btnYN.Enabled = btnYP.Enabled = bStep;
                    btnZN.Enabled = btnZP.Enabled = bStep;
                    break;

                case 24://Alt
                    break;

                case 25://Shift
                    break;

                case 26://Ctrl
                    break;

                case 27://Rot
                    bEnableZ = !bEnableZ;
                    label_Axis_Z.Enabled = label_Encoder_Z.Enabled = bEnableZ;
                    break;

                default:
                    break;
            }
        }

        #endregion

        #region State Changed Events

        private void cmbComport_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (iComportIndexLast != cmbComport.SelectedIndex)
            {
                if (cmbComport.Text != "" && cmbComport.Text != null)
                {
                    try
                    {
                        if (serialPort != null)
                        {
                            serialPort.Close();

                            zgcForceSensor_F.GraphPane.CurveList.Clear();
                            zgcForceSensor_F.AxisChange();//画到zedGraphControl1控件中
                            zgcForceSensor_F.Refresh();//重新刷新

                            zgcForceSensor_d.GraphPane.CurveList.Clear();
                            zgcForceSensor_d.AxisChange();//画到zedGraphControl1控件中
                            zgcForceSensor_d.Refresh();//重新刷新

                            //btnZeroing_Click(null, null);
                        }
                    }
                    catch (Exception ex)
                    { }
                }
            }

            iComportIndexLast = cmbComport.SelectedIndex;
        }

        private void cmbSensorVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbSensorVersion.SelectedIndex == 0)
                {
                    txbResolution.Text = strSensorDefaultValue[2];
                    txbMeasureRange.Text = strSensorDefaultValue[3];
                    txbSensitivity_F.Text = strSensorDefaultValue[4];
                    txbSensitivity_d.Text = strSensorDefaultValue[5];
                    txbStiffness.Text = strSensorDefaultValue[6];
                    txbExtraStiffness.Text = strSensorDefaultValue[7];
                    txbSensorForceThreshold.Text = strSensorDefaultValue[8];
                    ckbSafetyMode.Checked = Convert.ToBoolean(int.Parse(strSensorDefaultValue[9]));
                }
                else
                {
                    txbResolution.Text = strSensorDefaultValue[10];
                    txbMeasureRange.Text = strSensorDefaultValue[11];
                    txbSensitivity_F.Text = strSensorDefaultValue[12];
                    txbSensitivity_d.Text = strSensorDefaultValue[13];
                    txbStiffness.Text = strSensorDefaultValue[14];
                    txbExtraStiffness.Text = strSensorDefaultValue[15];
                    txbSensorForceThreshold.Text = strSensorDefaultValue[16];
                    ckbSafetyMode.Checked = Convert.ToBoolean(int.Parse(strSensorDefaultValue[17]));
                }

                dSensitivity_F = Convert.ToDouble(txbSensitivity_F.Text);
                dSensitivity_d = Convert.ToDouble(txbSensitivity_d.Text);
                dStiffness = Convert.ToDouble(txbStiffness.Text);
                dExtraStiffness = Convert.ToDouble(txbExtraStiffness.Text);
                dSensorForceThreshold = txbSensorForceThreshold.Text == "" ? 0.0 : Convert.ToDouble(txbSensorForceThreshold.Text);

                iPointCount_F = 0;
                iPointCount_d = 0;
                bInit_F = true;
                bInit_d = true;

                listRecord_F = new PointPairList();//数据点
                listRecord_d = new PointPairList();
                listCur_F = new PointPairList();//数据点
                listCur_d = new PointPairList();

                strSensitivityLast_F = txbSensitivity_F.Text;
                strSensitivityLast_d = txbSensitivity_d.Text;
                strStiffnessLast = txbStiffness.Text;
                strSensorForceThresholdLast = txbSensorForceThreshold.Text;
                bCkbSafetyModeCheckedChangedLast = ckbSafetyMode.Checked;
            }
            catch (Exception ex)
            { }
        }

        private void ckbSafetyMode_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbSafetyMode.Checked)
            {
                txbSensorForceThreshold.Enabled = true;
            }
            else
            {
                txbSensorForceThreshold.Enabled = false;
            }
        }

        private void ckbAutoScale_F_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbAutoScale_F.Checked)
            {
                txbMinForce_F.Enabled = false;
                txbMaxForce_F.Enabled = false;
            }
            else
            {
                txbMinForce_F.Enabled = true;
                txbMaxForce_F.Enabled = true;
            }
        }

        private void ckbAutoScale_d_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbAutoScale_d.Checked)
            {
                txbMinForce_d.Enabled = false;
                txbMaxForce_d.Enabled = false;
            }
            else
            {
                txbMinForce_d.Enabled = true;
                txbMaxForce_d.Enabled = true;
            }
        }

        private void ckbLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbLeft.Checked)
            {
                bLeft = true;
                iXP = int.Parse(strAxisDirection[0]);
                iYP = int.Parse(strAxisDirection[2]);
                iZP = int.Parse(strAxisDirection[4]);
                iXN = int.Parse(strAxisDirection[0]) * (-1);
                iYN = int.Parse(strAxisDirection[2]) * (-1);
                iZN = int.Parse(strAxisDirection[4]) * (-1);
                iClockwise = 1;
                iAnticlockwise = -1;
            }
            else
            {
                bLeft = false;
                iXP = int.Parse(strAxisDirection[1]);
                iYP = int.Parse(strAxisDirection[3]);
                iZP = int.Parse(strAxisDirection[5]);
                iXN = int.Parse(strAxisDirection[1]) * (-1);
                iYN = int.Parse(strAxisDirection[3]) * (-1);
                iZN = int.Parse(strAxisDirection[5]) * (-1);
                iClockwise = -1;
                iAnticlockwise = 1;
            }
        }

        //自动接近速度纪荣
        private void cmbApproachSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbApproachSpeed.SelectedIndex == 0)        //50nm/s
            {
                dApproachStep = 5;
                iApproachDelay = 100;
            }
            else if (cmbApproachSpeed.SelectedIndex == 1)   //200nm/s
            {
                dApproachStep = 10;
                iApproachDelay = 50;
            }
            else if (cmbApproachSpeed.SelectedIndex == 2)   //660nm/s
            {
                dApproachStep = 20;
                iApproachDelay = 30;
            }
            else
            {
                dApproachStep = 40;                         //2000nm/s
                iApproachDelay = 20;
            }
        }

        private void cmbIndentSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbIndentSpeed.SelectedIndex == 0)//1nm/s
            {
                uiIndentSpeed = 4;
                iIndentDelay = 180;
                dIndentApproximateSpeed_nm = 1;
            }
            else if (cmbIndentSpeed.SelectedIndex == 1)//5nm/s
            {
                uiIndentSpeed = 12;
                iIndentDelay = 100;
                dIndentApproximateSpeed_nm = 5;
            }
            else if (cmbIndentSpeed.SelectedIndex == 2)//10nm/s
            {
                uiIndentSpeed = 23;
                iIndentDelay = 100;
                dIndentApproximateSpeed_nm = 10;
            }
            else if (cmbIndentSpeed.SelectedIndex == 3)//20nm/s
            {
                uiIndentSpeed = 47;
                iIndentDelay = 90;
                dIndentApproximateSpeed_nm = 20;
            }
            else if (cmbIndentSpeed.SelectedIndex == 4)//50nm/s
            {
                uiIndentSpeed = 60;
                iIndentDelay = 40;
                dIndentApproximateSpeed_nm = 50;
            }
            else if (cmbIndentSpeed.SelectedIndex == 5)//100nm/s
            {
                uiIndentSpeed = 71;
                iIndentDelay = 30;
                dIndentApproximateSpeed_nm = 100;
            }
            else if (cmbIndentSpeed.SelectedIndex == 6)//150nm/s
            {
                uiIndentSpeed = 106;
                iIndentDelay = 20;
                dIndentApproximateSpeed_nm = 150;
            }
            else /*if (cmbIndentSpeed.SelectedIndex == 7)//300nm/s*/
            {
                uiIndentSpeed = 110;
                iIndentDelay = 10;
                dIndentApproximateSpeed_nm = 300;
            }
            //else//500nm/s
            //{
            //    uiIndentSpeed = 170;
            //    iIndentDelay = 5;
            //    dIndentApproximateSpeed_nm = 500;
            //}
        }

        private void cmbIndentType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbIndentType.SelectedIndex == 0)
            {
                txbIndentDeformation.Enabled = true;
                txbIndentDistance.Enabled = false;
                txbIndentForceThreshold.Enabled = false;
                MessageBox.Show("\"开始压痕\"前请确保压头尖端与样品刚好接触。", "注意", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (cmbIndentType.SelectedIndex == 1)
            {
                txbIndentDeformation.Enabled = false;
                txbIndentDistance.Enabled = true;
                txbIndentForceThreshold.Enabled = false;
            }

            if (cmbIndentType.SelectedIndex == 2)
            {
                txbIndentDeformation.Enabled = false;
                txbIndentDistance.Enabled = false;
                txbIndentForceThreshold.Enabled = true;
            }
        }

        private void ckbIndentHighFreq_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbIndentHighFreq.Checked)
            {
                bHighFreq = true;
            }
            else
            {
                bHighFreq = false;
            }
        }

        private void cmbCompressSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCompressSpeed.SelectedIndex == 0)//1nm/s
            {
                uiIndentSpeed = 4;
                iIndentDelay = 180;
                dIndentApproximateSpeed_nm = 1;
            }
            else if (cmbCompressSpeed.SelectedIndex == 1)//5nm/s
            {
                uiIndentSpeed = 12;
                iIndentDelay = 100;
                dIndentApproximateSpeed_nm = 5;
            }
            else if (cmbCompressSpeed.SelectedIndex == 2)//10nm/s
            {
                uiIndentSpeed = 23;
                iIndentDelay = 100;
                dIndentApproximateSpeed_nm = 10;
            }
            else if (cmbCompressSpeed.SelectedIndex == 3)//20nm/s
            {
                uiIndentSpeed = 47;
                iIndentDelay = 90;
                dIndentApproximateSpeed_nm = 20;
            }
            else if (cmbCompressSpeed.SelectedIndex == 4)//50nm/s
            {
                uiIndentSpeed = 60;
                iIndentDelay = 40;
                dIndentApproximateSpeed_nm = 50;
            }
            else if (cmbCompressSpeed.SelectedIndex == 5)//100nm/s
            {
                uiIndentSpeed = 71;
                iIndentDelay = 30;
                dIndentApproximateSpeed_nm = 100;
            }
            else if (cmbCompressSpeed.SelectedIndex == 6)//150nm/s
            {
                uiIndentSpeed = 106;
                iIndentDelay = 20;
                dIndentApproximateSpeed_nm = 150;
            }
            else /*if (cmbCompressSpeed.SelectedIndex == 7)//300nm/s*/
            {
                uiIndentSpeed = 110;
                iIndentDelay = 10;
                dIndentApproximateSpeed_nm = 300;
            }
            //else//500nm/s
            //{
            //    uiIndentSpeed = 170;
            //    iIndentDelay = 5;
            //    dIndentApproximateSpeed_nm = 500;
            //}
        }

        private void cmbCompressType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCompressType.SelectedIndex == 0)
            {
                txbCompressDeformation.Enabled = true;
                txbCompressDistance.Enabled = false;
                txbCompressForceThreshold.Enabled = false;
                MessageBox.Show("\"开始压痕\"前请确保压头尖端与样品刚好接触。", "注意", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (cmbCompressType.SelectedIndex == 1)
            {
                txbCompressDeformation.Enabled = false;
                txbCompressDistance.Enabled = true;
                txbCompressForceThreshold.Enabled = false;
            }

            if (cmbCompressType.SelectedIndex == 2)
            {
                txbCompressDeformation.Enabled = false;
                txbCompressDistance.Enabled = false;
                txbCompressForceThreshold.Enabled = true;
                txbCompressWithdrawDisp.Enabled = false;
                txbCompressWithdrawDelay.Enabled = false;
            }

        }

        private void ckbCompressHighFreq_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbIndentHighFreq.Checked)
            {
                bHighFreq = true;
            }
            else
            {
                bHighFreq = false;
            }
        }

        private void cmbTensionSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cmbTensionType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ckbTensionHighFreq_CheckedChanged(object sender, EventArgs e)
        {

        }

        #endregion

        #region Button Events

        private void btnSensorApply_Click(object sender, EventArgs e)
        {
            if (strSensitivityLast_F != txbSensitivity_F.Text)
            {
                if (txbSensitivity_F.Text != "" && txbSensitivity_F.Text != null)
                {
                    dSensitivity_F = Convert.ToDouble(txbSensitivity_F.Text);

                    iPointCount_F = 0;
                    bInit_F = true;
                    listRecord_F = new PointPairList();//数据点
                    listCur_F = new PointPairList();//数据点

                    strSensitivityLast_F = txbSensitivity_F.Text;
                }
            }

            if (strSensitivityLast_d != txbSensitivity_d.Text)
            {
                if (txbSensitivity_d.Text != "" && txbSensitivity_d.Text != null)
                {
                    dSensitivity_d = Convert.ToDouble(txbSensitivity_d.Text);

                    iPointCount_d = 0;
                    bInit_d = true;
                    listRecord_d = new PointPairList();//数据点
                    listCur_d = new PointPairList();//数据点

                    strSensitivityLast_d = txbSensitivity_d.Text;
                }
            }

            if (strStiffnessLast != txbStiffness.Text)
            {
                if (txbStiffness.Text != "" && txbStiffness.Text != null)
                {
                    dStiffness = Convert.ToDouble(txbStiffness.Text);

                    iPointCount_d = 0;
                    bInit_d = true;
                    listRecord_d = new PointPairList();//数据点
                    listCur_d = new PointPairList();//数据点

                    strStiffnessLast = txbStiffness.Text;
                }
            }

            if (strSensorForceThresholdLast != txbSensorForceThreshold.Text || bCkbSafetyModeCheckedChangedLast != ckbSafetyMode.Checked)
            {
                if (ckbSafetyMode.Checked)
                {
                    if (txbSensorForceThreshold.Text != "" && txbSensorForceThreshold.Text != null)
                    {
                        dSensorForceThreshold = Convert.ToDouble(txbSensorForceThreshold.Text);
                        strSensorForceThresholdLast = txbSensorForceThreshold.Text;
                        bCkbSafetyModeCheckedChangedLast = ckbSafetyMode.Checked;
                    }
                }
            }

            UpdateSensorDefaultValue();
        }

        private void btnService_Click(object sender, EventArgs e)
        {
            //FrmPassword frmPassword = new FrmPassword();
            //if (frmPassword.ShowDialog() == DialogResult.OK)
            //{
            FrmService frmService = new FrmService();
            frmService.ShowDialog();
            //}
        }

        private void btnStiffnessCalibration_Click(object sender, EventArgs e)
        {
            FrmStiffnessCalibration frmStiffnessCalibration = new FrmStiffnessCalibration(this);
            frmStiffnessCalibration.ShowDialog();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (strMaxDurationLast != txbMaxDuration.Text)
            {
                if (txbMaxDuration.Text != "" && txbMaxDuration.Text != null)
                {
                    if (isNumberic(txbMaxDuration.Text))
                    {
                        iMaxDurationTime = int.Parse(txbMaxDuration.Text);
                    }
                    else
                    {
                        iMaxDurationTime = 5;
                    }

                    strMaxDurationLast = txbMaxDuration.Text;
                }
            }

            if (bCkbAutoScaleCheckedChangedLast_F != ckbAutoScale_F.Checked
                || strMinForceLast_F != txbMinForce_F.Text || strMaxForceLast_F != txbMaxForce_F.Text)
            {
                if (!ckbAutoScale_F.Checked)
                {
                    if (txbMinForce_F.Text != "" && txbMinForce_F.Text != null
                        && txbMaxForce_F.Text != "" && txbMaxForce_F.Text != null
                        && Convert.ToDouble(txbMinForce_F.Text) < Convert.ToDouble(txbMaxForce_F.Text))
                    {
                        zgcForceSensor_F.GraphPane.YAxis.Scale.Min = Convert.ToDouble(txbMinForce_F.Text);
                        zgcForceSensor_F.GraphPane.YAxis.Scale.Max = Convert.ToDouble(txbMaxForce_F.Text);
                        zgcForceSensor_F.AxisChange();//画到zedGraphControl1控件中
                        zgcForceSensor_F.Refresh();//重新刷新

                        bCkbAutoScaleCheckedChangedLast_F = ckbAutoScale_F.Checked;
                        strMinForceLast_F = txbMinForce_F.Text;
                        strMaxForceLast_F = txbMaxForce_F.Text;
                    }
                    else
                    {
                        if (!bCN)
                        {
                            MessageBox.Show("\'Min Force\' and \'Max Force\' can not be empty, and \'Min Force\' must be less than \'Max Force\' !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show("\'最小作用力\'和\'最大作用力\'不能为空，而且\'最小作用力\'一定要小于\'最大作用力\'!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else
                {
                    zgcForceSensor_F.GraphPane.YAxis.Scale.MaxAuto = true;
                    zgcForceSensor_F.GraphPane.YAxis.Scale.MinAuto = true;
                    zgcForceSensor_F.AxisChange();//画到zedGraphControl1控件中
                    zgcForceSensor_F.Refresh();//重新刷新

                    bCkbAutoScaleCheckedChangedLast_F = ckbAutoScale_F.Checked;
                    strMinForceLast_F = txbMinForce_F.Text;
                    strMaxForceLast_F = txbMaxForce_F.Text;
                }
            }

            if (bCkbAutoScaleCheckedChangedLast_d != ckbAutoScale_d.Checked
                || strMinForceLast_d != txbMinForce_d.Text || strMaxForceLast_d != txbMaxForce_d.Text)
            {
                if (!ckbAutoScale_d.Checked)
                {
                    if (txbMinForce_d.Text != "" && txbMinForce_d.Text != null
                        && txbMaxForce_d.Text != "" && txbMaxForce_d.Text != null
                        && Convert.ToDouble(txbMinForce_d.Text) < Convert.ToDouble(txbMaxForce_d.Text))
                    {
                        zgcForceSensor_d.GraphPane.YAxis.Scale.Min = Convert.ToDouble(txbMinForce_d.Text);
                        zgcForceSensor_d.GraphPane.YAxis.Scale.Max = Convert.ToDouble(txbMaxForce_d.Text);
                        zgcForceSensor_d.AxisChange();//画到zedGraphControl1控件中
                        zgcForceSensor_d.Refresh();//重新刷新

                        bCkbAutoScaleCheckedChangedLast_d = ckbAutoScale_d.Checked;
                        strMinForceLast_d = txbMinForce_d.Text;
                        strMaxForceLast_d = txbMaxForce_d.Text;
                    }
                    else
                    {
                        if (!bCN)
                        {
                            MessageBox.Show("\'Min Displ.\' and \'Max Displ.\' can not be empty, and \'Min Displ.\' must be less than \'Max Displ.\' !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show("\'最小位移\'和\'最大位移\'不能为空，而且\'最小位移\'一定要小于\'最大位移\'!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else
                {
                    zgcForceSensor_d.GraphPane.YAxis.Scale.MaxAuto = true;
                    zgcForceSensor_d.GraphPane.YAxis.Scale.MinAuto = true;
                    zgcForceSensor_d.AxisChange();//画到zedGraphControl1控件中
                    zgcForceSensor_d.Refresh();//重新刷新

                    bCkbAutoScaleCheckedChangedLast_d = ckbAutoScale_d.Checked;
                    strMinForceLast_d = txbMinForce_d.Text;
                    strMaxForceLast_d = txbMaxForce_d.Text;
                }
            }
        }

        public void btnZeroing_Click(object sender, EventArgs e)
        {
            if (!(bIndentation && bHighFreq))
            {
                bStabilization = false;

                iPointCount_F = 0;
                iPointCount_d = 0;
                bInit_F = true;
                bInit_d = true;

                listRecord_F = new PointPairList();//数据点
                listRecord_d = new PointPairList();
                listCur_F = new PointPairList();//数据点
                listCur_d = new PointPairList();

                zgcForceSensorInitialize();

                zgcForceSensor_F.GraphPane.CurveList.Clear();
                zgcForceSensor_F.AxisChange();  //画到zedGraphControl1控件中
                zgcForceSensor_F.Refresh();     //重新刷新

                zgcForceSensor_d.GraphPane.CurveList.Clear();
                zgcForceSensor_d.AxisChange();  //画到zedGraphControl1控件中
                zgcForceSensor_d.Refresh();     //重新刷新

                Delay(200);
                dSetDAC = dInitValue_d;
                bStabilization = true;
            }
        }

        private void btnRecordingData_Click(object sender, EventArgs e)
        {
            if (btnRecordingData.Text == "Start Recording Data" || btnRecordingData.Text == "开始记录数据")
            {
                if (!bCN)
                {
                    btnRecordingData.Text = "Stop Recording Data";
                }
                else
                {
                    btnRecordingData.Text = "停止记录数据";
                }
                AutoApproachEnabledControls(false);
                IndentationEnabledControls(false);
                btnRecordingData.Enabled = true;
                listRecord_F = new PointPairList();
                listRecord_d = new PointPairList();
                bRecord = true;
            }
            else
            {
                if (!bCN)
                {
                    btnRecordingData.Text = "Start Recording Data";
                }
                else
                {
                    btnRecordingData.Text = "开始记录数据";
                }
                AutoApproachEnabledControls(true);
                IndentationEnabledControls(true);
                bRecord = false;

                saveFileDialog1.Filter = "文本文件| *.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK && saveFileDialog1.FileName.Length > 0)
                {
                    string fileName1 = saveFileDialog1.FileName.Remove(saveFileDialog1.FileName.Length - 4) + "_F.txt";
                    string fileName2 = saveFileDialog1.FileName.Remove(saveFileDialog1.FileName.Length - 4) + "_d.txt";
                    WritePointsToTxt(listRecord_F, fileName1, true);
                    WritePointsToTxt(listRecord_d, fileName2, false);
                }
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (mCCoarsePositioner[0].IsConnected)
            {
                mCCoarsePositioner[0].default_set();
            }

            //uiStartBit = uiCurrentBit = uiInitBit;
            //mCScanPositioner.MoveToFinePosition(mCaxis_F, uiInitBit, 10, 20);
            //Delay(100);
            //btnZeroing_Click(null, null);
        }

        private void btnModeSwitch_Click(object sender, EventArgs e)
        {
            if (mCCoarsePositioner[0].IsConnected)
            {
                if (btnModeSwitch.Text == "Switch to Open Loop" || btnModeSwitch.Text == "切换到开环")
                {
                    if (!bCN)
                    {
                        btnModeSwitch.Text = "Switch to Close Loop";
                    }
                    else
                    {
                        btnModeSwitch.Text = "切换到闭环";
                    }

                    mCCoarsePositioner[0].SetClosedLoopHoldEnabled(mCaxis_X, 0);
                    mCCoarsePositioner[0].Stop(mCaxis_X);
                    mCCoarsePositioner[0].SetClosedLoopHoldEnabled(mCaxis_Y, 0);
                    mCCoarsePositioner[0].Stop(mCaxis_Y);
                    mCCoarsePositioner[0].SetClosedLoopHoldEnabled(mCaxis_Z, 0);
                    mCCoarsePositioner[0].Stop(mCaxis_Z);
                }
                else
                {
                    if (!bCN)
                    {
                        btnModeSwitch.Text = "Switch to Open Loop";
                    }
                    else
                    {
                        btnModeSwitch.Text = "切换到开环";
                    }

                    mCCoarsePositioner[0].SetClosedLoopHoldEnabled(mCaxis_X, 1);
                    mCCoarsePositioner[0].MoveDistance_CloseLoop_Absolute(mCaxis_X, iPositionX);
                    mCCoarsePositioner[0].SetClosedLoopHoldEnabled(mCaxis_Y, 1);
                    mCCoarsePositioner[0].MoveDistance_CloseLoop_Absolute(mCaxis_Y, iPositionY);
                    mCCoarsePositioner[0].SetClosedLoopHoldEnabled(mCaxis_Z, 1);
                    mCCoarsePositioner[0].MoveDistance_CloseLoop_Absolute(mCaxis_Z, iPositionZ);
                }
            }
        }

        private void btnContinuous_Click(object sender, EventArgs e)
        {
            bContinuous = true;
            bStep = false;
            btnContinuous.BackColor = bContinuous ? Color.DodgerBlue : Color.Transparent;
            btnStep.BackColor = bStep ? Color.DodgerBlue : Color.Transparent;
            btnXN.Enabled = btnXP.Enabled = bStep;
            btnYN.Enabled = btnYP.Enabled = bStep;
            btnZN.Enabled = btnZP.Enabled = bStep;
        }

        private void btnContinuousDown_Click(object sender, EventArgs e)
        {
            if (iContinuousSpeed > 1)
            {
                iContinuousSpeed -= 1;

                lblContinuousSpeed.Text = "Speed " + iContinuousSpeed.ToString();
                iBtnSteps = int.Parse(dSAF_OL[iContinuousSpeed - 1, 0].ToString());
                uiAmplitude = (uint)dSAF_OL[iContinuousSpeed - 1, 1];
                mGetFunctions.uiFrequencyMax = uiFrequencyMax = (uint)dSAF_OL[iContinuousSpeed - 1, 2];
                mGetFunctions.dDistanceMax = dDistanceMax = dDistance_CL[iContinuousSpeed - 1];
            }
        }

        private void btnContinuousUp_Click(object sender, EventArgs e)
        {
            if (iContinuousSpeed < iSpeedTotalLevel)
            {
                iContinuousSpeed += 1;

                lblContinuousSpeed.Text = "Speed " + iContinuousSpeed.ToString();
                iBtnSteps = int.Parse(dSAF_OL[iContinuousSpeed - 1, 0].ToString());
                uiAmplitude = (uint)dSAF_OL[iContinuousSpeed - 1, 1];
                mGetFunctions.uiFrequencyMax = uiFrequencyMax = (uint)dSAF_OL[iContinuousSpeed - 1, 2];
                mGetFunctions.dDistanceMax = dDistanceMax = dDistance_CL[iContinuousSpeed - 1];
            }
        }

        private void btnStep_Click(object sender, EventArgs e)
        {
            bContinuous = false;
            bStep = true;
            btnContinuous.BackColor = bContinuous ? Color.DodgerBlue : Color.Transparent;
            btnStep.BackColor = bStep ? Color.DodgerBlue : Color.Transparent;
            btnXN.Enabled = btnXP.Enabled = bStep;
            btnYN.Enabled = btnYP.Enabled = bStep;
            btnZN.Enabled = btnZP.Enabled = bStep;
        }

        private void btnStepDown_Click(object sender, EventArgs e)
        {
            if (iStepSpeed > 1)
            {
                iStepSpeed -= 1;
                lblStep.Text = dStep_CL[iStepSpeed - 1].ToString() + " um";
            }
        }

        private void btnStepUp_Click(object sender, EventArgs e)
        {
            if (iStepSpeed < iSpeedTotalLevel)
            {
                iStepSpeed += 1;
                lblStep.Text = dStep_CL[iStepSpeed - 1].ToString() + " um";
            }
        }


        private void btnXP_Click(object sender, EventArgs e)
        {
            if (ckbConvert.CheckState == CheckState.Checked)
            {
                if (bStep)
                {
                    if (mCCoarsePositioner[0].IsConnected && bMoveX && bEnableX)
                    {
                        if (lblStep.Text != "" && lblStep.Text != null)
                        {
                            double dX = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_X, dX * iXP);
                        }
                    }
                }
            }
            else
            {
                if (bStep)
                {
                    if (mCCoarsePositioner[0].IsConnected && bMoveX && bEnableX && bMoveXDown)
                    {
                        if (lblStep.Text != "" && lblStep.Text != null)
                        {
                            double dX = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_X, dX * iXN);
                        }
                    }
                }
            }
        }


        private void btnXN_Click(object sender, EventArgs e)
        {
            if (ckbConvert.CheckState == CheckState.Checked)
            {
                if (bStep)
                {
                    if (mCCoarsePositioner[0].IsConnected && bMoveX && bEnableX && bMoveXDown)
                    {
                        if (lblStep.Text != "" && lblStep.Text != null)
                        {
                            double dX = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_X, dX * iXN);
                        }
                    }
                }
            }
            else 
            {
                if (bStep)
                {
                    if (mCCoarsePositioner[0].IsConnected && bMoveX && bEnableX)
                    {
                        if (lblStep.Text != "" && lblStep.Text != null)
                        {
                            double dX = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_X, dX * iXP);
                        }
                    }
                }
            }
        }

        private void btnYP_Click(object sender, EventArgs e)
        {
            if (ckbConvert.CheckState == CheckState.Checked)
            {
                if (bStep)
                {
                    if (mCCoarsePositioner[0].IsConnected && bMoveY && bEnableY)
                    {
                        if (lblStep.Text != "" && lblStep.Text != null)
                        {
                            double dY = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dY * iYP);
                        }
                    }
                }
            }
            else
            {
                if (bStep)
                {
                    if (mCCoarsePositioner[0].IsConnected && bMoveY && bEnableY && bMoveYDown)
                    {
                        if (lblStep.Text != "" && lblStep.Text != null)
                        {
                            double dY = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dY * iYN);
                        }
                    }
                }
            }
        }

        private void btnYN_Click(object sender, EventArgs e)
        {
            if (ckbConvert.CheckState == CheckState.Checked)
            {
                if (bStep)
                {
                    if (mCCoarsePositioner[0].IsConnected && bMoveY && bEnableY && bMoveYDown)
                    {
                        if (lblStep.Text != "" && lblStep.Text != null)
                        {
                            double dY = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dY * iYN);
                        }
                    }
                }
            }else
            {
                if (bStep)
                {
                    if (mCCoarsePositioner[0].IsConnected && bMoveY && bEnableY)
                    {
                        if (lblStep.Text != "" && lblStep.Text != null)
                        {
                            double dY = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dY * iYP);
                        }
                    }
                }
            }
        }

        //Sam
        //private void btnZP_Click(object sender, EventArgs e)
        //{
        //    if (bStep)
        //    {
        //        if (mCSmarActPositioner.IsConnected && bMoveZ && bEnableZ)
        //        {
        //            if (lblStep.Text != "" && lblStep.Text != null)
        //            {
        //                double dZ = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
        //                mCSmarActPositioner.SetSpeedClosedLoop(mCaxis_Z, 0);
        //                mCSmarActPositioner.MoveDistance_CloseLoop(mCaxis_Z, dZ * iZP);
        //            }
        //        }
        //        /*
        //        if (mCCoarsePositioner[0].IsConnected && bMoveZ && bEnableZ)
        //        {
        //            if (lblStep.Text != "" && lblStep.Text != null)
        //            {
        //                double dZ = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
        //                mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Z, dZ * iZP);
        //            }
        //        }*/
        //    }
        //}

        ////Sam
        //private void btnZN_Click(object sender, EventArgs e)
        //{
        //    if (mCSmarActPositioner.IsConnected && bMoveZ && bEnableZ)
        //    {
        //        if (lblStep.Text != "" && lblStep.Text != null)
        //        {
        //            double dZ = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
        //            mCSmarActPositioner.SetSpeedClosedLoop(mCaxis_Z, 0);
        //            mCSmarActPositioner.MoveDistance_CloseLoop(mCaxis_Z, dZ * iZN);
        //        }
        //    }

        //    /* Sam
        //    if (bStep)
        //    {
        //        if (mCCoarsePositioner[0].IsConnected && bMoveZ && bEnableZ && bMoveZDown)
        //        {
        //            if (lblStep.Text != "" && lblStep.Text != null)
        //            {
        //                double dZ = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
        //                mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Z, dZ * iZN);
        //            }
        //        }
        //    }*/
        //}

        //Sam
        private void btnZP_Click(object sender, EventArgs e)
        {
            if (bStep)
            {
                if (mCCoarsePositioner[0].IsConnected && bMoveZ && bEnableZ)
                {
                    if (lblStep.Text != "" && lblStep.Text != null)
                    {
                        double dZ = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                        mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Z, dZ * iZP);
                    }
                }
            }
        }

        private void btnZN_Click(object sender, EventArgs e)
        {
            if (bStep)
            {
                if (mCCoarsePositioner[0].IsConnected && bMoveZ && bEnableZ && bMoveZDown)
                {
                    if (lblStep.Text != "" && lblStep.Text != null)
                    {
                        double dZ = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                        mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Z, dZ * iZN);
                    }
                }
            }
        }

        private void btnClockwise_MouseDown(object sender, MouseEventArgs e)
        {
            dClickStart = DateTime.Now;
            bClockwiseDown = true;
            if (!timerShort.Enabled)
            {
                timerShort.Start();
            }

            if (!timerLong.Enabled)
            {
                timerLong.Start();
            }
        }

        private void btnClockwise_MouseUp(object sender, MouseEventArgs e)
        {
            bClockwiseDown = false;
            if (timerLong.Enabled)
            {
                timerLong.Stop();
            }
        }

        private void btnAnticlockwise_MouseDown(object sender, MouseEventArgs e)
        {
            dClickStart = DateTime.Now;
            bAnticlockwiseDown = true;
            if (!timerShort.Enabled)
            {
                timerShort.Start();
            }

            if (!timerLong.Enabled)
            {
                timerLong.Start();
            }
        }

        private void btnAnticlockwise_MouseUp(object sender, MouseEventArgs e)
        {
            bAnticlockwiseDown = false;
            if (timerLong.Enabled)
            {
                timerLong.Stop();
            }
        }

        //自动靠近按钮
        private void btnAutoApproach_Click(object sender, EventArgs e)
        {
            if (btnAutoApproach.Text == "Auto Approach" || btnAutoApproach.Text == "自动靠近")
            {
                if (mCCoarsePositioner[0].IsConnected && serialPort.IsOpen)
                {
                    if (cmbApproachSpeed.Text != "" && cmbApproachSpeed.Text != null
                        && cmbApproachMotionDirection.Text != "" && cmbApproachMotionDirection.Text != null
                        && txbApproachForceThreshold.Text != "" && txbApproachForceThreshold.Text != null
                        && txbApproachRetreatDistance.Text != "" && txbApproachRetreatDistance.Text != null)
                    {
                        if (!bCN)
                        {
                            btnAutoApproach.Text = "Stop Approach";
                        }
                        else
                        {
                            btnAutoApproach.Text = "停止靠近";
                        }

                        btnZeroing_Click(null, null);
                        Delay(100);

                        dApproachForceThreshold = Convert.ToDouble(txbApproachForceThreshold.Text);
                        dApproachRetreatDistance = Convert.ToDouble(txbApproachRetreatDistance.Text) * Math.Pow(10, 3);
                        iApproachDirectionIndex = -1;
                        dApproachByCL = 625;

                        bApproach = true;
                        bWhileApproach = true;
                        bWhileAdjust = true;
                        bStopApproach = false;
                        bApproachFirstIn = false;

                        AutoApproachEnabledControls(false);

                        mThread_AutoApproach = new Thread(new ThreadStart(ThreadFunction_Approach));
                        mThread_AutoApproach.Start();
                    }
                }
            }
            else
            {
                //if (!bCN)
                //{
                //    btnAutoApproach.Text = "Auto Approach";
                //}
                //else
                //{
                //    btnAutoApproach.Text = "自动靠近";
                //}
                bApproach = false;
                bWhileApproach = false;
                bWhileAdjust = false;

                bStopApproach = true;
                //AutoApproachEnabledControls(true);
            }
        }

        private void btnIndentation_Click(object sender, EventArgs e)
        {
            if (mThread_ShowCurve != null)
            {
                mThread_ShowCurve.Abort();
            }

            if (mThread_Indent != null)
            {
                mThread_Indent.Abort();
            }

            if (btnIndentation.Text == "Start Indentation" || btnIndentation.Text == "开始压痕")
            {
                bPause = false;                 //暂停功能关闭

                if (mCScanPositioner.IsConnected && serialPort.IsOpen)
                {
                    if (txbIndentDelay.Text != "" && txbIndentDelay.Text != null)
                    {
                        try
                        {
                            iExtendDelay = int.Parse(txbIndentDelay.Text);
                        }
                        catch (Exception ex)
                        {
                            iExtendDelay = 0;
                        }
                    }
                    else
                    {
                        iExtendDelay = 0;
                    }

                    if (txbIndentWithdrawDisp.Text != "" && txbIndentWithdrawDisp.Text != null)
                    {
                        try
                        {
                            dWithdrawDisp = Convert.ToDouble(txbIndentWithdrawDisp.Text) * Math.Pow(10, 3);
                        }
                        catch (Exception ex)
                        {
                            dWithdrawDisp = 0;
                        }
                    }
                    else
                    {
                        dWithdrawDisp = 0;
                    }

                    if (txbIndentWithdrawDelay.Text != "" && txbIndentWithdrawDelay.Text != null)
                    {
                        try
                        {
                            //iWithdrawDelay = 0;
                            iWithdrawDelay = int.Parse(txbIndentWithdrawDelay.Text);

                        }
                        catch (Exception ex)
                        {
                            iWithdrawDelay = 0;
                        }
                    }
                    else
                    {
                        iWithdrawDelay = 0;
                    }

                    //当两个都有数时
                    if (dWithdrawDisp > 0 && iWithdrawDelay > 0)
                    {
                        bWithdrawDelay = true;
                    }
                    else
                    {
                        bWithdrawDelay = false;
                    }

                    if (cmbIndentSpeed.Text != "" && cmbIndentSpeed.Text != null)
                    {
                        if (cmbIndentSpeed.SelectedIndex == 0)//1nm/s
                        {
                            uiIndentSpeed = 4;
                            iIndentDelay = 180;
                            dIndentApproximateSpeed_nm = 1;
                        }
                        else if (cmbIndentSpeed.SelectedIndex == 1)//5nm/s
                        {
                            uiIndentSpeed = 12;
                            iIndentDelay = 100;
                            dIndentApproximateSpeed_nm = 5;
                        }
                        else if (cmbIndentSpeed.SelectedIndex == 2)//10nm/s
                        {
                            uiIndentSpeed = 23;
                            iIndentDelay = 100;
                            dIndentApproximateSpeed_nm = 10;
                        }
                        else if (cmbIndentSpeed.SelectedIndex == 3)//20nm/s
                        {
                            uiIndentSpeed = 47;
                            iIndentDelay = 90;
                            dIndentApproximateSpeed_nm = 20;
                        }
                        else if (cmbIndentSpeed.SelectedIndex == 4)//50nm/s
                        {
                            uiIndentSpeed = 60;
                            iIndentDelay = 40;
                            dIndentApproximateSpeed_nm = 50;
                        }
                        else if (cmbIndentSpeed.SelectedIndex == 5)//100nm/s
                        {
                            uiIndentSpeed = 71;
                            iIndentDelay = 30;
                            dIndentApproximateSpeed_nm = 100;
                        }
                        else if (cmbIndentSpeed.SelectedIndex == 6)//150nm/s
                        {
                            uiIndentSpeed = 106;
                            iIndentDelay = 20;
                            dIndentApproximateSpeed_nm = 150;
                        }
                        else /*if (cmbIndentSpeed.SelectedIndex == 7)//300nm/s*/
                        {
                            uiIndentSpeed = 110;
                            iIndentDelay = 10;
                            dIndentApproximateSpeed_nm = 300;
                        }
                        //else//500nm/s
                        //{
                        //    uiIndentSpeed = 170;
                        //    iIndentDelay = 5;
                        //    dIndentApproximateSpeed_nm = 500;
                        //}
                    }

                    bPause = false;
                    bIndentPause.Text = "暂停";
                    bIndentPause.Enabled = true;

                    if (cmbIndentType.Text != "" && cmbIndentType.Text != null)
                    {
                        if (cmbIndentType.SelectedIndex == 0)
                        {
                            if (cmbIndentSpeed.Text != "" && cmbIndentSpeed.Text != null
                                && txbIndentDeformation.Text != "" && txbIndentDeformation.Text != null)
                            {

                                bCompression = false;

                                zgcIndentation.GraphPane.CurveList.Clear();
                                zgcIndentation.AxisChange();//画到zedGraphControl1控件中
                                zgcIndentation.Refresh();//重新刷新

                                if (!bCN)
                                {
                                    btnIndentation.Text = "Stop Indentation";
                                }
                                else
                                {
                                    btnIndentation.Text = "停止压痕";
                                }

                                IndentationEnabledControls(false);

                                if (bHighFreq)
                                {
                                    //if (!bFirstIndent)
                                    {
                                        frmHighFreqWaiting.Show();
                                    }

                                    serialPort.Write("5l\r\n");
                                    //Delay(1000);

                                    strHighFreqListExtend = new List<string>();
                                    strHighFreqListWithdraw = new List<string>();
                                }

                                uiStartBit = uiCurrentBit = mCScanPositioner.GetFinePosition(mCaxis_F);
                                strTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                                uiIndentSpeedIndex = (uint)(cmbIndentSpeed.SelectedIndex + 1);
                                iIndentTypeIndex = cmbIndentType.SelectedIndex;
                                dIndentDistance = Convert.ToDouble(txbIndentDeformation.Text) * Math.Pow(10, 3);

                                mThread_ShowCurve = new Thread(new ThreadStart(ThreadFunction_ShowCurve));
                                mThread_ShowCurve.Start();

                                listIndentExtend = new PointPairList();
                                listIndentWithdraw = new PointPairList();
                                listIndentExtend_ori = new PointPairList();
                                listIndentWithdraw_ori = new PointPairList();
                                listIndentNoMissing = new PointPairList();
                                listIndentNoMissing_ori = new PointPairList();

                                //压痕的刚度取值为两项之和
                                dStiffness = Convert.ToDouble(txbStiffness.Text) + Convert.ToDouble(txbExtraStiffness.Text);
                                Delay(100);
                                btnZeroing_Click(null, null);
                                Delay(100);

                                bStabilization = false;
                                bIndent = true;
                                bIndentation = true;
                                bFirstWithdraw_1 = true;
                                bFirstWithdraw_2 = true;
                                bWhileIndent = true;
                                bWhileIndentWithdraw = true;

                                mThread_Indent = new Thread(new ThreadStart(ThreadFunction_Indent));
                                mThread_Indent.Start();
                                //}
                                //else
                                //{
                                //    if (!bCN)
                                //    {
                                //        MessageBox.Show("\'Indent Distance\' out of range !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                //    }
                                //    else
                                //    {
                                //        MessageBox.Show("\'加载距离\'超出范围！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                //    }
                                //}
                            }
                        }
                        else if (cmbIndentType.SelectedIndex == 1)
                        {
                            if (cmbIndentSpeed.Text != "" && cmbIndentSpeed.Text != null
                                && txbIndentDistance.Text != "" && txbIndentDistance.Text != null)
                            {
                                if (Convert.ToDouble(txbIndentDistance.Text) >= dIndentDistanceMin
                                    && Convert.ToDouble(txbIndentDistance.Text) <= dIndentDistanceMax)
                                {
                                    bCompression = false;

                                    zgcIndentation.GraphPane.CurveList.Clear();
                                    zgcIndentation.AxisChange();//画到zedGraphControl1控件中
                                    zgcIndentation.Refresh();//重新刷新

                                    if (!bCN)
                                    {
                                        btnIndentation.Text = "Stop Indentation";
                                    }
                                    else
                                    {
                                        btnIndentation.Text = "停止压痕";
                                    }
                                    IndentationEnabledControls(false);

                                    if (bHighFreq)
                                    {
                                        //if (!bFirstIndent)
                                        {
                                            frmHighFreqWaiting.Show();
                                        }

                                        serialPort.Write("5l\r\n");
                                        //Delay(1000);

                                        strHighFreqListExtend = new List<string>();
                                        strHighFreqListWithdraw = new List<string>();
                                    }

                                    uiStartBit = uiCurrentBit = mCScanPositioner.GetFinePosition(mCaxis_F);
                                    strTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                                    uiIndentSpeedIndex = (uint)(cmbIndentSpeed.SelectedIndex + 1);
                                    iIndentTypeIndex = cmbIndentType.SelectedIndex;
                                    dIndentDistance = Convert.ToDouble(txbIndentDistance.Text) * Math.Pow(10, 3);

                                    mThread_ShowCurve = new Thread(new ThreadStart(ThreadFunction_ShowCurve));
                                    mThread_ShowCurve.Start();

                                    listIndentExtend = new PointPairList();
                                    listIndentWithdraw = new PointPairList();
                                    listIndentExtend_ori = new PointPairList();
                                    listIndentWithdraw_ori = new PointPairList();
                                    listIndentNoMissing = new PointPairList();
                                    listIndentNoMissing_ori = new PointPairList();

                                    dStiffness = Convert.ToDouble(txbStiffness.Text) + Convert.ToDouble(txbExtraStiffness.Text);
                                    Delay(100);
                                    btnZeroing_Click(null, null);
                                    Delay(100);

                                    bStabilization = false;
                                    bIndent = true;
                                    bIndentation = true;
                                    bFirstWithdraw_1 = true;
                                    bFirstWithdraw_2 = true;
                                    bWhileIndent = true;
                                    bWhileIndentWithdraw = true;

                                    mThread_Indent = new Thread(new ThreadStart(ThreadFunction_Indent));
                                    mThread_Indent.Start();
                                }
                                else
                                {
                                    if (!bCN)
                                    {
                                        MessageBox.Show("\'Indent Distance\' out of range !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                    else
                                    {
                                        MessageBox.Show("\'加载距离\'超出范围！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (cmbIndentSpeed.Text != "" && cmbIndentSpeed.Text != null
                                && txbIndentForceThreshold.Text != "" && txbIndentForceThreshold.Text != null)
                            {
                                if (Convert.ToDouble(txbIndentForceThreshold.Text) >= dIndentForceThresholdMin
                                    && Convert.ToDouble(txbIndentForceThreshold.Text) <= dIndentForceThresholdMax)
                                {
                                    bCompression = false;

                                    zgcIndentation.GraphPane.CurveList.Clear();
                                    zgcIndentation.AxisChange();                //画到zedGraphControl1控件中
                                    zgcIndentation.Refresh();                   //重新刷新

                                    if (!bCN)
                                    {
                                        btnIndentation.Text = "Stop Indentation";
                                    }
                                    else
                                    {
                                        btnIndentation.Text = "停止压痕";
                                    }
                                    IndentationEnabledControls(false);

                                    if (bHighFreq)
                                    {
                                        //if (!bFirstIndent)
                                        {
                                            frmHighFreqWaiting.Show();
                                        }

                                        serialPort.Write("5l\r\n");
                                        //Delay(1000);

                                        strHighFreqListExtend = new List<string>();
                                        strHighFreqListWithdraw = new List<string>();
                                    }

                                    uiStartBit = uiCurrentBit = mCScanPositioner.GetFinePosition(mCaxis_F);
                                    strTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                                    iIndentTypeIndex = cmbIndentType.SelectedIndex;
                                    dIndentForceThreshold = Convert.ToDouble(txbIndentForceThreshold.Text);

                                    mThread_ShowCurve = new Thread(new ThreadStart(ThreadFunction_ShowCurve));
                                    mThread_ShowCurve.Start();

                                    listIndentExtend = new PointPairList();
                                    listIndentWithdraw = new PointPairList();
                                    listIndentExtend_ori = new PointPairList();
                                    listIndentWithdraw_ori = new PointPairList();
                                    listIndentNoMissing = new PointPairList();
                                    listIndentNoMissing_ori = new PointPairList();

                                    dStiffness = Convert.ToDouble(txbStiffness.Text) + Convert.ToDouble(txbExtraStiffness.Text);
                                    Delay(100);
                                    btnZeroing_Click(null, null);
                                    Delay(100);

                                    bStabilization = false;
                                    bIndent = true;
                                    bIndentation = true;
                                    bFirstWithdraw_1 = true;
                                    bFirstWithdraw_2 = true;
                                    bWhileIndent = true;
                                    bWhileIndentWithdraw = true;

                                    mThread_Indent = new Thread(new ThreadStart(ThreadFunction_Indent));
                                    mThread_Indent.Start();
                                }
                                else
                                {
                                    if (!bCN)
                                    {
                                        MessageBox.Show("\'Force Threshold\' out of range !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                    else
                                    {
                                        MessageBox.Show("\'力阈值\'超出范围！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                bStabilization = false;
                bShowCurve = false;
                //bFirstIndent = false;
                bIndent = false;
                bWhileIndent = false;
                bWhileIndentWithdraw = true;
                bPause = true;

                //暂停按钮失能
                bIndentPause.Enabled = false;

                if (bHighFreq)
                {
                    Delay(1000);
                    serialPort.Write("5f\r\n");
                    frmHighFreqWaiting.Hide();
                }

                bIndentation = false;
                if (!bCN)
                {
                    btnIndentation.Text = "Start Indentation";
                }
                else
                {
                    btnIndentation.Text = "开始压痕";
                }
                IndentationEnabledControls(true);

                //mCScanPositioner.MoveToFinePosition(mCaxis_F, uiStartBit, 300000);

                int iBackStep = 16;
                double dStepBit = 0;
                if (uiCurrentBit >= uiStartBit)
                {
                    dStepBit = (uiCurrentBit - uiStartBit) * 1.0 / iBackStep;
                    for (int i = 0; i < iBackStep; i++)
                    {
                        Delay(25);
                        uiCurrentBit -= (uint)dStepBit;
                        mCScanPositioner.MoveToFinePosition(mCaxis_F, uiCurrentBit, 10, 20);
                    }
                }
                else
                {
                    dStepBit = (uiStartBit - uiCurrentBit) * 1.0 / iBackStep;
                    for (int i = 0; i < iBackStep; i++)
                    {
                        Delay(25);
                        uiCurrentBit += (uint)dStepBit;
                        mCScanPositioner.MoveToFinePosition(mCaxis_F, uiCurrentBit, 10, 20);
                    }
                }

                //uiCurrentBit = uiStartBit;
                Delay(100);

                //SaveData();
                SaveDataNoMissing();

                dSetDAC = dInitValue_d;
                bStabilization = true;
            }
        }

        private void btnSaveIndentationData_Click(object sender, EventArgs e)
        {
            if (strTime != "" && strTime != null)
            {
                saveFileDialog1.Filter = "文本文件| *.txt";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK && saveFileDialog1.FileName.Length > 0)
                {
                    string strSourceFileName, strSourceFileName_ori, strDestFileName, strDestFileName_ori;
                    strSourceFileName_ori = strFilePathTemporary + "\\" + strTime + "_ori.txt";
                    strSourceFileName = strFilePathTemporary + "\\" + strTime + ".txt";

                    strDestFileName_ori = saveFileDialog1.FileName.Remove(saveFileDialog1.FileName.Length - 4) + "_ori.txt";
                    if (File.Exists(strSourceFileName_ori))
                    {
                        if (File.Exists(strDestFileName_ori))
                        {
                            File.Delete(strDestFileName_ori);
                        }

                        File.Move(strSourceFileName_ori, strDestFileName_ori);
                    }

                    strDestFileName = saveFileDialog1.FileName;
                    if (File.Exists(strSourceFileName))
                    {
                        if (File.Exists(strDestFileName))
                        {
                            File.Delete(strDestFileName);
                        }

                        File.Move(strSourceFileName, strDestFileName);
                    }
                }
            }
        }


        private void btnCompression_Click(object sender, EventArgs e)
        {
            if (mThread_ShowCurve != null)
            {
                mThread_ShowCurve.Abort();
            }

            if (mThread_Indent != null)
            {
                mThread_Indent.Abort();
            }

            if (btnCompression.Text == "Start Compression" || btnCompression.Text == "开始压缩")
            {
                if (mCScanPositioner.IsConnected && serialPort.IsOpen)
                {
                    if (txbCompressDelay.Text != "" && txbCompressDelay.Text != null)
                    {
                        try
                        {
                            iExtendDelay = int.Parse(txbCompressDelay.Text);
                        }
                        catch (Exception ex)
                        {
                            iExtendDelay = 0;
                        }
                    }
                    else
                    {
                        iExtendDelay = 0;
                    }

                    if (txbCompressWithdrawDisp.Text != "" && txbCompressWithdrawDisp.Text != null)
                    {
                        try
                        {
                            dWithdrawDisp = Convert.ToDouble(txbCompressWithdrawDisp.Text) * Math.Pow(10, 3);
                        }
                        catch (Exception ex)
                        {
                            dWithdrawDisp = 0;
                        }
                    }
                    else
                    {
                        dWithdrawDisp = 0;
                    }

                    if (txbCompressWithdrawDelay.Text != "" && txbCompressWithdrawDelay.Text != null)
                    {
                        try
                        {
                            iWithdrawDelay = int.Parse(txbCompressWithdrawDelay.Text);

                        }
                        catch (Exception ex)
                        {
                            iWithdrawDelay = 0;
                        }
                    }
                    else
                    {
                        iWithdrawDelay = 0;
                    }

                    if (dWithdrawDisp > 0 && iWithdrawDelay > 0)
                    {
                        bWithdrawDelay = true;
                    }
                    else
                    {
                        bWithdrawDelay = false;
                    }

                    if (cmbCompressSpeed.Text != "" && cmbCompressSpeed.Text != null)
                    {
                        if (cmbCompressSpeed.SelectedIndex == 0)//1nm/s
                        {//71  30
                            uiIndentSpeed = 50;
                            iIndentDelay =80;
                            dIndentApproximateSpeed_nm = 1;
                        }
                        else if (cmbCompressSpeed.SelectedIndex == 1)//5nm/s
                        {
                            uiIndentSpeed = 12;
                            iIndentDelay = 100;
                            dIndentApproximateSpeed_nm = 5;
                        }
                        else if (cmbCompressSpeed.SelectedIndex == 2)//10nm/s
                        {
                            uiIndentSpeed = 23;
                            iIndentDelay = 100;
                            dIndentApproximateSpeed_nm = 10;
                        }
                        else if (cmbCompressSpeed.SelectedIndex == 3)//20nm/s
                        {
                            uiIndentSpeed = 47;
                            iIndentDelay = 90;
                            dIndentApproximateSpeed_nm = 20;
                        }
                        else if (cmbCompressSpeed.SelectedIndex == 4)//50nm/s
                        {
                            uiIndentSpeed = 60;
                            iIndentDelay = 40;
                            dIndentApproximateSpeed_nm = 50;
                        }
                        else if (cmbCompressSpeed.SelectedIndex == 5)//100nm/s
                        {
                            uiIndentSpeed = 71;
                            iIndentDelay = 30;
                            dIndentApproximateSpeed_nm = 100;
                        }
                        else if (cmbCompressSpeed.SelectedIndex == 6)//150nm/s
                        {
                            uiIndentSpeed = 106;
                            iIndentDelay = 20;
                            dIndentApproximateSpeed_nm = 150;
                        }
                        else /*if (cmbCompressSpeed.SelectedIndex == 7)//300nm/s*/
                        {
                            uiIndentSpeed = 110;
                            iIndentDelay = 10;
                            dIndentApproximateSpeed_nm = 300;
                        }
                        //else//500nm/s
                        //{
                        //    uiIndentSpeed = 170;
                        //    iIndentDelay = 5;
                        //    dIndentApproximateSpeed_nm = 500;
                        //}
                    }

                    bPause = false;
                    bPauseIndent.Text = "暂停";
                    bPauseIndent.Enabled = true;

                    if (cmbCompressType.Text != "" && cmbCompressType.Text != null)
                    {
                        if (cmbCompressType.SelectedIndex == 0)
                        {
                            if (cmbCompressSpeed.Text != "" && cmbCompressSpeed.Text != null
                                && txbCompressDeformation.Text != "" && txbCompressDeformation.Text != null)
                            {
                                //if (Convert.ToDouble(txbCompressDistance.Text) >= dIndentDistanceMin
                                //    && Convert.ToDouble(txbCompressDistance.Text) <= dIndentDistanceMax)
                                //{
                                bCompression = true;

                                zgcCompress.GraphPane.CurveList.Clear();
                                zgcCompress.AxisChange();//画到zedGraphControl1控件中
                                zgcCompress.Refresh();//重新刷新

                                if (!bCN)
                                {
                                    btnCompression.Text = "Stop Compression";
                                }
                                else
                                {
                                    btnCompression.Text = "终止压缩";
                                }
                                IndentationEnabledControls(false);

                                if (bHighFreq)
                                {
                                    //if (!bFirstIndent)
                                    {
                                        frmHighFreqWaiting.Show();
                                    }

                                    serialPort.Write("5l\r\n");
                                    //Delay(1000);

                                    strHighFreqListExtend = new List<string>();
                                    strHighFreqListWithdraw = new List<string>();
                                }

                                uiStartBit = uiCurrentBit = mCScanPositioner.GetFinePosition(mCaxis_F);
                                strTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                                uiIndentSpeedIndex = (uint)(cmbCompressSpeed.SelectedIndex + 1);
                                iIndentTypeIndex = cmbCompressType.SelectedIndex;
                                dIndentDistance = Convert.ToDouble(txbCompressDeformation.Text) * Math.Pow(10, 3);

                                mThread_ShowCurve = new Thread(new ThreadStart(ThreadFunction_ShowCurve));
                                mThread_ShowCurve.Start();

                                listIndentExtend = new PointPairList();
                                listIndentWithdraw = new PointPairList();
                                listIndentExtend_ori = new PointPairList();
                                listIndentWithdraw_ori = new PointPairList();
                                listIndentNoMissing = new PointPairList();
                                listIndentNoMissing_ori = new PointPairList();

                                dStiffness = Convert.ToDouble(txbStiffness.Text);
                                Delay(100);
                                btnZeroing_Click(null, null);
                                Delay(100);

                                bStabilization = false;
                                bIndent = true;
                                bIndentation = true;
                                bFirstWithdraw_1 = true;
                                bWhileIndent = true;
                                bWhileIndentWithdraw = true;

                                mThread_Indent = new Thread(new ThreadStart(ThreadFunction_Indent));
                                mThread_Indent.Start();
                                //}
                                //else
                                //{
                                //    if (!bCN)
                                //    {
                                //        MessageBox.Show("\'Compress Distance\' out of range !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                //    }
                                //    else
                                //    {
                                //        MessageBox.Show("\'加载距离\'超出范围！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                //    }
                                //}
                            }
                        }
                        else if (cmbCompressType.SelectedIndex == 1)
                        {
                            if (cmbCompressSpeed.Text != "" && cmbCompressSpeed.Text != null
                                && txbCompressDistance.Text != "" && txbCompressDistance.Text != null)
                            {
                                if (Convert.ToDouble(txbCompressDistance.Text) >= dIndentDistanceMin
                                    && Convert.ToDouble(txbCompressDistance.Text) <= dIndentDistanceMax)
                                {
                                    bCompression = true;

                                    zgcCompress.GraphPane.CurveList.Clear();
                                    zgcCompress.AxisChange();//画到zedGraphControl1控件中
                                    zgcCompress.Refresh();//重新刷新

                                    if (!bCN)
                                    {
                                        btnCompression.Text = "Stop Compression";
                                    }
                                    else
                                    {
                                        btnCompression.Text = "终止压缩";
                                    }
                                    IndentationEnabledControls(false);

                                    if (bHighFreq)
                                    {
                                        //if (!bFirstIndent)
                                        {
                                            frmHighFreqWaiting.Show();
                                        }

                                        serialPort.Write("5l\r\n");
                                        //Delay(1000);

                                        strHighFreqListExtend = new List<string>();
                                        strHighFreqListWithdraw = new List<string>();
                                    }

                                    uiStartBit = uiCurrentBit = mCScanPositioner.GetFinePosition(mCaxis_F);
                                    strTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                                    uiIndentSpeedIndex = (uint)(cmbCompressSpeed.SelectedIndex + 1);
                                    iIndentTypeIndex = cmbCompressType.SelectedIndex;
                                    dIndentDistance = Convert.ToDouble(txbCompressDistance.Text) * Math.Pow(10, 3);

                                    mThread_ShowCurve = new Thread(new ThreadStart(ThreadFunction_ShowCurve));
                                    mThread_ShowCurve.Start();

                                    listIndentExtend = new PointPairList();
                                    listIndentWithdraw = new PointPairList();
                                    listIndentExtend_ori = new PointPairList();
                                    listIndentWithdraw_ori = new PointPairList();
                                    listIndentNoMissing = new PointPairList();
                                    listIndentNoMissing_ori = new PointPairList();

                                    dStiffness = Convert.ToDouble(txbStiffness.Text);
                                    Delay(100);
                                    btnZeroing_Click(null, null);
                                    Delay(100);

                                    bStabilization = false;
                                    bIndent = true;
                                    bIndentation = true;
                                    bFirstWithdraw_1 = true;
                                    bWhileIndent = true;
                                    bWhileIndentWithdraw = true;

                                    mThread_Indent = new Thread(new ThreadStart(ThreadFunction_Indent));
                                    mThread_Indent.Start();
                                }
                                else
                                {
                                    if (!bCN)
                                    {
                                        MessageBox.Show("\'Compress Distance\' out of range !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                    else
                                    {
                                        MessageBox.Show("\'加载距离\'超出范围！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (cmbCompressSpeed.Text != "" && cmbCompressSpeed.Text != null
                                && txbCompressForceThreshold.Text != "" && txbCompressForceThreshold.Text != null)
                            {
                                if (Convert.ToDouble(txbCompressForceThreshold.Text) >= dIndentForceThresholdMin
                                    && Convert.ToDouble(txbCompressForceThreshold.Text) <= dIndentForceThresholdMax)
                                {
                                    bCompression = true;

                                    zgcCompress.GraphPane.CurveList.Clear();
                                    zgcCompress.AxisChange();//画到zedGraphControl1控件中
                                    zgcCompress.Refresh();//重新刷新

                                    if (!bCN)
                                    {
                                        btnCompression.Text = "Stop Compression";
                                    }
                                    else
                                    {
                                        btnCompression.Text = "终止压缩";
                                    }
                                    IndentationEnabledControls(false);

                                    if (bHighFreq)
                                    {
                                        //if (!bFirstIndent)
                                        {
                                            frmHighFreqWaiting.Show();
                                        }

                                        serialPort.Write("5l\r\n");
                                        //Delay(1000);

                                        strHighFreqListExtend = new List<string>();
                                        strHighFreqListWithdraw = new List<string>();
                                    }

                                    uiStartBit = uiCurrentBit = mCScanPositioner.GetFinePosition(mCaxis_F);
                                    strTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                                    iIndentTypeIndex = cmbCompressType.SelectedIndex;
                                    dIndentForceThreshold = Convert.ToDouble(txbCompressForceThreshold.Text);

                                    mThread_ShowCurve = new Thread(new ThreadStart(ThreadFunction_ShowCurve));
                                    mThread_ShowCurve.Start();

                                    listIndentExtend = new PointPairList();
                                    listIndentWithdraw = new PointPairList();
                                    listIndentExtend_ori = new PointPairList();
                                    listIndentWithdraw_ori = new PointPairList();
                                    listIndentNoMissing = new PointPairList();
                                    listIndentNoMissing_ori = new PointPairList();

                                    dStiffness = Convert.ToDouble(txbStiffness.Text);
                                    Delay(100);
                                    btnZeroing_Click(null, null);
                                    Delay(100);

                                    bStabilization = false;
                                    bIndent = true;
                                    bIndentation = true;
                                    bFirstWithdraw_1 = true;
                                    bWhileIndent = true;
                                    bWhileIndentWithdraw = true;

                                    mThread_Indent = new Thread(new ThreadStart(ThreadFunction_Indent));
                                    mThread_Indent.Start();
                                }
                                else
                                {
                                    if (!bCN)
                                    {
                                        MessageBox.Show("\'Force Threshold\' out of range !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                    else
                                    {
                                        MessageBox.Show("\'力阈值\'超出范围！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                bStabilization = false;
                bShowCurve = false;
                //bFirstIndent = false;
                bIndent = false;
                bWhileIndent = false;
                bWhileIndentWithdraw = true;
                bPause = true;

                //暂停按钮失能
                bPauseIndent.Enabled = false;

                if (bHighFreq)
                {
                    Delay(1000);
                    serialPort.Write("5f\r\n");
                    frmHighFreqWaiting.Hide();
                }

                bIndentation = false;
                if (!bCN)
                {
                    btnCompression.Text = "Start Compression";
                }
                else
                {
                    btnCompression.Text = "开始压缩";
                }
                IndentationEnabledControls(true);

                //mCScanPositioner.MoveToFinePosition(mCaxis_F, uiStartBit, 300000);

                int iBackStep = 16;
                double dStepBit = 0;
                if (uiCurrentBit >= uiStartBit)
                {
                    dStepBit = (uiCurrentBit - uiStartBit) * 1.0 / iBackStep;
                    for (int i = 0; i < iBackStep; i++)
                    {
                        Delay(25);
                        uiCurrentBit -= (uint)dStepBit;
                        mCScanPositioner.MoveToFinePosition(mCaxis_F, uiCurrentBit, 10, 20);
                    }
                }
                else
                {
                    dStepBit = (uiStartBit - uiCurrentBit) * 1.0 / iBackStep;
                    for (int i = 0; i < iBackStep; i++)
                    {
                        Delay(25);
                        uiCurrentBit += (uint)dStepBit;
                        mCScanPositioner.MoveToFinePosition(mCaxis_F, uiCurrentBit, 10, 20);
                    }
                }

                //uiCurrentBit = uiStartBit;
                Delay(100);

                //SaveData();
                SaveDataNoMissing();

                dSetDAC = dInitValue_d;
                bStabilization = true;
            }
        }


        private void btnSaveCompressionData_Click(object sender, EventArgs e)
        {
            if (strTime != "" && strTime != null)
            {
                saveFileDialog1.Filter = "文本文件| *.txt";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK && saveFileDialog1.FileName.Length > 0)
                {
                    string strSourceFileName, strSourceFileName_ori, strDestFileName, strDestFileName_ori;
                    strSourceFileName_ori = strFilePathTemporary + "\\" + strTime + "_ori.txt";
                    strSourceFileName = strFilePathTemporary + "\\" + strTime + ".txt";

                    strDestFileName_ori = saveFileDialog1.FileName.Remove(saveFileDialog1.FileName.Length - 4) + "_ori.txt";
                    if (File.Exists(strSourceFileName_ori))
                    {
                        if (File.Exists(strDestFileName_ori))
                        {
                            File.Delete(strDestFileName_ori);
                        }

                        File.Move(strSourceFileName_ori, strDestFileName_ori);
                    }

                    strDestFileName = saveFileDialog1.FileName;
                    if (File.Exists(strSourceFileName))
                    {
                        if (File.Exists(strDestFileName))
                        {
                            File.Delete(strDestFileName);
                        }

                        File.Move(strSourceFileName, strDestFileName);
                    }
                }
            }
        }

        private void btnTension_Click(object sender, EventArgs e)
        {

        }

        private void btnSaveTensionData_Click(object sender, EventArgs e)
        {

        }

        private void btnCloseOpenCL_Click(object sender, EventArgs e)
        {
            if (btnCloseOpenCL.Text == "Close CL" || btnCloseOpenCL.Text == "关闭闭环")
            {
                if (!bCN)
                {
                    btnCloseOpenCL.Text = "Open CL";
                }
                else
                {
                    btnCloseOpenCL.Text = "打开闭环";
                }
                bStabilization = false;
                bOpenCL = false;
            }
            else
            {
                if (!bCN)
                {
                    btnCloseOpenCL.Text = "关闭闭环";
                }
                else
                {

                }
                dSetDAC = dReadDAC;
                bStabilization = true;
                bOpenCL = true;
            }
        }

        private void btnAdcDown_Click(object sender, EventArgs e)
        {
            if (mCScanPositioner.IsConnected && txbAdcVariation.Text != "" && txbAdcVariation.Text != null)
            {
                bStabilization = false;
                uint uiAdcVariation = uint.Parse(txbAdcVariation.Text);
                uint uiCurAdc = mCScanPositioner.GetFinePosition(mCaxis_F);
                if (uiCurAdc < uiAdcVariation + uiMinADCValue)
                {
                    uiCurrentBit = uiMinADCValue;
                }
                else
                {
                    uiCurrentBit = uiCurAdc - uiAdcVariation;
                }
                mCScanPositioner.MoveToFinePosition(mCaxis_F, uiCurrentBit, 10, 20);

                Delay(250);
                dSetDAC = dReadDAC;
                uint uiGetVoltageLevel = mCScanPositioner.GetFinePosition(mCaxis_F);
                txbCurVOL.Text = uiGetVoltageLevel.ToString();

                if (bOpenCL)
                {
                    bStabilization = true;
                }
            }
        }

        private void btnAdcUp_Click(object sender, EventArgs e)
        {
            if (mCScanPositioner.IsConnected && txbAdcVariation.Text != "" && txbAdcVariation.Text != null)
            {
                bStabilization = false;
                uint uiAdcVariation = uint.Parse(txbAdcVariation.Text);
                uint uiCurAdc = mCScanPositioner.GetFinePosition(mCaxis_F);
                if (uiCurAdc + uiAdcVariation > uiMaxADCValue)
                {
                    uiCurrentBit = uiMaxADCValue;
                }
                else
                {
                    uiCurrentBit = uiCurAdc + uiAdcVariation;
                }
                mCScanPositioner.MoveToFinePosition(mCaxis_F, uiCurrentBit, 10, 20);

                Delay(250);
                dSetDAC = dReadDAC;
                uint uiGetVoltageLevel = mCScanPositioner.GetFinePosition(mCaxis_F);
                txbCurVOL.Text = uiGetVoltageLevel.ToString();

                if (bOpenCL)
                {
                    bStabilization = true;
                }
            }
        }

        private void btnAdcReset_Click(object sender, EventArgs e)
        {
            if (mCScanPositioner.IsConnected)
            {
                bStabilization = false;
                uiStartBit = uiCurrentBit = uiInitBit;
                mCScanPositioner.MoveToFinePosition(mCaxis_F, uiInitBit, 10, 20);

                Delay(500);

                //iPointCount_d = 0;
                //bInit_d = true;

                //listRecord_d = new PointPairList();
                //listCur_d = new PointPairList();

                //zgcForceSensorInitialize();

                //zgcForceSensor_d.GraphPane.CurveList.Clear();
                //zgcForceSensor_d.AxisChange();//画到zedGraphControl1控件中
                //zgcForceSensor_d.Refresh();//重新刷新

                btnZeroing_Click(null, null);

                Delay(50);
                dSetDAC = dReadDAC;

                if (bOpenCL)
                {
                    bStabilization = true;
                }
            }
        }

        private void btnStartRecord_Click(object sender, EventArgs e)
        {
            if (btnStartRecord.Text == "Start Record")
            {
                btnStartRecord.Text = "Stop Record";
                bTestRecord = true;
            }
            else
            {
                btnStartRecord.Text = "Start Record";
                bTestRecord = false;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txbTotal.Clear();
        }

        private void btnSetOriPID_Click(object sender, EventArgs e)
        {
            if (txbP_ori.Text != "" && txbP_ori.Text != null
                && txbI_ori.Text != "" && txbI_ori.Text != null
                && txbD_ori.Text != "" && txbD_ori.Text != null)
            {
                P = (decimal)Convert.ToDouble(txbP_ori.Text);
                I = (decimal)Convert.ToDouble(txbI_ori.Text);
                D = (decimal)Convert.ToDouble(txbD_ori.Text);

                txbP.Text = P.ToString();
                txbI.Text = I.ToString();
                txbD.Text = D.ToString();
            }
            else
            {
                if (!bCN)
                {
                    MessageBox.Show("PID can not be empty or 0 !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("PID不能为空或0！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnSetPID_Click(object sender, EventArgs e)
        {
            if (txbP.Text != "" && txbP.Text != null)
            {
                P = (decimal)Convert.ToDouble(txbP.Text);
            }
            else
            {
                P = P_ori;
                txbP.Text = P.ToString();
            }

            if (txbI.Text != "" && txbI.Text != null)
            {
                I = (decimal)Convert.ToDouble(txbI.Text);
            }
            else
            {
                I = I_ori;
                txbI.Text = I.ToString();
            }

            if (txbD.Text != "" && txbD.Text != null)
            {
                D = (decimal)Convert.ToDouble(txbD.Text);
            }
            else
            {
                D = D_ori;
                txbD.Text = D.ToString();
            }
        }

        #endregion

        #region Enable Controls

        public void EnabledControls(bool bEnabled)
        {
            btnReset.Enabled = bEnabled;
            btnModeSwitch.Enabled = bEnabled;
            btnSensorApply.Enabled = bEnabled;
            btnService.Enabled = bEnabled;

            btnApply.Enabled = bEnabled;
            btnZeroing.Enabled = bEnabled;
            btnRecordingData.Enabled = bEnabled;

            btnContinuous.Enabled = bEnabled;
            btnStep.Enabled = bEnabled;

            if (!bEnabled)
            {
                btnXP.Enabled = bEnabled;
                btnXN.Enabled = bEnabled;
                btnYP.Enabled = bEnabled;
                btnYN.Enabled = bEnabled;
                btnZP.Enabled = bEnabled;
                btnZN.Enabled = bEnabled;
            }
            else
            {
                btnXP.Enabled = bStep;
                btnXN.Enabled = bStep;
                btnYP.Enabled = bStep;
                btnYN.Enabled = bStep;
                btnZP.Enabled = bStep;
                btnZN.Enabled = bStep;
            }

            btnClockwise.Enabled = bEnabled;
            btnAnticlockwise.Enabled = bEnabled;

            btnCloseOpenCL.Enabled = bEnabled;
            btnAdcDown.Enabled = bEnabled;
            btnAdcUp.Enabled = bEnabled;
            btnAdcReset.Enabled = bEnabled;
        }

        public void AutoApproachEnabledControls(bool bEnabled)
        {
            EnabledControls(bEnabled);

            cmbApproachSpeed.Enabled = bEnabled;
            cmbApproachMotionDirection.Enabled = bEnabled;
            txbApproachForceThreshold.Enabled = bEnabled;
            txbApproachRetreatDistance.Enabled = bEnabled;

            btnIndentation.Enabled = bEnabled;
            btnSaveIndentationData.Enabled = bEnabled;

            btnCompression.Enabled = bEnabled;
            btnSaveCompressionData.Enabled = bEnabled;
            //Sam
            btIndentationCalibration.Enabled = bEnabled;
        }

        public void IndentationEnabledControls(bool bEnabled)
        {
            EnabledControls(bEnabled);

            btnAutoApproach.Enabled = bEnabled;

            if (!bCompression)
            {
                cmbIndentSpeed.Enabled = bEnabled;
                cmbIndentType.Enabled = bEnabled;
                if (cmbIndentType.SelectedIndex == 0)
                {
                    txbIndentDeformation.Enabled = bEnabled;
                }
                else if (cmbIndentType.SelectedIndex == 1)
                {
                    txbIndentDistance.Enabled = bEnabled;
                }
                else
                {
                    txbIndentForceThreshold.Enabled = bEnabled;
                }
                txbIndentDelay.Enabled = bEnabled;
                txbIndentWithdrawDisp.Enabled = bEnabled;
                txbIndentWithdrawDelay.Enabled = bEnabled;
                ckbIndentHighFreq.Enabled = bEnabled;
                btnSaveIndentationData.Enabled = bEnabled;

                btnCompression.Enabled = bEnabled;
                btnSaveCompressionData.Enabled = bEnabled;

            }
            else
            {
                btnIndentation.Enabled = bEnabled;
                btnSaveIndentationData.Enabled = bEnabled;

                cmbCompressSpeed.Enabled = bEnabled;
                cmbCompressType.Enabled = bEnabled;
                if (cmbCompressType.SelectedIndex == 0)
                {
                    txbCompressDeformation.Enabled = bEnabled;
                }
                else if (cmbCompressType.SelectedIndex == 1)
                {
                    txbCompressDistance.Enabled = bEnabled;
                }
                else
                {
                    txbCompressForceThreshold.Enabled = bEnabled;
                }
                txbCompressDelay.Enabled = bEnabled;
                txbCompressWithdrawDisp.Enabled = bEnabled;
                txbCompressWithdrawDelay.Enabled = bEnabled;
                ckbCompressHighFreq.Enabled = bEnabled;
                btnSaveCompressionData.Enabled = bEnabled;
                //Sam
                btIndentationCalibration.Enabled = bEnabled;
            }
        }

        #endregion

        #region Control Initialize

        public void GeneralControlInitialize()
        {
            try
            {
                strSensitivityLast_F = txbSensitivity_F.Text;
                strSensitivityLast_d = txbSensitivity_d.Text;
                strStiffnessLast = txbStiffness.Text;
                strSensorForceThresholdLast = txbSensorForceThreshold.Text;
                bCkbSafetyModeCheckedChangedLast = ckbSafetyMode.Checked;

                strMaxDurationLast = txbMaxDuration.Text;
                bCkbAutoScaleCheckedChangedLast_F = ckbAutoScale_F.Checked;
                strMinForceLast_F = txbMinForce_F.Text;
                strMaxForceLast_F = txbMaxForce_F.Text;
                bCkbAutoScaleCheckedChangedLast_d = ckbAutoScale_d.Checked;
                strMinForceLast_d = txbMinForce_d.Text;
                strMaxForceLast_d = txbMaxForce_d.Text;

                bContinuous = false;
                bStep = true;
                btnContinuous.BackColor = bContinuous ? Color.DodgerBlue : Color.Transparent;
                btnStep.BackColor = bStep ? Color.DodgerBlue : Color.Transparent;
                btnXN.Enabled = btnXP.Enabled = bStep;
                btnYN.Enabled = btnYP.Enabled = bStep;
                btnZN.Enabled = btnZP.Enabled = bStep;

                lblContinuousSpeed.Text = "Speed " + iContinuousSpeed.ToString();
                iBtnSteps = int.Parse(dSAF_OL[iContinuousSpeed - 1, 0].ToString());
                uiAmplitude = (uint)dSAF_OL[iContinuousSpeed - 1, 1];
                mGetFunctions.uiFrequencyMax = uiFrequencyMax = (uint)dSAF_OL[iContinuousSpeed - 1, 2];
                mGetFunctions.dDistanceMax = dDistanceMax = dDistance_CL[iContinuousSpeed - 1];

                lblStep.Text = dStep_CL[iStepSpeed - 1].ToString() + " um";
            }
            catch (Exception e)
            {

            }
        }

        public void zgcForceSensorInitialize()
        {
            if (!bCN)
            {
                zgcForceSensor_F.GraphPane.Title.Text = "Force - Time";
            }
            else
            {
                zgcForceSensor_F.GraphPane.Title.Text = "力 - 时间";
            }
            zgcForceSensor_F.GraphPane.Title.FontSpec.Size = 20f;

            if (!bCN)
            {
                zgcForceSensor_F.GraphPane.XAxis.Title.Text = "Time(s)";//X轴标题
            }
            else
            {
                zgcForceSensor_F.GraphPane.XAxis.Title.Text = "时间(s)";//X轴标题
            }
            zgcForceSensor_F.GraphPane.XAxis.Title.FontSpec.Size = 19f;
            zgcForceSensor_F.GraphPane.XAxis.Type = ZedGraph.AxisType.LinearAsOrdinal;
            zgcForceSensor_F.GraphPane.XAxis.Scale.MaxAuto = true;
            zgcForceSensor_F.GraphPane.XAxis.CrossAuto = true;//容许x轴的自动放大或缩小

            if (!bCN)
            {
                zgcForceSensor_F.GraphPane.YAxis.Title.Text = "Force(uN)";//Y轴标题
            }
            else
            {
                zgcForceSensor_F.GraphPane.YAxis.Title.Text = "力(uN)";//Y轴标题
            }
            zgcForceSensor_F.GraphPane.YAxis.Title.FontSpec.Size = 19f;


            if (!bCN)
            {
                zgcForceSensor_d.GraphPane.Title.Text = "Displacement - Time";
            }
            else
            {
                zgcForceSensor_d.GraphPane.Title.Text = "位移 - 时间";
            }
            zgcForceSensor_d.GraphPane.Title.FontSpec.Size = 20f;

            if (!bCN)
            {
                zgcForceSensor_d.GraphPane.XAxis.Title.Text = "Time(s)";//X轴标题
            }
            else
            {
                zgcForceSensor_d.GraphPane.XAxis.Title.Text = "时间(s)";//X轴标题
            }
            zgcForceSensor_d.GraphPane.XAxis.Title.FontSpec.Size = 19f;
            zgcForceSensor_d.GraphPane.XAxis.Type = ZedGraph.AxisType.LinearAsOrdinal;
            zgcForceSensor_d.GraphPane.XAxis.Scale.MaxAuto = true;
            zgcForceSensor_d.GraphPane.XAxis.CrossAuto = true;//容许x轴的自动放大或缩小

            if (!bCN)
            {
                zgcForceSensor_d.GraphPane.YAxis.Title.Text = "Displacement(nm)";//Y轴标题
            }
            else
            {
                zgcForceSensor_d.GraphPane.YAxis.Title.Text = "位移(nm)";//Y轴标题
            }
            zgcForceSensor_d.GraphPane.YAxis.Title.FontSpec.Size = 19f;
        }

        public void zgcIndentationInitialize()
        {
            if (!bCN)
            {
                zgcIndentation_ori.GraphPane.Title.Text = "Force - Displacement";
            }
            else
            {
                zgcIndentation_ori.GraphPane.Title.Text = "力 - 位移";
            }
            zgcIndentation_ori.GraphPane.Title.FontSpec.Size = 20f;

            if (!bCN)
            {
                zgcIndentation_ori.GraphPane.XAxis.Title.Text = "Displacement(nm)";//X轴标题
            }
            else
            {
                zgcIndentation_ori.GraphPane.XAxis.Title.Text = "位移(nm)";//X轴标题
            }
            zgcIndentation_ori.GraphPane.XAxis.Title.FontSpec.Size = 19f;
            //zgcIndentation.GraphPane.XAxis.Type = ZedGraph.AxisType.LinearAsOrdinal;
            zgcIndentation_ori.GraphPane.XAxis.Scale.MaxAuto = true;
            zgcIndentation_ori.GraphPane.XAxis.CrossAuto = true;//容许x轴的自动放大或缩小

            if (!bCN)
            {
                zgcIndentation_ori.GraphPane.YAxis.Title.Text = "Force(uN)";//Y轴标题
            }
            else
            {
                zgcIndentation_ori.GraphPane.YAxis.Title.Text = "力(uN)";//Y轴标题
            }
            zgcIndentation_ori.GraphPane.YAxis.Title.FontSpec.Size = 19f;



            if (!bCN)
            {
                zgcIndentation.GraphPane.Title.Text = "Force - Deformation";
            }
            else
            {
                zgcIndentation.GraphPane.Title.Text = "力 - 形变";
            }
            zgcIndentation.GraphPane.Title.FontSpec.Size = 20f;

            if (!bCN)
            {
                zgcIndentation.GraphPane.XAxis.Title.Text = "Deformation(nm)";//X轴标题
            }
            else
            {
                zgcIndentation.GraphPane.XAxis.Title.Text = "形变(nm)";//X轴标题
            }
            zgcIndentation.GraphPane.XAxis.Title.FontSpec.Size = 19f;
            //zgcIndentation.GraphPane.XAxis.Type = ZedGraph.AxisType.LinearAsOrdinal;
            zgcIndentation.GraphPane.XAxis.Scale.MaxAuto = true;
            zgcIndentation.GraphPane.XAxis.CrossAuto = true;//容许x轴的自动放大或缩小

            if (!bCN)
            {
                zgcIndentation.GraphPane.YAxis.Title.Text = "Force(uN)";//Y轴标题
            }
            else
            {
                zgcIndentation.GraphPane.YAxis.Title.Text = "力(uN)";//Y轴标题
            }
            zgcIndentation.GraphPane.YAxis.Title.FontSpec.Size = 19f;
        }

        public void zgcCompressionInitialize()
        {
            if (!bCN)
            {
                zgcCompression_ori.GraphPane.Title.Text = "Force - Displacement";
            }
            else
            {
                zgcCompression_ori.GraphPane.Title.Text = "力 - 位移";
            }
            zgcCompression_ori.GraphPane.Title.FontSpec.Size = 20f;

            if (!bCN)
            {
                zgcCompression_ori.GraphPane.XAxis.Title.Text = "Displacement(nm)";//X轴标题
            }
            else
            {
                zgcCompression_ori.GraphPane.XAxis.Title.Text = "位移(nm)";//X轴标题
            }
            zgcCompression_ori.GraphPane.XAxis.Title.FontSpec.Size = 19f;
            //zgcCompression_ori.GraphPane.XAxis.Type = ZedGraph.AxisType.LinearAsOrdinal;
            zgcCompression_ori.GraphPane.XAxis.Scale.MaxAuto = true;
            zgcCompression_ori.GraphPane.XAxis.CrossAuto = true;//容许x轴的自动放大或缩小

            if (!bCN)
            {
                zgcCompression_ori.GraphPane.YAxis.Title.Text = "Force(uN)";//Y轴标题
            }
            else
            {
                zgcCompression_ori.GraphPane.YAxis.Title.Text = "力(uN)";//Y轴标题
            }
            zgcCompression_ori.GraphPane.YAxis.Title.FontSpec.Size = 19f;



            if (!bCN)
            {
                zgcCompress.GraphPane.Title.Text = "Force - Deformation";
            }
            else
            {
                zgcCompress.GraphPane.Title.Text = "力 - 形变";
            }
            zgcCompress.GraphPane.Title.FontSpec.Size = 20f;

            if (!bCN)
            {
                zgcCompress.GraphPane.XAxis.Title.Text = "Deformation(nm)";//X轴标题
            }
            else
            {
                zgcCompress.GraphPane.XAxis.Title.Text = "形变(nm)";//X轴标题
            }
            zgcCompress.GraphPane.XAxis.Title.FontSpec.Size = 19f;
            //zgcCompress.GraphPane.XAxis.Type = ZedGraph.AxisType.LinearAsOrdinal;
            zgcCompress.GraphPane.XAxis.Scale.MaxAuto = true;
            zgcCompress.GraphPane.XAxis.CrossAuto = true;//容许x轴的自动放大或缩小

            if (!bCN)
            {
                zgcCompress.GraphPane.YAxis.Title.Text = "Force(uN)";//Y轴标题
            }
            else
            {
                zgcCompress.GraphPane.YAxis.Title.Text = "力(uN)";//Y轴标题
            }
            zgcCompress.GraphPane.YAxis.Title.FontSpec.Size = 19f;
        }

        #endregion

        #region Load And Update lfd File

        public void LoadStiffnessCalibration()
        {
            if (File.Exists(strStiffnessCalibrationPath))
            {
                using (FileStream fsReadStiffnessCalibration = new FileStream(strStiffnessCalibrationPath, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //创建缓冲区的大小
                    byte[] bufStiffnessCalibration = new byte[fsReadStiffnessCalibration.Length];
                    //这次读取实际读到的字节数
                    int r = fsReadStiffnessCalibration.Read(bufStiffnessCalibration, 0, bufStiffnessCalibration.Length);

                    //将字节数组转换成字符串
                    strStiffnessCalibration = Encoding.Default.GetString(bufStiffnessCalibration).Split(',');

                    if (strStiffnessCalibration.Length == 9)
                    {
                        try
                        {
                            dStepFirst = Convert.ToDouble(strStiffnessCalibration[0]);
                            iDelayFirst = int.Parse(strStiffnessCalibration[1]);
                            dForceFirst = Convert.ToDouble(strStiffnessCalibration[2]);
                            dStepSecond = Convert.ToDouble(strStiffnessCalibration[3]);
                            iDelaySecond = int.Parse(strStiffnessCalibration[4]);
                            dForceSecond = Convert.ToDouble(strStiffnessCalibration[5]);
                            uiStepThird = uint.Parse(strStiffnessCalibration[6]);
                            iDelayThird = int.Parse(strStiffnessCalibration[7]);
                            dForceThird = Convert.ToDouble(strStiffnessCalibration[8]);
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        public void LoadAxisDirection()
        {
            if (File.Exists(strAxisDirectionPath))
            {
                using (FileStream fsReadAxisDirection = new FileStream(strAxisDirectionPath, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //创建缓冲区的大小
                    byte[] bufAxisDirection = new byte[fsReadAxisDirection.Length];
                    //这次读取实际读到的字节数
                    int r = fsReadAxisDirection.Read(bufAxisDirection, 0, bufAxisDirection.Length);

                    //将字节数组转换成字符串
                    strAxisDirection = Encoding.Default.GetString(bufAxisDirection).Split(',');
                }
            }
        }

        private void LoadControllerID()
        {
            if (File.Exists(strControllerIDPath))
            {
                using (FileStream fsReadControllerID = new FileStream(strControllerIDPath, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //创建缓冲区的大小
                    byte[] bufControllerID = new byte[fsReadControllerID.Length];
                    //这次读取实际读到的字节数
                    int r = fsReadControllerID.Read(bufControllerID, 0, bufControllerID.Length);

                    //将字节数组转换成字符串
                    strControllerID = Encoding.Default.GetString(bufControllerID).Split(',');

                    //if (strControllerID.Length == 3)
                    //{
                    //    try
                    //    {
                    //        strID[0] = strControllerID[0];
                    //        strID[1] = strControllerID[1];
                    //        strID[2] = strControllerID[2];
                    //    }
                    //    catch (Exception ex)
                    //    { }
                    //}
                }
            }
        }

        public void LoadSensorDirection()
        {
            if (File.Exists(strSensorDirectionPath))
            {
                using (FileStream fsReadSensorDirection = new FileStream(strSensorDirectionPath, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //创建缓冲区的大小
                    byte[] bufSensorDirection = new byte[fsReadSensorDirection.Length];
                    //这次读取实际读到的字节数
                    int r = fsReadSensorDirection.Read(bufSensorDirection, 0, bufSensorDirection.Length);

                    //将字节数组转换成字符串
                    string[] strSensorDirection = new string[4];
                    strSensorDirection = Encoding.Default.GetString(bufSensorDirection).Split(',');

                    if (strSensorDirection.Length == 4)
                    {
                        try
                        {
                            iDirection_F = int.Parse(strSensorDirection[0]);
                            strSuffix_F = strSensorDirection[1];
                            iDirection_d = int.Parse(strSensorDirection[2]);
                            strSuffix_d = strSensorDirection[3];
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        public void LoadRangeSettings()
        {
            if (File.Exists(strRangeSettingsPath))
            {
                using (FileStream fsReadRangeSettings = new FileStream(strRangeSettingsPath, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //创建缓冲区的大小
                    byte[] bufRangeSettings = new byte[fsReadRangeSettings.Length];
                    //这次读取实际读到的字节数
                    int r = fsReadRangeSettings.Read(bufRangeSettings, 0, bufRangeSettings.Length);

                    //将字节数组转换成字符串
                    string[] strRangeSettings = new string[4];
                    strRangeSettings = Encoding.Default.GetString(bufRangeSettings).Split(',');

                    if (strRangeSettings.Length == 4)
                    {
                        try
                        {
                            dIndentDistanceMin = Convert.ToDouble(strRangeSettings[0]);
                            dIndentDistanceMax = Convert.ToDouble(strRangeSettings[1]);
                            dIndentForceThresholdMin = Convert.ToDouble(strRangeSettings[2]);
                            dIndentForceThresholdMax = Convert.ToDouble(strRangeSettings[3]);
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        public void LoadCurveSelection()
        {
            if (File.Exists(strCurveSelectionPath))
            {
                using (FileStream fsReadCurveSelection = new FileStream(strCurveSelectionPath, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //创建缓冲区的大小
                    byte[] bufCurveSelection = new byte[fsReadCurveSelection.Length];
                    //这次读取实际读到的字节数
                    int r = fsReadCurveSelection.Read(bufCurveSelection, 0, bufCurveSelection.Length);

                    //将字节数组转换成字符串
                    string[] strCurveSelection = new string[2];
                    strCurveSelection = Encoding.Default.GetString(bufCurveSelection).Split(',');

                    if (strCurveSelection.Length == 2)
                    {
                        try
                        {
                            bSample = Convert.ToBoolean(int.Parse(strCurveSelection[0]));
                            bScanner = Convert.ToBoolean(int.Parse(strCurveSelection[1]));

                            //if (bSample)
                            //{
                            //    zgcIndentation_ori.BringToFront();
                            //    zgcIndentation.SendToBack();
                            //}

                            //if (bScanner)
                            //{
                            //    zgcIndentation_ori.SendToBack();
                            //    zgcIndentation.BringToFront();
                            //}
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        private void LoadDACRange()
        {
            if (File.Exists(strDACRangePath))
            {
                using (FileStream fsReadDACRange = new FileStream(strDACRangePath, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //创建缓冲区的大小
                    byte[] bufDACRange = new byte[fsReadDACRange.Length];
                    //这次读取实际读到的字节数
                    int r = fsReadDACRange.Read(bufDACRange, 0, bufDACRange.Length);

                    //将字节数组转换成字符串
                    string[] strDACRange = new string[2];
                    strDACRange = Encoding.Default.GetString(bufDACRange).Split(',');

                    if (strDACRange.Length == 2)
                    {
                        try
                        {
                            dDACMin = Convert.ToDouble(strDACRange[0]);
                            dDACMax = Convert.ToDouble(strDACRange[1]);

                            dDACMin_Zoom = dDACMin /*+ (dDACMax - dDACMin) * 0.1*/;
                            dDACMax_Zoom = dDACMax /*- (dDACMax - dDACMin) * 0.1*/;

                            P_ori = (decimal)Convert.ToDouble(((uiMaxADCValue - uiMinADCValue) / (dDACMax - dDACMin)).ToString("f4")) / u;

                            txbP_ori.Text = P_ori.ToString();
                            txbI_ori.Text = I_ori.ToString();
                            txbD_ori.Text = D_ori.ToString();
                        }
                        catch (Exception ex)
                        {}
                    }
                }
            }
        }

        public void LoadSensorDefaultValue()
        {
            if (File.Exists(strSensorDefaultValuePath))
            {
                using (FileStream fsReadSensorDefaultValue = new FileStream(strSensorDefaultValuePath, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //创建缓冲区的大小
                    byte[] bufSensorDefaultValue = new byte[fsReadSensorDefaultValue.Length];
                    //这次读取实际读到的字节数
                    int r = fsReadSensorDefaultValue.Read(bufSensorDefaultValue, 0, bufSensorDefaultValue.Length);

                    //将字节数组转换成字符串
                    strSensorDefaultValue = Encoding.Default.GetString(bufSensorDefaultValue).Split(',');

                    try
                    {
                        cmbComport.SelectedIndex = int.Parse(strSensorDefaultValue[0]);
                    }
                    catch (Exception ex1)
                    { }

                    try
                    {
                        cmbSensorVersion.SelectedIndex = int.Parse(strSensorDefaultValue[1]);
                    }
                    catch (Exception ex2)
                    { }

                    try
                    {
                        ckbLeft.Checked = Convert.ToBoolean(int.Parse(strSensorDefaultValue[18]));
                        if (ckbLeft.Checked)
                        {
                            bLeft = true;
                            iXP = int.Parse(strAxisDirection[0]);
                            iYP = int.Parse(strAxisDirection[2]);
                            iZP = int.Parse(strAxisDirection[4]);
                            iXN = int.Parse(strAxisDirection[0]) * (-1);
                            iYN = int.Parse(strAxisDirection[2]) * (-1);
                            iZN = int.Parse(strAxisDirection[4]) * (-1);
                            iClockwise = 1;
                            iAnticlockwise = -1;
                        }
                        else
                        {
                            bLeft = false;
                            iXP = int.Parse(strAxisDirection[1]);
                            iYP = int.Parse(strAxisDirection[3]);
                            iZP = int.Parse(strAxisDirection[5]);
                            iXN = int.Parse(strAxisDirection[1]) * (-1);
                            iYN = int.Parse(strAxisDirection[3]) * (-1);
                            iZN = int.Parse(strAxisDirection[5]) * (-1);
                            iClockwise = -1;
                            iAnticlockwise = 1;
                        }
                    }
                    catch (Exception ex3)
                    { }
                }
            }
        }

        public void UpdateSensorDefaultValue()
        {
            using (FileStream fsWriteSensorDefaultValue = new FileStream(strSensorDefaultValuePath, FileMode.Create))
            {
                string str = "";
                strSensorDefaultValue[0] = cmbComport.SelectedIndex.ToString();
                strSensorDefaultValue[1] = cmbSensorVersion.SelectedIndex.ToString();
                if (cmbSensorVersion.SelectedIndex == 0)
                {
                    strSensorDefaultValue[2] = txbResolution.Text;
                    strSensorDefaultValue[3] = txbMeasureRange.Text;
                    strSensorDefaultValue[4] = txbSensitivity_F.Text;
                    strSensorDefaultValue[5] = txbSensitivity_d.Text;
                    strSensorDefaultValue[6] = txbStiffness.Text;
                    strSensorDefaultValue[7] = txbExtraStiffness.Text;
                    strSensorDefaultValue[8] = txbSensorForceThreshold.Text;
                    strSensorDefaultValue[9] = Convert.ToInt32(ckbSafetyMode.Checked).ToString();
                }
                else
                {
                    strSensorDefaultValue[10] = txbResolution.Text;
                    strSensorDefaultValue[11] = txbMeasureRange.Text;
                    strSensorDefaultValue[12] = txbSensitivity_F.Text;
                    strSensorDefaultValue[13] = txbSensitivity_d.Text;
                    strSensorDefaultValue[14] = txbStiffness.Text;
                    strSensorDefaultValue[15] = txbExtraStiffness.Text;
                    strSensorDefaultValue[16] = txbSensorForceThreshold.Text;
                    strSensorDefaultValue[17] = Convert.ToInt32(ckbSafetyMode.Checked).ToString();
                }
                strSensorDefaultValue[18] = Convert.ToInt32(ckbLeft.Checked).ToString();

                for (int i = 0; i < strSensorDefaultValue.Length; i++)
                {
                    if (i < strSensorDefaultValue.Length - 1)
                    {
                        str += strSensorDefaultValue[i] + ",";
                    }
                    else
                    {
                        str += strSensorDefaultValue[i];
                    }
                }

                //将字符串转换成字节数组
                byte[] bufSensorDefaultValue = System.Text.Encoding.GetEncoding("gbk").GetBytes(str);
                fsWriteSensorDefaultValue.Write(bufSensorDefaultValue, 0, bufSensorDefaultValue.Length);
            }
        }

        public void LoadZgcDefaultValue()
        {
            if (File.Exists(strZgcDefaultValuePath))
            {
                using (FileStream fsReadZgcDefaultValue = new FileStream(strZgcDefaultValuePath, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //创建缓冲区的大小
                    byte[] bufZgcDefaultValue = new byte[fsReadZgcDefaultValue.Length];
                    //这次读取实际读到的字节数
                    int r = fsReadZgcDefaultValue.Read(bufZgcDefaultValue, 0, bufZgcDefaultValue.Length);

                    //将字节数组转换成字符串
                    strZgcDefaultValue = Encoding.Default.GetString(bufZgcDefaultValue).Split(',');

                    try
                    {
                        txbMaxDuration.Text = strZgcDefaultValue[0] == "" ? "5" : strZgcDefaultValue[0];
                        ckbAutoScale_F.Checked = Convert.ToBoolean(int.Parse(strZgcDefaultValue[1]));
                        txbMinForce_F.Text = strZgcDefaultValue[2];
                        txbMaxForce_F.Text = strZgcDefaultValue[3];
                        ckbAutoScale_d.Checked = Convert.ToBoolean(int.Parse(strZgcDefaultValue[4]));
                        txbMinForce_d.Text = strZgcDefaultValue[5];
                        txbMaxForce_d.Text = strZgcDefaultValue[6];

                        iMaxDurationTime = int.Parse(txbMaxDuration.Text);

                        if (ckbAutoScale_F.Checked)
                        {
                            txbMinForce_F.Enabled = false;
                            txbMaxForce_F.Enabled = false;

                            zgcForceSensor_F.GraphPane.YAxis.Scale.MaxAuto = true;
                            zgcForceSensor_F.GraphPane.YAxis.Scale.MinAuto = true;
                            zgcForceSensor_F.AxisChange();//画到zedGraphControl1控件中
                            zgcForceSensor_F.Refresh();//重新刷新
                        }
                        else
                        {
                            txbMinForce_F.Enabled = true;
                            txbMaxForce_F.Enabled = true;

                            zgcForceSensor_F.GraphPane.YAxis.Scale.Min = Convert.ToDouble(txbMinForce_F.Text);
                            zgcForceSensor_F.GraphPane.YAxis.Scale.Max = Convert.ToDouble(txbMaxForce_F.Text);
                            zgcForceSensor_F.AxisChange();//画到zedGraphControl1控件中
                            zgcForceSensor_F.Refresh();//重新刷新
                        }

                        if (ckbAutoScale_d.Checked)
                        {
                            txbMinForce_d.Enabled = false;
                            txbMaxForce_d.Enabled = false;

                            zgcForceSensor_d.GraphPane.YAxis.Scale.MaxAuto = true;
                            zgcForceSensor_d.GraphPane.YAxis.Scale.MinAuto = true;
                            zgcForceSensor_d.AxisChange();//画到zedGraphControl1控件中
                            zgcForceSensor_d.Refresh();//重新刷新
                        }
                        else
                        {
                            txbMinForce_d.Enabled = true;
                            txbMaxForce_d.Enabled = true;

                            zgcForceSensor_d.GraphPane.YAxis.Scale.Min = Convert.ToDouble(txbMinForce_d.Text);
                            zgcForceSensor_d.GraphPane.YAxis.Scale.Max = Convert.ToDouble(txbMaxForce_d.Text);
                            zgcForceSensor_d.AxisChange();//画到zedGraphControl1控件中
                            zgcForceSensor_d.Refresh();//重新刷新
                        }
                    }
                    catch (Exception ex)
                    { }
                }
            }
        }

        public void UpdateZgcDefaultValue()
        {
            using (FileStream fsWriteZgcDefaultValue = new FileStream(strZgcDefaultValuePath, FileMode.Create))
            {
                string str = "";
                strZgcDefaultValue[0] = txbMaxDuration.Text;
                strZgcDefaultValue[1] = Convert.ToInt32(ckbAutoScale_F.Checked).ToString();
                strZgcDefaultValue[2] = txbMinForce_F.Text;
                strZgcDefaultValue[3] = txbMaxForce_F.Text;
                strZgcDefaultValue[4] = Convert.ToInt32(ckbAutoScale_d.Checked).ToString();
                strZgcDefaultValue[5] = txbMinForce_d.Text;
                strZgcDefaultValue[6] = txbMaxForce_d.Text;

                for (int i = 0; i < strZgcDefaultValue.Length; i++)
                {
                    if (i < strZgcDefaultValue.Length - 1)
                    {
                        str += strZgcDefaultValue[i] + ",";
                    }
                    else
                    {
                        str += strZgcDefaultValue[i];
                    }
                }

                //将字符串转换成字节数组
                byte[] bufZgcDefaultValue = System.Text.Encoding.GetEncoding("gbk").GetBytes(str);
                fsWriteZgcDefaultValue.Write(bufZgcDefaultValue, 0, bufZgcDefaultValue.Length);
            }
        }

        public void LoadPIDDefaultValue()
        {
            if (File.Exists(strPIDDefaultValuePath))
            {
                using (FileStream fsReadPIDDefaultValue = new FileStream(strPIDDefaultValuePath, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //创建缓冲区的大小
                    byte[] bufPIDDefaultValue = new byte[fsReadPIDDefaultValue.Length];
                    //这次读取实际读到的字节数
                    int r = fsReadPIDDefaultValue.Read(bufPIDDefaultValue, 0, bufPIDDefaultValue.Length);

                    //将字节数组转换成字符串
                    strPIDDefaultValue = Encoding.Default.GetString(bufPIDDefaultValue).Split(',');

                    try
                    {
                        txbP.Text = strPIDDefaultValue[0];
                        txbI.Text = strPIDDefaultValue[1];
                        txbD.Text = strPIDDefaultValue[2];

                        if (txbP.Text != "" && txbP.Text != null)
                        {
                            P = (decimal)Convert.ToDouble(txbP.Text);
                        }
                        else
                        {
                            P = P_ori;
                        }

                        if (txbI.Text != "" && txbI.Text != null)
                        {
                            I = (decimal)Convert.ToDouble(txbI.Text);
                        }
                        else
                        {
                            I = I_ori;
                        }

                        if (txbD.Text != "" && txbD.Text != null)
                        {
                            D = (decimal)Convert.ToDouble(txbD.Text);
                        }
                        else
                        {
                            D = D_ori;
                        }
                    }
                    catch (Exception ex)
                    { }
                }
            }
        }

        public void UpdatePIDDefaultValue()
        {
            using (FileStream fsWritePIDDefaultValue = new FileStream(strPIDDefaultValuePath, FileMode.Create))
            {
                string str = "";
                strPIDDefaultValue[0] = txbP.Text;
                strPIDDefaultValue[1] = txbI.Text;
                strPIDDefaultValue[2] = txbD.Text;

                for (int i = 0; i < strPIDDefaultValue.Length; i++)
                {
                    if (i < strPIDDefaultValue.Length - 1)
                    {
                        str += strPIDDefaultValue[i] + ",";
                    }
                    else
                    {
                        str += strPIDDefaultValue[i];
                    }
                }

                //将字符串转换成字节数组
                byte[] bufPIDDefaultValue = System.Text.Encoding.GetEncoding("gbk").GetBytes(str);
                fsWritePIDDefaultValue.Write(bufPIDDefaultValue, 0, bufPIDDefaultValue.Length);
            }
        }

        public void LoadApproachIndentDefaultValue()
        {
            if (File.Exists(strApproachIndentDefaultValuePath))
            {
                using (FileStream fsReadApproachIndentDefaultValue = new FileStream(strApproachIndentDefaultValuePath, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //创建缓冲区的大小
                    byte[] bufApproachIndentDefaultValue = new byte[fsReadApproachIndentDefaultValue.Length];
                    //这次读取实际读到的字节数
                    int r = fsReadApproachIndentDefaultValue.Read(bufApproachIndentDefaultValue, 0, bufApproachIndentDefaultValue.Length);

                    //将字节数组转换成字符串
                    strApproachIndentDefaultValue = Encoding.Default.GetString(bufApproachIndentDefaultValue).Split(',');

                    try
                    {
                        cmbApproachSpeed.SelectedIndex = int.Parse(strApproachIndentDefaultValue[0]);
                    }
                    catch (Exception ex)
                    { }

                    try
                    {
                        cmbApproachMotionDirection.SelectedIndex = int.Parse(strApproachIndentDefaultValue[1]);
                    }
                    catch (Exception ex)
                    { }

                    try
                    {
                        txbApproachForceThreshold.Text = strApproachIndentDefaultValue[2];
                        txbApproachRetreatDistance.Text = strApproachIndentDefaultValue[3];
                    }
                    catch (Exception ex)
                    { }



                    try
                    {
                        cmbIndentSpeed.SelectedIndex = int.Parse(strApproachIndentDefaultValue[4]);
                    }
                    catch (Exception ex)
                    { }

                    try
                    {
                        cmbIndentType.SelectedIndex = int.Parse(strApproachIndentDefaultValue[5]);
                    }
                    catch (Exception ex)
                    { }

                    try
                    {
                        txbIndentDeformation.Text = strApproachIndentDefaultValue[6];
                        txbIndentDistance.Text = strApproachIndentDefaultValue[7];
                        txbIndentForceThreshold.Text = strApproachIndentDefaultValue[8];
                        txbIndentDelay.Text = strApproachIndentDefaultValue[9];
                        txbIndentWithdrawDisp.Text = strApproachIndentDefaultValue[10];
                        txbIndentWithdrawDelay.Text = strApproachIndentDefaultValue[11];
                    }
                    catch (Exception ex)
                    { }

                    try
                    {
                        ckbIndentHighFreq.Checked = Convert.ToBoolean(int.Parse(strApproachIndentDefaultValue[12]));
                    }
                    catch (Exception ex)
                    { }



                    try
                    {
                        cmbCompressSpeed.SelectedIndex = int.Parse(strApproachIndentDefaultValue[13]);
                    }
                    catch (Exception ex)
                    { }

                    try
                    {
                        cmbCompressType.SelectedIndex = int.Parse(strApproachIndentDefaultValue[14]);
                    }
                    catch (Exception ex)
                    { }

                    try
                    {
                        txbCompressDeformation.Text = strApproachIndentDefaultValue[15];
                        txbCompressDistance.Text = strApproachIndentDefaultValue[16];
                        txbCompressForceThreshold.Text = strApproachIndentDefaultValue[17];
                        txbCompressDelay.Text = strApproachIndentDefaultValue[18];
                        txbCompressWithdrawDisp.Text = strApproachIndentDefaultValue[19];
                        txbCompressWithdrawDelay.Text = strApproachIndentDefaultValue[20];
                    }
                    catch (Exception ex)
                    { }

                    try
                    {
                        ckbCompressHighFreq.Checked = Convert.ToBoolean(int.Parse(strApproachIndentDefaultValue[21]));
                    }
                    catch (Exception ex)
                    { }
                }
            }
        }

        public void UpdateApproachIndentDefaultValue()
        {
            using (FileStream fsWriteApproachIndentDefaultValue = new FileStream(strApproachIndentDefaultValuePath, FileMode.Create))
            {
                string str = "";
                strApproachIndentDefaultValue[0] = cmbApproachSpeed.SelectedIndex.ToString();
                strApproachIndentDefaultValue[1] = cmbApproachMotionDirection.SelectedIndex.ToString();
                strApproachIndentDefaultValue[2] = txbApproachForceThreshold.Text;
                strApproachIndentDefaultValue[3] = txbApproachRetreatDistance.Text;

                strApproachIndentDefaultValue[4] = cmbIndentSpeed.SelectedIndex.ToString();
                strApproachIndentDefaultValue[5] = cmbIndentType.SelectedIndex.ToString();
                strApproachIndentDefaultValue[6] = txbIndentDeformation.Text;
                strApproachIndentDefaultValue[7] = txbIndentDistance.Text;
                strApproachIndentDefaultValue[8] = txbIndentForceThreshold.Text;
                strApproachIndentDefaultValue[9] = txbIndentDelay.Text;
                strApproachIndentDefaultValue[10] = txbIndentWithdrawDisp.Text;
                strApproachIndentDefaultValue[11] = txbIndentWithdrawDelay.Text;
                strApproachIndentDefaultValue[12] = Convert.ToInt32(ckbIndentHighFreq.Checked).ToString();

                strApproachIndentDefaultValue[13] = cmbCompressSpeed.SelectedIndex.ToString();
                strApproachIndentDefaultValue[14] = cmbCompressType.SelectedIndex.ToString();
                strApproachIndentDefaultValue[15] = txbCompressDeformation.Text;
                strApproachIndentDefaultValue[16] = txbCompressDistance.Text;
                strApproachIndentDefaultValue[17] = txbCompressForceThreshold.Text;
                strApproachIndentDefaultValue[18] = txbCompressDelay.Text;
                strApproachIndentDefaultValue[19] = txbCompressWithdrawDisp.Text;
                strApproachIndentDefaultValue[20] = txbCompressWithdrawDelay.Text;
                strApproachIndentDefaultValue[21] = Convert.ToInt32(ckbCompressHighFreq.Checked).ToString();

                for (int i = 0; i < strApproachIndentDefaultValue.Length; i++)
                {
                    if (i < strApproachIndentDefaultValue.Length - 1)
                    {
                        str += strApproachIndentDefaultValue[i] + ",";
                    }
                    else
                    {
                        str += strApproachIndentDefaultValue[i];
                    }
                }

                //将字符串转换成字节数组
                byte[] bufApproachIndentDefaultValue = System.Text.Encoding.GetEncoding("gbk").GetBytes(str);
                fsWriteApproachIndentDefaultValue.Write(bufApproachIndentDefaultValue, 0, bufApproachIndentDefaultValue.Length);
            }
        }

        #endregion

        #region Save Data

        public void StringListToPointList()
        {
            for (int i = 0; i < strHighFreqListExtend.Count; i++)
            {
                string[] strSplit = strHighFreqListExtend[i].Replace(" ", "").Replace("\r\n", " ").Trim().Split(' ');
                //string[] strSplit = Regex.Split(strInput, "0b\r\n", RegexOptions.IgnoreCase);
                for (int m = 0; m < strSplit.Length; m++)
                {
                    if (strSplit[m].Length == 8)
                    {
                        string strAdd = strSplit[m].Substring(0, strSplit[m].Length - 2);
                        if (String.Equals(strSplit[m][strSplit[m].Length - 1].ToString(), strSuffix_F, StringComparison.CurrentCultureIgnoreCase))//忽略大小写
                        {
                            try
                            {
                                double dRead = Convert.ToDouble(Convert.ToInt64(strAdd, 16));
                                double dChange = Convert.ToDouble(dRead) - dInitValue_F;
                                dForce = Convert.ToDouble((dChange * dSensitivity_F * Math.Pow(10, 6)).ToString("f4")) * iDirection_F;//uN
                                bGetPoint = true;
                            }
                            catch (Exception ex)
                            {
                                if (!bCN)
                                {
                                    MessageBox.Show("Data conversion error:" + strAdd, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    MessageBox.Show("数据转换错误：" + strAdd, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }

                        if (String.Equals(strSplit[m][strSplit[m].Length - 1].ToString(), strSuffix_d, StringComparison.CurrentCultureIgnoreCase))//忽略大小写
                        {
                            try
                            {
                                double dRead = Convert.ToDouble(Convert.ToInt64(strAdd, 16));
                                double dChange = Convert.ToDouble(dRead) - dInitValue_d;
                                dDisplacement_um_ori = Convert.ToDouble((dChange * dSensitivity_d * Math.Pow(10, 3)).ToString("f6")) * iDirection_d;//um
                                //dDisplacement_um_Indent = Convert.ToDouble((dChange * dSensitivity_d * Math.Pow(10, 3) - Math.Abs(dForce / dStiffness)).ToString("f4")) * iDirection_d;//um
                                dDisplacement_um_Indent = Convert.ToDouble((dChange * dSensitivity_d * Math.Pow(10, 3) * iDirection_d - dForce / dStiffness).ToString("f6"));//um
                                dDisplacement_nm_ori = dDisplacement_um_ori * 1000.0;//nm
                                dDisplacement_nm_Indent = dDisplacement_um_Indent * 1000.0;//nm
                                bGetPoint = false;
                            }
                            catch (Exception ex)
                            {
                                if (!bCN)
                                {
                                    MessageBox.Show("Data conversion error:" + strAdd, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    MessageBox.Show("数据转换错误：" + strAdd, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }

                        if (bGetPoint)
                        {
                            double x1 = (double)((decimal)dDisplacement_nm_ori - (decimal)dStartDistance_ori);
                            double x2 = (double)((decimal)dDisplacement_nm_Indent - (decimal)dStartDistance_Indent);
                            double y = dForce;
                            listIndentExtend_ori.Add(x1, y);
                            listIndentExtend.Add(x2, y);
                            listIndentNoMissing_ori.Add(x1, y);
                            listIndentNoMissing.Add(x2, y);
                        }
                    }
                }
            }

            for (int i = 0; i < strHighFreqListWithdraw.Count; i++)
            {
                string[] strSplit = strHighFreqListWithdraw[i].Replace(" ", "").Replace("\r\n", " ").Trim().Split(' ');
                //string[] strSplit = Regex.Split(strInput, "0b\r\n", RegexOptions.IgnoreCase);
                for (int m = 0; m < strSplit.Length; m++)
                {
                    if (strSplit[m].Length == 8)
                    {
                        string strAdd = strSplit[m].Substring(0, strSplit[m].Length - 2);
                        if (String.Equals(strSplit[m][strSplit[m].Length - 1].ToString(), strSuffix_F, StringComparison.CurrentCultureIgnoreCase))//忽略大小写
                        {
                            try
                            {
                                double dRead = Convert.ToDouble(Convert.ToInt64(strAdd, 16));
                                double dChange = Convert.ToDouble(dRead) - dInitValue_F;
                                dForce = Convert.ToDouble((dChange * dSensitivity_F * Math.Pow(10, 6)).ToString("f4")) * iDirection_F;//uN
                                bGetPoint = true;
                            }
                            catch (Exception ex)
                            {
                                if (!bCN)
                                {
                                    MessageBox.Show("Data conversion error:" + strAdd, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    MessageBox.Show("数据转换错误：" + strAdd, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }

                        if (String.Equals(strSplit[m][strSplit[m].Length - 1].ToString(), strSuffix_d, StringComparison.CurrentCultureIgnoreCase))//忽略大小写
                        {
                            try
                            {
                                double dRead = Convert.ToDouble(Convert.ToInt64(strAdd, 16));
                                double dChange = Convert.ToDouble(dRead) - dInitValue_d;
                                dDisplacement_um_ori = Convert.ToDouble((dChange * dSensitivity_d * Math.Pow(10, 3)).ToString("f4")) * iDirection_d;//um
                                //dDisplacement_um_Indent = Convert.ToDouble((dChange * dSensitivity_d * Math.Pow(10, 3) - Math.Abs(dForce / dStiffness)).ToString("f4")) * iDirection_d;//um
                                dDisplacement_um_Indent = Convert.ToDouble((dChange * dSensitivity_d * Math.Pow(10, 3) * iDirection_d - dForce / dStiffness).ToString("f4"));//um
                                dDisplacement_nm_ori = dDisplacement_um_ori * 1000.0;//nm
                                dDisplacement_nm_Indent = dDisplacement_um_Indent * 1000.0;//nm
                                bGetPoint = false;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Data conversion error:" + strAdd, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }

                        if (bGetPoint)
                        {
                            double x1 = (double)((decimal)dDisplacement_nm_ori - (decimal)dStartDistance_ori);
                            double x2 = (double)((decimal)dDisplacement_nm_Indent - (decimal)dStartDistance_Indent);
                            double y = dForce;
                            listIndentWithdraw_ori.Add(x1, y);
                            listIndentWithdraw.Add(x2, y);
                            listIndentNoMissing_ori.Add(x1, y);
                            listIndentNoMissing.Add(x2, y);
                        }
                    }
                }
            }
        }

        public void ShowHighFreqCurve()
        {
            zgcIndentation_ori.GraphPane.CurveList.Clear();
            zgcIndentation.GraphPane.CurveList.Clear();

            zgcIndentation_ori.GraphPane.AddCurve("", listIndentExtend_ori, Color.Blue, SymbolType.None);
            zgcIndentation_ori.GraphPane.AddCurve("", listIndentWithdraw_ori, Color.Red, SymbolType.None);
            zgcIndentation.GraphPane.AddCurve("", listIndentExtend, Color.Blue, SymbolType.None);
            zgcIndentation.GraphPane.AddCurve("", listIndentWithdraw, Color.Red, SymbolType.None);

            zgcIndentation_ori.AxisChange();//画到zgcIndentation控件中
            zgcIndentation_ori.Refresh();//重新刷新
            zgcIndentation.AxisChange();//画到zgcIndentation控件中
            zgcIndentation.Refresh();//重新刷新
        }

        /// <summary>
        /// 点的坐标写入txt文件
        /// </summary>
        /// <param name="list">点的集合</param>
        /// <param name="strFileFullPath">txt文件全路径</param>
        public void WritePointsToTxt(PointPairList list, string strFileFullPath, bool bForce)
        {
            FileStream fileStream = new System.IO.FileStream(strFileFullPath, FileMode.Create);//using System.IO;
            StreamWriter streamWriter = new StreamWriter(fileStream);
            if (bForce)
            {
                streamWriter.WriteLine("Time(s)               Force(uN)");
            }
            else
            {
                streamWriter.WriteLine("Time(s)               Displacement(nm)");
            }
            for (int i = 0; i < list.Count; i++)
            {
                double dX = list[i].X;
                double dY = list[i].Y;
                string strPointLine = dX.ToString() + "               " + dY.ToString();//行：x y
                streamWriter.WriteLine(strPointLine);
            }
            streamWriter.Flush();//清空缓冲区
            streamWriter.Close();//关闭流
            fileStream.Close();
        }

        /// <summary>
        /// 点的坐标写入txt文件
        /// </summary>
        /// <param name="list">点的集合</param>
        /// <param name="strFileFullPath">txt文件全路径</param>
        public void WritePointsToTxt(PointPairList list, string strFilePath, string strTxt)
        {
            if (!Directory.Exists(strFilePath))
            {
                Directory.CreateDirectory(strFilePath);
            }
            string strFileFullPath = strFilePath + "\\" + strTxt;
            FileStream fileStream = new System.IO.FileStream(strFileFullPath, FileMode.Create);//using System.IO;
            StreamWriter streamWriter = new StreamWriter(fileStream);
            for (int i = 0; i < list.Count; i++)
            {
                double dY = list[i].Y;
                string xjPointLine = dY.ToString();//行：x y
                streamWriter.WriteLine(xjPointLine);
            }
            streamWriter.Flush();//清空缓冲区
            streamWriter.Close();//关闭流
            fileStream.Close();
        }

        /// <summary>
        /// 点的坐标写入txt文件
        /// </summary>
        /// <param name="list">点的集合</param>
        /// <param name="strFileFullPath">txt文件全路径</param>
        public static void WritePointsToTxt_Indent(PointPairList list, string strFilePath, string strTxt)
        {
            if (!Directory.Exists(strFilePath))
            {
                Directory.CreateDirectory(strFilePath);
            }
            string strFileFullPath = strFilePath + "\\" + strTxt;
            FileStream fileStream = new System.IO.FileStream(strFileFullPath, FileMode.Create);//using System.IO;
            StreamWriter streamWriter = new StreamWriter(fileStream);
            streamWriter.WriteLine("Distance_nm     Force_uN");
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

        public void SaveData()
        {
            PointPairList listIndent_ori = new PointPairList();//数据点
            PointPairList listIndent = new PointPairList();//数据点

            for (int i = 0; i < listIndentExtend_ori.Count; i++)
            {
                listIndent_ori.Add(listIndentExtend_ori[i].X, listIndentExtend_ori[i].Y);
            }

            for (int i = 1; i < listIndentWithdraw_ori.Count; i++)
            {
                listIndent_ori.Add(listIndentWithdraw_ori[i].X, listIndentWithdraw_ori[i].Y);
            }

            for (int i = 0; i < listIndentExtend.Count; i++)
            {
                listIndent.Add(listIndentExtend[i].X, listIndentExtend[i].Y);
            }

            for (int i = 1; i < listIndentWithdraw.Count; i++)
            {
                listIndent.Add(listIndentWithdraw[i].X, listIndentWithdraw[i].Y);
            }

            //if (bSample)
            {
                string strTxt_ori = strTime + "_ori.txt";
                WritePointsToTxt_Indent(listIndent_ori, strFilePathTemporary, strTxt_ori);
            }
            //if (bScanner)
            {
                string strTxt = strTime + ".txt";
                WritePointsToTxt_Indent(listIndent, strFilePathTemporary, strTxt);
            }
        }

        public void SaveDataNoMissing()
        {
            PointPairList listIndent_ori = new PointPairList();//数据点
            PointPairList listIndent = new PointPairList();//数据点

            for (int i = 0; i < listIndentNoMissing_ori.Count; i++)
            {
                listIndent_ori.Add(listIndentNoMissing_ori[i].X, listIndentNoMissing_ori[i].Y);
            }

            for (int i = 0; i < listIndentNoMissing.Count; i++)
            {
                listIndent.Add(listIndentNoMissing[i].X, listIndentNoMissing[i].Y);
            }

            //if (bSample)
            {
                string strTxt_ori = strTime + "_ori.txt";
                WritePointsToTxt_Indent(listIndent_ori, strFilePathTemporary, strTxt_ori);
            }
            //if (bScanner)
            {
                string strTxt = strTime + ".txt";
                WritePointsToTxt_Indent(listIndent, strFilePathTemporary, strTxt);
            }
        }

        #endregion

        #region Create Directory

        public void CreatSaveDirectory()
        {
            strFilePathTemporary = Application.StartupPath + "\\DataSave\\Temporary";
            if (!Directory.Exists(strFilePathTemporary))
            {
                Directory.CreateDirectory(strFilePathTemporary);
            }
        }

        #endregion

        #region Other Functions

        //自定义
        public static void Delay(int milliSecond)
        {
            int start = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - start) < milliSecond)
            {
                Application.DoEvents();
            }
        }

        private void Log(string functionName, string result)
        {
            string msg = string.Format("{0}: Function {1} returns {2}{3}", DateTime.Now, functionName, result, Environment.NewLine);
            try
            {
                File.AppendAllText(logFileName, msg);
            }
            catch { };
        }

        protected bool isNumberic(string message)
        {
            Regex rex = new Regex(@"^\d+$");
            if (rex.IsMatch(message))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region 2的N次方

        private static bool GetFlag(int num)
        {
            if (num < 1) return false;
            return (num & (num - 1)) == 0;
        }

        public int log2(int value)   //非递归判断一个数是2的多少次方
        {
            int x = 0;
            while (value > 1)
            {
                value >>= 1;
                x++;
            }
            return x;
        }

        #endregion

        #region Timer

        private void timerShort_Tick(object sender, EventArgs e)
        {
            double dTimeSpan = (DateTime.Now - dClickStart).TotalMilliseconds;
            if (dTimeSpan <= 800)
            {
                if (mCScanPositioner.IsConnected)
                {
                    if (bClockwiseDown)
                    {
                        mCCoarsePositioner[1].MoveCoarse_OpenLoop(mCaxis_R, iBtnSteps * iClockwise, uiAmplitude, uiFrequencyMax);
                    }

                    if (bAnticlockwiseDown)
                    {
                        mCCoarsePositioner[1].MoveCoarse_OpenLoop(mCaxis_R, iBtnSteps * iAnticlockwise, uiAmplitude, uiFrequencyMax);
                    }
                }

                timerShort.Stop();
            }
        }

        private void timerLong_Tick(object sender, EventArgs e)
        {
            double dTimeSpan = (DateTime.Now - dClickStart).TotalMilliseconds;
            if (dTimeSpan > 800)
            {
                if (mCScanPositioner.IsConnected)
                {
                    if (bClockwiseDown)
                    {
                        mCCoarsePositioner[1].MoveCoarse_OpenLoop(mCaxis_R, iBtnSteps * iClockwise, uiAmplitude, uiFrequencyMax);
                    }

                    if (bAnticlockwiseDown)
                    {
                        mCCoarsePositioner[1].MoveCoarse_OpenLoop(mCaxis_R, iBtnSteps * iAnticlockwise, uiAmplitude, uiFrequencyMax);
                    }
                }
            }
        }

        #endregion

        #region 内存回收

        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);

        /// <summary>
        /// 释放内存
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }

        #endregion

        #region Keyboard Events

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (bStep)
            {
                if (mCCoarsePositioner[0].IsConnected && bMoveX && bEnableX)
                {
                    if (lblStep.Text != "" && lblStep.Text != null)
                    {
                        double dX = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                        if (keyData == Keys.Left)
                        {
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_X, dX * iXN);
                        }
                        if (keyData == Keys.Right)
                        {
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_X, dX * iXP);
                        }
                    }
                }

                if (mCCoarsePositioner[0].IsConnected && bMoveY && bEnableY)
                {
                    if (lblStep.Text != "" && lblStep.Text != null)
                    {
                        double dY = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                        if (keyData == Keys.Up)
                        {
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dY * iYP);
                        }
                        if (keyData == Keys.Down)
                        {
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dY * iYN);
                        }
                    }
                }

                if (mCCoarsePositioner[0].IsConnected && bMoveZ && bEnableZ)
                {
                    if (lblStep.Text != "" && lblStep.Text != null)
                    {
                        double dZ = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                        if (keyData == Keys.PageUp)
                        {
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Z, dZ * iZP);
                        }
                        if (keyData == Keys.PageDown && bMoveZDown)
                        {
                            mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Z, dZ * iZN);
                        }
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        private void btIndentationCalibration_Click(object sender, EventArgs e)
        {
            FrmIndentationCalibration showdialog = new FrmIndentationCalibration(this);
            showdialog.ShowDialog();
        }

        private void bntCalibration_Click(object sender, EventArgs e)
        {
            FrmWinCalibrate WinCalibrate = new FrmWinCalibrate(this);
            WinCalibrate.ShowDialog();

            //if (bntCalibration.Text == "标定")
            //{
            //    mThread_Calibrate = new Thread(new ThreadStart(ThreadFunction_Calibrate)) { IsBackground = true };
            //    mThread_Calibrate.Start();
            //}
            //else if (mThread_Calibrate.ThreadState == System.Threading.ThreadState.Running)
            //{ 
            //    mThread_Calibrate.Abort();
            //    Delay(2000);

            //    bStabilization = false;
            //    //移动Scanner到初始位置
            //    mCScanPositioner.MoveToFinePosition(mCaxis_F, uiInitBit, 10, 20);
            //    dSetDAC = dReadDAC;
            //    bStabilization = true;
            //}
        }

        public void ThreadFunction_Calibrate()
        {
            ////bCalibratingOrNot，防止按下后程序还在进行
            //bool bCalibratingOrNot = true;

            //if (bntCalibration.Text == "标定")
            //    bCalibratingOrNot = true;
            //else
            //    bCalibratingOrNot = false;

            //if (bntCalibration.Text == "标定")
            //{
            //    bntCalibration.Text = "停止标定";

            //    uint uiIndentSpeed = 1200;      //加/减压速度
            //    uint CycleTime = 1;             //循环速度
            //    bool bMax = false;

            //    //从Scanner的初始位置开始移动
            //    mCScanPositioner.MoveToFinePosition(mCaxis_F, uiInitBit, 10, 20);

            //    //开始循环
            //    for (int i = 0; i < CycleTime; i++)
            //    {
            //        if (bCalibratingOrNot)
            //        {
            //            bStabilization = false;
            //        }

            //        /*从初始位置移动到最大位置*/
            //        while (uiCurrentBit <= uiMaxADCValue && !bMax && bCalibratingOrNot)
            //        {
            //            uiCurrentBit += uiIndentSpeed;

            //            if (uiCurrentBit >= uiMaxADCValue)
            //            {
            //                uiCurrentBit = uiMaxADCValue;
            //                bMax = true;
            //            }
            //            mCScanPositioner.MoveToFinePosition(mCaxis_F, uiCurrentBit, 10, 20);
            //            Delay(300);
            //        }

            //        if (bCalibratingOrNot)
            //        {
            //            //到达最大电压后保持2s
            //            dSetDAC = dReadDAC;
            //            bStabilization = true;
            //            Delay(2000);


            //            /*从最大位置移动到初始位置*/
            //            bStabilization = false;
            //        }
            //        while (uiCurrentBit >= uiInitBit && bCalibratingOrNot)
            //        {
            //            uiCurrentBit -= uiIndentSpeed;
            //            mCScanPositioner.MoveToFinePosition(mCaxis_F, uiCurrentBit, 10, 20);
            //            Delay(300);
            //        }

            //        if (bCalibratingOrNot)
            //        {
            //            dSetDAC = dReadDAC;
            //            bStabilization = true;
            //            Delay(2000);
            //        }
            //    }

            //    bStabilization = false;
            //    //移动Scanner到初始位置
            //    mCScanPositioner.MoveToFinePosition(mCaxis_F, uiInitBit, 10, 20);
            //    dSetDAC = dReadDAC;
            //    bStabilization = true;

            //    FrmCalculateSensortivity frmCS = new FrmCalculateSensortivity();
            //    frmCS.ShowDialog();
            //}
            //else
            //{
            //    bntCalibration.Text = "标定";
            //    bStabilization = false;
            //    //移动Scanner到初始位置
            //    mCScanPositioner.MoveToFinePosition(mCaxis_F, uiInitBit, 10, 20);
            //    dSetDAC = dReadDAC;
            //    bStabilization = true;
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void bPause_Click(object sender, EventArgs e)
        {
            if (bPauseIndent.Text == "暂停")
            {
                bPause = true;
                bPauseIndent.Text = "继续";
            }
            else
            {
                bPause = false;
                bPauseIndent.Text = "暂停";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FrmIndentationCalibration showdialog = new FrmIndentationCalibration(this);
            showdialog.ShowDialog();
        }

        private void bntCaliWin_Click(object sender, EventArgs e)
        {
            FrmCaliWin caliwin = new FrmCaliWin(this);
            caliwin.ShowDialog();
        }


        private void bPause_Click_1(object sender, EventArgs e)
        {

        }

        private void bIndentPause_Click(object sender, EventArgs e)
        {
            if (bIndentPause.Text == "暂停")
            {
                bPause = true;
                bIndentPause.Text = "继续";
            }
            else
            {
                bPause = false;
                bIndentPause.Text = "暂停";
            }
        }
    }
}
