#region Copyright

// ****************************************************************************
// <copyright file="UserControl.cs">
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
using Android.Runtime;
using Android.Widget;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Android.Views
{
    [Register("mugenmvvmtoolkit.android.views.UserControl")]
    public class UserControl : FrameLayout, IHasDisplayName
    {
        #region Constructors

        public UserControl(int viewId)
            : base(AndroidToolkitExtensions.CurrentActivity)
        {
            SetOnHierarchyChangeListener(GlobalViewParentListener.Instance);
            Context.GetBindableLayoutInflater().Inflate(viewId, this, true);
        }

        protected UserControl(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        #endregion

        #region Properties

        public virtual string DisplayName { get; set; }

        #endregion        
    }
}
