using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace NMT.Nators
{
    [DefaultPropertyAttribute("AFM paremeters")]
    public class CParameter
    {
        public bool CoarsePositionSensorConnected = true;

        public double Coarse_Max_StepSize_nm = 1783;
        public double Coarse_Max_DACValue_Bit = 4095;
    }
}
