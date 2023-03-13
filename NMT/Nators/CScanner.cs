using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace NMT.Nators
{
    class CScanner
    {
        [DllImport(@"ScanControl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern uint SCAN_OpenSystem(ref uint systemIndex, [MarshalAs(UnmanagedType.LPStr)] string locator, [MarshalAs(UnmanagedType.LPStr)] string options);

        [DllImport(@"ScanControl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern uint SCAN_CloseSystem(uint systemIndex);

        [DllImport(@"ScanControl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern uint SCAN_GetSystemLocator(uint systemIndex, ref byte outBuffer, ref uint ioBufferSize);

        [DllImport(@"ScanControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint SCAN_ScanMoveAbsolute_S(uint systemIndex, uint channelIndex, uint target, uint sacnStep, uint scanDelay);//scanDelay:us

        [DllImport(@"ScanControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint SCAN_ScanMoveRelative_S(uint systemIndex, uint channelIndex, int diff, uint sacnStep, uint scanDelay);//scanDelay:us

        [DllImport(@"ScanControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint SCAN_GetVoltageLevel_S(uint systemIndex, uint channelIndex, ref uint level);

        public const uint SCAN_OK = 0;
        public const uint SCAN_INITIALIZATION_ERROR = 1;
        public const uint SCAN_NOT_INITIALIZED_ERROR = 2;
        public const uint SCAN_NO_SYSTEMS_FOUND_ERROR = 3;
        public const uint SCAN_TOO_MANY_SYSTEMS_ERROR = 4;
        public const uint SCAN_INVALID_SYSTEM_INDEX_ERROR = 5;
        public const uint SCAN_INVALID_CHANNEL_INDEX_ERROR = 6;
        public const uint SCAN_TRANSMIT_ERROR = 7;
        public const uint SCAN_WRITE_ERROR = 8;
        public const uint SCAN_INVALID_PARAMETER_ERROR = 9;
        public const uint SCAN_READ_ERROR = 10;
        public const uint SCAN_INTERNAL_ERROR = 12;
        public const uint SCAN_WRONG_MODE_ERROR = 13;
        public const uint SCAN_PROTOCOL_ERROR = 14;
        public const uint SCAN_TIMEOUT_ERROR = 15;
        public const uint SCAN_NOTIFICATION_ALREADY_SET_ERROR = 16;
        public const uint SCAN_ID_LIST_TOO_SMALL_ERROR = 17;
        public const uint SCAN_SYSTEM_ALREADY_ADDED_ERROR = 18;
        public const uint SCAN_WRONG_CHANNEL_TYPE_ERROR = 19;
        public const uint SCAN_NO_SENSOR_PRESESCAN_ERROR = 129;
        public const uint SCAN_AMPLITUDE_TOO_LOW_ERROR = 130;
        public const uint SCAN_AMPLITUDE_TOO_HIGH_ERROR = 131;
        public const uint SCAN_FREQUENCY_TOO_LOW_ERROR = 132;
        public const uint SCAN_FREQUENCY_TOO_HIGH_ERROR = 133;
        public const uint SCAN_SCAN_TARGET_TOO_HIGH_ERROR = 135;
        public const uint SCAN_SCAN_SPEED_TOO_LOW_ERROR = 136;
        public const uint SCAN_SCAN_SPEED_TOO_HIGH_ERROR = 137;
        public const uint SCAN_SENSOR_DISABLED_ERROR = 140;
        public const uint SCAN_COMMAND_OVERRIDDEN_ERROR = 141;
        public const uint SCAN_END_STOP_REACHED_ERROR = 142;
        public const uint SCAN_WRONG_SENSOR_TYPE_ERROR = 143;
        public const uint SCAN_COULD_NOT_FIND_REF_ERROR = 144;
        public const uint SCAN_WRONG_END_EFFECTOR_TYPE_ERROR = 145;
        public const uint SCAN_RANGE_LIMIT_REACHED_ERROR = 147;
        public const uint SCAN_PHYSICAL_POSITION_UNKNOWN_ERROR = 148;
        public const uint SCAN_OUTPUT_BUFFER_OVERFLOW_ERROR = 149;
        public const uint SCAN_INVALID_COMPONESCAN_ERROR = 154;
        public const uint SCAN_PERMISSION_DENIED_ERROR = 157;
        public const uint SCAN_UNKNOWN_COMMAND_ERROR = 240;
        public const uint SCAN_OTHER_ERROR = 255;

        // configuration flags for SCAN_InitDevices;
        public const uint SCAN_SYNCHRONOUS_COMMUNICATION = 0;
        public const uint SCAN_ASYNCHRONOUS_COMMUNICATION = 1;
        public const uint SCAN_HARDWARE_RESET = 2;

        // return values from SCAN_GetInitState;
        public const uint SCAN_INIT_STATE_NONE = 0;
        public const uint SCAN_INIT_STATE_SYNC = 1;
        public const uint SCAN_INIT_STATE_ASYNC = 2;

        // configuration flags for SCAN_SetStepWhileScan_X;
        public const uint SCAN_NO_STEP_WHILE_SCAN = 0;
        public const uint SCAN_STEP_WHILE_SCAN = 1;

        // configuration flags for SCAN_SetSensorEnabled_X;
        public const uint SCAN_SENSOR_DISABLED = 0;
        public const uint SCAN_SENSOR_ENABLED = 1;
        public const uint SCAN_SENSOR_POWERSAVE = 2;

        // configuration flags for SCAN_SetReportOnComplete_A;
        public const uint SCAN_NO_REPORT_ON_COMPLETE = 0;
        public const uint SCAN_REPORT_ON_COMPLETE = 1;

        // configuration flags for SCAN_SetAccumulateRelativePositions_X;
        public const uint SCAN_NO_ACCUMULATE_RELATIVE_POSITIONS = 0;
        public const uint SCAN_ACCUMULATE_RELATIVE_POSITIONS = 1;

        // packet types (for asynchronous mode);
        public const uint SCAN_NO_PACKET_TYPE = 0;
        public const uint SCAN_ERROR_PACKET_TYPE = 1;
        public const uint SCAN_POSITION_PACKET_TYPE = 2;
        public const uint SCAN_COMPLETED_PACKET_TYPE = 3;
        public const uint SCAN_STATUS_PACKET_TYPE = 4;
        public const uint SCAN_ANGLE_PACKET_TYPE = 5;
        public const uint SCAN_VOLTAGE_LEVEL_PACKET_TYPE = 6;
        public const uint SCAN_SENSOR_TYPE_PACKET_TYPE = 7;
        public const uint SCAN_SENSOR_ENABLED_PACKET_TYPE = 8;
        public const uint SCAN_END_EFFECTOR_TYPE_PACKET_TYPE = 9;
        public const uint SCAN_GRIPPER_OPENING_PACKET_TYPE = 10;
        public const uint SCAN_FORCE_PACKET_TYPE = 11;
        public const uint SCAN_MOVE_SPEED_PACKET_TYPE = 12;
        public const uint SCAN_PHYSICAL_POSITION_KNOWN_PACKET_TYPE = 13;
        public const uint SCAN_POSITION_LIMIT_PACKET_TYPE = 14;
        public const uint SCAN_ANGLE_LIMIT_PACKET_TYPE = 15;
        public const uint SCAN_SAFE_DIRECTION_PACKET_TYPE = 16;
        public const uint SCAN_SCALE_PACKET_TYPE = 17;
        public const uint SCAN_INVALID_PACKET_TYPE = 255;

        // channel status codes;
        public const uint SCAN_STOPPED_STATUS = 0;
        public const uint SCAN_STEPPING_STATUS = 1;
        public const uint SCAN_SCANNING_STATUS = 2;
        public const uint SCAN_HOLDING_STATUS = 3;
        public const uint SCAN_TARGET_STATUS = 4;
        public const uint SCAN_MOVE_DELAY_STATUS = 5;
        public const uint SCAN_CALIBRATING_STATUS = 6;
        public const uint SCAN_FINDING_REF_STATUS = 7;
        public const uint SCAN_OPENING_STATUS = 8;

        // HCM enabled levels (for SCAN_SetHCMEnabled);
        public const uint SCAN_HCM_DISABLED = 0;
        public const uint SCAN_HCM_ENABLED = 1;
        public const uint SCAN_HCM_CONTROLS_DISABLED = 2;

        // sensor types (for SCAN_SetSensorType_X and SCAN_GetSensorType_X);
        public const uint SCAN_NO_SENSOR_TYPE = 0;
        public const uint SCAN_S_SENSOR_TYPE = 1;
        public const uint SCAN_SR_SENSOR_TYPE = 2;
        public const uint SCAN_ML_SENSOR_TYPE = 3;
        public const uint SCAN_MR_SENSOR_TYPE = 4;
        public const uint SCAN_SP_SENSOR_TYPE = 5;
        public const uint SCAN_SC_SENSOR_TYPE = 6;
        public const uint SCAN_M25_SENSOR_TYPE = 7;
        public const uint SCAN_SR20_SENSOR_TYPE = 8;
        public const uint SCAN_M_SENSOR_TYPE = 9;
        public const uint SCAN_GC_SENSOR_TYPE = 10;
        public const uint SCAN_GD_SENSOR_TYPE = 11;
        public const uint SCAN_GE_SENSOR_TYPE = 12;
        public const uint SCAN_RA_SENSOR_TYPE = 13;
        public const uint SCAN_GF_SENSOR_TYPE = 14;
        public const uint SCAN_RB_SENSOR_TYPE = 15;
        public const uint SCAN_G605S_SENSOR_TYPE = 16;
        public const uint SCAN_G775S_SENSOR_TYPE = 17;

        // compatibility definitions;
        public const uint SCAN_LIN20UMS_SENSOR_TYPE = SCAN_S_SENSOR_TYPE;
        public const uint SCAN_ROT3600S_SENSOR_TYPE = SCAN_SR_SENSOR_TYPE;
        public const uint SCAN_ROT50LS_SENSOR_TYPE = SCAN_ML_SENSOR_TYPE;
        public const uint SCAN_ROT50RS_SENSOR_TYPE = SCAN_MR_SENSOR_TYPE;
        public const uint SCAN_LINEAR_SENSOR_TYPE = SCAN_S_SENSOR_TYPE;
        public const uint SCAN_ROTARY_SENSOR_TYPE = SCAN_SR_SENSOR_TYPE;

        // movement directions (for SCAN_FindReferenceMark_X);
        public const uint SCAN_FORWARD_DIRECTION = 0;
        public const uint SCAN_BACKWARD_DIRECTION = 1;
        public const uint SCAN_FORWARD_BACKWARD_DIRECTION = 2;
        public const uint SCAN_BACKWARD_FORWARD_DIRECTION = 3;

        // auto zero (for SCAN_FindReferenceMark_X);
        public const uint SCAN_NO_AUTO_ZERO = 0;
        public const uint SCAN_AUTO_ZERO = 1;

        // physical position (for SCAN_GetPhyscialPositionKnown_X);
        public const uint SCAN_PHYSICAL_POSITION_UNKNOWN = 0;
        public const uint SCAN_PHYSICAL_POSITION_KNOWN = 1;

        // channel types (for SCAN_GetChannelType);
        public const uint SCAN_POSITIONER_CHANNEL_TYPE = 0;
        public const uint SCAN_END_EFFECTOR_CHANNEL_TYPE = 1;

        // end effector types;
        public const uint SCAN_ANALOG_SENSOR_END_EFFECTOR_TYPE = 0;
        public const uint SCAN_GRIPPER_END_EFFECTOR_TYPE = 1;
        public const uint SCAN_FORCE_SENSOR_END_EFFECTOR_TYPE = 2;
        public const uint SCAN_FORCE_GRIPPER_END_EFFECTOR_TYPE = 3;
        public const uint SCAN_UNBUFFERED_OUTPUT = 0;
        public const uint SCAN_BUFFERED_OUTPUT = 1;

        //channel properties
        //component
        public const uint SCAN_GENERAL = 1;

        //sub component
        public const uint SCAN_LOW_VIBRATION = 2;

        //property
        public const uint SCAN_OPERATION_MODE = 1;

        //valid value range
        public const int SCAN_DISABLED = 0;
        public const int SCAN_ENABLED = 1;
    }
}
