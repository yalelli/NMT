using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace NMT.Nators
{
    class CNators
    {
        //[DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern uint NT_GetDLLVersion(ref uint version);        

        //[DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern uint NT_GetNumberOfChannels(uint systemIndex, ref uint channels);

        /**********************
        General note:
        All functions have a return value of NT_STATUS
        indicating success (NT_OK) or failure of execution. See the above
        definitions for a list of error codes.
        ***********************/

        /************************************************************************
        *************************************************************************
        **                 Section I: Initialization Functions                 **
        *************************************************************************   
        ************************************************************************/
        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_AddSystemToInitSystemsList(uint systemId);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_ClearInitSystemsList();

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_GetAvailableSystems(ref uint idList, ref uint idListSize);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_GetChannelType(uint systemIndex, uint channelIndex, ref uint type);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_GetDLLVersion(ref uint version);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_GetInitState(ref uint initMode);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_GetNumberOfChannels(uint systemIndex, ref uint channels);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_GetNumberOfSystems(ref uint number);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_GetSystemID(uint systemIndex, ref uint systemId);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_InitSystems(uint configuration);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_ReleaseSystems();

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_SetHCMEnabled(uint systemIndex, uint enabled);


        /// <summary>
        /// not in use 
        /// </summary>
        /// <param name="systemIndex"></param>
        /// <param name="locator"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern uint NT_OpenSystem(ref uint systemIndex, [MarshalAs(UnmanagedType.LPStr)] string locator, [MarshalAs(UnmanagedType.LPStr)] string options);
        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern uint NT_CloseSystem(uint systemIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern uint NT_FindSystems([MarshalAs(UnmanagedType.LPStr)] string options, [MarshalAs(UnmanagedType.LPStr)]  ref string outBuffer, ref uint ioBufferSize);
        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern uint NT_GetSystemLocator(uint systemIndex, ref byte outBuffer, ref uint ioBufferSize);

        ////////////////////////////////



        /************************************************************************
        *************************************************************************
        **        Section IIa:  Functions for SYNCHRONOUS communication        **
        *************************************************************************
        ************************************************************************/

        /*************************************************
        **************************************************
        **    Section IIa.1: Configuration Functions    **
        **************************************************
        *************************************************/
        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetAngleLimit_S(uint systemIndex, uint channelIndex, ref uint minAngle, ref int minRevolution, ref uint maxAngle, ref int maxRevolution);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetClosedLoopMoveSpeed_S(uint systemIndex, uint channelIndex, ref uint speed);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_GetEndEffectorType_S(uint systemIndex, uint channelIndex, ref uint type, ref int param1, ref int param2);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetPositionLimit_S(uint systemIndex, uint channelIndex, ref int minPosition, ref int maxPosition);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetSafeDirection_S(uint systemIndex, uint channelIndex, ref uint direction);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetScale_S(uint systemIndex, uint channelIndex, ref int scale, ref uint reserved);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Global
        public static extern uint NT_GetSensorEnabled_S(uint systemIndex, ref uint enabled);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetSensorType_S(uint systemIndex, uint channelIndex, ref uint type);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetAccumulateRelativePositions_S(uint systemIndex, uint channelIndex, uint accumulate);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetAngleLimit_S(uint systemIndex, uint channelIndex, uint minAngle, int minRevolution, uint maxAngle, int maxRevolution);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetClosedLoopMaxFrequency_S(uint systemIndex, uint channelIndex, uint frequency);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetClosedLoopMoveSpeed_S(uint systemIndex, uint channelIndex, uint speed);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_SetEndEffectorType_S(uint systemIndex, uint channelIndex, uint type, int param1, int param2);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetPosition_S(uint systemIndex, uint channelIndex, int position);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetPositionLimit_S(uint systemIndex, uint channelIndex, int minPosition, int maxPosition);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetSafeDirection_S(uint systemIndex, uint channelIndex, uint direction);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetScale_S(uint systemIndex, uint channelIndex, int scale, uint reserved);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Global
        public static extern uint NT_SetSensorEnabled_S(uint systemIndex, uint enabled);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetSensorType_S(uint systemIndex, uint channelIndex, uint type);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetStepWhileScan_S(uint systemIndex, uint channelIndex, uint step);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_SetZeroForce_S(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetZeroPosition_S(uint systemIndex, uint channelIndex);

        /*************************************************
        **************************************************
        **  Section IIa.2: Movement Control Functions   **
        **************************************************
        *************************************************/
        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_CalibrateSensor_S(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_FindReferenceMark_S(uint systemIndex, uint channelIndex, uint direction, uint holdTime, uint autoZero);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GotoAngleAbsolute_S(uint systemIndex, uint channelIndex, uint angle, int revolution, uint holdTime);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GotoAngleRelative_S(uint systemIndex, uint channelIndex, int angleDiff, int revolutionDiff, uint holdTime);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_GotoGripperForceAbsolute_S(uint systemIndex, uint channelIndex, int force, uint speed, uint holdTime);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_GotoGripperOpeningAbsolute_S(uint systemIndex, uint channelIndex, uint opening, uint speed);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_GotoGripperOpeningRelative_S(uint systemIndex, uint channelIndex, int diff, uint speed);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GotoPositionAbsolute_S(uint systemIndex, uint channelIndex, int position, uint holdTime);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GotoPositionRelative_S(uint systemIndex, uint channelIndex, int diff, uint holdTime);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_ScanMoveAbsolute_S(uint systemIndex, uint channelIndex, uint target, uint scanSpeed);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetScanMoveRelativeVoltageOverturnEnabled_S(uint systemIndex, uint channelIndex, uint enabled);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_ScanMoveRelative_S(uint systemIndex, uint channelIndex, int diff, uint scanSpeed);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_StepMove_S(uint systemIndex, uint channelIndex, int steps, uint amplitude, uint frequency);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner, End effector
        public static extern uint NT_Stop_S(uint systemIndex, uint channelIndex);

        /************************************************
        *************************************************
        **  Section IIa.3: Channel Feedback Functions  **
        *************************************************
        *************************************************/
        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetAngle_S(uint systemIndex, uint channelIndex, ref uint angle, ref int revolution);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_GetForce_S(uint systemIndex, uint channelIndex, ref int force);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_GetGripperOpening_S(uint systemIndex, uint channelIndex, ref uint opening);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetPhysicalPositionKnown_S(uint systemIndex, uint channelIndex, ref uint known);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetPosition_S(uint systemIndex, uint channelIndex, ref int position);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner, End effector
        public static extern uint NT_GetStatus_S(uint systemIndex, uint channelIndex, ref uint status);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetVoltageLevel_S(uint systemIndex, uint channelIndex, ref uint level);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetClosedLoopHoldEnabled_S(uint systemIndex, uint channelIndex, uint enabled);

        /************************************************************************
        *************************************************************************
        **       Section IIb:  Functions for ASYNCHRONOUS communication        **
        *************************************************************************
        ************************************************************************/

        /*************************************************
        **************************************************
        **    Section IIb.1: Configuration Functions    **
        **************************************************
        *************************************************/
        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_FlushOutput_A(uint systemIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetAngleLimit_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_GetBufferedOutput_A(uint systemIndex, ref uint mode);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetClosedLoopMoveSpeed_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_GetEndEffectorType_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetPhysicalPositionKnown_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetPositionLimit_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetSafeDirection_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetScale_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_GetSensorEnabled_A(uint systemIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetSensorType_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetAccumulateRelativePositions_A(uint systemIndex, uint channelIndex, uint accumulate);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetAngleLimit_A(uint systemIndex, uint channelIndex, uint minAngle, int minRevolution, uint maxAngle, int maxRevolution);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_SetBufferedOutput_A(uint systemIndex, uint mode);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetClosedLoopMaxFrequency_A(uint systemIndex, uint channelIndex, uint frequency);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetClosedLoopMoveSpeed_A(uint systemIndex, uint channelIndex, uint speed);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_SetEndEffectorType_A(uint systemIndex, uint channelIndex, uint type, int param1, int param2);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetPosition_A(uint systemIndex, uint channelIndex, int position);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetPositionLimit_A(uint systemIndex, uint channelIndex, int minPosition, int maxPosition);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner, End effector
        public static extern uint NT_SetReportOnComplete_A(uint systemIndex, uint channelIndex, uint report);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetSafeDirection_A(uint systemIndex, uint channelIndex, uint direction);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetScale_A(uint systemIndex, uint channelIndex, int scale, uint reserved);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_SetSensorEnabled_A(uint systemIndex, uint enabled);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetSensorType_A(uint systemIndex, uint channelIndex, uint type);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetStepWhileScan_A(uint systemIndex, uint channelIndex, uint step);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_SetZeroForce_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetZeroPosition_A(uint systemIndex, uint channelIndex);

        /*************************************************
        **************************************************
        **  Section IIb.2: Movement Control Functions   **
        **************************************************
        *************************************************/
        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_CalibrateSensor_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_FindReferenceMark_A(uint systemIndex, uint channelIndex, uint direction, uint holdTime, uint autoZero);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GotoAngleAbsolute_A(uint systemIndex, uint channelIndex, uint angle, int revolution, uint holdTime);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GotoAngleRelative_A(uint systemIndex, uint channelIndex, int angleDiff, int revolutionDiff, uint holdTime);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_GotoGripperForceAbsolute_A(uint systemIndex, uint channelIndex, int force, uint speed, uint holdTime);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_GotoGripperOpeningAbsolute_A(uint systemIndex, uint channelIndex, uint opening, uint speed);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_GotoGripperOpeningRelative_A(uint systemIndex, uint channelIndex, int diff, uint speed);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GotoPositionAbsolute_A(uint systemIndex, uint channelIndex, int position, uint holdTime);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GotoPositionRelative_A(uint systemIndex, uint channelIndex, int diff, uint holdTime);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_ScanMoveAbsolute_A(uint systemIndex, uint channelIndex, uint target, uint scanSpeed);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_ScanMoveRelative_A(uint systemIndex, uint channelIndex, int diff, uint scanSpeed);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_StepMove_A(uint systemIndex, uint channelIndex, int steps, uint amplitude, uint frequency);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner, End effector
        public static extern uint NT_Stop_A(uint systemIndex, uint channelIndex);

        /************************************************
        *************************************************
        **  Section IIb.3: Channel Feedback Functions  **
        *************************************************
        ************************************************/
        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetAngle_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_GetForce_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // End effector
        public static extern uint NT_GetGripperOpening_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetPosition_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner, End effector
        public static extern uint NT_GetStatus_A(uint systemIndex, uint channelIndex);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetVoltageLevel_A(uint systemIndex, uint channelIndex);

        /******************
        * Answer retrieval
        ******************/
        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_DiscardPacket_A(uint systemIndex);

        //[DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern uint NT_LookAtNextPacket_A(uint  systemIndex, uint timeout, NT_PACKET *packet);

        //[DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern uint NT_ReceiveNextPacket_A(uint  systemIndex, uint timeout, NT_PACKET *packet);

        //[DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern uint NT_ReceiveNextPacketIfChannel_A(uint  systemIndex, uint  channelIndex, uint timeout, NT_PACKET *packet);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_SetReceiveNotification_A(uint systemIndex, ref uint var_event);

        // function status return values;
        public const uint NT_OK = 0;
        public const uint NT_INITIALIZATION_ERROR = 1;
        public const uint NT_NOT_INITIALIZED_ERROR = 2;
        public const uint NT_NO_SYSTEMS_FOUND_ERROR = 3;
        public const uint NT_TOO_MANY_SYSTEMS_ERROR = 4;
        public const uint NT_INVALID_SYSTEM_INDEX_ERROR = 5;
        public const uint NT_INVALID_CHANNEL_INDEX_ERROR = 6;
        public const uint NT_TRANSMIT_ERROR = 7;
        public const uint NT_WRITE_ERROR = 8;
        public const uint NT_INVALID_PARAMETER_ERROR = 9;
        public const uint NT_READ_ERROR = 10;
        public const uint NT_INTERNAL_ERROR = 12;
        public const uint NT_WRONG_MODE_ERROR = 13;
        public const uint NT_PROTOCOL_ERROR = 14;
        public const uint NT_TIMEOUT_ERROR = 15;
        public const uint NT_NOTIFICATION_ALREADY_SET_ERROR = 16;
        public const uint NT_ID_LIST_TOO_SMALL_ERROR = 17;
        public const uint NT_SYSTEM_ALREADY_ADDED_ERROR = 18;
        public const uint NT_WRONG_CHANNEL_TYPE_ERROR = 19;
        public const uint NT_NO_SENSOR_PRESENT_ERROR = 129;
        public const uint NT_AMPLITUDE_TOO_LOW_ERROR = 130;
        public const uint NT_AMPLITUDE_TOO_HIGH_ERROR = 131;
        public const uint NT_FREQUENCY_TOO_LOW_ERROR = 132;
        public const uint NT_FREQUENCY_TOO_HIGH_ERROR = 133;
        public const uint NT_SCAN_TARGET_TOO_HIGH_ERROR = 135;
        public const uint NT_SCAN_SPEED_TOO_LOW_ERROR = 136;
        public const uint NT_SCAN_SPEED_TOO_HIGH_ERROR = 137;
        public const uint NT_SENSOR_DISABLED_ERROR = 140;
        public const uint NT_COMMAND_OVERRIDDEN_ERROR = 141;
        public const uint NT_END_STOP_REACHED_ERROR = 142;
        public const uint NT_WRONG_SENSOR_TYPE_ERROR = 143;
        public const uint NT_COULD_NOT_FIND_REF_ERROR = 144;
        public const uint NT_WRONG_END_EFFECTOR_TYPE_ERROR = 145;
        public const uint NT_RANGE_LIMIT_REACHED_ERROR = 147;
        public const uint NT_PHYSICAL_POSITION_UNKNOWN_ERROR = 148;
        public const uint NT_OUTPUT_BUFFER_OVERFLOW_ERROR = 149;
        public const uint NT_INVALID_COMPONENT_ERROR = 154;
        public const uint NT_PERMISSION_DENIED_ERROR = 157;
        public const uint NT_UNKNOWN_COMMAND_ERROR = 240;
        public const uint NT_OTHER_ERROR = 255;

        // configuration flags for NT_InitDevices;
        public const uint NT_SYNCHRONOUS_COMMUNICATION = 0;
        public const uint NT_ASYNCHRONOUS_COMMUNICATION = 1;
        public const uint NT_HARDWARE_RESET = 2;

        // return values from NT_GetInitState;
        public const uint NT_INIT_STATE_NONE = 0;
        public const uint NT_INIT_STATE_SYNC = 1;
        public const uint NT_INIT_STATE_ASYNC = 2;

        // configuration flags for NT_SetStepWhileScan_X;
        public const uint NT_NO_STEP_WHILE_SCAN = 0;
        public const uint NT_STEP_WHILE_SCAN = 1;

        // configuration flags for NT_SetSensorEnabled_X;
        public const uint NT_SENSOR_DISABLED = 0;
        public const uint NT_SENSOR_ENABLED = 1;
        public const uint NT_SENSOR_POWERSAVE = 2;

        // configuration flags for NT_SetReportOnComplete_A;
        public const uint NT_NO_REPORT_ON_COMPLETE = 0;
        public const uint NT_REPORT_ON_COMPLETE = 1;

        // configuration flags for NT_SetAccumulateRelativePositions_X;
        public const uint NT_NO_ACCUMULATE_RELATIVE_POSITIONS = 0;
        public const uint NT_ACCUMULATE_RELATIVE_POSITIONS = 1;

        // packet types (for asynchronous mode);
        public const uint NT_NO_PACKET_TYPE = 0;
        public const uint NT_ERROR_PACKET_TYPE = 1;
        public const uint NT_POSITION_PACKET_TYPE = 2;
        public const uint NT_COMPLETED_PACKET_TYPE = 3;
        public const uint NT_STATUS_PACKET_TYPE = 4;
        public const uint NT_ANGLE_PACKET_TYPE = 5;
        public const uint NT_VOLTAGE_LEVEL_PACKET_TYPE = 6;
        public const uint NT_SENSOR_TYPE_PACKET_TYPE = 7;
        public const uint NT_SENSOR_ENABLED_PACKET_TYPE = 8;
        public const uint NT_END_EFFECTOR_TYPE_PACKET_TYPE = 9;
        public const uint NT_GRIPPER_OPENING_PACKET_TYPE = 10;
        public const uint NT_FORCE_PACKET_TYPE = 11;
        public const uint NT_MOVE_SPEED_PACKET_TYPE = 12;
        public const uint NT_PHYSICAL_POSITION_KNOWN_PACKET_TYPE = 13;
        public const uint NT_POSITION_LIMIT_PACKET_TYPE = 14;
        public const uint NT_ANGLE_LIMIT_PACKET_TYPE = 15;
        public const uint NT_SAFE_DIRECTION_PACKET_TYPE = 16;
        public const uint NT_SCALE_PACKET_TYPE = 17;
        public const uint NT_INVALID_PACKET_TYPE = 255;

        // channel status codes;
        public const uint NT_STOPPED_STATUS = 0;
        public const uint NT_STEPPING_STATUS = 1;
        public const uint NT_SCANNING_STATUS = 2;
        public const uint NT_HOLDING_STATUS = 3;
        public const uint NT_TARGET_STATUS = 4;
        public const uint NT_MOVE_DELAY_STATUS = 5;
        public const uint NT_CALIBRATING_STATUS = 6;
        public const uint NT_FINDING_REF_STATUS = 7;
        public const uint NT_OPENING_STATUS = 8;

        // HCM enabled levels (for NT_SetHCMEnabled);
        public const uint NT_HCM_DISABLED = 0;
        public const uint NT_HCM_ENABLED = 1;
        public const uint NT_HCM_CONTROLS_DISABLED = 2;

        // sensor types (for NT_SetSensorType_X and NT_GetSensorType_X);
        public const uint NT_NO_SENSOR_TYPE = 0;
        public const uint NT_S_SENSOR_TYPE = 1;
        public const uint NT_SR_SENSOR_TYPE = 2;
        public const uint NT_ML_SENSOR_TYPE = 3;
        public const uint NT_MR_SENSOR_TYPE = 4;
        public const uint NT_SP_SENSOR_TYPE = 5;
        public const uint NT_SC_SENSOR_TYPE = 6;
        public const uint NT_M25_SENSOR_TYPE = 7;
        public const uint NT_SR20_SENSOR_TYPE = 8;
        public const uint NT_M_SENSOR_TYPE = 9;
        public const uint NT_GC_SENSOR_TYPE = 10;
        public const uint NT_GD_SENSOR_TYPE = 11;
        public const uint NT_GE_SENSOR_TYPE = 12;
        public const uint NT_RA_SENSOR_TYPE = 13;
        public const uint NT_GF_SENSOR_TYPE = 14;
        public const uint NT_RB_SENSOR_TYPE = 15;
        public const uint NT_G605S_SENSOR_TYPE = 16;
        public const uint NT_G775S_SENSOR_TYPE = 17;

        // compatibility definitions;
        public const uint NT_LIN20UMS_SENSOR_TYPE = NT_S_SENSOR_TYPE;
        public const uint NT_ROT3600S_SENSOR_TYPE = NT_SR_SENSOR_TYPE;
        public const uint NT_ROT50LS_SENSOR_TYPE = NT_ML_SENSOR_TYPE;
        public const uint NT_ROT50RS_SENSOR_TYPE = NT_MR_SENSOR_TYPE;
        public const uint NT_LINEAR_SENSOR_TYPE = NT_S_SENSOR_TYPE;
        public const uint NT_ROTARY_SENSOR_TYPE = NT_SR_SENSOR_TYPE;

        // movement directions (for NT_FindReferenceMark_X);
        public const uint NT_FORWARD_DIRECTION = 0;
        public const uint NT_BACKWARD_DIRECTION = 1;
        public const uint NT_FORWARD_BACKWARD_DIRECTION = 2;
        public const uint NT_BACKWARD_FORWARD_DIRECTION = 3;

        // auto zero (for NT_FindReferenceMark_X);
        public const uint NT_NO_AUTO_ZERO = 0;
        public const uint NT_AUTO_ZERO = 1;

        // physical position (for NT_GetPhyscialPositionKnown_X);
        public const uint NT_PHYSICAL_POSITION_UNKNOWN = 0;
        public const uint NT_PHYSICAL_POSITION_KNOWN = 1;

        // channel types (for NT_GetChannelType);
        public const uint NT_POSITIONER_CHANNEL_TYPE = 0;
        public const uint NT_END_EFFECTOR_CHANNEL_TYPE = 1;

        // end effector types;
        public const uint NT_ANALOG_SENSOR_END_EFFECTOR_TYPE = 0;
        public const uint NT_GRIPPER_END_EFFECTOR_TYPE = 1;
        public const uint NT_FORCE_SENSOR_END_EFFECTOR_TYPE = 2;
        public const uint NT_FORCE_GRIPPER_END_EFFECTOR_TYPE = 3;
        public const uint NT_UNBUFFERED_OUTPUT = 0;
        public const uint NT_BUFFERED_OUTPUT = 1;




        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_GetChannelProperty_S(uint systemIndex, uint channelIndex, uint key, ref int value);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)] // Positioner
        public static extern uint NT_SetChannelProperty_S(uint systemIndex, uint channelIndex, uint key, int value);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_EPK(uint selector, uint subSelector, uint property);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_GetClosedLoopMoveAcceleration_S(uint systemIndex, uint channelIndex, ref uint acceleration);

        [DllImport(@"NTControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint NT_SetClosedLoopMoveAcceleration_S(uint systemIndex, uint channelIndex, uint acceleration);

        //channel properties
        //component
        public const uint NT_GENERAL = 1;

        //sub component
        public const uint NT_LOW_VIBRATION = 2;

        //property
        public const uint NT_OPERATION_MODE = 1;

        //valid value range
        public const int NT_DISABLED = 0;
        public const int NT_ENABLED = 1;
    }
}
