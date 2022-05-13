///////////////////////////////////////////////////////////////////////////////////////
//
//  IMPORTANT: READ BEFORE DOWNLOADING, COPYING, INSTALLING OR USING.
//
//  By downloading, copying, installing or using the software you agree to this license.
//  If you do not agree to this license, do not download, install,
//  copy or use the software.
//
//                           License Agreement
//                     For Vive Super Reality Library
//
// Copyright (c) 2019,		HTC Corporation
//
// All rights reserved. Third party copyrights are property of their respective owners.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//   * Redistribution's of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//
//   * Redistribution's in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//
//   * The name of the copyright holders may not be used to endorse or promote products
//     derived from this software without specific prior written permission.
//
// This software is provided by the copyright holders and contributors "as is" and
// any express or implied warranties, including, but not limited to, the implied
// warranties of merchantability and fitness for a particular purpose are disclaimed.
// In no event shall the Intel Corporation or contributors be liable for any direct,
// indirect, incidental, special, exemplary, or consequential damages
// (including, but not limited to, procurement of substitute goods or services;
// loss of use, data, or profits; or business interruption) however caused
// and on any theory of liability, whether in contract, strict liability,
// or tort (including negligence or otherwise) arising in any way out of
// the use of this software, even if advised of the possibility of such damage.
//
///////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Runtime.InteropServices;

namespace ViveSR
{
    namespace anipal
    {
        namespace Eye
        {
            public static class SRanipal_Eye
            {
                public const int ANIPAL_TYPE_EYE = 0;
                public const int ANIPAL_TYPE_EYE_V2 = 2;
                /// <summary>
                /// Check HMD device is ViveProEye or not.
                /// </summary>
                /// <returns>true : ViveProEye, false : other HMD.</returns>
                [DllImport("SRanipal")]
                public static extern bool IsViveProEye();


                /// <summary>
                /// Gets data from anipal's Eye module.
                /// </summary>
                /// <param name="data">ViveSR.anipal.Eye.EyeData</param>
                /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
                [DllImport("SRanipal")]
                public static extern Error GetEyeData(ref EyeData data);

                /// <summary>
                /// Gets data from anipal's Eye module.
                /// </summary>
                /// <param name="data">ViveSR.anipal.Eye.EyeData</param>
                /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
                [DllImport("SRanipal")]
                public static extern Error GetEyeData_v2(ref EyeData_v2 data);

                /// <summary>
                /// Sets the parameter of anipal's Eye module.
                /// </summary>
                /// <param name="parameter">ViveSR.anipal.Eye.EyeParameter</param>
                /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
                [DllImport("SRanipal")]
                public static extern Error SetEyeParameter(EyeParameter parameter);

                /// <summary>
                /// Gets the parameter of anipal's Eye module.
                /// </summary>
                /// <param name="parameter">ViveSR.anipal.Eye.EyeParameter</param>
                /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
                [DllImport("SRanipal")]
                public static extern Error GetEyeParameter(ref EyeParameter parameter);

                /// <summary>
                /// Indicate if user need to do eye calibration now.
                /// </summary>
                /// <param name="need">If need calibration, it will be true, otherwise it will be false.</param>
                /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
                [DllImport("SRanipal")]
                public static extern int IsUserNeedCalibration(ref bool need);

                /// <summary>
                /// Launches anipal's Eye Calibration tool (an overlay program).
                /// </summary>
                /// <param name="callback">(Upcoming feature) A callback method invoked at the end of the calibration process.</param>
                /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
                [DllImport("SRanipal")]
                public static extern int LaunchEyeCalibration(IntPtr callback);

                /* Register a callback function to receive eye camera related data when the module has new outputs.
                [in] function pointer of callback
                [out] error code. please refer Error in ViveSR_Enums.h
                */
                [DllImport("SRanipal")]
                public static extern int RegisterEyeDataCallback(IntPtr callback);

                /* Unegister a callback function to stop receiving eye camera related data.
                * [in] function pointer of callback
                * [out] error code. please refer Error in ViveSR_Enums.h
                */
                [DllImport("SRanipal")]
                public static extern int UnregisterEyeDataCallback(IntPtr callback);

                /* Register a callback function to receive eye camera related data when the module has new outputs.
                * [in] function pointer of callback
                * [out] error code. please refer Error in ViveSR_Enums.h
                */
                [DllImport("SRanipal")]
                public static extern int RegisterEyeDataCallback_v2(IntPtr callback);

                /* Unegister a callback function to stop receiving eye camera related data.
                * [in] function pointer of callback
                * [out] error code. please refer Error in ViveSR_Enums.h
                */
                [DllImport("SRanipal")]
                public static extern int UnregisterEyeDataCallback_v2(IntPtr callback);

                /* Synchronization the clock on the device and the clock on the system.
				* @param[in] Trigger for Synchronization function.
				* @return error code. please refer Error in ViveSR_Enums.h
				*/
                [DllImport("SRanipal")]
                public static extern Error SRanipal_UpdateTimeSync();

                /* Get the system timestamp.
				* @param[out] the value of system timestamp.
				* @return error code. please refer Error in ViveSR_Enums.h
				*/
                [DllImport("SRanipal")]
                public static extern Error SRanipal_GetSystemTime(ref Int64 time);
            }
        }
    }
}


