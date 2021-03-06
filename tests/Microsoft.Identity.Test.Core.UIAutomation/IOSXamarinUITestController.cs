﻿//----------------------------------------------------------------------
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
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.UITest.Queries;

namespace Microsoft.Identity.Test.UIAutomation.Infrastructure
{
    public class IOSXamarinUiTestController : XamarinUiTestControllerBase
    {

        public IOSXamarinUiTestController()
        {
            Platform = Xamarin.UITest.Platform.iOS;
        }

        protected override void Tap(string elementID, XamarinSelector xamarinSelector, TimeSpan timeout)
        {
            WaitForElement(elementID, xamarinSelector, timeout);

            switch (xamarinSelector)
            {
                case XamarinSelector.ByAutomationId:
                    Application.Tap(x => x.Marked(elementID));
                    break;
                case XamarinSelector.ByHtmlIdAttribute:
                        Application.Query(InvokeTapByCssId(elementID));
                    break;
                case XamarinSelector.ByHtmlValue:
                    Application.Query(InvokeTapByHtmlElementValue(elementID));
                    break;
                default:
                    throw new NotImplementedException("Invalid enum value " + xamarinSelector);
            }
        }

        protected override void EnterText(string elementID, string text, XamarinSelector xamarinSelector, TimeSpan timeout)
        {
            WaitForElement(elementID, xamarinSelector, timeout);

            switch (xamarinSelector)
            {
                case XamarinSelector.ByAutomationId:
                    Application.Tap(x => x.Marked(elementID));
                    Application.ClearText();
                    Application.EnterText(x => x.Marked(elementID), text);
                    break;
                case XamarinSelector.ByHtmlIdAttribute:
                    Application.EnterText(QueryByHtmlElementValue(elementID), text);
                    break;
                case XamarinSelector.ByHtmlValue:
                    throw new InvalidOperationException("Test error - you can't input text in an html element that has a value");
                default:
                    throw new NotImplementedException("Invalid enum value " + xamarinSelector);
            }

            DismissKeyboard();
        }

        private static Func<AppQuery, InvokeJSAppQuery> InvokeTapByCssId(string elementID)
        {
            return c => c.Class("WKWebView").InvokeJS(String.Format(CultureInfo.InvariantCulture, "document.getElementById('{0}').click()", elementID));
        }

        private static Func<AppQuery, InvokeJSAppQuery> InvokeTapByHtmlElementValue(string elementID)
        {
            return c => c.Class("WKWebView").InvokeJS(String.Format(CultureInfo.InvariantCulture, "document.evaluate('//*[text()=\"{0}\"]', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.click()", elementID));
        }

        protected override Func<AppQuery, AppWebQuery> QueryByCssId(string elementID)
        {
            return c => c.Class("WKWebView").Css(String.Format(CultureInfo.InvariantCulture, "#{0}", elementID));
        }

        protected override Func<AppQuery, AppWebQuery> QueryByHtmlElementValue(string elementID)
        {
            return c => c.Class("WKWebView").Css(String.Format(CultureInfo.InvariantCulture, "#{0}", elementID));
        }
    }
}
