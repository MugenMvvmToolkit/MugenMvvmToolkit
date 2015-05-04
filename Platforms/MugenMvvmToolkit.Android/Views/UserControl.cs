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
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Widget;
using MugenMvvmToolkit.Interfaces.Navigation;

namespace MugenMvvmToolkit.Views
{
    [Register("mugenmvvmtoolkit.views.UserControl")]
    public abstract class UserControl : FrameLayout
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserControl" /> class.
        /// </summary>
        protected UserControl(int viewId)
            : base(GetCurrentContext())
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

        #region Methods

        private static Context GetCurrentContext()
        {
            Activity context = PlatformExtensions.CurrentActivity;
            if (context == null)
            {
                INavigationProvider service;
                if (ServiceProvider.IocContainer.TryGet(out service))
                    context = service.CurrentContent as Activity;
            }
            return context;
        }

        #endregion
    }
}