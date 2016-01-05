#region Copyright

// ****************************************************************************
// <copyright file="FragmentExtensions.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using System.Threading;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
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
        #region Fields

        private static Func<Fragment, IDataContext, IMvvmFragmentMediator> _mvvmFragmentMediatorFactory;

        #endregion

        #region Constructors

#if APPCOMPAT
        static FragmentExtensions()
        {
            _mvvmFragmentMediatorFactory = MvvmFragmentMediatorFactoryMethod;
        }
#endif
        #endregion

        #region Properties

        [NotNull]
        public static Func<Fragment, IDataContext, IMvvmFragmentMediator> MvvmFragmentMediatorFactory
        {
            get { return _mvvmFragmentMediatorFactory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _mvvmFragmentMediatorFactory = value;
            }
        }

#if !APPCOMPAT
        public static bool CacheFragmentViewDefault { get; set; }
#endif
        #endregion

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

        internal static IMvvmFragmentMediator GetOrCreateMediator(this Fragment fragment, ref IMvvmFragmentMediator mediator)
        {
            if (mediator == null)
                Interlocked.CompareExchange(ref mediator, MvvmFragmentMediatorFactory(fragment, DataContext.Empty), null);
            return mediator;
        }

        private static IMvvmFragmentMediator MvvmFragmentMediatorFactoryMethod(Fragment fragment, IDataContext dataContext)
        {
            return new MvvmFragmentMediator(fragment);
        }

        #endregion
    }
}
