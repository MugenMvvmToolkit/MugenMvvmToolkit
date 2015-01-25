#region Copyright

// ****************************************************************************
// <copyright file="FragmentExtensions.cs">
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
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces.Models;
#if APPCOMPAT
using MugenMvvmToolkit.AppCompat.Infrastructure.Mediators;
using MugenMvvmToolkit.AppCompat.Interfaces.Mediators;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Fragment = Android.Support.V4.App.Fragment;

namespace MugenMvvmToolkit.AppCompat
#else
using Android.App;
using MugenMvvmToolkit.FragmentSupport.Infrastructure.Mediators;
using MugenMvvmToolkit.FragmentSupport.Interfaces.Mediators;

namespace MugenMvvmToolkit.FragmentSupport
#endif
{
    public static class FragmentExtensions
    {
        #region Fields

        /// <summary>
        ///     Gets the attached member for view.
        /// </summary>
        public static readonly IAttachedBindingMemberInfo<View, Fragment> FragmentViewMember;

        private static Func<Fragment, IDataContext, IMvvmFragmentMediator> _mvvmFragmentMediatorFactory;

        #endregion

        #region Constructors

        static FragmentExtensions()
        {
            FragmentViewMember = AttachedBindingMember.CreateAutoProperty<View, Fragment>("!$fragment");
            _mvvmFragmentMediatorFactory = MvvmFragmentMediatorFactoryMethod;
            AutoDisposeFragmentDefault = false;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the factory that creates an instance of <see cref="IMvvmFragmentMediator" />.
        /// </summary>
        [NotNull]
        public static Func<Fragment, IDataContext, IMvvmFragmentMediator> MvvmFragmentMediatorFactory
        {
            get { return _mvvmFragmentMediatorFactory; }
            set
            {
                Should.PropertyBeNotNull(value);
                _mvvmFragmentMediatorFactory = value;
            }
        }

        /// <summary>
        ///     Gets or sets the default value for the attached property AutoDispose for Activity.
        /// </summary>
        public static bool? AutoDisposeFragmentDefault { get; set; }

        /// <summary>
        ///     Gets or sets that is responsible for cache view in fragment.
        /// </summary>
        public static bool CacheFragmentViewDefault { get; set; }

        #endregion

        #region Methods

        internal static FragmentManager GetFragmentManager(this View view)
        {
            var treeView = view;
            while (treeView != null)
            {
                var fragment = FragmentViewMember.GetValue(treeView, null);
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

        private static IMvvmFragmentMediator MvvmFragmentMediatorFactoryMethod(Fragment fragment, IDataContext dataContext)
        {
            return new MvvmFragmentMediator(fragment);
        }

        #endregion
    }
}