#region Copyright

// ****************************************************************************
// <copyright file="FragmentExtensions.cs">
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
using Android.Views;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces.Models;
#if APPCOMPAT
using MugenMvvmToolkit.Android.AppCompat.Infrastructure.Mediators;
using MugenMvvmToolkit.Android.AppCompat.Interfaces.Mediators;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Fragment = Android.Support.V4.App.Fragment;

namespace MugenMvvmToolkit.Android.AppCompat
{
    public static class FragmentExtensions
#else
using Android.App;
using MugenMvvmToolkit.Android.Infrastructure.Mediators;
using MugenMvvmToolkit.Android.Interfaces.Mediators;

namespace MugenMvvmToolkit.Android
{
    public static partial class PlatformExtensions
#endif
    {
        #region Methods

        public static FragmentManager GetFragmentManager(this View view)
        {
            var treeView = view;
            while (treeView != null)
            {
                var fragment = treeView.GetBindingMemberValue(AttachedMembers.View.Fragment) as Fragment;
                if (fragment != null)
                    return fragment.ChildFragmentManager;
                treeView = treeView.Parent as View;
            }
            var activity = view.Context.GetActivity();
            if (activity == null)
            {
                Tracer.Warn("The activity is null {0}", view);
                return null;
            }
            return activity.GetFragmentManager();
        }

        public static object MvvmFragmentMediatorDefaultFactory(object fragment, IDataContext dataContext, Type mediatorType)
        {
            if (fragment is Fragment && typeof(IMvvmFragmentMediator).IsAssignableFrom(mediatorType))
                return new MvvmFragmentMediator((Fragment)fragment);
            return null;
        }

        #endregion
    }
}
