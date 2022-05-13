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
// Copyright (c) 2017,		HTC Corporation
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
        public class SRanipal_API
		{
            /// <summary>
            /// Invokes an anipal module.
            /// </summary>
            /// <param name="anipalType">The index of an anipal module.</param>
            /// <param name="config">Indicates the resulting ViveSR.Error status of this method.</returns>
            /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
            [DllImport("SRanipal")]
            public static extern Error Initial(int anipalType, IntPtr config);

            /// <summary>
            /// Terminates an anipal module.
            /// </summary>
            /// <param name="anipalType">The index of an anipal module.</param>
            /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
            [DllImport("SRanipal")]
            public static extern Error Release(int anipalType);

            /// <summary>
            /// Gets the status of an anipal module.
            /// </summary>
            /// <param name="anipalType">The index of an anipal module.</param>
            /// <param name="status">The status of an anipal module.</param>
            /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
            [DllImport("SRanipal")]
            public static extern Error GetStatus(int anipalType, out AnipalStatus status);
        }
    }
}
