#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsAndroidToolkitExtensions.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using Android.App;
using Android.Content;

namespace MugenMvvmToolkit.Xamarin.Forms.Android
{
    public static class XamarinFormsAndroidToolkitExtensions
    {
        #region Properties

        public static Func<Context> GetCurrentContext { get; set; }

        #endregion

        #region Methods

        public static Context GetContext()
        {
            return GetCurrentContext?.Invoke() ?? Application.Context;
        }

        #endregion
    }
}