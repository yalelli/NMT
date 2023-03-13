using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using ZedGraph;
using System.Diagnostics;

namespace NMT
{
    public partial class FrmMain : Form
    {
        public void BackgroundThread_Initialization()
        {
            mThread_StateMonitoring = new Thread(new ThreadStart(ThreadFunction_SystemStateMonitoring), 1024) { IsBackground = true };
            mThread_StateMonitoring.Start();

            mThread_PositionMonitoring = new Thread(new ThreadStart(ThreadFunction_PositionMonitoring), 1024) { IsBackground = true };
            mThread_PositionMonitoring.Start();

            mThread_PositionDisplay = new Thread(new ThreadStart(ThreadFunction_PositionDisplay), 1024) { IsBackground = true };
            mThread_PositionDisplay.Start();

            mThread_MovementMonitoring = new Thread(new ThreadStart(ThreadFunction_MovementMonitoring), 1024) { IsBackground = true };
            mThread_MovementMonitoring.Start();

            mThread_JoyStick = new Thread(new ThreadStart(ThreadFunction_JoyStick_CL), 1024) { IsBackground = true };
            mThread_JoyStick.Start();

            mThread_GetFinePosition = new Thread(new ThreadStart(ThreadFunction_GetFinePosition), 1024) { IsBackground = true };
            mThread_GetFinePosition.Start();

            mThread_ScannerStabilization_PID = new Thread(new ThreadStart(ThreadFunction_ScannerStabilization_PID), 1024) { IsBackground = true };
            mThread_ScannerStabilization_PID.Start();

            mThread_ShowForceSensorCurve = new Thread(new ThreadStart(ThreadFunction_ShowForceSensorCurve)) { IsBackground = true };
            mThread_ShowForceSensorCurve.Start();
        }

        public void ThreadFunction_SystemStateMonitoring()
        {
            while (bMainFormRunning)
            {
                //Delay(10);
                Thread.Sleep(10);

                AFM_CoarsePositioner_CheckReconnection_1();// recheck coarse connection status
                label_Controller1.BackColor = Color.FromArgb(0, 255 * Convert.ToInt32(mCCoarsePositioner[0].IsConnected), 0);

                //Sam
                AFM_CoarsePositioner_CheckReconnection_2();// recheck coarse connection status
                label_Controller2.BackColor = Color.FromArgb(0, 255 * Convert.ToInt32(mCCoarsePositioner[1].IsConnected), 0);


                //Sam SmarAct
                //AFM_SmarActPositioner_CheckReconnection();
                //label_Controller2.BackColor = Color.FromArgb(0, 255 * Convert.ToInt32(mCSmarActPositioner.IsConnected), 0);


                AFM_CoarsePositioner_CheckReconnection_3();
                label_Controller3.BackColor = Color.FromArgb(0, 255 * Convert.ToInt32(mCScanPositioner.IsConnected), 0);

                ForceSensor_CheckReconnection();
                label_Sensor.BackColor = Color.FromArgb(0, 255 * Convert.ToInt32(serialPort.IsOpen), 0);

                Joystick_CheckReconnection();
                label_Joystick.BackColor = Color.FromArgb(0, 255 * Convert.ToInt32(devName.name == "SpaceMouse Pro"), 0);
            }
        }

        public void ThreadFunction_PositionMonitoring()
        {
            while (bMainFormRunning)
            {
                //Delay(10);
                Thread.Sleep(10);

                iPositionX = mCCoarsePositioner[0].GetCurPosition(mCaxis_X);
                iPositionY = mCCoarsePositioner[0].GetCurPosition(mCaxis_Y);
                iPositionZ = mCCoarsePositioner[0].GetCurPosition(mCaxis_Z);
                //iPositionZ = mCSmarActPositioner.GetCurPosition(mCaxis_Z);//SAM

                dPositionX = iPositionX / 1000.0;
                dPositionY = iPositionY / 1000.0;
                dPositionZ = iPositionZ / 1000.0;
            }
        }

        public void ThreadFunction_PositionDisplay()
        {
            while (bMainFormRunning)
            {
                //Delay(200);
                Thread.Sleep(200);

                label_Encoder_X.Text = (dPositionX * iXP).ToString("f3");
                label_Encoder_Y.Text = (dPositionY * iYP).ToString("f3");
                label_Encoder_Z.Text = (dPositionZ * iZP).ToString("f3");
            }
        }

