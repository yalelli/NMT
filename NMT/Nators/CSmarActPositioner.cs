using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NMT.Nators;

namespace NMT.SmarAct
{
    public class CSmarActPositioner
    {
        public double mMax_StepSize_nm = 1783;//1500.0;// motion distance in nm
        public double mMax_DACValue_Bit = 4095;// 2^12-1, 12 bit DAC
        double NM2STEPS(double x) { return ((x) / mMax_StepSize_nm * mMax_DACValue_Bit); }
        double STEPS2NM(double x) { return ((x) * mMax_StepSize_nm / mMax_DACValue_Bit); }
        private bool mSensorConnected = true;
        private int mIsIdle = 0;// for delay checkstatus
        private bool mIsBusy = false;

        CThread mThreadBackground_CP;
        GerneralFunction mGnlFunction;

        bool mSensorAutoOff = true;
        const int mNumberOfChannel = 6;
        bool mConnected = false;
        public bool IsConnected { get { return mConnected; } }
        uint[] mSensorMode = new uint[mNumberOfChannel];
        private bool moving;
        const int NUM_OF_AXIS = (3);
        const int X_AXIS = (0);
        const int Y_AXIS = (1);
        const int Z_AXIS = (2);
        const int AXIS_DELTA = 0;// (3);     //0
        const int X_CP_AXIS = (AXIS_DELTA + X_AXIS);
        const int Y_CP_AXIS = (AXIS_DELTA + Y_AXIS);
        const int Z_CP_AXIS = (AXIS_DELTA + Z_AXIS);

        uint mID = 0;
        uint mSystemIndex = 0;
        uint mResult;
        uint mFrequency_OpenLoop;
        uint[] mFrequency_ClosedLoop;
        int[] mPositionStore;
        double[] mDirection;// +-1, control the direction of each axis
        public uint uiKey = 0x01020001;


        public CSmarActPositioner(bool sensor_connected, double Coarse_Max_StepSize_nm, double Coarse_Max_DACValue_Bit)
        {
            mMax_StepSize_nm = Coarse_Max_StepSize_nm;
            mMax_DACValue_Bit = Coarse_Max_DACValue_Bit;

            mSensorConnected = sensor_connected;

            mDirection = new double[6] { -1, -1, -1, -1, -1, -1 };
            mFrequency_ClosedLoop = new uint[6] { 0, 0, 0, 0, 0, 0 };
            mSensorMode = new uint[6] { 1, 1, 1, 1, 1, 1 };
            mPositionStore = new int[6] { 0, 0, 0, 0, 0, 0 };
            moving = false;
            mThreadBackground_CP = new CThread();
            mThreadBackground_CP.SetName(null);
            mGnlFunction = new GerneralFunction();
        }

        public bool IsBusy
        {
            get { return mIsBusy; }
        }

        public void SetSensorAutoOff(bool auto_off)
        {
            mSensorAutoOff = auto_off;
        }

        ~CSmarActPositioner() { Disconnect(); }
        public void Disconnect()
        {
            mConnected = false;
            mIsBusy = true;
            //mResult = CSmarAct.SA_ReleaseSystems();
            mResult = CSmarAct.SA_CloseSystem(mSystemIndex); 
            Delay(5);
            mIsBusy = false;

            if (mResult != CSmarAct.SA_OK)
            {
                mGnlFunction.MY_DEBUG("Nanopositioner SA_ReleaseSystems error!");
            }
        }

        public bool IsMoving()
        {
            return moving;
        }

        public uint GetSystemID()
        {
            mResult = CSmarAct.SA_InitSystems(CSmarAct.SA_SYNCHRONOUS_COMMUNICATION);//同步	 //+ CSmarAct.SA_HARDWARE_RESET init systems	
            mResult = CSmarAct.SA_GetSystemID((uint)mSystemIndex, ref mID);
            mGnlFunction.MY_DEBUG("SA_GetSystemID:" + mID.ToString());

            return mID;
        }

