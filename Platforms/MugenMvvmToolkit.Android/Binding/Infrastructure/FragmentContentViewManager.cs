#region Copyright

// ****************************************************************************
// <copyright file="FragmentContentViewManager.cs">
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

using Android.OS;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Binding;
#if APPCOMPAT
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace MugenMvvmToolkit.Android.AppCompat.Infrastructure
#else
using Android.App;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
#endif
{
    public class FragmentContentViewManager : IContentViewManager
    {
        #region Fields

        private const string AddedToBackStackKey = "@$backstack";

        #endregion

        #region Implementation of IContentViewManager

        public bool SetContent(object view, object content)
        {
            var targetView = view as ViewGroup;
            if (targetView == null)
                return false;
            if (content == null)
            {
                FragmentManager fragmentManager = targetView.GetFragmentManager();
                if (fragmentManager != null)
                {
                    Fragment oldFragment = fragmentManager.FindFragmentById(targetView.Id);
                    if (oldFragment != null && !fragmentManager.IsDestroyed)
                    {
                        BeginTransaction(fragmentManager, targetView, null)
                            .Remove(oldFragment)
                            .CommitAllowingStateLoss();
                        fragmentManager.ExecutePendingTransactions();
                        return true;
                    }
                }
                return false;
            }
            var fragment = content as Fragment;
            if (fragment == null)
                return false;
            PlatformExtensions.ValidateViewIdFragment(targetView, fragment);
            FragmentManager manager = targetView.GetFragmentManager();
            if (manager == null)
                return false;
            FragmentTransaction transaction = BeginTransaction(manager, targetView, fragment);
            var addToBackStack = targetView.GetBindingMemberValue(AttachedMembers.ViewGroup.AddToBackStack);
            if (addToBackStack && fragment.Arguments != null)
                addToBackStack = !fragment.Arguments.GetBoolean(AddedToBackStackKey);

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
                transaction.Replace(targetView.Id, fragment);
            }
            if (addToBackStack)
                transaction.AddToBackStack(null);

            transaction.Commit();
            manager.ExecutePendingTransactions();
            return true;
        }

        #endregion

        #region Methods

        protected virtual FragmentTransaction BeginTransaction([NotNull] FragmentManager fragmentManager,
            [NotNull] View view, [CanBeNull] Fragment content)
        {
            return fragmentManager.BeginTransaction();
        }

        #endregion
    }
}