        private void ThreadFunction_JoyStick_OL()
        {
            while (bMainFormRunning)
            {
                //Delay(100);

                if (bContinuous)
                {
                    Thread.Sleep(1);

                    if (mCCoarsePositioner[0].IsConnected)
                    {
                        uint[] uiFrequency = mGetFunctions.GetXYFrequency_OL(iXData, iYData);
                        uint uiXFrequency = uiFrequency[0];
                        uint uiYFrequency = uiFrequency[1];
                        uint uiZFrequency = mGetFunctions.GetZFrequency_OL(iZData);

                        if (uiXFrequency != 0 && bMoveX && bEnableX)
                        {
                            bJoystickXCur = true;
                            if (iXData > 0)
                            {
                                iCurSteps = iBtnSteps;
                            }
                            else
                            {
                                iCurSteps = -iBtnSteps;
                            }
                            mCCoarsePositioner[0].MoveCoarse_OpenLoop(mCaxis_X, iCurSteps, uiAmplitude, uiXFrequency);
                        }
                        else
                        {
                            bJoystickXCur = false;
                            if (bJoystickXLast && !bJoystickXCur)
                            {
                                mCCoarsePositioner[0].Stop(mCaxis_X);
                            }
                        }
                        bJoystickXLast = bJoystickXCur;

                        if (uiYFrequency != 0 && bMoveY && bEnableY)
                        {
                            bJoystickYCur = true;
                            if (iYData > 0)
                            {
                                iCurSteps = -iBtnSteps;
                                mCCoarsePositioner[0].MoveCoarse_OpenLoop(mCaxis_Y, iCurSteps, uiAmplitude, uiYFrequency);
                            }
                            else
                            {
                                iCurSteps = iBtnSteps;
                                if (bMoveYDown)
                                {
                                    mCCoarsePositioner[0].MoveCoarse_OpenLoop(mCaxis_Y, iCurSteps, uiAmplitude, uiYFrequency);
                                }
                            }
                        }
                        else
                        {
                            bJoystickYCur = false;
                            if (bJoystickYLast && !bJoystickYCur)
                            {
                                mCCoarsePositioner[0].Stop(mCaxis_Y);
                            }
                        }
                        bJoystickYLast = bJoystickYCur;

                        if (uiZFrequency != 0 && bMoveZ && bEnableZ)
                        {
                            bJoystickZCur = true;
                            if (iZData > 0)
                            {
                                iCurSteps = -iBtnSteps;
                            }
                            else
                            {
                                iCurSteps = iBtnSteps;
                            }
                            mCCoarsePositioner[0].MoveCoarse_OpenLoop(mCaxis_Z, iCurSteps, uiAmplitude, uiZFrequency);
                        }
                        else
                        {
                            bJoystickZCur = false;
                            if (bJoystickZLast && !bJoystickZCur)
                            {
                                mCCoarsePositioner[0].Stop(mCaxis_Z);
                            }
                        }
                        bJoystickZLast = bJoystickZCur;
                    }
                    else
                    {
                        //Delay(100);
                        Thread.Sleep(100);
                    }
                }

                if (bStep)
                {
                    Thread.Sleep(100);

                    if (mCCoarsePositioner[0].IsConnected)
                    {
                        if (Math.Abs(iXData) > 50000 && bMoveX && bEnableX)
                        {
                            bJoystickXCur = true;
                            if (iXData > 0)
                            {
                                dStep = -dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                            }
                            else
                            {
                                dStep = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                            }

                            if (bJoystickXLast != bJoystickXCur)
                            {
                                mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_X, dStep);
                            }
                        }
                        else
                        {
                            bJoystickXCur = false;
                        }
                        bJoystickXLast = bJoystickXCur;

                        if (Math.Abs(iYData) > 50000 && bMoveY && bEnableY)
                        {
                            bJoystickYCur = true;
                            if (iYData > 0)
                            {
                                dStep = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);

                                if (bJoystickYLast != bJoystickYCur)
                                {
                                    mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dStep);
                                }
                            }
                            else
                            {
                                if (bMoveYDown)
                                {
                                    dStep = -dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);

                                    if (bJoystickYLast != bJoystickYCur)
                                    {
                                        mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dStep);
                                    }
                                }
                            }
                        }
                        else
                        {
                            bJoystickYCur = false;
                        }
                        bJoystickYLast = bJoystickYCur;

