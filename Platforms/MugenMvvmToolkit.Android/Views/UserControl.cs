#region Copyright

// ****************************************************************************
// <copyright file="UserControl.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using Android.Runtime;
using Android.Widget;

namespace MugenMvvmToolkit.Android.Views
{
    [Register("mugenmvvmtoolkit.android.views.UserControl")]
    public class UserControl : FrameLayout
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserControl" /> class.
        /// </summary>
        public UserControl(int viewId)
            : base(PlatformExtensions.CurrentActivity)
        {
            Context.GetBindableLayoutInflater().Inflate(viewId, this, true);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserControl" /> class.
        /// </summary>
        protected UserControl(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        #endregion        
    }
}