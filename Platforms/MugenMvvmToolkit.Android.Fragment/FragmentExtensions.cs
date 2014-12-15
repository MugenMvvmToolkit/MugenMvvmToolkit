#region Copyright
// ****************************************************************************
// <copyright file="FragmentExtensions.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using Android.OS;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces.Models;
#if APPCOMPAT
using MugenMvvmToolkit.AppCompat.Infrastructure.Mediators;
using MugenMvvmToolkit.AppCompat.Interfaces.Mediators;
using MugenMvvmToolkit.AppCompat.Modules;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace MugenMvvmToolkit.AppCompat
#else
using MugenMvvmToolkit.FragmentSupport.Infrastructure.Mediators;
using MugenMvvmToolkit.FragmentSupport.Interfaces.Mediators;
using MugenMvvmToolkit.FragmentSupport.Modules;

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
        private const string AddedToBackStackKey = "@$backstack";

        #endregion

        #region Constructors

        static FragmentExtensions()
        {
            FragmentViewMember = FragmentViewMember = AttachedBindingMember.CreateAutoProperty<View, Fragment>("!$fragment");
            _mvvmFragmentMediatorFactory = MvvmFragmentMediatorFactoryMethod;
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

        #endregion

        #region Methods

        /// <summary>
        ///     Sets the content.
        /// </summary>
        public static object SetContentView([NotNull] this ViewGroup frameLayout, object content, int? templateId,
            IDataTemplateSelector templateSelector, FragmentTransaction transaction = null,
            Action<ViewGroup, Fragment, FragmentTransaction> updateAction = null)
        {
            content = PlatformExtensions.GetContentView(frameLayout, frameLayout.Context, content, templateId, templateSelector);
            frameLayout.SetContentView(content, transaction, updateAction);
            return content;
        }

        public static void SetContentView([NotNull] this ViewGroup frameLayout, object content,
            FragmentTransaction transaction = null, Action<ViewGroup, Fragment, FragmentTransaction> updateAction = null)
        {
            Should.NotBeNull(frameLayout, "frameLayout");
            if (content == null)
            {
                var hasFragment = false;
                var fragmentManager = frameLayout.GetFragmentManager();
                if (fragmentManager != null)
                {
                    var fragment = fragmentManager.FindFragmentById(frameLayout.Id);
                    hasFragment = fragment != null;
                    if (hasFragment && !fragmentManager.IsDestroyed)
                    {
                        fragmentManager.BeginTransaction().Remove(fragment).CommitAllowingStateLoss();
                        fragmentManager.ExecutePendingTransactions();
                    }
                }
                if (!hasFragment)
                    frameLayout.RemoveAllViews();
                return;
            }

            var view = content as View;
            if (view == null)
            {
                var fragment = (Fragment)content;
                PlatformExtensions.ValidateViewIdFragment(frameLayout, fragment);
                var addToBackStack = FragmentDataBindingModule.AddToBackStackMember.GetValue(frameLayout, null);
                FragmentManager manager = null;
                if (transaction == null)
                {
                    manager = frameLayout.GetFragmentManager();
                    if (manager == null)
                        return;
                    transaction = manager.BeginTransaction();
                }
                if (addToBackStack && fragment.Arguments != null)
                    addToBackStack = !fragment.Arguments.GetBoolean(AddedToBackStackKey);

                if (updateAction == null)
                {
                    if (fragment.IsDetached)
                        transaction.Attach(fragment);
                    else
                    {
                        if (addToBackStack)
                        {
                            if (fragment.Arguments == null)
                                fragment.Arguments = new Bundle();
                            fragment.Arguments.PutBoolean(AddedToBackStackKey, true);
                        }
                        transaction.Replace(frameLayout.Id, fragment);
                    }
                }
                else
                    updateAction(frameLayout, fragment, transaction);
                if (addToBackStack)
                    transaction.AddToBackStack(null);


                if (manager != null)
                {
                    transaction.Commit();
                    manager.ExecutePendingTransactions();
                }
            }
            else
            {
                if (frameLayout.ChildCount == 1 && frameLayout.GetChildAt(0) == view)
                    return;
                frameLayout.RemoveAllViews();
                frameLayout.AddView(view);
            }
        }

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