                        if (Math.Abs(iZData) > 50000 && bMoveZ && bEnableZ)
                        {
                            bJoystickZCur = true;
                            if (iZData > 0)
                            {
                                dStep = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                            }
                            else
                            {
                                dStep = -dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);
                            }

                            if (bJoystickZLast != bJoystickZCur)
                            {
                                mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Z, dStep);
                            }
                        }
                        else
                        {
                            bJoystickZCur = false;
                        }
                        bJoystickZLast = bJoystickZCur;
                    }
                    else
                    {
                        //Delay(100);
                        Thread.Sleep(100);
                    }
                }
            }
        }

        private void ThreadFunction_JoyStick_CL()
        {
            while (bMainFormRunning)
            {
                //Delay(100);

                if (bContinuous)
                {
                    Thread.Sleep(1);

                    if (mCCoarsePositioner[0].IsConnected)
                    {
                        double[] dDistance = mGetFunctions.GetXYDistance_CL(iXData, iYData);
                        double dXDistance = dDistance[0];
                        double dYDistance = dDistance[1];
                        double dZDistance = mGetFunctions.GetZDistance_CL(iZData);

                        if (dXDistance != 0 && bMoveX && bEnableX)
                        {
                            bJoystickXCur = true;
                            double dX;

                            if (iXData > 0)
                            {
                                dX = dXDistance;
                                mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_X, dX * iXP);
                            }
                            else
                            {
                                dX = dXDistance;
                                if (bMoveXDown)
                                {
                                    mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_X, dX * iXN);
                                }
                            }
                        }
                        else
                        {
                            bJoystickXCur = false;
                            if (bJoystickXLast && !bJoystickXCur)
                            {
                                mCCoarsePositioner[0].MoveDistance_CloseLoop_Absolute(mCaxis_X, iPositionX);
                            }
                        }
                        bJoystickXLast = bJoystickXCur;

                        if (dYDistance != 0 && bMoveY && bEnableY)
                        {
                            bJoystickYCur = true;
                            double dY;
                            if (iYData < 0)
                            {
                                dY = dYDistance;
                                mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dY * iYP);
                            }
                            else
                            {
                                dY = dYDistance;
                                if (bMoveYDown)
                                {
                                    mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dY * iYN);
                                }
                            }
                        }
                        else
                        {
                            bJoystickYCur = false;
                            if (bJoystickYLast && !bJoystickYCur)
                            {
                                mCCoarsePositioner[0].MoveDistance_CloseLoop_Absolute(mCaxis_Y, iPositionY);
                            }
                        }
                        bJoystickYLast = bJoystickYCur;

                        if (dZDistance != 0 && bMoveZ && bEnableZ)
                        {
                            bJoystickZCur = true;
                            double dZ;
                            if (iZData > 0)
                            {
                                dZ = dZDistance;
                                mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Z, dZ * iZP);
                            }
                            else
                            {
                                dZ = dZDistance;
                                if (bMoveZDown)
                                {
                                    mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Z, dZ * iZN);
                                }
                            }
                        }
                        else
                        {
                            bJoystickZCur = false;
                            if (bJoystickZLast && !bJoystickZCur)
                            {
                                mCCoarsePositioner[0].MoveDistance_CloseLoop_Absolute(mCaxis_Z, iPositionZ);
                            }
                        }
                        bJoystickZLast = bJoystickZCur;
                    }
                    else
                    {
                        //Delay(100);
                        Thread.Sleep(100);
                    }
                }

                if (bStep)
                {
                    Thread.Sleep(100);

                    if (mCCoarsePositioner[0].IsConnected)
                    {
                        //if (Math.Abs(iXData) > 50000 && bMoveX && bEnableX)
                        if (Math.Abs(iXData) > 25000 && bMoveX && bEnableX)
                        {
                            bJoystickXCur = true;
                            if (iXData > 0)
                            {
                                dStep = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);

                                if (bJoystickXLast != bJoystickXCur)
                                {
                                    mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_X, dStep * iXP);
                                }
                            }
                            else
                            {
                                if (bMoveXDown)
                                {
                                    dStep = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);

                                    if (bJoystickXLast != bJoystickXCur)
                                    {
                                        mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_X, dStep * iXN);
                                    }
                                }
                            }
                        }
                        else
                        {
                            bJoystickXCur = false;
                        }
                        bJoystickXLast = bJoystickXCur;

                        //if (Math.Abs(iYData) > 50000 && bMoveY && bEnableY)
                        if (Math.Abs(iYData) > 25000 && bMoveY && bEnableY)
                        {
                            bJoystickYCur = true;
                            if (iYData < 0)
                            {
                                dStep = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);

                                if (bJoystickYLast != bJoystickYCur)
                                {
                                    mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dStep * iYP);
                                }
                            }
                            else
                            {
                                if (bMoveYDown)
                                {
                                    dStep = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);

                                    if (bJoystickYLast != bJoystickYCur)
                                    {
                                        mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dStep * iYN);
                                    }
                                }
                            }
                        }
                        else
                        {
                            bJoystickYCur = false;
                        }
                        bJoystickYLast = bJoystickYCur;

                        //if (Math.Abs(iZData) > 50000 && bMoveZ && bEnableZ)
                        if (Math.Abs(iZData) > 25000 && bMoveZ && bEnableZ)
                        {
                            bJoystickZCur = true;
                            if (iZData > 0)
                            {
                                dStep = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);

                                if (bJoystickZLast != bJoystickZCur)
                                {
                                    mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Z, dStep * iZP);
                                }
                            }
                            else
                            {
                                if (bMoveZDown)
                                {
                                    dStep = dStep_CL[iStepSpeed - 1] * Math.Pow(10, 3);

                                    if (bJoystickZLast != bJoystickZCur)
                                    {
                                        mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Z, dStep * iZN);
                                    }
                                }
                            }
                        }
                        else
                        {
                            bJoystickZCur = false;
                        }
                        bJoystickZLast = bJoystickZCur;
                    }
                    else
                    {
                        //Delay(100);
                        Thread.Sleep(100);
                    }
                }
            }
        }


        //运动监控
        public void ThreadFunction_MovementMonitoring()
        {
            while (bMainFormRunning)
            {
                //Delay(1);
                Thread.Sleep(1);

                if (bApproach)
                {
                    bMoveX = bMoveY = bMoveZ = false;

                    if (dForce >= Math.Abs(dApproachForceThreshold))
                    {
                        bApproachFirstIn = true;
                        //当第一次检测到力大于所设阈值时，nanomotor停止0.5s
                        mCCoarsePositioner[0].Stop(mCaxis_Y); // MoveDistance_CloseLoop(mCaxis_Y, dApproachStep * iYP);    //移动方向   
                        Thread.Sleep(500);      //0.5s

                        //继续检测如果还是大于阈值
                        if (dForce >= Math.Abs(dApproachForceThreshold))
                            bApproach = false;  
                    
                        bApproachFirstIn = false;
                    }
                }
                else
                {
                    if (!bIndent)
                    {
                        bMoveX = bMoveY = bMoveZ = true;

                        if (ckbSafetyMode.Checked && dSensorForceThreshold != 0)
                        {
                            if (dForce >= Math.Abs(dSensorForceThreshold))
                            {
                                bMoveYDown = false;
                            }
                            else
                            {
                                bMoveYDown = true;
                            }
                        }
                        else
                        {
                            bMoveYDown = true;
                        }
                    }
                    else
                    {
                        bMoveX = bMoveY = bMoveZ = false;

                        if (iIndentTypeIndex == 0)
                        {
                            if ((dDisplacement_nm_Indent >= dStartDistance_Indent + Math.Abs(dIndentDistance)) || (dForce >= dIndentForceThresholdMax))
                            {
                                bIndent = false;
                                bWhileIndent = false;
                                //bExtend = false;
                                bIndentExtend = false;
                            }
                            //else
                            //{
                            //    if (dDisplacement_nm_Indent >= dStartDistance_Indent + Math.Abs(dIndentDistance) - dIndentApproximateSpeed_nm / uiIndentSpeedIndex)
                            //    {
                            //        bIndentExtend = true;
                            //    }
                            //}
                        }

                        if (iIndentTypeIndex == 1)
                        {
                            if ((dDisplacement_nm_ori >= dStartDistance_ori + Math.Abs(dIndentDistance)) || (dForce >= dIndentForceThresholdMax))
                            {
                                bIndent = false;
                                bWhileIndent = false;
                                //bExtend = false;
                                bIndentExtend = false;
                            }
                            //else
                            //{
                            //    if (dDisplacement_nm_ori >= dStartDistance_ori + Math.Abs(dIndentDistance) - dIndentApproximateSpeed_nm / uiIndentSpeedIndex)
                            //    {
                            //        bIndentExtend = true;
                            //    }
                            //}
                        }

                        if (iIndentTypeIndex == 2)
                        {
                            if (dForce >= dIndentStartForce + Math.Abs(dIndentForceThreshold))
                            {
                                bIndent = false;
                                bWhileIndent = false;
                                //bExtend = false;
                                bIndentExtend = false;

                                //dForceDistance_test = dDisplacement_nm_ori;
                            }
                            //else
                            //{
                            //    if (dForce >= dIndentStartForce + Math.Abs(dIndentForceThreshold) - uiIndentSpeedIndex * 10)
                            //    {
                            //        bIndentExtend = true;
                            //    }
                            //}
                        }
                    }
                }
            }
        }


        public void ThreadFunction_Approach()
        {
            while (bWhileApproach)
            {
                //Delay(iApproachDelay);
                Thread.Sleep(iApproachDelay);

                if (bApproach)
                {
                    if (!bApproachFirstIn)
                        mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dApproachStep * iYP);    //移动方向
                }
                else
                {
                    bWhileApproach = false;
                    //mCCoarsePositioner[0].MoveDistance_CloseLoop_Absolute(mCaxis_Y, iPositionZ);
                    break;
                }
            }

            while (bWhileAdjust)
            {
                //Delay(25);
                Thread.Sleep(25);

                //太小就向前压
                if (dForce - Math.Abs(dApproachForceThreshold) < -10)//the same direction of approach//Sensor A:0.5,Sensor C:10
                {
                    iAdjustNum = 0;
                    uint uiResult = mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, 2 * iZP);
                }
                else if (dForce - Math.Abs(dApproachForceThreshold) > 10)//the negative direction of approach//Sensor A:0.5,Sensor C:10
                {
                    iAdjustNum = 0;
                    uint uiResult = mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, 2 * iZN);
                }
                else
                {
                    iAdjustNum++;
                    if (iAdjustNum == 10)
                    {
                        iAdjustNum = 0;
                        bWhileAdjust = false;
                    }
                }
            }

            if (!bStopApproach)
            {
                if (txbApproachRetreatDistance.Text != "" && txbApproachRetreatDistance.Text != null)
                {
                    mCCoarsePositioner[0].MoveDistance_CloseLoop(mCaxis_Y, dApproachRetreatDistance * iZN);
                }
            }

            if (!bCN)
            {
                btnAutoApproach.Text = "Auto Approach";
            }
            else
            {
                btnAutoApproach.Text = "自动靠近";
            }

            AutoApproachEnabledControls(true);
            Delay(10);

            mThread_AutoApproach.Abort();
        }



        public void ThreadFunction_Indent()
        {
            //记录Indentation开始时，scanner的原始距离值和原始压缩值
            dIndentStartForce = dForce;
            dStartDistance_ori = dDisplacement_nm_ori;
            dStartDistance_Indent = dDisplacement_nm_Indent;

            bShowCurve = true;
            bExtend = true;
            bWithdraw1 = false;
            bWithdraw2 = false;

            //当在Indentation中时
            while (bWhileIndent)
            {
                //Sam
                if (!bPause)
                {
                    bStabilization = false;

                    if (bIndent)
                    {
                        //if (!bIndentExtend)
                        {
                            //一直加压
                            uiCurrentBit += uiIndentSpeed;

                            if (uiCurrentBit >= uiMaxADCValue)
                            {
                                uiCurrentBit = uiMaxADCValue;
                                bIndent = false;
                                bWhileIndent = false;
                                dForceDistance_test = dDisplacement_nm_ori;
                            }
                            mCScanPositioner.MoveToFinePosition(mCaxis_F, uiCurrentBit, 10, 20);
                            //Delay(iIndentDelay);
                            Thread.Sleep(iIndentDelay);
                        }
                    }
                    else
                    {
                        //这里一直进不来，因为多线程赋值标志位的原因，indent一直在bWhileIndent之后赋值为false
                        bWhileIndent = false;
                    }
                }
            }

            Delay(30);

            //压缩等待
            if (iExtendDelay > 0)
            {
                bExtendDelay = true;

                //bExtend = true;
                dSetDAC = dReadDAC;
                bStabilization = true;

                //Delay(iExtendDelay);
                Thread.Sleep(iExtendDelay);
                bStabilization = false;

                bExtendDelay = false;
            }

            bExtend = false;

            //Sam
            if (!bWithdrawDelay)
            {
                dPointXLast_2 = dPointXLast_1;
                dPointYLast_2 = dPointYLast_1;
            }

            //indentation结束，开始回退，结束时回退到0
            while (bWhileIndentWithdraw)
            {
                bStabilization = false;

                if (!bPause)
                {
                    //当当前读取值小于Scanner初始值时，回退结束
                    if (dReadDAC > dInitValue_d)
                    {
                        //如果当前模式选择是位移或是形变,
                        //且 回退距离>0 && 回退保持时间>0时（bWithdrawDelay）,
                        //且 当前形变刚刚到达预设定形变时
                        //ps:bWithdrawDelay在indent按下后判断赋值
                        if (((iIndentTypeIndex == 0) && (dDisplacement_nm_Indent <= dStartDistance_Indent + dIndentDistance - dWithdrawDisp) && bWithdrawDelay)
                            || ((iIndentTypeIndex == 1) && (dDisplacement_nm_ori <= dStartDistance_ori + dIndentDistance - dWithdrawDisp) && bWithdrawDelay))
                        {
                            //当到达阈值后，停止画线
                            bWithdraw1 = false;
                            bWithdraw2 = false;
                            dSetDAC = dReadDAC;

                            bStabilization = true;
                            Thread.Sleep(iWithdrawDelay);
                            bStabilization = false;

                            bWithdrawDelay = false;
                        }
                        else
                        {
                            //阈值和延时都有但未到达
                            if (bWithdrawDelay)
                            {
                                bWithdraw1 = true;
                                bWithdraw2 = false;
                            }//到达/ 没有阈值和延时
                            else
                            {
                                bWithdraw1 = false;
                                bWithdraw2 = true;
                            }

                            //当当前电压值比在indentation中设定的速度值还小，将当前电压值置0，并关闭indentation的回退
                            if (uiCurrentBit <= uiIndentSpeed)
                            {
                                uiCurrentBit = 0;
                                bWhileIndentWithdraw = false;
                            }
                            else
                            {
                                uiCurrentBit -= uiIndentSpeed;
                            }

                            mCScanPositioner.MoveToFinePosition(mCaxis_F, uiCurrentBit, 10, 20);
                            //Delay(iIndentDelay);
                            Thread.Sleep(iIndentDelay);

                            //if (dReadDAC > dInitValue_d + uiIndentSpeedIndex * 100)
                            //{
                            //    if (uiCurrentBit <= uiIndentSpeed)
                            //    {
                            //        uiCurrentBit = 0;
                            //        bWhileIndentWithdraw = false;
                            //    }
                            //    else
                            //    {
                            //        uiCurrentBit -= uiIndentSpeed;
                            //    }
                            //    mCScanPositioner.MoveToFinePosition(mCaxis_F, uiCurrentBit, 10, 20);
                            //    Delay(iIndentDelay);
                            //}
                            //else
                            //{
                            //    if (uiCurrentBit <= uiIndentSpeed / uiIndentSpeedIndex)
                            //    {
                            //        uiCurrentBit = 0;
                            //        bWhileIndentWithdraw = false;
                            //    }
                            //    else
                            //    {
                            //        uiCurrentBit -= uiIndentSpeed / uiIndentSpeedIndex;
                            //    }
                            //    mCScanPositioner.MoveToFinePosition(mCaxis_F, uiCurrentBit, 10, 20);
                            //    Delay(iIndentDelay);
                            //}
                        }
                    }
                    else
                    {
                        bWhileIndentWithdraw = false;
                    }
                }
            }

            bShowCurve = false;
            //bFirstIndent = false;
            bIndent = false;

            bIndentation = false;
            if (!bCompression)
            {
                if (!bCN)
                {
                    btnIndentation.Text = "Start Indentation";
                }
                else
                {
                    btnIndentation.Text = "开始压痕";
                }
            }
            else
            {
                if (!bCN)
                {
                    btnCompression.Text = "Start Compression";
                }
                else
                {
                    btnCompression.Text = "开始压缩";
                }
            }

            IndentationEnabledControls(true);

            if (bHighFreq)
            {
                serialPort.Write("5f\r\n");
                //Delay(1000);
                frmHighFreqWaiting.Hide();

                StringListToPointList();
                ShowHighFreqCurve();
            }

            //暂停按钮失能
            bPauseIndent.Enabled = false;

            SaveData();
            //SaveDataNoMissing();

            Delay(50);
            dSetDAC = dInitValue_d;
            bStabilization = true;

            mThread_ShowCurve.Abort();
            mThread_Indent.Abort();
        }


        public void ThreadFunction_ShowCurve()
        {
            while (bMainFormRunning)
            {
                //Delay(25);
                Thread.Sleep(25);

                if (bIndentation && !bHighFreq && bShowCurve /*&& bGetPoint*/)
                {
                    double x1 = (double)((decimal)dDisplacement_nm_ori - (decimal)dStartDistance_ori);
                    double x2 = (double)((decimal)dDisplacement_nm_Indent - (decimal)dStartDistance_Indent);
                    double y = dForce;

                    if (!bCompression)
                    {
                        //当向下的压缩开始到压到底时，这里的曲线没问题
                        if (bExtend)
                        {
                            //如果没有压缩到底的保持时间，那么就不计算
                            if (!bExtendDelay)
                            {
                                zgcIndentation.GraphPane.CurveList.Clear();
                                listIndentExtend.Add(x2, y);
   
                                zgcIndentation_ori.GraphPane.AddCurve("", listIndentExtend_ori, Color.Blue, SymbolType.None);
                                zgcIndentation.GraphPane.AddCurve("", listIndentExtend, Color.Blue, SymbolType.None);
                            
                                //保存最后一次进入函数的数据
                                dPointXLast_1 = x2;
                                dPointYLast_1 = y;
                            }
                        }//当向下压缩到底开始回撤时
                        else
                        {
                            if (bWithdraw1)
                            {
                                zgcIndentation.GraphPane.CurveList.Clear();
                                
                                //计算压缩曲线和回退曲线的delta
                                if (bFirstWithdraw_1)
                                {
                                    deltaX_1 = x2 - dPointXLast_1;
                                    deltaY_1 = y - dPointYLast_1;

                                    listIndentWithdraw.Add(dPointXLast_1, dPointYLast_1);
                                    bFirstWithdraw_1 = false;
                                }
                                else
                                {
                                    listIndentWithdraw.Add(x2 - deltaX_1, y - deltaY_1);
                                    zgcIndentation.GraphPane.AddCurve("", listIndentExtend, Color.Blue, SymbolType.None);
                                    zgcIndentation.GraphPane.AddCurve("", listIndentWithdraw, Color.Red, SymbolType.None);

                                    dPointXLast_ori_2 = x1;
                                    dPointYLast_ori_2 = y;
                                    dPointXLast_2 = x2;
                                    dPointYLast_2 = y;
                                }
                            }

                            if (bWithdraw2)
                            {
                                zgcIndentation.GraphPane.CurveList.Clear();

                                if (bFirstWithdraw_2)
                                {
                                    deltaX_2 = x2 - dPointXLast_2;
                                    deltaY_2 = y - dPointYLast_2;
                                    listIndentWithdraw.Add(dPointXLast_2, dPointYLast_2);
                                    bFirstWithdraw_2 = false;
                                }

                                listIndentWithdraw.Add(x2 - deltaX_2, y - deltaY_2);
                                zgcIndentation.GraphPane.AddCurve("", listIndentExtend, Color.Blue, SymbolType.None);
                                zgcIndentation.GraphPane.AddCurve("", listIndentWithdraw, Color.Red, SymbolType.None);
                            }
                        }

                        zgcIndentation.AxisChange();//画到zgcIndentation控件中
                        zgcIndentation.Refresh();//重新刷新
                    }
                    else
                    {
                        zgcCompression_ori.GraphPane.CurveList.Clear();
                        zgcCompress.GraphPane.CurveList.Clear();

                        if (bExtend)
                        {
                            if(bIndent)     //Sam
                            {
                                listIndentExtend_ori.Add(x1, y);
                                listIndentExtend.Add(x2, y);
                                zgcCompression_ori.GraphPane.AddCurve("", listIndentExtend_ori, Color.Blue, SymbolType.None);

                                dPointXLast_ori_1 = x1;
                                dPointYLast_ori_1 = y;
                                dPointXLast_1 = x2;
                                dPointYLast_1 = y;
                            }
                            zgcCompress.GraphPane.AddCurve("", listIndentExtend, Color.Blue, SymbolType.None);
                        }
                        else
                        {
                            if (bFirstWithdraw_1)
                            {
                                listIndentWithdraw_ori.Add(dPointXLast_ori_1, dPointYLast_ori_1);
                                listIndentWithdraw.Add(dPointXLast_1, dPointYLast_1);

                                //Sam 测试将回撤时对齐
                                deltaX_Sam = x2 - dPointXLast_1;
                                deltaY_Sam = y - dPointYLast_1;

                                bFirstWithdraw_1 = false;
                            }

                            listIndentWithdraw_ori.Add(x1, y);
     
                            //Sam
                            listIndentWithdraw.Add(x2 - deltaX_Sam, y - deltaY_Sam);
                            //listIndentWithdraw.Add(x2, y);    //原版

                            zgcCompress.GraphPane.AddCurve("", listIndentExtend, Color.Blue, SymbolType.None);
                            zgcCompress.GraphPane.AddCurve("", listIndentWithdraw, Color.Red, SymbolType.None);
                        }

                        zgcCompress.AxisChange();
                        zgcCompress.Refresh();
                    }
                }
            }
        }

        public void ThreadFunction_GetFinePosition()
        {
            while (bMainFormRunning)
            {
                //Delay(200);
                Thread.Sleep(200);

                if (!bStabilization)
                {
                    if (mCScanPositioner.IsConnected)
                    {
                        uint uiCurAdc = mCScanPositioner.GetFinePosition(mCaxis_F);
                        txbCurVOL.Text = uiCurAdc.ToString();
                    }

                    if (serialPort.IsOpen)
                    {
                        txbCurDAC.Text = dReadDAC.ToString();
                        txbCurDisp.Text = dDisplacement_nm_ori.ToString();
                    }

                    if (bTestRecord)
                    {
                        txbTotal.AppendText(txbCurVOL.Text + "     " + txbCurDAC.Text + "     " + txbCurDisp.Text + "\r\n");
                    }
                }
            }
        }

        public void ThreadFunction_ScannerStabilization_PID()
        {
            while (bMainFormRunning)
            {
                //Delay(50);//延时越小，调整速度越快，越不容易稳定
                Thread.Sleep(50);

                if (bStabilization)
                {
                    if (serialPort.IsOpen && mCScanPositioner.IsConnected)
                    {
                        try
                        {
                            delta = (decimal)(dSetDAC - dReadDAC);
                            dSum += delta;
                            uiCurrentBit = (uint)(uiCurrentBit + u * (P * delta + I * dSum + D * (delta - deltaLast)));
                            
                            //
                            mCScanPositioner.MoveToFinePosition(mCaxis_F, uiCurrentBit, 10, 20);
                            deltaLast = delta;

                            txbCurVOL.Text = uiCurrentBit.ToString();
                            txbCurDAC.Text = dReadDAC.ToString();
                            txbCurDisp.Text = dDisplacement_nm_ori.ToString();

                            if (bTestRecord)
                            {
                                txbTotal.AppendText(txbCurVOL.Text + "     " + txbCurDAC.Text + "     " + txbCurDisp.Text + "\r\n");
                            }
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        public void ThreadFunction_ShowForceSensorCurve()
        {
            while (bMainFormRunning)
            {
                Thread.Sleep(10);

                if (listCur_F.Count != 0)
                {
                    try
                    {
                        zgcForceSensor_F.GraphPane.CurveList.Clear();
                        zgcForceSensor_F.GraphPane.AddCurve("", listCur_F, Color.Blue, SymbolType.None);
                        zgcForceSensor_F.AxisChange();//画到zedGraphControl1控件中
                        zgcForceSensor_F.Refresh();//重新刷新
                    }
                    catch (Exception ex)
                    { }
                }

                if (listCur_d.Count != 0)
                {
                    try
                    {
                        zgcForceSensor_d.GraphPane.CurveList.Clear();
                        zgcForceSensor_d.GraphPane.AddCurve("", listCur_d, Color.Blue, SymbolType.None);
                        zgcForceSensor_d.AxisChange();//画到zedGraphControl1控件中
                        zgcForceSensor_d.Refresh();//重新刷新
                    }
                    catch (Exception ex)
                    { }
                }
            }
        }

    }
}
