﻿// ------------------------------------------------------------------------------
// 
// Copyright (c) Microsoft Corporation.
// All rights reserved.
// 
// This code is licensed under the MIT License.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Identity.Client.UI;

#if iOS
using UIKit;
#endif

#if ANDROID
using Android.App;
#endif

#if DESKTOP
using System.Windows.Forms;
#endif

namespace Microsoft.Identity.Client.ApiConfig
{
    /// <summary>
    /// Owner UI parent for the dialog in which authentication will take place.
    /// </summary>
    public class OwnerUiParent
    {
        internal CoreUIParent CoreUiParent { get; } = new CoreUIParent();

#if ANDROID
        internal void SetAndroidActivity(Activity activity)
        {
            CoreUiParent.Activity = activity;
            CoreUiParent.CallerActivity = activity;
        }
#endif

#if iOS
        internal void SetUIViewController(UIViewController uiViewController)
        {
            CoreUiParent.CallerViewController = uiViewController;
        }
#endif

#if DESKTOP
        internal void SetOwnerWindow(IWin32Window ownerWindow)
        {
            CoreUiParent.OwnerWindow = ownerWindow;
        }

        internal void SetOwnerWindow(IntPtr ownerWindow)
        {
            CoreUiParent.OwnerWindow = ownerWindow;
        }
#endif
    }
}