using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace NMT
{
    public partial class FrmService : Form
    {
        public string strControllerIDPath;
        public string strSensorDirectionPath;
        public string strRangeSettingsPath;
        public string strCurveSelectionPath;
        public string strDACRangePath;
        public string strAxisDirectionPath;
        public string strStiffnessCalibrationPath;

        public FrmService()
        {
            InitializeComponent();

            ckbSample.Checked = false;
            ckbScanner.Checked = true;

            strControllerIDPath = Application.StartupPath + "\\ControllerID.lfd";
            strSensorDirectionPath = Application.StartupPath + "\\SensorDirection.lfd";
            strRangeSettingsPath = Application.StartupPath + "\\RangeSettings.lfd";
            strCurveSelectionPath = Application.StartupPath + "\\CurveSelection.lfd";
            strDACRangePath = Application.StartupPath + "\\DACRange.lfd";
            strAxisDirectionPath = Application.StartupPath + "\\AxisDirection.lfd";
            strStiffnessCalibrationPath = Application.StartupPath + "\\StiffnessCalibration.lfd";

            btnLoadControllerID_Click(null, null);
            btnLoadeSensorDirection_Click(null, null);
            btnLoadRangeSettings_Click(null, null);
            btnLoadCurveSelection_Click(null, null);
            btnLoadDACRange_Click(null, null);
            btnLoadAxisDirection_Click(null, null);
            btnLoadStiffnessCalibration_Click(null, null);
        }

        private void btnLoadControllerID_Click(object sender, EventArgs e)
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
                    string[] strControllerID = new string[4];
                    strControllerID = Encoding.Default.GetString(bufControllerID).Split(',');

                    if (strControllerID.Length == 4)
                    {
                        try
                        {
                            txbNanoDriveID_1.Text = strControllerID[0];
                            txbNanoDriveID_2.Text = strControllerID[1];
                            txbScannerID.Text = strControllerID[2];
                            txbSensorID.Text = strControllerID[3];
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        private void btnUpdateControllerID_Click(object sender, EventArgs e)
        {
            using (FileStream fsWriteControllerID = new FileStream(strControllerIDPath, FileMode.Create))
            {
                if (txbNanoDriveID_1.Text != "" && txbNanoDriveID_1.Text != null
                    && txbNanoDriveID_2.Text != "" && txbNanoDriveID_2.Text != null
                    && txbScannerID.Text != "" && txbScannerID.Text != null
                    && txbSensorID.Text != "" && txbSensorID.Text != null)
                {
                    string[] strControllerID = new string[4];
                    string str = "";
                    strControllerID[0] = txbNanoDriveID_1.Text;
                    strControllerID[1] = txbNanoDriveID_2.Text;
                    strControllerID[2] = txbScannerID.Text;
                    strControllerID[3] = txbSensorID.Text;

                    for (int i = 0; i < strControllerID.Length; i++)
                    {
                        if (i < strControllerID.Length - 1)
                        {
                            str += strControllerID[i] + ",";
                        }
                        else
                        {
                            str += strControllerID[i];
                        }
                    }

                    //将字符串转换成字节数组
                    byte[] bufControllerID = System.Text.Encoding.GetEncoding("gbk").GetBytes(str);
                    fsWriteControllerID.Write(bufControllerID, 0, bufControllerID.Length);
                }
            }
        }

        private void btnLoadeSensorDirection_Click(object sender, EventArgs e)
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
                            txbForceDirection.Text = strSensorDirection[0];
                            txbForceSuffix.Text = strSensorDirection[1];
                            txbDisplacementDirection.Text = strSensorDirection[2];
                            txbDisplacementSuffix.Text = strSensorDirection[3];
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        private void btnUpdateSensorDirection_Click(object sender, EventArgs e)
        {
            using (FileStream fsWriteSensorDirection = new FileStream(strSensorDirectionPath, FileMode.Create))
            {
                if (txbForceDirection.Text != "" && txbForceDirection.Text != null
                    && txbForceSuffix.Text != "" && txbForceSuffix.Text != null
                    && txbDisplacementDirection.Text != "" && txbDisplacementDirection.Text != null
                    && txbDisplacementSuffix.Text != "" && txbDisplacementSuffix.Text != null)
                {
                    string[] strSensorDirection = new string[4];
                    string str = "";
                    strSensorDirection[0] = txbForceDirection.Text;
                    strSensorDirection[1] = txbForceSuffix.Text;
                    strSensorDirection[2] = txbDisplacementDirection.Text;
                    strSensorDirection[3] = txbDisplacementSuffix.Text;

                    for (int i = 0; i < strSensorDirection.Length; i++)
                    {
                        if (i < strSensorDirection.Length - 1)
                        {
                            str += strSensorDirection[i] + ",";
                        }
                        else
                        {
                            str += strSensorDirection[i];
                        }
                    }

                    //将字符串转换成字节数组
                    byte[] bufSensorDirection = System.Text.Encoding.GetEncoding("gbk").GetBytes(str);
                    fsWriteSensorDirection.Write(bufSensorDirection, 0, bufSensorDirection.Length);
                }
            }
        }

        private void btnLoadRangeSettings_Click(object sender, EventArgs e)
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
                            txbIndentDistanceMin.Text = strRangeSettings[0];
                            txbIndentDistanceMax.Text = strRangeSettings[1];
                            txbIndentForceThresholdMin.Text = strRangeSettings[2];
                            txbIndentForceThresholdMax.Text = strRangeSettings[3];
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        private void btnUpdateRangeSettings_Click(object sender, EventArgs e)
        {
            using (FileStream fsWriteRangeSettings = new FileStream(strRangeSettingsPath, FileMode.Create))
            {
                if (txbIndentDistanceMin.Text != "" && txbIndentDistanceMin.Text != null
                    && txbIndentDistanceMax.Text != "" && txbIndentDistanceMax.Text != null
                    && txbIndentForceThresholdMin.Text != "" && txbIndentForceThresholdMin.Text != null
                    && txbIndentForceThresholdMax.Text != "" && txbIndentForceThresholdMax.Text != null)
                {
                    string[] strRangeSettings = new string[4];
                    string str = "";
                    strRangeSettings[0] = txbIndentDistanceMin.Text;
                    strRangeSettings[1] = txbIndentDistanceMax.Text;
                    strRangeSettings[2] = txbIndentForceThresholdMin.Text;
                    strRangeSettings[3] = txbIndentForceThresholdMax.Text;

                    for (int i = 0; i < strRangeSettings.Length; i++)
                    {
                        if (i < strRangeSettings.Length - 1)
                        {
                            str += strRangeSettings[i] + ",";
                        }
                        else
                        {
                            str += strRangeSettings[i];
                        }
                    }

                    //将字符串转换成字节数组
                    byte[] bufRangeSettings = System.Text.Encoding.GetEncoding("gbk").GetBytes(str);
                    fsWriteRangeSettings.Write(bufRangeSettings, 0, bufRangeSettings.Length);
                }
            }
        }

        private void btnLoadCurveSelection_Click(object sender, EventArgs e)
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
                            ckbSample.Checked = Convert.ToBoolean(int.Parse(strCurveSelection[0]));
                            ckbScanner.Checked = Convert.ToBoolean(int.Parse(strCurveSelection[1]));
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        private void btnUpdateCurveSelection_Click(object sender, EventArgs e)
        {
            using (FileStream fsWriteCurveSelection = new FileStream(strCurveSelectionPath, FileMode.Create))
            {
                string[] strCurveSelection = new string[2];
                string str = "";
                strCurveSelection[0] = Convert.ToInt32(ckbSample.Checked).ToString();
                strCurveSelection[1] = Convert.ToInt32(ckbScanner.Checked).ToString();

                for (int i = 0; i < strCurveSelection.Length; i++)
                {
                    if (i < strCurveSelection.Length - 1)
                    {
                        str += strCurveSelection[i] + ",";
                    }
                    else
                    {
                        str += strCurveSelection[i];
                    }
                }

                //将字符串转换成字节数组
                byte[] bufCurveSelection = System.Text.Encoding.GetEncoding("gbk").GetBytes(str);
                fsWriteCurveSelection.Write(bufCurveSelection, 0, bufCurveSelection.Length);
            }
        }

        private void btnLoadDACRange_Click(object sender, EventArgs e)
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
                            txbDACMin.Text = strDACRange[0];
                            txbDACMax.Text = strDACRange[1];
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        private void btnUpdateDACRange_Click(object sender, EventArgs e)
        {
            using (FileStream fsWriteDACRange = new FileStream(strDACRangePath, FileMode.Create))
            {
                if (txbDACMin.Text != "" && txbDACMin.Text != null
                    && txbDACMax.Text != "" && txbDACMax.Text != null)
                {
                    string[] strDACRange = new string[2];
                    string str = "";
                    strDACRange[0] = txbDACMin.Text;
                    strDACRange[1] = txbDACMax.Text;

                    for (int i = 0; i < strDACRange.Length; i++)
                    {
                        if (i < strDACRange.Length - 1)
                        {
                            str += strDACRange[i] + ",";
                        }
                        else
                        {
                            str += strDACRange[i];
                        }
                    }

                    //将字符串转换成字节数组
                    byte[] bufDACRange = System.Text.Encoding.GetEncoding("gbk").GetBytes(str);
                    fsWriteDACRange.Write(bufDACRange, 0, bufDACRange.Length);
                }
            }
        }

        private void ckbSample_CheckedChanged(object sender, EventArgs e)
        {
            //if (ckbSample.Checked)
            //{
            //    ckbScanner.Checked = false;
            //}
            //else
            //{
            //    ckbScanner.Checked = true;
            //}
        }

        private void ckbScanner_CheckedChanged(object sender, EventArgs e)
        {
            //if (ckbScanner.Checked)
            //{
            //    ckbSample.Checked = false;
            //}
            //else
            //{
            //    ckbSample.Checked = true;
            //}
        }

        private void btnLoadAxisDirection_Click(object sender, EventArgs e)
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
                    string[] strAxisDirection = new string[6];
                    strAxisDirection = Encoding.Default.GetString(bufAxisDirection).Split(',');

                    if (strAxisDirection.Length == 6)
                    {
                        try
                        {
                            txbXPLeft.Text = strAxisDirection[0];
                            txbXPRight.Text = strAxisDirection[1];
                            txbYPLeft.Text = strAxisDirection[2];
                            txbYPRight.Text = strAxisDirection[3];
                            txbZPLeft.Text = strAxisDirection[4];
                            txbZPRight.Text = strAxisDirection[5];
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        private void btnUpdateAxisDirection_Click(object sender, EventArgs e)
        {
            using (FileStream fsWriteAxisDirection = new FileStream(strAxisDirectionPath, FileMode.Create))
            {
                if (txbXPLeft.Text != "" && txbXPLeft.Text != null && txbXPRight.Text != "" && txbXPRight.Text != null
                    && txbYPLeft.Text != "" && txbYPLeft.Text != null && txbYPRight.Text != "" && txbYPRight.Text != null
                    && txbZPLeft.Text != "" && txbZPLeft.Text != null && txbZPRight.Text != "" && txbZPRight.Text != null)
                {
                    string[] strAxisDirection = new string[6];
                    string str = "";
                    strAxisDirection[0] = txbXPLeft.Text;
                    strAxisDirection[1] = txbXPRight.Text;
                    strAxisDirection[2] = txbYPLeft.Text;
                    strAxisDirection[3] = txbYPRight.Text;
                    strAxisDirection[4] = txbZPLeft.Text;
                    strAxisDirection[5] = txbZPRight.Text;

                    for (int i = 0; i < strAxisDirection.Length; i++)
                    {
                        if (i < strAxisDirection.Length - 1)
                        {
                            str += strAxisDirection[i] + ",";
                        }
                        else
                        {
                            str += strAxisDirection[i];
                        }
                    }

                    //将字符串转换成字节数组
                    byte[] bufAxisDirection = System.Text.Encoding.GetEncoding("gbk").GetBytes(str);
                    fsWriteAxisDirection.Write(bufAxisDirection, 0, bufAxisDirection.Length);
                }
            }
        }

        private void btnLoadStiffnessCalibration_Click(object sender, EventArgs e)
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
                    string[] strStiffnessCalibration = new string[9];
                    strStiffnessCalibration = Encoding.Default.GetString(bufStiffnessCalibration).Split(',');

                    if (strStiffnessCalibration.Length == 9)
                    {
                        try
                        {
                            txbStepFirst.Text = strStiffnessCalibration[0];
                            txbDelayFirst.Text = strStiffnessCalibration[1];
                            txbForceFirst.Text = strStiffnessCalibration[2];
                            txbStepSecond.Text = strStiffnessCalibration[3];
                            txbDelaySecond.Text = strStiffnessCalibration[4];
                            txbForceSecond.Text = strStiffnessCalibration[5];
                            txbStepThird.Text = strStiffnessCalibration[6];
                            txbDelayThird.Text = strStiffnessCalibration[7];
                            txbForceThird.Text = strStiffnessCalibration[8];
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        private void btnUpdateStiffnessCalibration_Click(object sender, EventArgs e)
        {
            using (FileStream fsWriteStiffnessCalibration = new FileStream(strStiffnessCalibrationPath, FileMode.Create))
            {
                if (txbStepFirst.Text != "" && txbStepFirst.Text != null
                    && txbDelayFirst.Text != "" && txbDelayFirst.Text != null
                    && txbForceFirst.Text != "" && txbForceFirst.Text != null
                    && txbStepSecond.Text != "" && txbStepSecond.Text != null
                    && txbDelaySecond.Text != "" && txbDelaySecond.Text != null
                    && txbForceSecond.Text != "" && txbForceSecond.Text != null
                    && txbStepThird.Text != "" && txbStepThird.Text != null
                    && txbDelayThird.Text != "" && txbDelayThird.Text != null
                    && txbForceThird.Text != "" && txbForceThird.Text != null)
                {
                    string[] strStiffnessCalibration = new string[9];
                    string str = "";
                    strStiffnessCalibration[0] = txbStepFirst.Text;
                    strStiffnessCalibration[1] = txbDelayFirst.Text;
                    strStiffnessCalibration[2] = txbForceFirst.Text;
                    strStiffnessCalibration[3] = txbStepSecond.Text;
                    strStiffnessCalibration[4] = txbDelaySecond.Text;
                    strStiffnessCalibration[5] = txbForceSecond.Text;
                    strStiffnessCalibration[6] = txbStepThird.Text;
                    strStiffnessCalibration[7] = txbDelayThird.Text;
                    strStiffnessCalibration[8] = txbForceThird.Text;

                    for (int i = 0; i < strStiffnessCalibration.Length; i++)
                    {
                        if (i < strStiffnessCalibration.Length - 1)
                        {
                            str += strStiffnessCalibration[i] + ",";
                        }
                        else
                        {
                            str += strStiffnessCalibration[i];
                        }
                    }

                    //将字符串转换成字节数组
                    byte[] bufStiffnessCalibration = System.Text.Encoding.GetEncoding("gbk").GetBytes(str);
                    fsWriteStiffnessCalibration.Write(bufStiffnessCalibration, 0, bufStiffnessCalibration.Length);
                }
            }
        }

    }
}
