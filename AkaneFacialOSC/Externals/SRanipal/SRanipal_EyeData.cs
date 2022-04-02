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
using System;
using System.Runtime.InteropServices;
namespace ViveSR
{
    namespace anipal
    {
        namespace Eye
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Vector3
            {
                public float x;
                public float y;
                public float z;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct Vector2
            {
                public float x;
                public float y;
            }

            #region VerboseData
            public enum EyeIndex { LEFT, RIGHT, }
            public enum GazeIndex { LEFT, RIGHT, COMBINE }

            /** @enum SingleEyeDataValidity
			An enum type for getting validity from the structure: eye data's bitmask
			*/
            public enum SingleEyeDataValidity : int
            {
                /** The validity of the origin of gaze of the eye data */
                SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY,
                /** The validity of the direction of gaze of the eye data */
                SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY,
                /** The validity of the diameter of gaze of the eye data */
                SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY,
                /** The validity of the openness of the eye data */
                SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY,
                /** The validity of normalized position of pupil */
                SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY
            };

            public enum TrackingImprovement : int
            {
                TRACKING_IMPROVEMENT_USER_POSITION_HMD,
                TRACKING_IMPROVEMENT_CALIBRATION_CONTAINS_POOR_DATA,
                TRACKING_IMPROVEMENT_CALIBRATION_DIFFERENT_BRIGHTNESS,
                TRACKING_IMPROVEMENT_IMAGE_QUALITY,
                TRACKING_IMPROVEMENT_INCREASE_EYE_RELIEF,
            };

            [StructLayout(LayoutKind.Sequential)]
            public struct TrackingImprovements
            {
                public int count;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
                public TrackingImprovement[] items;
            };

            /** @struct SingleEyeData
			* A struct containing status related an eye.
			* @image html EyeData.png width=1040px height=880px
			*/
            [StructLayout(LayoutKind.Sequential)]
            public struct SingleEyeData
            {
                /** The bits containing all validity for this frame.*/
                public System.UInt64 eye_data_validata_bit_mask;
                /** The point in the eye from which the gaze ray originates in millimeter.(right-handed coordinate system)*/
                public Vector3 gaze_origin_mm;
                /** The normalized gaze direction of the eye in [0,1].(right-handed coordinate system)*/
                public Vector3 gaze_direction_normalized;
                /** The diameter of the pupil in millimeter*/
                public float pupil_diameter_mm;
                /** A value representing how open the eye is.*/
                public float eye_openness;
                /** The normalized position of a pupil in [0,1]*/
                public Vector2 pupil_position_in_sensor_area;

                public bool GetValidity(SingleEyeDataValidity validity)
                {
                    return (eye_data_validata_bit_mask & (ulong)(1 << (int)validity)) > 0;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CombinedEyeData
            {
                public SingleEyeData eye_data;
                public bool convergence_distance_validity;
                public float convergence_distance_mm;
            }

            [StructLayout(LayoutKind.Sequential)]
            /** @struct VerboseData
			* A struct containing all data listed below.
			*/
            public struct VerboseData
            {
                /** A instance of the struct as @ref EyeData related to the left eye*/
                public SingleEyeData left;
                /** A instance of the struct as @ref EyeData related to the right eye*/
                public SingleEyeData right;
                /** A instance of the struct as @ref EyeData related to the combined eye*/
                public CombinedEyeData combined;
                public TrackingImprovements tracking_improvements;
            }
            #endregion

            #region EyeParameter
            [StructLayout(LayoutKind.Sequential)]
            /** @struct GazeRayParameter
			* A struct containing all data listed below.
			*/
            public struct GazeRayParameter
            {
                /** The sensitive factor of gaze ray in [0,1]. The bigger factor is, the more sensitive the gaze ray is.*/
                public double sensitive_factor;
            };

            [StructLayout(LayoutKind.Sequential)]
            /** @struct EyeParameter
			* A struct containing all data listed below.
			*/
            public struct EyeParameter
            {
                public GazeRayParameter gaze_ray_parameter;
            };
            #endregion

            #region CalibrationResult
            public enum CalibrationResult
            {
                SUCCESS,
                FAIL,
                BUSY,
            }
            #endregion
        }
    }
}