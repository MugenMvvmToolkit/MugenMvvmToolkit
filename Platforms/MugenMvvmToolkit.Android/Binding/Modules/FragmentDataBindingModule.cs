#region Copyright

// ****************************************************************************
// <copyright file="FragmentDataBindingModule.cs">
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
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Modules;
#if APPCOMPAT
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
using ActionBar = Android.Support.V7.App.ActionBar;
using FragmentContentViewManager = MugenMvvmToolkit.Android.AppCompat.Infrastructure.FragmentContentViewManager;

namespace MugenMvvmToolkit.Android.AppCompat.Modules
#else
using Android.App;
using Android.Views;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Android.Binding.Modules
#endif
{
    public class FragmentDataBindingModule : ModuleBase
    {
        #region Fields

#if !APPCOMPAT
        internal static readonly IAttachedBindingMemberInfo<View, bool> AddToBackStackMember;
#endif
        #endregion

        #region Constructors

#if !APPCOMPAT
        static FragmentDataBindingModule()
        {
            AddToBackStackMember = AttachedBindingMember.CreateAutoProperty<View, bool>("AddToBackStack");
        }
#endif
        /// <summary>
        ///     Initializes a new instance of the <see cref="FragmentDataBindingModule" /> class.
        /// </summary>
        public FragmentDataBindingModule()
            : base(true)
        {
        }

        #endregion

        #region Methods

        private static void RegisterMembers(IBindingMemberProvider memberProvider)
        {
#if !APPCOMPAT
            //View
            memberProvider.Register(AddToBackStackMember);
#endif
        }

        private static void OnRemoveTab(Action<TabHostItemsSourceGenerator, TabHostItemsSourceGenerator.TabInfo> baseAction, TabHostItemsSourceGenerator generator, TabHostItemsSourceGenerator.TabInfo tab)
        {
            var fragment = tab.Content as Fragment;
            if (fragment == null)
            {
                baseAction(generator, tab);
                return;
            }
            var fragmentManager = generator.TabHost.GetFragmentManager();
            if (fragmentManager == null)
                return;
            fragmentManager.BeginTransaction()
                .Remove(fragment)
                .Commit();
            fragmentManager.ExecutePendingTransactions();
        }

        private static void OnTabChanged(Action<TabHostItemsSourceGenerator, object, object, bool, bool> baseAction, TabHostItemsSourceGenerator generator, object oldValue, object newValue, bool clearOldValue, bool setNewValue)
        {
            FragmentManager fragmentManager = null;
            FragmentTransaction ft = null;
            if (clearOldValue)
            {
                var fragment = oldValue as Fragment;
                if (fragment == null)
                    baseAction(generator, oldValue, newValue, true, false);
                else if (fragment.IsAlive())
                {
                    fragmentManager = generator.TabHost.GetFragmentManager();
                    if (fragmentManager != null)
                        ft = fragmentManager.BeginTransaction().Detach(fragment);
                }
            }
            if (setNewValue)
            {
                var fragment = newValue as Fragment;
                if (fragment == null)
                    baseAction(generator, oldValue, newValue, false, true);
                else
                {
                    if (ft == null)
                    {
                        fragmentManager = generator.TabHost.GetFragmentManager();
                        if (fragmentManager != null)
                            ft = fragmentManager.BeginTransaction();
                    }
                    if (ft != null)
                    {
                        ft = fragment.IsDetached
                            ? ft.Attach(fragment)
                            : ft.Replace(generator.TabHost.TabContentView.Id, fragment);
                    }
                }
            }
            if (ft != null)
                ft.CommitAllowingStateLoss();
            if (fragmentManager != null)
                fragmentManager.ExecutePendingTransactions();
        }

        #endregion

        #region Overrides of ModuleBase

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        protected override bool LoadInternal()
        {
#if !APPCOMPAT
            if (!PlatformExtensions.IsApiGreaterThanOrEqualTo17)
                return false;
#endif
            var isActionBar = PlatformExtensions.IsActionBar;
            var isFragment = PlatformExtensions.IsFragment;
            var tabChangedDelegate = TabHostItemsSourceGenerator.TabChangedDelegate;
            var removeTabDelegate = TabHostItemsSourceGenerator.RemoveTabDelegate;

            PlatformExtensions.IsActionBar = o => isActionBar(o) || o is ActionBar;
            PlatformExtensions.IsFragment = o => isFragment(o) || o is Fragment;
            PlatformExtensions.AddContentViewManager(new FragmentContentViewManager());

            TabHostItemsSourceGenerator.RemoveTabDelegate = (generator, info) => OnRemoveTab(removeTabDelegate, generator, info);
            TabHostItemsSourceGenerator.TabChangedDelegate = (generator, o, arg3, arg4, arg5) => OnTabChanged(tabChangedDelegate, generator, o, arg3, arg4, arg5);

            RegisterMembers(BindingServiceProvider.MemberProvider);
            TypeCache<Fragment>.Initialize(null);
            return true;
        }

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        protected override void UnloadInternal()
        {
        }

        #endregion
    }
}