        public int Initialize(string strID)
        {
            mConnected = false;
            try
            {
                if (mIsBusy == false)
                {
                    mIsBusy = true;
                    //CSmarAct.SA_ReleaseSystems();
                    CSmarAct.SA_CloseSystem(mSystemIndex);
                    Delay(100);

                    //mResult = CSmarAct.SA_InitSystems(CSmarAct.SA_SYNCHRONOUS_COMMUNICATION);//同步	 //+ CSmarAct.SA_HARDWARE_RESET init systems	
                    //string strLoc = "usb:id:" + mID.ToString();
                    string strLoc = "usb:id:" + strID;
                    mResult = CSmarAct.SA_OpenSystem(ref mSystemIndex, strLoc, "sync");
                    if (mResult == CSmarAct.SA_OK)
                    {
                        mConnected = true;
                    }

                    Delay(5);
                    Delay(100);
                    mIsBusy = false;
                    mGnlFunction.MY_DEBUG("Nanopositioner SA_OpenSystem SA_HARDWARE_RESET:" + mResult.ToString());
                }
                else
                {
                    mGnlFunction.MY_DEBUG("coarse busy: Initialize");
                }

                //mResult = CSmarAct.SA_GetSystemID((uint)mSystemIndex, ref mID);
                //mGnlFunction.MY_DEBUG("SA_GetSystemID:" + mID.ToString());

                Delay(5);
            }
            catch (Exception ex)
            {
                mGnlFunction.MY_DEBUG("coarse nanopositioner initial error!");
            }

            default_set();

            mGnlFunction.MY_DEBUG("coarse nanopositioner connected OK.");
            return 0;
        }

        public void default_set()
        {
            if (mSensorConnected == false)
                SetSensorModeDisable();

            //CSmarAct.SA_SetAccumulateRelativePositions_S((uint)mSystemIndex, X_CP_AXIS, CSmarAct.SA_NO_ACCUMULATE_RELATIVE_POSITIONS);
            //CSmarAct.SA_SetAccumulateRelativePositions_S((uint)mSystemIndex, Y_CP_AXIS, CSmarAct.SA_NO_ACCUMULATE_RELATIVE_POSITIONS);
            CSmarAct.SA_SetAccumulateRelativePositions_S((uint)mSystemIndex, X_CP_AXIS, CSmarAct.SA_ACCUMULATE_RELATIVE_POSITIONS);
            CSmarAct.SA_SetAccumulateRelativePositions_S((uint)mSystemIndex, Y_CP_AXIS, CSmarAct.SA_ACCUMULATE_RELATIVE_POSITIONS);//累积
            mResult = CSmarAct.SA_SetAccumulateRelativePositions_S((uint)mSystemIndex, Z_CP_AXIS, CSmarAct.SA_NO_ACCUMULATE_RELATIVE_POSITIONS);//不累积，直接覆盖

            ////Low Vibration
            //SetAllChannelProperty();

            if (mSensorConnected == false)
            {
                SetSpeedOpenLoop(1000);
            }

            int position = 0;
            uint sensor_type = 1;    // linear positioners with nanosensor

            for (uint channelIndex = X_CP_AXIS; channelIndex < X_CP_AXIS + 3; channelIndex++)
            {
                CSmarAct.SA_SetSensorType_S(mSystemIndex, channelIndex, sensor_type);   // Set Sensor Type
                CSmarAct.SA_SetPosition_S(mSystemIndex, channelIndex, position);   // Set position to 0
            }
        }

        public void SetAllChannelProperty()
        {
            SetChannelProperty(X_CP_AXIS);
            SetChannelProperty(Y_CP_AXIS);
            SetChannelProperty(Z_CP_AXIS);
        }

        public void SetChannelProperty(uint channel)
        {
            //// activate low vibration mode for channel
            ////mResult = CSmarAct.SA_SetChannelProperty_S(0, 0, uiKey, 1);

            // note: enabling the low vibration mode implicitly enables the acceleration control.
            mResult = CSmarAct.SA_SetChannelProperty_S((uint)mSystemIndex, channel, CSmarAct.SA_EPK(CSmarAct.SA_GENERAL, CSmarAct.SA_LOW_VIBRATION, CSmarAct.SA_OPERATION_MODE), CSmarAct.SA_ENABLED);
            if (mResult != CSmarAct.SA_OK)
            {
                //CSmarAct.SA_ReleaseSystems();
                CSmarAct.SA_CloseSystem(mSystemIndex);
                return;
            }

            // read out current acceleration (given in um/s^2)
            uint acc = 0;
            mResult = CSmarAct.SA_GetClosedLoopMoveAcceleration_S((uint)mSystemIndex, channel, ref acc);	// also 1.9 function
            if (mResult != CSmarAct.SA_OK)
            {
                //CSmarAct.SA_ReleaseSystems();
                CSmarAct.SA_CloseSystem(mSystemIndex);
                return;
            }

            // "acc" will have the default value 10000000 um/s^2
            // decrease acceleration to get smooth ramps
            mResult = CSmarAct.SA_SetClosedLoopMoveAcceleration_S((uint)mSystemIndex, channel, 0);	// also 1.9 function
            if (mResult != CSmarAct.SA_OK)
            {
                //CSmarAct.SA_ReleaseSystems();
                CSmarAct.SA_CloseSystem(mSystemIndex);
                return;
            }
        }

