using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NMT.Nators
{
    public class CScanPositioner
    {
        public double mMax_StepSize_nm = 1783;//1500.0;// motion distance in nm
        public double mMax_DACValue_Bit = 4095;// 2^12-1, 12 bit DAC
        double NM2STEPS(double x) { return ((x) / mMax_StepSize_nm * mMax_DACValue_Bit); }
        double STEPS2NM(double x) { return ((x) * mMax_StepSize_nm / mMax_DACValue_Bit); }
        CThread mThreadBackground_CP;
        GerneralFunction mGnlFunction;
        bool mSensorAutoOff = true;
        public bool mConnected = false;
        private bool mSensorConnected = true;
        private bool moving;
        public bool IsConnected { get { return mConnected; } }
        public bool bFirstConnected = true;
        uint[] mSensorMode = new uint[mNumberOfChannel];
        const uint mCaxisNum = 3;
        const int mNumberOfChannel = 6;
        const int NUM_OF_AXIS = (3);
        const int X_AXIS = (0);
        const int Y_AXIS = (1);
        const int Z_AXIS = (2);
        const int AXIS_DELTA = 0;// (3);     //0
        const int X_CP_AXIS = (AXIS_DELTA + X_AXIS);
        const int Y_CP_AXIS = (AXIS_DELTA + Y_AXIS);
        const int Z_CP_AXIS = (AXIS_DELTA + Z_AXIS);

        public uint uiKey = 0x01020001;
        public uint mSystemIndex = 0;
        public uint mResult;
        public uint mFrequency_OpenLoop;
        public uint[] mFrequency_ClosedLoop;
        public uint uiMaxVolADC = 262143;//2^18-1

        int[] mPositionStore;
        double[] mDirection;// +-1, control the direction of each axis


        public CScanPositioner(bool sensor_connected, double Coarse_Max_StepSize_nm, double Coarse_Max_DACValue_Bit)
        {
            mMax_StepSize_nm = Coarse_Max_StepSize_nm;
            mMax_DACValue_Bit = Coarse_Max_DACValue_Bit;

            mSensorConnected = sensor_connected;

            mDirection = new double[6] { -1, 1, 1, -1, -1, -1 };
            mFrequency_ClosedLoop = new uint[6] { 0, 0, 0, 0, 0, 0 };
            mSensorMode = new uint[6] { 1, 1, 1, 1, 1, 1 };
            mPositionStore = new int[6] { 0, 0, 0, 0, 0, 0 };
            moving = false;
            mThreadBackground_CP = new CThread();
            mThreadBackground_CP.SetName(null);
            mGnlFunction = new GerneralFunction();
        }

        public void SetSensorAutoOff(bool auto_off)
        {
            mSensorAutoOff = auto_off;
        }

        ~CScanPositioner() { Disconnect(); }
        public void Disconnect()
        {
            mConnected = false;
            //mResult = CScanner.SCAN_ReleaseSystems();
            mResult = CScanner.SCAN_CloseSystem(mSystemIndex);
            Delay(5);

            if (mResult != CScanner.SCAN_OK)
            {
                mGnlFunction.MY_DEBUG("Nanopositioner SCAN_CloseSystem error!");
            }
        }

        public bool IsMoving()
        {
            return moving;
        }

        public int Initialize(string strID)
        {
            //mConnected = false;
            try
            {
                string strLoc = "usb:id:" + strID;
                //string strLoc = "usb:ix:0";
                mResult = CScanner.SCAN_OpenSystem(ref mSystemIndex, strLoc, "sync");//sync:同步,async异步
                if (mResult == CScanner.SCAN_OK)
                {
                    mConnected = true;
                }

                //Delay(10);
                mGnlFunction.MY_DEBUG("Nanopositioner SCAN_OpenSystem SCAN_HARDWARE_RESET:" + mResult.ToString());
            }
            catch (Exception ex)
            {
                mGnlFunction.MY_DEBUG("coarse nanopositioner initial error!");
            }

            if (mConnected)
            {
                if (bFirstConnected)
                {
                    bFirstConnected = false;
                    //default_set();
                }
                mGnlFunction.MY_DEBUG("coarse nanopositioner connected OK.");
                return 0;
            }
            else
            {
                return -1;
            }
        }

        public void SetSpeedOpenLoop(uint frequency)
        {
            if (frequency < 1)
                frequency = 1;
            if (frequency > 18500)
                frequency = 18500;

            mFrequency_OpenLoop = frequency;
        }

        public void MoveToFinePosition(uint channel, uint position, uint sacnStep, uint scanDelay)
        {
            if (position > uiMaxVolADC || position < 0)
            {
                mGnlFunction.MY_DEBUG("MoveToFinePosition input exceeding error!\n");
            }
            if (position > uiMaxVolADC)
                position = uiMaxVolADC;
            if (position < 0)
                position = 0;

            mResult = CScanner.SCAN_ScanMoveAbsolute_S((uint)mSystemIndex, (uint)channel, position, sacnStep, scanDelay);

            //if (mResult != CScanner.SCAN_OK)
            //{
            //    mConnected = false;
            //    mGnlFunction.MY_DEBUG("MoveToFinePosition error!\n");
            //}
        }

        public void MoveFineDistance(uint channel, int distance, uint sacnStep, uint scanDelay)//scanDelay:us
        {
            mResult = CScanner.SCAN_ScanMoveRelative_S((uint)mSystemIndex, channel, distance, sacnStep, scanDelay);

            //if (mResult != CScanner.SCAN_OK)
            //{
            //    mConnected = false;
            //    mGnlFunction.MY_DEBUG("MoveFineDistance error!\n");
            //}
        }

        public uint GetFinePosition(uint channelIndex)
        {
            uint level = 0;
            CScanner.SCAN_GetVoltageLevel_S((uint)mSystemIndex, (uint)channelIndex, ref level);
            //if (mResult != CScanner.SCAN_OK)
            //{
            //    mConnected = false;
            //    mGnlFunction.MY_DEBUG("GetFinePosition error!\n");
            //}
            return level;
        }

        public byte[] GetSystemLocator()
        {
            byte[] outBuffer = new byte[4096];
            uint uiBufferSize = (uint)outBuffer.Length;
            mResult = CScanner.SCAN_GetSystemLocator((uint)mSystemIndex, ref outBuffer[0], ref uiBufferSize);

            if (mResult != CScanner.SCAN_OK)
            {
                mConnected = false;
                mGnlFunction.MY_DEBUG("GetSystemLocator error!\n");
            }

            return outBuffer;
        }

        public static void Delay(int milliSecond)
        {
            int start = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - start) < milliSecond)
            {
                Application.DoEvents();
            }
        }
    }
}
