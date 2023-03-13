using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMT.Joystick
{
    public class GetFunctions
    {
        public uint uiDelayMax = 30, uiDelayMin = 10;
        public uint uiFrequencyMax;
        public int iDataMin = 40000, iDataMax = 80000;
        public int iDataMinZ = 40000, iDataMaxZ = 80000;
        public double dDistanceMax;


        public uint[] GetXYFrequency_OL(int iDataX, int iDataY)
        {
            uint[] uiFrequency = new uint[2] { 0, 0 };

            double dLine = Math.Sqrt((long)iDataX * (long)iDataX + (long)iDataY * (long)iDataY);
            if (dLine >= iDataMin && dLine <= iDataMax)
            {
                uiFrequency[0] = (uint)(((double)Math.Abs(iDataX) / (double)iDataMax) * uiFrequencyMax);
                uiFrequency[1] = (uint)(((double)Math.Abs(iDataY) / (double)iDataMax) * uiFrequencyMax);
            }
            else
            {
                if (dLine > iDataMax)
                {
                    uiFrequency[0] = (uint)(((double)Math.Abs(iDataX) / dLine) * uiFrequencyMax);
                    uiFrequency[1] = (uint)(((double)Math.Abs(iDataY) / dLine) * uiFrequencyMax);
                }
                else
                {
                    uiFrequency[0] = 0;
                    uiFrequency[1] = 0;
                }
            }

            return uiFrequency;
        }

        public uint GetZFrequency_OL(int iData)
        {
            uint uiFrequency = 0;

            if (Math.Abs(iData) >= iDataMinZ && Math.Abs(iData) <= iDataMaxZ)
            {
                uiFrequency = (uint)(((double)Math.Abs(iData) - iDataMinZ) / (uint)(iDataMaxZ - iDataMinZ) * uiFrequencyMax);
            }
            else
            {
                if (Math.Abs(iData) < iDataMinZ)
                {
                    uiFrequency = 0;
                }
                else
                {
                    uiFrequency = uiFrequencyMax;
                }
            }

            return uiFrequency;
        }

        public uint GetXYZFrequency_OL(int iData)
        {
            uint uiFrequency = 0;

            if (Math.Abs(iData) < iDataMin)
            {
                uiFrequency = 0;
            }
            else if (Math.Abs(iData) > iDataMax)
            {
                uiFrequency = uiFrequencyMax;
            }
            else
            {
                uiFrequency = (uint)((double)Math.Abs(iData) - iDataMin) / (uint)(iDataMax - iDataMin) * uiFrequencyMax;
            }

            return uiFrequency;
        }

        public double[] GetXYDistance_CL(int iDataX, int iDataY)
        {
            double[] dDistance = new double[2] { 0, 0 };

            double dLine = Math.Sqrt((long)iDataX * (long)iDataX + (long)iDataY * (long)iDataY);
            if (dLine >= iDataMin && dLine <= iDataMax)
            {
                dDistance[0] = (((double)Math.Abs(iDataX) / (double)iDataMax) * dDistanceMax);
                dDistance[1] = (((double)Math.Abs(iDataY) / (double)iDataMax) * dDistanceMax);
            }
            else
            {
                if (dLine > iDataMax)
                {
                    dDistance[0] = (((double)Math.Abs(iDataX) / dLine) * dDistanceMax);
                    dDistance[1] = (((double)Math.Abs(iDataY) / dLine) * dDistanceMax);
                }
                else
                {
                    dDistance[0] = 0;
                    dDistance[1] = 0;
                }
            }

            return dDistance;
        }

        public double GetZDistance_CL(int iData)
        {
            double dDistance = 0;

            if (Math.Abs(iData) >= iDataMinZ && Math.Abs(iData) <= iDataMaxZ)
            {
                dDistance = ((double)Math.Abs(iData) - iDataMinZ) / (iDataMaxZ - iDataMinZ) * dDistanceMax;
            }
            else
            {
                if (Math.Abs(iData) < iDataMinZ)
                {
                    dDistance = 0;
                }
                else
                {
                    dDistance = dDistanceMax;
                }
            }

            return dDistance;
        }

        public double GetXYZDistance_CL(int iData)
        {
            double dDistance = 0;

            if (Math.Abs(iData) < iDataMin)
            {
                dDistance = 0;
            }
            else if (Math.Abs(iData) > iDataMax)
            {
                dDistance = dDistanceMax;
            }
            else
            {
                dDistance = ((double)Math.Abs(iData) - iDataMin) / (iDataMax - iDataMin) * dDistanceMax;//正比例函数
                //dDistance = (double)Math.Pow(Math.Abs(iData) - iDataMin, 2) / Math.Pow(iDataMax - iDataMin, 2) * dDistanceMax;//开口向上的二次函数
                //dDistance = (double)Math.Pow(Math.Abs(iData) - iDataMax, 2) / Math.Pow(iDataMin - iDataMax, 2) * (-dDistanceMax) + dDistanceMax;//开口向下的二次函数
            }

            return dDistance;
        }

        public uint GetXYZDelay(int iData)
        {
            uint uiDelay = 0;

            if (Math.Abs(iData) >= iDataMin && Math.Abs(iData) <= iDataMax)
            {
                uiDelay = (uint)(uiDelayMax - (uiDelayMax - uiDelayMin) * ((double)Math.Abs(iData) - iDataMin) / (iDataMax - iDataMin));//直线
            }
            else
            {
                if (Math.Abs(iData) > iDataMax)
                {
                    uiDelay = uiDelayMin;
                }
                else
                {
                    uiDelay = 0;
                }
            }

            return uiDelay;
        }
    }
}