        public void SetChannelProperty(uint channel, uint acceleration)
        {
            //// activate low vibration mode for channel
            ////mResult = CSmarAct.SA_SetChannelProperty_S(0, 0, uiKey, 1);
            //mResult = CSmarAct.SA_SetChannelProperty_S((uint)mSystemIndex, channel, CSmarAct.SA_EPK(CSmarAct.SA_GENERAL, CSmarAct.SA_LOW_VIBRATION, CSmarAct.SA_OPERATION_MODE), CSmarAct.SA_ENABLED);
            //if (mResult != CSmarAct.SA_OK)
            //{
            //    //CSmarAct.SA_ReleaseSystems();
            //    CSmarAct.SA_CloseSystem(mSystemIndex);
            //    return;
            //}

            // note: enabling the low vibration mode implicitly enables the acceleration control.
            // read out current acceleration (given in um/s^2)
            uint acc = 0;
            mResult = CSmarAct.SA_GetClosedLoopMoveAcceleration_S((uint)mSystemIndex, channel, ref acc);
            if (mResult != CSmarAct.SA_OK)
            {
                //CSmarAct.SA_ReleaseSystems();
                CSmarAct.SA_CloseSystem(mSystemIndex);
                return;
            }

            // "acc" will have the default value 10000000 um/s^2
            // decrease acceleration to get smooth ramps
            mResult = CSmarAct.SA_SetClosedLoopMoveAcceleration_S((uint)mSystemIndex, channel, acceleration);
            if (mResult != CSmarAct.SA_OK)
            {
                //CSmarAct.SA_ReleaseSystems();
                CSmarAct.SA_CloseSystem(mSystemIndex);
                return;
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

        public void MoveDistance_OpenLoop(uint channel, double distance)
        {
            MoveDistance_OpenLoop_F((uint)channel, distance, mFrequency_OpenLoop);
        }

        public void MoveDistance_OpenLoop_Rough_F(uint channel, double distance, uint frequency, bool wait_until_finish = true)
        {
            mFrequency_OpenLoop = frequency;

            distance *= mDirection[(uint)channel];
            if (moving == true)
            {
                mGnlFunction.MY_DEBUG("Coarse positioner busy, abort.");
                return;
            }

            moving = true;
            mIsIdle++;

            mGnlFunction.MY_DEBUG("MoveDistance_OpenLoop_Rough_F:\t" + ((char)(channel + (uint)'x')).ToString() + "\t" + distance.ToString() + "\t" + frequency.ToString());

            distance = NM2STEPS(distance);
            int number_of_steps = 0, step_left = (int)distance;
            int dir = Math.Sign(distance);
            distance = Math.Abs(distance);

            double STEP_MAX = (NM2STEPS(1000));
            double STEP_MIN = (NM2STEPS(50));

            double STEP_Prefer = (NM2STEPS(700));

            if (distance <= STEP_MIN)
            {
                MoveFineSteps((uint)channel, 1 * dir, (uint)STEP_MIN, frequency);
            }

            if (distance < STEP_MAX && distance > STEP_MIN)
            {
                MoveFineSteps((uint)channel, 1 * dir, (uint)distance, frequency);
            }

            if (distance < STEP_MAX * 2 && distance > STEP_MAX)
            {
                MoveFineSteps((uint)channel, 2 * dir, (uint)(distance / 2), frequency);
            }

            if (distance > STEP_MAX * 2)
            {
                number_of_steps = (int)(distance / STEP_Prefer - 1);// try to move less, no exceed

                MoveFineSteps((uint)channel, number_of_steps * dir, (uint)STEP_Prefer, frequency);
                if (wait_until_finish == true)
                {
                    WaitForIdle((uint)channel);

                    Delay((int)(number_of_steps * 1000 / frequency + 500));
                }
            }
            moving = false;
        }

        /// <summary>
        /// for UI move coarse
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="distance"></param>
        public void MoveDistance_Steps_OpenLoop(uint channel, double distance, uint frequency)
        {
            distance *= mDirection[(uint)channel];
            mIsIdle++;

            mGnlFunction.MY_DEBUG("MoveDistance_Steps_OpenLoop:\t" + ((char)(channel + (uint)'x')).ToString() + "\t" + distance.ToString() + "\t" + frequency.ToString());

            distance = NM2STEPS(distance);

            int dir = Math.Sign(distance);
            distance = Math.Abs(distance);

            double STEP_MAX = (NM2STEPS(mMax_StepSize_nm));
            double STEP_MIN = (NM2STEPS(50));

            double amplitude = ((STEP_MAX + STEP_MIN) / 2);

            double fsteps = distance / amplitude;
            int steps = (int)fsteps;
            if (fsteps - steps > 0.5)
                steps++;
            steps *= dir;


            if (mIsBusy == false)
            {
                mIsBusy = true;
                mIsIdle++;
                mResult = CSmarAct.SA_StepMove_S((uint)mSystemIndex, (uint)channel, steps, (uint)amplitude, frequency);

                Delay(5); mIsBusy = false;
                if (mResult != CSmarAct.SA_OK)
                {
                    mConnected = false;
                    GerneralFunction.MY_DEBUG("MoveDistance_Steps_OpenLoop error!\n", new double[] { channel, steps, amplitude, frequency });

                }
            }
            else
                mGnlFunction.MY_DEBUG("coarse busy: MoveFineSteps");
        }

        /// <summary>
        /// for UI move coarse
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="steps"></param>
        /// <param name="amplitude"></param>
        /// <param name="frequency"></param>
        public void MoveCoarse_OpenLoop(uint channel, int steps, uint amplitude, uint frequency)
        {
            mResult = CSmarAct.SA_StepMove_S((uint)mSystemIndex, (uint)channel, steps, (uint)amplitude, frequency);

            if (mResult != CSmarAct.SA_OK)
            {
                mConnected = false;
                GerneralFunction.MY_DEBUG("MoveDistance_Steps_OpenLoop error!\n", new double[] { channel, steps, amplitude, frequency });
            }
        }

        public void MoveDistance_OpenLoop_F(uint channel, double distance, uint frequency, bool wait_until_finish = true)
        {
            mFrequency_OpenLoop = frequency;

            distance *= mDirection[(uint)channel];
            if (moving == true)
            {
                mGnlFunction.MY_DEBUG("Coarse positioner busy, abort.");
                return;
            }

            moving = true;
            mIsIdle++;

            mGnlFunction.MY_DEBUG("MoveDistance_OpenLoop_F:\t" + ((char)(channel + (uint)'x')).ToString() + "\t" + distance.ToString() + "\t" + frequency.ToString());

            distance = NM2STEPS(distance);
            int number_of_steps = 0, step_left = (int)distance;
            int dir = Math.Sign(distance);
            distance = Math.Abs(distance);

            double STEP_MAX = (NM2STEPS(mMax_StepSize_nm));
            double STEP_MIN = (NM2STEPS(50));
            if (distance <= STEP_MIN)
            {
                MoveFineSteps((uint)channel, 1 * dir, (uint)STEP_MIN, frequency);
            }

            if (distance < STEP_MAX && distance > STEP_MIN)
            {
                MoveFineSteps((uint)channel, 1 * dir, (uint)distance, frequency);
            }

            if (distance < STEP_MAX * 2 && distance > STEP_MAX)
            {
                MoveFineSteps((uint)channel, 2 * dir, (uint)(distance / 2), frequency);
            }

            if (distance > STEP_MAX * 2)
            {
                number_of_steps = (int)(distance / STEP_MAX - 1);
                step_left = (int)(distance - STEP_MAX * (double)(number_of_steps));
                step_left /= 2;

                MoveFineSteps((uint)channel, number_of_steps * dir, (uint)STEP_MAX, frequency);
                if (wait_until_finish == true)
                {
                    WaitForIdle((uint)channel);
                }

                Delay((int)(number_of_steps * 1000 / frequency + 500));

                MoveFineSteps((uint)channel, 2 * dir, (uint)step_left, frequency);
            }
            moving = false;
        }

        void SetSensorMode(uint mode)
        {
            for (int k = 0; k < mNumberOfChannel; k++)
                mSensorMode[k] = mode;

            SetSensorModeHardWare(mode);
        }

        void SetSensorMode(uint mode, int channelIndex)
        {
            mSensorMode[channelIndex] = mode;
            SetSensorModeHardWare(mode);
        }

        void SetSensorModeHardWare(uint mode)
        {
            if (mIsBusy == false)
            {
                mGnlFunction.MY_DEBUG("CP SetSensorModeHardWare: " + mode.ToString());
                mIsBusy = true;
                mIsIdle++;
                mResult = CSmarAct.SA_SetSensorEnabled_S((uint)mSystemIndex, mode);
                Delay(10); //5
                mIsBusy = false;
                if (mResult != CSmarAct.SA_OK)
                {
                    mConnected = false;
                    mGnlFunction.MY_DEBUG("SetSensorModeHardWare error, mode:" + mode.ToString());
                }
            }
            else
                mGnlFunction.MY_DEBUG("coarse busy: SetSensorModeHardWare");
        }

        public void SetSensorModeHardWareDisable()
        {
            SetSensorModeHardWare(CSmarAct.SA_SENSOR_DISABLED);
        }

        public void SetSensorModeHardWareEnable()
        {
            if (mSensorConnected == true)
                SetSensorModeHardWare(CSmarAct.SA_SENSOR_ENABLED);
        }

        public void SetSensorModeHardWarePowerSave()
        {
            if (mSensorConnected == true)
                SetSensorModeHardWare(CSmarAct.SA_SENSOR_POWERSAVE);
        }

        public void SetSensorModeDisable()
        {
            SetSensorMode(CSmarAct.SA_SENSOR_DISABLED);
        }

        public void SetSensorModeEnable()
        {
            SetSensorModePowerSave();
        }

        public void SetSensorModePowerSave()
        {
            if (mSensorConnected == true)
                SetSensorMode(CSmarAct.SA_SENSOR_POWERSAVE);
        }

        public void StopMove(uint axis)
        {
            CSmarAct.SA_Stop_S((uint)mSystemIndex, axis);
        }

        public void Stop(uint axis)
        {
            if (mIsBusy == false)
            {
                mIsBusy = true;
                mIsIdle++;
                CSmarAct.SA_Stop_S((uint)mSystemIndex, axis);
                Delay(5);
                Delay(5); mIsBusy = false;
            }
            else
                mGnlFunction.MY_DEBUG("coarse busy: Stop");
        }

        public void MoveToFinePosition(uint channel, uint position, uint speed)
        {
            if (position > 4095 || position < 0)
            {
                mGnlFunction.MY_DEBUG("MoveToFinePosition input exceeding error!\n");
            }
            if (position > 4095)
                position = 4095;
            if (position < 0)
                position = 0;

            mIsIdle++;
            mResult = CSmarAct.SA_ScanMoveAbsolute_S((uint)mSystemIndex, (uint)channel, position, speed);

            if (mResult != CSmarAct.SA_OK)
            {
                mConnected = false;
                mGnlFunction.MY_DEBUG("MoveToFinePosition error!\n");
            }
        }

        public void MoveFineDistance(int channel, int distance, uint speed)
        {
            mIsIdle++;
            mResult = CSmarAct.SA_ScanMoveRelative_S((uint)mSystemIndex, (uint)channel, distance, speed);

            if (mResult != CSmarAct.SA_OK)
            {
                mConnected = false;
                mGnlFunction.MY_DEBUG("MoveFineDistance error!\n");
            }
        }

        public void MoveAtSpeed(uint channelIndex, double dir, double step_size, double frequency)
        {
            dir *= 30000;
            MoveFineSteps(channelIndex, (int)dir, (uint)Math.Abs(step_size), (uint)Math.Abs(frequency));
            mGnlFunction.MY_DEBUG("coarse MoveAtSpeed step_size: " + step_size.ToString() + ", frequency:" + frequency.ToString());
        }

        public void MoveFineSteps(uint channelIndex, int steps, uint amplitude, uint frequency)
        {
            steps = GerneralFunction.LIMIT_MAX_MIN(steps, 30000, -30000);
            amplitude = GerneralFunction.LIMIT_MAX_MIN(amplitude, 4095, 2500);//500.0/1500.0*4095.0=1365
            frequency = GerneralFunction.LIMIT_MAX_MIN(frequency, 18500, 1);

            if (mIsBusy == false)
            {
                mIsBusy = true;
                mIsIdle++;
                mResult = CSmarAct.SA_StepMove_S((uint)(uint)mSystemIndex, (uint)(uint)channelIndex, steps, amplitude, frequency);

                Delay(5); mIsBusy = false;
                if (mResult != CSmarAct.SA_OK)
                {
                    mConnected = false;
                    GerneralFunction.MY_DEBUG("MoveFineSteps error!\n", new double[] { channelIndex, steps, amplitude, frequency });

                }
            }
            else
                mGnlFunction.MY_DEBUG("coarse busy: MoveFineSteps");
        }

        void MoveWait(uint channelIndex, int stepsize)
        {
            mIsIdle++;
            CSmarAct.SA_GotoPositionRelative_S((uint)mSystemIndex, (uint)channelIndex, stepsize, 1000);
            WaitForIdle(mSystemIndex);
        }

        public bool CheckConnection(uint channelIndex)
        {
            uint status = 0;
            return GetStatus(channelIndex, ref status);
        }

        public bool GetCurStatus(uint channelIndex, ref uint status)
        {
            //status = CSmarAct.SA_STOPPED_STATUS;
            mResult = CSmarAct.SA_GetStatus_S((uint)mSystemIndex, (uint)channelIndex, ref status);
            return mResult == CSmarAct.SA_OK;
        }

        public bool GetStatus(uint channelIndex, ref uint status)
        {
            if (mIsIdle > 0)
            {
                mIsIdle--;
                return true;
            }

            status = CSmarAct.SA_STOPPED_STATUS;
            if (mIsBusy == false)
            {
                mIsBusy = true;
                mResult = CSmarAct.SA_GetStatus_S((uint)mSystemIndex, (uint)channelIndex, ref status);		  // get mStatus
                Delay(5); 
                mIsBusy = false;

                if (mResult != CSmarAct.SA_OK)
                {
                    mConnected = false;
                    mGnlFunction.MY_DEBUG("coarse GetStatus error:" + mResult.ToString() + "_" + status.ToString());
                }
            }
            else
                mGnlFunction.MY_DEBUG("coarse busy: GetStatus");
            return mResult == CSmarAct.SA_OK;
        }

        void WaitForIdle(uint channelIndex)
        {
            mResult = 0;
            int k = 100;
            do
            {
                uint pR = 0;
                GetStatus(channelIndex, ref pR);
                if (pR != CSmarAct.SA_STOPPED_STATUS)
                {
                    if (mSensorConnected == true)
                        mGnlFunction.MY_DEBUG("coarse positioner is busy: " + k.ToString());
                }

                Delay(10);
                if (k-- < 0)
                    break;
            }
            while (mResult != CSmarAct.SA_TARGET_STATUS);
        }

        public void MoveDistance(uint channel, double distance)
        {
            mThreadBackground_CP.Start(
                delegate()
                {
                    if (mSensorMode[channel] == CSmarAct.SA_SENSOR_DISABLED || mSensorConnected == false)
                        MoveDistance_OpenLoop(channel, distance);
                    else
                    {
                        MoveDistance_CloseLoop(channel, distance);
                    }
                });
        }

        public void MoveDistance_CloseLoop_NoSleep(uint channel, double distance)
        {
            distance *= mDirection[(uint)channel];
            CSmarAct.SA_GotoPositionRelative_S(mSystemIndex, channel, (int)distance, 0);
        }

        //这个函数是不是有问题
        public void MoveDistance_CloseLoop(uint channel, double distance)//uint channelIndex, int stepsize, int speed)// close loop
        {
            //distance *= mDirection[(uint)channel];

            int k = 5;
            while (k-- > 0)
            {
                if (mIsBusy == false)
                    break;
                Delay(5);
            }

            if (mIsBusy == false)
            {
                mIsBusy = true;
                mIsIdle++;
                CSmarAct.SA_GotoPositionRelative_S(mSystemIndex, channel, (int)distance, 60000);
                Delay(5);
                mIsBusy = false;

                mGnlFunction.MY_DEBUG("CoarsePositioiner MoveDistance_CloseLoop:\t" + ((char)(channel + (uint)'x')).ToString() + "\t" + distance.ToString() + "\t" + mFrequency_ClosedLoop[channel].ToString());
            }
            else
                mGnlFunction.MY_DEBUG("coarse busy: MoveDistance_CloseLoop, abort");
        }

        public void MoveDistance_CloseLoop_Absolute(uint channel, int position)//uint channelIndex, int stepsize, int speed)// close loop
        {
            //distance *= mDirection[(uint)channel];

            int k = 5;
            while (k-- > 0)
            {
                if (mIsBusy == false)
                    break;
                Delay(5);
            }

            if (mIsBusy == false)
            {
                mIsBusy = true;
                mIsIdle++;
                CSmarAct.SA_GotoPositionAbsolute_S(mSystemIndex, channel, position, 60000);
                Delay(5);
                mIsBusy = false;

                mGnlFunction.MY_DEBUG("CoarsePositioiner MoveDistance_CloseLoop:\t" + ((char)(channel + (uint)'x')).ToString() + "\t" + position.ToString() + "\t" + mFrequency_ClosedLoop[channel].ToString());
            }
            else
                mGnlFunction.MY_DEBUG("coarse busy: MoveDistance_CloseLoop, abort");
        }

        public void MoveDistance_CloseLoop_Speed(uint channel, double distance, uint speed)
        {
            //distance *= mDirection[(uint)channel];
            SetSpeedClosedLoop(channel, speed);
            CSmarAct.SA_GotoPositionRelative_S(mSystemIndex, channel, (int)distance, 60000);
            //if (mResult != CSmarAct.SA_OK)
            //{
            //    mConnected = false;
            //    GerneralFunction.MY_DEBUG("CoarsePositioiner MoveDistance_CloseLoop_F_Speed error!\n", new double[] { channel, distance, frequency, speed });
            //}
        }

        public void MoveDistance_F(uint channel, double distance, uint frequency)
        {
            mThreadBackground_CP.Start(
                delegate()
                {
                    if (mSensorMode[channel] == CSmarAct.SA_SENSOR_DISABLED)
                        MoveDistance_Steps_OpenLoop(channel, distance, frequency);
                    else
                    {
                        MoveDistance_CloseLoop_F(channel, distance, frequency);
                    }
                });
        }

        public void MoveDistance_CloseLoop_F_NoSleep(uint channel, double distance, uint frequency, uint speed)
        {
            distance *= mDirection[(uint)channel];
            SetSpeedClosedLoop_F_S(channel, frequency, speed);
            CSmarAct.SA_GotoPositionRelative_S(mSystemIndex, channel, (int)distance, 0);
        }

        public void MoveDistance_CloseLoop_F(uint channel, double distance, uint frequency)//uint channelIndex, int stepsize, int speed)// close loop
        {
            distance *= mDirection[(uint)channel];

            int k = 5;
            while (k-- > 0)
            {
                if (mIsBusy == false)
                    break;
                Delay(5);
            }
            SetSpeedClosedLoop(channel, frequency);
            if (mIsBusy == false)
            {
                mIsBusy = true;
                mIsIdle++;
                CSmarAct.SA_GotoPositionRelative_S(mSystemIndex, channel, (int)distance, 1000);
                Delay(5);
                mIsBusy = false;

                mGnlFunction.MY_DEBUG("CoarsePositioiner MoveDistance_CloseLoop_F:\t" + ((char)(channel + (uint)'x')).ToString() + "\t" + distance.ToString() + "\t" + frequency.ToString());
            }
            else
                mGnlFunction.MY_DEBUG("coarse busy: MoveDistance_CloseLoop, abort");
        }

        public void SetSpeedClosedLoop_F_S(uint channel, uint frequency, uint speed)
        {
            if (frequency < 50)
                frequency = 50;
            if (frequency > 18500)
                frequency = 18500;

            CSmarAct.SA_SetClosedLoopMoveSpeed_S(mSystemIndex, channel, speed);
            mFrequency_ClosedLoop[channel] = speed;
            Delay(10);
            mResult = CSmarAct.SA_SetClosedLoopMaxFrequency_S(mSystemIndex, channel, frequency);
            Delay(10);
        }

        public void SetSpeedClosedLoop_F(uint channel, uint frequency)
        {
            uint speed = (uint)Math.Abs(frequency * 1000 * 50);
            if (speed < 50)
                speed = 50;

            if (mIsBusy == false)
            {
                mIsBusy = true;
                mIsIdle++;
                CSmarAct.SA_SetClosedLoopMoveSpeed_S(mSystemIndex, channel, speed);
                mFrequency_ClosedLoop[channel] = speed;
                Delay(10);
                mResult = CSmarAct.SA_SetClosedLoopMaxFrequency_S(mSystemIndex, channel, speed * 10);
                Delay(10);
                mIsBusy = false;
            }
            else
                mGnlFunction.MY_DEBUG("coarse busy: SetSpeedClosedLoop, abort");
        }

        public void SetSpeedClosedLoop(uint channel, uint speed)
        {
            //if (speed < 50)
            //    speed = 50;

            if (mIsBusy == false)
            {
                mIsBusy = true;
                mIsIdle++;
                CSmarAct.SA_SetClosedLoopMoveSpeed_S(mSystemIndex, channel, speed);
                mFrequency_ClosedLoop[channel] = speed;
                Delay(10);
                //mResult = CSmarAct.SA_SetClosedLoopMaxFrequency_S(mSystemIndex, channel, speed * 10);
                //Delay(10);
                mIsBusy = false;
            }
            else
                mGnlFunction.MY_DEBUG("coarse busy: SetSpeedClosedLoop, abort");
        }

        public void SetSpeedClosedLoop_All_F(uint frequency)
        {
            SetSpeedClosedLoop_F(X_CP_AXIS, frequency);
            SetSpeedClosedLoop_F(Y_CP_AXIS, frequency);
            SetSpeedClosedLoop_F(Z_CP_AXIS, frequency);
        }

        public void SetSpeedClosedLoop_All_Speed(uint speed)
        {
            SetSpeedClosedLoop(X_CP_AXIS, speed);
            SetSpeedClosedLoop(Y_CP_AXIS, speed);
            SetSpeedClosedLoop(Z_CP_AXIS, speed);
        }

        public void SetPosition(uint channelIndex, int position)
        {
            if (mIsBusy == false)
            {
                mIsBusy = true;
                mIsIdle++;
                mResult = CSmarAct.SA_SetPosition_S((uint)mSystemIndex, (uint)channelIndex, position);
                Delay(5); mIsBusy = false;
                if (mResult != CSmarAct.SA_OK)
                {
                    mConnected = false;
                    mGnlFunction.MY_DEBUG("SetPosition error!\n");
                }
            }
            else
                mGnlFunction.MY_DEBUG("coarse busy: SetPosition");
        }

        public uint GetFinePosition(uint channelIndex)
        {
            uint level = 0;
            mResult = CSmarAct.SA_GetVoltageLevel_S((uint)mSystemIndex, (uint)channelIndex, ref level);
            if (mResult != CSmarAct.SA_OK)
            {
                mConnected = false;
                mGnlFunction.MY_DEBUG("GetFinePosition error!\n");
            }
            return level;
        }

        public int GetStorePosition(uint channelIndex)
        {
            return mPositionStore[channelIndex];
        }

        public int GetCurPosition(uint channelIndex)
        {
            int position = 0;
            mResult = CSmarAct.SA_GetPosition_S((uint)mSystemIndex, (uint)channelIndex, ref position);
            if (mResult != CSmarAct.SA_OK)
            {
                mConnected = false;
                mGnlFunction.MY_DEBUG("GetCurPosition error:" + mResult.ToString());
            }
            return position;
        }

        public int GetPosition(uint channelIndex)
        {
            if (mConnected == false) return -1;
            if (mSensorMode[channelIndex] == CSmarAct.SA_SENSOR_DISABLED)
                return 0;

            int position = 0;

            if (mIsBusy == false)
            {
                mIsBusy = true;
                mResult = CSmarAct.SA_GetPosition_S((uint)mSystemIndex, (uint)channelIndex, ref position);
                Delay(6); mIsBusy = false;

                if (mResult != CSmarAct.SA_OK)
                {
                    mConnected = false;
                    mGnlFunction.MY_DEBUG("GetPosition error:" + mResult.ToString());
                    mSensorMode[channelIndex] = CSmarAct.SA_SENSOR_DISABLED;
                }
                position *= (int)mDirection[(uint)channelIndex];
            }
            mPositionStore[channelIndex] = position;
            return position;
        }
        
        int redo_count = 0;
        public void SetChannelVoltage(uint channelIndex, uint V_0_150)
        {
            if (V_0_150 > 150) V_0_150 = 150;
            V_0_150 *= (uint)(4095.0 / 150.0);

            mIsIdle++;

            mResult = CSmarAct.SA_ScanMoveAbsolute_S((uint)mSystemIndex, (uint)(uint)channelIndex, V_0_150, 100000);

            if (mResult != CSmarAct.SA_OK)
            {
                mConnected = false;
                mGnlFunction.MY_DEBUG("SetChannelVoltage error!\n");
            }
            redo_count = 0;
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
