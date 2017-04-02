#region Copyright

// ****************************************************************************
// <copyright file="FragmentDataBindingModule.cs">
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
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
#if APPCOMPAT
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
using ActionBar = Android.Support.V7.App.ActionBar;
using FragmentContentViewManager = MugenMvvmToolkit.Android.AppCompat.Infrastructure.FragmentContentViewManager;

namespace MugenMvvmToolkit.Android.AppCompat.Modules
#else
using Android.App;

namespace MugenMvvmToolkit.Android.Binding.Modules
#endif
{
    public class FragmentDataBindingModule : IModule
    {
        #region Methods

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
            ft?.CommitAllowingStateLoss();
            fragmentManager?.ExecutePendingTransactions();
        }

        #endregion

        #region Implementation of IModule

        public int Priority => ApplicationSettings.ModulePriorityDefault;

        public bool Load(IModuleContext context)
        {
#if !APPCOMPAT
            if (!AndroidToolkitExtensions.IsApiGreaterThanOrEqualTo17)
                return false;
#endif
            var isActionBar = AndroidToolkitExtensions.IsActionBar;
            var isFragment = AndroidToolkitExtensions.IsFragment;
            var tabChangedDelegate = TabHostItemsSourceGenerator.TabChangedDelegate;
            var removeTabDelegate = TabHostItemsSourceGenerator.RemoveTabDelegate;

            AndroidToolkitExtensions.IsActionBar = o => isActionBar(o) || o is ActionBar;
            AndroidToolkitExtensions.IsFragment = o => isFragment(o) || o is Fragment;
            AndroidToolkitExtensions.AddContentViewManager(new FragmentContentViewManager());

            TabHostItemsSourceGenerator.RemoveTabDelegate = (generator, info) => OnRemoveTab(removeTabDelegate, generator, info);
            TabHostItemsSourceGenerator.TabChangedDelegate = (generator, o, arg3, arg4, arg5) => OnTabChanged(tabChangedDelegate, generator, o, arg3, arg4, arg5);
            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}
